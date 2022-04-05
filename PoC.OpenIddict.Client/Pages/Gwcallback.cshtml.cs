using IdentityModel.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace PoC.OpnIddict.Client.Pages
{
    public class GwCallback : PageModel
    {
        public string? ResponseText { get; private set; }

        private string OpeniddictUrl = "https://localhost:7063";
        public string ClientUrl { get; set; } = "https://localhost:7216";

        public async Task<ActionResult> OnGetAsync()
        {
            var parsed = QueryHelpers.ParseQuery(Request.QueryString.Value);
            var code = parsed["code"].SingleOrDefault();
            var codeVerifier = IndexModel.CodeVerifier;

            var client = new HttpClient();
            var redirectUri = $"{ClientUrl}/gwcallback";
            var response = await client.RequestAuthorizationCodeTokenAsync(new AuthorizationCodeTokenRequest
            {
                ClientId = "gwClient",
                CodeVerifier = codeVerifier,
                RequestUri = new Uri($"{OpeniddictUrl}/connect/token"),
                RedirectUri = redirectUri,
                ClientSecret = "901564A5-E7FE-42CB-B10D-61EF6A8F3654",
                Code = code
            });

            if (response.IsError)
            {
                ResponseText = $"{response.Error}, {response.ErrorDescription}";
            }
            else
            {
                ResponseText =  response.AccessToken;
            }

            return Page(); 
        }
    }
}
