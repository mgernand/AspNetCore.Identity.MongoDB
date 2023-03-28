namespace MadEyeMatt.AspNetCore.Identity.MongoDB
{
	using System.Threading.Tasks;
	using JetBrains.Annotations;

	/// <summary>
	///		A contract for services that ensure a schema exist.
	/// </summary>
	[PublicAPI]
	public interface IEnsureSchema
	{
		/// <summary>
		///		Execute to ensure the schema.
		/// </summary>
		/// <returns></returns>
		Task ExecuteAsync();
	}
}
