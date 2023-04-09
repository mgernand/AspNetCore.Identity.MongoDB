namespace AspNetCore.Identity.MongoDB.IntegrationTests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Claims;
	using System.Threading.Tasks;
	using FluentAssertions;
	using global::MongoDB.Driver;
	using MadEyeMatt.AspNetCore.Identity.MongoDB;
	using Microsoft.AspNetCore.Identity;
	using Microsoft.Extensions.DependencyInjection;
	using NUnit.Framework;

	[TestFixture]
	public class RoleStoreTests
	{
		private IServiceProvider serviceProvider;
		private MongoDbContext context;

        [SetUp]
		public async Task SetUp()
		{
			IMongoClient client = this.serviceProvider.GetRequiredService<IMongoClient>();
			await client.DropDatabaseAsync(GlobalFixture.Database);

			this.context = this.serviceProvider.GetRequiredService<MongoDbContext>();
		}

		[OneTimeSetUp]
		public async Task OneTimeSetUp()
		{
			IServiceCollection services = new ServiceCollection();
			services.AddMongoDbContext<MongoDbContext>(options =>
			{
				options.ConnectionString = GlobalFixture.ConnectionString;
				options.DatabaseName = GlobalFixture.Database;
			});

			this.serviceProvider = services.BuildServiceProvider();

			await this.serviceProvider.InitializeMongoDbStores();
		}

		private RoleStore GetRoleStore()
		{
			RoleStore store = new RoleStore(this.context);
			store.Should().NotBeNull();

			return store;
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
			RoleStore store = this.GetRoleStore();

			MongoIdentityRole role = CreateRole("Tester");
			IdentityResult result = await store.CreateAsync(role);
			result.Should().BeSuccess();

			role.Id.Should().NotBeNullOrWhiteSpace();
			(await this.context.ExistsRole(role.Id)).Should().BeTrue();
		}

		[Test]
		public async Task ShouldUpdate()
		{
			RoleStore store = this.GetRoleStore();

			MongoIdentityRole role = CreateRole("Tester");
			IdentityResult result = await store.CreateAsync(role);
			result.Should().BeSuccess();
			role.Name = "Changed";
			result = await store.UpdateAsync(role);
			result.Should().BeSuccess();

			MongoIdentityRole expected = await this.context.GetRole(role.Id);
			expected.Name.Should().Be("Changed");
		}

        [Test]
		public async Task ShouldDelete()
		{
			RoleStore store = this.GetRoleStore();

			MongoIdentityRole role = CreateRole("Tester");
			IdentityResult result = await store.CreateAsync(role);
			result.Should().BeSuccess();
			(await this.context.ExistsRole(role.Id)).Should().BeTrue();
			result = await store.DeleteAsync(role);
			result.Should().BeSuccess();

			(await this.context.ExistsRole(role.Id)).Should().BeFalse();
		}

		[Test]
		public async Task ShouldFindById()
		{
			RoleStore store = this.GetRoleStore();

			MongoIdentityRole role = CreateRole("Tester");
			IdentityResult result = await store.CreateAsync(role);
			result.Should().BeSuccess();

			MongoIdentityRole<string> expected = await store.FindByIdAsync(role.Id);
			expected.Should().NotBeNull();
		}

		[Test]
		public async Task ShouldFindByName()
		{
			RoleStore store = this.GetRoleStore();

			MongoIdentityRole role = CreateRole("Tester");
			IdentityResult result = await store.CreateAsync(role);
			result.Should().BeSuccess();

			MongoIdentityRole<string> expected = await store.FindByNameAsync(role.NormalizedName);
			expected.Should().NotBeNull();
		}

		[Test]
		public async Task ShouldGetClaims()
		{
			RoleStore store = this.GetRoleStore();

			MongoIdentityRole role = CreateRole("Tester");
			IdentityResult result = await store.CreateAsync(role);
			result.Should().BeSuccess();

			IList<Claim> claims = await store.GetClaimsAsync(role);
			claims.Should().NotBeNull();
			claims.Should().HaveCount(1);
			claims[0].Type.Should().Be("test-claim");
			claims[0].Value.Should().Be("test-value");
		}

		[Test]
		public async Task ShouldAddClaim()
		{
			RoleStore store = this.GetRoleStore();

			MongoIdentityRole role = CreateRole("Tester");
			IdentityResult result = await store.CreateAsync(role);
			result.Should().BeSuccess();

			await store.AddClaimAsync(role, new Claim("new-claim", "new-value"));
			result = await store.UpdateAsync(role);
			result.Should().BeSuccess();

			IList<Claim> claims = await store.GetClaimsAsync(role);
			claims.Should().NotBeNull();
			claims.Should().HaveCount(2);
			claims[0].Type.Should().Be("test-claim");
			claims[0].Value.Should().Be("test-value");
			claims[1].Type.Should().Be("new-claim");
			claims[1].Value.Should().Be("new-value");
		}

        [Test]
		public async Task ShouldRemoveClaim()
		{
			RoleStore store = this.GetRoleStore();

			MongoIdentityRole role = CreateRole("Tester");
			IdentityResult result = await store.CreateAsync(role);
			result.Should().BeSuccess();
			await store.RemoveClaimAsync(role, role.Claims.First().ToClaim());
			result = await store.UpdateAsync(role);
			result.Should().BeSuccess();

			IList<Claim> claims = await store.GetClaimsAsync(role);
			claims.Should().NotBeNull();
			claims.Should().HaveCount(0);
		}
	}
}
