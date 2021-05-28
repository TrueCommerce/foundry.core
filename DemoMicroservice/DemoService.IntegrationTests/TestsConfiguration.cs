using System.IO;
using Microsoft.Extensions.Configuration;

namespace DemoService.IntegrationTests
{
	public static class TestsConfiguration
	{
		#region Properties
		public static string DemoServiceUri { get; }
		public static string JwtSecret { get; }
		#endregion


		#region Constructor
		static TestsConfiguration()
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json")
				.AddEnvironmentVariables();

			var configuration = builder.Build();

			DemoServiceUri = configuration["DemoServiceUri"];
			JwtSecret = configuration["JwtSecret"];
		}
		#endregion
	}
}