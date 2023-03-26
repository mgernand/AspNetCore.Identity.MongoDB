namespace MadEyeMatt.AspNetCore.Identity.MongoDB
{
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
		protected IMongoDatabase Database { get; }

		/// <summary>
		///     Gets the <see cref="IMongoClient" /> instance.
		/// </summary>
		protected IMongoClient Client { get; }

		/// <summary>
		///     Returns a collection for a document type that has an (optional) partition key.
		/// </summary>
		/// <typeparam name="TDocument"></typeparam>
		public IMongoCollection<TDocument> GetCollection<TDocument>()
		{
			return this.Database.GetCollection<TDocument>(this.GetCollectionName<TDocument>());
		}

		/// <summary>
        ///     Given the document type and the partition key, returns the name of the collection it belongs to.
        /// </summary>
        /// <typeparam name="TDocument">The type representing a Document.</typeparam>
        /// <returns>The name of the collection.</returns>
        protected virtual string GetCollectionName<TDocument>()
		{
			return typeof(TDocument).Name;
        }
	}
}
