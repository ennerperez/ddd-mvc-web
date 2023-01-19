using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class _638096927092788578 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "db_default");

            migrationBuilder.EnsureSchema(
                name: "db_identity");

            migrationBuilder.CreateTable(
                name: "Clients",
                schema: "db_default",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Identification = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    FullName = table.Column<string>(type: "varchar(max)", nullable: false, defaultValue: "Text"),
                    Address = table.Column<string>(type: "varchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "varchar(max)", nullable: true),
                    Category = table.Column<string>(type: "varchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "db_identity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "varchar(400)", maxLength: 400, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    NormalizedName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                schema: "db_default",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false, defaultValue: 7),
                    Value = table.Column<string>(type: "varchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "db_identity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "varchar(400)", maxLength: 400, nullable: true),
                    SecurityStamp = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true),
                    PhoneNumber = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolesClaims",
                schema: "db_identity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    ClaimValue = table.Column<string>(type: "varchar(400)", maxLength: 400, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolesClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolesClaims_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "db_identity",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Budgets",
                schema: "db_default",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Taxes = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExpireAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedById = table.Column<int>(type: "int", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Budgets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Budgets_Clients_ClientId",
                        column: x => x.ClientId,
                        principalSchema: "db_default",
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Budgets_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalSchema: "db_identity",
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Budgets_Users_DeletedById",
                        column: x => x.DeletedById,
                        principalSchema: "db_identity",
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Budgets_Users_ModifiedById",
                        column: x => x.ModifiedById,
                        principalSchema: "db_identity",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UsersClaims",
                schema: "db_identity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    ClaimValue = table.Column<string>(type: "varchar(400)", maxLength: 400, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsersClaims_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "db_identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsersLogins",
                schema: "db_identity",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "varchar(400)", maxLength: 400, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProviderKey = table.Column<string>(type: "varchar(400)", maxLength: 400, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersLogins", x => new { x.UserId, x.LoginProvider });
                    table.ForeignKey(
                        name: "FK_UsersLogins_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "db_identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsersRoles",
                schema: "db_identity",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UsersRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "db_identity",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsersRoles_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "db_identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsersTokens",
                schema: "db_identity",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LoginProvider = table.Column<string>(type: "varchar(400)", maxLength: 400, nullable: false),
                    Name = table.Column<string>(type: "varchar(400)", maxLength: 400, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Value = table.Column<string>(type: "varchar(400)", maxLength: 400, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_UsersTokens_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "db_identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_ClientId",
                schema: "db_default",
                table: "Budgets",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_Code",
                schema: "db_default",
                table: "Budgets",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_CreatedAt",
                schema: "db_default",
                table: "Budgets",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_CreatedById",
                schema: "db_default",
                table: "Budgets",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_DeletedById",
                schema: "db_default",
                table: "Budgets",
                column: "DeletedById");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_ExpireAt",
                schema: "db_default",
                table: "Budgets",
                column: "ExpireAt");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_ModifiedAt",
                schema: "db_default",
                table: "Budgets",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_ModifiedById",
                schema: "db_default",
                table: "Budgets",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_Status",
                schema: "db_default",
                table: "Budgets",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_CreatedAt",
                schema: "db_default",
                table: "Clients",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Identification",
                schema: "db_default",
                table: "Clients",
                column: "Identification",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_ModifiedAt",
                schema: "db_default",
                table: "Clients",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_CreatedAt",
                schema: "db_identity",
                table: "Roles",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_ModifiedAt",
                schema: "db_identity",
                table: "Roles",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                schema: "db_identity",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_NormalizedName",
                schema: "db_identity",
                table: "Roles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolesClaims_ClaimType",
                schema: "db_identity",
                table: "RolesClaims",
                column: "ClaimType");

            migrationBuilder.CreateIndex(
                name: "IX_RolesClaims_CreatedAt",
                schema: "db_identity",
                table: "RolesClaims",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RolesClaims_ModifiedAt",
                schema: "db_identity",
                table: "RolesClaims",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RolesClaims_RoleId",
                schema: "db_identity",
                table: "RolesClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Settings_CreatedAt",
                schema: "db_default",
                table: "Settings",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Settings_Key",
                schema: "db_default",
                table: "Settings",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Settings_ModifiedAt",
                schema: "db_default",
                table: "Settings",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CreatedAt",
                schema: "db_identity",
                table: "Users",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                schema: "db_identity",
                table: "Users",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ModifiedAt",
                schema: "db_identity",
                table: "Users",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Users_NormalizedEmail",
                schema: "db_identity",
                table: "Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Users_NormalizedUserName",
                schema: "db_identity",
                table: "Users",
                column: "NormalizedUserName");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                schema: "db_identity",
                table: "Users",
                column: "UserName");

            migrationBuilder.CreateIndex(
                name: "IX_UsersClaims_ClaimType",
                schema: "db_identity",
                table: "UsersClaims",
                column: "ClaimType");

            migrationBuilder.CreateIndex(
                name: "IX_UsersClaims_CreatedAt",
                schema: "db_identity",
                table: "UsersClaims",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UsersClaims_ModifiedAt",
                schema: "db_identity",
                table: "UsersClaims",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UsersClaims_UserId",
                schema: "db_identity",
                table: "UsersClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UsersLogins_CreatedAt",
                schema: "db_identity",
                table: "UsersLogins",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UsersLogins_ModifiedAt",
                schema: "db_identity",
                table: "UsersLogins",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UsersRoles_CreatedAt",
                schema: "db_identity",
                table: "UsersRoles",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UsersRoles_ModifiedAt",
                schema: "db_identity",
                table: "UsersRoles",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UsersRoles_RoleId",
                schema: "db_identity",
                table: "UsersRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UsersTokens_CreatedAt",
                schema: "db_identity",
                table: "UsersTokens",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UsersTokens_ModifiedAt",
                schema: "db_identity",
                table: "UsersTokens",
                column: "ModifiedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Budgets",
                schema: "db_default");

            migrationBuilder.DropTable(
                name: "RolesClaims",
                schema: "db_identity");

            migrationBuilder.DropTable(
                name: "Settings",
                schema: "db_default");

            migrationBuilder.DropTable(
                name: "UsersClaims",
                schema: "db_identity");

            migrationBuilder.DropTable(
                name: "UsersLogins",
                schema: "db_identity");

            migrationBuilder.DropTable(
                name: "UsersRoles",
                schema: "db_identity");

            migrationBuilder.DropTable(
                name: "UsersTokens",
                schema: "db_identity");

            migrationBuilder.DropTable(
                name: "Clients",
                schema: "db_default");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "db_identity");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "db_identity");
        }
    }
}
