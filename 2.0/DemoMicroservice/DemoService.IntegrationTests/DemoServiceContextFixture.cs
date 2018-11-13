using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DemoService.Client.OData.Context;
using DemoService.Client.OData.Models;
using Foundry.Core.Shared.Client;

namespace DemoService.IntegrationTests
{
	public class DemoServiceContextFixture : IDisposable
	{
		#region Properties
		public const string XUnitTestPrefix = "core_xunit_test_";
		#endregion


		#region Constructor - Integration tests initialization
		public DemoServiceContextFixture()
		{
			DeleteTestsData().Wait();
		}
		#endregion

		#region Dispose - Integration tests cleanup
		public void Dispose()
		{
			DeleteTestsData().Wait();
		}
		#endregion


		#region private void DeleteTestsData
		private async Task DeleteTestsData()
		{
			IEnumerable<Guid> entities2Delete = (await CreateDemoServiceContext()
					.Orders
					// ReSharper disable once StringStartsWithIsCultureSpecific
					.Where(e => e.CustomerName.StartsWith(XUnitTestPrefix))
					.ExecuteAsync())
				.Select(e => e.Id);
			foreach (Guid id in entities2Delete)
				await CreateDemoServiceContext().DeleteAsync<Order>(id);
		}
		#endregion


		#region public CreateDemoServiceContext
		// ReSharper disable once MemberCanBeMadeStatic.Global
		public DemoServiceODataContext CreateDemoServiceContext()
		{
			string key;

			// TODO: generating fake key
			using (var hmac = new HMACSHA256())
				key = Convert.ToBase64String(hmac.Key);


			return new DemoServiceODataContext(
				new Uri(TestsConfiguration.DemoServiceUri + "/api/v1"),
				key,
				"sa",
				string.Empty);
		}
		#endregion
	}
}