using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DemoService.Client.OData.Context;
using DemoService.Client.OData.Models;
using Foundry.Core.Shared.Client;
using Foundry.Shared.Extensions;
using Xunit;

namespace DemoService.IntegrationTests.Tests
{
	[Trait("Category", "Orders")]
	public class OrdersFixture : TestsBase
	{
		#region Constructor
		public OrdersFixture(DemoServiceContextFixture fixture)
			: base(fixture)
		{
		}
		#endregion


		#region internal static GenerateAndSaveTestOrder
		internal static async Task<Order> GenerateAndSaveTestOrder(DemoServiceODataContext securityContext)
		{
			string customerName = DemoServiceContextFixture.XUnitTestPrefix + Guid.NewGuid();

			var order = new Order
			{
				Id = Guid.NewGuid(),
				TenantId = string.Empty,
				CustomerName = customerName
			};

			securityContext.StartTracking(order);
			await securityContext.SaveChangesAsync();

			order = await securityContext.Orders
				.Expand(e => e.Lines)
				.Where(e => e.CustomerName == customerName)
				.FoundryExecuteFirstOrDefaultAsync();

			return order;
		}
		#endregion


		#region public async CanCRUD
		[Fact(DisplayName = "Order: can CRUD")]
		public async Task CanCRUD()
		{
			var sourceEntity = await GenerateAndSaveTestOrder(DemoServiceContext);
			var originalEntity = sourceEntity.JsonClone();

			// create entity
			DemoServiceContext.StartTracking(sourceEntity);
			await DemoServiceContext.SaveChangesAsync();
			var createdEntity = await DemoServiceContext.Orders
				.Expand(e => e.Lines)
				.Where(e => e.CustomerName == originalEntity.CustomerName)
				.FoundryExecuteFirstOrDefaultAsync();
			Assert.NotNull(createdEntity);
			Assert.Equal(originalEntity.CustomerName, createdEntity.CustomerName);

			// update entity
			DemoServiceContext.StartTracking(createdEntity);
			var newCustomerName = DemoServiceContextFixture.XUnitTestPrefix + "updated_" + createdEntity.Id;
			createdEntity.CustomerName = newCustomerName;
			await DemoServiceContext.SaveChangesAsync();
			var updatedEntity = await DemoServiceContext.Orders
				.Expand(e => e.Lines)
				.Where(e => e.CustomerName == newCustomerName)
				.FoundryExecuteFirstOrDefaultAsync();
			Assert.NotNull(updatedEntity);
			Assert.Equal(newCustomerName, updatedEntity.CustomerName);

			// delete entity
			await DemoServiceContext.FoundryDeleteAsync<Order>(updatedEntity.Id);
		}
		#endregion

