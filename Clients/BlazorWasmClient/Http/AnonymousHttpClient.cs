using BlazorWasmClient.Http.Interfaces;

namespace BlazorWasmClient.Http;

public class AnonymousHttpClient : IAnonymousHttpClient
{
    public AnonymousHttpClient(HttpClient httpClient)
    {
        Client = httpClient;
    }

    public HttpClient Client { get; }
}