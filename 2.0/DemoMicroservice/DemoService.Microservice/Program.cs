using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace DemoService.Microservice
{
	/// <summary>Program</summary>
	// ReSharper disable once ClassNeverInstantiated.Global
	public class Program
	{
		/// <summary>Main entry</summary>
		/// <param name="args">arguments</param>
		public static void Main(string[] args)
		{
			CreateWebHostBuilder(args).Build().Run();
		}

		/// <summary>Build web host</summary>
		/// <param name="args">Arguments</param>
		/// <returns>Web host instance</returns>
		public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.UseStartup<Startup>();
	}
}