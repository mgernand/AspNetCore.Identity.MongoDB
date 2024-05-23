// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace SampleWebApp.Areas.Identity.Pages.Account.Manage
{
	using System.Reflection;
	using MadEyeMatt.AspNetCore.Identity.MongoDB;

	public class DownloadPersonalDataModel : PageModel
    {
        private readonly UserManager<MongoIdentityUser> _userManager;
        private readonly ILogger<DownloadPersonalDataModel> _logger;

        public DownloadPersonalDataModel(
            UserManager<MongoIdentityUser> userManager,
            ILogger<DownloadPersonalDataModel> logger)
        {
	        this._userManager = userManager;
	        this._logger = logger;
        }

        public IActionResult OnGet()
        {
            return this.NotFound();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            MongoIdentityUser user = await this._userManager.GetUserAsync(this.User);
            if (user == null)
            {
                return this.NotFound($"Unable to load user with ID '{this._userManager.GetUserId(this.User)}'.");
            }

            this._logger.LogInformation("User with ID '{UserId}' asked for their personal data.", this._userManager.GetUserId(this.User));

            // Only include personal data for download
            Dictionary<string, string> personalData = new Dictionary<string, string>();
            IEnumerable<PropertyInfo> personalDataProps = typeof(IdentityUser).GetProperties().Where(
                            prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));
            foreach (PropertyInfo p in personalDataProps)
            {
                personalData.Add(p.Name, p.GetValue(user)?.ToString() ?? "null");
            }

            IList<UserLoginInfo> logins = await this._userManager.GetLoginsAsync(user);
            foreach (UserLoginInfo l in logins)
            {
                personalData.Add($"{l.LoginProvider} external login provider key", l.ProviderKey);
            }

            personalData.Add($"Authenticator Key", await this._userManager.GetAuthenticatorKeyAsync(user));

            this.Response.Headers.Add("Content-Disposition", "attachment; filename=PersonalData.json");
            return new FileContentResult(JsonSerializer.SerializeToUtf8Bytes(personalData), "application/json");
        }
    }
}
