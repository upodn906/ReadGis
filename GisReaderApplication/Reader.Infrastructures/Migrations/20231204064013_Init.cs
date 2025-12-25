using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reader.Infrastructures.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Gis");

            migrationBuilder.CreateTable(
                name: "t_GisInfoTmp",
                schema: "Gis",
                columns: table => new
                {
                    GisInfoTmpId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GisSymbolCode = table.Column<int>(type: "INTEGER", nullable: false),
                    LayerName = table.Column<string>(type: "TEXT", nullable: true),
                    GisInfoShapeStr = table.Column<string>(type: "TEXT", nullable: true),
                    GisInfoShapeLatLngStr = table.Column<string>(type: "TEXT", nullable: true),
                    GisInfoJson = table.Column<string>(type: "TEXT", nullable: true),
                    GisInfoGeoCode = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_GisInfoTmp", x => x.GisInfoTmpId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "t_GisInfoTmp",
                schema: "Gis");
        }
    }
}
