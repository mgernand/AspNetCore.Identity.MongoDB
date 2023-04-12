namespace AspNetCore.Identity.MongoDB.IntegrationTests
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using FluentAssertions;
	using global::MongoDB.Driver;
	using MadEyeMatt.AspNetCore.Identity.MongoDB;
	using MadEyeMatt.MongoDB.DbContext;
	using Microsoft.AspNetCore.Identity;
	using Microsoft.Extensions.DependencyInjection;
	using NUnit.Framework;

	[TestFixture]
	public class RoleManagerTests
	{
		private IServiceProvider serviceProvider;
		private RoleManager<MongoIdentityRole> manager;

		[SetUp]
		public async Task SetUp()
		{
			IMongoClient client = this.serviceProvider.GetRequiredService<IMongoClient>();
			await client.DropDatabaseAsync(GlobalFixture.Database);

			this.manager = this.serviceProvider.GetRequiredService<RoleManager<MongoIdentityRole>>();
		}

		[OneTimeSetUp]
		public async Task OneTimeSetUp()
		{
			IServiceCollection services = new ServiceCollection();

			services.AddMongoDbContext<MongoDbContext>(options =>
			{
				options.UseDatabase(GlobalFixture.ConnectionString, GlobalFixture.Database);
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
			.AddDefaultTokenProviders()
			.AddMongoDbStores<MongoDbContext>();

            this.serviceProvider = services.BuildServiceProvider();

			await using (AsyncServiceScope serviceScope = this.serviceProvider.CreateAsyncScope())
			{
				await serviceScope.ServiceProvider.InitializeMongoDbIdentityStores();
			}
		}

		private static MongoIdentityRole CreateRole(string roleName)
		{
			return new MongoIdentityRole(roleName)
			{
				NormalizedName = roleName.ToUpperInvariant(),
				Claims = new List<MongoClaim>
				{
					new MongoClaim
					{
						ClaimType = "test-claim",
						ClaimValue = "test-value"
					}
				}
			};
		}

        [Test]
		public async Task ShouldCreate()
		{
			MongoIdentityRole role = CreateRole("Tester");
			IdentityResult result = await this.manager.CreateAsync(role);
			result.Should().BeSuccess();

			MongoIdentityRole expected = await this.manager.FindByIdAsync(role.Id);
			expected.Should().NotBeNull();
			expected.Id.Should().Be(role.Id);
		}

		[Test]
		public async Task ShouldUpdate()
		{
			MongoIdentityRole role = CreateRole("Tester");
			IdentityResult result = await this.manager.CreateAsync(role);
			result.Should().BeSuccess();

			result = await this.manager.SetRoleNameAsync(role, "Developer");
			result.Should().BeSuccess();

			result = await this.manager.UpdateAsync(role); 
			result.Should().BeSuccess();

			MongoIdentityRole expected = await this.manager.FindByIdAsync(role.Id);
			expected.Should().NotBeNull();
			expected.Id.Should().Be(role.Id);
			expected.Name.Should().Be("Developer");
        }

		[Test]
		public async Task ShouldDelete()
		{
			MongoIdentityRole role = CreateRole("Tester");
			IdentityResult result = await this.manager.CreateAsync(role);
			result.Should().BeSuccess();

			result = await this.manager.DeleteAsync(role);
			result.Should().BeSuccess();

			MongoIdentityRole expected = await this.manager.FindByIdAsync(role.Id);
			expected.Should().BeNull();
        }
    }
}
