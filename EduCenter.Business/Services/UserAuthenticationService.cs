using EduCenter.Business.Abstractions.Repositories;
using EduCenter.Business.Models;
using Microsoft.AspNetCore.Identity;

namespace EduCenter.Business.Services
{
    public sealed class UserAuthenticationService : IUserAuthenticationService
    {
        private readonly IUserCredentialRepository _userCredentialRepository;
        private readonly IPasswordHasher<UserCredential> _passwordHasher;
        private readonly IUserService _userService;

        public UserAuthenticationService(
            IUserCredentialRepository userCredentialRepository,
            IPasswordHasher<UserCredential> passwordHasher,
            IUserService userService)
        {
            _userCredentialRepository = userCredentialRepository;
            _passwordHasher = passwordHasher;
            _userService = userService;
        }

        public async Task<string?> AuthenticateAsync(
            string username,
            string password,
            CancellationToken cancellationToken = default)
        {
            var credential = await _userCredentialRepository.GetByUsernameAsync(
                username,
                cancellationToken);

            if (credential is null)
            {
                return null;
            }

            var passwordVerification = _passwordHasher.VerifyHashedPassword(
                credential,
                credential.PasswordHash,
                password);

            if (passwordVerification == PasswordVerificationResult.Failed)
            {
                return null;
            }

            var user = await _userService.GetByUserIdAsync(
                credential.UserId,
                cancellationToken);

            return user is null ? null : credential.UserId;
        }
    }
}
