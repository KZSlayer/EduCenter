using System.Text;

namespace EduCenter.Api.Authentication
{
    public sealed class JwtOptions
    {
        public const string SectionName = "Jwt";

        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string SigningKey { get; set; } = string.Empty;
        public int AccessTokenLifetimeMinutes { get; set; } = 30;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Issuer))
            {
                throw new InvalidOperationException("JWT issuer is not configured.");
            }

            if (string.IsNullOrWhiteSpace(Audience))
            {
                throw new InvalidOperationException("JWT audience is not configured.");
            }

            if (AccessTokenLifetimeMinutes <= 0)
            {
                throw new InvalidOperationException("JWT access token lifetime must be positive.");
            }

            GetSigningKeyBytes();
        }

        public byte[] GetSigningKeyBytes()
        {
            if (string.IsNullOrWhiteSpace(SigningKey))
            {
                throw new InvalidOperationException("JWT signing key is not configured.");
            }

            var signingKey = Encoding.UTF8.GetBytes(SigningKey);
            if (signingKey.Length < 32)
            {
                throw new InvalidOperationException("JWT signing key must contain at least 32 UTF-8 bytes.");
            }

            return signingKey;
        }
    }
}
