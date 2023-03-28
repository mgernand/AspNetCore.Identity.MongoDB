namespace MadEyeMatt.AspNetCore.Identity.MongoDB
{
	using System;
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

			ConventionRegistry.Register("IdentityConventionPack", pack, type =>
				IsGenericBaseType(type, typeof(IdentityUser<>)) ||
				IsGenericBaseType(type, typeof(MongoIdentityUser<>)) ||
				IsGenericBaseType(type, typeof(IdentityRole<>)) ||
				IsGenericBaseType(type, typeof(MongoIdentityRole<>)) ||
				type == typeof(MongoClaim) ||
				type == typeof(MongoUserLogin) ||
				type == typeof(MongoUserToken) ||
				type == typeof(MongoClaim));

			IEnumerable<IEnsureSchema> ensureSchemata = applicationBuilder.ApplicationServices.GetServices<IEnsureSchema>();
			foreach(IEnsureSchema ensureSchema in ensureSchemata)
			{
				await ensureSchema.ExecuteAsync();
			}
		}

		private static bool IsGenericBaseType(Type currentType, Type genericBaseType)
		{
			Type type = currentType;
			while(type != null)
			{
				Type genericType = type.IsGenericType ? type.GetGenericTypeDefinition() : null;
				if(genericType != null && genericType == genericBaseType)
				{
					return true;
				}

				type = type.BaseType;
			}

			return false;
		}
	}
}
