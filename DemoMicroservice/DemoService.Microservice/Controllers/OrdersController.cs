using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using DemoService.Core.Models;
using DemoService.Core.Repositories;
using Foundry.Core.Shared;
using Foundry.Core.Shared.Services.OData.Controllers;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;

namespace DemoService.Microservice.Controllers
{
	/// <summary><see cref="Order"/> operations</summary>
	[ODataController(
		"Orders",
		typeof(Order),
		Description = "Orders",
		SupportedRequests = ODataRequests.GetCollection |
			ODataRequests.GetSingle |
			ODataRequests.Post |
			ODataRequests.Put |
			ODataRequests.Patch |
			ODataRequests.Delete)]
	public class OrdersController
		: CoreTenantedModelODataController<
			Order,
			Guid,
			IOrdersRepository>
	{
		#region Constructor
		/// <summary>Creates a new instance of the <see cref="OrdersController"/></summary>
		/// <param name="mapper">Injected mapper</param>
		/// <param name="unitOfWork">Injected unit of work</param>
		public OrdersController(IMapper mapper, IUnitOfWork unitOfWork)
			: base(ApiVersion.ApiVersions, mapper, unitOfWork)
		{
		}
		#endregion


		#region PostOrderLine
		/// <summary>Posts Order line</summary>
		/// <param name="entity">Order line</param>
		[HttpPost]
		[ValidateModel]
		[ODataRoute("OrderLines")]
		public async Task<IActionResult> PostOrderLine([FromBody] OrderLine entity)
		{
			if (entity == null)
				return BadRequest("OrderLine payload is empty");

			var parentEntity = await Repository.GetById(entity.OrderId);

			if (parentEntity == null)
				return NotFound("Order is not found");

			if (!CheckPostSecurity(parentEntity))
				return new StatusCodeResult((int)HttpStatusCode.Forbidden);

			if (parentEntity.Lines == null)
				parentEntity.Lines = new List<OrderLine>();

			var existingEntity =
				parentEntity.Lines.FirstOrDefault(
					e => e.LineNumber == entity.LineNumber);

			if (existingEntity == null)
				parentEntity.Lines.Add(entity);
			else
			{
				existingEntity.ItemName = entity.ItemName;
				existingEntity.ItemQty = entity.ItemQty;
			}

			await Repository.Update(parentEntity);

			return Created(
				GetAbsoluteUriWithOutQuery(Request) +
				$"(OrderId={entity.OrderId},LineNumber={entity.LineNumber})",
				entity);
		}
		#endregion

		#region PostOrderLines
		/// <summary>Posts Order line</summary>
		/// <param name="id">Order id</param>
		/// <param name="entity">Order line</param>
		[HttpPost]
		[ValidateModel]
		[ODataRoute("Orders({id})/Lines")]
		public async Task<IActionResult> PostOrderLines(
			Guid id,
			[FromBody] OrderLine entity)
		{
			if (entity == null)
				return BadRequest("OrderLine payload is empty");

			if (entity.OrderId != id)
				return BadRequest("OrderId is not equal to passed entity id");

			var parentEntity = await Repository.GetById(entity.OrderId);

			if (parentEntity == null)
				return NotFound("Order is not found");

			if (!CheckPostSecurity(parentEntity))
				return new StatusCodeResult((int)HttpStatusCode.Forbidden);

			if (parentEntity.Lines == null)
				parentEntity.Lines = new List<OrderLine>();

			var existingEntity =
				parentEntity.Lines.FirstOrDefault(
					e => e.LineNumber == entity.LineNumber);

			if (existingEntity == null)
				parentEntity.Lines.Add(entity);
			else
			{
				existingEntity.ItemName = entity.ItemName;
				existingEntity.ItemQty = entity.ItemQty;
			}

			await Repository.Update(parentEntity);

			return Created(
				GetAbsoluteUriWithOutQuery(Request) +
				$"(OrderId={entity.OrderId},LineNumber={entity.LineNumber})",
				entity);
		}
		#endregion

		#region PatchOrderLine
		/// <summary>Patch Order line</summary>
		/// <param name="orderId">Order id</param>
		/// <param name="lineNumber">Order line number</param>
		/// <param name="patch">Order line</param>
		[HttpPatch]
		[ValidateModel]
		[ODataRoute("OrderLines(OrderId={orderId},LineNumber={lineNumber})")]
		public async Task<IActionResult> PatchOrderLine(
			Guid orderId,
			int lineNumber,
			[FromBody] Delta<OrderLine> patch)
		{
			if (patch == null)
				return BadRequest("OrderLine payload is empty");

			var parentEntity = await Repository.GetById(orderId);

			if (parentEntity == null)
				return NotFound("Order is not found");

			if (!CheckPutSecurity(parentEntity))
				return new StatusCodeResult((int)HttpStatusCode.Forbidden);

			if (parentEntity.Lines == null)
				return NotFound("OrderLine is not found");

			var existingEntity =
				parentEntity.Lines.FirstOrDefault(e => e.LineNumber == lineNumber);

			if (existingEntity == null)
				return NotFound("OrderLine is not found");

			patch.Patch(existingEntity);

			await Repository.Update(parentEntity);

			return Updated(existingEntity);
		}
		#endregion

		#region DeleteOrderLine
		/// <summary>Delete Order secret question answer</summary>
		/// <param name="orderId">Order id</param>
		/// <param name="lineNumber">Order secret question id</param>
		[HttpDelete]
		[ValidateModel]
		[ODataRoute("OrderLines(OrderId={orderId},LineNumber={lineNumber})")]
		public async Task<IActionResult> DeleteOrderLine(Guid orderId, int lineNumber)
		{
			var parentEntity = await Repository.GetById(orderId);

			if (parentEntity == null)
				return NotFound("Order is not found");

			if (!CheckPostSecurity(parentEntity))
				return new StatusCodeResult((int)HttpStatusCode.Forbidden);

			if (parentEntity.Lines == null)
				return Ok();

			var existingEntity =
				parentEntity.Lines.FirstOrDefault(e => e.LineNumber == lineNumber);

			if (existingEntity == null)
				return Ok();

			parentEntity.Lines.Remove(existingEntity);

			await Repository.Update(parentEntity);

			return Ok();
		}
		#endregion
	}
}