using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations.MySql
{
    /// <inheritdoc />
    public partial class _638096926660493668 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "db_default_Clients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Identification = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FullName = table.Column<string>(type: "longtext", nullable: false, defaultValue: "Text")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Address = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhoneNumber = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Category = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "now()"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_db_default_Clients", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "db_default_Settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Key = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<int>(type: "int", nullable: false, defaultValue: 7),
                    Value = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "now()"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_db_default_Settings", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "db_identity_Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "now()"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NormalizedName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConcurrencyStamp = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_db_identity_Roles", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "db_identity_Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "now()"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UserName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NormalizedUserName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NormalizedEmail = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmailConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PasswordHash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SecurityStamp = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConcurrencyStamp = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhoneNumber = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhoneNumberConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_db_identity_Users", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "db_identity_RolesClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "now()"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClaimValue = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_db_identity_RolesClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_db_identity_RolesClaims_db_identity_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "db_identity_Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "db_default_Budgets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Taxes = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExpireAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "now()"),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedById = table.Column<int>(type: "int", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
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
                    table.ForeignKey(
                        name: "FK_db_default_Budgets_db_identity_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "db_identity_Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_db_default_Budgets_db_identity_Users_DeletedById",
                        column: x => x.DeletedById,
                        principalTable: "db_identity_Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_db_default_Budgets_db_identity_Users_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "db_identity_Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "db_identity_UsersClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "now()"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClaimValue = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_db_identity_UsersClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_db_identity_UsersClaims_db_identity_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "db_identity_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "db_identity_UsersLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "now()"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ProviderKey = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProviderDisplayName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_db_identity_UsersLogins", x => new { x.UserId, x.LoginProvider });
                    table.ForeignKey(
                        name: "FK_db_identity_UsersLogins_db_identity_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "db_identity_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "db_identity_UsersRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "now()"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_db_identity_UsersRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_db_identity_UsersRoles_db_identity_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "db_identity_Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_db_identity_UsersRoles_db_identity_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "db_identity_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "db_identity_UsersTokens",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LoginProvider = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "now()"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Value = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_db_identity_UsersTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_db_identity_UsersTokens_db_identity_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "db_identity_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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
                name: "IX_db_default_Budgets_CreatedById",
                table: "db_default_Budgets",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_db_default_Budgets_DeletedById",
                table: "db_default_Budgets",
                column: "DeletedById");

            migrationBuilder.CreateIndex(
                name: "IX_db_default_Budgets_ExpireAt",
                table: "db_default_Budgets",
                column: "ExpireAt");

            migrationBuilder.CreateIndex(
                name: "IX_db_default_Budgets_ModifiedAt",
                table: "db_default_Budgets",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_db_default_Budgets_ModifiedById",
                table: "db_default_Budgets",
                column: "ModifiedById");

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

            migrationBuilder.CreateIndex(
                name: "IX_db_identity_Roles_CreatedAt",
                table: "db_identity_Roles",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_db_identity_Roles_ModifiedAt",
                table: "db_identity_Roles",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_db_identity_Roles_Name",
                table: "db_identity_Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_db_identity_Roles_NormalizedName",
                table: "db_identity_Roles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_db_identity_RolesClaims_ClaimType",
                table: "db_identity_RolesClaims",
                column: "ClaimType");

            migrationBuilder.CreateIndex(
                name: "IX_db_identity_RolesClaims_CreatedAt",
                table: "db_identity_RolesClaims",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_db_identity_RolesClaims_ModifiedAt",
                table: "db_identity_RolesClaims",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_db_identity_RolesClaims_RoleId",
                table: "db_identity_RolesClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_db_identity_Users_CreatedAt",
                table: "db_identity_Users",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_db_identity_Users_Email",
                table: "db_identity_Users",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_db_identity_Users_ModifiedAt",
                table: "db_identity_Users",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_db_identity_Users_NormalizedEmail",
                table: "db_identity_Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_db_identity_Users_NormalizedUserName",
                table: "db_identity_Users",
                column: "NormalizedUserName");

            migrationBuilder.CreateIndex(
                name: "IX_db_identity_Users_UserName",
                table: "db_identity_Users",
                column: "UserName");

            migrationBuilder.CreateIndex(
                name: "IX_db_identity_UsersClaims_ClaimType",
                table: "db_identity_UsersClaims",
                column: "ClaimType");

            migrationBuilder.CreateIndex(
                name: "IX_db_identity_UsersClaims_CreatedAt",
                table: "db_identity_UsersClaims",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_db_identity_UsersClaims_ModifiedAt",
                table: "db_identity_UsersClaims",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_db_identity_UsersClaims_UserId",
                table: "db_identity_UsersClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_db_identity_UsersLogins_CreatedAt",
                table: "db_identity_UsersLogins",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_db_identity_UsersLogins_ModifiedAt",
                table: "db_identity_UsersLogins",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_db_identity_UsersRoles_CreatedAt",
                table: "db_identity_UsersRoles",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_db_identity_UsersRoles_ModifiedAt",
                table: "db_identity_UsersRoles",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_db_identity_UsersRoles_RoleId",
                table: "db_identity_UsersRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_db_identity_UsersTokens_CreatedAt",
                table: "db_identity_UsersTokens",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_db_identity_UsersTokens_ModifiedAt",
                table: "db_identity_UsersTokens",
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
                name: "db_identity_RolesClaims");

            migrationBuilder.DropTable(
                name: "db_identity_UsersClaims");

            migrationBuilder.DropTable(
                name: "db_identity_UsersLogins");

            migrationBuilder.DropTable(
                name: "db_identity_UsersRoles");

            migrationBuilder.DropTable(
                name: "db_identity_UsersTokens");

            migrationBuilder.DropTable(
                name: "db_default_Clients");

            migrationBuilder.DropTable(
                name: "db_identity_Roles");

            migrationBuilder.DropTable(
                name: "db_identity_Users");
        }
    }
}
