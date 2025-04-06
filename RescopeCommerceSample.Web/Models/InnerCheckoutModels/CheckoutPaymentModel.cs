using Rescope.Commerce.Core.Entities;

namespace Rescope.Commerce.Web.Models
{
    public class CheckoutPaymentModel
    {
        public CheckoutPaymentModel() { }
        
        public IEnumerable<PaymentMethod> PaymentMethods { get; set; }
    }
}
