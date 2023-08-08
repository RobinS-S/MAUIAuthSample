#nullable enable
using System.Net.Http.Headers;
using CommunityToolkit.Maui.Alerts;
using IdentityModel.OidcClient;
using MauiXamlClient.Auth.Pages;

namespace MauiXamlClient.Auth.Services;

public class AuthService
{
    private const string AuthTokenStorageKey = "authToken";
    private const string RefreshTokenStorageKey = "refreshToken";
    private const string IdentityTokenStorageKey = "identityToken";
    private readonly AuthenticatedHttpClientService _authenticatedHttpClient;
    private readonly HttpClient _httpClient;
    private readonly OidcClient _openIdClient;
    private string? _identityToken;

    private string? _refreshToken;

    public AuthService(HttpClient httpClient, AuthenticatedHttpClientService authenticatedHttpClient)
    {
        _httpClient = httpClient;
        _authenticatedHttpClient = authenticatedHttpClient;

        _openIdClient = new OidcClient(new OidcClientOptions
        {
            Authority = ApiConfig.Instance.ApiUrl,
            ClientId = ApiConfig.Instance.ClientId,
            Scope = ApiConfig.Instance.Scopes,
            RedirectUri = ApiConfig.Instance.CustomProtocolRedirectUri,
            HttpClientFactory = _ => _httpClient,
            PostLogoutRedirectUri = ApiConfig.Instance.CustomProtocolRedirectUri
        });
    }

    public AuthResult? AuthResult { get; set; }
    public bool IsLoggedIn { get; set; }

    /// <summary>
    ///     Triggered when authentication status changes.
    /// </summary>
    public event EventHandler<bool>? AuthChanged;

    /// <summary>
    ///     This retrieves the tokens from SecureStorage, provided by each OS
    /// </summary>
    /// <returns></returns>
    private async Task<string?> GetAuthTokenAsync()
    {
        return await SecureStorage.Default.GetAsync(AuthTokenStorageKey);
    }

    private async Task<string?> GetRefreshTokenAsync()
    {
        return await SecureStorage.Default.GetAsync(RefreshTokenStorageKey);
    }

    private async Task<string?> GetIdentityTokenAsync()
    {
        return await SecureStorage.Default.GetAsync(IdentityTokenStorageKey);
    }

