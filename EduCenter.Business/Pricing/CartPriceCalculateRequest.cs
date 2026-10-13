namespace EduCenter.Business.Pricing
{
    public record class CartPriceCalculateRequest(
        IReadOnlyCollection<string> CourseIds,
        string UserId);
}
