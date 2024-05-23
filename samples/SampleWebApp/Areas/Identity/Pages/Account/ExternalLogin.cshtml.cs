// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace SampleWebApp.Areas.Identity.Pages.Account
{
	using MadEyeMatt.AspNetCore.Identity.MongoDB;
	using Microsoft.AspNetCore.Authentication;
	using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

	[AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<MongoIdentityUser> _signInManager;
        private readonly UserManager<MongoIdentityUser> _userManager;
        private readonly IUserStore<MongoIdentityUser> _userStore;
        private readonly IUserEmailStore<MongoIdentityUser> _emailStore;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginModel> _logger;

        public ExternalLoginModel(
            SignInManager<MongoIdentityUser> signInManager,
            UserManager<MongoIdentityUser> userManager,
            IUserStore<MongoIdentityUser> userStore,
            ILogger<ExternalLoginModel> logger,
            IEmailSender emailSender)
        {
	        this._signInManager = signInManager;
	        this._userManager = userManager;
	        this._userStore = userStore;
	        this._emailStore = this.GetEmailStore();
	        this._logger = logger;
	        this._emailSender = emailSender;
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
        public string ProviderDisplayName { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

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
            [EmailAddress]
            public string Email { get; set; }
        }
        
        public IActionResult OnGet() => this.RedirectToPage("./Login");

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            string redirectUrl = this.Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            AuthenticationProperties properties = this._signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? this.Url.Content("~/");
            if (remoteError != null)
            {
	            this.ErrorMessage = $"Error from external provider: {remoteError}";
                return this.RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
            ExternalLoginInfo info = await this._signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
	            this.ErrorMessage = "Error loading external login information.";
                return this.RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Sign in the user with this external login provider if the user already has a login.
            SignInResult result = await this._signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
	            this._logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
                return this.LocalRedirect(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return this.RedirectToPage("./Lockout");
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                this.ReturnUrl = returnUrl;
                this.ProviderDisplayName = info.ProviderDisplayName;
                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                {
	                this.Input = new InputModel
                    {
                        Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                    };
                }
                return this.Page();
            }
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? this.Url.Content("~/");
            // Get the information about the user from the external login provider
            ExternalLoginInfo info = await this._signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
	            this.ErrorMessage = "Error loading external login information during confirmation.";
                return this.RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (this.ModelState.IsValid)
            {
                MongoIdentityUser user = this.CreateUser();

                await this._userStore.SetUserNameAsync(user, this.Input.Email, CancellationToken.None);
                await this._emailStore.SetEmailAsync(user, this.Input.Email, CancellationToken.None);

                IdentityResult result = await this._userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await this._userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
	                    this._logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                        string userId = await this._userManager.GetUserIdAsync(user);
                        string code = await this._userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        string callbackUrl = this.Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = userId, code = code },
                            protocol: this.Request.Scheme);

                        await this._emailSender.SendEmailAsync(this.Input.Email, "Confirm your email",
                            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                        // If account confirmation is required, we need to show the link if we don't have a real email sender
                        if (this._userManager.Options.SignIn.RequireConfirmedAccount)
                        {
                            return this.RedirectToPage("./RegisterConfirmation", new { Email = this.Input.Email });
                        }

                        await this._signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                        return this.LocalRedirect(returnUrl);
                    }
                }
                foreach (IdentityError error in result.Errors)
                {
	                this.ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            this.ProviderDisplayName = info.ProviderDisplayName;
            this.ReturnUrl = returnUrl;
            return this.Page();
        }

        private MongoIdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<MongoIdentityUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(MongoIdentityUser)}'. " +
                    $"Ensure that '{nameof(MongoIdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the external login page in /Areas/Identity/Pages/Account/ExternalLogin.cshtml");
            }
        }

        private IUserEmailStore<MongoIdentityUser> GetEmailStore()
        {
            if (!this._userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<MongoIdentityUser>)this._userStore;
        }
    }
}
