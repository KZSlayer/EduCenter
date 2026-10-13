namespace EduCenter.Business.Models
{
    public sealed record UserCredential(
        string UserId,
        string Username,
        string PasswordHash);
}
