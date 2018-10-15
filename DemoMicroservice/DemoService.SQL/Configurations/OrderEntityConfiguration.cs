using DemoService.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DemoService.SQL.Configurations
{
	public class OrderEntityConfiguration : IEntityTypeConfiguration<Order>
	{
		#region public Configure
		public void Configure(EntityTypeBuilder<Order> builder)
		{
			builder.ToTable("Core_Orders");
		}
		#endregion
	}
}