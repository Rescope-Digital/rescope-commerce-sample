using Rescope.Commerce.Core.Entities;
using Rescope.Commerce.Web.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace RescopeCommerceSample.Web.Models
{
    public class CheckoutOrderViewModel : PublishedContentWrapped
    {
        public CheckoutOrderViewModel(Order order, CheckoutPaymentModel? model, IPublishedContent content, IPublishedValueFallback publishedValueFallback) : base(content, publishedValueFallback)
        {
            Order = order;
            CheckoutModel = model;
        }

        public Order Order { get; set; }
        public CheckoutPaymentModel? CheckoutModel { get; set; }
        public bool IsLoggedIn { get; set; }
    }
}

