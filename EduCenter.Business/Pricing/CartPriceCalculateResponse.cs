namespace EduCenter.Business.Pricing
{
    public record class CartPriceCalculateResponse(
        IReadOnlyList<CartPriceLine> Items,
        decimal TotalBeforeDiscount,
        decimal TotalDiscount,
        decimal TotalAfterDiscount);
}
