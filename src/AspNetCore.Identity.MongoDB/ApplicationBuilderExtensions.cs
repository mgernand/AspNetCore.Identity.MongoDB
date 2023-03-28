namespace MadEyeMatt.AspNetCore.Identity.MongoDB
{
	using System.Collections.Generic;
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
			StoreOptions options = applicationBuilder
				.ApplicationServices
				.GetRequiredService<IOptions<IdentityOptions>>()
				.Value.Stores;

			ConventionPack pack = new ConventionPack
			{
				new NamedIdMemberConvention("Id"),
				new IdGeneratorConvention(),
				new CamelCaseElementNameConvention()
			};

			if(options.ProtectPersonalData)
			{
				pack.Add(new DataProtectionConvention(applicationBuilder.ApplicationServices));
			}
			
			ConventionRegistry.Register("IdentityConventionPack", pack, _ => true);

			IEnumerable<IEnsureSchema> ensureSchemata = applicationBuilder.ApplicationServices.GetServices<IEnsureSchema>();
			foreach(IEnsureSchema ensureSchema in ensureSchemata)
			{
				await ensureSchema.ExecuteAsync();
			}
		}
	}
}
