using Foundry.Core.Shared.SQL;
using Foundry.Core.Shared.SQL.Events;
using Microsoft.Extensions.Logging;

namespace DemoService.Microservice
{
	/// <summary>Events broker</summary>
	public static class EventsBroker
	{
		#region Properties
		private static bool _isInitialized;

		private static ILoggerFactory _loggerFactory;
		#endregion


		#region public static Initialize
		/// <summary>Initializes events broker</summary>
		public static void Initialize(ILoggerFactory loggerFactory)
		{
			if (_isInitialized)
				return;

			_loggerFactory = loggerFactory;

			CoreDbEventsBroker.EntityChanged += OnEntityChanged;

			_isInitialized = true;
		}
		#endregion


		#region private static OnEntityChanged
		private static void OnEntityChanged(object sender, EntityChangedEventArgs e)
		{
			_loggerFactory.CreateLogger("Audit")
				.LogInformation("{tenantid}{@payload}", e.EntityChange.TenantId, e.EntityChange);
		}
		#endregion
	}
}