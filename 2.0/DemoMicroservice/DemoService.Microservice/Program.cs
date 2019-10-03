using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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
			BuildWebHost(args).Build().Run();
		}

		/// <summary>Build web host</summary>
		/// <param name="args">Arguments</param>
		/// <returns>Web host instance</returns>
		public static IWebHostBuilder BuildWebHost(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.ConfigureAppConfiguration((hostingContext, config) =>
				{
					config.AddJsonFile("appsettings.local.json", true, true); //load local settings
				})
				.ConfigureLogging((hostingContext, logging) =>
				{
					logging.ClearProviders();
					logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
					logging.AddConsole();
					logging.AddDebug();
				})
				.UseStartup<Startup>();
	}
}