using EduCenter.Business.Models;

namespace EduCenter.Business.Services
{
    public interface ILeadNotificationSender
    {
        Task SendAsync(
            Lead lead,
            string recipient,
            CancellationToken cancellationToken = default);
    }
}
