using System;
using System.Threading.Tasks;
using Foundry.Core.Security.Client.OData.Context;
using Foundry.Shared.Extensions;
using Microsoft.OData.Client;

namespace UseCasesPlayground.Cases
{
	public static class Extensions
	{
		#region public static async TestTaskRetryHelper
		public static async Task TestTaskRetryHelper()
		{
			var securityContext = CoreSecurityODataContext.Create(new Uri("http://localhost:30101"));

			await securityContext.EnableDisableProduct("abc", true)
				.ExecuteAsync()
				.RetryOnExceptionAsync(
					3,
					new[] { typeof(DataServiceTransportException) },
					new[] { TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5) });

			// will retry on any exception
			// delays will be 5s,10s,15s,20s,25s,30s,30s,30s,...
			string jwt = await securityContext
				.RequestJwt(string.Empty, "sa", "sa", null, 30)
				.GetValueAsync()
				.RetryOnExceptionAsync(2);

			Console.WriteLine($"JWT: {jwt}");
		}
		#endregion
	}
}
