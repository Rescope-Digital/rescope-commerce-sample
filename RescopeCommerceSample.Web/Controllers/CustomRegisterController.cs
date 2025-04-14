using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RescopeCommerceSample.Web.Models;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Cms.Web.Website.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Website.Controllers;

public class CustomRegisterController : SurfaceController
{
    private readonly IMemberManager _memberManager;
    private readonly IMemberService _memberService;
    private readonly IMemberSignInManager _memberSignInManager;
    private readonly ICoreScopeProvider _scopeProvider;

    public CustomRegisterController(
        IMemberManager memberManager,
        IMemberService memberService,
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider,
        IMemberSignInManager memberSignInManager,
        ICoreScopeProvider scopeProvider
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
        _memberManager = memberManager;
        _memberService = memberService;
        _memberSignInManager = memberSignInManager;
        _scopeProvider = scopeProvider;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ValidateUmbracoFormRouteString]
    public async Task<IActionResult> HandleRegisterMember(
        [Bind(Prefix = "registerModel")] CustomRegisterModel model
    )
    {
        if (ModelState.IsValid == false)
        {
            return CurrentUmbracoPage();
        }

        MergeRouteValuesToModel(model);

        IdentityResult result = await RegisterMemberAsync(model);
        if (result.Succeeded)
        {
            TempData["FormSuccess"] = true;

            // If there is a specified path to redirect to then use it.
            if (model.RedirectUrl.IsNullOrWhiteSpace() == false)
            {
                return Redirect(model.RedirectUrl!);
            }

            // Redirect to current page by default.
            return RedirectToCurrentUmbracoPage();
        }

        AddErrors(result);
        return CurrentUmbracoPage();
    }

    /// <summary>
    ///     We pass in values via encrypted route values so they cannot be tampered with and merge them into the model for use
    /// </summary>
    /// <param name="model"></param>
    private void MergeRouteValuesToModel(CustomRegisterModel model)
    {
        if (
            RouteData.Values.TryGetValue(
                nameof(CustomRegisterModel.RedirectUrl),
                out var redirectUrl
            )
            && redirectUrl != null
        )
        {
            model.RedirectUrl = redirectUrl.ToString();
        }
    }

    private void AddErrors(IdentityResult result)
    {
        foreach (IdentityError? error in result.Errors)
        {
            ModelState.AddModelError("registerModel", error.Description);
        }
    }

    /// <summary>
    ///     Registers a new member.
    /// </summary>
    /// <param name="model">Register member model.</param>
    /// <returns>Result of registration operation.</returns>
    private async Task<IdentityResult> RegisterMemberAsync(CustomRegisterModel model)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();

        var identityUser = MemberIdentityUser.CreateNew(
            model.Email,
            model.Email,
            Constants.Conventions.MemberTypes.DefaultAlias,
            true,
            $"{model.FirstName} {model.Surname}"
        );
        IdentityResult identityResult = await _memberManager.CreateAsync(
            identityUser,
            model.Password
        );

        if (identityResult.Succeeded)
        {
            // Update the custom properties
            // TODO: See TODO in MembersIdentityUser, Should we support custom member properties for persistence/retrieval?
            IMember? member = _memberService.GetByKey(identityUser.Key);
            if (member == null)
            {
                // should never happen
                throw new InvalidOperationException(
                    $"Could not find a member with key: {member?.Key}."
                );
            }

            member.Properties["commerceFirstName"]?.SetValue(model.FirstName);
            member.Properties["commerceLastName"]?.SetValue(model.Surname);

            _memberService.Save(member);

            await _memberSignInManager.SignInAsync(identityUser, false);
        }

        scope.Complete();

        return identityResult;
    }
}
