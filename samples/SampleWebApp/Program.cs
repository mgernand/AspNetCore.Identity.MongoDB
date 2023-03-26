namespace SampleWebApp
{
	using MadEyeMatt.AspNetCore.Identity.MongoDB;
	using Microsoft.AspNetCore.Identity;
	using MongoDB.Driver;

	public static class Program
    {
        public static async Task Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

			builder.Services.AddControllers();
			builder.Services.AddRazorPages();
			builder.Services.AddAuthorization();
			builder.Services.AddHttpContextAccessor();

			builder.Services
				.AddAuthentication(IdentityConstants.ApplicationScheme)
				.AddIdentityCookies();

			builder.Services.AddMongoDbContext<SampleContext>(options =>
			{
				options.ConnectionString = "mongodb://localhost:27017";
				options.DatabaseName = "identity";
			})
			.AddIdentityCore<MongoIdentityUser>(options =>
			{
				options.Password.RequireDigit = false;
				options.Password.RequireLowercase = false;
				options.Password.RequireNonAlphanumeric = false;
				options.Password.RequireUppercase = false;
				options.Password.RequiredLength = 6;
				options.Password.RequiredUniqueChars = 0;
			})
			.AddRoles<MongoIdentityRole>()
			.AddDefaultUI()
			.AddDefaultTokenProviders()
			.AddSignInManager<SignInManager<MongoIdentityUser>>()
			.AddUserManager<AspNetUserManager<MongoIdentityUser>>()
			.AddRoleManager<AspNetRoleManager<MongoIdentityRole>>()
			.AddMongoDbStores<SampleContext>();

			WebApplication app = builder.Build();

            // Configure the HTTP request pipeline.

			IMongoDatabase database = app.Services.GetRequiredService<IMongoDatabase>();

			IAsyncCursor<string> cursor = await database.ListCollectionNamesAsync();

			while(await cursor.MoveNextAsync())
			{
				IEnumerable<string> names = cursor.Current;
				foreach (string name in names)
				{
					Console.WriteLine(name);
				}
            }

			app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.MapRazorPages();
            await app.RunAsync();
        }
    }
}