using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Foundry.Core.Security.Client.OData.Context;
using Foundry.Core.Shared;
using Foundry.Core.Shared.Security;
using Foundry.Core.Shared.Services.OData;
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
		private readonly Uri _coreSecurityRoot;
		#endregion


		#region Constructors
		/// <summary>Initializes a new instance of the <see cref="AuthenticationHandler"/></summary>
		/// <param name="next">Next handler</param>
		/// <param name="configuration">Application configuration</param>
		public AuthenticationHandler(RequestDelegate next, IConfiguration configuration)
		{
			_next = next;
			_coreSecurityRoot =
				new Uri(configuration.GetCachedValue(Foundry.Core.Security.ConfigurationKeys.RootUrl, string.Empty));
		}
		#endregion


		#region public async Invoke
		/// <summary>Invokes handler</summary>
		/// <param name="httpContext">HTTP context</param>
		/// <param name="unitOfWork">Unit of work</param>
		public async Task Invoke(HttpContext httpContext, IUnitOfWork unitOfWork)
		{
			httpContext.Items[ODataUtils.ContextItemsUserProperty] = null;
			httpContext.Items[ODataUtils.ContextItemsTenantIdProperty] = null;

			StringValues authorizations = httpContext.Request.Headers["Authorization"];

			string authorizationString = authorizations.FirstOrDefault();

			if (authorizationString != null &&
				authorizationString.Length > 7 &&
				authorizationString.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
			{
				try
				{
					string token = authorizationString.Substring(7);

					var user = await CoreSecurityODataContext
						.CreateWithJwt(_coreSecurityRoot, token)
						.GetAuthorizedCachedUser();

					#region try to parse token and extract some values
					var tokenHandler = new JwtSecurityTokenHandler();

					if (!(tokenHandler.ReadToken(token) is JwtSecurityToken jwtSecurityToken))
					{
						await _next(httpContext);

						return;
					}

					#region calculate tenant id (impersonated tenant id)
					var impersonatedTenantId = jwtSecurityToken.Claims
						.FirstOrDefault(e => e.Type == SystemDefaults.JwtClaimTypeImpersonatedTenantId)
						?.Value;

					var calculatedTenantId = AuthorizationUtils.CalculateTenantId(
						impersonatedTenantId,
						user.TenantId,
						user.IsAdministrator);
					#endregion

					#region try to identify session id
					var sessionIdSource = jwtSecurityToken.Claims
						.FirstOrDefault(e => e.Type == SystemDefaults.JwtClaimTypeSessionId)
						?.Value;

					var sessionId = sessionIdSource != null && Guid.TryParse(sessionIdSource, out var parsedSessionId)
						? parsedSessionId
						: (Guid?)null;
					#endregion
					#endregion

					httpContext.Items[ODataUtils.ContextItemsUserProperty] = user;
					httpContext.Items[ODataUtils.ContextItemsTenantIdProperty] = calculatedTenantId;
					httpContext.Items[ODataUtils.ContextItemsSessionIdProperty] = sessionId;

					Thread.SetData(Thread.GetNamedDataSlot(ODataUtils.ContextItemsUserProperty), user);
					Thread.SetData(Thread.GetNamedDataSlot(ODataUtils.ContextItemsTenantIdProperty), calculatedTenantId);
					Thread.SetData(Thread.GetNamedDataSlot(ODataUtils.ContextItemsSessionIdProperty), sessionId);
				}
				catch (Exception e)
				{
					throw new AuthenticationException(CoreUtils.SimpleException2String(e));
				}
			}

			await _next(httpContext);
		}
		#endregion
	}
}