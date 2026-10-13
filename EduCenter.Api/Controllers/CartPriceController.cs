using System.Security.Claims;
using EduCenter.Business.Pricing;
using EduCenter.Business.Services;
using EduCenter.Api.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace EduCenter.Api.Controllers
{
    [Route("api/cart-price")]
    [ApiController]
    public class CartPriceController : ControllerBase
    {
        private ICartPricingService _cartPricingService;

        public CartPriceController(ICartPricingService cartPricingService)
        {
            _cartPricingService = cartPricingService;
        }

        [HttpPost]
        public async Task<ActionResult<CartPriceResponseDTO>> Calculate(
            [FromBody] CartPriceRequestDTO request,
            CancellationToken cancellationToken)
        {
            var calculateRequest = new CartPriceCalculateRequest(
                CourseIds: request.CourseIds,
                UserId: "2");

            var result = await _cartPricingService.CalculateAsync(
                calculateRequest,
                cancellationToken);

            var response = new CartPriceResponseDTO(
                TotalBeforeDiscount: result.TotalBeforeDiscount,
                TotalDiscount: result.TotalDiscount,
                TotalAfterDiscount: result.TotalAfterDiscount);

            return Ok(response);
        }
    }
}
