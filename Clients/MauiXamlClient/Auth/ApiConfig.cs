using IdentityModel;
using MauiXamlClient.Pages.ForecastOverview;
using MauiXamlClient.Pages.ForecastToday;

namespace MauiXamlClient.Auth;
#nullable enable
public class ApiConfig
{
    public static ApiConfig Instance { get; private set; } = new();

#if DEBUG
    public string ApiUrl { get; } = "https://authtest.local:7142";
    public string? ApiTestAuthenticatedUrl { get; } = "https://authtest.local:7142/api/account/profile";
    public string CustomProtocolRedirectUri { get; } = "authtest://processauth";

    public string ClientId { get; } = "Clients.MauiXamlClient";
    public string Scopes { get; } = $"{OidcConstants.StandardScopes.OpenId} {OidcConstants.StandardScopes.Profile}";

    public string AppShellDefaultPageAfterLogin { get; } = nameof(ForecastOverviewPage);
    public string AppShellDefaultPageAnonymous { get; } = nameof(ForecastTodayPage);
    public string LoginPage { get; } = nameof(LoginPage);
    public string LogoutPage { get; } = nameof(LogoutPage);
#else
    public string ApiUrl { get; } = "";
    public string? ApiTestAuthenticatedUrl { get; } = "";
    public string CustomProtocolRedirectUri { get; } = "authtest://processauth";

    public string ClientId { get; } = "Clients.MauiXamlClient";
    public string Scopes { get; } = $"{OidcConstants.StandardScopes.OpenId} {OidcConstants.StandardScopes.Profile}";

    public string AppShellDefaultPageAfterLogin { get; } = nameof(ForecastOverviewPage);
    public string AppShellDefaultPageAnonymous { get; } = nameof(ForecastTodayPage);
    public string LoginPage { get; } = nameof(LoginPage);
    public string LogoutPage { get; } = nameof(LogoutPage);
#endif
}