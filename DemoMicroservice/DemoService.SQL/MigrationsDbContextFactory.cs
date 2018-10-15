using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DemoService.SQL
{
	public class MigrationsDbContextFactory : IDesignTimeDbContextFactory<DemoServiceDbContext>
	{
		private const string SQLConnection =
			"Server=localhost; Database=A1F Core 3.x; User Id=accellos; Password=accellos";

		public DemoServiceDbContext CreateDbContext(string[] args)
		{
			var builder = new DbContextOptionsBuilder<DemoServiceDbContext>();

			builder.UseSqlServer(
				SQLConnection,
				x => x.MigrationsHistoryTable(DemoServiceDbContext.SchemaTableName));

			return new DemoServiceDbContext(builder.Options);
		}
	}
}