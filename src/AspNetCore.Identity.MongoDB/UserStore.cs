using Microsoft.AspNetCore.Identity;

namespace MadEyeMatt.AspNetCore.Identity.MongoDB
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Linq.Expressions;
	using System.Security.Claims;
	using System.Threading;
	using System.Threading.Tasks;
	using global::MongoDB.Driver;
    using JetBrains.Annotations;

	/// <summary>
	///     Provides an API for a persistence store for users.
	/// </summary>
    [PublicAPI]
    public class UserStore : UserStore<MongoIdentityUser<string>>
    {
		/// <summary>
		///     Initializes a new instance of the <see cref="UserStore" /> type.
		/// </summary>
		/// <param name="context">The <see cref="MongoDbContext" />.</param>
		/// <param name="describer">The <see cref="IdentityErrorDescriber" />.</param>
        public UserStore(MongoDbContext context, IdentityErrorDescriber describer = null)
        : base(context, describer)
        {
		}
    }

	/// <summary>
	///     Provides an API for a persistence store for users.
	/// </summary>
	/// <typeparam name="TUser">The type of the class representing a user.</typeparam>
	[PublicAPI]
	public class UserStore<TUser> : UserStore<TUser, MongoIdentityRole<string>>
		where TUser : MongoIdentityUser<string>
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="UserStore{TUser}" /> type.
		/// </summary>
		/// <param name="context">The <see cref="MongoDbContext" />.</param>
		/// <param name="describer">The <see cref="IdentityErrorDescriber" />.</param>
		public UserStore(MongoDbContext context, IdentityErrorDescriber describer = null)
			: base(context, describer)
		{
		}
	}

    /// <summary>
    ///     Provides an API for a persistence store for users.
    /// </summary>
    /// <typeparam name="TUser">The type of the class representing a user.</typeparam>
    /// <typeparam name="TRole">The type of the class representing a role.</typeparam>
    [PublicAPI]
	public class UserStore<TUser, TRole> : UserStore<TUser, TRole, MongoDbContext>
		where TUser : MongoIdentityUser<string>
		where TRole : MongoIdentityRole<string>
    {
		/// <summary>
		///     Initializes a new instance of the <see cref="UserStore{TUser}" /> type.
		/// </summary>
		/// <param name="context">The <see cref="MongoDbContext" />.</param>
		/// <param name="describer">The <see cref="IdentityErrorDescriber" />.</param>
		public UserStore(MongoDbContext context, IdentityErrorDescriber describer = null)
			: base(context, describer)
		{
		}
	}

    /// <summary>
    ///     Provides an API for a persistence store for users.
    /// </summary>
    /// <typeparam name="TUser">The type of the class representing a user.</typeparam>
    /// <typeparam name="TRole">The type of the class representing a role.</typeparam>
    /// <typeparam name="TContext">The type of the data context class used to access the store.</typeparam>
    [PublicAPI]
    public class UserStore<TUser, TRole, TContext> : UserStore<TUser, TRole, TContext, string>
		where TUser : MongoIdentityUser<string>
		where TRole : MongoIdentityRole<string>
		where TContext : MongoDbContext
    {
		/// <summary>
		///     Initializes a new instance of the <see cref="UserStore{TUser, TRole, TContext}" /> type.
		/// </summary>
		/// <param name="context">The <see cref="MongoDbContext" />.</param>
		/// <param name="describer">The <see cref="IdentityErrorDescriber" />.</param>
        public UserStore(TContext context, IdentityErrorDescriber describer = null)
			: base(context, describer)
		{
		}
    }

    /// <summary>
    ///     Provides an API for a persistence store for users.
    /// </summary>
    /// <typeparam name="TUser">The type of the class representing a user.</typeparam>
	/// <typeparam name="TRole">The type of the class representing a role.</typeparam>
    /// <typeparam name="TContext">The type of the data context class used to access the store.</typeparam>
    /// <typeparam name="TKey">The type of the primary key for a user.</typeparam>
    [PublicAPI]
	public class UserStore<TUser, TRole, TContext, TKey> : UserStoreBase<TUser, TRole, TKey, IdentityUserClaim<TKey>, IdentityUserRole<TKey>, IdentityUserLogin<TKey>, IdentityUserToken<TKey>, IdentityRoleClaim<TKey>>
		where TUser : MongoIdentityUser<TKey>
		where TRole : MongoIdentityRole<TKey>
		where TContext : MongoDbContext
		where TKey : IEquatable<TKey>
    {
		private readonly TContext context;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UserStore{TUser, TRole, TContext, TKey}" /> type.
        /// </summary>
        /// <param name="context">The <see cref="MongoDbContext" />.</param>
        /// <param name="describer">The <see cref="IdentityErrorDescriber" />.</param>
        public UserStore(TContext context, IdentityErrorDescriber describer = null)
			: base(describer ?? new IdentityErrorDescriber())
		{
			ArgumentNullException.ThrowIfNull(context);

			this.context = context;
        }

		/// <inheritdoc />
		public override IQueryable<TUser> Users => this.UsersCollection.AsQueryable();

        /// <summary>
        ///		Returns an <see cref="T:System.Linq.IQueryable`1" /> collection of roles.
        /// </summary>
        /// <value>An <see cref="T:System.Linq.IQueryable`1" /> collection of roles.</value>
        public virtual IQueryable<TRole> Roles => this.RolesCollection.AsQueryable();

        /// <summary>
        ///		The collection of users in the database.
        /// </summary>
        public virtual IMongoCollection<TUser> UsersCollection => this.context.GetCollection<TUser>();

		/// <summary>
		///		The collection of users in the database.
		/// </summary>
		public virtual IMongoCollection<TRole> RolesCollection => this.context.GetCollection<TRole>();

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
        public override Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            IList<Claim> claims = user.Claims.Select(x => x.ToClaim()).ToList();
            return Task.FromResult(claims);
        }

        /// <inheritdoc />
        public override async Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);
            ArgumentNullException.ThrowIfNull(claims);

            bool addedClaims = claims.Aggregate(false, (current, claim) => current | user.AddClaim(claim));
            if (addedClaims)
            {
                Expression<Func<TUser, bool>> predicate = x => x.Id.Equals(user.Id);
                UpdateDefinition<TUser> updateDefinition = Builders<TUser>.Update.Set(x => x.Claims, user.Claims);

                await this.UsersCollection.UpdateOneAsync(predicate, updateDefinition, cancellationToken: cancellationToken);
            }
        }

        /// <inheritdoc />
        public override async Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);
            ArgumentNullException.ThrowIfNull(claim);
            ArgumentNullException.ThrowIfNull(newClaim);

            if (user.ReplaceClaim(claim, newClaim))
            {
                Expression<Func<TUser, bool>> predicate = x => x.Id.Equals(user.Id);
                UpdateDefinition<TUser> updateDefinition = Builders<TUser>.Update.Set(x => x.Claims, user.Claims);

                await this.UsersCollection.UpdateOneAsync(predicate, updateDefinition, cancellationToken: cancellationToken);
            }
        }

        /// <inheritdoc />
        public override async Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);
            ArgumentNullException.ThrowIfNull(claims);

            bool removedClaims = claims.Aggregate(false, (current, claim) => current | user.RemoveClaim(claim));
            if (removedClaims)
            {
                Expression<Func<TUser, bool>> predicate = x => x.Id.Equals(user.Id);
                UpdateDefinition<TUser> updateDefinition = Builders<TUser>.Update.Set(x => x.Claims, user.Claims);

                await this.UsersCollection.UpdateOneAsync(predicate, updateDefinition, cancellationToken: cancellationToken);
            }
        }

        /// <inheritdoc />
        public override async Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);
            ArgumentNullException.ThrowIfNull(login);

            if (user.AddLogin(login))
            {
                Expression<Func<TUser, bool>> predicate = x => x.Id.Equals(user.Id);
                UpdateDefinition<TUser> updateDefinition = Builders<TUser>.Update.Set(x => x.Logins, user.Logins);

                await this.UsersCollection.UpdateOneAsync(predicate, updateDefinition, cancellationToken: cancellationToken);
            }
        }

        /// <inheritdoc />
        public override async Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            MongoUserLogin mongoUserLogin = user.Logins.FirstOrDefault(x => x.LoginProvider == loginProvider && x.ProviderKey == providerKey);
            if (mongoUserLogin is not null)
            {
                if (user.RemoveLogin(mongoUserLogin.ToUserLoginInfo()))
                {
                    Expression<Func<TUser, bool>> predicate = x => x.Id.Equals(user.Id);
                    UpdateDefinition<TUser> updateDefinition = Builders<TUser>.Update.Set(x => x.Logins, user.Logins);

                    await this.UsersCollection.UpdateOneAsync(predicate, updateDefinition, cancellationToken: cancellationToken);
                }
            }
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
        public override async Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.ThrowIfDisposed();

            return await this.UsersCollection.Find(x => x.NormalizedEmail == normalizedEmail).FirstOrDefaultAsync(cancellationToken);
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
        protected override Task<IdentityUserToken<TKey>> FindTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken)
        {
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            IdentityUserToken<TKey> userToken = user.GetToken(loginProvider, name);
            return Task.FromResult(userToken);
        }

        /// <inheritdoc />
        protected override async Task AddUserTokenAsync(IdentityUserToken<TKey> token)
        {
            this.ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(token);

            TUser user = await this.UsersCollection.Find(x => x.Id.Equals(token.UserId)).FirstOrDefaultAsync();
            if (user is not null)
            {
                if (user.AddToken(token))
                {
                    Expression<Func<TUser, bool>> predicate = x => x.Id.Equals(user.Id);
                    UpdateDefinition<TUser> updateDefinition = Builders<TUser>.Update.Set(x => x.Tokens, user.Tokens);

                    await this.UsersCollection.UpdateOneAsync(predicate, updateDefinition);
                }
            }
        }

        /// <inheritdoc />
        protected override async Task RemoveUserTokenAsync(IdentityUserToken<TKey> token)
        {
            this.ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(token);

            TUser user = await this.UsersCollection.Find(x => x.Id.Equals(token.UserId)).FirstOrDefaultAsync();
            if (user is not null)
            {
                if (user.RemoveToken(token))
                {
                    Expression<Func<TUser, bool>> predicate = x => x.Id.Equals(user.Id);
                    UpdateDefinition<TUser> updateDefinition = Builders<TUser>.Update.Set(x => x.Tokens, user.Tokens);

                    await this.UsersCollection.UpdateOneAsync(predicate, updateDefinition);
                }
            }
        }

		/// <inheritdoc />
		public override async Task<bool> IsInRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
            this.ThrowIfDisposed();
			ArgumentNullException.ThrowIfNull(user);
			ArgumentException.ThrowIfNullOrEmpty(normalizedRoleName);

			TRole role = await this.RolesCollection.Find(x => x.NormalizedName == normalizedRoleName).FirstOrDefaultAsync(cancellationToken);
			return role is not null && user.Roles.Any(x => x.Equals(role.Id));
		}

		/// <inheritdoc />
		protected override async Task<TRole> FindRoleAsync(string normalizedRoleName, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			ArgumentException.ThrowIfNullOrEmpty(normalizedRoleName);

            return await this.RolesCollection.Find(x => x.NormalizedName == normalizedRoleName).FirstOrDefaultAsync(cancellationToken);
        }

		/// <inheritdoc />
		protected override async Task<IdentityUserRole<TKey>> FindUserRoleAsync(TKey userId, TKey roleId, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();

			IdentityUserRole<TKey> userRole = await this.UsersCollection
				.Find(x => x.Id.Equals(userId) && x.Roles.Any(r => r.Equals(roleId)))
				.Project(x => new IdentityUserRole<TKey>
				{
					UserId = x.Id,
					RoleId = roleId
                })
				.FirstOrDefaultAsync(cancellationToken);

			return userRole;
		}

		/// <inheritdoc />
		public override async Task<IList<TUser>> GetUsersInRoleAsync(string normalizedRoleName, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			ArgumentException.ThrowIfNullOrEmpty(normalizedRoleName);

			TRole role = await FindRoleAsync(normalizedRoleName, cancellationToken);
			if(role is not null)
			{
				return await this.UsersCollection.Find(x => x.Roles.Contains(role.Id)).ToListAsync(cancellationToken);
			}

			return new List<TUser>(0);
		}

		/// <inheritdoc />
		public override async Task AddToRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			ArgumentNullException.ThrowIfNull(user);
			ArgumentException.ThrowIfNullOrEmpty(normalizedRoleName);

			TRole role = await FindRoleAsync(normalizedRoleName, cancellationToken);
			if(role is null)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.RoleNotFound, normalizedRoleName));
            }

			if(user.AddRole(role.Id))
			{
				Expression<Func<TUser, bool>> predicate = x => x.Id.Equals(user.Id);
				UpdateDefinition<TUser> updateDefinition = Builders<TUser>.Update.Set(x => x.Roles, user.Roles);

				await this.UsersCollection.UpdateOneAsync(predicate, updateDefinition, cancellationToken: cancellationToken);
            }
        }

		/// <inheritdoc />
		public override async Task RemoveFromRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			ArgumentNullException.ThrowIfNull(user);
			ArgumentException.ThrowIfNullOrEmpty(normalizedRoleName);

			TRole role = await FindRoleAsync(normalizedRoleName, cancellationToken);
			if (role is null)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.RoleNotFound, normalizedRoleName));
			}

			if (user.RemoveRole(role.Id))
			{
				Expression<Func<TUser, bool>> predicate = x => x.Id.Equals(user.Id);
				UpdateDefinition<TUser> updateDefinition = Builders<TUser>.Update.Set(x => x.Roles, user.Roles);

				await this.UsersCollection.UpdateOneAsync(predicate, updateDefinition, cancellationToken: cancellationToken);
			}
        }

		/// <inheritdoc />
		public override async Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			ArgumentNullException.ThrowIfNull(user);

			if(user.Roles.Any())
			{
				return await this.RolesCollection
					.Find(x => user.Roles.Contains(x.Id))
					.Project(x => x.Name)
					.ToListAsync(cancellationToken);
			}

            return new List<string>(0);
        }
	}
}
