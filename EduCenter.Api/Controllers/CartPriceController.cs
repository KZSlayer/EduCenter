using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EduCenter.Api.DTOs;
using EduCenter.Business.Pricing;
using EduCenter.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduCenter.Api.Controllers
{
    [Route("api/cart-price")]
    [ApiController]
    [Authorize]
    public class CartPriceController : ControllerBase
    {
        private readonly ICartPricingService _cartPricingService;

        public CartPriceController(ICartPricingService cartPricingService)
        {
            _cartPricingService = cartPricingService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(CartPriceResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status413PayloadTooLarge)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [RequestSizeLimit(CartPriceRequestLimits.MaximumRequestBodySizeBytes)]
        public async Task<ActionResult<CartPriceResponseDTO>> Calculate(
            [FromBody] CartPriceRequestDTO request,
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var calculateRequest = new CartPriceCalculateRequest(
                CourseIds: request.CourseIds,
                UserId: userId);

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
