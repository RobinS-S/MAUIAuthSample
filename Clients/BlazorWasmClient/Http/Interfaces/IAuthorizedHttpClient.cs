namespace BlazorWasmClient.Http.Interfaces;

public interface IAuthorizedHttpClient
{
    HttpClient Client { get; }
}