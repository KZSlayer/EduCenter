using EduCenter.Api.Authentication;
using EduCenter.Api.DTOs;
using EduCenter.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduCenter.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public sealed class AuthController : ControllerBase
    {
        private readonly IUserAuthenticationService _userAuthenticationService;
        private readonly IAccessTokenService _accessTokenService;

        public AuthController(
            IUserAuthenticationService userAuthenticationService,
            IAccessTokenService accessTokenService)
        {
            _userAuthenticationService = userAuthenticationService;
            _accessTokenService = accessTokenService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LoginResponseDTO>> Login(
            [FromBody] LoginRequestDTO request,
            CancellationToken cancellationToken)
        {
            var userId = await _userAuthenticationService.AuthenticateAsync(
                request.Username,
                request.Password,
                cancellationToken);

            if (userId is null)
            {
                return Unauthorized();
            }

            var token = _accessTokenService.Create(userId);
            
            return Ok(new LoginResponseDTO(
                AccessToken: token.Token,
                TokenType: "Bearer",
                ExpiresAtUtc: token.ExpiresAtUtc));
        }
    }
}
