
# How to Implement WebAuthenticator with OAuth

This tutorial will guide you on how to implement WebAuthenticator with OAuth in your .NET MAUI application.

## Preparations

Before you begin, install the following NuGet packages to your MAUI project:

- `IdentityModel.OidcClient`
- `CommunityToolkit.Maui` (optional, for toast notifications)

## Injectable HttpClient

In .NET MAUI, we typically use HttpClient to make API calls. For our app to use the HttpClient, we need to register it as a service in our MauiProgram.cs file. By registering HttpClient as a singleton, we ensure that only one instance of HttpClient is created and used throughout the application's lifetime. This is a recommended practice as it allows for connection reuse, improving the performance of the application.

Once the HttpClient is registered, we can inject it into our view models, allowing us to utilize it for making HTTP requests.

We must add a singleton HTTP client. We do this in MauiProgram.cs. After setting up the 'builder', we add:

```csharp
builder.Services.AddSingleton(new HttpClient());
```
[See example file](../Clients/MauiXamlClient/MauiProgram.cs#L35)

From now on, we inject HttpClient into the constructor and store it in a private readonly field so you can make HTTP requests that use the `Authentication` header of the client. You will have to inject your view model and the page you're using as well.

```csharp
private readonly HttpClient _httpClient;

public ForecastTodayViewModel(HttpClient httpClient)
{
    _httpClient = httpClient;
}

[RelayCommand]
private async Task LoadData()
{
    var forecast =
        await _httpClient.GetFromJsonAsync<WeatherForecast>($"{ApiConfig.Instance.ApiUrl}/api/weatherforecast");
}
```
[See example file](../Clients/MauiXamlClient/Pages/ForecastToday/ForecastTodayViewModel.cs#L17)

```csharp
builder.Services.AddTransient<ForecastTodayViewModel>();
builder.Services.AddTransient<ForecastTodayPage>();
```
[See example file](../Clients/MauiXamlClient/MauiProgram.cs#L41)

## Setup Custom URL Scheme

### Android

Add the following XML inside the `<manifest>` tag in your `AndroidManifest.xml` file, just after `<uses-permission>`:

```xml
<queries>
    <intent>
        <action android:name="android.support.customtabs.action.CustomTabsService" />
    </intent>
</queries>
```
[See example file](../Clients/MauiXamlClient/Platforms/Android/AndroidManifest.xml)

Add a new `WebAuthenticationCallbackActivity.cs` class in `Platforms/Android`. This class will handle the custom URL scheme:

```csharp
[Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
[IntentFilter(new[] { AndroidLibrary.Content.Intent.ActionView },
    Categories = new[] { AndroidLibrary.Content.Intent.CategoryDefault, AndroidLibrary.Content.Intent.CategoryBrowsable, AndroidLibrary.Content.Intent.ActionView },
    DataScheme = CallbackScheme)]
public class WebAuthenticationCallbackActivity : WebAuthenticatorCallbackActivity
{
    private const string CallbackScheme = "authtest";
}
```
[See example file](../Clients/MauiXamlClient/Platforms/Android/WebAuthenticationCallbackActivity.cs)

### iOS

Add the following XML inside the `<dict>` tag in your `Platforms/iOS/Info.plist` file:

```xml
<key>CFBundleURLTypes</key>
<array>
    <dict>
        <key>CFBundleURLName</key>
        <string>MauiXamlAuthTestClient</string>
        <key>CFBundleURLSchemes</key>
        <array>
            <string>authtest</string>
        </array>
        <key>CFBundleTypeRole</key>
        <string>Editor</string>
    </dict>
</array>
```
[See example file](../Clients/MauiXamlClient/Platforms/iOS/Info.plist)

### Windows

Add the following XML inside the `<Application>` tag in your `Package.appxmanifest` file, just after `</uap:VisualElements>`:

```xml
<Extensions>
    <uap:Extension Category="windows.protocol">
        <uap:Protocol Name="authtest">
            <uap:DisplayName>Auth test app</uap:DisplayName>
        </uap:Protocol>
    </uap:Extension>
</Extensions>
```
[See example file](../Clients/MauiXamlClient/Platforms/Windows/Package.appxmanifest)

## Implement the AuthBrowser class

Create a new `AuthBrowser.cs` file:

```csharp
public class AuthBrowser : IBrowser
{
    public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
    {
        var authResult = await WebAuthenticator.Default
            .AuthenticateAsync(new Uri(options.StartUrl), new Uri(ApiConfig.Instance.CustomProtocolRedirectUri));

        var res = ParseAuthenticatorResult(authResult.Properties);

        return new BrowserResult
        {
            Response = res
        };
    }

    private static string ParseAuthenticatorResult(Dictionary<string, string>? resultProperties)
    {
        if (resultProperties == null) throw new InvalidOperationException();

        string[] properties = { "code", "scope", "session_state" };
        var values = properties
            .Select(p => resultProperties.TryGetValue(p, out var val) && val != null ? $"{p}={val}" : null)
            .Where(s => s != null);

        var state = resultProperties.TryGetValue("state", out var stateVal) && stateVal != null
            ? $"state={HttpUtility.UrlDecode(stateVal)}"
            : null;
        if (state != null) values = values.Append(state);

        var url = $"{ApiConfig.Instance.CustomProtocolRedirectUri}#{string.Join("&", values)}";

        return url;
    }
}
```
[See example file](../Clients/MauiXamlClient/Auth/AuthBrowser.cs)

## Implement the login/logout process

In your login page or authentication service, declare the `oidcClient` and trigger the login/logout/refresh process from a command or in `OnParentSet`. You can use the Login, Logout, RefreshToken functions from OidcClient. Also inject the HttpClient that you added in the first step.

```csharp
private readonly HttpClient _httpClient;
public AuthService(HttpClient client)
{
    _client = client;
}

public async Task StartLogin()
{
    var oidcClient = new OidcClient(new OidcClientOptions
    {
        Authority = "https://authtest.local:7142",
        ClientId = "Clients.MauiXamlClient",
        Scope = "openid profile",
        RedirectUri = "authtest://processauth",
        HttpClientFactory = _ => new HttpClient(),
        PostLogoutRedirectUri = "authtest://processauth"
    });
    try
    {
        oidcClient.Options.Browser = new AuthBrowser();
        var loginResult = await oidcClient.LoginAsync(new LoginRequest());
        // loginResult will contain token and other information returned from login
        await Toast.Make("Logged in successfully!").Show();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult?.AccessToken);
    }
    catch (TaskCanceledException)
    {
        await Toast.Make("Logging in was cancelled").Show();
    }
    catch (Exception)
    {
        await Toast.Make("Error logging in").Show();
    }
}

public async Task StartLogout()
{
    var oidcClient = new OidcClient(new OidcClientOptions
    {
        Authority = "https://authtest.local:7142",
        ClientId = "Clients.MauiXamlClient",
        Scope = "openid profile",
        RedirectUri = "authtest://processauth",
        HttpClientFactory = _ => new HttpClient(),
        PostLogoutRedirectUri = "authtest://processauth"
    });
    try
    {
        oidcClient.Options.Browser = new AuthBrowser();
        var logoutResult = await oidcClient.LogoutAsync(new LogoutRequest());
        // loginResult will contain information returned from logout
        await Toast.Make("Logged out successfully!").Show();
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }
    catch (TaskCanceledException)
    {
        await Toast.Make("Logging out was cancelled").Show();
    }
    catch (Exception)
    {
        await Toast.Make("Error logging out").Show();
    }
}
```
[See example file](../Clients/MauiXamlClient/Auth/Services/AuthService.cs#L230)

Do not forget to clear the header after you log out (after you get a logoutResult):
```csharp
_httpClient.DefaultRequestHeaders.Authorization = null;
```

Remember to replace `"authtest"` with your own URL scheme, for example `"avans"` that would result in `"avans://processauth"`.

That's it! You have now successfully implemented WebAuthenticator with OAuth in your .NET MAUI application. If you need a more advanced example, view the [example authentication service](../Clients/MauiXamlClient/Auth/Services/AuthService.cs).
