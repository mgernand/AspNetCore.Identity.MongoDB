namespace MadEyeMatt.AspNetCore.Identity.MongoDB
{
	using System;
	using System.Linq.Expressions;
	using System.Text.RegularExpressions;
	using JetBrains.Annotations;

	/// <summary>
	///		Extension methods used when ensuring a schema.
	/// </summary>
	[PublicAPI]
    public static class EnsureSchemaExtensions
    {
		/// <summary>
		///		Gets the name of the field/property from the given expression.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="field"></param>
		/// <returns></returns>
		public static string GetFieldName<T>(this Expression<Func<T, object>> field)
		{
			MemberExpression memberExpression = (MemberExpression)field.Body;
			string fieldName = memberExpression.Member.Name;

			return Camelize(fieldName);
		}

		private static string Pascalize(string str)
		{
			return Regex.Replace(str, "(?:^|_)(.)", match => match.Groups[1].Value.ToUpper());
		}

		private static string Uncapitalize(string str)
		{
			return str.Substring(0, 1).ToLower() + str.Substring(1);
		}

		private static string Camelize(string str)
		{
			return Uncapitalize(Pascalize(str));
        }
    }
}
