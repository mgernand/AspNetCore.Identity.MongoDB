namespace MadEyeMatt.AspNetCore.Identity.MongoDB
{
	using System;
	using JetBrains.Annotations;

	/// <summary>
	///		The options for the MongoDB driver.
	/// </summary>
	[PublicAPI]
	public sealed class MongoDbOptions
	{
		/// <summary>
		///		The connection string.
		/// </summary>
		public string ConnectionString { get; set; }

		/// <summary>
		///		The database name.
		/// </summary>
		public string DatabaseName { get; set; }

		/// <summary>
		///		Gets or sets a function that can overrides the default collection naming.
		/// </summary>
		public Func<Type, string> CollectionNameFactory { get; set; }
	}
}
