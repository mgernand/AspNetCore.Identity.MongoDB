namespace MadEyeMatt.AspNetCore.Identity.MongoDB
{
	using System;
	using System.Threading.Tasks;
	using global::MongoDB.Bson;
	using global::MongoDB.Driver;
	using JetBrains.Annotations;

	/// <summary>
    ///     A database context for MongoDB.
    /// </summary>
    [PublicAPI]
	public class MongoDbContext
	{
        /// <summary>
        ///     Initializes a new instance of the <see cref="MongoDbContext" /> type.
        /// </summary>
        /// <param name="database"></param>
        public MongoDbContext(IMongoDatabase database)
		{
			this.Database = database;
			this.Client = this.Database.Client;
		}

		/// <summary>
		///     Gets the <see cref="IMongoDatabase" /> instance.
		/// </summary>
		public IMongoDatabase Database { get; }

        /// <summary>
        ///     Gets the <see cref="IMongoClient" /> instance.
        /// </summary>
		public IMongoClient Client { get; }

		/// <summary>
		///     Returns a collection for a document type.
		/// </summary>
		/// <typeparam name="TDocument"></typeparam>
		public IMongoCollection<TDocument> GetCollection<TDocument>()
		{
			return this.Database.GetCollection<TDocument>(this.GetCollectionName<TDocument>());
		}

		/// <summary>
		///		Checks of the collection for the document already exist int eh database.
		/// </summary>
		/// <typeparam name="TDocument"></typeparam>
		/// <returns></returns>
		public async Task<bool> CollectionExistsAsync<TDocument>()
		{
			string collectionName = this.GetCollectionName<TDocument>();
			BsonDocument filter = new BsonDocument("name", collectionName);
			IAsyncCursor<BsonDocument> collections = await this.Database.ListCollectionsAsync(new ListCollectionsOptions
			{
				Filter = filter
			});

			return await collections.AnyAsync();
		}

        /// <summary>
        ///     Given the document type and the partition key, returns the name of the collection it belongs to.
        /// </summary>
        /// <typeparam name="TDocument">The type representing a Document.</typeparam>
        /// <returns>The name of the collection.</returns>
        public virtual string GetCollectionName<TDocument>()
		{
			return typeof(TDocument).Name;
        }

		/// <summary>
		///		Checks if the current type has the given base type.
		/// </summary>
		protected static bool IsGenericBaseType(Type currentType, Type genericBaseType)
		{
			if (currentType == genericBaseType)
			{
				return true;
			}

			Type type = currentType;
			while (type != null)
			{
				Type genericType = type.IsGenericType ? type.GetGenericTypeDefinition() : null;
				if (genericType != null && genericType == genericBaseType)
				{
					return true;
				}

				type = type.BaseType;
			}

			return false;
		}
	}
}
