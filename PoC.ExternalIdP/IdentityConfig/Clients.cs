using IdentityServer4;
using IdentityServer4.Models;

namespace DemoDmsServer.IdentityConfig
{
    public class Clients
    {
        public static IEnumerable<Client> Get()
        {
            return new List<Client>
        {
            new Client
            {
                ClientId = "dmsApi",
                ClientName = "ASP.NET Core Specific Dms Api",
                AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                ClientSecrets = new List<Secret> {new Secret("MySecret".Sha256())},
                AllowedScopes = new List<string> { "dmsApi.read" },
                //RedirectUris = new List<string> { "https://localhost:44307/callback" }
            },
            new Client
            {
                ClientId = "oidcDmsApi",
                ClientName = "ASP.NET Core Specific Dms Api",
                //ClientSecrets = new List<Secret> {new Secret("MySecret".Sha256())},
                RequireClientSecret = false,
                AllowedGrantTypes = GrantTypes.Code,
                RedirectUris = new List<string> 
                { 
                    "https://localhost:5001/callback",
                    "https://localhost:7216/gwcallback",
                    "https://localhost:7063/signin-oidc"
                },
                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    "role",
                    "dmsApi.read"
                },

                RequirePkce = true,
                //AllowPlainTextPkce = false,
                AllowAccessTokensViaBrowser =true,
                AlwaysIncludeUserClaimsInIdToken = true,
                Enabled = true,
                AlwaysSendClientClaims = true,
            }
        };
        }
    }
}
