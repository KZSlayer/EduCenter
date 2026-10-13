using System.ComponentModel.DataAnnotations;

namespace EduCenter.Api.DTOs
{
    public sealed class LoginRequestDTO
    {
        [Required]
        public string Username { get; init; } = string.Empty;

        [Required]
        public string Password { get; init; } = string.Empty;
    }
}
