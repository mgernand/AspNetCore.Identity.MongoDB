namespace MadEyeMatt.AspNetCore.Identity.MongoDB
{
	using System.Threading.Tasks;
	using global::MongoDB.Bson;
	using global::MongoDB.Driver;
	using JetBrains.Annotations;

	/// <summary>
	///		Extension methods for the <see cref="IMongoDatabase"/> type.
	/// </summary>
	[PublicAPI]
	public static class MongoDatabaseExtensions
	{
		/// <summary>
		///		Checks if a collection exists.
		/// </summary>
		/// <param name="database"></param>
		/// <param name="collectionName"></param>
		/// <returns></returns>
		public static async Task<bool> CollectionExistsAsync(this IMongoDatabase database, string collectionName)
		{
			BsonDocument filter = new BsonDocument("name", collectionName);
			IAsyncCursor<BsonDocument> collections = await database.ListCollectionsAsync(new ListCollectionsOptions
			{
				Filter = filter
			});

			return await collections.AnyAsync();
		}
	}
}
