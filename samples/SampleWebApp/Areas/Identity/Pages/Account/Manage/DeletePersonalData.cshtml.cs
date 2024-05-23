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

namespace SampleWebApp.Areas.Identity.Pages.Account.Manage
{
	using MadEyeMatt.AspNetCore.Identity.MongoDB;

	public class DeletePersonalDataModel : PageModel
    {
        private readonly UserManager<MongoIdentityUser> _userManager;
        private readonly SignInManager<MongoIdentityUser> _signInManager;
        private readonly ILogger<DeletePersonalDataModel> _logger;

        public DeletePersonalDataModel(
            UserManager<MongoIdentityUser> userManager,
            SignInManager<MongoIdentityUser> signInManager,
            ILogger<DeletePersonalDataModel> logger)
        {
	        this._userManager = userManager;
	        this._signInManager = signInManager;
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
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public bool RequirePassword { get; set; }

        public async Task<IActionResult> OnGet()
        {
            MongoIdentityUser user = await this._userManager.GetUserAsync(this.User);
            if (user == null)
            {
                return this.NotFound($"Unable to load user with ID '{this._userManager.GetUserId(this.User)}'.");
            }

            this.RequirePassword = await this._userManager.HasPasswordAsync(user);
            return this.Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            MongoIdentityUser user = await this._userManager.GetUserAsync(this.User);
            if (user == null)
            {
                return this.NotFound($"Unable to load user with ID '{this._userManager.GetUserId(this.User)}'.");
            }

            this.RequirePassword = await this._userManager.HasPasswordAsync(user);
            if (this.RequirePassword)
            {
                if (!await this._userManager.CheckPasswordAsync(user, this.Input.Password))
                {
	                this.ModelState.AddModelError(string.Empty, "Incorrect password.");
                    return this.Page();
                }
            }

            IdentityResult result = await this._userManager.DeleteAsync(user);
            string userId = await this._userManager.GetUserIdAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred deleting user.");
            }

            await this._signInManager.SignOutAsync();

            this._logger.LogInformation("User with ID '{UserId}' deleted themselves.", userId);

            return this.Redirect("~/");
        }
    }
}
