using EduCenter.Business.Abstractions.Repositories;
using EduCenter.Business.Models;

namespace EduCenter.Business.Services
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;

        public CourseService(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        public async Task<IReadOnlyList<Course>> GetByCourseIdsAsync(
            IReadOnlyCollection<string> ids,
            CancellationToken cancellationToken = default)
        {
            var courseList = _courseRepository.GetByCourseIdsAsync(ids);
            
            return courseList.Result;
        }
    }
}
