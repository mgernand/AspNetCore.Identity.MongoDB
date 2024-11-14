namespace MadEyeMatt.AspNetCore.Identity.MongoDB.Serialization.Serializers
{
    using global::MongoDB.Bson.Serialization;
    using global::MongoDB.Bson.Serialization.Serializers;
    using Microsoft.AspNetCore.Identity;

    internal sealed class DataProtectionSerializer : SerializerBase<string>
	{
        private readonly StringSerializer serializer = StringSerializer.Instance;
        private readonly IPersonalDataProtector protector;

		public DataProtectionSerializer(IPersonalDataProtector protector)
        {
            this.protector = protector;
        }

        /// <inheritdoc />
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, string value)
        {
            string protectedString = this.protector.Protect(value);
			this.serializer.Serialize(context, args, protectedString);
        }

        /// <inheritdoc />
        public override string Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            string protectedString = this.serializer.Deserialize(context, args);
            return this.protector.Unprotect(protectedString);
        }
    }
}
