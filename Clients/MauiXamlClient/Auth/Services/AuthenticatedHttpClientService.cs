#nullable enable
using System.Net;

namespace MauiXamlClient.Auth.Services;

public class AuthenticatedHttpClientService
{
    public AuthenticatedHttpClientService()
    {
        Client = new HttpClient();
    }

    public HttpClient Client { get; set; }

    /// <summary>
    ///     This function needs to be called after loading credentials to check if it's still valid.
    /// </summary>
    /// <returns></returns>
    public async Task<bool> TestLogin()
    {
        if (ApiConfig.Instance.ApiTestAuthenticatedUrl == null) return true;

        try
        {
            var response = await Client.GetAsync(new Uri(ApiConfig.Instance.ApiTestAuthenticatedUrl));
            return response is { IsSuccessStatusCode: true, StatusCode: not (HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden) };
        }
        catch
        {
        }

        return false;
    }
}