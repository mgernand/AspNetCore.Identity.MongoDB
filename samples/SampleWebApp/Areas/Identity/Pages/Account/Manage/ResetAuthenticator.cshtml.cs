// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace SampleWebApp.Areas.Identity.Pages.Account.Manage
{
	using MadEyeMatt.AspNetCore.Identity.MongoDB;

	public class ResetAuthenticatorModel : PageModel
    {
        private readonly UserManager<MongoIdentityUser> _userManager;
        private readonly SignInManager<MongoIdentityUser> _signInManager;
        private readonly ILogger<ResetAuthenticatorModel> _logger;

        public ResetAuthenticatorModel(
            UserManager<MongoIdentityUser> userManager,
            SignInManager<MongoIdentityUser> signInManager,
            ILogger<ResetAuthenticatorModel> logger)
        {
	        this._userManager = userManager;
	        this._signInManager = signInManager;
	        this._logger = logger;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGet()
        {
            MongoIdentityUser user = await this._userManager.GetUserAsync(this.User);
            if (user == null)
            {
                return this.NotFound($"Unable to load user with ID '{this._userManager.GetUserId(this.User)}'.");
            }

            return this.Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            MongoIdentityUser user = await this._userManager.GetUserAsync(this.User);
            if (user == null)
            {
                return this.NotFound($"Unable to load user with ID '{this._userManager.GetUserId(this.User)}'.");
            }

            await this._userManager.SetTwoFactorEnabledAsync(user, false);
            await this._userManager.ResetAuthenticatorKeyAsync(user);
            string userId = await this._userManager.GetUserIdAsync(user);
            this._logger.LogInformation("User with ID '{UserId}' has reset their authentication app key.", user.Id);

            await this._signInManager.RefreshSignInAsync(user);
            this.StatusMessage = "Your authenticator app key has been reset, you will need to configure your authenticator app using the new key.";

            return this.RedirectToPage("./EnableAuthenticator");
        }
    }
}
