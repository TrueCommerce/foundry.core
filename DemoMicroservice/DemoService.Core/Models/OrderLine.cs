using System;
using System.ComponentModel.DataAnnotations;
using Foundry.Core.Shared.Models;

namespace DemoService.Core.Models
{
	[CoreEntity("OrderLines")]
	public class OrderLine
	{
		[Key]
		[Required]
		public Guid OrderId { get; set; }

		[Key]
		[Required]
		public int LineNumber { get; set; }

		[Required]
		public string ItemName { get; set; }

		[Required]
		public decimal ItemQty { get; set; }
	}
}