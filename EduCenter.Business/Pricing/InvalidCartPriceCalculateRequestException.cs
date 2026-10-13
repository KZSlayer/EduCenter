namespace EduCenter.Business.Pricing
{
    public sealed class InvalidCartPriceCalculateRequestException : Exception
    {
        public string Field { get; }

        public InvalidCartPriceCalculateRequestException(string field, string message)
            : base(message)
        {
            Field = field;
        }
    }
}
