namespace AspNetCore.Identity.MongoDB.IntegrationTests
{
	using System.Threading.Tasks;
	using global::MongoDB.Driver;
	using MadEyeMatt.AspNetCore.Identity.MongoDB;

	public static class MongoDbContextExtensions
	{
		public static async Task<bool> ExistsRole(this MongoDbContext context, string roleId)
		{
			IMongoCollection<MongoIdentityRole> collection = context.GetCollection<MongoIdentityRole>();
			MongoIdentityRole role = await collection.Find(x => x.Id.Equals(roleId)).FirstOrDefaultAsync();
			return role is not null;
		}

		public static async Task<bool> ExistsUser(this MongoDbContext context, string userId)
		{
			IMongoCollection<MongoIdentityUser> collection = context.GetCollection<MongoIdentityUser>();
			MongoIdentityUser user = await collection.Find(x => x.Id.Equals(userId)).FirstOrDefaultAsync();
			return user is not null;
		}

        public static async Task<MongoIdentityRole> GetRole(this MongoDbContext context, string roleId)
		{
			IMongoCollection<MongoIdentityRole> collection = context.GetCollection<MongoIdentityRole>();
			MongoIdentityRole role = await collection.Find(x => x.Id.Equals(roleId)).FirstOrDefaultAsync();
			return role;
		}

		public static async Task<MongoIdentityUser> GetUser(this MongoDbContext context, string userId)
		{
			IMongoCollection<MongoIdentityUser> collection = context.GetCollection<MongoIdentityUser>();
			MongoIdentityUser user = await collection.Find(x => x.Id.Equals(userId)).FirstOrDefaultAsync();
			return user;
		}
    }
}
