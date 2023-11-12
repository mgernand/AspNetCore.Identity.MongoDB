namespace MadEyeMatt.AspNetCore.Identity.MongoDB.Serialization.Conventions
{
    using System;
    using global::MongoDB.Bson.Serialization;
    using global::MongoDB.Bson.Serialization.Conventions;
    using JetBrains.Annotations;
    using MadEyeMatt.AspNetCore.Identity.MongoDB;
	using MadEyeMatt.AspNetCore.Identity.MongoDB.Serialization.Serializers;
	using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    ///		A convention for enabling data protection.
    /// </summary>
    [PublicAPI]
    public sealed class DataProtectionConvention : ConventionBase, IMemberMapConvention
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        ///		Initializes a new instance of the <see cref="DataProtectionConvention"/> type.
        /// </summary>
        /// <param name="serviceProvider"></param>
        public DataProtectionConvention(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public void Apply(BsonMemberMap memberMap)
        {
            bool isProtected = memberMap.MemberInfo.IsDefined(typeof(ProtectedPersonalDataAttribute), true);
            if (isProtected)
            {
                Type originalMemberType = memberMap.MemberType;
                Type memberType = Nullable.GetUnderlyingType(originalMemberType) ?? originalMemberType;
                if (memberType == typeof(string))
                {
                    memberMap.SetSerializer(new DataProtectionSerializer(this.serviceProvider.GetRequiredService<IPersonalDataProtector>()));
                }
                else
                {
                    throw new InvalidOperationException(Resources.CanOnlyProtectStrings);
                }
            }
        }
    }
}
