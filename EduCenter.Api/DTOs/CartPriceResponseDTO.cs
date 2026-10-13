namespace EduCenter.Api.DTOs
{
    public sealed record class CartPriceResponseDTO(
        decimal TotalBeforeDiscount,
        decimal TotalDiscount,
        decimal TotalAfterDiscount);
}
