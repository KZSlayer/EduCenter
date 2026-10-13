using EduCenter.Business.Abstractions.Repositories;
using EduCenter.Business.Models;

namespace EduCenter.Data.Repositories
{
    public class CourseRepository : ICourseRepository
    {
        private List<Course> _courses = new();

        public CourseRepository(TimeProvider timeProvider)
        {
            var nowUtc = timeProvider.GetUtcNow();

            _courses.Add(new Course
            {
                Id = "1",
                Title = "Introduction to C#",
                Description = "Learn the basics of C# programming.",
                Price = 99.99m,
                StartDateUtc = nowUtc.AddDays(4)
            });
            _courses.Add(new Course
            {
                Id = "2",
                Title = "Advanced ASP.NET Core",
                Description = "Deep dive into ASP.NET Core development.",
                Price = 199.99m,
                StartDateUtc = nowUtc.AddDays(10)
            });
        }

        public Task<IReadOnlyList<Course>> GetByCourseIdsAsync(
            IReadOnlyCollection<string> ids,
            CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<IReadOnlyList<Course>>(cancellationToken);
            }

            IReadOnlyList<Course> courses = _courses.Where(c => ids.Contains(c.Id)).ToList();
            return Task.FromResult(courses);
        }
    }
}
