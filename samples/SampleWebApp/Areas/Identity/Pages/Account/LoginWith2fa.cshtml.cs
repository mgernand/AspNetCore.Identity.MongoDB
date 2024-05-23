// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;

namespace SampleWebApp.Areas.Identity.Pages.Account
{
	using MadEyeMatt.AspNetCore.Identity.MongoDB;
	using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

	public class LoginWith2faModel : PageModel
    {
        private readonly SignInManager<MongoIdentityUser> _signInManager;
        private readonly UserManager<MongoIdentityUser> _userManager;
        private readonly ILogger<LoginWith2faModel> _logger;

        public LoginWith2faModel(
            SignInManager<MongoIdentityUser> signInManager,
            UserManager<MongoIdentityUser> userManager,
            ILogger<LoginWith2faModel> logger)
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
        public bool RememberMe { get; set; }

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
            [Required]
            [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Text)]
            [Display(Name = "Authenticator code")]
            public string TwoFactorCode { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Display(Name = "Remember this machine")]
            public bool RememberMachine { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(bool rememberMe, string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            MongoIdentityUser user = await this._signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }

            this.ReturnUrl = returnUrl;
            this.RememberMe = rememberMe;

            return this.Page();
        }

        public async Task<IActionResult> OnPostAsync(bool rememberMe, string returnUrl = null)
        {
            if (!this.ModelState.IsValid)
            {
                return this.Page();
            }

            returnUrl = returnUrl ?? this.Url.Content("~/");

            MongoIdentityUser user = await this._signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }

            string authenticatorCode = this.Input.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            SignInResult result = await this._signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, this.Input.RememberMachine);

            string userId = await this._userManager.GetUserIdAsync(user);

            if (result.Succeeded)
            {
	            this._logger.LogInformation("User with ID '{UserId}' logged in with 2fa.", user.Id);
                return this.LocalRedirect(returnUrl);
            }
            else if (result.IsLockedOut)
            {
	            this._logger.LogWarning("User with ID '{UserId}' account locked out.", user.Id);
                return this.RedirectToPage("./Lockout");
            }
            else
            {
	            this._logger.LogWarning("Invalid authenticator code entered for user with ID '{UserId}'.", user.Id);
	            this.ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
                return this.Page();
            }
        }
    }
}
