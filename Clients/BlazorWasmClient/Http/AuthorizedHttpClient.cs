using BlazorWasmClient.Http.Interfaces;

namespace BlazorWasmClient.Http;

public class AuthorizedHttpClient : IAuthorizedHttpClient
{
    public AuthorizedHttpClient(HttpClient httpClient)
    {
        Client = httpClient;
    }

    public HttpClient Client { get; }
}