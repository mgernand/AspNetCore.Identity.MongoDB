namespace MadEyeMatt.AspNetCore.Identity.MongoDB
{
	using System;
	using global::MongoDB.Bson;
	using global::MongoDB.Bson.Serialization;
	using global::MongoDB.Bson.Serialization.Conventions;
	using global::MongoDB.Bson.Serialization.IdGenerators;
	using global::MongoDB.Bson.Serialization.Serializers;
	using JetBrains.Annotations;

	/// <summary>
	///		A convention for setting the ID generator.
	/// </summary>
	[PublicAPI]
	public sealed class IdGeneratorConvention : ConventionBase, IPostProcessingConvention
	{
		/// <inheritdoc />
		public void PostProcess(BsonClassMap classMap)
		{
			BsonMemberMap idMemberMap = classMap.IdMemberMap;
			if(idMemberMap == null || idMemberMap.IdGenerator != null)
			{
				return;
			}

			Type idMemberType = Nullable.GetUnderlyingType(idMemberMap.MemberType) ?? idMemberMap.MemberType;
			if(idMemberType == typeof(string))
			{
				idMemberMap
					.SetIdGenerator(StringObjectIdGenerator.Instance)
					.SetSerializer(new StringSerializer(BsonType.ObjectId));
			}
			else if(idMemberType == typeof(Guid))
			{
				idMemberMap
					.SetIdGenerator(CombGuidGenerator.Instance)
					.SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
			}
			else
			{
				throw new InvalidOperationException("The MongoDB Identity Stores only support Guid or string as type for keys.");
			}
		}
	}
}
