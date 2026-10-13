namespace EduCenter.Api.Authentication
{
    public interface IAccessTokenService
    {
        IssuedAccessToken Create(string userId);
    }
}
