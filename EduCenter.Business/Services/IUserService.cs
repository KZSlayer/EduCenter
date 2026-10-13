using EduCenter.Business.Models;

namespace EduCenter.Business.Services
{
    public interface IUserService
    {
        Task<User?> GetByUserIdAsync(
            string id,
            CancellationToken cancellationToken = default);
    }
}
