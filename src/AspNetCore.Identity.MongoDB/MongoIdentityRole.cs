namespace MadEyeMatt.AspNetCore.Identity.MongoDB
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Claims;
	using JetBrains.Annotations;
	using Microsoft.AspNetCore.Identity;

	/// <summary>
	///     Represents a role in the identity system.
	/// </summary>
	[PublicAPI]
	public class MongoIdentityRole : MongoIdentityRole<string>
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="MongoIdentityRole" /> type.
		/// </summary>
		public MongoIdentityRole() : this(null)
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="MongoIdentityRole" /> type.
		/// </summary>
		/// <param name="roleName">The name of the role.</param>
		public MongoIdentityRole(string roleName) : base(roleName)
		{
		}
	}

	/// <summary>
	///     Represents a role in the identity system.
	/// </summary>
	/// <typeparam name="TKey">The type of the primary key or the role.</typeparam>
	[PublicAPI]
	public class MongoIdentityRole<TKey> : IdentityRole<TKey>
		where TKey : IEquatable<TKey>
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="MongoIdentityRole{TKey}" /> type.
		/// </summary>
		public MongoIdentityRole() : this(null)
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="MongoIdentityRole{TKey}" /> type.
		/// </summary>
		/// <param name="roleName">The name of the role.</param>
		public MongoIdentityRole(string roleName) : base(roleName)
		{
			this.Claims = new List<MongoClaim>();
		}

		/// <summary>
		///     The claims associated to the role.
		/// </summary>
		public IList<MongoClaim> Claims { get; set; }

        /// <summary>
		///		Adds a claim to a the role.
        /// </summary>
        /// <param name="claim">The claim to add.</param>
        /// <returns>Returns <c>true</c> if the claim was successfully added.</returns>
        public bool AddClaim(Claim claim)
		{
			ArgumentNullException.ThrowIfNull(claim);

			// Prevent adding duplicate claims.
			bool hasClaim = this.Claims.Any(x => x.ClaimValue == claim.Value && x.ClaimType == claim.Type);
			if(hasClaim)
			{
				return false;
            }

			MongoClaim mongoClaim = new MongoClaim();
			mongoClaim.InitializeFromClaim(claim);
			this.Claims.Add(mongoClaim);

			return true;
        }

		/// <summary>
		///		Removes a <see cref="Claim"/> from the role.
		/// </summary>
		/// <param name="claim">The claim to remove.</param>
		/// <returns>Returns <c>true</c> if the claim was successfully removed.</returns>
        public bool RemoveClaim(Claim claim)
		{
			ArgumentNullException.ThrowIfNull(claim);

			MongoClaim mongoClaim = this.Claims.FirstOrDefault(x => x.ClaimValue == claim.Value && x.ClaimType == claim.Type);
			if(mongoClaim is null)
			{
				return false;
			}

			this.Claims.Remove(mongoClaim);
			return true;
        }
    }
}
