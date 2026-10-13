using EduCenter.Business.Pricing;

namespace EduCenter.Business.Services
{
    public interface ICartPricingService
    {
        Task<CartPriceCalculateResponse> CalculateAsync(
            CartPriceCalculateRequest calculateRequest,
            CancellationToken cancellationToken = default);
    }
}
