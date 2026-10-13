namespace EduCenter.Api.DTOs
{
    public record class CartPriceResponseDTO(
        decimal TotalBeforeDiscount,
        decimal TotalDiscount,
        decimal TotalAfterDiscount);
}
