using DemoService.Client.OData.Context;
using Xunit;
using Xunit.Abstractions;

namespace DemoService.IntegrationTests
{
	[Collection("Integration Tests")]
	public class TestsBase
	{
		#region Properties
		protected DemoServiceContextFixture Fixture { get; }

		#region DemoServiceContext
		private DemoServiceODataContext _securityContext;

		protected DemoServiceODataContext DemoServiceContext
		{
			get
			{
				if (_securityContext != null)
					return _securityContext;

				_securityContext = Fixture.CreateDemoServiceContext();

				return _securityContext;
			}
		}
		#endregion

		public ITestOutputHelper Output { get; }
		#endregion


		#region Constructor
		public TestsBase(DemoServiceContextFixture fixture)
		{
			Fixture = fixture;
		}

		public TestsBase(DemoServiceContextFixture fixture, ITestOutputHelper output)
		{
			Fixture = fixture;
			Output = output;
		}
		#endregion
	}
}