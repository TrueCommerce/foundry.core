using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DemoService.SQL.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Core_Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: true),
                    UserCreated = table.Column<string>(nullable: true),
                    DateModified = table.Column<DateTime>(nullable: true),
                    UserModified = table.Column<string>(nullable: true),
                    TenantId = table.Column<string>(maxLength: 255, nullable: false),
                    CustomerName = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Core_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DemoService_OrderLines",
                columns: table => new
                {
                    OrderId = table.Column<Guid>(nullable: false),
                    LineNumber = table.Column<int>(nullable: false),
                    ItemName = table.Column<string>(nullable: false),
                    ItemQty = table.Column<decimal>(nullable: false),
                    OrderId1 = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemoService_OrderLines", x => new { x.OrderId, x.LineNumber });
                    table.UniqueConstraint("AK_DemoService_OrderLines_LineNumber_OrderId", x => new { x.LineNumber, x.OrderId });
                    table.ForeignKey(
                        name: "FK_DemoService_OrderLines_Core_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Core_Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DemoService_OrderLines_Core_Orders_OrderId1",
                        column: x => x.OrderId1,
                        principalTable: "Core_Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DemoService_OrderLines_OrderId1",
                table: "DemoService_OrderLines",
                column: "OrderId1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DemoService_OrderLines");

            migrationBuilder.DropTable(
                name: "Core_Orders");
        }
    }
}
