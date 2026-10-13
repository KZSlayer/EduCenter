namespace EduCenter.Api.DTOs
{
    public class CartPriceResponseDTO
    {
        public decimal TotalBeforeDiscount { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalAfterDiscount { get; set; }
    }
}
