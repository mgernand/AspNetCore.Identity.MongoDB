namespace MadEyeMatt.AspNetCore.Identity.MongoDB
{
	using System;
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

        /// <inheritdoc />
        public override string GetCollectionName<TDocument>()
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
		protected static bool IsGenericBaseType(Type currentType, Type genericBaseType)
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
