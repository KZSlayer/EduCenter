using EduCenter.Business.Models;
using System.ComponentModel.DataAnnotations;

namespace EduCenter.Api.DTOs
{
    public sealed class LeadRequestDTO
    {
        [Required]
        public string ContactName { get; init; } = string.Empty;

        [Required]
        public string Phone { get; init; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; init; } = string.Empty;

        [Required]
        [EnumDataType(typeof(ClientType))]
        public ClientType? ClientType { get; init; }
    }
}
