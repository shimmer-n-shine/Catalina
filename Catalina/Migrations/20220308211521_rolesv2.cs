using Microsoft.EntityFrameworkCore.Migrations;

namespace Catalina.Migrations
{
    public partial class rolesv2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GuildID",
                table: "Role");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "GuildID",
                table: "Role",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);
        }
    }
}
