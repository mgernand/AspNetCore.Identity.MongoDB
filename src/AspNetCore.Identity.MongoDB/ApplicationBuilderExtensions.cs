namespace MadEyeMatt.AspNetCore.Identity.MongoDB
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.CompilerServices;
	using System.Threading.Tasks;
	using global::MongoDB.Bson.Serialization.Conventions;
	using JetBrains.Annotations;
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
		public static async Task InitializeMongoDbStores(this IApplicationBuilder applicationBuilder)
		{
			await applicationBuilder.ApplicationServices.InitializeMongoDbStores();
		}

		/// <summary>
		///     Initializes the MongoDB driver and ensures schema and indexes.
		/// </summary>
		/// <param name="serviceProvider"></param>
		/// <returns></returns>
		public static async Task InitializeMongoDbStores(this IServiceProvider serviceProvider)
		{
			StoreOptions options = serviceProvider
				.GetRequiredService<IOptions<IdentityOptions>>()
				.Value.Stores;

			ConventionPack pack = new ConventionPack
			{
				new NamedIdMemberConvention("Id"),
				new IdGeneratorConvention(),
				new CamelCaseElementNameConvention()
			};

			if (options.ProtectPersonalData)
			{
				pack.Add(new DataProtectionConvention(serviceProvider));
			}

			ConventionRegistry.Register("IdentityConventionPack", pack, _ => true);

			IEnumerable<IEnsureSchema> ensureSchemata = serviceProvider.GetServices<IEnsureSchema>();
			foreach (IEnsureSchema ensureSchema in ensureSchemata)
			{
				await ensureSchema.ExecuteAsync();
			}
        }
	}
}
