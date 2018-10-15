using Xunit;

namespace DemoService.IntegrationTests
{
	[CollectionDefinition("Integration Tests")]
	public class IntegrationTestsDefinition : ICollectionFixture<DemoServiceContextFixture>
	{
	}
}