namespace MadEyeMatt.AspNetCore.Identity.MongoDB
{
	using System;
	using global::MongoDB.Bson.Serialization.Conventions;
	using global::MongoDB.Driver;
    using JetBrains.Annotations;
	using Microsoft.AspNetCore.Identity;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Options;

	/// <summary>
    ///		Extensions methods for the <see cref="IServiceCollection"/> type.
    /// </summary>
	[PublicAPI]
	public static class ServiceCollectionExtensions
	{
		///  <summary>
		/// 		Registers the given context as a service in the <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
		///  </summary>
		///  <typeparam name="TContext"></typeparam>
		///  <param name="services"></param>
		///  <param name="optionsAction"></param>
		///  <returns></returns>
		public static IServiceCollection AddMongoDbContext<TContext>(this IServiceCollection services, Action<MongoDbOptions> optionsAction = null)
			where TContext : MongoDbContext
		{
			if(optionsAction is not null)
			{
				services.Configure(optionsAction);
			}

			services.AddSingleton<IValidateOptions<MongoDbOptions>, MongoDbOptionsValidator>();

			ConventionPack pack = new ConventionPack
			{
				new NamedIdMemberConvention("Id"),
				new IdGeneratorConvention(),
				new CamelCaseElementNameConvention(),
			};

			ConventionRegistry.Register("IdentityConventionPack", pack, type =>
				IsGenericBaseType(type, typeof(IdentityUser<>)) ||
				IsGenericBaseType(type, typeof(MongoIdentityUser<>)) ||
				IsGenericBaseType(type, typeof(IdentityRole<>)) ||
				IsGenericBaseType(type, typeof(MongoIdentityRole<>)) ||
				type == typeof(MongoClaim) ||
				type == typeof(MongoUserLogin) ||
				type == typeof(MongoUserToken) ||
				type == typeof(MongoClaim));

            services.AddSingleton<IMongoClient>(serviceProvider =>
			{
				MongoDbOptions options = serviceProvider.GetRequiredService<IOptions<MongoDbOptions>>().Value;

				return new MongoClient(options.ConnectionString);
            });

			services.AddSingleton<IMongoDatabase>(serviceProvider =>
			{
				IMongoClient client = serviceProvider.GetRequiredService<IMongoClient>();
				MongoDbOptions options = serviceProvider.GetRequiredService<IOptions<MongoDbOptions>>().Value;

				return client.GetDatabase(options.DatabaseName);
			});

			services.AddSingleton<TContext>();

			return services;
		}

		private static bool IsGenericBaseType(Type currentType, Type genericBaseType)
		{
			Type type = currentType;
			while (type != null)
			{
				Type genericType = type.IsGenericType ? type.GetGenericTypeDefinition() : null;
				if (genericType != null && genericType == genericBaseType)
				{
					return true;
				}

				type = type.BaseType;
			}

			return false;
		}
    }
}
