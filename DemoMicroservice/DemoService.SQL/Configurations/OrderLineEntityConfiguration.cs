using DemoService.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DemoService.SQL.Configurations
{
	public class OrderLineEntityConfiguration : IEntityTypeConfiguration<OrderLine>
	{
		#region public Configure
		public void Configure(EntityTypeBuilder<OrderLine> builder)
		{
			builder.ToTable("DemoService_OrderLines");

			builder.HasKey(e => new { e.OrderId, e.LineNumber });

			builder.HasOne<Order>()
				.WithMany()
				.HasForeignKey(e => e.OrderId)
				.OnDelete(DeleteBehavior.Cascade);
		}
		#endregion
	}
}