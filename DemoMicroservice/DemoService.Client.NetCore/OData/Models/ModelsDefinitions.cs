using System;
using Foundry.Core.Shared.Client;
using Microsoft.OData.Client;

namespace DemoService.Client.OData.Models
{
	[EntitySet("Orders")]
	public partial class Order : IClientProxyModel<Guid>, IClientTenantedProxyModel
	{
	}
}