// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
namespace SampleWebApp.Areas.Identity.Pages.Account
{
	using MadEyeMatt.AspNetCore.Identity.MongoDB;
	using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

	public class LoginWithRecoveryCodeModel : PageModel
    {
        private readonly SignInManager<MongoIdentityUser> _signInManager;
        private readonly UserManager<MongoIdentityUser> _userManager;
        private readonly ILogger<LoginWithRecoveryCodeModel> _logger;

        public LoginWithRecoveryCodeModel(
            SignInManager<MongoIdentityUser> signInManager,
            UserManager<MongoIdentityUser> userManager,
            ILogger<LoginWithRecoveryCodeModel> logger)
        {
	        this._signInManager = signInManager;
	        this._userManager = userManager;
	        this._logger = logger;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [BindProperty]
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Recovery Code")]
            public string RecoveryCode { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            MongoIdentityUser user = await this._signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }

            this.ReturnUrl = returnUrl;

            return this.Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            if (!this.ModelState.IsValid)
            {
                return this.Page();
            }

            MongoIdentityUser user = await this._signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }

            string recoveryCode = this.Input.RecoveryCode.Replace(" ", string.Empty);

            SignInResult result = await this._signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

            string userId = await this._userManager.GetUserIdAsync(user);

            if (result.Succeeded)
            {
	            this._logger.LogInformation("User with ID '{UserId}' logged in with a recovery code.", user.Id);
                return this.LocalRedirect(returnUrl ?? this.Url.Content("~/"));
            }
            if (result.IsLockedOut)
            {
	            this._logger.LogWarning("User account locked out.");
                return this.RedirectToPage("./Lockout");
            }
            else
            {
	            this._logger.LogWarning("Invalid recovery code entered for user with ID '{UserId}' ", user.Id);
	            this.ModelState.AddModelError(string.Empty, "Invalid recovery code entered.");
                return this.Page();
            }
        }
    }
}
