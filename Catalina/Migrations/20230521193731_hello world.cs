using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalina.Migrations;
{
    /// <inheritdoc />
    public partial class helloworld : Migration
    {
        /// <inheritdoc />
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
                name: "Starboards",
                columns: table => new
                {
                    ID = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ChannelID = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    EmojiNameOrID = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Threshhold = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Starboards", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Starboards_Emojis_EmojiNameOrID",
                        column: x => x.EmojiNameOrID,
                        principalTable: "Emojis",
                        principalColumn: "NameOrID");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GuildProperties",
                columns: table => new
                {
                    ID = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    StarboardID = table.Column<ulong>(type: "bigint unsigned", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildProperties", x => x.ID);
                    table.ForeignKey(
                        name: "FK_GuildProperties_Starboards_StarboardID",
                        column: x => x.StarboardID,
                        principalTable: "Starboards",
                        principalColumn: "ID");
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
                    StarboardID = table.Column<ulong>(type: "bigint unsigned", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StarboardMessages", x => x.ID);
                    table.ForeignKey(
                        name: "FK_StarboardMessages_Starboards_StarboardID",
                        column: x => x.StarboardID,
                        principalTable: "Starboards",
                        principalColumn: "ID");
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
                        principalColumn: "ID");
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
                        principalColumn: "ID");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "StarboardVotes",
                columns: table => new
                {
                    ID = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    MessageID = table.Column<ulong>(type: "bigint unsigned", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StarboardVotes", x => x.ID);
                    table.ForeignKey(
                        name: "FK_StarboardVotes_StarboardMessages_MessageID",
                        column: x => x.MessageID,
                        principalTable: "StarboardMessages",
                        principalColumn: "ID");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_GuildProperties_StarboardID",
                table: "GuildProperties",
                column: "StarboardID");

            migrationBuilder.CreateIndex(
                name: "IX_Responses_GuildPropertyID",
                table: "Responses",
                column: "GuildPropertyID");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_GuildPropertyID",
                table: "Roles",
                column: "GuildPropertyID");

            migrationBuilder.CreateIndex(
                name: "IX_StarboardMessages_StarboardID",
                table: "StarboardMessages",
                column: "StarboardID");

            migrationBuilder.CreateIndex(
                name: "IX_Starboards_EmojiNameOrID",
                table: "Starboards",
                column: "EmojiNameOrID");

            migrationBuilder.CreateIndex(
                name: "IX_StarboardVotes_MessageID",
                table: "StarboardVotes",
                column: "MessageID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Responses");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "StarboardVotes");

            migrationBuilder.DropTable(
                name: "GuildProperties");

            migrationBuilder.DropTable(
                name: "StarboardMessages");

            migrationBuilder.DropTable(
                name: "Starboards");

            migrationBuilder.DropTable(
                name: "Emojis");
        }
    }
