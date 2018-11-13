using System;
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Accellos.Platform.Security.Authentication;
using Foundry.Core.Security.Client.OData.Models;
using Foundry.Core.Shared;
using Foundry.Core.Shared.Security;
using Foundry.Core.Shared.Services.OData;
using Foundry.Core.Shared.Services.Security;
using Foundry.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace DemoService.Microservice
{
	/// <summary>Authentication handler</summary>
	public class AuthenticationHandler
	{
		#region Properties
		private readonly RequestDelegate _next;
		private readonly IConfiguration _configuration;
		#endregion


		#region Constructors
		/// <summary>Initializes a new instance of the <see cref="AuthenticationHandler"/></summary>
		/// <param name="next">Next handler</param>
		/// <param name="configuration">Application configuration</param>
		public AuthenticationHandler(RequestDelegate next, IConfiguration configuration)
		{
			_next = next;
			_configuration = configuration;
		}
		#endregion


		#region public async Invoke
		/// <summary>Invokes handler</summary>
		/// <param name="httpContext">HTTP context</param>
		/// <param name="unitOfWork">Unit of work</param>
		public async Task Invoke(HttpContext httpContext, IUnitOfWork unitOfWork)
		{
			httpContext.Items[ODataUtils.ContextItemsUserProperty] = null;
			httpContext.Items[ODataUtils.ContextItemsTenantIdProperty] = string.Empty;
			httpContext.Items[ODataUtils.ContextItemsTenantIdProperty] = null;

			var demoRole = new Role
			{
				TenantId = string.Empty,
				Id = Guid.NewGuid(),
				IsEnabled = true,
				SystemRoleId = SystemDefaults.AdministratorRoleId,
				Description = "Administrator"
			};

			var demoUser = new User
			{
				TenantId = string.Empty,
				Id = SystemDefaults.SystemAdministratorUserId,
				DefaultRoleId = demoRole.Id,
				LogOnName = "sa",
				SystemUserId = SystemDefaults.SystemAdministratorUserId,
				IsEnabled = true
			};

			demoUser.Roles.Load(new[] {new UserRole {Role = demoRole, RoleId = demoRole.Id, UserId = demoUser.Id}});

			Guid? sessionId = null;

			AuthenticationTicket authenticationTicket = AuthorizationUtils.ExtractAuthenticationTicket(httpContext);

			if (authenticationTicket != null)
			{
				sessionId = authenticationTicket.SessionId == Guid.Empty ? null : (Guid?)authenticationTicket.SessionId;

				#region use AuthenticationTicket
				// TODO: Add support for Foundry.Core.Security microservice
				//var authorizedUser = await new UsersManager(
				//		unitOfWork.CreatEntityRepository<User, Guid, IUsersRepository>(
				//			ApiVersion.CurrentApiVersion,
				//			string.Empty),
				//		unitOfWork)
				//	.GetUserByAuthenticationTicket(authenticationTicket);

				var authorizedUser = demoUser;

				// ReSharper disable once ConditionIsAlwaysTrueOrFalse
				if (authorizedUser != null)
				{
					httpContext.Items[ODataUtils.ContextItemsUserProperty] = authorizedUser;
					httpContext.Items[ODataUtils.ContextItemsTenantIdProperty] = AuthorizationUtils.CalculateTenantId(
						authenticationTicket.ImpersonatedTenantId,
						authorizedUser.TenantId,
						authorizedUser.IsAdministrator);
					httpContext.Items[ODataUtils.ContextItemsSessionIdProperty] = sessionId;
				}
				#endregion

				await _next(httpContext);

				return;
			}

			StringValues authorizations = httpContext.Request.Headers["Authorization"];

			string authorizationString = authorizations.FirstOrDefault();

			if (authorizationString != null &&
				authorizationString.Length > 7 &&
				authorizationString.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
			{
				#region use JWT token
				// ReSharper disable once TooWideLocalVariableScope
				string userName;
				// ReSharper disable once NotAccessedVariable
				string tenantId;
				string impersonatedTenantId;

				#region validate and parse token
				try
				{
					//string token = authorizationString.Substring(7);
					//var tokenHandler = new JwtSecurityTokenHandler();

					//if (!(tokenHandler.ReadToken(token) is JwtSecurityToken))
					//{
					//	await _next(httpContext);

					//	return;
					//}

					//var symmetricKey = Convert.FromBase64String(_configuration["JwtSecret"]);

					//var validationParameters = new TokenValidationParameters()
					//{
					//	RequireExpirationTime = true,
					//	ValidateIssuer = false,
					//	ValidateAudience = false,
					//	IssuerSigningKey = new SymmetricSecurityKey(symmetricKey)
					//};

					//var principal = tokenHandler.ValidateToken(token, validationParameters, out _);

					//if (!(principal.Identity is ClaimsIdentity identity))
					//{
					//	await _next(httpContext);

					//	return;
					//}

					//var claim = identity.FindFirst(ClaimTypes.Name);
					//userName = claim?.Value;

					//if (string.IsNullOrEmpty(userName))
					//{
					//	await _next(httpContext);

					//	return;
					//}

					//claim = identity.FindFirst(ODataUtils.JwtClaimTypeTenantId);
					// ReSharper disable once RedundantAssignment
					//tenantId = claim?.Value;
					tenantId = string.Empty;

					//claim = identity.FindFirst(ODataUtils.JwtClaimTypeImpersonatedTenantId);
					//impersonatedTenantId = claim?.Value;
					impersonatedTenantId = null;

					//claim = identity.FindFirst(ODataUtils.JwtClaimTypeSessionId);
					//if (claim?.Value != null)
					//	if (Guid.TryParse(claim.Value, out var parsedSessionId))
					//		sessionId = parsedSessionId == Guid.Empty ? null : (Guid?)parsedSessionId;
				}
				catch (Exception e)
				{
					throw new AuthenticationException(CoreUtils.SimpleException2String(e));
				}
				#endregion

				// TODO: Add support for Foundry.Core.Security microservice
				//IUsersRepository usersRepository = unitOfWork.CreatEntityRepository<User, Guid, IUsersRepository>(
				//	ApiVersion.CurrentApiVersion,
				//	string.Empty);

				//var users = tenantId == null
				//	? usersRepository.CreateQuery().Where(e => e.LogOnName == userName).ToList()
				//	: usersRepository.CreateQuery().Where(e => (e.LogOnName == userName) && e.TenantId == tenantId)
				//		.ToList();

				//if (users.Count == 0)
				//	throw new AuthenticationException("User not found");

				//if (users.Count > 1)
				//	throw new AuthenticationException("Need to specify tenant, because more than one user found");

				//var user = users[0];

				var user = demoUser;

				var calculatedTenantId = AuthorizationUtils.CalculateTenantId(
					impersonatedTenantId,
					user.TenantId,
					user.IsAdministrator);

				httpContext.Items[ODataUtils.ContextItemsUserProperty] = user;
				httpContext.Items[ODataUtils.ContextItemsTenantIdProperty] = calculatedTenantId;
				httpContext.Items[ODataUtils.ContextItemsSessionIdProperty] = sessionId;
				Thread.SetData(Thread.GetNamedDataSlot(ODataUtils.ContextItemsUserProperty), user);
				Thread.SetData(Thread.GetNamedDataSlot(ODataUtils.ContextItemsTenantIdProperty), calculatedTenantId);
				Thread.SetData(Thread.GetNamedDataSlot(ODataUtils.ContextItemsSessionIdProperty), sessionId);
				#endregion
			}

			await _next(httpContext);
		}
		#endregion
	}
}