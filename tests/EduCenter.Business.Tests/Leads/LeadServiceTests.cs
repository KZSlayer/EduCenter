using EduCenter.Business.Models;
using EduCenter.Business.Services;

namespace EduCenter.Business.Tests.Leads
{
    public sealed class LeadServiceTests
    {
        public static TheoryData<ClientType, string> FallbackRecipients => new()
        {
            { ClientType.Individual, "specialist.ufl@yandex.ru" },
            { ClientType.LegalEntity, "specialist.corp@yandex.ru" }
        };

        [Theory]
        [MemberData(nameof(FallbackRecipients))]
        public async Task SendsFallbackToRecipientForClientType(
            ClientType clientType,
            string expectedRecipient)
        {
            var sender = new RecordingLeadNotificationSender();
            var service = new LeadService(sender);
            var lead = new Lead
            {
                Id = Guid.NewGuid(),
                ClientType = clientType
            };

            await service.SendFallbackAsync(lead);

            Assert.Same(lead, sender.Lead);
            Assert.Equal(expectedRecipient, sender.Recipient);
        }

        private sealed class RecordingLeadNotificationSender : ILeadNotificationSender
        {
            public Lead? Lead { get; private set; }
            public string? Recipient { get; private set; }

            public Task SendAsync(
                Lead lead,
                string recipient,
                CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Lead = lead;
                Recipient = recipient;
                return Task.CompletedTask;
            }
        }
    }
}
