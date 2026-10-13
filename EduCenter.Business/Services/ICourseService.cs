using EduCenter.Business.Models;

namespace EduCenter.Business.Services
{
    public interface ICourseService
    {
        Task<IReadOnlyList<Course>> GetByCourseIdsAsync(
            IReadOnlyCollection<Guid> ids,
            CancellationToken cancellationToken = default);
    }
}
