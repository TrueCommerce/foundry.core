using System;
using System.Collections.Generic;
using System.Linq;
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
					.FoundryExecuteAsync())
				.Select(e => e.Id);
			foreach (Guid id in entities2Delete)
				await CreateDemoServiceContext().FoundryDeleteAsync<Order>(id);
		}
		#endregion


		#region public CreateDemoServiceContext
		// ReSharper disable once MemberCanBeMadeStatic.Global
		public DemoServiceODataContext CreateDemoServiceContext()
		{
			return DemoServiceODataContext.CreateWithJwtSecret(
				new Uri(TestsConfiguration.DemoServiceUri),
				TestsConfiguration.JwtSecret,
				"sa",
				string.Empty);
		}
		#endregion
	}
}