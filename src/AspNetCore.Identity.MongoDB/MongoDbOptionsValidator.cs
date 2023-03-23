namespace MadEyeMatt.AspNetCore.Identity.MongoDB
{
	using Microsoft.Extensions.Options;

	internal sealed class MongoDbOptionsValidator : IValidateOptions<MongoDbOptions>
	{
		/// <inheritdoc />
		public ValidateOptionsResult Validate(string name, MongoDbOptions options)
		{
			if(string.IsNullOrWhiteSpace(options.ConnectionString))
			{
				return ValidateOptionsResult.Fail("Missing MongoDB connection string.");
			}

			if(string.IsNullOrWhiteSpace(options.DatabaseName))
			{
				return ValidateOptionsResult.Fail("Missing MongoDB database name.");
			}

			return ValidateOptionsResult.Success;

		}
	}
}
