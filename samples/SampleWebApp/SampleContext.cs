namespace SampleWebApp
{
	using JetBrains.Annotations;
	using MadEyeMatt.AspNetCore.Identity.MongoDB;
	using Microsoft.AspNetCore.Identity;
	using MongoDB.Driver;

	/// <inheritdoc />
	[PublicAPI]
	public sealed class SampleContext : MongoDbContext
    {
		/// <inheritdoc />
		public SampleContext(IMongoDatabase database) 
			: base(database)
		{
		}

		/// <inheritdoc />
		public override string GetCollectionName<TDocument>()
		{
			string collectionName = null;

			if (IsGenericBaseType(typeof(TDocument), typeof(IdentityUser<>)))
			{
				collectionName = "Users";
			}
			else if (IsGenericBaseType(typeof(TDocument), typeof(IdentityRole<>)))
			{
				collectionName = "Roles";
			}

			return collectionName ?? base.GetCollectionName<TDocument>();
		}
	}
}
