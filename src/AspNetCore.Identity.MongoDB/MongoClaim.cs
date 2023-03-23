namespace MadEyeMatt.AspNetCore.Identity.MongoDB
{
	using System.Security.Claims;
	using JetBrains.Annotations;

	/// <summary>
	///     Represents a claim that is granted to users and roles.
	/// </summary>
	[PublicAPI]
	public sealed class MongoClaim
	{
		/// <summary>
		///     Gets or sets the claim type for this claim.
		/// </summary>
		public string ClaimType { get; set; }

		/// <summary>
		///     Gets or sets the claim value for this claim.
		/// </summary>
		public string ClaimValue { get; set; }

		/// <summary>
		///     Constructs a new claim with the type and value.
		/// </summary>
		/// <returns>The <see cref="T:System.Security.Claims.Claim" /> that was produced.</returns>
		public Claim ToClaim()
		{
			return new Claim(this.ClaimType, this.ClaimValue);
		}

		/// <summary>
		///     Initializes by copying ClaimType and ClaimValue from the other claim.
		/// </summary>
		/// <param name="other">The claim to initialize from.</param>
		public void InitializeFromClaim(Claim other)
		{
			this.ClaimType = other?.Type;
			this.ClaimValue = other?.Value;
		}
	}
}
