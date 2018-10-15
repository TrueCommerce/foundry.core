using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Foundry.Core.Shared.Models;

namespace DemoService.Core.Models
{
	[CoreEntity("Orders")]
	public class Order : BaseTenantEntity<Guid>
	{
		[Required]
		public string CustomerName { get; set; }

		public virtual IList<OrderLine> Lines { get; set; } = new List<OrderLine>();
	}
}