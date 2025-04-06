using System.ComponentModel.DataAnnotations;
using Rescope.Commerce.Core.Entities;
using Rescope.Commerce.Core.Models;

namespace Rescope.Commerce.Web.Models
{
    public class UpdateShippingMethodRequest
    {
        public Guid Method { get; set; }
    }

    public class CheckoutDetailsModel
    {
        public CheckoutDetailsModel() { }

        public CheckoutDetailsModel(Basket basket)
        {
            Phone = basket.Phone;
            BillingAddress = basket.BillingAddress;
            DeliveryAddress = basket.DeliveryAddress;
            Email = basket.Email;
            BillingAddressSameAsDelivery =
                basket.BillingAddress == null
                || basket.BillingAddress.AddressLine1.IsNullOrWhiteSpace();
        }

        public CheckoutDetailsModel(Order basket)
        {
            Phone = basket.Phone;
            BillingAddress = basket.BillingAddress;
            DeliveryAddress = basket.DeliveryAddress;
            Email = basket.Email;
            BillingAddressSameAsDelivery =
                basket.BillingAddress == null
                || basket.BillingAddress.AddressLine1.IsNullOrWhiteSpace();
        }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string Phone { get; set; }

        public PostalAddress? DeliveryAddress { get; set; }
        public PostalAddress? BillingAddress { get; set; }

        public bool BillingAddressSameAsDelivery { get; set; }
    }
}
