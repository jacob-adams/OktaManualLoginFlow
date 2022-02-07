using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = new PathString("/Account/SignIn");
    });
//.AddOpenIdConnect(options =>
//{
//    options.ClientId = builder.Configuration.GetValue<string>("Okta:ClientId");
//    options.ClientSecret = builder.Configuration.GetValue<string>("Okta:ClientSecret");
//    options.Authority = $"{EnsureTrailingSlash(builder.Configuration.GetValue<string>("Okta:OktaDomain"))}oauth2/default";
//    options.CallbackPath = new PathString("/Account/Callback");
//    options.SignedOutCallbackPath = new PathString("/Account/SignoutCallback");
//    options.ResponseType = OpenIdConnectResponseType.Code;
//    options.SecurityTokenValidator = new JwtSecurityTokenValidator();
//    options.SaveTokens = true;
//    options.UseTokenLifetime = false;
//    options.Scope.Add("openid");
//    options.Scope.Add("email");
//    options.Scope.Add("profile");
//    options.TokenValidationParameters = new TokenValidationParameters()
//    {
//        ValidAudiences = new List<string>() { builder.Configuration.GetValue<string>("Okta:ClientId") },
//        NameClaimType = "name",
//        RequireExpirationTime = true,
//        RequireSignedTokens = true,
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateIssuerSigningKey = true,
//        ValidateLifetime = true
//    };
//});
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
