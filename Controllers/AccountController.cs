using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using OktaManualLoginFlow.Utility;
using System.Net;

namespace OktaManualLoginFlow.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;

        public AccountController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SignIn([FromForm] string sessionToken)
        {
            if (!HttpContext.User.Identity?.IsAuthenticated == true)
            {
                //var properties = new AuthenticationProperties();
                //properties.Items.Add(OktaParams.SessionToken, sessionToken);
                //properties.RedirectUri = "/Home/";

                //return Challenge(properties, OktaDefaults.MvcAuthenticationScheme);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public override SignOutResult SignOut()
        {
            return new SignOutResult(
                new[]
                {
                     //OktaDefaults.MvcAuthenticationScheme,
                     CookieAuthenticationDefaults.AuthenticationScheme,
                },
                new AuthenticationProperties { RedirectUri = "/Home/" });
        }

        [HttpPost]
        public async Task<IActionResult> Callback([FromForm] string code, [FromForm] string state)
        {
            var url = new Uri($"{_configuration.GetValue<string>("Okta:OktaDomain").EnsureTrailingSlash()}oauth2/default/v1/token");

            //run it through fiddler
            var proxiedHttpClientHandler = new HttpClientHandler() { UseProxy = true };
            proxiedHttpClientHandler.Proxy = new WebProxy("127.0.0.1", 8888);

            using HttpClient client = new HttpClient(proxiedHttpClientHandler);
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string?> {
                    { "client_id", _configuration.GetValue<string>("Okta:ClientId") },
                    { "client_secret", _configuration.GetValue<string>("Okta:ClientSecret") },
                    { "grant_type", "authorization_code" },
                    { "code", code },
                    { "redirect_uri", Url.Action("Callback", "Account", null, "https", null) },
                })
            };
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.SendAsync(request);
            var token = response.Content.ReadAsStringAsync().Result;

            //HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, jwtTokenClaims, authenticationProperties)
            return RedirectToAction("Index", "Home");
        }
    }
}
