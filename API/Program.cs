using AuthenticationSample.Data;
using AuthenticationSample.Models;
using AuthenticationSample.Swagger;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AuthenticationSample;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.Configure<Config>(builder.Configuration)
            .AddScoped(sp =>
                sp.GetRequiredService<IOptionsSnapshot<Config>>()
                    .Value); // Typed app configuration, can be used in DI-injected classes by using Config as a constructor param

        // This sets up the MySQL database and Entity Framework DB context.
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                               throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(connectionString, new MySqlServerVersion(new Version())));

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services
            .AddIdentity<ApplicationUser,
                IdentityRole>(options =>
                options.SignIn.RequireConfirmedAccount =
                    false) // NOTE: the error for 'your account is not confirmed yet' is quite cryptic
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultUI()
            .AddDefaultTokenProviders();

        if (builder.Environment.IsDevelopment()) // Do NOT expose Swagger in production
        {
            // Swagger can be accessed at https://authtest.local:7142/swagger/index.html
            builder.Services
                .AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerGenOptions>();
            builder.Services.AddSwaggerGen();
        }

        // Adds Identity Server, clients are loaded from appsettings
        builder.Services.AddIdentityServer(options =>
            {
                if (!builder.Environment.IsDevelopment())
                    return; // To make sure we can find errors in the console in debug mode
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            })
            // You could add Facebook, Microsoft and Google here and more!
            .AddApiAuthorization<ApplicationUser, ApplicationDbContext>();

        // We will use the JWT scheme for controllers, we will not be setting it as default as for Identity pages cookies are used
        builder.Services.AddAuthentication()
            .AddJwtBearer(options =>
            {
                options.Authority = builder.Configuration["AppUrl"];
                options.TokenValidationParameters.ValidateAudience = false;

                options.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };
            });
        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseIdentityServer();
        app.UseAuthorization();

        await DatabaseSeeder.SeedDatabase(app.Services); // Ensure the roles and a default user exist

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(setup =>
            {
                // Sets Swagger OAuth settings
                setup.OAuthClientId("Clients.Swagger");
                setup.OAuthAppName("Clients.Swagger");
                setup.OAuthScopeSeparator(" ");
                setup.OAuthUsePkce();
                setup.SwaggerEndpoint("/swagger/v1/swagger.json", "Version 1.0");
            });
        }

        app.MapRazorPages();
        app.MapControllers();
        app.MapFallbackToFile("index.html");

        await app.RunAsync();
    }
}