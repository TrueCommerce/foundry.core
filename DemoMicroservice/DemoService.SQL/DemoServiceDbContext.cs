using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using DemoService.Core.Models;
using DemoService.SQL.Configurations;
using Foundry.Core.Shared.SQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DemoService.SQL
{
	public class DemoServiceDbContext : CoreDbContext<DemoServiceDbContext>
	{
		#region Properties
		public const string SchemaTableName = "__EFMigrations_DemoService_History";

		protected override List<Assembly> ConfigurationAssemblies { get; } =
			new List<Assembly> { Assembly.GetAssembly(typeof(OrderEntityConfiguration)) };

		public DbSet<Order> Orders { get; set; }
		public DbSet<OrderLine> OrderLines { get; set; }
		#endregion


		#region Constructor
		// ReSharper disable once SuggestBaseTypeForParameter
		public DemoServiceDbContext(DbContextOptions<DemoServiceDbContext> options)
			: base(options)
		{
		}
		#endregion


		#region public CheckBasicIntegrity
#pragma warning disable 1998
		public async Task CheckBasicIntegrity(IConfiguration configuration)
#pragma warning restore 1998
		{
			// TODO: add some seeding or integrity check if need
		}
		#endregion
	}
}