// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace SampleWebApp.Areas.Identity.Pages.Account
{
	using MadEyeMatt.AspNetCore.Identity.MongoDB;

	public class LogoutModel : PageModel
    {
        private readonly SignInManager<MongoIdentityUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(SignInManager<MongoIdentityUser> signInManager, ILogger<LogoutModel> logger)
        {
	        this._signInManager = signInManager;
	        this._logger = logger;
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            await this._signInManager.SignOutAsync();
            this._logger.LogInformation("User logged out.");
            if (returnUrl != null)
            {
                return this.LocalRedirect(returnUrl);
            }
            else
            {
                // This needs to be a redirect so that the browser performs a new
                // request and the identity for the user gets updated.
                return this.RedirectToPage();
            }
        }
    }
}
