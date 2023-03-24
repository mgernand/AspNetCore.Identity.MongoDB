namespace MadEyeMatt.AspNetCore.Identity.MongoDB
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Claims;
	using JetBrains.Annotations;
	using Microsoft.AspNetCore.Identity;

	/// <summary>
	///     Represents a user in the identity system.
	/// </summary>
	[PublicAPI]
	public class MongoIdentityUser : MongoIdentityUser<string>
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="MongoIdentityUser" /> type.
		/// </summary>
		public MongoIdentityUser() : this(null)
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="MongoIdentityUser" /> type.
		/// </summary>
		/// <param name="userName">The user name.</param>
		public MongoIdentityUser(string userName) : base(userName)
		{
		}
	}

	/// <summary>
	///     Represents a user in the identity system.
	/// </summary>
	/// <typeparam name="TKey">The type of the primary key or the user.</typeparam>
	[PublicAPI]
	public class MongoIdentityUser<TKey> : IdentityUser<TKey>
		where TKey : IEquatable<TKey>
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="MongoIdentityUser{TKey}" /> type.
		/// </summary>
		public MongoIdentityUser() : this(null)
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="MongoIdentityUser{TKey}" /> type.
		/// </summary>
		/// <param name="userName">The user name.</param>
		public MongoIdentityUser(string userName) : base(userName)
		{
			this.Roles = new List<TKey>();
			this.Claims = new List<MongoClaim>();
			this.Logins = new List<MongoUserLogin>();
			this.Tokens = new List<MongoUserToken>();
		}

		/// <summary>
		///     The IDs of the roles of the user.
		/// </summary>
		public IList<TKey> Roles { get; set; }

		/// <summary>
		///     The claims of the user.
		/// </summary>
		public IList<MongoClaim> Claims { get; set; }

		/// <summary>
		///     The logins of the user.
		/// </summary>
		public IList<MongoUserLogin> Logins { get; set; }

		/// <summary>
		///     The authentication tokens of the user.
		/// </summary>
		public IList<MongoUserToken> Tokens { get; set; }

		/// <summary>
		///     Tries to get a user login for the provided parameters.
		/// </summary>
		/// <param name="loginProvider"></param>
		/// <param name="providerKey"></param>
		/// <returns></returns>
		public IdentityUserLogin<TKey> GetUserLogin(string loginProvider, string providerKey)
		{
			MongoUserLogin login = this.Logins.FirstOrDefault(x => x.LoginProvider == loginProvider && x.ProviderKey == providerKey);
			if(login != null)
			{
				return new IdentityUserLogin<TKey>
				{
					UserId = this.Id,
					LoginProvider = login.LoginProvider,
					ProviderDisplayName = login.ProviderDisplayName,
					ProviderKey = login.ProviderKey
				};
			}

			return null;
		}

		/// <summary>
		///     Tries to get a user token for the provided parameters.
		/// </summary>
		/// <param name="loginProvider"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public MongoIdentityUserToken<TKey> GetToken(string loginProvider, string name)
		{
			MongoUserToken token = this.Tokens.FirstOrDefault(x => x.LoginProvider == loginProvider && x.Name == name);
			if(token != null)
			{
				return new MongoIdentityUserToken<TKey>()
                {
					UserId = this.Id,
					LoginProvider = token.LoginProvider,
					Name = token.Name,
					Value = token.Value,

					User = this,
					Token = token
				};
			}

			return null;
		}

		/// <summary>
		///     Adds a claim to a the user.
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
		///     Removes a claim from the user.
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

		/// <summary>
		///     Replaces a claim of the user.
		/// </summary>
		/// <param name="claim">The claim to replace.</param>
		/// <param name="newClaim">The new claim to set.</param>
		/// <returns>Returns <c>true</c> if the claim was successfully replaced.</returns>
		public bool ReplaceClaim(Claim claim, Claim newClaim)
		{
			bool replaced = false;

			IList<MongoClaim> mongoClaims = this.Claims.Where(uc => uc.ClaimValue == claim.Value && uc.ClaimType == claim.Type).ToList();
			foreach(MongoClaim mongoClaim in mongoClaims)
			{
				mongoClaim.ClaimType = newClaim.Type;
				mongoClaim.ClaimValue = newClaim.Value;
				replaced = true;
			}

			return replaced;
		}

		/// <summary>
		///     Adds a role to a the user.
		/// </summary>
		/// <param name="roleId">The id of the role add.</param>
		/// <returns>Returns <c>true</c> if the role was successfully added.</returns>
		public bool AddRole(TKey roleId)
		{
			ArgumentNullException.ThrowIfNull(roleId);
			if(roleId.Equals(default))
			{
				throw new ArgumentNullException(nameof(roleId));
			}

			// Prevent adding duplicate roles.
			if(this.Roles.Contains(roleId))
			{
				return false;
			}

			this.Roles.Add(roleId);
			return true;
		}

		/// <summary>
		///     Removes a role from the user.
		/// </summary>
		/// <param name="roleId">The id of the role to remove.</param>
		/// <returns>Returns <c>true</c> if the role was successfully removed.</returns>

        public bool RemoveRole(TKey roleId)
		{
			ArgumentNullException.ThrowIfNull(roleId);
			if(roleId.Equals(default))
			{
				throw new ArgumentNullException(nameof(roleId));
			}

			TKey id = this.Roles.FirstOrDefault(e => e.Equals(roleId));
			if(id == null || id.Equals(default))
			{
				return false;
			}

			this.Roles.Remove(roleId);
			return true;

		}

		/// <summary>
		///     Adds a login to a the user.
		/// </summary>
		/// <param name="login">The login to add.</param>
		/// <returns>Returns <c>true</c> if the login was successfully added.</returns>
		public bool AddLogin(UserLoginInfo login)
		{
			ArgumentNullException.ThrowIfNull(login);

			// Prevent adding duplicate logins.
			bool hasLogin = this.Logins.Any(x => x.LoginProvider == login.LoginProvider && x.ProviderKey == login.ProviderKey);
			if(hasLogin)
			{
				return false;
			}

			MongoUserLogin mongoUserLogin = new MongoUserLogin();
			mongoUserLogin.InitializeFromLoginInfo(login);
			this.Logins.Add(mongoUserLogin);

			return true;
		}

		/// <summary>
		///     Removes a login from the user.
		/// </summary>
		/// <param name="login">The login to remove.</param>
		/// <returns>Returns <c>true</c> if the login was successfully removed.</returns>
		public bool RemoveLogin(UserLoginInfo login)
		{
			ArgumentNullException.ThrowIfNull(login);

			MongoUserLogin mongoUserLogin = this.Logins.FirstOrDefault(x => x.LoginProvider == login.LoginProvider && x.ProviderKey == login.ProviderKey);
			if(mongoUserLogin is null)
			{
				return false;
			}

			this.Logins.Remove(mongoUserLogin);
			return true;
		}

		/// <summary>
		///     Adds a token to a the user.
		/// </summary>
		/// <param name="token">The token to add.</param>
		/// <returns>Returns <c>true</c> if the token was successfully added.</returns>
		public bool AddToken(IdentityUserToken<TKey> token)
		{
			ArgumentNullException.ThrowIfNull(token);

			// Prevent adding duplicate tokens.
			bool hasToken = this.Tokens.Any(x => x.LoginProvider == token.LoginProvider && x.Name == token.Name && x.Value == token.Value);
			if(hasToken)
			{
				return false;
			}

			MongoUserToken mongoUserToken = new MongoUserToken
			{
				LoginProvider = token.LoginProvider,
				Name = token.Name,
				Value = token.Value
			};
			this.Tokens.Add(mongoUserToken);

			return true;
		}

		/// <summary>
		///     Removes a token from the user.
		/// </summary>
		/// <param name="token">The token to remove.</param>
		/// <returns>Returns <c>true</c> if the token was successfully removed.</returns>
		public bool RemoveToken(IdentityUserToken<TKey> token)
		{
			ArgumentNullException.ThrowIfNull(token);

			MongoUserToken mongoUserToken = this.Tokens.FirstOrDefault(x => x.LoginProvider == token.LoginProvider && x.Name == token.Name);
			if(mongoUserToken is null)
			{
				return false;
			}

			this.Tokens.Remove(mongoUserToken);
			return true;
		}
	}
}
