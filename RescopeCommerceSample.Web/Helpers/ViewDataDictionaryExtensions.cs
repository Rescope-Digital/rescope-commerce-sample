using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace RescopeCommerceSample.Web.Helpers
{
    /// <summary>
    /// Helpful extension methods to easily store order information on viewdata
    /// </summary>
    public static class ViewDataDictionaryExtensions
    {
        public static ViewDataDictionary SetOrderId(this ViewDataDictionary viewData, Guid? orderId)
        {
            if (orderId != null)
                viewData[DemoConstants.RescopeOrderId] = orderId;
            else
                viewData.RemoveAll(x => x.Key == DemoConstants.RescopeOrderId);

            return viewData;
        }

        public static bool HasOrderId(this ViewDataDictionary viewData)
        {
            return viewData.ContainsKey(DemoConstants.RescopeOrderId);
        }

        public static Guid? GetOrderId(this ViewDataDictionary viewData)
        {
            var guid = viewData[DemoConstants.RescopeOrderId] as Guid?;
            return guid;
        }

        public static ViewDataDictionary SetPaymentHTML(this ViewDataDictionary viewData, string? html)
        {
            if (!string.IsNullOrEmpty(html))
                viewData[DemoConstants.RescopePaymentHtml] = html;
            else
                viewData.RemoveAll(x => x.Key == DemoConstants.RescopePaymentHtml);

            return viewData;
        }

        public static bool HasPaymentHTML(this ViewDataDictionary viewData)
        {
            return viewData.ContainsKey(DemoConstants.RescopePaymentHtml);
        }

        public static string? GetPaymentHTML(this ViewDataDictionary viewData)
        {
            var str = viewData[DemoConstants.RescopePaymentHtml] as string;
            return str;
        }
    }
}

