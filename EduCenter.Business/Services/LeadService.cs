using EduCenter.Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduCenter.Business.Services
{
    public sealed class LeadService : ILeadService
    {
        public Task SendPrimaryAsync(
            Lead lead,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(lead);
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromException(
                new PrimaryLeadNotificationException(
                    "Primary lead notification failed."));
        }

        public Task SendFallbackAsync(
            Lead lead,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(lead);
            cancellationToken.ThrowIfCancellationRequested();

            var recipient = lead.ClientType switch
            {
                ClientType.LegalEntity => "specialist.corp@yandex.ru",
                ClientType.Individual => "specialist.ufl@yandex.ru",
                _ => throw new ArgumentOutOfRangeException(
                    nameof(lead.ClientType),
                    lead.ClientType,
                    "Unsupported client type.")
            };

            return SimulateFallbackAsync(
                lead,
                recipient,
                cancellationToken);
        }

        private static Task SimulateFallbackAsync(
            Lead lead,
            string recipient,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
