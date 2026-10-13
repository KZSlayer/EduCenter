using EduCenter.Business.Models;

namespace EduCenter.Business.Abstractions.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByUserIdAsync(
            string id,
            CancellationToken cancellationToken = default);
    }
}
