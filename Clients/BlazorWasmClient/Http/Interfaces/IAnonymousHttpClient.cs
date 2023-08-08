namespace BlazorWasmClient.Http.Interfaces;

public interface IAnonymousHttpClient
{
    HttpClient Client { get; }
}