using System.Collections.Generic;

namespace DemoService.Microservice
{
	/// <summary>API versions</summary>
	public static class ApiVersion
	{
		#region Properties
		/// <summary>Current API version</summary>
		// ReSharper disable once MemberCanBePrivate.Global
		public static int CurrentApiVersion => 1;

		/// <summary>API versions</summary>
		public static Dictionary<int, string> ApiVersions => new Dictionary<int, string>
		{
			{1, "api/v1"} //,
			//{2, "api/v2"}
		};
		#endregion
	}
}