namespace MadEyeMatt.AspNetCore.Identity.MongoDB
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Linq.Expressions;
	using System.Security.Claims;
	using System.Threading;
	using System.Threading.Tasks;
	using global::MongoDB.Driver;
	using JetBrains.Annotations;
	using Microsoft.AspNetCore.Identity;

	/// <summary>
	///     Provides an API for a persistence store for users without roles.
	/// </summary>
	[PublicAPI]
	public class UserOnlyStore : UserOnlyStore<MongoIdentityUser<string>>
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="UserOnlyStore{TUser}" /> type.
		/// </summary>
		/// <param name="context">The <see cref="MongoDbContext" />.</param>
		/// <param name="describer">The <see cref="IdentityErrorDescriber" />.</param>
		public UserOnlyStore(MongoDbContext context, IdentityErrorDescriber describer = null)
			: base(context, describer)
		{
		}
	}

	/// <summary>
	///     Provides an API for a persistence store for users without roles.
	/// </summary>
	/// <typeparam name="TUser">The type of the class representing a user.</typeparam>
	[PublicAPI]
	public class UserOnlyStore<TUser> : UserOnlyStore<TUser, MongoDbContext>
		where TUser : MongoIdentityUser<string>
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="UserOnlyStore{TUser}" /> type.
		/// </summary>
		/// <param name="context">The <see cref="MongoDbContext" />.</param>
		/// <param name="describer">The <see cref="IdentityErrorDescriber" />.</param>
		public UserOnlyStore(MongoDbContext context, IdentityErrorDescriber describer = null)
			: base(context, describer)
		{
		}
	}

	/// <summary>
	///     Provides an API for a persistence store for users without roles.
	/// </summary>
	/// <typeparam name="TUser">The type of the class representing a user.</typeparam>
	/// <typeparam name="TContext">The type of the data context class used to access the store.</typeparam>
	[PublicAPI]
	public class UserOnlyStore<TUser, TContext> : UserOnlyStore<TUser, TContext, string>
		where TUser : MongoIdentityUser<string>
		where TContext : MongoDbContext
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="UserOnlyStore{TUser, TContext}" /> type.
		/// </summary>
		/// <param name="context">The <see cref="MongoDbContext" />.</param>
		/// <param name="describer">The <see cref="IdentityErrorDescriber" />.</param>
		public UserOnlyStore(TContext context, IdentityErrorDescriber describer = null)
			: base(context, describer)
		{
		}
	}

	/// <summary>
	///     Provides an API for a persistence store for users without roles.
	/// </summary>
	/// <typeparam name="TUser">The type of the class representing a user.</typeparam>
	/// <typeparam name="TContext">The type of the data context class used to access the store.</typeparam>
	/// <typeparam name="TKey">The type of the primary key for a user.</typeparam>
	[PublicAPI]
	public class UserOnlyStore<TUser, TContext, TKey> : UserStoreBase<TUser, TKey, IdentityUserClaim<TKey>, IdentityUserLogin<TKey>, MongoIdentityUserToken<TKey>>
		where TUser : MongoIdentityUser<TKey>
		where TContext : MongoDbContext
		where TKey : IEquatable<TKey>
	{
		private readonly TContext context;

		/// <summary>
		///     Initializes a new instance of the <see cref="UserOnlyStore{TUser, TContext, TKey}" /> type.
		/// </summary>
		/// <param name="context">The <see cref="MongoDbContext" />.</param>
		/// <param name="describer">The <see cref="IdentityErrorDescriber" />.</param>
		public UserOnlyStore(TContext context, IdentityErrorDescriber describer)
			: base(describer ?? new IdentityErrorDescriber())
		{
			ArgumentNullException.ThrowIfNull(context);

			this.context = context;
		}

		/// <inheritdoc />
		public override IQueryable<TUser> Users => this.UsersCollection.AsQueryable();

		/// <summary>
		///     The collection of users in the database.
		/// </summary>
		public virtual IMongoCollection<TUser> UsersCollection => this.context.GetCollection<TUser>();

		/// <inheritdoc />
		public override async Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			ArgumentNullException.ThrowIfNull(user);

			user.ConcurrencyStamp = Guid.NewGuid().ToString("N");
			await this.UsersCollection.InsertOneAsync(user, new InsertOneOptions(), cancellationToken);

			return IdentityResult.Success;
		}

		/// <inheritdoc />
		public override async Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			ArgumentNullException.ThrowIfNull(user);

			string oldConcurrencyStamp = user.ConcurrencyStamp;
			user.ConcurrencyStamp = Guid.NewGuid().ToString("N");

			Expression<Func<TUser, bool>> predicate = x => x.Id.Equals(user.Id) && x.ConcurrencyStamp.Equals(oldConcurrencyStamp);
			ReplaceOneResult result = await this.UsersCollection.ReplaceOneAsync(predicate, user, cancellationToken: cancellationToken);

			return result.ModifiedCount == 0
				? IdentityResult.Failed(this.ErrorDescriber.ConcurrencyFailure())
				: IdentityResult.Success;
		}

		/// <inheritdoc />
		public override async Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			ArgumentNullException.ThrowIfNull(user);

			string oldConcurrencyStamp = user.ConcurrencyStamp;
			user.ConcurrencyStamp = Guid.NewGuid().ToString("N");

			Expression<Func<TUser, bool>> predicate = x => x.Id.Equals(user.Id) && x.ConcurrencyStamp.Equals(oldConcurrencyStamp);
			DeleteResult result = await this.UsersCollection.DeleteOneAsync(predicate, cancellationToken);

			return result.DeletedCount == 0
				? IdentityResult.Failed(this.ErrorDescriber.ConcurrencyFailure())
				: IdentityResult.Success;
		}

		/// <inheritdoc />
		public override async Task<TUser> FindByIdAsync(string id, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();

			TKey userId = this.ConvertIdFromString(id);
			return await this.UsersCollection.Find(x => x.Id.Equals(userId)).FirstOrDefaultAsync(cancellationToken);
		}

		/// <inheritdoc />
		public override async Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();

			return await this.UsersCollection.Find(x => x.NormalizedUserName == normalizedUserName).FirstOrDefaultAsync(cancellationToken);
		}

		/// <inheritdoc />
		public override async Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();

			return await this.UsersCollection.Find(x => x.NormalizedEmail == normalizedEmail).FirstOrDefaultAsync(cancellationToken);
		}

        /// <inheritdoc />
        public override Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			ArgumentNullException.ThrowIfNull(user);

			IList<Claim> claims = user.Claims.Select(x => x.ToClaim()).ToList();
			return Task.FromResult(claims);
		}

		/// <inheritdoc />
		public override Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			ArgumentNullException.ThrowIfNull(user);
			ArgumentNullException.ThrowIfNull(claims);

			foreach(Claim claim in claims)
			{
				user.AddClaim(claim);
			}

			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public override Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			ArgumentNullException.ThrowIfNull(user);
			ArgumentNullException.ThrowIfNull(claim);
			ArgumentNullException.ThrowIfNull(newClaim);

			user.ReplaceClaim(claim, newClaim);

			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public override Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			ArgumentNullException.ThrowIfNull(user);
			ArgumentNullException.ThrowIfNull(claims);

			foreach(Claim claim in claims)
			{
				user.RemoveClaim(claim);
			}

			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public override Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			ArgumentNullException.ThrowIfNull(user);
			ArgumentNullException.ThrowIfNull(login);

			user.AddLogin(login);

			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public override Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			ArgumentNullException.ThrowIfNull(user);

			MongoUserLogin mongoUserLogin = user.Logins.FirstOrDefault(x => x.LoginProvider == loginProvider && x.ProviderKey == providerKey);
			if(mongoUserLogin is not null)
			{
				user.RemoveLogin(mongoUserLogin.ToUserLoginInfo());
			}

			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public override Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			ArgumentNullException.ThrowIfNull(user);

			IList<UserLoginInfo> logins = user.Logins.Select(x => x.ToUserLoginInfo()).ToList();
			return Task.FromResult(logins);
		}

		/// <inheritdoc />
		public override async Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			ArgumentNullException.ThrowIfNull(claim);

			FilterDefinition<TUser> filterDefinition = Builders<TUser>.Filter.ElemMatch(x => x.Claims, claims => claims.ClaimValue == claim.Value && claims.ClaimType == claim.Type);
			return await this.UsersCollection.Find(filterDefinition).ToListAsync(cancellationToken);
		}

		/// <inheritdoc />
		protected override Task<MongoIdentityUserToken<TKey>> FindTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			ArgumentNullException.ThrowIfNull(user);

			// HACK
            MongoIdentityUserToken<TKey> token = user.GetToken(loginProvider, name);
            return Task.FromResult(token);
		}

		/// <inheritdoc />
		protected override async Task<TUser> FindUserAsync(TKey userId, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();

			return await this.UsersCollection.Find(x => x.Id.Equals(userId)).FirstOrDefaultAsync(cancellationToken);
		}

		/// <inheritdoc />
		protected override async Task<IdentityUserLogin<TKey>> FindUserLoginAsync(TKey userId, string loginProvider, string providerKey, CancellationToken cancellationToken)
		{
			Expression<Func<TUser, bool>> predicate = x => x.Id.Equals(userId) && x.Logins.Any(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey);
			TUser user = await this.UsersCollection.Find(predicate).FirstOrDefaultAsync(cancellationToken);
			return user?.GetUserLogin(loginProvider, providerKey);
		}

		/// <inheritdoc />
		protected override async Task<IdentityUserLogin<TKey>> FindUserLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
		{
			Expression<Func<TUser, bool>> predicate = x => x.Logins.Any(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey);
			TUser user = await this.UsersCollection.Find(predicate).FirstOrDefaultAsync(cancellationToken);
			return user?.GetUserLogin(loginProvider, providerKey);
		}

        /// <inheritdoc />
        protected override Task AddUserTokenAsync(MongoIdentityUserToken<TKey> token)
		{
			this.ThrowIfDisposed();
			ArgumentNullException.ThrowIfNull(token);

			//TUser user = await this.UsersCollection.Find(x => x.Id.Equals(token.UserId)).FirstOrDefaultAsync();
			token.User.AddToken(token);

			return Task.CompletedTask;
		}

		/// <inheritdoc />
		protected override Task RemoveUserTokenAsync(MongoIdentityUserToken<TKey> token)
		{
			this.ThrowIfDisposed();
			ArgumentNullException.ThrowIfNull(token);

			//TUser user = await this.UsersCollection.Find(x => x.Id.Equals(token.UserId)).FirstOrDefaultAsync();
			token.User.RemoveToken(token);

			return Task.CompletedTask;
        }

		/// <inheritdoc />
		protected override MongoIdentityUserToken<TKey> CreateUserToken(TUser user, string loginProvider, string name, string value)
		{
			// HACK
			MongoIdentityUserToken<TKey> token = base.CreateUserToken(user, loginProvider, name, value);
			token.User = user;
			return token;
		}
    }
}
