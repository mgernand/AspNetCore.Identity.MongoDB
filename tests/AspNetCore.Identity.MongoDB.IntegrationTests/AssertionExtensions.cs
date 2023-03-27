// ReSharper disable once CheckNamespace
namespace FluentAssertions
{
	using Microsoft.AspNetCore.Identity;

	/// <summary>
	/// Contains extension methods for custom assertions in unit tests.
	/// </summary>
	public static class AssertionExtensions
	{
		public static IdentityResultAssertions Should(this IdentityResult actualValue)
		{
			return new IdentityResultAssertions(actualValue);
		}
	}
}
