using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Sabrina.Models
{
    public partial class DiscordContext : DbContext
    {
        public DiscordContext()
        {
        }

        public DiscordContext(DbContextOptions<DiscordContext> options)
            : base(options)
        {
        }

        public virtual DbSet<DungeonText> DungeonText { get; set; }
        public virtual DbSet<DungeonVariablesConnection> DungeonVariablesConnection { get; set; }
        public virtual DbSet<DungeonVariablesVariables> DungeonVariablesVariables { get; set; }
        public virtual DbSet<KinkHashes> KinkHashes { get; set; }
        public virtual DbSet<Messages> Messages { get; set; }
        public virtual DbSet<PornhubVideos> PornhubVideos { get; set; }
        public virtual DbSet<Puns> Puns { get; set; }
        public virtual DbSet<SabrinaSettings> SabrinaSettings { get; set; }
        public virtual DbSet<Slavereports> Slavereports { get; set; }
        public virtual DbSet<TumblrPosts> TumblrPosts { get; set; }
        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<UserSettings> UserSettings { get; set; }
        public virtual DbSet<WheelChances> WheelChances { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=joidb.ddns.net;Database=Discord;user id=DiscordUser;password=zkCrMGv3uGK8P7Tr");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DungeonText>(entity =>
            {
                entity.ToTable("Dungeon.Text");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.RoomType)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.Text)
                    .IsRequired()
                    .HasColumnType("ntext");

                entity.Property(e => e.TextType)
                    .IsRequired()
                    .HasMaxLength(20);
            });

            modelBuilder.Entity<DungeonVariablesConnection>(entity =>
            {
                entity.ToTable("Dungeon.Variables.Connection");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.TextId).HasColumnName("TextID");

                entity.Property(e => e.VariableId).HasColumnName("VariableID");

                entity.HasOne(d => d.Variable)
                    .WithMany(p => p.DungeonVariablesConnection)
                    .HasForeignKey(d => d.VariableId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Dungeon.Variables.Connection_Dungeon.Text");
            });

            modelBuilder.Entity<DungeonVariablesVariables>(entity =>
            {
                entity.ToTable("Dungeon.Variables.Variables");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<KinkHashes>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.Property(e => e.UserId)
                    .HasColumnName("UserID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Hash)
                    .IsRequired()
                    .HasColumnType("ntext");

                entity.HasOne(d => d.User)
                    .WithOne(p => p.KinkHashes)
                    .HasForeignKey<KinkHashes>(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_KinkHashes_Users");
            });

            modelBuilder.Entity<Messages>(entity =>
            {
                entity.HasKey(e => e.MessageId);

                entity.Property(e => e.MessageId).HasColumnName("MessageID");

                entity.Property(e => e.AuthorId).HasColumnName("AuthorID");

                entity.Property(e => e.ChannelId).HasColumnName("ChannelID");

                entity.Property(e => e.CreationDate).HasColumnType("datetime");

                entity.Property(e => e.MessageText).IsRequired();

                entity.HasOne(d => d.Author)
                    .WithMany(p => p.Messages)
                    .HasForeignKey(d => d.AuthorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Messages_Users");
            });

            modelBuilder.Entity<PornhubVideos>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Creator).IsRequired();

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.ImageUrl).IsRequired();

                entity.Property(e => e.Title).IsRequired();

                entity.Property(e => e.Url)
                    .IsRequired()
                    .HasColumnName("URL");
            });

            modelBuilder.Entity<Puns>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.LastUsed).HasColumnType("datetime");

                entity.Property(e => e.Text).IsRequired();
            });

            modelBuilder.Entity<SabrinaSettings>(entity =>
            {
                entity.HasKey(e => e.GuildId);

                entity.Property(e => e.GuildId)
                    .HasColumnName("GuildID")
                    .ValueGeneratedNever();

                entity.Property(e => e.LastIntroductionPost).HasColumnType("datetime");

                entity.Property(e => e.LastTumblrPost).HasColumnType("datetime");

                entity.Property(e => e.LastTumblrUpdate).HasColumnType("datetime");
            });

            modelBuilder.Entity<Slavereports>(entity =>
            {
                entity.HasKey(e => e.SlaveReportId);

                entity.Property(e => e.SlaveReportId).HasColumnName("SlaveReportID");

                entity.Property(e => e.SessionOutcome)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.TimeOfReport).HasColumnType("datetime");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Slavereports)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Slavereports_Users");
            });

            modelBuilder.Entity<TumblrPosts>(entity =>
            {
                entity.HasKey(e => e.TumblrId);

                entity.Property(e => e.TumblrId)
                    .HasColumnName("TumblrID")
                    .ValueGeneratedNever();

                entity.Property(e => e.LastPosted).HasColumnType("datetime");
            });

            modelBuilder.Entity<Users>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.Property(e => e.UserId)
                    .HasColumnName("UserID")
                    .ValueGeneratedNever();

                entity.Property(e => e.BanTime).HasColumnType("datetime");

                entity.Property(e => e.DenialTime).HasColumnType("datetime");

                entity.Property(e => e.LockTime).HasColumnType("datetime");

                entity.Property(e => e.MuteTime).HasColumnType("datetime");

                entity.Property(e => e.RuinTime).HasColumnType("datetime");

                entity.Property(e => e.SpecialTime).HasColumnType("datetime");
            });

            modelBuilder.Entity<UserSettings>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.Property(e => e.UserId)
                    .HasColumnName("UserID")
                    .ValueGeneratedNever();

                entity.HasOne(d => d.User)
                    .WithOne(p => p.UserSettings)
                    .HasForeignKey<UserSettings>(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserSettings_Users");
            });

            modelBuilder.Entity<WheelChances>(entity =>
            {
                entity.HasKey(e => e.Difficulty);

                entity.Property(e => e.Difficulty).ValueGeneratedNever();
            });
        }
    }
}
