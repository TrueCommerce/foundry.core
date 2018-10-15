using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DemoService.Core.Models;
using Foundry.Core.Shared;
using Foundry.Core.Shared.Models;

namespace DemoService.Core.Repositories
{
	public interface IOrdersRepository : ICoreTenantEntityRepository<Order, Guid>
	{
		Task<List<Order>> Export(ExportParameters exportParameters);
		Task Import(List<Order> entities, bool overwriteExisting);
	}
}