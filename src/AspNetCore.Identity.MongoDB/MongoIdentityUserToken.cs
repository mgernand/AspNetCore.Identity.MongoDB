namespace MadEyeMatt.AspNetCore.Identity.MongoDB
{
	using System;
	using global::MongoDB.Bson.Serialization.Attributes;
	using JetBrains.Annotations;
	using Microsoft.AspNetCore.Identity;

	/// <summary>
	///     Represents an authentication token for a user.
	/// </summary>
	/// <remarks>
	///		HACK
	/// </remarks>
	/// <typeparam name="TKey">The type of the primary key used for users.</typeparam>
	[PublicAPI]
	public class MongoIdentityUserToken<TKey> : IdentityUserToken<TKey>
		where TKey : IEquatable<TKey>
	{
		/// <summary>
		///     The user of this token.
		/// </summary>
		[BsonIgnore]
		public MongoIdentityUser<TKey> User { get; set; }

		/// <summary>
		///     The wrapped user token.
		/// </summary>
		[BsonIgnore]
		public MongoUserToken Token { get; set; }


		/// <inheritdoc />
		public override string LoginProvider
		{
			get => base.LoginProvider;
			set
			{
				base.LoginProvider = value;

				// HACK
				this.Token ??= new MongoUserToken();
				this.Token.LoginProvider = value;
            }
		}

		/// <inheritdoc />
		public override string Name
		{
			get => base.Name;
			set
			{
				base.Name = value;

				// HACK
				this.Token ??= new MongoUserToken();
				this.Token.Name = value;
			}
        }

		/// <inheritdoc />
		[ProtectedPersonalData]
        public override string Value
		{
			get => base.Value;
			set
			{
				base.Value = value;

				// HACK
				this.Token ??= new MongoUserToken();
                this.Token.Value = value;
			}
		}
	}
}
