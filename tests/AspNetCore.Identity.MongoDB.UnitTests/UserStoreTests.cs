namespace AspNetCore.Identity.MongoDB.UnitTests
{
	using System;
	using System.Security.Claims;
	using System.Threading;
	using System.Threading.Tasks;
    using FluentAssertions;
	using global::MongoDB.Driver;
    using MadEyeMatt.AspNetCore.Identity.MongoDB;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
	public class UserStoreTests
	{
		[SetUp]
		public void SetUp()
		{
			this.clientMock = new Mock<IMongoClient>();
			this.databaseMock = new Mock<IMongoDatabase>();
			this.databaseMock.Setup(x => x.Client).Returns(this.clientMock.Object);
		}

		private Mock<IMongoDatabase> databaseMock;
		private Mock<IMongoClient> clientMock;

		private static async Task ShouldThrowObjectDisposedException(Func<Task> func)
		{
			await func.Should().ThrowExactlyAsync<ObjectDisposedException>();
		}

		private static async Task ShouldThrowOperationCanceledException(Func<Task> func)
		{
			await func.Should().ThrowExactlyAsync<OperationCanceledException>();
		}

		private static async Task ShouldThrowArgumentNullException(Func<Task> func)
		{
			await func.Should().ThrowExactlyAsync<ArgumentNullException>();
		}

		[Test]
        public async Task ShouldThrowWhenDisposed()
        {
			UserStore store = new UserStore(new MongoDbContext(this.databaseMock.Object));
            store.Should().NotBeNull();

            store.Dispose();

            await ShouldThrowObjectDisposedException(async () => await store.CreateAsync(null));
            await ShouldThrowObjectDisposedException(async () => await store.UpdateAsync(null));
            await ShouldThrowObjectDisposedException(async () => await store.DeleteAsync(null));
            await ShouldThrowObjectDisposedException(async () => await store.FindByIdAsync(null));
            await ShouldThrowObjectDisposedException(async () => await store.FindByNameAsync(null));
			await ShouldThrowObjectDisposedException(async () => await store.FindByEmailAsync(null));
			await ShouldThrowObjectDisposedException(async () => await store.GetClaimsAsync(null));
            await ShouldThrowObjectDisposedException(async () => await store.AddClaimsAsync(null, null));
            await ShouldThrowObjectDisposedException(async () => await store.ReplaceClaimAsync(null, null, null));
			await ShouldThrowObjectDisposedException(async () => await store.RemoveClaimsAsync(null, null));
			await ShouldThrowObjectDisposedException(async () => await store.AddLoginAsync(null, null));
			await ShouldThrowObjectDisposedException(async () => await store.RemoveLoginAsync(null, null, null));
			await ShouldThrowObjectDisposedException(async () => await store.GetLoginsAsync(null));
			await ShouldThrowObjectDisposedException(async () => await store.GetUsersForClaimAsync(null));
			await ShouldThrowObjectDisposedException(async () => await store.IsInRoleAsync(null, null));
			await ShouldThrowObjectDisposedException(async () => await store.GetUsersInRoleAsync(null));
			await ShouldThrowObjectDisposedException(async () => await store.AddToRoleAsync(null, null));
			await ShouldThrowObjectDisposedException(async () => await store.RemoveFromRoleAsync(null, null));
			await ShouldThrowObjectDisposedException(async () => await store.GetRolesAsync(null));
        }

        [Test]
        public async Task ShouldThrowWhenCancelled()
        {
			UserStore store = new UserStore(new MongoDbContext(this.databaseMock.Object));
            store.Should().NotBeNull();

            CancellationTokenSource cts = new CancellationTokenSource();
            cts.Cancel();

			await ShouldThrowOperationCanceledException(async () => await store.CreateAsync(null, cts.Token));
			await ShouldThrowOperationCanceledException(async () => await store.UpdateAsync(null, cts.Token));
			await ShouldThrowOperationCanceledException(async () => await store.DeleteAsync(null, cts.Token));
			await ShouldThrowOperationCanceledException(async () => await store.FindByIdAsync(null, cts.Token));
			await ShouldThrowOperationCanceledException(async () => await store.FindByNameAsync(null, cts.Token));
			await ShouldThrowOperationCanceledException(async () => await store.FindByEmailAsync(null, cts.Token));
			await ShouldThrowOperationCanceledException(async () => await store.GetClaimsAsync(null, cts.Token));
			await ShouldThrowOperationCanceledException(async () => await store.AddClaimsAsync(null, null, cts.Token));
			await ShouldThrowOperationCanceledException(async () => await store.ReplaceClaimAsync(null, null, null, cts.Token));
			await ShouldThrowOperationCanceledException(async () => await store.RemoveClaimsAsync(null, null, cts.Token));
			await ShouldThrowOperationCanceledException(async () => await store.AddLoginAsync(null, null, cts.Token));
			await ShouldThrowOperationCanceledException(async () => await store.RemoveLoginAsync(null, null, null, cts.Token));
			await ShouldThrowOperationCanceledException(async () => await store.GetLoginsAsync(null, cts.Token));
			await ShouldThrowOperationCanceledException(async () => await store.GetUsersForClaimAsync(null, cts.Token));
			await ShouldThrowOperationCanceledException(async () => await store.IsInRoleAsync(null, null, cts.Token));
			await ShouldThrowOperationCanceledException(async () => await store.GetUsersInRoleAsync(null, cts.Token));
			await ShouldThrowOperationCanceledException(async () => await store.AddToRoleAsync(null, null, cts.Token));
			await ShouldThrowOperationCanceledException(async () => await store.RemoveFromRoleAsync(null, null, cts.Token));
			await ShouldThrowOperationCanceledException(async () => await store.GetRolesAsync(null, cts.Token));
        }

        [Test]
        public async Task ShouldThrowWhenParameterIsNull()
        {
			UserStore store = new UserStore(new MongoDbContext(this.databaseMock.Object));
            store.Should().NotBeNull();

			await ShouldThrowArgumentNullException(async () => await store.CreateAsync(null));
			await ShouldThrowArgumentNullException(async () => await store.UpdateAsync(null));
			await ShouldThrowArgumentNullException(async () => await store.DeleteAsync(null));
			await ShouldThrowArgumentNullException(async () => await store.FindByIdAsync(null));
			await ShouldThrowArgumentNullException(async () => await store.FindByNameAsync(null));
			await ShouldThrowArgumentNullException(async () => await store.FindByEmailAsync(null));
			await ShouldThrowArgumentNullException(async () => await store.GetClaimsAsync(null));
			await ShouldThrowArgumentNullException(async () => await store.AddClaimsAsync(null, null));
			await ShouldThrowArgumentNullException(async () => await store.AddClaimsAsync(new MongoIdentityUser(), null));
            await ShouldThrowArgumentNullException(async () => await store.ReplaceClaimAsync(null, null, null));
			await ShouldThrowArgumentNullException(async () => await store.ReplaceClaimAsync(new MongoIdentityUser(), null, null));
			await ShouldThrowArgumentNullException(async () => await store.ReplaceClaimAsync(new MongoIdentityUser(), new Claim("", ""), null));
            await ShouldThrowArgumentNullException(async () => await store.RemoveClaimsAsync(null, null));
			await ShouldThrowArgumentNullException(async () => await store.RemoveClaimsAsync(new MongoIdentityUser(), null));
            await ShouldThrowArgumentNullException(async () => await store.AddLoginAsync(null, null));
			await ShouldThrowArgumentNullException(async () => await store.AddLoginAsync(new MongoIdentityUser(), null));
            await ShouldThrowArgumentNullException(async () => await store.RemoveLoginAsync(null, null, null));
			await ShouldThrowArgumentNullException(async () => await store.GetLoginsAsync(null));
			await ShouldThrowArgumentNullException(async () => await store.GetUsersForClaimAsync(null));
			await ShouldThrowArgumentNullException(async () => await store.IsInRoleAsync(null, null));
			await ShouldThrowArgumentNullException(async () => await store.IsInRoleAsync(new MongoIdentityUser(), null));
			await ShouldThrowArgumentNullException(async () => await store.GetUsersInRoleAsync(null));
			await ShouldThrowArgumentNullException(async () => await store.AddToRoleAsync(null, null));
			await ShouldThrowArgumentNullException(async () => await store.AddToRoleAsync(new MongoIdentityUser(), null));
			await ShouldThrowArgumentNullException(async () => await store.RemoveFromRoleAsync(null, null));
			await ShouldThrowArgumentNullException(async () => await store.RemoveFromRoleAsync(new MongoIdentityUser(), null));
			await ShouldThrowArgumentNullException(async () => await store.GetRolesAsync(null));
        }
    }
}
