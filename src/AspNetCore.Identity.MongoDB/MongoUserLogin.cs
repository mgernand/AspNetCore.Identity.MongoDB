namespace MadEyeMatt.AspNetCore.Identity.MongoDB
{
	using JetBrains.Annotations;
	using Microsoft.AspNetCore.Identity;

	/// <summary>
	///     Represents a login and its associated provider for a user.
	/// </summary>
	[PublicAPI]
	public sealed class MongoUserLogin
	{
		/// <summary>
		///     Gets or sets the login provider for the login (e.g. facebook, google)
		/// </summary>
		public string LoginProvider { get; set; }

		/// <summary>
		///     Gets or sets the unique provider identifier for this login.
		/// </summary>
		public string ProviderKey { get; set; }

		/// <summary>
		///     Gets or sets the friendly name used in a UI for this login.
		/// </summary>
		public string ProviderDisplayName { get; set; }

        /// <summary>
        ///     Initializes by copying values from the login info.
        /// </summary>
        /// <param name="login">The login info to initialize from.</param>
        public void InitializeFromLoginInfo(UserLoginInfo login)
		{
			this.LoginProvider = login?.LoginProvider;
			this.ProviderKey = login?.ProviderKey;
			this.ProviderDisplayName = login?.ProviderDisplayName;
		}

        /// <summary>
        ///		Creates a new <see cref="UserLoginInfo"/> instance from this login.
        /// </summary>
        /// <returns>The new <see cref="UserLoginInfo"/> instance.</returns>
        public UserLoginInfo ToUserLoginInfo()
		{
			return new UserLoginInfo(this.LoginProvider, this.ProviderKey, this.ProviderDisplayName);
		}
	}
}
