namespace EduCenter.Business.Pricing
{
    public sealed class UnknownCourseIdsException : Exception
    {
        public IReadOnlyList<string> CourseIds { get; }

        public UnknownCourseIdsException(IEnumerable<string> courseIds)
            : base("One or more course IDs do not exist.")
        {
            CourseIds = courseIds.ToArray();
        }
    }
}
