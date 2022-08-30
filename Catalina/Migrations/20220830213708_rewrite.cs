using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Catalina.Migrations
{
    public partial class rewrite : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

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

            migrationBuilder.CreateTable(
                name: "GuildProperties",
                columns: table => new
                {
                    ID = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    StarboardChannelID = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    StarboardEmojiNameOrID = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StarboardThreshhold = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildProperties", x => x.ID);
                    table.ForeignKey(
                        name: "FK_GuildProperties_Emojis_StarboardEmojiNameOrID",
                        column: x => x.StarboardEmojiNameOrID,
                        principalTable: "Emojis",
                        principalColumn: "NameOrID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Responses",
                columns: table => new
                {
                    ID = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Trigger = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Content = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GuildPropertyID = table.Column<ulong>(type: "bigint unsigned", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Responses", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Responses_GuildProperties_GuildPropertyID",
                        column: x => x.GuildPropertyID,
                        principalTable: "GuildProperties",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    ID = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    IsAutomaticallyAdded = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsColourable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsRenamabale = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    GuildPropertyID = table.Column<ulong>(type: "bigint unsigned", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Roles_GuildProperties_GuildPropertyID",
                        column: x => x.GuildPropertyID,
                        principalTable: "GuildProperties",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "StarboardMessages",
                columns: table => new
                {
                    ID = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ChannelID = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    MessageID = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    StarboardMessageID = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    GuildPropertyID = table.Column<ulong>(type: "bigint unsigned", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StarboardMessages", x => x.ID);
                    table.ForeignKey(
                        name: "FK_StarboardMessages_GuildProperties_GuildPropertyID",
                        column: x => x.GuildPropertyID,
                        principalTable: "GuildProperties",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Reactions",
                columns: table => new
                {
                    ID = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MessageID = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    RoleID = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    ChannelID = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    EmojiNameOrID = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GuildPropertyID = table.Column<ulong>(type: "bigint unsigned", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "StarboardVotes",
                columns: table => new
                {
                    ID = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    StarboardMessageID = table.Column<ulong>(type: "bigint unsigned", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StarboardVotes", x => x.ID);
                    table.ForeignKey(
                        name: "FK_StarboardVotes_StarboardMessages_StarboardMessageID",
                        column: x => x.StarboardMessageID,
                        principalTable: "StarboardMessages",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_GuildProperties_StarboardEmojiNameOrID",
                table: "GuildProperties",
                column: "StarboardEmojiNameOrID");

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

            migrationBuilder.CreateIndex(
                name: "IX_Responses_GuildPropertyID",
                table: "Responses",
                column: "GuildPropertyID");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_GuildPropertyID",
                table: "Roles",
                column: "GuildPropertyID");

            migrationBuilder.CreateIndex(
                name: "IX_StarboardMessages_GuildPropertyID",
                table: "StarboardMessages",
                column: "GuildPropertyID");

            migrationBuilder.CreateIndex(
                name: "IX_StarboardVotes_StarboardMessageID",
                table: "StarboardVotes",
                column: "StarboardMessageID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reactions");

            migrationBuilder.DropTable(
                name: "Responses");

            migrationBuilder.DropTable(
                name: "StarboardVotes");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "StarboardMessages");

            migrationBuilder.DropTable(
                name: "GuildProperties");

            migrationBuilder.DropTable(
                name: "Emojis");
        }
    }
}
