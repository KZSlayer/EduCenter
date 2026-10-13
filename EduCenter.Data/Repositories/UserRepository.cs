using EduCenter.Business.Abstractions.Repositories;
using EduCenter.Business.Models;

namespace EduCenter.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private List<User> _users = new();

        public UserRepository()
        {
            _users.Add(new User
            {
                Id = "1",
                IsGraduate = true
            });
            _users.Add(new User
            {
                Id = "2",
                IsGraduate = false
            });
        }

        public async Task<User?> GetByUserIdAsync(
            string id,
            CancellationToken cancellationToken = default)
        {
            return _users.FirstOrDefault(u => u.Id == id);
        }
    }
}
