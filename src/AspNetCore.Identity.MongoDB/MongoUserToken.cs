namespace MadEyeMatt.AspNetCore.Identity.MongoDB
{
	using JetBrains.Annotations;
	using Microsoft.AspNetCore.Identity;

	/// <summary>
	///     Represents an authentication token for a user.
	/// </summary>
	[PublicAPI]
	public sealed class MongoUserToken
	{
		/// <summary>
		///     Gets or sets the LoginProvider this token is from.
		/// </summary>
		public string LoginProvider { get; set; }

		/// <summary>
		///     Gets or sets the name of the token.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		///     Gets or sets the token value.
		/// </summary>
		[ProtectedPersonalData]
		public string Value { get; set; }
	}
}
