namespace MadEyeMatt.AspNetCore.Identity.MongoDB
{
	using System.Runtime.CompilerServices;
	using System;
	using System.Diagnostics.CodeAnalysis;

	internal static class Guard
	{
		internal static void ThrowIfNull([NotNull] object argument, [CallerArgumentExpression("argument")] string parameterName = null)
		{
			if (argument != null)
			{
				return;
			}

			throw new ArgumentNullException(parameterName);
		}

		public static void ThrowIfNullOrWhiteSpace([NotNull] string argument, [CallerArgumentExpression("argument")] string parameterName = null)
		{
			if (!string.IsNullOrWhiteSpace(argument))
			{
				return;
			}

			throw new ArgumentNullException(argument, parameterName);
		}
	}
}