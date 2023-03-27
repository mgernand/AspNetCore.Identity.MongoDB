// ReSharper disable once CheckNamespace
namespace FluentAssertions
{
	using FluentAssertions.Execution;
	using FluentAssertions.Primitives;
	using Microsoft.AspNetCore.Identity;

	public sealed class IdentityResultAssertions : ObjectAssertions<IdentityResult, IdentityResultAssertions>
	{
		/// <inheritdoc />
		public IdentityResultAssertions(IdentityResult subject) 
			: base(subject)
		{
		}

		[CustomAssertion]
		public AndConstraint<IdentityResultAssertions> BeSuccess(string because = "", params object[] becauseArgs)
		{
			Execute.Assertion
				.ForCondition(this.Subject.Succeeded)
				.BecauseOf(because, becauseArgs)
				.WithDefaultIdentifier(this.Identifier).FailWith("Expected {context} to be <true>{reason}, but found {0}.", new object[] { this.Subject });
			return new AndConstraint<IdentityResultAssertions>(this);
		}

		[CustomAssertion]
		public AndConstraint<IdentityResultAssertions> BeFailed(string because = "", params object[] becauseArgs)
		{
			Execute.Assertion
				.ForCondition(!this.Subject.Succeeded)
				.BecauseOf(because, becauseArgs)
				.WithDefaultIdentifier(this.Identifier).FailWith("Expected {context} to be <false>{reason}, but found {0}.", new object[] { this.Subject });
			return new AndConstraint<IdentityResultAssertions>(this);
		}

        /// <inheritdoc />
        protected override string Identifier => "IdentityResultAssertion";
	}
}
