using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using DemoService.Core.Models;
using DemoService.Core.Repositories;
using Foundry.Core.Shared.Models;
using Foundry.Core.Shared.SQL;
using Microsoft.EntityFrameworkCore;

namespace DemoService.SQL.Repositories
{
	public class OrdersRepository
		: CoreTenantEntityRepository<Order, Guid, DemoServiceDbContext>, IOrdersRepository
	{
		#region Properties
		public override bool InheritanceAllowed { get; } = false;

		protected override bool LockEntityToSave => true;

		protected override IsolationLevel DefaultTransactionIsolationLevel => IsolationLevel.ReadCommitted;
		#endregion


		#region Constructor
		public OrdersRepository(SQLUnitOfWork<DemoServiceDbContext> sqlUnitOfWork, string tenantId)
			: base(sqlUnitOfWork, tenantId)
		{
		}
		#endregion


		#region protected override BeforeEntitySaved
		//protected override void BeforeEntitySaved(Order dbEntity, Order entity, IDbContextTransaction transaction)
		//{
		//	base.BeforeEntitySaved(dbEntity, entity, transaction);

		//	// TODO: add code to validate before entity saved
		//}
		#endregion


		#region public async Export
		public async Task<List<Order>> Export(ExportParameters exportParameters)
		{
			if (exportParameters == null)
				throw new ArgumentNullException(nameof(exportParameters));

			List<Order> entities;

			#region Select entries to export
			if (exportParameters.Entities != null && exportParameters.Entities.Any())
				entities =
					exportParameters.Entities.Any()
						? await SQLUnitOfWork.Context.Orders
							.Where(e => (e.TenantId == TenantId || e.TenantId == string.Empty) &&
								exportParameters.Entities.Contains(e.Id))
							.OrderBy(e => e.CustomerName)
							.ToListAsync()
						: new List<Order>();
			else
				entities =
					await SQLUnitOfWork.Context.Orders
						.Where(e => e.TenantId == TenantId || e.TenantId == string.Empty)
						.OrderBy(e => e.CustomerName)
						.ToListAsync();
			#endregion

			return entities;
		}
		#endregion

		#region public async Import
		public async Task Import(List<Order> entities, bool overwriteExisting)
		{
			if (entities == null || !entities.Any())
				return;

			// ReSharper disable once LoopCanBePartlyConvertedToQuery
			foreach (var tenantGrouping in entities.GroupBy(e => e.TenantId))
			{
				foreach (var entity in tenantGrouping)
				{
					entity.Id = Guid.NewGuid();
					await Create(entity);
				}
			}
		}
		#endregion
	}
}