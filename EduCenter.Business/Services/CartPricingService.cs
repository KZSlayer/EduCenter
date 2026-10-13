using EduCenter.Business.Pricing;

using EduCenter.Business.Models;

namespace EduCenter.Business.Services
{
    public class CartPricingService : ICartPricingService
    {
        private const int CurrencyDecimalPlaces = 2;

        private const decimal GraduateDiscountPercent = 30m;
        private const decimal HotGroupDiscountPercent = 20m;

        private static readonly TimeSpan HotGroupWindow = TimeSpan.FromDays(5);

        private readonly IUserService _userService;
        private readonly ICourseService _courseService;
        private readonly TimeProvider _timeProvider;

        public CartPricingService(
            IUserService userService,
            ICourseService courseService,
            TimeProvider timeProvider)
        {
            _userService = userService;
            _courseService = courseService;
            _timeProvider = timeProvider;
        }

        public async Task<CartPriceCalculateResponse> CalculateAsync(
            CartPriceCalculateRequest calculateRequest,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(calculateRequest);
            var requestedCourseIds = ValidateRequest(calculateRequest);
            var calculatedAtUtc = _timeProvider.GetUtcNow().ToUniversalTime();

            var coursesTask = _courseService.GetByCourseIdsAsync(
                requestedCourseIds,
                cancellationToken);

            var userTask = _userService.GetByUserIdAsync(
                calculateRequest.UserId,
                cancellationToken);

            await Task.WhenAll(coursesTask, userTask);

            var courses = await coursesTask;
            var user = await userTask;

            if (user is null)
            {
                throw new PricingUserNotFoundException();
            }

            if (!string.Equals(user.Id, calculateRequest.UserId, StringComparison.Ordinal))
            {
                throw new InvalidPricingDataException("The user repository returned a mismatched user profile.");
            }

            if (courses is null)
            {
                throw new InvalidPricingDataException("The course repository returned no course collection.");
            }

            var requestedCourseIdSet = requestedCourseIds.ToHashSet(StringComparer.Ordinal);
            var availableCourseIds = new HashSet<string>(StringComparer.Ordinal);

            foreach (var course in courses)
            {
                var courseId = ValidateCourse(course, requestedCourseIdSet);
                if (!availableCourseIds.Add(courseId))
                {
                    throw new InvalidPricingDataException("The course repository returned duplicate course IDs.");
                }
            }

            var unknownCourseIds = requestedCourseIds
                .Where(courseId => !availableCourseIds.Contains(courseId))
                .ToArray();

            if (unknownCourseIds.Length > 0)
            {
                throw new UnknownCourseIdsException(unknownCourseIds);
            }

            var lines = new List<CartPriceLine>(courses.Count);

            foreach (var course in courses)
            {
                var (discoutPercent, ruleCode) = SelectDiscount(
                    user.IsGraduate,
                    course.StartDateUtc.ToUniversalTime(),
                    calculatedAtUtc);

                var discountAmount = decimal.Round(
                    course.Price * discoutPercent / 100m,
                    CurrencyDecimalPlaces,
                    MidpointRounding.AwayFromZero);

                var finalPrice = course.Price - discountAmount;

                lines.Add(new CartPriceLine(
                    CourseId: course.Id,
                    CourseTitle: course.Title,
                    OriginalPrice: course.Price,
                    DiscountPercent: discoutPercent,
                    DiscountAmount: discountAmount,
                    FinalPrice: finalPrice,
                    AppliedReluCode: ruleCode));
            }

            return new CartPriceCalculateResponse(
                Items: lines,
                TotalBeforeDiscount: lines.Sum(line => line.OriginalPrice),
                TotalDiscount: lines.Sum(line => line.DiscountAmount),
                TotalAfterDiscount: lines.Sum(line => line.FinalPrice)
                );
        }

        private static string[] ValidateRequest(CartPriceCalculateRequest calculateRequest)
        {
            if (string.IsNullOrWhiteSpace(calculateRequest.UserId))
            {
                throw new InvalidCartPriceCalculateRequestException(
                    nameof(calculateRequest.UserId),
                    "A user ID is required.");
            }

            if (calculateRequest.CourseIds is null)
            {
                throw new InvalidCartPriceCalculateRequestException(
                    nameof(calculateRequest.CourseIds),
                    "Course IDs are required.");
            }

            var courseIds = calculateRequest.CourseIds.ToArray();

            if (courseIds.Length == 0)
            {
                throw new InvalidCartPriceCalculateRequestException(
                    nameof(calculateRequest.CourseIds),
                    "At least one course ID is required.");
            }

            if (courseIds.Length > CartPricingLimits.MaximumCourseCount)
            {
                throw new InvalidCartPriceCalculateRequestException(
                    nameof(calculateRequest.CourseIds),
                    $"No more than {CartPricingLimits.MaximumCourseCount} course IDs are allowed.");
            }

            var seenCourseIds = new HashSet<string>(StringComparer.Ordinal);

            for (var index = 0; index < courseIds.Length; index++)
            {
                var courseId = courseIds[index];
                if (string.IsNullOrWhiteSpace(courseId))
                {
                    throw new InvalidCartPriceCalculateRequestException(
                        $"CourseIds[{index}]",
                        "Course IDs must not be null, empty, or whitespace.");
                }

                if (courseId.Length > CartPricingLimits.MaximumCourseIdLength)
                {
                    throw new InvalidCartPriceCalculateRequestException(
                        $"CourseIds[{index}]",
                        $"Course IDs must not exceed {CartPricingLimits.MaximumCourseIdLength} characters.");
                }

                if (!seenCourseIds.Add(courseId))
                {
                    throw new InvalidCartPriceCalculateRequestException(
                        nameof(calculateRequest.CourseIds),
                        "Course IDs must not contain duplicate values.");
                }
            }

            return courseIds;
        }

        private static string ValidateCourse(
            Course? course,
            HashSet<string> requestedCourseIds)
        {
            if (course is null ||
                string.IsNullOrWhiteSpace(course.Id) ||
                string.IsNullOrWhiteSpace(course.Title) ||
                course.StartDateUtc == default ||
                course.Price < 0m ||
                course.Price > CartPricingLimits.MaximumCoursePrice ||
                decimal.Round(course.Price, CurrencyDecimalPlaces, MidpointRounding.AwayFromZero) != course.Price)
            {
                throw new InvalidPricingDataException("The course repository returned invalid pricing data.");
            }

            if (!requestedCourseIds.Contains(course.Id))
            {
                throw new InvalidPricingDataException("The course repository returned an unrequested course.");
            }

            return course.Id;
        }

        private static (decimal Percent, string? RuleCode) SelectDiscount(
            bool isGraduate,
            DateTimeOffset startDateUtc,
            DateTimeOffset nowUtc)
        {
            var bestPercent = 0m;
            string? bestRuleCode = null;

            if (isGraduate)
            {
                bestPercent = GraduateDiscountPercent;
                bestRuleCode = "graduate";
            }

            var timeUntilStart = startDateUtc - nowUtc;
            var isHotGroup =
                timeUntilStart > TimeSpan.Zero &&
                timeUntilStart < HotGroupWindow;

            if (isHotGroup && HotGroupDiscountPercent > bestPercent)
            {
                bestPercent = HotGroupDiscountPercent;
                bestRuleCode = "hot_group";
            }

            return (bestPercent, bestRuleCode);
        }
    }
}
