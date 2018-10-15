﻿// <auto-generated />
using System;
using DemoService.SQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DemoService.SQL.Migrations
{
    [DbContext(typeof(DemoServiceDbContext))]
    [Migration("20181015032755_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.4-rtm-31024")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("DemoService.Core.Models.Order", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CustomerName")
                        .IsRequired();

                    b.Property<DateTime?>("DateCreated");

                    b.Property<DateTime?>("DateModified");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<string>("UserCreated");

                    b.Property<string>("UserModified");

                    b.HasKey("Id");

                    b.ToTable("Core_Orders");
                });

            modelBuilder.Entity("DemoService.Core.Models.OrderLine", b =>
                {
                    b.Property<Guid>("OrderId");

                    b.Property<int>("LineNumber");

                    b.Property<string>("ItemName")
                        .IsRequired();

                    b.Property<decimal>("ItemQty");

                    b.Property<Guid?>("OrderId1");

                    b.HasKey("OrderId", "LineNumber");

                    b.HasAlternateKey("LineNumber", "OrderId");

                    b.HasIndex("OrderId1");

                    b.ToTable("DemoService_OrderLines");
                });

            modelBuilder.Entity("DemoService.Core.Models.OrderLine", b =>
                {
                    b.HasOne("DemoService.Core.Models.Order")
                        .WithMany()
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DemoService.Core.Models.Order")
                        .WithMany("Lines")
                        .HasForeignKey("OrderId1");
                });
#pragma warning restore 612, 618
        }
    }
}
