using System.Reflection;
using BlazorWasmClient.Http;
using BlazorWasmClient.Http.Interfaces;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace BlazorWasmClient;

public class Program
{
    private const string HttpClientName = "Clients.BlazorWasmClient";

    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        // This way, we have two HTTP clients. The one that uses authentication and the one that doesn't.
        // The reason is the BaseAddressAuthorizationMessageHandler we only want to use in the authorized HTTP client.
        builder.Services.AddHttpClient<IAuthorizedHttpClient, AuthorizedHttpClient>($"{HttpClientName}.Authorized",
                client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
            .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();
        builder.Services.AddHttpClient<IAnonymousHttpClient, AnonymousHttpClient>(HttpClientName,
            client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));

        builder.Services.AddApiAuthorization(options =>
        {
            options.ProviderOptions.ConfigurationEndpoint = $"_configuration/Clients.BlazorWasmClient";
        });

        await builder.Build().RunAsync();
    }
}