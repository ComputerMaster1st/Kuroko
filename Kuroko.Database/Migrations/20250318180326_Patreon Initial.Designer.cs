﻿// <auto-generated />
using System;
using Kuroko.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Kuroko.Database.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20250318180326_Patreon Initial")]
    partial class PatreonInitial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.3")
                .HasAnnotation("Proxies:ChangeTracking", false)
                .HasAnnotation("Proxies:CheckEquality", false)
                .HasAnnotation("Proxies:LazyLoading", true)
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Kuroko.Database.GuildEntities.BanSyncGuildProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<bool>("AllowRequests")
                        .HasColumnType("boolean");

                    b.Property<decimal>("BanSyncChannelId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int>("ClientMode")
                        .HasColumnType("integer");

                    b.Property<int>("HostMode")
                        .HasColumnType("integer");

                    b.Property<bool>("IsEnabled")
                        .HasColumnType("boolean");

                    b.Property<decimal>("RootId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<Guid>("SyncId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("RootId")
                        .IsUnique();

                    b.ToTable("BanSyncProperties");
                });

            modelBuilder.Entity("Kuroko.Database.GuildEntities.Extras.BanSyncProfile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<Guid>("ClientSyncId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("HostSyncId")
                        .HasColumnType("uuid");

                    b.Property<int>("Mode")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("ClientSyncId");

                    b.HasIndex("HostSyncId");

                    b.ToTable("BanSyncProfiles");
                });

            modelBuilder.Entity("Kuroko.Database.GuildEntities.GuildEntity", b =>
                {
                    b.Property<decimal>("Id")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.ToTable("Guilds");
                });

            modelBuilder.Entity("Kuroko.Database.UserEntities.Extras.PremiumKey", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset>("ExpiresAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("Key")
                        .HasColumnType("text");

                    b.Property<int>("PatreonPropertiesId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("GuildId")
                        .IsUnique();

                    b.HasIndex("PatreonPropertiesId");

                    b.ToTable("PremiumKey");
                });

            modelBuilder.Entity("Kuroko.Database.UserEntities.PatreonProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<decimal>("RootId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.HasIndex("RootId")
                        .IsUnique();

                    b.ToTable("PatreonProperties");
                });

            modelBuilder.Entity("Kuroko.Database.UserEntities.UserEntity", b =>
                {
                    b.Property<decimal>("Id")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.ToTable("UserEntity");
                });

            modelBuilder.Entity("Kuroko.Database.GuildEntities.BanSyncGuildProperties", b =>
                {
                    b.HasOne("Kuroko.Database.GuildEntities.GuildEntity", "Guild")
                        .WithOne("BanSyncGuildProperties")
                        .HasForeignKey("Kuroko.Database.GuildEntities.BanSyncGuildProperties", "RootId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Guild");
                });

            modelBuilder.Entity("Kuroko.Database.GuildEntities.Extras.BanSyncProfile", b =>
                {
                    b.HasOne("Kuroko.Database.GuildEntities.BanSyncGuildProperties", "ClientGuildProperties")
                        .WithMany("ClientOfProfiles")
                        .HasForeignKey("ClientSyncId")
                        .HasPrincipalKey("SyncId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Kuroko.Database.GuildEntities.BanSyncGuildProperties", "HostGuildProperties")
                        .WithMany("HostForProfiles")
                        .HasForeignKey("HostSyncId")
                        .HasPrincipalKey("SyncId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ClientGuildProperties");

                    b.Navigation("HostGuildProperties");
                });

            modelBuilder.Entity("Kuroko.Database.UserEntities.Extras.PremiumKey", b =>
                {
                    b.HasOne("Kuroko.Database.GuildEntities.GuildEntity", "Guild")
                        .WithOne("PremiumKey")
                        .HasForeignKey("Kuroko.Database.UserEntities.Extras.PremiumKey", "GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Kuroko.Database.UserEntities.PatreonProperties", "PatreonProperties")
                        .WithMany("PremiumKeys")
                        .HasForeignKey("PatreonPropertiesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Guild");

                    b.Navigation("PatreonProperties");
                });

            modelBuilder.Entity("Kuroko.Database.UserEntities.PatreonProperties", b =>
                {
                    b.HasOne("Kuroko.Database.UserEntities.UserEntity", "User")
                        .WithOne("Patreon")
                        .HasForeignKey("Kuroko.Database.UserEntities.PatreonProperties", "RootId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Kuroko.Database.GuildEntities.BanSyncGuildProperties", b =>
                {
                    b.Navigation("ClientOfProfiles");

                    b.Navigation("HostForProfiles");
                });

            modelBuilder.Entity("Kuroko.Database.GuildEntities.GuildEntity", b =>
                {
                    b.Navigation("BanSyncGuildProperties");

                    b.Navigation("PremiumKey");
                });

            modelBuilder.Entity("Kuroko.Database.UserEntities.PatreonProperties", b =>
                {
                    b.Navigation("PremiumKeys");
                });

            modelBuilder.Entity("Kuroko.Database.UserEntities.UserEntity", b =>
                {
                    b.Navigation("Patreon");
                });
#pragma warning restore 612, 618
        }
    }
}
