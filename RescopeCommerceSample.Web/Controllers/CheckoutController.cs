using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Rescope.Commerce.Core.Entities;
using Rescope.Commerce.Core.Services;
using Rescope.Commerce.Web.Models;
using RescopeCommerceSample.Web.Helpers;
using RescopeCommerceSample.Web.Models;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.PublishedModels;
using Umbraco.Cms.Web.Website.Models;

namespace RescopeCommerceSample.Web.Controllers
{
    /// <summary>
    /// Take control over the rendering of the Checkout content type
    /// </summary>
    public class CheckoutController : RenderController
    {
        private readonly IOrderService _orderService;
        private readonly IBasketService _basketService;
        private readonly IMemberService _memberService;
        private readonly IPublishedValueFallback _publishedValueFallback;
        private readonly MemberModelBuilderFactory _memberModelBuilderFactory;
        private readonly IShippingCalculatorService _shippingCalculatorService;
        private readonly IPaymentMethodService _paymentMethodService;
        private readonly IProductStore _productStore;

        public CheckoutController(
            ILogger<CheckoutController> logger,
            ICompositeViewEngine compositeViewEngine,
            IUmbracoContextAccessor umbracoContextAccessor,
            IBasketService basketService,
            IMemberService memberService,
            IPublishedValueFallback publishedValueFallback,
            MemberModelBuilderFactory memberModelBuilderFactory,
            IShippingCalculatorService shippingCalculatorService,
            IPaymentMethodService paymentMethodService,
            IOrderService orderService,
            IProductStore productStore
        )
            : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
            _basketService = basketService;
            _memberService = memberService;
            _publishedValueFallback = publishedValueFallback;
            _memberModelBuilderFactory = memberModelBuilderFactory;
            _shippingCalculatorService = shippingCalculatorService;
            _paymentMethodService = paymentMethodService;
            _orderService = orderService;
            _productStore = productStore;
        }

        [NonAction]
        public sealed override IActionResult Index() => throw new NotImplementedException();

        public async Task<IActionResult> Index([FromQuery] string? stage, string? orderId)
        {
            if (CurrentPage == null)
                return BadRequest();

            // if there isn't a valid basket for this user, and theyre not on the payment screen,
            // redirect them to the basket page
            var basket = await _basketService.GetCurrentBasket(
                CurrentPage!.AncestorOrSelf<Home>()!.RescopeStore!.Id
            );
            if (basket == null || basket.BasketLineItems.Count() == 0)
            {
                if (stage != "payment" && string.IsNullOrEmpty(orderId))
                {
                    return Redirect(CurrentPage.Parent!.Url());
                }
            }

            // TODO: Get these two variables from a Umbraco settings node?
            var showLoginScreen = true;

            // Disable guest checkout IF the user has a subscription item in their basket
            var products = await _basketService.GetProducts(basket!);
            var allowGuestCheckout = !products.Any(x => x.IsSubscriptionItem);

            var isLoggedIn = User?.Identity?.IsAuthenticated ?? false;

            // User has initiated payment, and CheckoutSurfaceController has added the payment form to ViewData:
            if (ViewData.HasPaymentHTML())
            {
                var order = await _orderService.Get(ViewData.GetOrderId()!.Value);

                return View(
                    "Checkout/PaymentHTML",
                    new CheckoutOrderViewModel(order, null, CurrentPage, _publishedValueFallback)
                    {
                        IsLoggedIn = isLoggedIn,
                    }
                );
            }
            // User is on the payment screen:
            else if (stage == "payment")
            {
                // User has orderId in their query string, they are retrying payment:
                if (!string.IsNullOrEmpty(orderId))
                {
                    var order = await _orderService.GetByNumber(orderId);
                    if (order.PaymentStatus == PaymentStatus.PAID)
                    {
                        return Redirect(
                            CurrentPage.Parent!.FirstChild<Confirmation>()!.Url()
                                + "?orderId="
                                + HttpUtility.UrlEncode(order.OrderNumber)
                                + "&idHash="
                                + HttpUtility.UrlEncode(order.GetIdHash())
                        );
                    }

                    ViewData.SetOrderId(order.Id);
                    return View(
                        "Checkout/PaymentRetry",
                        new CheckoutOrderViewModel(
                            order,
                            new CheckoutPaymentModel()
                            {
                                PaymentMethods = await _paymentMethodService.List(),
                            },
                            CurrentPage,
                            _publishedValueFallback
                        )
                        {
                            IsLoggedIn = isLoggedIn,
                        }
                    );
                }
                // User is initiating payment for the first time:
                else
                    return View(
                        "Checkout/Payment",
                        new CheckoutViewModel<CheckoutPaymentModel>(
                            basket!,
                            new CheckoutPaymentModel()
                            {
                                PaymentMethods = await _paymentMethodService.List(),
                            },
                            CurrentPage,
                            _publishedValueFallback
                        )
                        {
                            IsLoggedIn = isLoggedIn,
                        }
                    );
            }
            // User is on the shipping screen:
            else if (stage == "shipping")
            {
                return View(
                    "Checkout/Shipping",
                    new CheckoutViewModel<CheckoutShippingModel>(
                        basket!,
                        new CheckoutShippingModel()
                        {
                            ShippingCalculationInfos =
                                await _shippingCalculatorService.GetCalculations(basket),
                        },
                        CurrentPage,
                        _publishedValueFallback
                    )
                    {
                        IsLoggedIn = isLoggedIn,
                    }
                );
            }
            // User is on the details page
            // We also show this as last resort if the user is logged in OR we do not require logins
            else if (!showLoginScreen || stage == "details" || isLoggedIn)
            {
                var checkout = new CheckoutDetailsModel(basket!);
                if (isLoggedIn)
                {
                    checkout.Email = User?.Identity?.GetEmail() ?? "";

                    if (
                        checkout.DeliveryAddress != null
                        && int.TryParse(User.Identity!.GetUserId(), out var id)
                    )
                    {
                        var user = _memberService.GetById(id);
                        if (user != null)
                            checkout.DeliveryAddress.Apply(user);
                        if (checkout.Phone.IsNullOrWhiteSpace())
                            checkout.Phone = checkout.DeliveryAddress.Telephone;
                    }
                }

                return View(
                    "Checkout/Details",
                    new CheckoutViewModel<CheckoutDetailsModel>(
                        basket!,
                        checkout,
                        CurrentPage,
                        _publishedValueFallback
                    )
                    {
                        IsLoggedIn = isLoggedIn,
                    }
                );
            }
            // User is on the sign-up page.
            else if (stage == "register")
            {
                return View(
                    "Checkout/Register",
                    new CheckoutViewModel<CheckoutRegisterModel>(
                        basket!,
                        new CheckoutRegisterModel() { RegisterModel = new CustomRegisterModel() },
                        CurrentPage,
                        _publishedValueFallback
                    )
                );
            }
            // User is prompted to login or continue as guest
            else
            {
                return View(
                    "Checkout/Login",
                    new CheckoutViewModel<CheckoutLoginModel>(
                        basket,
                        new CheckoutLoginModel() { AllowGuestCheckout = allowGuestCheckout },
                        CurrentPage,
                        _publishedValueFallback
                    )
                );
            }
        }
    }
}
