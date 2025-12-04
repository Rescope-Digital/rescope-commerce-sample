using Microsoft.AspNetCore.Mvc;
using Rescope.Commerce.Core.Services;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.PublishedModels;
using Umbraco.Cms.Web.Website.Controllers;
using uSync.Core;

namespace RescopeCommerceSample.Web.Controllers
{
    /// <summary>
    /// Used by the site's front-end to add things to the basket
    /// </summary>
    public class BasketSurfaceController : SurfaceController
    {
        private readonly IBasketService _basketService;
        private readonly IProductStore _productStore;
        private readonly IStockService _stockService;

        public BasketSurfaceController(
            IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger profilingLogger,
            IPublishedUrlProvider publishedUrlProvider,
            IBasketService basketService,
            IProductStore productStore,
            IStockService stockService
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
            _basketService = basketService;
            _productStore = productStore;
            _stockService = stockService;
        }

        /// <summary>
        /// Adds an item to the basket
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddToBasket(string productSku)
        {
            var store = CurrentPage?.AncestorOrSelf<Home>()?.RescopeStore;
            if (store == null)
                return BadRequest();

            var basket = await _basketService.GetOrCreateCurrentBasket(store.Id);

            var product = await _productStore.GetProductOrVariant(productSku);
            if (product == null)
                return BadRequest();

            var customisationFields = Request.Form
                .Where(x => x.Key.StartsWith("cf_"))
                .ToDictionary(x => Guid.Parse(x.Key.Substring(3)), x => x.Value.FirstOrDefault() ?? "");

            await basket.AddProduct(product, _stockService, customisationFields: customisationFields);
            await _basketService.Update(basket);

            TempData["addedToBasket"] = productSku;

            return RedirectToCurrentUmbracoPage();
        }

        /// <summary>
        /// Used on the basket page to update quantities in bulk
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateBasket()
        {
            var store = CurrentPage?.AncestorOrSelf<Home>()?.RescopeStore;
            if (store == null)
                return BadRequest();

            var basket = await _basketService.GetOrCreateCurrentBasket(store.Id);
            foreach (var value in Request.Form.Where(x => x.Key.StartsWith("product__")))
            {
                var sku = value.Key.Substring("product__".Length);
                var basketItem = basket.BasketLineItems.FirstOrDefault(x => x.ProductSku == sku);
                if (basketItem != null)
                {
                    var product = await _productStore.GetProductOrVariant(sku);
                    if (product != null)
                        await basket.SetProductQuantity(
                            product,
                            _stockService,
                            value.Value.GetValueAs<int>()
                        );
                }
            }

            await _basketService.Update(basket);

            return RedirectToCurrentUmbracoPage();
        }

        /// <summary>
        /// Update the delivery address of the active basket (for pricing etc)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateCountry([FromForm] string country)
        {
            var store = CurrentPage?.AncestorOrSelf<Home>()?.RescopeStore;
            if (store == null)
                return BadRequest();

            var basket = await _basketService.GetOrCreateCurrentBasket(store.Id);
            basket.DeliveryAddress!.Country = country;
            await _basketService.Update(basket);

            return RedirectToCurrentUmbracoPage();
        }
    }
}
