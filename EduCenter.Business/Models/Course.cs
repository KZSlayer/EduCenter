namespace EduCenter.Business.Models
{
    public class Course
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public DateTimeOffset StartDateUtc { get; set; }
    }
}
