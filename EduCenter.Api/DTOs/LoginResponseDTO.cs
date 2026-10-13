namespace EduCenter.Api.DTOs
{
    public sealed record LoginResponseDTO(
        string AccessToken,
        string TokenType,
        DateTimeOffset ExpiresAtUtc);
}
