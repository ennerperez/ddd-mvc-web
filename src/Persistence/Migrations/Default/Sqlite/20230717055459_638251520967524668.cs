using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Persistence.Migrations.Default.Sqlite
{
    public partial class _638251520967524668 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                "db_default_Clients",
                table => new
                {
                    Id = table.Column<int>("INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Identification = table.Column<string>("varchar(20)", maxLength: 20, nullable: false),
                    FullName = table.Column<string>("varchar(500)", nullable: false, defaultValue: "Text"),
                    Address = table.Column<string>("varchar(500)", nullable: true),
                    PhoneNumber = table.Column<string>("varchar(500)", nullable: true),
                    Category = table.Column<string>("varchar(500)", nullable: true),
                    CreatedAt = table.Column<DateTime>("datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>("datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_db_default_Clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                "db_default_Settings",
                table => new
                {
                    Id = table.Column<int>("INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>("varchar(20)", maxLength: 20, nullable: false),
                    Type = table.Column<int>("INTEGER", nullable: false, defaultValue: 7),
                    Value = table.Column<string>("varchar(500)", nullable: true),
                    CreatedAt = table.Column<DateTime>("datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>("datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_db_default_Settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                "db_identity_Roles",
                table => new
                {
                    Id = table.Column<int>("INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>("varchar(400)", maxLength: 400, nullable: true),
                    CreatedAt = table.Column<DateTime>("datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>("datetime", nullable: true),
                    Name = table.Column<string>("varchar(100)", maxLength: 100, nullable: false),
                    NormalizedName = table.Column<string>("varchar(100)", maxLength: 100, nullable: false),
                    ConcurrencyStamp = table.Column<string>("varchar(36)", maxLength: 36, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_db_identity_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                "db_identity_Users",
                table => new
                {
                    Id = table.Column<int>("INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedAt = table.Column<DateTime>("datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>("datetime", nullable: true),
                    UserName = table.Column<string>("varchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>("varchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>("varchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>("varchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>("INTEGER", nullable: false),
                    PasswordHash = table.Column<string>("varchar(400)", maxLength: 400, nullable: true),
                    SecurityStamp = table.Column<string>("varchar(36)", maxLength: 36, nullable: true),
                    ConcurrencyStamp = table.Column<string>("varchar(36)", maxLength: 36, nullable: true),
                    PhoneNumber = table.Column<string>("varchar(20)", maxLength: 20, nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>("INTEGER", nullable: false),
                    TwoFactorEnabled = table.Column<bool>("INTEGER", nullable: false),
                    LockoutEnd = table.Column<long>("INTEGER", nullable: true),
                    LockoutEnabled = table.Column<bool>("INTEGER", nullable: false),
                    AccessFailedCount = table.Column<int>("INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_db_identity_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                "db_identity_RolesClaims",
                table => new
                {
                    Id = table.Column<int>("INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedAt = table.Column<DateTime>("datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>("datetime", nullable: true),
                    RoleId = table.Column<int>("INTEGER", nullable: false),
                    ClaimType = table.Column<string>("varchar(256)", maxLength: 256, nullable: true),
                    ClaimValue = table.Column<string>("varchar(400)", maxLength: 400, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_db_identity_RolesClaims", x => x.Id);
                    table.ForeignKey(
                        "FK_db_identity_RolesClaims_db_identity_Roles_RoleId",
                        x => x.RoleId,
                        "db_identity_Roles",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                "db_default_Budgets",
                table => new
                {
                    Id = table.Column<Guid>("TEXT", nullable: false),
                    Code = table.Column<string>("varchar(20)", maxLength: 20, nullable: false),
                    ClientId = table.Column<int>("INTEGER", nullable: false),
                    Status = table.Column<short>("INTEGER", nullable: false),
                    Subtotal = table.Column<double>("double", nullable: false),
                    Taxes = table.Column<double>("double", nullable: false),
                    Total = table.Column<double>("double", nullable: false),
                    ExpireAt = table.Column<DateTime>("datetime", nullable: true),
                    IsDeleted = table.Column<bool>("INTEGER", nullable: false),
                    CreatedById = table.Column<int>("INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>("datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedById = table.Column<int>("INTEGER", nullable: true),
                    ModifiedAt = table.Column<DateTime>("datetime", nullable: true),
                    DeletedById = table.Column<int>("INTEGER", nullable: true),
                    DeletedAt = table.Column<DateTime>("datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_db_default_Budgets", x => x.Id);
                    table.ForeignKey(
                        "FK_db_default_Budgets_db_default_Clients_ClientId",
                        x => x.ClientId,
                        "db_default_Clients",
                        "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        "FK_db_default_Budgets_db_identity_Users_CreatedById",
                        x => x.CreatedById,
                        "db_identity_Users",
                        "Id");
                    table.ForeignKey(
                        "FK_db_default_Budgets_db_identity_Users_DeletedById",
                        x => x.DeletedById,
                        "db_identity_Users",
                        "Id");
                    table.ForeignKey(
                        "FK_db_default_Budgets_db_identity_Users_ModifiedById",
                        x => x.ModifiedById,
                        "db_identity_Users",
                        "Id");
                });

            migrationBuilder.CreateTable(
                "db_identity_UsersClaims",
                table => new
                {
                    Id = table.Column<int>("INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedAt = table.Column<DateTime>("datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>("datetime", nullable: true),
                    UserId = table.Column<int>("INTEGER", nullable: false),
                    ClaimType = table.Column<string>("varchar(256)", maxLength: 256, nullable: true),
                    ClaimValue = table.Column<string>("varchar(400)", maxLength: 400, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_db_identity_UsersClaims", x => x.Id);
                    table.ForeignKey(
                        "FK_db_identity_UsersClaims_db_identity_Users_UserId",
                        x => x.UserId,
                        "db_identity_Users",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                "db_identity_UsersLogins",
                table => new
                {
                    LoginProvider = table.Column<string>("varchar(400)", maxLength: 400, nullable: false),
                    UserId = table.Column<int>("INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>("datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>("datetime", nullable: true),
                    ProviderKey = table.Column<string>("varchar(400)", maxLength: 400, nullable: true),
                    ProviderDisplayName = table.Column<string>("varchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_db_identity_UsersLogins", x => new { x.UserId, x.LoginProvider });
                    table.ForeignKey(
                        "FK_db_identity_UsersLogins_db_identity_Users_UserId",
                        x => x.UserId,
                        "db_identity_Users",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                "db_identity_UsersRoles",
                table => new { UserId = table.Column<int>("INTEGER", nullable: false), RoleId = table.Column<int>("INTEGER", nullable: false), CreatedAt = table.Column<DateTime>("datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"), ModifiedAt = table.Column<DateTime>("datetime", nullable: true) },
                constraints: table =>
                {
                    table.PrimaryKey("PK_db_identity_UsersRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        "FK_db_identity_UsersRoles_db_identity_Roles_RoleId",
                        x => x.RoleId,
                        "db_identity_Roles",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        "FK_db_identity_UsersRoles_db_identity_Users_UserId",
                        x => x.UserId,
                        "db_identity_Users",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                "db_identity_UsersTokens",
                table => new
                {
                    UserId = table.Column<int>("INTEGER", nullable: false),
                    LoginProvider = table.Column<string>("varchar(400)", maxLength: 400, nullable: false),
                    Name = table.Column<string>("varchar(400)", maxLength: 400, nullable: false),
                    CreatedAt = table.Column<DateTime>("datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>("datetime", nullable: true),
                    Value = table.Column<string>("varchar(400)", maxLength: 400, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_db_identity_UsersTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        "FK_db_identity_UsersTokens_db_identity_Users_UserId",
                        x => x.UserId,
                        "db_identity_Users",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                "IX_db_default_Budgets_ClientId",
                "db_default_Budgets",
                "ClientId");

            migrationBuilder.CreateIndex(
                "IX_db_default_Budgets_Code",
                "db_default_Budgets",
                "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                "IX_db_default_Budgets_CreatedAt",
                "db_default_Budgets",
                "CreatedAt");

            migrationBuilder.CreateIndex(
                "IX_db_default_Budgets_CreatedById",
                "db_default_Budgets",
                "CreatedById");

            migrationBuilder.CreateIndex(
                "IX_db_default_Budgets_DeletedById",
                "db_default_Budgets",
                "DeletedById");

            migrationBuilder.CreateIndex(
                "IX_db_default_Budgets_ExpireAt",
                "db_default_Budgets",
                "ExpireAt");

            migrationBuilder.CreateIndex(
                "IX_db_default_Budgets_ModifiedAt",
                "db_default_Budgets",
                "ModifiedAt");

            migrationBuilder.CreateIndex(
                "IX_db_default_Budgets_ModifiedById",
                "db_default_Budgets",
                "ModifiedById");

            migrationBuilder.CreateIndex(
                "IX_db_default_Budgets_Status",
                "db_default_Budgets",
                "Status");

            migrationBuilder.CreateIndex(
                "IX_db_default_Clients_CreatedAt",
                "db_default_Clients",
                "CreatedAt");

            migrationBuilder.CreateIndex(
                "IX_db_default_Clients_Identification",
                "db_default_Clients",
                "Identification",
                unique: true);

            migrationBuilder.CreateIndex(
                "IX_db_default_Clients_ModifiedAt",
                "db_default_Clients",
                "ModifiedAt");

            migrationBuilder.CreateIndex(
                "IX_db_default_Settings_CreatedAt",
                "db_default_Settings",
                "CreatedAt");

            migrationBuilder.CreateIndex(
                "IX_db_default_Settings_Key",
                "db_default_Settings",
                "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                "IX_db_default_Settings_ModifiedAt",
                "db_default_Settings",
                "ModifiedAt");

            migrationBuilder.CreateIndex(
                "IX_db_identity_Roles_CreatedAt",
                "db_identity_Roles",
                "CreatedAt");

            migrationBuilder.CreateIndex(
                "IX_db_identity_Roles_ModifiedAt",
                "db_identity_Roles",
                "ModifiedAt");

            migrationBuilder.CreateIndex(
                "IX_db_identity_Roles_Name",
                "db_identity_Roles",
                "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                "IX_db_identity_Roles_NormalizedName",
                "db_identity_Roles",
                "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                "IX_db_identity_RolesClaims_ClaimType",
                "db_identity_RolesClaims",
                "ClaimType");

            migrationBuilder.CreateIndex(
                "IX_db_identity_RolesClaims_CreatedAt",
                "db_identity_RolesClaims",
                "CreatedAt");

            migrationBuilder.CreateIndex(
                "IX_db_identity_RolesClaims_ModifiedAt",
                "db_identity_RolesClaims",
                "ModifiedAt");

            migrationBuilder.CreateIndex(
                "IX_db_identity_RolesClaims_RoleId",
                "db_identity_RolesClaims",
                "RoleId");

            migrationBuilder.CreateIndex(
                "IX_db_identity_Users_CreatedAt",
                "db_identity_Users",
                "CreatedAt");

            migrationBuilder.CreateIndex(
                "IX_db_identity_Users_Email",
                "db_identity_Users",
                "Email");

            migrationBuilder.CreateIndex(
                "IX_db_identity_Users_ModifiedAt",
                "db_identity_Users",
                "ModifiedAt");

            migrationBuilder.CreateIndex(
                "IX_db_identity_Users_NormalizedEmail",
                "db_identity_Users",
                "NormalizedEmail");

            migrationBuilder.CreateIndex(
                "IX_db_identity_Users_NormalizedUserName",
                "db_identity_Users",
                "NormalizedUserName");

            migrationBuilder.CreateIndex(
                "IX_db_identity_Users_UserName",
                "db_identity_Users",
                "UserName");

            migrationBuilder.CreateIndex(
                "IX_db_identity_UsersClaims_ClaimType",
                "db_identity_UsersClaims",
                "ClaimType");

            migrationBuilder.CreateIndex(
                "IX_db_identity_UsersClaims_CreatedAt",
                "db_identity_UsersClaims",
                "CreatedAt");

            migrationBuilder.CreateIndex(
                "IX_db_identity_UsersClaims_ModifiedAt",
                "db_identity_UsersClaims",
                "ModifiedAt");

            migrationBuilder.CreateIndex(
                "IX_db_identity_UsersClaims_UserId",
                "db_identity_UsersClaims",
                "UserId");

            migrationBuilder.CreateIndex(
                "IX_db_identity_UsersLogins_CreatedAt",
                "db_identity_UsersLogins",
                "CreatedAt");

            migrationBuilder.CreateIndex(
                "IX_db_identity_UsersLogins_ModifiedAt",
                "db_identity_UsersLogins",
                "ModifiedAt");

            migrationBuilder.CreateIndex(
                "IX_db_identity_UsersRoles_CreatedAt",
                "db_identity_UsersRoles",
                "CreatedAt");

            migrationBuilder.CreateIndex(
                "IX_db_identity_UsersRoles_ModifiedAt",
                "db_identity_UsersRoles",
                "ModifiedAt");

            migrationBuilder.CreateIndex(
                "IX_db_identity_UsersRoles_RoleId",
                "db_identity_UsersRoles",
                "RoleId");

            migrationBuilder.CreateIndex(
                "IX_db_identity_UsersTokens_CreatedAt",
                "db_identity_UsersTokens",
                "CreatedAt");

            migrationBuilder.CreateIndex(
                "IX_db_identity_UsersTokens_ModifiedAt",
                "db_identity_UsersTokens",
                "ModifiedAt");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "db_default_Budgets");

            migrationBuilder.DropTable(
                "db_default_Settings");

            migrationBuilder.DropTable(
                "db_identity_RolesClaims");

            migrationBuilder.DropTable(
                "db_identity_UsersClaims");

            migrationBuilder.DropTable(
                "db_identity_UsersLogins");

            migrationBuilder.DropTable(
                "db_identity_UsersRoles");

            migrationBuilder.DropTable(
                "db_identity_UsersTokens");

            migrationBuilder.DropTable(
                "db_default_Clients");

            migrationBuilder.DropTable(
                "db_identity_Roles");

            migrationBuilder.DropTable(
                "db_identity_Users");
        }
    }
}
