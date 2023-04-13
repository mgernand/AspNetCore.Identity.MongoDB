namespace MadEyeMatt.AspNetCore.Identity.MongoDB
{
	using System;
	using System.Threading.Tasks;
	using global::MongoDB.Bson.Serialization.Conventions;
	using JetBrains.Annotations;
	using MadEyeMatt.AspNetCore.Identity.MongoDB.Serialization.Conventions;
	using MadEyeMatt.MongoDB.DbContext.Initialization;
	using Microsoft.AspNetCore.Builder;
	using Microsoft.AspNetCore.Identity;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Options;

	/// <summary>
	///     Extension methods for the <see cref="IApplicationBuilder" /> type.
	/// </summary>
	[PublicAPI]
	public static class ApplicationBuilderExtensions
	{
		/// <summary>
		///     Initializes the MongoDB driver and ensures schema and indexes.
		/// </summary>
		/// <param name="applicationBuilder"></param>
		/// <returns></returns>
		public static async Task InitializeMongoDbIdentityStores(this IApplicationBuilder applicationBuilder)
		{
			await InitializeMongoDbIdentityStores(applicationBuilder.ApplicationServices);
		}

		/// <summary>
		///     Initializes the MongoDB driver and ensures schema and indexes.
		/// </summary>
		/// <param name="serviceProvider"></param>
		/// <returns></returns>
		public static async Task InitializeMongoDbIdentityStores(this IServiceProvider serviceProvider)
		{
			StoreOptions options = serviceProvider
				.GetRequiredService<IOptions<IdentityOptions>>()
				.Value.Stores;

			if(options.ProtectPersonalData)
			{
				ConventionPack pack = new ConventionPack
				{
					new DataProtectionConvention(serviceProvider)
				};
				ConventionRegistry.Register("IdentityConventionPack", pack, _ => true);
			}

			await serviceProvider.InitializeMongoDB();
		}
	}
}
