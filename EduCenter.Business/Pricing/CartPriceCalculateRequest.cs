namespace EduCenter.Business.Pricing
{
    public record class CartPriceCalculateRequest(
        IReadOnlyCollection<Guid> CourseIds,
        string? userId);
}
