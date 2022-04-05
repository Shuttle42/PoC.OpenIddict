using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Cryptography;
using System.Text;

namespace PoC.OpnIddict.Client.Pages
{
    public class IndexModel : PageModel
    {
        public static string? CodeVerifier { get; private set; }
        public string ClientUrl { get; set; } = "https://localhost:7216";
        public string OpeniddictUrl { get; set; } = "https://localhost:7063";
        public static string Client { get; set; } = "gwClient";

        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }

        public async Task<ActionResult> OnPostGetAsync()
        {
            return await RunAuthorizeRequestAsync(true);
        }

        public async Task<ActionResult> OnPostPostAsync()
        {
            return await RunAuthorizeRequestAsync(false);
        }

        private async Task<ActionResult> RunAuthorizeRequestAsync(bool bGet)
        {
            CodeVerifier = Guid.NewGuid().ToString();
            string codeChallenge;
            using (var sha256 = SHA256.Create())
            {
                var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(CodeVerifier));
                codeChallenge = Base64UrlEncode(challengeBytes);
            }

            var callbackUrl = $"{ClientUrl}/gwcallback";

            if (bGet)
            {
                //GET request; for test purposes
                var queryBuilder = new QueryBuilder
                {
                    { "client_id", Client },
                    { "redirect_uri", callbackUrl },
                    { "response_type", "code" },
                    { "state", Guid.NewGuid().ToString("D") },
                    { "nonce", Guid.NewGuid().ToString("D") },
                    { "code_challenge", codeChallenge },
                    { "code_challenge_method", "S256" },
                    { "myParam", "123" },
                };

                var redirectUrl = new UriBuilder(OpeniddictUrl) { Path = "/connect/authorize", Query = queryBuilder.ToString() };
                return Redirect(redirectUrl.Uri.ToString());
            }
            else
            {
                // POST Request
                var values = new Dictionary<string, string>
                {
                    { "client_id", Client },
                    { "redirect_uri", callbackUrl },
                    { "response_type", "code" },
                    { "state", Guid.NewGuid().ToString("D") },
                    { "nonce", Guid.NewGuid().ToString("D") },
                    { "code_challenge", codeChallenge },
                    { "code_challenge_method", "S256" },
                    { "myParam", "123" },
                };

                var content = new FormUrlEncodedContent(values);

                using HttpClientHandler httpClientHandler = new();
                httpClientHandler.AllowAutoRedirect = false;
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true;
                using (var client = new HttpClient(httpClientHandler))
                {
                    var response = await client.PostAsync($"{OpeniddictUrl}/connect/authorize", content);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK || response.StatusCode == System.Net.HttpStatusCode.Found)
                    {
                        return Redirect(response.Headers.Location!.ToString());
                    }
                    else
                    {
                        var error = "n/a";
                        if (response.ReasonPhrase != null)
                        {
                            error = response.ReasonPhrase;
                        }
                        return BadRequest(error);
                    }
                }
            }
        }
        private string Base64UrlEncode(byte[] arg)
        {
            return Convert.ToBase64String(arg).Split('=')[0].Replace('+', '-').Replace('/', '_');
        }
    }
}