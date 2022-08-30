using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Catalina.Migrations
{
    public partial class rewriteemoji : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmojiName",
                table: "Reactions");

            migrationBuilder.DropColumn(
                name: "Prefix",
                table: "GuildProperties");

            migrationBuilder.RenameColumn(
                name: "AllowedChannelsSerialised",
                table: "Responses",
                newName: "Name");

            migrationBuilder.AlterColumn<ulong>(
                name: "ID",
                table: "Role",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<string>(
                name: "EmojiNameOrID",
                table: "Reactions",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<ulong>(
                name: "StarBoardChannel",
                table: "GuildProperties",
                type: "bigint unsigned",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StarboardEmojiNameOrID",
                table: "GuildProperties",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "StarboardThreshhold",
                table: "GuildProperties",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Emojis",
                columns: table => new
                {
                    NameOrID = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<byte>(type: "tinyint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emojis", x => x.NameOrID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Reactions_EmojiNameOrID",
                table: "Reactions",
                column: "EmojiNameOrID");

            migrationBuilder.CreateIndex(
                name: "IX_GuildProperties_StarboardEmojiNameOrID",
                table: "GuildProperties",
                column: "StarboardEmojiNameOrID");

            migrationBuilder.AddForeignKey(
                name: "FK_GuildProperties_Emojis_StarboardEmojiNameOrID",
                table: "GuildProperties",
                column: "StarboardEmojiNameOrID",
                principalTable: "Emojis",
                principalColumn: "NameOrID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reactions_Emojis_EmojiNameOrID",
                table: "Reactions",
                column: "EmojiNameOrID",
                principalTable: "Emojis",
                principalColumn: "NameOrID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuildProperties_Emojis_StarboardEmojiNameOrID",
                table: "GuildProperties");

            migrationBuilder.DropForeignKey(
                name: "FK_Reactions_Emojis_EmojiNameOrID",
                table: "Reactions");

            migrationBuilder.DropTable(
                name: "Emojis");

            migrationBuilder.DropIndex(
                name: "IX_Reactions_EmojiNameOrID",
                table: "Reactions");

            migrationBuilder.DropIndex(
                name: "IX_GuildProperties_StarboardEmojiNameOrID",
                table: "GuildProperties");

            migrationBuilder.DropColumn(
                name: "EmojiNameOrID",
                table: "Reactions");

            migrationBuilder.DropColumn(
                name: "StarBoardChannel",
                table: "GuildProperties");

            migrationBuilder.DropColumn(
                name: "StarboardEmojiNameOrID",
                table: "GuildProperties");

            migrationBuilder.DropColumn(
                name: "StarboardThreshhold",
                table: "GuildProperties");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Responses",
                newName: "AllowedChannelsSerialised");

            migrationBuilder.AlterColumn<ulong>(
                name: "ID",
                table: "Role",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<string>(
                name: "EmojiName",
                table: "Reactions",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Prefix",
                table: "GuildProperties",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
