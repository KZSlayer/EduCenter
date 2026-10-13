using EduCenter.Business.Abstractions.Repositories;
using EduCenter.Business.Models;

namespace EduCenter.Business.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User?> GetByUserIdAsync(
            string id,
            CancellationToken cancellationToken = default)
        {
            return await _userRepository.GetByUserIdAsync(id, cancellationToken);
        }
    }
}
