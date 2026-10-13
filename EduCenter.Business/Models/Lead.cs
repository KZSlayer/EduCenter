namespace EduCenter.Business.Models
{
    public class Lead
    {
        public Guid Id { get; set; }
        public string ContactName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Email { get; set; } = null!;
        public ClientType ClientType { get; set; }
    }

    public enum ClientType
    {
        Individual = 1,
        LegalEntity = 2
    }
}
