using Microsoft.EntityFrameworkCore.Migrations;

namespace Persistence.Migrations.Cache.Sqlite
{
    public partial class _638251520945663848 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                "db_default_Countries",
                table => new
                {
                    Id = table.Column<int>("INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>("varchar(500)", nullable: false, defaultValue: "100"),
                    ISO3166 = table.Column<string>("varchar(2)", maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_db_default_Countries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                "IX_db_default_Countries_ISO3166",
                "db_default_Countries",
                "ISO3166",
                unique: true);

            migrationBuilder.CreateIndex(
                "IX_db_default_Countries_Name",
                "db_default_Countries",
                "Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder) =>
            migrationBuilder.DropTable(
                "db_default_Countries");
    }
}
