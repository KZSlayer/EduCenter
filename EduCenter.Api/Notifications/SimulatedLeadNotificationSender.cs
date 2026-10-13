using EduCenter.Business.Models;
using EduCenter.Business.Services;
using Microsoft.Extensions.Logging;

namespace EduCenter.Api.Notifications
{
    public sealed class SimulatedLeadNotificationSender : ILeadNotificationSender
    {
        private readonly ILogger<SimulatedLeadNotificationSender> _logger;

        public SimulatedLeadNotificationSender(
            ILogger<SimulatedLeadNotificationSender> logger)
        {
            _logger = logger;
        }

        public Task SendAsync(
            Lead lead,
            string recipient,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(lead);
            ArgumentException.ThrowIfNullOrWhiteSpace(recipient);
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation(
                "Simulated fallback notification for lead {LeadId} to {Recipient}.",
                lead.Id,
                recipient);

            return Task.CompletedTask;
        }
    }
}
