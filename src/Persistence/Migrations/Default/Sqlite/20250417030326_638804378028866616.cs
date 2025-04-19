using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations.Default.Sqlite
{
    /// <inheritdoc />
    public partial class _638804378028866616 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "db_default_Clients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Identification = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    FullName = table.Column<string>(type: "varchar(500)", nullable: false, defaultValue: "Text"),
                    Address = table.Column<string>(type: "varchar(500)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "varchar(500)", nullable: true),
                    Category = table.Column<string>(type: "varchar(500)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_db_default_Clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "db_default_Settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    Type = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)7),
                    Value = table.Column<string>(type: "varchar(500)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_db_default_Settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "db_default_Budgets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    ClientId = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<short>(type: "INTEGER", nullable: false),
                    Subtotal = table.Column<double>(type: "double", nullable: false),
                    Taxes = table.Column<double>(type: "double", nullable: false),
                    Total = table.Column<double>(type: "double", nullable: false),
                    ExpireAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    CreatedById = table.Column<int>(type: "INTEGER", nullable: true),
                    ModifiedById = table.Column<int>(type: "INTEGER", nullable: true),
                    DeletedById = table.Column<int>(type: "INTEGER", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_db_default_Budgets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_db_default_Budgets_db_default_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "db_default_Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_db_default_Budgets_ClientId",
                table: "db_default_Budgets",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_db_default_Budgets_Code",
                table: "db_default_Budgets",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_db_default_Budgets_CreatedAt",
                table: "db_default_Budgets",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_db_default_Budgets_ExpireAt",
                table: "db_default_Budgets",
                column: "ExpireAt");

            migrationBuilder.CreateIndex(
                name: "IX_db_default_Budgets_ModifiedAt",
                table: "db_default_Budgets",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_db_default_Budgets_Status",
                table: "db_default_Budgets",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_db_default_Clients_CreatedAt",
                table: "db_default_Clients",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_db_default_Clients_Identification",
                table: "db_default_Clients",
                column: "Identification",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_db_default_Clients_ModifiedAt",
                table: "db_default_Clients",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_db_default_Settings_CreatedAt",
                table: "db_default_Settings",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_db_default_Settings_Key",
                table: "db_default_Settings",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_db_default_Settings_ModifiedAt",
                table: "db_default_Settings",
                column: "ModifiedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "db_default_Budgets");

            migrationBuilder.DropTable(
                name: "db_default_Settings");

            migrationBuilder.DropTable(
                name: "db_default_Clients");
        }
    }
}
