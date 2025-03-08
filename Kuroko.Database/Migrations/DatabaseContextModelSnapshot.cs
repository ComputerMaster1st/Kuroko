﻿// <auto-generated />
using System;
using Kuroko.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Kuroko.Database.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.2")
                .HasAnnotation("Proxies:ChangeTracking", false)
                .HasAnnotation("Proxies:CheckEquality", false)
                .HasAnnotation("Proxies:LazyLoading", true)
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Kuroko.Database.GuildEntities.BanSyncProperties", b =>
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

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int>("HostMode")
                        .HasColumnType("integer");

                    b.Property<bool>("IsEnabled")
                        .HasColumnType("boolean");

                    b.Property<Guid>("SyncId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("GuildId")
                        .IsUnique();

                    b.ToTable("BanSyncProperties");
                });

            modelBuilder.Entity("Kuroko.Database.GuildEntities.Extras.BanSyncProfile", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer");

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

            modelBuilder.Entity("Kuroko.Database.GuildEntities.BanSyncProperties", b =>
                {
                    b.HasOne("Kuroko.Database.GuildEntities.GuildEntity", "Guild")
                        .WithOne("BanSyncProperties")
                        .HasForeignKey("Kuroko.Database.GuildEntities.BanSyncProperties", "GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Guild");
                });

            modelBuilder.Entity("Kuroko.Database.GuildEntities.Extras.BanSyncProfile", b =>
                {
                    b.HasOne("Kuroko.Database.GuildEntities.BanSyncProperties", "ClientProperties")
                        .WithMany("ClientOfProfiles")
                        .HasForeignKey("ClientSyncId")
                        .HasPrincipalKey("SyncId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Kuroko.Database.GuildEntities.BanSyncProperties", "HostProperties")
                        .WithMany("HostForProfiles")
                        .HasForeignKey("HostSyncId")
                        .HasPrincipalKey("SyncId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ClientProperties");

                    b.Navigation("HostProperties");
                });

            modelBuilder.Entity("Kuroko.Database.GuildEntities.BanSyncProperties", b =>
                {
                    b.Navigation("ClientOfProfiles");

                    b.Navigation("HostForProfiles");
                });

            modelBuilder.Entity("Kuroko.Database.GuildEntities.GuildEntity", b =>
                {
                    b.Navigation("BanSyncProperties");
                });
#pragma warning restore 612, 618
        }
    }
}
