using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NSL.ASPNET.Configuration.Tests.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Variables",
                columns: table => new
                {
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true),
                    UpdateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Variables", x => x.Name);
                });

            migrationBuilder.InsertData(
                table: "Variables",
                columns: new[] { "Name", "UpdateTime", "Value" },
                values: new object[] { "dv0", new DateTime(2024, 8, 9, 0, 0, 0, 0, DateTimeKind.Utc), "1" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Variables");
        }
    }
}
