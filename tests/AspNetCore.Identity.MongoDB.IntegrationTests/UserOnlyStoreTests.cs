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
	using MadEyeMatt.MongoDB.DbContext;
	using Microsoft.AspNetCore.Identity;
	using Microsoft.Extensions.DependencyInjection;
	using NUnit.Framework;

	[TestFixture]
	public class UserOnlyStoreTests
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
				options.UseDatabase(GlobalFixture.ConnectionString, GlobalFixture.Database);
			});

			this.serviceProvider = services.BuildServiceProvider();

			await using (AsyncServiceScope serviceScope = this.serviceProvider.CreateAsyncScope())
			{
				await serviceScope.ServiceProvider.InitializeMongoDbIdentityStores();
			}
		}

        private UserOnlyStore GetUserStore()
		{
			UserOnlyStore store = new UserOnlyStore(this.context);
			store.Should().NotBeNull();

			return store;
		}

		private static MongoIdentityUser CreateUser(string userName, string email = null)
		{
			return new MongoIdentityUser(userName)
			{
				NormalizedUserName = userName.ToUpperInvariant(),
				Email = email,
				NormalizedEmail = email?.ToUpperInvariant(),
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
			UserOnlyStore store = this.GetUserStore();

			MongoIdentityUser user = CreateUser("Tester");
			IdentityResult result = await store.CreateAsync(user);
			result.Should().BeSuccess();

			user.Id.Should().NotBeNullOrWhiteSpace();
			(await context.ExistsUser(user.Id)).Should().BeTrue();
		}

		[Test]
		public async Task ShouldUpdate()
		{
			UserOnlyStore store = this.GetUserStore();

			MongoIdentityUser user = CreateUser("Tester");
			IdentityResult result = await store.CreateAsync(user);
			result.Should().BeSuccess();
			user.UserName = "Changed";
			result = await store.UpdateAsync(user);
			result.Should().BeSuccess();

			MongoIdentityUser expected = await context.GetUser(user.Id);
			expected.UserName.Should().Be("Changed");
		}

        [Test]
		public async Task ShouldDelete()
		{
			UserOnlyStore store = this.GetUserStore();

            MongoIdentityUser user = CreateUser("Tester");
			IdentityResult result = await store.CreateAsync(user);
			result.Should().BeSuccess();
			(await context.ExistsUser(user.Id)).Should().BeTrue();
			result = await store.DeleteAsync(user);
			result.Should().BeSuccess();

			(await context.ExistsUser(user.Id)).Should().BeFalse();
		}

		[Test]
		public async Task ShouldFindById()
		{
			UserOnlyStore store = this.GetUserStore();

			MongoIdentityUser user = CreateUser("Tester");
			IdentityResult result = await store.CreateAsync(user);
			result.Should().BeSuccess();

			MongoIdentityUser<string> expected = await store.FindByIdAsync(user.Id);
			expected.Should().NotBeNull();
		}

		[Test]
		public async Task ShouldFindByName()
		{
			UserOnlyStore store = this.GetUserStore();

			MongoIdentityUser user = CreateUser("Tester");
			IdentityResult result = await store.CreateAsync(user);
			result.Should().BeSuccess();

			MongoIdentityUser<string> expected = await store.FindByNameAsync(user.NormalizedUserName);
			expected.Should().NotBeNull();
		}

		[Test]
		public async Task ShouldFindByEmail()
		{
			UserOnlyStore store = this.GetUserStore();

			MongoIdentityUser user = CreateUser("Tester", "tester@example.com");
			IdentityResult result = await store.CreateAsync(user);
			result.Should().BeSuccess();

			MongoIdentityUser<string> expected = await store.FindByEmailAsync(user.NormalizedEmail);
			expected.Should().NotBeNull();
		}

		[Test]
		public async Task ShouldGetClaims()
		{
			UserOnlyStore store = this.GetUserStore();

			MongoIdentityUser user = CreateUser("Tester");
			IdentityResult result = await store.CreateAsync(user);
			result.Should().BeSuccess();

			IList<Claim> claims = await store.GetClaimsAsync(user);
			claims.Should().NotBeNull();
			claims.Should().HaveCount(1);
			claims[0].Type.Should().Be("test-claim");
			claims[0].Value.Should().Be("test-value");
		}

		[Test]
		public async Task ShouldAddClaims()
		{
			UserOnlyStore store = this.GetUserStore();

			MongoIdentityUser user = CreateUser("Tester");
			IdentityResult result = await store.CreateAsync(user);
			result.Should().BeSuccess();

			await store.AddClaimsAsync(user, new List<Claim> { new Claim("new-claim", "new-value") });
			result = await store.UpdateAsync(user);
			result.Should().BeSuccess();

			IList<Claim> claims = await store.GetClaimsAsync(user);
			claims.Should().NotBeNull();
			claims.Should().HaveCount(2);
			claims[0].Type.Should().Be("test-claim");
			claims[0].Value.Should().Be("test-value");
			claims[1].Type.Should().Be("new-claim");
			claims[1].Value.Should().Be("new-value");
		}

		[Test]
		public async Task ShouldReplaceClaim()
		{
			UserOnlyStore store = this.GetUserStore();

			MongoIdentityUser user = CreateUser("Tester");
			IdentityResult result = await store.CreateAsync(user);
			result.Should().BeSuccess();

			await store.ReplaceClaimAsync(user, user.Claims.First().ToClaim(), new Claim("new-claim", "new-value"));
			result = await store.UpdateAsync(user);
			result.Should().BeSuccess();

			IList<Claim> claims = await store.GetClaimsAsync(user);
            claims.Should().NotBeNull();
			claims.Should().HaveCount(1);
			claims[0].Type.Should().Be("new-claim");
			claims[0].Value.Should().Be("new-value");
		}

		[Test]
		public async Task ShouldRemoveClaims()
		{
			UserOnlyStore store = this.GetUserStore();

			MongoIdentityUser user = CreateUser("Tester");
            IdentityResult result = await store.CreateAsync(user);
			result.Should().BeSuccess();

			await store.RemoveClaimsAsync(user, user.Claims.Select(x => x.ToClaim()).ToList());
			result = await store.UpdateAsync(user);
			result.Should().BeSuccess();

			IList<Claim> claims = await store.GetClaimsAsync(user);
			claims.Should().NotBeNull();
			claims.Should().HaveCount(0);
		}

		[Test]
		public async Task ShouldAddLogin()
		{
			UserOnlyStore store = this.GetUserStore();

			MongoIdentityUser user = CreateUser("Tester");
			IdentityResult result = await store.CreateAsync(user);
			result.Should().BeSuccess();

			await store.AddLoginAsync(user, new UserLoginInfo("TestProvider", "Test", null));
			result = await store.UpdateAsync(user);
			result.Should().BeSuccess();

			IList<UserLoginInfo> logins = await store.GetLoginsAsync(user);
			logins.Should().NotBeNull();
			logins.Should().HaveCount(1);
			logins[0].LoginProvider.Should().Be("TestProvider");
			logins[0].ProviderKey.Should().Be("Test");
        }

		[Test]
		public async Task ShouldRemoveLogin()
		{
			UserOnlyStore store = this.GetUserStore();

			MongoIdentityUser user = CreateUser("Tester");
			user.Logins.Add(new MongoUserLogin
			{
				LoginProvider = "TestProvider", 
				ProviderKey = "Test"
			});
			IdentityResult result = await store.CreateAsync(user);
			result.Should().BeSuccess();

			IList<UserLoginInfo> logins = await store.GetLoginsAsync(user);
			logins.Should().NotBeNull();
			logins.Should().HaveCount(1);
			logins[0].LoginProvider.Should().Be("TestProvider");
			logins[0].ProviderKey.Should().Be("Test");

            await store.RemoveLoginAsync(user, "TestProvider", "Test");
			result = await store.UpdateAsync(user);
			result.Should().BeSuccess();

			logins = await store.GetLoginsAsync(user);
			logins.Should().NotBeNull();
			logins.Should().HaveCount(0);
		}


		[Test]
		public async Task ShouldGetLogins()
		{
			UserOnlyStore store = this.GetUserStore();

			MongoIdentityUser user = CreateUser("Tester");
			user.Logins.Add(new MongoUserLogin
			{
				LoginProvider = "TestProvider1",
				ProviderKey = "Test1"
			});
			user.Logins.Add(new MongoUserLogin
			{
				LoginProvider = "TestProvider2",
				ProviderKey = "Test2"
			});
            IdentityResult result = await store.CreateAsync(user);
			result.Should().BeSuccess();

			IList<UserLoginInfo> logins = await store.GetLoginsAsync(user);
			logins.Should().NotBeNull();
			logins.Should().HaveCount(2);
			logins[0].LoginProvider.Should().Be("TestProvider1");
			logins[0].ProviderKey.Should().Be("Test1");
			logins[1].LoginProvider.Should().Be("TestProvider2");
			logins[1].ProviderKey.Should().Be("Test2");
        }

		[Test]
		public async Task ShouldGetUsersForClaim()
		{
			UserOnlyStore store = this.GetUserStore();

			for(int i = 0; i < 10; i++)
			{
				MongoIdentityUser user = CreateUser($"Tester-{i}");

				if(i % 2 == 0)
				{
					user.Claims.Add(new MongoClaim
					{
						ClaimType = "special-claim",
						ClaimValue = "special-value"
					});
				}

				IdentityResult result = await store.CreateAsync(user);
				result.Should().BeSuccess();
            }

			IList<MongoIdentityUser<string>> users = await store.GetUsersForClaimAsync(new Claim("special-claim", "special-value"));
			users.Should().NotBeNull();
			users.Should().HaveCount(5);
		}
	}
}
