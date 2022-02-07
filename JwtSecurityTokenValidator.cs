using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

internal class JwtSecurityTokenValidator : ISecurityTokenValidator
{
    private readonly JwtSecurityTokenHandler _handler;

    public JwtSecurityTokenValidator()
    {
        _handler = new JwtSecurityTokenHandler();
    }

    public bool CanValidateToken => _handler.CanValidateToken;

    public int MaximumTokenSizeInBytes { get => _handler.MaximumTokenSizeInBytes; set => throw new NotImplementedException(); }

    public bool CanReadToken(string securityToken)
    {
        return _handler.CanReadToken(securityToken);
    }

    public ClaimsPrincipal ValidateToken(string securityToken, TokenValidationParameters validationParameters, out SecurityToken validatedToken) => _handler.ValidateToken(securityToken, validationParameters, out validatedToken);
}