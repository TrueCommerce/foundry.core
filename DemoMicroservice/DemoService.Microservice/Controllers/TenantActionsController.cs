using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using AutoMapper;
using DemoService.Core.Models;
using DemoService.Core.Repositories;
using DemoService.Core.Services;
using Foundry.Core.Shared;
using Foundry.Core.Shared.Models;
using Foundry.Core.Shared.Services.OData.Controllers;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;

namespace DemoService.Microservice.Controllers
{
	/// <summary><see cref="TenantAction"/> operations</summary>
	[ODataController(
		"TenantActions",
		typeof(TenantAction),
		Description = "Tenant actions",
		ServiceOperationsOnly = true)]
	public class TenantActionsController
		: CoreODataController
	{
		#region Constructor
		/// <summary>Creates a new instance of the <see cref="TenantActionsController"/></summary>
		/// <param name="mapper">Injected mapper</param>
		/// <param name="unitOfWork">Injected unit of work</param>
		public TenantActionsController(IMapper mapper, IUnitOfWork unitOfWork)
			: base(ApiVersion.ApiVersions, mapper, unitOfWork)
		{
		}
		#endregion


		#region public async Import
		/// <summary>Imports data</summary>
		/// <param name="kind">Entity kind</param>
		/// <param name="overwriteExisting">Indicates whether to overwrite existing entities</param>
		/// <returns></returns>
		[ValidateModel]
		[HttpPost]
		[ODataRoute("Import")]
		[ODataCustomAction(AcceptsStream = true)]
		public async Task<IActionResult> Import([FromODataUri] string kind, [FromODataUri] bool overwriteExisting)
		{
			if (!IsUserAdministrator)
				throw new AuthenticationException();

			#region read data
			if (HttpContext.Request.Form?.Files.Count != 1)
				throw new ValidationException("No binary data passed");

			byte[] data;

			using (var memoryStream = new MemoryStream())
			{
				await HttpContext.Request.Form.Files[0].CopyToAsync(memoryStream);

				data = memoryStream.ToArray();
			}
			#endregion

			#region import data
			switch (kind)
			{
				case nameof(Order):
					await new OrdersManager(
							UnitOfWork.CreatEntityRepository<Order, Guid, IOrdersRepository>(
								ActionApiVersion,
								TenantId),
							UnitOfWork)
						.Import(data, overwriteExisting);
					break;

				default:
					throw new ValidationException("kind is not recognized");
			}
			#endregion

			return Ok();
		}
		#endregion

		#region public async Export
		/// <summary>Exports data</summary>
		/// <param name="parameters">Export parameters</param>
		/// <returns></returns>
		[ValidateModel]
		[HttpPost]
		[ODataRoute("Export")]
		[ODataCustomAction(EntityReturnType = typeof(Stream))]
		[ODataCustomActionParameter(ParameterName = "kind", ParameterType = typeof(string))]
		[ODataCustomActionParameter(ParameterName = "additionalId", ParameterType = typeof(string))]
		[ODataCustomActionParameter(ParameterName = "productId", ParameterType = typeof(string))]
		[ODataCustomActionParameter(ParameterName = "ids", ParameterType = typeof(Guid), IsCollection = true)]
		public async Task<IActionResult> Export(ODataActionParameters parameters)
		{
			if (!IsUserAdministrator)
				throw new AuthenticationException();

			#region Read parameters
			IEnumerable<Guid> ids = null;
			string productId = null;
			string additionalId = null;
			string kind = null;

			if (parameters != null)
			{
				if (parameters.ContainsKey("ids"))
					ids = (IEnumerable<Guid>)parameters["ids"];

				if (parameters.ContainsKey("productId"))
					productId = (string)parameters["productId"];

				if (parameters.ContainsKey("additionalId"))
					additionalId = (string)parameters["additionalId"];

				if (parameters.ContainsKey("kind"))
					kind = (string)parameters["kind"];
			}

			var exportParameters =
				new ExportParameters
				{
					ProductId = productId,
					Entities = ids?.ToList(),
					AdditionalId = additionalId
				};
			#endregion

			if (string.IsNullOrEmpty(kind))
				throw new ValidationException("kind is not specified");

			byte[] data;

			#region export data
			switch (kind)
			{
				case nameof(Order):
					data = await new OrdersManager(
							UnitOfWork.CreatEntityRepository<Order, Guid, IOrdersRepository>(
								ActionApiVersion,
								TenantId),
							UnitOfWork)
						.Export(exportParameters);
					break;

				default:
					throw new ValidationException("kind is not recognized");
			}
			#endregion

			return File(data ?? new byte[0], "application/octet-stream");
		}
		#endregion
	}
}