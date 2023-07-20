namespace AspNetCore.Identity.MongoDB.IntegrationTests
{
	using System.Threading.Tasks;
	using MadEyeMatt.AspNetCore.Identity.MongoDB;

	internal static class MongoDbContextExtensions
	{
		public static async Task<bool> ExistsRole(this RoleStore store, string roleId)
		{
			MongoIdentityRole<string> role = await store.FindByIdAsync(roleId);
			return role is not null;
		}

		public static async Task<MongoIdentityRole> GetRole(this RoleStore store, string roleId)
		{
			MongoIdentityRole<string> role = await store.FindByIdAsync(roleId);
			return (MongoIdentityRole)role;
		}

		public static async Task<bool> ExistsUser(this UserStore store, string userId)
		{
			MongoIdentityUser<string> user = await store.FindByIdAsync(userId);
			return user is not null;
		}

		public static async Task<MongoIdentityUser> GetUser(this UserStore store, string userId)
		{
			MongoIdentityUser<string> user = await store.FindByIdAsync(userId);
			return (MongoIdentityUser)user;
		}

		public static async Task<bool> ExistsUser(this UserOnlyStore store, string userId)
		{
			MongoIdentityUser<string> user = await store.FindByIdAsync(userId);
			return user is not null;
		}

		public static async Task<MongoIdentityUser> GetUser(this UserOnlyStore store, string userId)
		{
			MongoIdentityUser<string> user = await store.FindByIdAsync(userId);
			return (MongoIdentityUser)user;
		}
	}
}
