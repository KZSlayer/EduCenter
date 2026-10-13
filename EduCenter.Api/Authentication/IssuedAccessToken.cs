namespace EduCenter.Api.Authentication
{
    public sealed record IssuedAccessToken(
        string Token,
        DateTimeOffset ExpiresAtUtc);
}
