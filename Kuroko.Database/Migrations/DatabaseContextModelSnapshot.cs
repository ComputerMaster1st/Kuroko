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
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Proxies:ChangeTracking", false)
                .HasAnnotation("Proxies:CheckEquality", false)
                .HasAnnotation("Proxies:LazyLoading", true)
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Kuroko.Database.Entities.Audio.HashEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long>("IndexedHash")
                        .HasColumnType("bigint");

                    b.Property<long>("SubFingerprintId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("IndexedHash");

                    b.HasIndex("SubFingerprintId");

                    b.ToTable("SFPHashes");
                });

            modelBuilder.Entity("Kuroko.Database.Entities.Audio.SubFingerprintEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<byte[]>("OriginalPoint")
                        .HasColumnType("bytea");

                    b.Property<float>("SequenceAt")
                        .HasColumnType("real");

                    b.Property<long>("SequenceNumber")
                        .HasColumnType("bigint");

                    b.Property<int>("TrackDataId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("TrackDataId");

                    b.ToTable("SFPSubFingerprints");
                });

            modelBuilder.Entity("Kuroko.Database.Entities.Audio.TrackDataEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Artist")
                        .HasColumnType("text");

                    b.Property<double>("Length")
                        .HasColumnType("double precision");

                    b.Property<string>("Title")
                        .HasColumnType("text");

                    b.Property<string>("TrackInfoId")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("SFPTrackData");
                });

            modelBuilder.Entity("Kuroko.Database.Entities.Guild.BlackboxEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<bool>("IsEnabled")
                        .HasColumnType("boolean");

                    b.Property<bool>("SaveAttachments")
                        .HasColumnType("boolean");

                    b.Property<bool>("SyncModLog")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.HasIndex("GuildId")
                        .IsUnique();

                    b.ToTable("Blackboxes");
                });

            modelBuilder.Entity("Kuroko.Database.Entities.Guild.GuildEntity", b =>
                {
                    b.Property<decimal>("Id")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.ToTable("Guilds");
                });

            modelBuilder.Entity("Kuroko.Database.Entities.Guild.ModLogEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<bool>("DeletedMessages")
                        .HasColumnType("boolean");

                    b.Property<bool>("EditedMessages")
                        .HasColumnType("boolean");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<bool>("Join")
                        .HasColumnType("boolean");

                    b.Property<bool>("Leave")
                        .HasColumnType("boolean");

                    b.Property<decimal>("LogChannelId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.HasIndex("GuildId")
                        .IsUnique();

                    b.ToTable("GuildModLogs");
                });

            modelBuilder.Entity("Kuroko.Database.Entities.Guild.ReportHandler", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("Level")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<int?>("ReportsEntityId")
                        .HasColumnType("integer");

                    b.Property<decimal>("RoleId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.HasIndex("ReportsEntityId");

                    b.ToTable("ReportHandler");
                });

            modelBuilder.Entity("Kuroko.Database.Entities.Guild.ReportsEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<bool>("RecordMessages")
                        .HasColumnType("boolean");

                    b.Property<decimal>("ReportCategoryId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("TranscriptsChannelId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.HasIndex("GuildId")
                        .IsUnique();

                    b.ToTable("GuildReports");
                });

            modelBuilder.Entity("Kuroko.Database.Entities.Guild.RoleRequestEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.HasIndex("GuildId")
                        .IsUnique();

                    b.ToTable("GuildRoleRequests");
                });

            modelBuilder.Entity("Kuroko.Database.Entities.Guild.TicketEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int>("ReportHandlerId")
                        .HasColumnType("integer");

                    b.Property<decimal?>("ReportedMessageId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("ReportedUserId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("RulesViolated")
                        .HasColumnType("text");

                    b.Property<int>("Severity")
                        .HasColumnType("integer");

                    b.Property<string>("Subject")
                        .HasColumnType("text");

                    b.Property<decimal>("SubmitterId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("SummaryMessageId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("GuildId");

                    b.ToTable("Tickets");
                });

            modelBuilder.Entity("Kuroko.Database.Entities.Message.AttachmentEntity", b =>
                {
                    b.Property<decimal>("Id")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("Base64Bytes")
                        .HasColumnType("text");

                    b.Property<string>("FileName")
                        .HasColumnType("text");

                    b.Property<long>("FileSize")
                        .HasColumnType("bigint");

                    b.Property<decimal?>("MessageEntityId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.HasIndex("MessageEntityId");

                    b.ToTable("AttachmentEntity");
                });

            modelBuilder.Entity("Kuroko.Database.Entities.Message.EditedMessageEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Content")
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("EditedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal?>("MessageEntityId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.HasIndex("MessageEntityId");

                    b.ToTable("EditedMessageEntity");
                });

            modelBuilder.Entity("Kuroko.Database.Entities.Message.MessageEntity", b =>
                {
                    b.Property<decimal>("Id")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("Content")
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset?>("DeletedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int?>("TicketEntityId")
                        .HasColumnType("integer");

                    b.Property<decimal>("UserId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.HasIndex("GuildId");

                    b.HasIndex("TicketEntityId");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("Kuroko.Database.Entities.UlongEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int?>("ModLogEntityId")
                        .HasColumnType("integer");

                    b.Property<int?>("RoleRequestEntityId")
                        .HasColumnType("integer");

                    b.Property<decimal>("Value")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.HasIndex("ModLogEntityId");

                    b.HasIndex("RoleRequestEntityId");

                    b.ToTable("UlongEntity");
                });

            modelBuilder.Entity("Kuroko.Database.Entities.User.UserEntity", b =>
                {
                    b.Property<decimal>("Id")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Kuroko.Database.Entities.Audio.HashEntity", b =>
                {
                    b.HasOne("Kuroko.Database.Entities.Audio.SubFingerprintEntity", "SubFingerprint")
                        .WithMany("Hashes")
                        .HasForeignKey("SubFingerprintId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("SubFingerprint");
                });

            modelBuilder.Entity("Kuroko.Database.Entities.Audio.SubFingerprintEntity", b =>
                {
                    b.HasOne("Kuroko.Database.Entities.Audio.TrackDataEntity", "TrackData")
                        .WithMany("SubFingerprints")
                        .HasForeignKey("TrackDataId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TrackData");
                });

            modelBuilder.Entity("Kuroko.Database.Entities.Guild.BlackboxEntity", b =>
                {
                    b.HasOne("Kuroko.Database.Entities.Guild.GuildEntity", "Guild")
                        .WithOne("Blackbox")
                        .HasForeignKey("Kuroko.Database.Entities.Guild.BlackboxEntity", "GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Guild");
                });

            modelBuilder.Entity("Kuroko.Database.Entities.Guild.ModLogEntity", b =>
                {
                    b.HasOne("Kuroko.Database.Entities.Guild.GuildEntity", "Guild")
                        .WithOne("ModLog")
                        .HasForeignKey("Kuroko.Database.Entities.Guild.ModLogEntity", "GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Guild");
                });

            modelBuilder.Entity("Kuroko.Database.Entities.Guild.ReportHandler", b =>
                {
                    b.HasOne("Kuroko.Database.Entities.Guild.ReportsEntity", null)
                        .WithMany("ReportHandlers")
                        .HasForeignKey("ReportsEntityId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Kuroko.Database.Entities.Guild.ReportsEntity", b =>
                {
                    b.HasOne("Kuroko.Database.Entities.Guild.GuildEntity", "Guild")
                        .WithOne("Reports")
                        .HasForeignKey("Kuroko.Database.Entities.Guild.ReportsEntity", "GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Guild");
                });

            modelBuilder.Entity("Kuroko.Database.Entities.Guild.RoleRequestEntity", b =>
                {
                    b.HasOne("Kuroko.Database.Entities.Guild.GuildEntity", "Guild")
                        .WithOne("RoleRequest")
                        .HasForeignKey("Kuroko.Database.Entities.Guild.RoleRequestEntity", "GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Guild");
                });

            modelBuilder.Entity("Kuroko.Database.Entities.Guild.TicketEntity", b =>
                {
                    b.HasOne("Kuroko.Database.Entities.Guild.GuildEntity", "Guild")
                        .WithMany("Tickets")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Guild");
                });

            modelBuilder.Entity("Kuroko.Database.Entities.Message.AttachmentEntity", b =>
                {
                    b.HasOne("Kuroko.Database.Entities.Message.MessageEntity", null)
                        .WithMany("Attachments")
                        .HasForeignKey("MessageEntityId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Kuroko.Database.Entities.Message.EditedMessageEntity", b =>
                {
                    b.HasOne("Kuroko.Database.Entities.Message.MessageEntity", null)
                        .WithMany("EditedMessages")
                        .HasForeignKey("MessageEntityId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Kuroko.Database.Entities.Message.MessageEntity", b =>
                {
                    b.HasOne("Kuroko.Database.Entities.Guild.GuildEntity", "Guild")
                        .WithMany("Messages")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Kuroko.Database.Entities.Guild.TicketEntity", null)
                        .WithMany("Messages")
                        .HasForeignKey("TicketEntityId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Guild");
                });

            modelBuilder.Entity("Kuroko.Database.Entities.UlongEntity", b =>
                {
                    b.HasOne("Kuroko.Database.Entities.Guild.ModLogEntity", null)
                        .WithMany("IgnoredChannelIds")
                        .HasForeignKey("ModLogEntityId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Kuroko.Database.Entities.Guild.RoleRequestEntity", null)
                        .WithMany("RoleIds")
                        .HasForeignKey("RoleRequestEntityId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Kuroko.Database.Entities.Audio.SubFingerprintEntity", b =>
                {
                    b.Navigation("Hashes");
                });

            modelBuilder.Entity("Kuroko.Database.Entities.Audio.TrackDataEntity", b =>
                {
                    b.Navigation("SubFingerprints");
                });

            modelBuilder.Entity("Kuroko.Database.Entities.Guild.GuildEntity", b =>
                {
                    b.Navigation("Blackbox");

                    b.Navigation("Messages");

                    b.Navigation("ModLog");

                    b.Navigation("Reports");

                    b.Navigation("RoleRequest");

                    b.Navigation("Tickets");
                });

            modelBuilder.Entity("Kuroko.Database.Entities.Guild.ModLogEntity", b =>
                {
                    b.Navigation("IgnoredChannelIds");
                });

            modelBuilder.Entity("Kuroko.Database.Entities.Guild.ReportsEntity", b =>
                {
                    b.Navigation("ReportHandlers");
                });

            modelBuilder.Entity("Kuroko.Database.Entities.Guild.RoleRequestEntity", b =>
                {
                    b.Navigation("RoleIds");
                });

            modelBuilder.Entity("Kuroko.Database.Entities.Guild.TicketEntity", b =>
                {
                    b.Navigation("Messages");
                });

            modelBuilder.Entity("Kuroko.Database.Entities.Message.MessageEntity", b =>
                {
                    b.Navigation("Attachments");

                    b.Navigation("EditedMessages");
                });
#pragma warning restore 612, 618
        }
    }
}
