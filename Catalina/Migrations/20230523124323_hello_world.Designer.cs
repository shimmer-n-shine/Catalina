﻿// <auto-generated />
using System;
using Catalina.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Catalina.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20230523124323_hello_world")]
    partial class hello_world
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.5")
                .HasAnnotation("Proxies:ChangeTracking", false)
                .HasAnnotation("Proxies:CheckEquality", false)
                .HasAnnotation("Proxies:LazyLoading", true)
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Catalina.Database.Models.Emoji", b =>
                {
                    b.Property<string>("NameOrID")
                        .HasColumnType("varchar(255)");

                    b.Property<byte>("Type")
                        .HasColumnType("tinyint unsigned");

                    b.HasKey("NameOrID");

                    b.ToTable("Emojis");
                });

            modelBuilder.Entity("Catalina.Database.Models.Guild", b =>
                {
                    b.Property<ulong>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong?>("StarboardSettingsID")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong?>("TimezoneSettingsID")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("ID");

                    b.HasIndex("StarboardSettingsID");

                    b.HasIndex("TimezoneSettingsID");

                    b.ToTable("Guilds");
                });

            modelBuilder.Entity("Catalina.Database.Models.Message", b =>
                {
                    b.Property<ulong>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("ChannelID")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("MessageID")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong?>("StarboardMessageID")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong?>("StarboardSettingsID")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("ID");

                    b.HasIndex("StarboardSettingsID");

                    b.ToTable("StarboardMessages");
                });

            modelBuilder.Entity("Catalina.Database.Models.Response", b =>
                {
                    b.Property<ulong>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("Content")
                        .HasColumnType("longtext");

                    b.Property<ulong?>("GuildID")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("Name")
                        .HasColumnType("longtext");

                    b.Property<string>("Trigger")
                        .HasColumnType("longtext");

                    b.HasKey("ID");

                    b.HasIndex("GuildID");

                    b.ToTable("Responses");
                });

            modelBuilder.Entity("Catalina.Database.Models.Role", b =>
                {
                    b.Property<ulong>("ID")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong?>("GuildID")
                        .HasColumnType("bigint unsigned");

                    b.Property<bool>("IsAutomaticallyAdded")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("IsColourable")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("IsRenamabale")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Timezone")
                        .HasColumnType("longtext");

                    b.HasKey("ID");

                    b.HasIndex("GuildID");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("Catalina.Database.Models.StarboardSettings", b =>
                {
                    b.Property<ulong>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong?>("ChannelID")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("EmojiNameOrID")
                        .HasColumnType("varchar(255)");

                    b.Property<int>("Threshhold")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.HasIndex("EmojiNameOrID");

                    b.ToTable("StarboardSettings");
                });

            modelBuilder.Entity("Catalina.Database.Models.TimezoneSettings", b =>
                {
                    b.Property<ulong>("ID")
                        .HasColumnType("bigint unsigned");

                    b.Property<bool>("Enabled")
                        .HasColumnType("tinyint(1)");

                    b.HasKey("ID");

                    b.ToTable("TimezonesSettings");
                });

            modelBuilder.Entity("Catalina.Database.Models.Vote", b =>
                {
                    b.Property<ulong>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong?>("MessageID")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("UserId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("ID");

                    b.HasIndex("MessageID");

                    b.ToTable("StarboardVotes");
                });

            modelBuilder.Entity("Catalina.Database.Models.Guild", b =>
                {
                    b.HasOne("Catalina.Database.Models.StarboardSettings", "StarboardSettings")
                        .WithMany()
                        .HasForeignKey("StarboardSettingsID");

                    b.HasOne("Catalina.Database.Models.TimezoneSettings", "TimezoneSettings")
                        .WithMany()
                        .HasForeignKey("TimezoneSettingsID");

                    b.Navigation("StarboardSettings");

                    b.Navigation("TimezoneSettings");
                });

            modelBuilder.Entity("Catalina.Database.Models.Message", b =>
                {
                    b.HasOne("Catalina.Database.Models.StarboardSettings", null)
                        .WithMany("Messages")
                        .HasForeignKey("StarboardSettingsID");
                });

            modelBuilder.Entity("Catalina.Database.Models.Response", b =>
                {
                    b.HasOne("Catalina.Database.Models.Guild", null)
                        .WithMany("Responses")
                        .HasForeignKey("GuildID");
                });

            modelBuilder.Entity("Catalina.Database.Models.Role", b =>
                {
                    b.HasOne("Catalina.Database.Models.Guild", "Guild")
                        .WithMany("Roles")
                        .HasForeignKey("GuildID");

                    b.Navigation("Guild");
                });

            modelBuilder.Entity("Catalina.Database.Models.StarboardSettings", b =>
                {
                    b.HasOne("Catalina.Database.Models.Emoji", "Emoji")
                        .WithMany()
                        .HasForeignKey("EmojiNameOrID");

                    b.Navigation("Emoji");
                });

            modelBuilder.Entity("Catalina.Database.Models.Vote", b =>
                {
                    b.HasOne("Catalina.Database.Models.Message", null)
                        .WithMany("Votes")
                        .HasForeignKey("MessageID");
                });

            modelBuilder.Entity("Catalina.Database.Models.Guild", b =>
                {
                    b.Navigation("Responses");

                    b.Navigation("Roles");
                });

            modelBuilder.Entity("Catalina.Database.Models.Message", b =>
                {
                    b.Navigation("Votes");
                });

            modelBuilder.Entity("Catalina.Database.Models.StarboardSettings", b =>
                {
                    b.Navigation("Messages");
                });
#pragma warning restore 612, 618
        }
    }
}
