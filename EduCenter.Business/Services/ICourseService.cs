using EduCenter.Business.Models;

namespace EduCenter.Business.Services
{
    public interface ICourseService
    {
        Task<IReadOnlyList<Course>> GetByCourseIdsAsync(
            IReadOnlyCollection<string> ids,
            CancellationToken cancellationToken = default);
    }
}
