using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Catalina.Migrations
{
    public partial class reaction_gone_crab : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reactions");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reactions",
                columns: table => new
                {
                    ID = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ChannelID = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    EmojiNameOrID = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GuildPropertyID = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    MessageID = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    RoleID = table.Column<ulong>(type: "bigint unsigned", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reactions", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Reactions_Emojis_EmojiNameOrID",
                        column: x => x.EmojiNameOrID,
                        principalTable: "Emojis",
                        principalColumn: "NameOrID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reactions_GuildProperties_GuildPropertyID",
                        column: x => x.GuildPropertyID,
                        principalTable: "GuildProperties",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reactions_Roles_RoleID",
                        column: x => x.RoleID,
                        principalTable: "Roles",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Reactions_EmojiNameOrID",
                table: "Reactions",
                column: "EmojiNameOrID");

            migrationBuilder.CreateIndex(
                name: "IX_Reactions_GuildPropertyID",
                table: "Reactions",
                column: "GuildPropertyID");

            migrationBuilder.CreateIndex(
                name: "IX_Reactions_RoleID",
                table: "Reactions",
                column: "RoleID");
        }
    }
}
