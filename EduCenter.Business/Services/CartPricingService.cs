using EduCenter.Business.Pricing;

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
            var calculatedAtUtc = _timeProvider.GetUtcNow();

            var courses = _courseService.GetByCourseIdsAsync(calculateRequest.CourseIds);

            var user = _userService.GetByUserIdAsync(calculateRequest.UserId);

            var lines = new List<CartPriceLine>(courses.Result.Count);

            foreach (var course in courses.Result)
            {
                var (discoutPercent, ruleCode) = SelectDiscount(
                    user.Result.IsGraduate,
                    course.StartDateUtc,
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
