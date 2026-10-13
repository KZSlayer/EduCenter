using EduCenter.Api.DTOs;
using EduCenter.Business.Models;
using EduCenter.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduCenter.Api.Controllers
{
    [ApiController]
    [Route("api/leads")]
    [AllowAnonymous]
    public sealed class LeadsController : ControllerBase
    {
        private readonly ILeadService _leadService;

        public LeadsController(ILeadService leadService)
        {
            _leadService = leadService;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ValidationProblemDetails),
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ProblemDetails),
            StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create(
            [FromBody] LeadRequestDTO request,
            CancellationToken cancellationToken)
        {
            var lead = new Lead
            {
                Id = Guid.NewGuid(),
                ContactName = request.ContactName,
                Phone = request.Phone,
                Email = request.Email,
                ClientType = request.ClientType!.Value
            };

            try
            {
                await _leadService.SendPrimaryAsync(lead, cancellationToken);
            }
            catch (PrimaryLeadNotificationException)
            {
                await _leadService.SendFallbackAsync(lead, cancellationToken);
            }

            return Ok();
        }
    }
}
