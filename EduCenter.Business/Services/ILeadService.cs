using EduCenter.Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduCenter.Business.Services
{
    public interface ILeadService
    {
        Task SendPrimaryAsync(
            Lead lead,
            CancellationToken cancellationToken = default);

        Task SendFallbackAsync(
            Lead lead,
            CancellationToken cancellationToken = default);
    }
}
