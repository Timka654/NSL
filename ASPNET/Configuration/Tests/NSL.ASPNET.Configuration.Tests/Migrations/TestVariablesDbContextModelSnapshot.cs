﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NSL.ASPNET.MemoryLogger.Tests;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NSL.ASPNET.Configuration.Tests.Migrations
{
    [DbContext(typeof(TestVariablesDbContext))]
    partial class TestVariablesDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("NSL.ASPNET.MemoryLogger.Tests.TestVariableModel", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("Name");

                    b.ToTable("Variables");

                    b.HasData(
                        new
                        {
                            Name = "dv0",
                            UpdateTime = new DateTime(2024, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Value = "1"
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
