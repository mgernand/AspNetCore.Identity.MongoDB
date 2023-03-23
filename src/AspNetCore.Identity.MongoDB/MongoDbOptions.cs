namespace MadEyeMatt.AspNetCore.Identity.MongoDB
{
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
	}
}
