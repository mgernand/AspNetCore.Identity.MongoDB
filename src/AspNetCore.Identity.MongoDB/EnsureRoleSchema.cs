namespace MadEyeMatt.AspNetCore.Identity.MongoDB
{
	using System;
	using System.Collections.Generic;
	using System.Linq.Expressions;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
    using global::MongoDB.Driver;
    using JetBrains.Annotations;

	[UsedImplicitly]
	internal sealed class EnsureRoleSchema<TRole, TKey> : IEnsureSchema
		where TRole : MongoIdentityRole<TKey>
		where TKey : IEquatable<TKey>
	{
		private readonly MongoDbContext context;

		public EnsureRoleSchema(MongoDbContext context)
		{
			this.context = context;
		}

		/// <inheritdoc />
		public async Task ExecuteAsync()
		{
			bool exists = await this.context.CollectionExistsAsync<TRole>();
			if (!exists)
			{
				string collectionName = this.context.GetCollectionName<TRole>();
				await this.context.Database.CreateCollectionAsync(collectionName);

				IMongoCollection<TRole> collection = this.context.GetCollection<TRole>();

				await collection.Indexes.CreateManyAsync(new List<CreateIndexModel<TRole>>
				{
					CreateIndexModel(x => x.NormalizedName),
				});
			}
		}

		private static CreateIndexModel<TRole> CreateIndexModel(Expression<Func<TRole, object>> field)
		{
			return new CreateIndexModel<TRole>(
				Builders<TRole>.IndexKeys.Ascending(field),
				new CreateIndexOptions<TRole>
				{
					Unique = true,
					Name = $"{field.GetFieldName()}_asc",
                });
		}
	}
}
