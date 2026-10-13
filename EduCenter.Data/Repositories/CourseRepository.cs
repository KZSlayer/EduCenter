using EduCenter.Business.Abstractions.Repositories;
using EduCenter.Business.Models;

namespace EduCenter.Data.Repositories
{
    public class CourseRepository : ICourseRepository
    {
        private List<Course> _courses = new();

        public CourseRepository()
        {
            _courses.Add(new Course
            {
                Id = "1",
                Title = "Introduction to C#",
                Description = "Learn the basics of C# programming.",
                Price = 99.99m,
                StartDateUtc = DateTimeOffset.Now.AddDays(4)
            });
            _courses.Add(new Course
            {
                Id = "2",
                Title = "Advanced ASP.NET Core",
                Description = "Deep dive into ASP.NET Core development.",
                Price = 199.99m,
                StartDateUtc = DateTimeOffset.Now.AddDays(10)
            });
        }

        public async Task<IReadOnlyList<Course>> GetByCourseIdsAsync(
            IReadOnlyCollection<string> ids,
            CancellationToken cancellationToken = default)
        {
            return _courses.Where(c => ids.Contains(c.Id)).ToList();
        }
    }
}
