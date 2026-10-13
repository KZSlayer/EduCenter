using System.ComponentModel.DataAnnotations;

namespace EduCenter.Api.DTOs
{
    public class CartPriceRequestDTO
    {
        [Required]
        [MinLength(1)]
        public Guid[] CourseIds { get; init; } = [];
    }
}
