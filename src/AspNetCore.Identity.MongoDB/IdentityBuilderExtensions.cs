namespace MadEyeMatt.AspNetCore.Identity.MongoDB
{
	using System;
	using JetBrains.Annotations;
	using Microsoft.AspNetCore.Identity;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.DependencyInjection.Extensions;

	/// <summary>
    ///		Extensions methods for the <see cref="IdentityBuilder"/> type.
    /// </summary>
    [PublicAPI]
	public static class IdentityBuilderExtensions
	{
        /// <summary>
        ///		Adds a MongoDB implementation of identity information stores.
        /// </summary>
		/// <typeparam name="TContext">The MongoDB database context to use.</typeparam>
		/// <param name="builder">The <see cref="IdentityBuilder"/> instance this method extends.</param>
		/// <returns>The <see cref="IdentityBuilder"/> instance this method extends.</returns>
        public static IdentityBuilder AddMongoDbStores<TContext>(this IdentityBuilder builder)
			where TContext : MongoDbContext
		{
			AddStores(builder.Services, builder.UserType, builder.RoleType, typeof(TContext));
            return builder;
		}

		private static void AddStores(IServiceCollection services, Type userType, Type roleType, Type contextType)
		{
			Type identityUserType = FindGenericBaseType(userType, typeof(MongoIdentityUser<>));
			if (identityUserType == null)
			{
				throw new InvalidOperationException("The given type is not a mongo identity user type.");
			}

			Type keyType = identityUserType.GenericTypeArguments[0];

			if(roleType is not null)
			{
				Type identityRoleType = FindGenericBaseType(roleType, typeof(MongoIdentityRole<>));
				if (identityRoleType == null)
				{
					throw new InvalidOperationException("The given type is not a mongo identity role type.");
				}

				// Configure user store.
				Type userStoreType = typeof(UserStore<,,,>).MakeGenericType(userType, roleType, contextType, keyType);
				services.TryAddScoped(typeof(IUserStore<>).MakeGenericType(userType), userStoreType);

                // Configure role store.
				Type roleStoreType = typeof(RoleStore<,,>).MakeGenericType(roleType, contextType, keyType);
				services.TryAddScoped(typeof(IRoleStore<>).MakeGenericType(roleType), roleStoreType);
            }
			else
			{
                // No roles: configure user only store.
				Type userStoreType = typeof(UserOnlyStore<,,>).MakeGenericType(userType, contextType, keyType);
				services.TryAddScoped(typeof(IUserStore<>).MakeGenericType(userType), userStoreType);
            }
		}

		private static Type FindGenericBaseType(Type currentType, Type genericBaseType)
		{
			Type type = currentType;
			while (type != null)
			{
				Type genericType = type.IsGenericType ? type.GetGenericTypeDefinition() : null;
				if (genericType != null && genericType == genericBaseType)
				{
					return type;
				}

				type = type.BaseType;
			}

			return null;
		}
    }
}
