using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations.Cache.Sqlite
{
    public partial class _638251520945663848 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "db_default_Countries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "varchar(500)", nullable: false, defaultValue: "100"),
                    ISO3166 = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_db_default_Countries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_db_default_Countries_ISO3166",
                table: "db_default_Countries",
                column: "ISO3166",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_db_default_Countries_Name",
                table: "db_default_Countries",
                column: "Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "db_default_Countries");
        }
    }
}
