#nullable enable
using System.Web;
using IdentityModel.OidcClient.Browser;
using IBrowser = IdentityModel.OidcClient.Browser.IBrowser;
#if WINDOWS
using WebAuthenticator = WinUIEx.WebAuthenticator;
#endif

namespace MauiXamlClient.Auth;

public class AuthBrowser : IBrowser
{
    public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
    {
        var authResult =
#if WINDOWS
            await WebAuthenticator
                .AuthenticateAsync(new Uri(options.StartUrl), new Uri(ApiConfig.Instance.CustomProtocolRedirectUri), cancellationToken);
#else
                    await WebAuthenticator.Default
                .AuthenticateAsync(new Uri(options.StartUrl), new Uri(ApiConfig.Instance.CustomProtocolRedirectUri));
#endif

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