using Microsoft.AspNetCore.Mvc;
using Rescope.Commerce.Core.Exceptions;
using Rescope.Commerce.Core.Models;
using Rescope.Commerce.Core.Services;
using Rescope.Commerce.Web.Models;
using RescopeCommerceSample.Web.Helpers;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.PublishedModels;
using Umbraco.Cms.Web.Website.Controllers;
using Order = Rescope.Commerce.Core.Entities.Order;

namespace RescopeCommerceSample.Web.Controllers
{
    /// <summary>
    /// Used by the site's front-end on the checkout screens to update the basket/order.
    /// </summary>
    public class CheckoutSurfaceController : SurfaceController
    {
        private readonly IBasketService _basketService;
        private readonly IOrderService _orderService; 
        private readonly IShippingCalculatorService _shippingCalculatorService;
        private readonly IMemberManager _memberManager;
        private readonly ILogger<CheckoutSurfaceController> _logger;

        public CheckoutSurfaceController(
            IMemberManager memberManager,
            IUmbracoContextAccessor umbracoContextAccessor,
            IOrderService orderService, 
            IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger profilingLogger,
            IPublishedUrlProvider publishedUrlProvider,
            IBasketService basketService,
            IShippingCalculatorService shippingCalculatorService,
            ILogger<CheckoutSurfaceController> logger
        )
            : base(
                umbracoContextAccessor,
                databaseFactory,
                services,
                appCaches,
                profilingLogger,
                publishedUrlProvider
            )
        {
            _orderService = orderService;
            _basketService = basketService;
            _shippingCalculatorService = shippingCalculatorService;
            _memberManager = memberManager;
            _logger = logger;
        }

        /// <summary>
        /// Used on the checkout screen to fetch updates about the order status
        /// </summary> 
        [HttpGet]
        public async Task<IActionResult> GetStatus(int pageId, string order)
        {
            var basket = await _orderService.GetByNumber(order);
            if (basket == null)
                return NotFound();

            return Ok(basket.PaymentStatus.ToString());
        }

        /// <summary>
        /// Used in checkout screens to set an address on the active basket.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateAddress(CheckoutDetailsModel checkout)
        {
            // Get our store instance. In this demo site, we store this in a StorePicker on the home node.
            var store = CurrentPage?.AncestorOrSelf<Home>()?.RescopeStore;
            if (store == null)
                return BadRequest();

            var basket = await _basketService.GetOrCreateCurrentBasket(store.Id);

            basket.DeliveryAddress = checkout.DeliveryAddress;
            basket.BillingAddress = checkout.BillingAddress;
            if (checkout.BillingAddressSameAsDelivery)
                basket.BillingAddress = checkout.DeliveryAddress;
            basket.Phone = checkout.Phone;
            basket.Email = checkout.Email;
            basket.UmbracoMemberKey = null;

            // If the user is a logged in member, connect this basket with the umbraco member
            if (User.Identity?.IsAuthenticated == true)
            {
                var member = await _memberManager.GetCurrentMemberAsync();
                basket.UmbracoMemberKey = member?.Key; 
            }

            // Check what shipping methods are available. If there is only one method, we'll
            // skip the shipping page and take the user straight to the payment screen.
            var calculations = await _shippingCalculatorService.GetCalculations(basket);
            var skipShipping = false;
            if (
                calculations.Count() == 1
                && (
                    basket.ShippingMethodId == null
                    || !calculations.Any(y => y.Method.Id == basket.ShippingMethodId)
                )
            )
            {
                basket.ShippingMethodId = calculations.First().Method.Id;
                skipShipping =true;
            }

            // save the changes we made to the basket
            await _basketService.Update(basket);

            if (basket.Errors.Any())
            {
                TempData["showDetailsErrors"] = true;
                return RedirectToCurrentUmbracoPage(new QueryString("?stage=details"));
            }

            if (skipShipping)
            {
                return RedirectToCurrentUmbracoPage(new QueryString("?stage=payment"));
            }
            else
            {
                return RedirectToCurrentUmbracoPage(new QueryString("?stage=shipping"));
            }
        }

