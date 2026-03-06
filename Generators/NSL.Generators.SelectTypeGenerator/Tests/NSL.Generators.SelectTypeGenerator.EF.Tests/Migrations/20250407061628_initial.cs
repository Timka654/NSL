using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NSL.Generators.SelectTypeGenerator.EF.Tests.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Dev1Model",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Data = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dev1Model", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Dev2Model",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Data = table.Column<string>(type: "text", nullable: false),
                    ParentId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dev2Model", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dev2Model_Dev1Model_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Dev1Model",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dev2Model_ParentId",
                table: "Dev2Model",
                column: "ParentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Dev2Model");

            migrationBuilder.DropTable(
                name: "Dev1Model");
        }
    }
}
