using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OktaManualLoginFlow.Models;
using OktaManualLoginFlow.Utility;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

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
            string stateValue = CryptoHelper.GenerateRandomString();
            string nonceValue = CryptoHelper.GenerateRandomString();
            HttpContext.Session.SetString("okta-state", stateValue);
            HttpContext.Session.SetString("okta-nonce", nonceValue);

            ViewData["state"] = stateValue;
            ViewData["nonce"] = nonceValue;

            return View();
        }

        public new async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string state)
        {
            if (!state.Equals(HttpContext.Session.GetString("okta-state")))
            {
                throw new StateMismatchException("state token did not match.");
            }
            var baseUrlString = $"{_configuration.GetValue<string>("Okta:OktaDomain").EnsureTrailingSlash()}oauth2/default";
            var tokenUrl = new Uri($"{baseUrlString}/v1/token");

            //run it through fiddler
            //var proxiedHttpClientHandler = new HttpClientHandler() { UseProxy = true };
            //proxiedHttpClientHandler.Proxy = new WebProxy("127.0.0.1", 8888);

            using HttpClient client = new HttpClient();// proxiedHttpClientHandler);

            TokenResponse? tokenValues = await GetJwtTokens(code, tokenUrl, client);

            JsonWebKeySet keySet = await GetOktaJsonKeySet(baseUrlString, client);

            ClaimsPrincipal claimsPrincipal = ValidateTokenAndGetClaims(baseUrlString, tokenValues, keySet);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

            return RedirectToAction("Index", "Home");
        }

        private ClaimsPrincipal ValidateTokenAndGetClaims(string baseUrlString, TokenResponse? tokenValues, JsonWebKeySet keySet)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenValidationParameters = new TokenValidationParameters()
            {
                IssuerSigningKeys = keySet.GetSigningKeys(),
                ValidAudiences = new List<string>() { _configuration.GetValue<string>("Okta:ClientId") },
                ValidIssuer = baseUrlString,
                NameClaimType = "name",
                RequireExpirationTime = true,
                RequireSignedTokens = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidAlgorithms = new List<string> { SecurityAlgorithms.RsaSha256 },
                ClockSkew = TimeSpan.FromMinutes(2)
            };
            var claimsPrincipal = tokenHandler.ValidateToken(tokenValues?.IdToken, tokenValidationParameters, out SecurityToken securityToken);

            bool nonceMatches = ((JwtSecurityToken)securityToken).Payload.TryGetValue("nonce", out var nonce) && nonce.ToString().Equals(HttpContext.Session.GetString("okta-nonce"));

            if (!nonceMatches)
            {
                throw new SecurityTokenValidationException("The nonce is invalid.");
            }

            return claimsPrincipal;
        }

        private async Task<TokenResponse?> GetJwtTokens(string code, Uri tokenUrl, HttpClient client)
        {
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string?> {
                    { "client_id", _configuration.GetValue<string>("Okta:ClientId") },
                    { "client_secret", _configuration.GetValue<string>("Okta:ClientSecret") },
                    { "grant_type", "authorization_code" },
                    { "code", code },
                    { "redirect_uri", Url.Action("Callback", "Account", null, "https", null) },
                })
            };
            tokenRequest.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            var tokenResponse = await client.SendAsync(tokenRequest);
            return await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
        }

        private async Task<JsonWebKeySet> GetOktaJsonKeySet(string baseUrlString, HttpClient client)
        {
            var keysUrl = new Uri($"{baseUrlString}/v1/keys?clientId={_configuration.GetValue<string>("Okta:ClientId")}");
            var keysRequest = new HttpRequestMessage(HttpMethod.Get, keysUrl);
            var keysResponse = await client.SendAsync(keysRequest);
            return new JsonWebKeySet(await keysResponse.Content.ReadAsStringAsync());
        }
    }
}
