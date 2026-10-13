using System.ComponentModel.DataAnnotations;

namespace EduCenter.Api.DTOs
{
    public sealed class CartPriceRequestDTO : IValidatableObject
    {
        [Required]
        [MinLength(1)]
        [MaxLength(CartPriceRequestLimits.MaximumCourseCount)]
        public string[] CourseIds { get; init; } = [];

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (CourseIds is null)
            {
                yield break;
            }

            if (CourseIds.Length > CartPriceRequestLimits.MaximumCourseCount)
            {
                yield break;
            }

            var seenCourseIds = new HashSet<string>(StringComparer.Ordinal);

            for (var index = 0; index < CourseIds.Length; index++)
            {
                var courseId = CourseIds[index];

                if (string.IsNullOrWhiteSpace(courseId))
                {
                    yield return new ValidationResult(
                        "Course IDs must not be null, empty, or whitespace.",
                        [$"{nameof(CourseIds)}[{index}]"]);
                    continue;
                }

                if (courseId.Length > CartPriceRequestLimits.MaximumCourseIdLength)
                {
                    yield return new ValidationResult(
                        $"Course IDs must not exceed {CartPriceRequestLimits.MaximumCourseIdLength} characters.",
                        [$"{nameof(CourseIds)}[{index}]"]);
                }

                if (!seenCourseIds.Add(courseId))
                {
                    yield return new ValidationResult(
                        "CourseIds must not contain duplicate values.",
                        [nameof(CourseIds)]);
                }
            }
        }
    }
}
