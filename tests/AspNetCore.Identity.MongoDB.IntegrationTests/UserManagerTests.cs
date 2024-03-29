﻿namespace AspNetCore.Identity.MongoDB.IntegrationTests
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using FluentAssertions;
	using MadEyeMatt.AspNetCore.Identity.MongoDB;
	using MadEyeMatt.MongoDB.DbContext;
	using Microsoft.AspNetCore.Identity;
	using Microsoft.Extensions.DependencyInjection;
	using NUnit.Framework;

	[TestFixture]
	public class UserManagerTests
	{
		[SetUp]
		public async Task SetUp()
		{
			MongoDbContext context = this.serviceProvider.GetRequiredService<MongoDbContext>();
			await context.Client.DropDatabaseAsync(GlobalFixture.Database);

			this.manager = this.serviceProvider.GetRequiredService<UserManager<MongoIdentityUser>>();
		}

		private IServiceProvider serviceProvider;
		private UserManager<MongoIdentityUser> manager;

		[OneTimeSetUp]
		public async Task OneTimeSetUp()
		{
			IServiceCollection services = new ServiceCollection();

			services.AddDataProtection();

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

			await using(AsyncServiceScope serviceScope = this.serviceProvider.CreateAsyncScope())
			{
				await serviceScope.ServiceProvider.InitializeMongoDbIdentityStores();
			}
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
			MongoIdentityUser user = CreateUser("Tester");
			IdentityResult result = await this.manager.CreateAsync(user);
			result.Should().BeSuccess();

			MongoIdentityUser expected = await this.manager.FindByIdAsync(user.Id);
			expected.Should().NotBeNull();
			expected.Id.Should().Be(user.Id);
		}

		[Test]
		public async Task ShouldDelete()
		{
			MongoIdentityUser role = CreateUser("Tester");
			IdentityResult result = await this.manager.CreateAsync(role);
			result.Should().BeSuccess();

			result = await this.manager.DeleteAsync(role);
			result.Should().BeSuccess();

			MongoIdentityUser expected = await this.manager.FindByIdAsync(role.Id);
			expected.Should().BeNull();
		}

		[Test]
		public async Task ShouldUpdate()
		{
			MongoIdentityUser role = CreateUser("Tester");
			IdentityResult result = await this.manager.CreateAsync(role);
			result.Should().BeSuccess();

			result = await this.manager.SetUserNameAsync(role, "Developer");
			result.Should().BeSuccess();

			result = await this.manager.UpdateAsync(role);
			result.Should().BeSuccess();

			MongoIdentityUser expected = await this.manager.FindByIdAsync(role.Id);
			expected.Should().NotBeNull();
			expected.Id.Should().Be(role.Id);
			expected.UserName.Should().Be("Developer");
		}
	}
}
