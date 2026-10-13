using System.ComponentModel.DataAnnotations;
using EduCenter.Api.Controllers;
using EduCenter.Api.DTOs;
using EduCenter.Api.Errors;
using EduCenter.Business.Models;
using EduCenter.Business.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace EduCenter.Business.Tests.Leads
{
    public sealed class LeadsControllerTests
    {
        [Fact]
        public async Task ReturnsOkWhenPrimaryNotificationFailsAndFallbackSucceeds()
        {
            var leadService = new FailingPrimaryLeadService();
            var controller = new LeadsController(leadService);
            var request = new LeadRequestDTO
            {
                ContactName = "Alex Student",
                Phone = "+1-555-0100",
                Email = "alex@example.com",
                ClientType = ClientType.Individual
            };

            var result = await controller.Create(request, CancellationToken.None);

            Assert.IsType<OkResult>(result);
            Assert.Equal(1, leadService.PrimaryCallCount);
            Assert.Equal(1, leadService.FallbackCallCount);
            Assert.Equal(ClientType.Individual, leadService.FallbackLead?.ClientType);
        }

        [Fact]
        public async Task HandlesFallbackFailureAsInternalServerError()
        {
            var fallbackFailure = new InvalidOperationException("Simulated fallback failure.");
            var leadService = new FailingPrimaryLeadService(fallbackFailure);
            var controller = new LeadsController(leadService);
            var request = new LeadRequestDTO
            {
                ContactName = "Alex Student",
                Phone = "+1-555-0100",
                Email = "alex@example.com",
                ClientType = ClientType.Individual
            };

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => controller.Create(request, CancellationToken.None));

            Assert.Same(fallbackFailure, exception);
            Assert.Equal(1, leadService.PrimaryCallCount);
            Assert.Equal(1, leadService.FallbackCallCount);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = "/api/leads";
            httpContext.Response.Body = new MemoryStream();
            var exceptionHandler = new ApiExceptionHandler(
                NullLogger<ApiExceptionHandler>.Instance);

            var handled = await exceptionHandler.TryHandleAsync(
                httpContext,
                exception,
                CancellationToken.None);

            Assert.True(handled);
            Assert.Equal(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);
        }

        [Theory]
        [InlineData("", "+1-555-0100", "alex@example.com", ClientType.Individual, nameof(LeadRequestDTO.ContactName))]
        [InlineData("Alex Student", " ", "alex@example.com", ClientType.Individual, nameof(LeadRequestDTO.Phone))]
        [InlineData("Alex Student", "+1-555-0100", "invalid-email", ClientType.Individual, nameof(LeadRequestDTO.Email))]
        [InlineData("Alex Student", "+1-555-0100", "alex@example.com", (ClientType)999, nameof(LeadRequestDTO.ClientType))]
        public void RejectsInvalidLeadRequestData(
            string contactName,
            string phone,
            string email,
            ClientType clientType,
            string expectedField)
        {
            var request = new LeadRequestDTO
            {
                ContactName = contactName,
                Phone = phone,
                Email = email,
                ClientType = clientType
            };
            var validationResults = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(
                request,
                new ValidationContext(request),
                validationResults,
                validateAllProperties: true);

            Assert.False(isValid);
            Assert.Contains(
                validationResults,
                result => result.MemberNames.Contains(expectedField));
        }

        private sealed class FailingPrimaryLeadService : ILeadService
        {
            private readonly Exception? _fallbackFailure;

            public FailingPrimaryLeadService(Exception? fallbackFailure = null)
            {
                _fallbackFailure = fallbackFailure;
            }

            public int PrimaryCallCount { get; private set; }
            public int FallbackCallCount { get; private set; }
            public Lead? FallbackLead { get; private set; }

            public Task SendPrimaryAsync(
                Lead lead,
                CancellationToken cancellationToken = default)
            {
                PrimaryCallCount++;
                return Task.FromException(
                    new PrimaryLeadNotificationException("Simulated primary failure."));
            }

            public Task SendFallbackAsync(
                Lead lead,
                CancellationToken cancellationToken = default)
            {
                FallbackCallCount++;
                FallbackLead = lead;
                return _fallbackFailure is null
                    ? Task.CompletedTask
                    : Task.FromException(_fallbackFailure);
            }
        }
    }
}
