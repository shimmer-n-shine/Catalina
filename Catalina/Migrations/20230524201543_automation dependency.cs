using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalina.Migrations
{
    /// <inheritdoc />
    public partial class automationdependency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRetroactivelyAdded",
                table: "Roles",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "DependentRoles",
                columns: table => new
                {
                    ID = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoleID = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    DependentRoleID = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DependentRoles", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DependentRoles_Roles_DependentRoleID",
                        column: x => x.DependentRoleID,
                        principalTable: "Roles",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DependentRoles_Roles_RoleID",
                        column: x => x.RoleID,
                        principalTable: "Roles",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_DependentRoles_DependentRoleID",
                table: "DependentRoles",
                column: "DependentRoleID");

            migrationBuilder.CreateIndex(
                name: "IX_DependentRoles_RoleID",
                table: "DependentRoles",
                column: "RoleID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DependentRoles");

            migrationBuilder.DropColumn(
                name: "IsRetroactivelyAdded",
                table: "Roles");
        }
    }
}