        /// <summary>
        /// Used in checkout screens to choose a shipping method
        /// </summary> 
        [HttpPost]
        public async Task<IActionResult> UpdateShipping(UpdateShippingMethodRequest checkout)
        {
            // Get our store instance. In this demo site, we store this in a StorePicker on the home node.
            var store = CurrentPage?.AncestorOrSelf<Home>()?.RescopeStore;
            if (store == null)
                return BadRequest();

            var basket = await _basketService.GetOrCreateCurrentBasket(store.Id); 
            basket.ShippingMethodId = checkout.Method;
            basket.UmbracoMemberKey = null;

            // If the user is a logged in member, connect this basket with the umbraco member
            if (User.Identity?.IsAuthenticated == true)
            {
                var member = await _memberManager.GetCurrentMemberAsync();
                basket.UmbracoMemberKey = member?.Key;
            }

            try
            {
                await _basketService.Update(basket);

                // Handle any price changes which may occur since we first added items to the basket.
                await _basketService.RefreshPrices(basket);

                if (basket.Errors.Any())
                    return RedirectToCurrentUmbracoPage();

                return RedirectToCurrentUmbracoPage(new QueryString("?stage=payment"));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to switch shipping method");
                return RedirectToCurrentUmbracoPage(new QueryString("?stage=shipping"));
            }
        }

        /// <summary>
        /// Used in checkout screens to initiate payment
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> StartPayment(UpdatePaymentMethodRequest checkout)
        {
            // Get our store instance. In this demo site, we store this in a StorePicker on the home node.
            var store = CurrentPage?.AncestorOrSelf<Home>()?.RescopeStore;
            if (store == null)
                return BadRequest();

            var basket = await _basketService.GetOrCreateCurrentBasket(store.Id);
            basket.PaymentMethodId = checkout.Method;
            basket.UmbracoMemberKey = null;

            // If the user is a logged in member, connect this basket with the umbraco member
            if (User.Identity?.IsAuthenticated == true)
            {
                var member = await _memberManager.GetCurrentMemberAsync();
                basket.UmbracoMemberKey = member?.Key;
            }

            try
            {
                await _basketService.Update(basket);

                // Handle any price changes which may occur since we first added items to the basket.
                await _basketService.RefreshPrices(basket);

                if (basket.Errors.Any())
                    return RedirectToCurrentUmbracoPage();

                try
                {
                    var (res, order) = await _basketService.InitiatePayment(
                        basket,
                        CurrentPage!.AncestorOrSelf<Basket>()!.FirstChild<Confirmation>()!
                    );
                    
                    return HandlePaymentResult(order, res);
                }
                catch (EmptyOrderException e)
                {
                    // items became unavailable during checkout
                    return RedirectToCurrentUmbracoPage(new QueryString("?empty=true"));
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to switch payment method");
                return RedirectToCurrentUmbracoPage(new QueryString("?stage=payment"));
            }
        }

        /// <summary>
        /// Used in checkout screens to retry payment
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RetryPayment(RetryPaymentMethodRequest checkout)
        {
            // Get our store instance. In this demo site, we store this in a StorePicker on the home node.
            var store = CurrentPage?.AncestorOrSelf<Home>()?.RescopeStore;
            if (store == null)
                return BadRequest();

            var order = await _orderService.Get(checkout.OrderId);
            order.PaymentMethodId = checkout.Method;

            await _orderService.Update(order); 

            var res = await _orderService.RetryPayment(
                order,
                CurrentPage!.AncestorOrSelf<Basket>()!.FirstChild<Confirmation>()!
            );

            return HandlePaymentResult(order, res);
        }

        /// <summary>
        /// Used by this controller to convert Rescope Commerce's InitiatePaymentResult into a IActionResult
        /// </summary> 
        protected IActionResult HandlePaymentResult(Order order, InitiatePaymentResult initiatePaymentResult)
        {
            switch (initiatePaymentResult.Type)
            {
                // Handle payment providers that require us to redirect the user to a payment gateway
                case ResultType.REDIRECT:
                    return Redirect(initiatePaymentResult.Url);

                // Handle providers that allow us to embed the payment form HTML directly within our own website
                case ResultType.HTML:
                    // We'll just store the form in ViewData to be used by the cshtml template.
                    ViewData
                        .SetOrderId(order.Id)
                        .SetPaymentHTML(initiatePaymentResult.Html);

                    return CurrentUmbracoPage();
            }

            _logger.LogError("Unhandled payment result type {type}", initiatePaymentResult.Type);
            return BadRequest();
        }
    }
}
