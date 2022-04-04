using IdentityServer4.Models;

namespace DemoDmsServer.IdentityConfig
{
    public class Resources
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new[]
            {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email(),
            new IdentityResource
            {
                Name = "role",
                UserClaims = new List<string> {"role"}
            }
        };
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new[]
            {
            new ApiResource
            {
                Name = "dmsApi",
                DisplayName = "Specific DMS Api",
                Description = "Allow the application to access Specific Api on your behalf",
                Scopes = new List<string> { "dmsApi.read", "dmsApi.write"},
                ApiSecrets = new List<Secret> {new Secret("MySecret".Sha256())},
                UserClaims = new List<string> {"role"}
            }
        };
        }
    }
}
