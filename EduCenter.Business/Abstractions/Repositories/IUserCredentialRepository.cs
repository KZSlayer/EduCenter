using EduCenter.Business.Models;

namespace EduCenter.Business.Abstractions.Repositories
{
    public interface IUserCredentialRepository
    {
        Task<UserCredential?> GetByUsernameAsync(
            string username,
            CancellationToken cancellationToken = default);
    }
}
