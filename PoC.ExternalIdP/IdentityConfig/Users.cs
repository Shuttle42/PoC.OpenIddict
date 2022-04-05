using IdentityModel;
using IdentityServer4.Test;
using System.Security.Claims;

namespace DemoDmsServer.IdentityConfig
{
    public class Users
    {
        public static List<TestUser> Get()
        {
            return new List<TestUser>
        {
            new TestUser
            {
                SubjectId = "42100500",
                Username = "mei",
                Password = "test",
                Claims = new List<Claim>
                {
                    new Claim(JwtClaimTypes.Email, "joschka@mycompany.com"),
                    new Claim(JwtClaimTypes.Role, "developer"),
                    new Claim(JwtClaimTypes.WebSite, "https://google.de"),
                    new(JwtClaimTypes.Scope, "dmsApi.read"), //mei
                    new(JwtClaimTypes.Scope, "openid"),
                    new(JwtClaimTypes.Scope, "profile"),
                    new(JwtClaimTypes.Subject, "42100500")
                }
            }
        };
        }
    }
}
