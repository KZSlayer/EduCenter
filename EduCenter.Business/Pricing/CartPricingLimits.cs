namespace EduCenter.Business.Pricing
{
    public static class CartPricingLimits
    {
        public const int MaximumCourseCount = 100;
        public const int MaximumCourseIdLength = 64;
        public static readonly decimal MaximumCoursePrice = decimal.MaxValue / MaximumCourseCount;
    }
}
