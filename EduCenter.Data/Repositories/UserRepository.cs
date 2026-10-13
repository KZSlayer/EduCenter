using EduCenter.Business.Abstractions.Repositories;
using EduCenter.Business.Models;
using Microsoft.AspNetCore.Identity;

namespace EduCenter.Data.Repositories
{
    public class UserRepository : IUserRepository, IUserCredentialRepository
    {
        private static readonly UserSeed[] UserSeeds =
        {
            new("1", true, "graduate-demo", "GraduateDemo!2026"),
            new("2", false, "student-demo", "StudentDemo!2026")
        };

        private readonly Dictionary<string, User> _users = new(StringComparer.Ordinal);
        private readonly Dictionary<string, UserCredential> _credentials =
            new(StringComparer.OrdinalIgnoreCase);

        public UserRepository(IPasswordHasher<UserCredential> passwordHasher)
        {
            foreach (var seed in UserSeeds)
            {
                var user = new User
                {
                    Id = seed.Id,
                    IsGraduate = seed.IsGraduate
                };

                _users.Add(user.Id, user);

                var credential = new UserCredential(user.Id, seed.Username, string.Empty);
                _credentials.Add(
                    seed.Username,
                    credential with
                    {
                        PasswordHash = passwordHasher.HashPassword(credential, seed.Password)
                    });
            }
        }

        public Task<User?> GetByUserIdAsync(
            string id,
            CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<User?>(cancellationToken);
            }

            if (!_users.TryGetValue(id, out var user))
            {
                return Task.FromResult<User?>(null);
            }

            return Task.FromResult<User?>(new User
            {
                Id = user.Id,
                IsGraduate = user.IsGraduate
            });
        }

        public Task<UserCredential?> GetByUsernameAsync(
            string username,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _credentials.TryGetValue(username.Trim(), out var credential);
            return Task.FromResult(credential);
        }

        private sealed record UserSeed(
            string Id,
            bool IsGraduate,
            string Username,
            string Password);
    }
}
