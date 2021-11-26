﻿// <auto-generated />
using System;
using Assignment3;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Assignment3.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20211125181956_first")]
    partial class first
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.10")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Assignment3.Cinema", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("City")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ID");

                    b.ToTable("Cinema");
                });

            modelBuilder.Entity("Assignment3.Movie", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("PosterPath")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("ReleaseDate")
                        .HasColumnType("datetime2");

                    b.Property<short>("Runtime")
                        .HasColumnType("smallint");

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ID");

                    b.ToTable("Movie");
                });

            modelBuilder.Entity("Assignment3.Screening", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("CinemaID")
                        .HasColumnType("int");

                    b.Property<int?>("MovieID")
                        .HasColumnType("int");

                    b.Property<TimeSpan>("Time")
                        .HasColumnType("time");

                    b.HasKey("ID");

                    b.HasIndex("CinemaID");

                    b.HasIndex("MovieID");

                    b.ToTable("Screening");
                });

            modelBuilder.Entity("Assignment3.Ticket", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("ScreeningID")
                        .HasColumnType("int");

                    b.Property<DateTime>("TimePurchased")
                        .HasColumnType("datetime2");

                    b.HasKey("ID");

                    b.HasIndex("ScreeningID");

                    b.ToTable("Ticket");
                });

            modelBuilder.Entity("Assignment3.Screening", b =>
                {
                    b.HasOne("Assignment3.Cinema", "Cinema")
                        .WithMany()
                        .HasForeignKey("CinemaID");

                    b.HasOne("Assignment3.Movie", "Movie")
                        .WithMany()
                        .HasForeignKey("MovieID");

                    b.Navigation("Cinema");

                    b.Navigation("Movie");
                });

            modelBuilder.Entity("Assignment3.Ticket", b =>
                {
                    b.HasOne("Assignment3.Screening", "Screening")
                        .WithMany()
                        .HasForeignKey("ScreeningID");

                    b.Navigation("Screening");
                });
#pragma warning restore 612, 618
        }
    }
}
