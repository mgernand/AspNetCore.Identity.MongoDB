namespace SampleWebApp
{
	using MadEyeMatt.AspNetCore.Identity.MongoDB;
	using MongoDB.Driver;

	public class SampleContext : MongoDbContext
	{
		/// <inheritdoc />
		public SampleContext(IMongoDatabase database) 
			: base(database)
		{
		}
	}
}
