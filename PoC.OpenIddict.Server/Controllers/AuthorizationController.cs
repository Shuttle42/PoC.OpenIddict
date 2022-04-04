
using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace PoC.OpenIddict.Server.Controllers;

public class AuthorizationController : Controller
{
    private readonly IOpenIddictApplicationManager _applicationManager;

    public AuthorizationController(IOpenIddictApplicationManager applicationManager)
        => _applicationManager = applicationManager;

    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Authorize()
    {
        var result = await HttpContext.AuthenticateAsync();

        if (result is not { Succeeded: true })
        {
            return Challenge(
                authenticationSchemes: OpenIdConnectDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties
                {
                    RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                        Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList())
                });
        }

        // Create a new identity and populate it based on the specified hardcoded identity identifier.
        var identity = new ClaimsIdentity(TokenValidationParameters.DefaultAuthenticationType);

        identity.AddClaim("sub", "mySub"); // test only

        // Allow all the claims resolved from the principal to be copied to the access and identity tokens.
        foreach (var claim in identity.Claims)
        {
            claim.SetDestinations(Destinations.AccessToken, Destinations.IdentityToken);
        }

        var principal = new ClaimsPrincipal(identity);

        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpPost("~/connect/token"), Produces("application/json")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest();
        if (!request.IsAuthorizationCodeGrantType())
        {
            throw new NotImplementedException("The specified grant is not implemented.");
        }

        var application = await _applicationManager.FindByClientIdAsync(request.ClientId) ??
            throw new InvalidOperationException("The application cannot be found.");

        var identity = new ClaimsIdentity(TokenValidationParameters.DefaultAuthenticationType, Claims.Name, Claims.Role);

        // Use the client_id as the subject identifier.
        identity.AddClaim(Claims.Subject,
            await _applicationManager.GetClientIdAsync(application),
            Destinations.AccessToken, Destinations.IdentityToken);

        identity.AddClaim(Claims.Name,
            await _applicationManager.GetDisplayNameAsync(application),
            Destinations.AccessToken, Destinations.IdentityToken);

        var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        if (result is not { Succeeded: true })
        {
            throw new InvalidOperationException("Unable to issue an access token");
        }

        var dmsRefToken = new Claim("MyCustomClaim", "12345").SetDestinations(Destinations.AccessToken);
        identity.AddClaim(dmsRefToken);

        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
}
