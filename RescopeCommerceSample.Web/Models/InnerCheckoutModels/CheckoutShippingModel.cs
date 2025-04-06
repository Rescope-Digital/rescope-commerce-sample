using Rescope.Commerce.Core.Models;

namespace Rescope.Commerce.Web.Models
{
    public class CheckoutShippingModel
    {
        public CheckoutShippingModel() { }
        
        public IEnumerable<ShippingCalculationInfo> ShippingCalculationInfos { get; set; }
    }
}
