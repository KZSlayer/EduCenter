namespace EduCenter.Business.Pricing
{
    public sealed class InvalidPricingDataException : Exception
    {
        public InvalidPricingDataException(string message)
            : base(message)
        {
        }
    }
}
