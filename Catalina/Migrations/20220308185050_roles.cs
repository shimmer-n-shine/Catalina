using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Catalina.Migrations
{
    public partial class roles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminRoleIDsSerialised",
                table: "GuildProperties");

            migrationBuilder.DropColumn(
                name: "CommandChannelsSerialised",
                table: "GuildProperties");

            migrationBuilder.DropColumn(
                name: "DefaultRole",
                table: "GuildProperties");

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    ID = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GuildID = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    IsAutomaticallyAdded = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsColourable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsRenamabale = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    GuildPropertyID = table.Column<ulong>(type: "bigint unsigned", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Role_GuildProperties_GuildPropertyID",
                        column: x => x.GuildPropertyID,
                        principalTable: "GuildProperties",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Role_GuildPropertyID",
                table: "Role",
                column: "GuildPropertyID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.AddColumn<string>(
                name: "AdminRoleIDsSerialised",
                table: "GuildProperties",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CommandChannelsSerialised",
                table: "GuildProperties",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<ulong>(
                name: "DefaultRole",
                table: "GuildProperties",
                type: "bigint unsigned",
                nullable: true);
        }
    }
}
