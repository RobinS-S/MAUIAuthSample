using System.Security.Authentication;
using AuthenticationSample.Data;
using Duende.IdentityServer;
using IdentityModel.Client;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AuthenticationSample.Swagger;

public class ConfigureSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly Config _settings;

    public ConfigureSwaggerGenOptions(
        IOptions<Config> settings)
    {
        _settings = settings.Value;
    }

    public void Configure(SwaggerGenOptions options)
    {
        var discoveryDocument = GetDiscoveryDocument();

        options.OperationFilter<AuthorizeOperationFilter>();
        options.DescribeAllParametersInCamelCase();
        options.CustomSchemaIds(x => x.FullName);
        options.SwaggerDoc("v1", CreateOpenApiInfo());

        options.AddSecurityDefinition("OAuth2", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri(discoveryDocument.AuthorizeEndpoint),
                    TokenUrl = new Uri(discoveryDocument.TokenEndpoint),
                    Scopes = new Dictionary<string, string>
                    {
                        { IdentityServerConstants.StandardScopes.OpenId, "OpenID" },
                        { IdentityServerConstants.StandardScopes.Profile, "Profile" }
                    }
                }
            },
            Description = "AuthenticationSample API access"
        });
    }

    private DiscoveryDocumentResponse GetDiscoveryDocument()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
            SslProtocols = SslProtocols.Tls12,
            ClientCertificateOptions = ClientCertificateOption.Manual
        };
        return new HttpClient(handler)
            .GetDiscoveryDocumentAsync(_settings.AppUrl)
            .GetAwaiter()
            .GetResult();
    }

    private static OpenApiInfo CreateOpenApiInfo()
    {
        return new OpenApiInfo
        {
            Title = "Test server API",
            Version = "v1",
            Description = "Provides endpoints for weather forecasts."
        };
    }
}