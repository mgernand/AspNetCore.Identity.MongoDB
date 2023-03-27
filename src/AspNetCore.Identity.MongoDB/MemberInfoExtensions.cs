namespace MadEyeMatt.AspNetCore.Identity.MongoDB
{
	using System;
	using System.Reflection;
	using JetBrains.Annotations;

    /// <summary>
    ///		Extension methods for the <see cref="MemberInfo"/> type.
    /// </summary>
    [PublicAPI]
	public static class MemberInfoExtensions
	{
		/// <summary>
		///		Gets the type of the value of the property or field the member info represents.
		/// </summary>
		/// <param name="memberInfo"></param>
		/// <returns></returns>
		/// <exception cref="NotSupportedException"></exception>
		public static Type GetMemberType(this MemberInfo memberInfo)
		{
			return memberInfo.MemberType switch
			{
				MemberTypes.Property => ((PropertyInfo)memberInfo).PropertyType,
				MemberTypes.Field => ((FieldInfo)memberInfo).FieldType,
				_ => throw new NotSupportedException($"The member type {memberInfo.GetType()} is not supported.")
			};
		}
	}
}
