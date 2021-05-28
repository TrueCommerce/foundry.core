using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundry.Shared;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DemoService.Microservice
{
	/// <summary>Health check</summary>
	public class HealthCheck : IHealthCheck
	{
		#region Properties
		private readonly IConfiguration _configuration;
		#endregion


		#region Constructor
		/// <summary></summary>
		public HealthCheck(IConfiguration configuration)
		{
			_configuration = configuration;
		}
		#endregion


		#region public CheckHealthAsync
		/// <inheritdoc />
		public Task<HealthCheckResult> CheckHealthAsync(
			HealthCheckContext context,
			CancellationToken cancellationToken = default(CancellationToken))
		{
			var fails = new Dictionary<string, object>();

			if (string.IsNullOrEmpty(_configuration.GetCachedValue(Core.ConfigurationKeys.SqlConnection, null)))
				fails[Core.ConfigurationKeys.SqlConnection] = "configuration is not defined";

			if (string.IsNullOrEmpty(_configuration.GetCachedValue(Foundry.Core.Security.ConfigurationKeys.RootUrl, null)))
				fails[Foundry.Core.Security.ConfigurationKeys.RootUrl] = "configuration is not defined";

			if (!EnumerableExtensions.Any(fails))
				return Task.FromResult(HealthCheckResult.Healthy());

			return Task.FromResult(
				HealthCheckResult.Unhealthy(
					string.Join(", ", fails.Select(e => e.Key + " " + e.Value)),
					null,
					fails));
		}
		#endregion
	}
}