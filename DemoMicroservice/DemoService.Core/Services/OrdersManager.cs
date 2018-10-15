using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DemoService.Core.Models;
using DemoService.Core.Repositories;
using Foundry.Core.Shared;
using Foundry.Core.Shared.Extensions;
using Foundry.Core.Shared.Models;
using Newtonsoft.Json.Linq;

namespace DemoService.Core.Services
{
	public class OrdersManager
	{
		#region Properties
		private readonly IOrdersRepository _repository;
		// ReSharper disable once NotAccessedField.Local
		private readonly IUnitOfWork _unitOfWork;
		#endregion


		#region Constructor
		public OrdersManager(IOrdersRepository repository, IUnitOfWork unitOfWork)
		{
			_repository = repository;
			_unitOfWork = unitOfWork;
		}
		#endregion


		#region public static Export
		public async Task<byte[]> Export(ExportParameters exportParameters)
		{
			var entities = await _repository.Export(exportParameters);

			if (entities.Count == 0)
				return null;

			var data = entities.ToJson(
				delegate(Order entity)
				{
					var orderLines = new JArray();

					// ReSharper disable once InvertIf
					if (entity.Lines != null)
						#region Serialize Lines
						foreach (var child in entity.Lines)
						{
							orderLines.Add(
								new JObject
								{
									new JProperty(nameof(OrderLine.LineNumber), child.LineNumber),
									new JProperty(nameof(OrderLine.ItemName), child.ItemName),
									new JProperty(nameof(OrderLine.ItemQty), child.ItemQty)
								});
						}
					#endregion

					return new JObject(
						new JProperty(nameof(Order.CustomerName), entity.CustomerName),
						new JProperty(nameof(Order.Lines), orderLines)
					);
				}).ToString();

			return Encoding.UTF8.GetBytes(data);
		}
		#endregion

		#region public static Import
		public async Task Import(byte[] data, bool overwriteExisting)
		{
			if (data == null || data.Length == 0)
				return;

			string source = Encoding.UTF8.GetString(data);

			List<Order> entities = source.ParseJson(
				delegate(int version, JToken token)
				{
					#region Parse Order
					Order entity = new Order
					{
						Id
							= Guid.NewGuid(),
						TenantId
							= _repository.TenantId,
						CustomerName
							= token[nameof(Order.CustomerName)]?.Value<string>()
					};
					#endregion

					#region parse Lines
					foreach (JToken jToken in token[nameof(Order.Lines)]?.Children() ??
						new JEnumerable<JToken>())
					{
						entity.Lines.Add(
							new OrderLine
							{
								OrderId = entity.Id,
								LineNumber = jToken[nameof(OrderLine.LineNumber)].Value<int>(),
								ItemName = jToken[nameof(OrderLine.ItemName)].Value<string>(),
								ItemQty = jToken[nameof(OrderLine.ItemQty)].Value<decimal>()
							});
					}
					#endregion

					return entity;
				});

			if (!entities.Any())
				return;

			await _repository.Import(entities, overwriteExisting);
		}
		#endregion
	}
}