		#region public async CanLinesCRUD
		[Fact(DisplayName = "Order: can CRUD Lines")]
		public async Task CanLinesCRUD()
		{
			var sourceEntity = await GenerateAndSaveTestOrder(DemoServiceContext);
			var originalEntity = sourceEntity.JsonClone();

			// create entity
			DemoServiceContext.StartTracking(sourceEntity);
			await DemoServiceContext.SaveChangesAsync();
			var createdEntity = await DemoServiceContext.Orders
				.Expand(e => e.Lines)
				.Where(e => e.CustomerName == originalEntity.CustomerName)
				.FoundryExecuteFirstOrDefaultAsync();

			// add OrderLine to Lines collection
			DemoServiceContext.StartTracking(createdEntity);
			var child1 =
				new OrderLine
				{
					OrderId = createdEntity.Id,
					LineNumber = 1,
					ItemName = "Item 1",
					ItemQty = 1
				};
			createdEntity.Lines.Add(child1);
			await DemoServiceContext.SaveChangesAsync();
			var updatedEntity = await DemoServiceContext.Orders
				.Expand(e => e.Lines)
				.Where(e => e.CustomerName == originalEntity.CustomerName)
				.FoundryExecuteFirstOrDefaultAsync();
			Assert.NotNull(updatedEntity);
			Assert.NotNull(updatedEntity.Lines);
			Assert.NotEmpty(updatedEntity.Lines);
			Assert.Contains(updatedEntity.Lines, e => e.LineNumber == 1);

			// add RoleProperty to Lines collection
			DemoServiceContext.StartTracking(updatedEntity);
			var child2 =
				new OrderLine
				{
					OrderId = createdEntity.Id,
					LineNumber = 2,
					ItemName = "Item 2",
					ItemQty = 2
				};
			createdEntity.Lines.Add(child2);
			await DemoServiceContext.SaveChangesAsync();
			updatedEntity = await DemoServiceContext.Orders
				.Expand(e => e.Lines)
				.Where(e => e.CustomerName == originalEntity.CustomerName)
				.FoundryExecuteFirstOrDefaultAsync();
			Assert.NotNull(updatedEntity);
			Assert.NotNull(updatedEntity.Lines);
			Assert.NotEmpty(updatedEntity.Lines);
			Assert.Contains(updatedEntity.Lines, e => e.LineNumber == 1);
			Assert.Contains(updatedEntity.Lines, e => e.LineNumber == 2);

			// update property 1
			DemoServiceContext.StartTracking(updatedEntity);
			child1 = updatedEntity.Lines.First(e => e.LineNumber == 1);
			child1.ItemName = DemoServiceContextFixture.XUnitTestPrefix;
			await DemoServiceContext.SaveChangesAsync();
			updatedEntity = await DemoServiceContext.Orders
				.Expand(e => e.Lines)
				.Where(e => e.CustomerName == originalEntity.CustomerName)
				.FoundryExecuteFirstOrDefaultAsync();
			Assert.NotNull(updatedEntity);
			Assert.NotNull(updatedEntity.Lines);
			Assert.NotEmpty(updatedEntity.Lines);
			Assert.Contains(updatedEntity.Lines, e => e.LineNumber == 1);
			Assert.Equal(
				DemoServiceContextFixture.XUnitTestPrefix,
				updatedEntity.Lines.First(e => e.LineNumber == 1).ItemName);

			// clear Lines collection
			DemoServiceContext.StartTracking(updatedEntity);
			// updatedEntity.Lines.Clear() will be not tracked.
			foreach (var child in updatedEntity.Lines.ToList())
				updatedEntity.Lines.Remove(child);
			await DemoServiceContext.SaveChangesAsync();
			updatedEntity = await DemoServiceContext.Orders
				.Expand(e => e.Lines)
				.Where(e => e.CustomerName == originalEntity.CustomerName)
				.FoundryExecuteFirstOrDefaultAsync();
			Assert.NotNull(updatedEntity);
			Assert.NotNull(updatedEntity.Lines);
			Assert.DoesNotContain(updatedEntity.Lines, e => e.LineNumber == 1);
			Assert.DoesNotContain(updatedEntity.Lines, e => e.LineNumber == 1);

			// delete entity
			await DemoServiceContext.FoundryDeleteAsync<Order>(updatedEntity.Id);
		}
		#endregion

		#region public async CanImportExport
		[Fact(DisplayName = "Order: can import/export")]
		public async Task CanImportExport()
		{
			const string customerName = DemoServiceContextFixture.XUnitTestPrefix + "import";

			#region source
			const string source = @"
{
  ""EntityVersion"": 1,
			""EntityType"": ""Order"",
			""Entities"": [
			{
				""CustomerName"": """ + customerName + @"""
			}
			]
}";
			#endregion

			await DemoServiceContext.ImportAssets(nameof(Order), true, Encoding.UTF8.GetBytes(source));

			var entity = await DemoServiceContext.Orders
				.Where(e => e.CustomerName == customerName)
				.FoundryExecuteFirstOrDefaultAsync();

			Assert.NotNull(entity);

			var data = await DemoServiceContext.ExportAssets(
				nameof(Order),
				null,
				null,
				new[] { entity.Id });

			Assert.NotEmpty(data);
			Assert.Contains(customerName, Encoding.UTF8.GetString(data), StringComparison.OrdinalIgnoreCase);

			await DemoServiceContext.FoundryDeleteAsync<Order>(entity.Id);
		}
		#endregion
	}
}