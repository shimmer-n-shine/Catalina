using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalina.Migrations
{
    /// <inheritdoc />
    public partial class hello_world : Migration
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

            migrationBuilder.CreateTable(
                name: "StarboardSettings",
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
                    table.PrimaryKey("PK_StarboardSettings", x => x.ID);
                    table.ForeignKey(
                        name: "FK_StarboardSettings_Emojis_EmojiNameOrID",
                        column: x => x.EmojiNameOrID,
                        principalTable: "Emojis",
                        principalColumn: "NameOrID");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    ID = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    StarboardSettingsID = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    TimezoneSettingsID = table.Column<ulong>(type: "bigint unsigned", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Guilds_StarboardSettings_StarboardSettingsID",
                        column: x => x.StarboardSettingsID,
                        principalTable: "StarboardSettings",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Guilds_TimezonesSettings_TimezoneSettingsID",
                        column: x => x.TimezoneSettingsID,
                        principalTable: "TimezonesSettings",
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
                    StarboardSettingsID = table.Column<ulong>(type: "bigint unsigned", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StarboardMessages", x => x.ID);
                    table.ForeignKey(
                        name: "FK_StarboardMessages_StarboardSettings_StarboardSettingsID",
                        column: x => x.StarboardSettingsID,
                        principalTable: "StarboardSettings",
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
                    GuildID = table.Column<ulong>(type: "bigint unsigned", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Responses", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Responses_Guilds_GuildID",
                        column: x => x.GuildID,
                        principalTable: "Guilds",
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
                    Timezone = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GuildID = table.Column<ulong>(type: "bigint unsigned", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Roles_Guilds_GuildID",
                        column: x => x.GuildID,
                        principalTable: "Guilds",
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
                name: "IX_Guilds_StarboardSettingsID",
                table: "Guilds",
                column: "StarboardSettingsID");

            migrationBuilder.CreateIndex(
                name: "IX_Guilds_TimezoneSettingsID",
                table: "Guilds",
                column: "TimezoneSettingsID");

            migrationBuilder.CreateIndex(
                name: "IX_Responses_GuildID",
                table: "Responses",
                column: "GuildID");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_GuildID",
                table: "Roles",
                column: "GuildID");

            migrationBuilder.CreateIndex(
                name: "IX_StarboardMessages_StarboardSettingsID",
                table: "StarboardMessages",
                column: "StarboardSettingsID");

            migrationBuilder.CreateIndex(
                name: "IX_StarboardSettings_EmojiNameOrID",
                table: "StarboardSettings",
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
                name: "Guilds");

            migrationBuilder.DropTable(
                name: "StarboardMessages");

            migrationBuilder.DropTable(
                name: "TimezonesSettings");

            migrationBuilder.DropTable(
                name: "StarboardSettings");

            migrationBuilder.DropTable(
                name: "Emojis");
        }
    }
}
