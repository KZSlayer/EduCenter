namespace EduCenter.Business.Services
{
    public interface IUserAuthenticationService
    {
        Task<string?> AuthenticateAsync(
            string username,
            string password,
            CancellationToken cancellationToken = default);
    }
}
