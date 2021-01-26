using System;
using System.IO;
using Foundry.Core.Security.Client.Events;
using Foundry.Shared;
using Microsoft.Extensions.Configuration;
using UseCasesPlayground.Cases;

namespace UseCasesPlayground
{
	internal class Program
	{
		static void Main()
		{
			try
			{
				var builder = new ConfigurationBuilder()
					.SetBasePath(Directory.GetCurrentDirectory())
					.AddJsonFile("appsettings.json")
					.AddEnvironmentVariables();
				var configuration = builder.Build();
				var coreSecurityRootUri = new Uri(configuration["CoreSecurityRootUri"]);

				SecurityEventsBroker.SkipEvents = true;

				//SecurityRelated.LoginRefreshLogout(coreSecurityRootUri).Wait();
				//SecurityRelated.JwtWithCachedUser(coreSecurityRootUri).Wait();
				//SecurityRelated.CertificateWithCachedUser(coreSecurityRootUri).Wait();

				Extensions.TestTaskRetryHelper().Wait();
			}
			catch (Exception e)
			{
				Console.WriteLine(CoreUtils.Exception2String(e));
			}

			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine("Press ENTER to exit");
			Console.ReadLine();
		}
	}
}