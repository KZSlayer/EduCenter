namespace EduCenter.Business.Pricing
{
    public record class CartPriceLine(
        Guid CourseId,
        string CourseTitle,
        decimal OriginalPrice,
        decimal DiscountPercent,
        decimal DiscountAmount,
        decimal FinalPrice,
        string? AppliedReluCode);
}
