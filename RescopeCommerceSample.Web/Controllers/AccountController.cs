using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace RescopeCommerceSample.Web.Controllers
{
    public class AccountController : RenderController
    {
        public AccountController(ILogger<AccountController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoContextAccessor umbracoContextAccessor)
            : base(logger, compositeViewEngine, umbracoContextAccessor)
        {

        }

        public sealed override IActionResult Index()
        {
            var isLoggedIn = User?.Identity?.IsAuthenticated ?? false;
            if (!isLoggedIn)
            {
                var loginPage = CurrentPage?.FirstChild<Login>();
                return Redirect(loginPage?.Url() ?? "/");
            }

            return CurrentTemplate(CurrentPage);
        }
    }
}

