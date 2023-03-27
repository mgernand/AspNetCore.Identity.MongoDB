namespace SampleWebApp
{
	using JetBrains.Annotations;
	using MadEyeMatt.AspNetCore.Identity.MongoDB;
	using MongoDB.Driver;

	/// <inheritdoc />
	[PublicAPI]
	public sealed class SampleContext : IdentityMongoDbContext
    {
		/// <inheritdoc />
		public SampleContext(IMongoDatabase database) 
			: base(database)
		{
		}

		/// <inheritdoc />
		protected override string UsersCollectionName => "Users";

		/// <inheritdoc />
		protected override string RolesCollectionName => "Roles";
	}
}
