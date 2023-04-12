namespace MadEyeMatt.AspNetCore.Identity.MongoDB.Initialization
{
	using System;
	using System.Collections.Generic;
	using System.Linq.Expressions;
	using System.Threading.Tasks;
	using global::MongoDB.Driver;
	using JetBrains.Annotations;
	using MadEyeMatt.MongoDB.DbContext;
	using MadEyeMatt.MongoDB.DbContext.Initialization;

	[UsedImplicitly]
    internal sealed class EnsureRoleSchema<TRole, TKey, TContext> : EnsureSchemaBase<TContext>
		where TRole : MongoIdentityRole<TKey>
        where TKey : IEquatable<TKey>
		where TContext : MongoDbContext
	{
        private readonly TContext context;

        public EnsureRoleSchema(TContext context) : base(context)
        {
            this.context = context;
        }

        /// <inheritdoc />
        public override async Task ExecuteAsync()
        {
            bool exists = await this.CollectionExistsAsync<TRole>();
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
