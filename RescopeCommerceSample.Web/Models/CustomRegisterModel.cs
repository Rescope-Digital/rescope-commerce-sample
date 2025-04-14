using System.ComponentModel.DataAnnotations;
using Rescope.Commerce.Core.Entities;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.Models;
using DataType = System.ComponentModel.DataAnnotations.DataType;

namespace RescopeCommerceSample.Web.Models
{
    public class CustomRegisterModel : PostRedirectModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        /// <summary>
        ///     The members forename
        /// </summary>
        [Required]
        [MinLength(1)]
        public string FirstName { get; set; } = null!;

        /// <summary>
        ///     The members surname
        /// </summary>
        [Required]
        [MinLength(1)]
        public string Surname { get; set; } = null!;

        /// <summary>
        ///     The members password
        /// </summary>
        [Required]
        [StringLength(256)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = null!;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
