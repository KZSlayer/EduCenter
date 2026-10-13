using EduCenter.Business.Models;

namespace EduCenter.Business.Abstractions.Repositories
{
    public interface ICourseRepository
    {
        Task<IReadOnlyList<Course>> GetByCourseIdsAsync(
            IReadOnlyCollection<Guid> ids,
            CancellationToken cancellationToken = default);
    }
}
