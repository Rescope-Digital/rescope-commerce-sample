namespace Rescope.Commerce.Web.Models
{
    public class RetryPaymentMethodRequest
    {
        public Guid Method { get; set; }
        public Guid OrderId { get; set; }
    }
}
