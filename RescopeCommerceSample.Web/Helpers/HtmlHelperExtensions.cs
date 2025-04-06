using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rescope.Commerce.Core.Entities;

namespace RescopeCommerceSample.Web.Helpers
{
    public static class HtmlHelperExtensions
    {
        public static void SetStore(this IHtmlHelper htmlHelper, Store? store)
        {
            htmlHelper.ViewData["RescopeStoreId"] = store?.Id;
        }

        public static Guid GetStoreId(this IHtmlHelper htmlHelper)
        {
            var guid = htmlHelper.ViewData["RescopeStoreId"] as Guid?;
            return guid.Value;
        }

        public static Guid GetStoreId(this Controller controller)
        {
            var guid = controller.ViewData["RescopeStoreId"] as Guid?;
            return guid.Value;
        }

        public static void SetStore(this Controller controller, Store? store)
        {
            controller.ViewData["RescopeStoreId"] = store?.Id;
        }
    }
}

