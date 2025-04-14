using Microsoft.AspNetCore.Mvc;
using Rescope.Commerce.Core.Services;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.PublishedModels;
using Umbraco.Cms.Web.Website.Controllers;

namespace RescopeCommerceSample.Web.Controllers
{
    /// <summary>
    /// Used by the site's front-end for subscription management
    /// </summary>
    public class AccountSurfaceController : SurfaceController
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly IMemberManager _memberManager;

        public AccountSurfaceController(
            IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger profilingLogger,
            IPublishedUrlProvider publishedUrlProvider,
            ISubscriptionService subscriptionService,
            IMemberManager memberManager
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
            _subscriptionService = subscriptionService;
            _memberManager = memberManager;
        }

        /// <summary>
        /// Cancels a subscription
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CancelSubscription(Guid subscriptionId)
        {
            var member = await _memberManager.GetCurrentMemberAsync();
            if (member == null)
                return BadRequest();

            var store = CurrentPage?.AncestorOrSelf<Home>()?.RescopeStore;
            if (store == null)
                return BadRequest();

            var subscription = await _subscriptionService.Get(subscriptionId);
            if (subscription == null || subscription.UmbracoMemberKey != member.Key)
                return RedirectToCurrentUmbracoPage();

            if (!subscription.Enabled)
                return RedirectToCurrentUmbracoPage();

            subscription.Enabled = false;
            await _subscriptionService.Update(subscription);

            return RedirectToCurrentUmbracoPage();
        }
    }
}
