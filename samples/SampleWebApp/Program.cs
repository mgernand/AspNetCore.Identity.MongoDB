namespace SampleWebApp
{
	using System.Threading.Tasks;
	using MadEyeMatt.AspNetCore.Identity.MongoDB;
	using Microsoft.AspNetCore.Builder;
	using Microsoft.AspNetCore.Identity;
	using Microsoft.Extensions.DependencyInjection;

	public static class Program
    {
        public static async Task Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

			builder.Services.AddDataProtection();
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

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.MapRazorPages();

			await app.InitializeMongoDbStores();

            await app.RunAsync();
        }
	}
}