    /// <summary>
    ///     This makes the AuthorizedHttpClient use the Bearer token
    /// </summary>
    /// <param name="token"></param>
    private void SetToken(string? token)
    {
        _authenticatedHttpClient.Client.DefaultRequestHeaders.Authorization =
            token == null ? null : new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task<bool> TryTestLoginAsync()
    {
        var testLogin = await _authenticatedHttpClient.TestLogin();

        if (testLogin || string.IsNullOrEmpty(_refreshToken)) return testLogin;

        var result = await TryRefreshTokenAsync();
        return result;

    }

    /// <summary>
    ///     Loads the credentials, call this once upon app start up
    /// </summary>
    /// <returns></returns>
    public async Task LoadCredentialsAsync()
    {
        var authToken = await GetAuthTokenAsync();
        var refreshToken = await GetRefreshTokenAsync();
        var identityToken = await GetIdentityTokenAsync();

        SetToken(authToken);
        _refreshToken = refreshToken;
        _identityToken = identityToken;

        if (authToken != null)
        {
            IsLoggedIn = await TryTestLoginAsync();
        }
        AuthChanged?.Invoke(this, IsLoggedIn);
    }

    /// <summary>
    ///     Resets credentials and forgets bearer token.
    /// </summary>
    private void ResetCredentials()
    {
        SecureStorage.Default?.Remove(AuthTokenStorageKey);
        SecureStorage.Default?.Remove(RefreshTokenStorageKey);
        SecureStorage.Default?.Remove(IdentityTokenStorageKey);
        IsLoggedIn = false;
        _identityToken = null;
        _refreshToken = null;
        SetToken(null);
    }

    /// <summary>
    ///     Saves the credentials to SecureStorage, provided by each OS
    /// </summary>
    /// <returns></returns>
    private async Task SaveCredentialsAsync()
    {
        if (AuthResult?.LoginResult?.AccessToken != null)
        {
            await SecureStorage.Default.SetAsync(AuthTokenStorageKey, AuthResult.LoginResult.AccessToken);

            if (!string.IsNullOrEmpty(AuthResult.LoginResult?.RefreshToken))
                await SecureStorage.Default.SetAsync(RefreshTokenStorageKey, AuthResult.LoginResult.RefreshToken);
            if (!string.IsNullOrEmpty(AuthResult.LoginResult?.IdentityToken))
                await SecureStorage.Default.SetAsync(IdentityTokenStorageKey, AuthResult.LoginResult.IdentityToken);
        }
    }

    public async Task<AuthResult> LoginAsync()
    {
        var result = await HandleLoginProcess();
        SetToken(result.LoginResult?.AccessToken);
        AuthResult = result;
        IsLoggedIn = result.LoginResult?.AccessToken != null;
        _refreshToken = result.LoginResult?.RefreshToken;
        _identityToken = result.LoginResult?.IdentityToken;
        if (IsLoggedIn)
            await SaveCredentialsAsync();
        else
            ResetCredentials();
        AuthChanged?.Invoke(null, IsLoggedIn);
        return result;
    }

    public async Task<AuthResult> LogoutAsync()
    {
        var result = await HandleLogoutProcess();
        AuthResult = result;
        ResetCredentials();
        AuthChanged?.Invoke(null, false);
        return result;
    }

    public async Task<AuthResult> RefreshTokenAsync()
    {
        if (_refreshToken == null) return new AuthResult(AuthResultType.Error);

        var result = await HandleRefreshTokenProcessAsync();
        AuthResult = result;
        IsLoggedIn = result.RefreshTokenResult?.AccessToken != null;
        if (IsLoggedIn)
            await SaveCredentialsAsync();
        else
            ResetCredentials();
        AuthChanged?.Invoke(null, IsLoggedIn);
        return result;
    }

    public async Task RedirectToLogin()
    {
        if (Shell.Current.CurrentPage is not LoginPage)
        {
            await Shell.Current.GoToAsync(ApiConfig.Instance.LoginPage);
        }
    }

    private async Task<AuthResult> HandleRefreshTokenProcessAsync()
    {
        if (string.IsNullOrWhiteSpace(_refreshToken)) return new AuthResult(AuthResultType.Error);

        try
        {
            _openIdClient.Options.Browser = new AuthBrowser();
            var refreshTokenResult = await _openIdClient.RefreshTokenAsync(_refreshToken);

            return new AuthResult(!refreshTokenResult.IsError ? AuthResultType.Successful : AuthResultType.Error)
                { RefreshTokenResult = refreshTokenResult };
        }
        catch (TaskCanceledException)
        {
            return new AuthResult(AuthResultType.Cancelled);
        }
        catch (Exception)
        {
            return new AuthResult(AuthResultType.Error);
        }
    }

    private async Task<AuthResult> HandleLogoutProcess()
    {
        try
        {
            _openIdClient.Options.Browser = new AuthBrowser();
            var logoutResult = await _openIdClient.LogoutAsync(new LogoutRequest { IdTokenHint = _identityToken });

            await Toast.Make("Logged out successfully!").Show();
            return new AuthResult(!logoutResult.IsError ? AuthResultType.Successful : AuthResultType.Error)
                { LogoutResult = logoutResult };
        }
        catch (TaskCanceledException)
        {
            await Toast.Make("Logging out was cancelled").Show();
            return new AuthResult(AuthResultType.Cancelled);
        }
        catch (Exception)
        {
            await Toast.Make("Error logging out").Show();
            return new AuthResult(AuthResultType.Error);
        }
    }

    private async Task<AuthResult> HandleLoginProcess()
    {
        try
        {
            _openIdClient.Options.Browser = new AuthBrowser();
            var loginResult = await _openIdClient.LoginAsync(new LoginRequest());

            await Toast.Make("Logged in successfully!").Show();
            return new AuthResult(!loginResult.IsError ? AuthResultType.Successful : AuthResultType.Error)
                { LoginResult = loginResult };
        }
        catch (TaskCanceledException)
        {
            await Toast.Make("Logging in was cancelled").Show();
            return new AuthResult(AuthResultType.Cancelled);
        }
        catch (Exception)
        {
            await Toast.Make("Error logging in").Show();
            return new AuthResult(AuthResultType.Error);
        }
    }

    public async Task<bool> TryRefreshTokenAsync()
    {
        var refreshedToken = await RefreshTokenAsync();
        return refreshedToken.RefreshTokenResult is { IsError: false };
    }

    /// <summary>
    ///     Call this if you execute an API request and return 401 on an authorized page, the token may have expired or the
    ///     user has been logged out
    /// </summary>
    /// <returns></returns>
    public async Task<bool> HandleUnauthorizedHttpRequest()
    {
        if (!IsLoggedIn)
        {
            return true;
        }

        if (Shell.Current.CurrentPage is LoginPage)
        {
            return true;
        }

        var refreshedToken = await TryRefreshTokenAsync();
        if (refreshedToken) return false;

        ResetCredentials();
        AuthChanged?.Invoke(this, false);
        await RedirectToLogin();

        return false;

    }
}