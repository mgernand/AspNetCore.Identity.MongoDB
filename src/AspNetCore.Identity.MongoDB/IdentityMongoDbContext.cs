namespace MadEyeMatt.AspNetCore.Identity.MongoDB
{
	using System;
	using System.Collections.Generic;
	using System.Linq.Expressions;
	using System.Threading.Tasks;
	using global::MongoDB.Driver;
	using JetBrains.Annotations;
	using Microsoft.AspNetCore.Identity;

	/// <summary>
    ///     An identity database context for MongoDB.
    /// </summary>
    [PublicAPI]
	public class IdentityMongoDbContext : MongoDbContext
	{
		/// <inheritdoc />
		public IdentityMongoDbContext(IMongoDatabase database) 
			: base(database)
		{
		}

		/// <summary>
		///		Makes sure the collections and indexes exist in the database.
		/// </summary>
		/// <typeparam name="TUser"></typeparam>
		/// <typeparam name="TKey"></typeparam>
		/// <returns></returns>
        public async Task EnsureSchema<TUser, TKey>()
			where TUser : MongoIdentityUser<TKey>
			where TKey : IEquatable<TKey>
		{
			bool existsUsersCollection = await this.Database.CollectionExistsAsync(this.UsersCollectionName);
			if (!existsUsersCollection)
			{
				await this.Database.CreateCollectionAsync(this.UsersCollectionName);

				IMongoCollection<TUser> collection = this.GetCollection<TUser>();

				await collection.Indexes.CreateManyAsync(new List<CreateIndexModel<TUser>>
				{
					CreateUserIndexModel<TUser, TKey>(x => x.NormalizedUserName),
					CreateUserIndexModel<TUser, TKey>(x => x.NormalizedEmail)
				});
			}
        }

		///  <summary>
		/// 		Makes sure the collections and indexes exist in the database.
		///  </summary>
		///  <typeparam name="TUser"></typeparam>
		///  <typeparam name="TRole"></typeparam>
		///  <typeparam name="TKey"></typeparam>
		///  <returns></returns>	
		public async Task EnsureSchema<TUser, TRole, TKey>()
			where TUser : MongoIdentityUser<TKey>
			where TRole : MongoIdentityRole<TKey>
			where TKey : IEquatable<TKey>
		{
			await this.EnsureSchema<TUser, TKey>();

			bool existsRolesCollection = await this.Database.CollectionExistsAsync(this.RolesCollectionName);
			if (!existsRolesCollection)
			{
				await this.Database.CreateCollectionAsync(this.RolesCollectionName);

				IMongoCollection<TRole> collection = this.GetCollection<TRole>();

				await collection.Indexes.CreateManyAsync(new List<CreateIndexModel<TRole>>
				{
					CreateRoleIndexModel<TRole, TKey>(x => x.NormalizedName),
                });
            }
        }

		private CreateIndexModel<TUser> CreateUserIndexModel<TUser, TKey>(Expression<Func<TUser, object>> field)
			where TUser : MongoIdentityUser<TKey>
			where TKey : IEquatable<TKey>
		{
			return new CreateIndexModel<TUser>(
				Builders<TUser>.IndexKeys.Ascending(field), 
				new CreateIndexOptions<TUser>
				{
					Unique = true
				});
		}

		private CreateIndexModel<TRole> CreateRoleIndexModel<TRole, TKey>(Expression<Func<TRole, object>> field)
			where TRole : MongoIdentityRole<TKey>
			where TKey : IEquatable<TKey>
		{
			return new CreateIndexModel<TRole>(
				Builders<TRole>.IndexKeys.Ascending(field),
				new CreateIndexOptions<TRole>
				{
					Unique = true
				});
        }

        /// <inheritdoc />
        protected override sealed string GetCollectionName<TDocument>()
		{
			Type type = typeof(TDocument);

			string name = type.Name;

			if (IsGenericBaseType(type, typeof(IdentityUser<>)))
			{
				name = this.UsersCollectionName;
			}
			else if (IsGenericBaseType(type, typeof(IdentityRole<>)))
			{
				name = this.RolesCollectionName;
			}

			return name;
		}

		/// <summary>
		///		Gets the name for the users collection.
		/// </summary>
		protected virtual string UsersCollectionName  => "AspNetUsers";


		/// <summary>
		///		Gets the name for the roles collection.
		/// </summary>
		protected virtual string RolesCollectionName => "AspNetRoles";

		/// <summary>
		///		Checks if the current type has the given base type.
		/// </summary>
		private static bool IsGenericBaseType(Type currentType, Type genericBaseType)
		{
			if (currentType == genericBaseType)
			{
				return true;
			}

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
