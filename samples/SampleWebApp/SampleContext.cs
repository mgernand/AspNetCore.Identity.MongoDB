namespace SampleWebApp
{
	using JetBrains.Annotations;
	using MadEyeMatt.MongoDB.DbContext;
	using Microsoft.AspNetCore.Identity;
	using System;

	/// <inheritdoc />
	[PublicAPI]
	public sealed class SampleContext : MongoDbContext
    {
		public SampleContext(MongoDbContextOptions options) 
			: base(options)
		{
		}

		/// <inheritdoc />
		public override string GetCollectionName<TDocument>()
		{
			string collectionName = null;

			if (IsGenericBaseType(typeof(TDocument), typeof(IdentityUser<>)))
			{
				collectionName = "Users";
			}
			else if (IsGenericBaseType(typeof(TDocument), typeof(IdentityRole<>)))
			{
				collectionName = "Roles";
			}

			return collectionName ?? base.GetCollectionName<TDocument>();
		}

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
