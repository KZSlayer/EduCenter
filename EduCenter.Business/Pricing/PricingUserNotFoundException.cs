namespace EduCenter.Business.Pricing
{
    public sealed class PricingUserNotFoundException : Exception
    {
        public PricingUserNotFoundException()
            : base("The authenticated user does not have a pricing profile.")
        {
        }
    }
}
