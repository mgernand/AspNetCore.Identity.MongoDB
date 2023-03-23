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
	///     Provides an API for a persistence store for roles.
	/// </summary>
	[PublicAPI]
	public class RoleStore : RoleStore<MongoIdentityRole<string>>
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="RoleStore{TRole}" /> type.
		/// </summary>
		/// <param name="context">The <see cref="MongoDbContext" />.</param>
		/// <param name="describer">The <see cref="IdentityErrorDescriber" />.</param>
		public RoleStore(MongoDbContext context, IdentityErrorDescriber describer = null)
			: base(context, describer)
		{
		}
	}

    /// <summary>
    ///     Provides an API for a persistence store for roles.
    /// </summary>
    /// <typeparam name="TRole">The type of the class representing a role.</typeparam>
    [PublicAPI]
	public class RoleStore<TRole> : RoleStore<TRole, MongoDbContext>
		where TRole : MongoIdentityRole<string>
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="RoleStore{TRole}" /> type.
		/// </summary>
		/// <param name="context">The <see cref="MongoDbContext" />.</param>
		/// <param name="describer">The <see cref="IdentityErrorDescriber" />.</param>
		public RoleStore(MongoDbContext context, IdentityErrorDescriber describer = null)
			: base(context, describer)
		{
		}
	}

	/// <summary>
	///     Provides an API for a persistence store for roles.
	/// </summary>
	/// <typeparam name="TRole">The type of the class representing a role.</typeparam>
	/// <typeparam name="TContext">The type of the data context class used to access the store.</typeparam>
	[PublicAPI]
	public class RoleStore<TRole, TContext> : RoleStore<TRole, TContext, string>
		where TRole : MongoIdentityRole<string>
		where TContext : MongoDbContext
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="RoleStore{TRole, TContext}" /> type.
		/// </summary>
		/// <param name="context">The <see cref="MongoDbContext" />.</param>
		/// <param name="describer">The <see cref="IdentityErrorDescriber" />.</param>
		public RoleStore(TContext context, IdentityErrorDescriber describer = null)
			: base(context, describer)
		{
		}
	}

	/// <summary>
	///     Provides an API for a persistence store for roles.
	/// </summary>
	/// <typeparam name="TRole">The type of the class representing a role.</typeparam>
	/// <typeparam name="TContext">The type of the data context class used to access the store.</typeparam>
	/// <typeparam name="TKey">The type of the primary key for a role.</typeparam>
	[PublicAPI]
	public class RoleStore<TRole, TContext, TKey> : RoleStoreBase<TRole, TKey, IdentityUserRole<TKey>, IdentityRoleClaim<TKey>>
		where TRole : MongoIdentityRole<TKey>
		where TContext : MongoDbContext
		where TKey : IEquatable<TKey>
	{
		private readonly TContext context;

		/// <summary>
		///     Initializes a new instance of the <see cref="RoleStore{TRole, TContext, TKey}" /> type.
		/// </summary>
		/// <param name="context">The <see cref="MongoDbContext" />.</param>
		/// <param name="describer">The <see cref="IdentityErrorDescriber" />.</param>
		public RoleStore(TContext context, IdentityErrorDescriber describer)
			: base(describer ?? new IdentityErrorDescriber())
		{
			ArgumentNullException.ThrowIfNull(context);

			this.context = context;
		}

		/// <inheritdoc />
		public override IQueryable<TRole> Roles => this.RolesCollection.AsQueryable();

		/// <summary>
		///     The collection of roles in the database.
		/// </summary>
		public virtual IMongoCollection<TRole> RolesCollection => this.context.GetCollection<TRole>();

		/// <inheritdoc />
		public override async Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			ArgumentNullException.ThrowIfNull(role);

			role.ConcurrencyStamp = Guid.NewGuid().ToString("N");
			await this.RolesCollection.InsertOneAsync(role, new InsertOneOptions(), cancellationToken);

			return IdentityResult.Success;
		}

		/// <inheritdoc />
		public override async Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			ArgumentNullException.ThrowIfNull(role);

			string oldConcurrencyStamp = role.ConcurrencyStamp;
			role.ConcurrencyStamp = Guid.NewGuid().ToString("N");

			Expression<Func<TRole, bool>> predicate = x => x.Id.Equals(role.Id) && x.ConcurrencyStamp.Equals(oldConcurrencyStamp);
			ReplaceOneResult result = await this.RolesCollection.ReplaceOneAsync(predicate, role, cancellationToken: cancellationToken);

			return result.ModifiedCount == 0
				? IdentityResult.Failed(this.ErrorDescriber.ConcurrencyFailure())
				: IdentityResult.Success;
		}

		/// <inheritdoc />
		public override async Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			ArgumentNullException.ThrowIfNull(role);

			string oldConcurrencyStamp = role.ConcurrencyStamp;
			role.ConcurrencyStamp = Guid.NewGuid().ToString("N");

			Expression<Func<TRole, bool>> predicate = x => x.Id.Equals(role.Id) && x.ConcurrencyStamp.Equals(oldConcurrencyStamp);
			DeleteResult result = await this.RolesCollection.DeleteOneAsync(predicate, cancellationToken);

			return result.DeletedCount == 0
				? IdentityResult.Failed(this.ErrorDescriber.ConcurrencyFailure())
				: IdentityResult.Success;
		}

		/// <inheritdoc />
		public override async Task<TRole> FindByIdAsync(string id, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();

			TKey roleId = this.ConvertIdFromString(id);
			return await this.RolesCollection.Find(x => x.Id.Equals(roleId)).FirstOrDefaultAsync(cancellationToken);
		}

		/// <inheritdoc />
		public override async Task<TRole> FindByNameAsync(string normalizedName, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();

			return await this.RolesCollection.Find(x => x.NormalizedName == normalizedName).FirstOrDefaultAsync(cancellationToken);
		}

		/// <inheritdoc />
		public override Task<IList<Claim>> GetClaimsAsync(TRole role, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			ArgumentNullException.ThrowIfNull(role);

			IList<Claim> claims = role.Claims.Select(x => x.ToClaim()).ToList();
			return Task.FromResult(claims);
		}

		/// <inheritdoc />
		public override async Task AddClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			ArgumentNullException.ThrowIfNull(role);
			ArgumentNullException.ThrowIfNull(claim);

			if(role.AddClaim(claim))
			{
				Expression<Func<TRole, bool>> predicate = x => x.Id.Equals(role.Id);
				UpdateDefinition<TRole> updateDefinition = Builders<TRole>.Update.Set(x => x.Claims, role.Claims);

				await this.RolesCollection.UpdateOneAsync(predicate, updateDefinition, cancellationToken: cancellationToken);
			}
		}

		/// <inheritdoc />
		public override async Task RemoveClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ThrowIfDisposed();
			ArgumentNullException.ThrowIfNull(role);
			ArgumentNullException.ThrowIfNull(claim);

			if(role.RemoveClaim(claim))
			{
				Expression<Func<TRole, bool>> predicate = x => x.Id.Equals(role.Id);
				UpdateDefinition<TRole> updateDefinition = Builders<TRole>.Update.Set(x => x.Claims, role.Claims);

				await this.RolesCollection.UpdateOneAsync(predicate, updateDefinition, cancellationToken: cancellationToken);
			}
		}
	}
}
