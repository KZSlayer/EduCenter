using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EduCenter.Api.Authentication
{
    public sealed class JwtAccessTokenService : IAccessTokenService
    {
        private readonly JwtOptions _options;
        private readonly TimeProvider _timeProvider;

        public JwtAccessTokenService(
            IOptions<JwtOptions> options,
            TimeProvider timeProvider)
        {
            _options = options.Value;
            _timeProvider = timeProvider;
        }

        public IssuedAccessToken Create(string userId)
        {
            _options.Validate();

            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(_options.GetSigningKeyBytes()),
                SecurityAlgorithms.HmacSha256);

            var issuedAt = _timeProvider.GetUtcNow();
            var expiresAt = issuedAt.AddMinutes(_options.AccessTokenLifetimeMinutes);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
            };
            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: issuedAt.UtcDateTime,
                expires: expiresAt.UtcDateTime,
                signingCredentials: signingCredentials);

            return new IssuedAccessToken(
                new JwtSecurityTokenHandler().WriteToken(token),
                expiresAt);
        }
    }
}
