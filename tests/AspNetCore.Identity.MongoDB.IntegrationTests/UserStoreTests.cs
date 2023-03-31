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
	public class UserStoreTests
	{
        private IServiceProvider serviceProvider;
        private IdentityMongoDbContext context;

        [SetUp]
        public async Task SetUp()
        {
            IMongoClient client = this.serviceProvider.GetRequiredService<IMongoClient>();
            await client.DropDatabaseAsync(GlobalFixture.Database);

            this.context = this.serviceProvider.GetRequiredService<IdentityMongoDbContext>();
        }

		[OneTimeSetUp]
		public async Task OneTimeSetUp()
		{
			IServiceCollection services = new ServiceCollection();
			services.AddMongoDbContext<IdentityMongoDbContext>(options =>
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

		private UserStore GetUserStore()
        {
			UserStore store = new UserStore(this.context);
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
						ClaimType = "test-role-claim",
						ClaimValue = "test-role-value"
					}
				}
			};
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
			UserStore store = this.GetUserStore();

            MongoIdentityUser user = CreateUser("Tester");
            IdentityResult result = await store.CreateAsync(user);
            result.Should().BeSuccess();

            user.Id.Should().NotBeNullOrWhiteSpace();
            (await context.ExistsUser(user.Id)).Should().BeTrue();
        }

		[Test]
		public async Task ShouldUpdate()
		{
			UserStore store = this.GetUserStore();

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
			UserStore store = this.GetUserStore();

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
            UserStore store = this.GetUserStore();

            MongoIdentityUser user = CreateUser("Tester");
            IdentityResult result = await store.CreateAsync(user);
            result.Should().BeSuccess();

            MongoIdentityUser<string> expected = await store.FindByIdAsync(user.Id);
            expected.Should().NotBeNull();
        }

        [Test]
        public async Task ShouldFindByName()
        {
            UserStore store = this.GetUserStore();

            MongoIdentityUser user = CreateUser("Tester");
            IdentityResult result = await store.CreateAsync(user);
            result.Should().BeSuccess();

            MongoIdentityUser<string> expected = await store.FindByNameAsync(user.NormalizedUserName);
            expected.Should().NotBeNull();
        }

        [Test]
        public async Task ShouldFindByEmail()
        {
            UserStore store = this.GetUserStore();

            MongoIdentityUser user = CreateUser("Tester", "tester@example.com");
            IdentityResult result = await store.CreateAsync(user);
            result.Should().BeSuccess();

            MongoIdentityUser<string> expected = await store.FindByEmailAsync(user.NormalizedEmail);
            expected.Should().NotBeNull();
        }

        [Test]
        public async Task ShouldGetClaims()
        {
            UserStore store = this.GetUserStore();

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
            UserStore store = this.GetUserStore();

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
            UserStore store = this.GetUserStore();

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
            UserStore store = this.GetUserStore();

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
            UserStore store = this.GetUserStore();

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
			UserStore store = this.GetUserStore();

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
            UserStore store = this.GetUserStore();

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
			UserStore store = this.GetUserStore();

            for (int i = 0; i < 10; i++)
            {
                MongoIdentityUser user = CreateUser($"Tester-{i}");

                if (i % 2 == 0)
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

		[Test]
		public async Task ShouldCheckIfUserIsInRole()
		{
			RoleStore roleStore = this.GetRoleStore();
			UserStore userStore = this.GetUserStore();

			MongoIdentityRole role = CreateRole("Tester");
			IdentityResult result = await roleStore.CreateAsync(role);
			result.Should().BeSuccess();

			MongoIdentityUser user = CreateUser("TestUser");
			user.AddRole(role.Id);
			result = await userStore.CreateAsync(user);
			result.Should().BeSuccess();

			bool hasRole = await userStore.IsInRoleAsync(user, "NonExistingRole");
			hasRole.Should().BeFalse();

			hasRole = await userStore.IsInRoleAsync(user, "TESTER");
			hasRole.Should().BeTrue();
        }

		[Test]
		public async Task ShouldGetUsersInRole()
		{
			RoleStore roleStore = this.GetRoleStore();
			UserStore userStore = this.GetUserStore();

			MongoIdentityRole role = CreateRole("Tester");
			IdentityResult result = await roleStore.CreateAsync(role);
			result.Should().BeSuccess();

			for(int i = 0; i < 10; i++)
			{
				MongoIdentityUser user = CreateUser($"TestUser-{i}");
				if(i % 2 == 0)
				{
					user.AddRole(role.Id);
                }
				result = await userStore.CreateAsync(user);
				result.Should().BeSuccess();
            }

			IList<MongoIdentityUser<string>> users = await userStore.GetUsersInRoleAsync("TESTER");
			users.Should().NotBeNull();
			users.Should().HaveCount(5);
        }

		[Test]
		public async Task ShouldAddToRole()
		{
			RoleStore roleStore = this.GetRoleStore();
			UserStore userStore = this.GetUserStore();

			MongoIdentityRole role = CreateRole("Tester");
			IdentityResult result = await roleStore.CreateAsync(role);
			result.Should().BeSuccess();

			MongoIdentityUser user = CreateUser("TestUser");
			result = await userStore.CreateAsync(user);
			result.Should().BeSuccess();

			bool hasRole = await userStore.IsInRoleAsync(user, "TESTER");
			hasRole.Should().BeFalse();

			await userStore.AddToRoleAsync(user, "TESTER");
			result = await userStore.UpdateAsync(user);
			result.Should().BeSuccess();

			hasRole = await userStore.IsInRoleAsync(user, "TESTER");
			hasRole.Should().BeTrue();
        }

		[Test]
		public async Task ShouldRemoveFromRole()
		{
			RoleStore roleStore = this.GetRoleStore();
			UserStore userStore = this.GetUserStore();

			MongoIdentityRole role = CreateRole("Tester");
			IdentityResult result = await roleStore.CreateAsync(role);
			result.Should().BeSuccess();

			MongoIdentityUser user = CreateUser("TestUser");
			user.AddRole(role.Id);
            result = await userStore.CreateAsync(user);
			result.Should().BeSuccess();

			bool hasRole = await userStore.IsInRoleAsync(user, "TESTER");
			hasRole.Should().BeTrue();

			await userStore.RemoveFromRoleAsync(user, "TESTER");
			result = await userStore.UpdateAsync(user);
			result.Should().BeSuccess();

			hasRole = await userStore.IsInRoleAsync(user, "TESTER");
			hasRole.Should().BeFalse();
		}

		[Test]
		public async Task ShouldGetRoles()
		{
			RoleStore roleStore = this.GetRoleStore();
			UserStore userStore = this.GetUserStore();

			MongoIdentityUser user = CreateUser("TestUser");

            MongoIdentityRole role = CreateRole("Tester");
			IdentityResult result = await roleStore.CreateAsync(role);
			result.Should().BeSuccess();
			user.AddRole(role.Id);

            role = CreateRole("Developer");
			result = await roleStore.CreateAsync(role);
			result.Should().BeSuccess();
			user.AddRole(role.Id);

            role = CreateRole("Manager");
			result = await roleStore.CreateAsync(role);
			result.Should().BeSuccess();

			result = await userStore.CreateAsync(user);
			result.Should().BeSuccess();

			IList<string> roles = await userStore.GetRolesAsync(user);
			roles.Should().NotBeNull();
			roles.Should().HaveCount(2);
			roles[0].Should().Be("Tester");
			roles[1].Should().Be("Developer");
        }
    }
}
