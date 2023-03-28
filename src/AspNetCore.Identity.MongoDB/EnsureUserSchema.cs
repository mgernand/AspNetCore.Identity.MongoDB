namespace MadEyeMatt.AspNetCore.Identity.MongoDB
{
	using System;
	using System.Collections.Generic;
	using System.Linq.Expressions;
	using System.Threading.Tasks;
	using global::MongoDB.Driver;
	using JetBrains.Annotations;

	[UsedImplicitly]
	internal sealed class EnsureUserSchema<TUser, TKey> : IEnsureSchema
		where TUser : MongoIdentityUser<TKey>
		where TKey : IEquatable<TKey>
	{
		private readonly MongoDbContext context;

		public EnsureUserSchema(MongoDbContext context)
		{
			this.context = context;
		}

		/// <inheritdoc />
		public async Task ExecuteAsync()
		{
			bool exists = await this.context.CollectionExistsAsync<TUser>();
			if (!exists)
			{
				string collectionName = this.context.GetCollectionName<TUser>();
				await this.context.Database.CreateCollectionAsync(collectionName);

				IMongoCollection<TUser> collection = this.context.GetCollection<TUser>();

				await collection.Indexes.CreateManyAsync(new List<CreateIndexModel<TUser>>
				{
					CreateIndexModel(x => x.NormalizedUserName),
					CreateIndexModel(x => x.NormalizedEmail)
				});
			}
		}

		private static CreateIndexModel<TUser> CreateIndexModel(Expression<Func<TUser, object>> field)
		{
			return new CreateIndexModel<TUser>(
				Builders<TUser>.IndexKeys.Ascending(field),
				new CreateIndexOptions<TUser>
				{
					Unique = true
				});
		}
	}
}
