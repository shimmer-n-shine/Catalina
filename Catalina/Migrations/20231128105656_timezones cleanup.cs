using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalina.Migrations
{
    /// <inheritdoc />
    public partial class timezonescleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Guilds_TimezonesSettings_TimezoneSettingsID",
                table: "Guilds");

            migrationBuilder.DropTable(
                name: "TimezonesSettings");

            migrationBuilder.DropIndex(
                name: "IX_Guilds_TimezoneSettingsID",
                table: "Guilds");

            migrationBuilder.DropColumn(
                name: "TimezoneSettingsID",
                table: "Guilds");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "TimezoneSettingsID",
                table: "Guilds",
                type: "bigint unsigned",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TimezonesSettings",
                columns: table => new
                {
                    ID = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Enabled = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimezonesSettings", x => x.ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Guilds_TimezoneSettingsID",
                table: "Guilds",
                column: "TimezoneSettingsID");

            migrationBuilder.AddForeignKey(
                name: "FK_Guilds_TimezonesSettings_TimezoneSettingsID",
                table: "Guilds",
                column: "TimezoneSettingsID",
                principalTable: "TimezonesSettings",
                principalColumn: "ID");
        }
    }
}
