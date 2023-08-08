#nullable enable
using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Results;

namespace MauiXamlClient.Auth;

public class AuthResult
{
    public AuthResult(AuthResultType authResult)
    {
        ResultType = authResult;
    }

    public AuthResultType ResultType { get; set; }

    public LoginResult? LoginResult { get; set; }

    public LogoutResult? LogoutResult { get; set; }

    public RefreshTokenResult? RefreshTokenResult { get; set; }
}