using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Net;
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

                options.AuthenticationMethod = OpenIdConnectRedirectBehavior.FormPost;

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
                       .EnableAuthorizationRequestCaching()
                       .EnableStatusCodePagesIntegration(); 
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

        app.UseCookiePolicy();

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

    private Task RemoteAuthFail(RemoteFailureContext context) 
    { 
        context.Response.Redirect("https://localhost:7216/gwcallback" + context.Request.QueryString); 
        context.HandleResponse(); 
        return Task.CompletedTask; 
    }

    //private Task OnRedirectToIdentityProvider(RedirectContext redirectContext)
    //{
    //    if (redirectContext.Request.Path.StartsWithSegments("/connect"))
    //    {
    //        if (redirectContext.Response.StatusCode == (int)HttpStatusCode.OK)
    //        {
    //            redirectContext.ProtocolMessage.State = options.StateDataFormat.Protect(redirectContext.Properties);
    //            redirectContext.Response.StatusCode = (int)HttpStatusCode.OK;
    //            redirectContext.Response.Headers["Location"] = redirectContext.ProtocolMessage.CreateAuthenticationRequestUrl();
    //        }
    //        redirectContext.HandleResponse();
    //    }
    //    return Task.CompletedTask;
    //}
}
