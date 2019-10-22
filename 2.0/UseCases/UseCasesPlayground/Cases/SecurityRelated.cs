using System;
using System.Linq;
using System.Threading.Tasks;
using Foundry.Core.Security.Client.OData.Context;
using Foundry.Core.Shared.Client;
using Foundry.Core.Shared.Security;
using Foundry.Shared.Licensing;
using Newtonsoft.Json;

namespace UseCasesPlayground.Cases
{
	public static class SecurityRelated
	{
		#region public static async LoginRefreshLogout
		public static async Task LoginRefreshLogout(Uri securityServiceRoot)
		{
			Console.WriteLine("---JwtWithCachedUser ---");

			// login user
			var securityContext = CoreSecurityODataContext.Create(securityServiceRoot);
			var sessionTicket = await securityContext
				.SessionTickets
				.Expand(e => e.User)
				.Expand(e => e.User.IdentityClaims)
				.Expand(e => e.User.Roles)
				.Expand("User($expand=Roles($expand=Role))")
				.LogOn(
					LicenseUtils.PlatformApplicationId,
					string.Empty,
					"sa",
					"sa",
					SystemDefaults.AnyConnection)
				.GetValueAsync();
			Console.WriteLine("--- SessionTicket ---");
			Console.WriteLine(JsonConvert.SerializeObject(sessionTicket, Formatting.Indented));
			Console.WriteLine("------------");

			// save ticket somewhere in cache, key is sessionTicket.Id

			// refresh session
			securityContext = CoreSecurityODataContext.CreateWithJwt(securityServiceRoot, sessionTicket.GeneratedJwt);
			sessionTicket = await securityContext
				.SessionTickets
				.Expand(e => e.User)
				.Expand(e => e.User.IdentityClaims)
				.Expand(e => e.User.Roles)
				.Expand("User($expand=Roles($expand=Role))")
				.RefreshSession(sessionTicket.Id)
				.GetValueAsync();
			Console.WriteLine("--- SessionTicket ---");
			Console.WriteLine(JsonConvert.SerializeObject(sessionTicket, Formatting.Indented));
			Console.WriteLine("------------");

			// logout
			securityContext = CoreSecurityODataContext.CreateWithJwt(securityServiceRoot, sessionTicket.GeneratedJwt);
			await securityContext.LogOff(sessionTicket.Id).ExecuteAsync();
			Console.WriteLine("Logged out ...");
		}
		#endregion

		#region public static async JwtWithCachedUser
		public static async Task JwtWithCachedUser(Uri securityServiceRoot)
		{
			Console.WriteLine("---JwtWithCachedUser ---");

			// get JWT by login info
			var securityContext = CoreSecurityODataContext.Create(securityServiceRoot);
			string jwt = await securityContext.RequestJwt(string.Empty, "sa", "sa", null, 30).GetValueAsync();
			Console.WriteLine($"JWT: {jwt}");

			// save JWT somewhere (like on client and attach to header with each call)
			// need to refresh it time to time
			securityContext = CoreSecurityODataContext.CreateWithJwt(securityServiceRoot, jwt);
			jwt = await securityContext.RefreshJwtIfNeed();

			// get authorized user by JWT (could be cached, since cache is domain wide)
			securityContext = CoreSecurityODataContext.CreateWithJwt(securityServiceRoot, jwt);
			var user = await securityContext.GetAuthorizedCachedUser();
			Console.WriteLine("--- User ---");
			Console.WriteLine(JsonConvert.SerializeObject(user, Formatting.Indented));
			Console.WriteLine("------------");

			// get user by JWT (cached) ... it will take it from cache if any there
			// TODO: need to wire to RabbitMQ events
			// TODO: need to auto-refresh JWT if about to expire
			securityContext = CoreSecurityODataContext.CreateWithJwt(securityServiceRoot, jwt);
			user = await securityContext.GetAuthorizedCachedUser();
			Console.WriteLine("--- User ---");
			Console.WriteLine(JsonConvert.SerializeObject(user, Formatting.Indented));
			Console.WriteLine("------------");
		}
		#endregion

		#region public static async CertificateWithCachedUser
		public static async Task CertificateWithCachedUser(Uri securityServiceRoot)
		{
			Console.WriteLine("---CertificateWithCachedUser ---");

			// get JWT by login info
			var securityContext = CoreSecurityODataContext.Create(securityServiceRoot);
			string jwt = await securityContext.RequestJwt(string.Empty, "sa", "sa", null, 30).GetValueAsync();
			Console.WriteLine($"JWT: {jwt}");

			// get global tenant admin user
			securityContext = CoreSecurityODataContext.CreateWithJwt(securityServiceRoot, jwt);
			var user = await securityContext.Users
				.Where(e => e.TenantId == string.Empty && e.SystemUserId == SystemDefaults.TenantAdministratorUserId)
				.FoundryExecuteFirstOrDefaultAsync();

			// store cert key at the safe place on worker (3rd party service) (in docker can be passed as environment parameters to the container)
			// note: can use SystemDefaults.SystemAdministratorUserId as userId if just need to bind certificate to "sa" ... but usually it is not a good idea
			string certificateKey = await securityContext.RegisterNewAuthenticationCertificate(
				new Guid("3530ACB2-913F-4DA7-8BD1-B352FFD28746"),
				user.Id,
				"IG Worker XYZ certificate",
				null);
			//string certificateKey = await securityContext.RegisterNewAuthenticationCertificate(user.Id, "IG Worker XYZ certificate");
			Console.WriteLine($"Certificate Key: {certificateKey}");

			// on worker (3rd party service) attach generated JWT to call header: 
			// request.Headers["Authorization"] = "Bearer " + jwt
			jwt = CoreSecurityODataContext.GenerateTokenByCertificateKey(certificateKey);
			var authorizatoinHeader = "Bearer " + jwt;


			// on service that called by the worker (3rd party service)
			// get JWT from the Authorization header (remember to remove "Bearer ")
			jwt = authorizatoinHeader.Substring(7);

			// get user by certificate id and key (cached) ... it will take it from cache if any there
			user = await CoreSecurityODataContext.CreateWithJwt(securityServiceRoot, jwt).GetAuthorizedCachedUser();
			Console.WriteLine("--- User ---");
			Console.WriteLine(JsonConvert.SerializeObject(user, Formatting.Indented));
			Console.WriteLine("------------");

			// get user by certificate id and key (cached) ... it will take it from cache if any there
			user = await CoreSecurityODataContext.CreateWithJwt(securityServiceRoot, jwt).GetAuthorizedCachedUser();
			Console.WriteLine("--- User ---");
			Console.WriteLine(JsonConvert.SerializeObject(user, Formatting.Indented));
			Console.WriteLine("------------");
		}
		#endregion
	}
}