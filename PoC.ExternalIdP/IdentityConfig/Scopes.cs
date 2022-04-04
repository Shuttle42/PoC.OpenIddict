using IdentityServer4.Models;

namespace DemoDmsServer.IdentityConfig
{
    public class Scopes
    {
        public static IEnumerable<ApiScope> GetApiScopes()
        {
            return new[]
            {
            new ApiScope("dmsApi.read", "Read Access to Specific DMS API"),
            new ApiScope("dmsApi.write", "Write Access to Specific DMS API"),
        };
        }
    }
}
