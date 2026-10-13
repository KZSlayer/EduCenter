using EduCenter.Business.Pricing;

namespace EduCenter.Api.DTOs
{
    internal static class CartPriceRequestLimits
    {
        internal const int MaximumCourseCount = CartPricingLimits.MaximumCourseCount;
        internal const int MaximumCourseIdLength = CartPricingLimits.MaximumCourseIdLength;
        internal const long MaximumRequestBodySizeBytes = 32 * 1024;
    }
}
