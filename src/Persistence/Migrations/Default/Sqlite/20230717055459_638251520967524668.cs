using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations.Default.Sqlite
{
    public partial class _638251520967524668 : Migration
    {
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
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    Type = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 7),
                    Value = table.Column<string>(type: "varchar(500)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_db_default_Settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "db_identity_Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "varchar(400)", maxLength: 400, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    NormalizedName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_db_identity_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "db_identity_Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    UserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "varchar(400)", maxLength: 400, nullable: true),
                    SecurityStamp = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true),
                    PhoneNumber = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockoutEnd = table.Column<long>(type: "INTEGER", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_db_identity_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "db_identity_RolesClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    RoleId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClaimType = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    ClaimValue = table.Column<string>(type: "varchar(400)", maxLength: 400, nullable: true)
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
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedById = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedById = table.Column<int>(type: "INTEGER", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    DeletedById = table.Column<int>(type: "INTEGER", nullable: true),
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
                });

            migrationBuilder.CreateTable(
                name: "db_identity_UsersClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClaimType = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    ClaimValue = table.Column<string>(type: "varchar(400)", maxLength: 400, nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "db_identity_UsersLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "varchar(400)", maxLength: 400, nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    ProviderKey = table.Column<string>(type: "varchar(400)", maxLength: 400, nullable: true),
                    ProviderDisplayName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "db_identity_UsersRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    RoleId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime", nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "db_identity_UsersTokens",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    LoginProvider = table.Column<string>(type: "varchar(400)", maxLength: 400, nullable: false),
                    Name = table.Column<string>(type: "varchar(400)", maxLength: 400, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    Value = table.Column<string>(type: "varchar(400)", maxLength: 400, nullable: true)
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
