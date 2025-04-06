using Rescope.Commerce.Core.Entities;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace RescopeCommerceSample.Web.Models
{
    public class CheckoutViewModel<T> : PublishedContentWrapped
    {
        public CheckoutViewModel(Basket basket, T model, IPublishedContent content, IPublishedValueFallback publishedValueFallback) : base(content, publishedValueFallback)
        {
            Basket = basket;
            CheckoutModel = model;
        }

        public Basket Basket { get; set; }
        public T CheckoutModel { get; set; }
        public bool IsLoggedIn { get; set; }
    }
}

