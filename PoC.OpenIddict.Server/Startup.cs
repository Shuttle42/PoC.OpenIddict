using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace PoC.OpenIddict.Server;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllersWithViews();
        services.AddRazorPages();

        services.AddDbContext<DbContext>(options =>
        {
            options.UseInMemoryDatabase("db");
            options.UseOpenIddict();
        });

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
            .AddCookie()
            .AddOpenIdConnect(options =>
            {
                options.Authority = "https://localhost:7049";
                options.ClientId = "oidcDmsApi";
                options.ClientSecret = "MySecret";
                options.ResponseType = OpenIdConnectResponseType.Code;

                options.ResponseMode = "query";
                options.Scope.Add("dmsApi.read");
                options.SaveTokens = true;
                options.UsePkce = true;

                options.CallbackPath = "/signin-oidc";
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });

        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                       .UseDbContext<DbContext>();
            })
            .AddServer(options =>
            {
                options.SetAuthorizationEndpointUris("/connect/authorize")
                       .SetTokenEndpointUris("/connect/token");

                options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles);

                options.AllowAuthorizationCodeFlow();
                options.DisableAccessTokenEncryption(); // for test purposes

                options.AddDevelopmentEncryptionCertificate()
                       .AddDevelopmentSigningCertificate();

                options.UseAspNetCore()
                       .EnableTokenEndpointPassthrough()
                       .EnableAuthorizationEndpointPassthrough()
                       .EnableStatusCodePagesIntegration(); // ??
            })

            // Register the OpenIddict validation components.
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });

        services.AddHostedService<Worker>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(options =>
        {
            options.MapControllers();
            options.MapDefaultControllerRoute();
            options.MapRazorPages();
        });

        app.UseWelcomePage();
    }
}
