﻿// <auto-generated />
using ERNI.PhotoDatabase.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;

namespace ERNI.PhotoDatabase.DataAccess.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("ERNI.PhotoDatabase.DataAccess.DomainModel.Photo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("FullSizeImageId");

                    b.Property<int>("Height");

                    b.Property<string>("Mime");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(60);

                    b.Property<Guid>("ThumbnailImageId");

                    b.Property<int>("Width");

                    b.HasKey("Id");

                    b.HasIndex("FullSizeImageId")
                        .IsUnique();

                    b.HasIndex("ThumbnailImageId")
                        .IsUnique();

                    b.ToTable("Photos");
                });

            modelBuilder.Entity("ERNI.PhotoDatabase.DataAccess.DomainModel.PhotoTag", b =>
                {
                    b.Property<int>("PhotoId");

                    b.Property<int>("TagId");

                    b.HasKey("PhotoId", "TagId");

                    b.HasIndex("TagId");

                    b.ToTable("PhotoTag");
                });

            modelBuilder.Entity("ERNI.PhotoDatabase.DataAccess.DomainModel.Tag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Text")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("Text")
                        .IsUnique();

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("ERNI.PhotoDatabase.DataAccess.DomainModel.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("CanUpload");

                    b.Property<string>("FirstName");

                    b.Property<bool>("IsAdmin");

                    b.Property<string>("LastName");

                    b.Property<string>("UniqueIdentifier");

                    b.Property<string>("UserName");

                    b.HasKey("Id");

                    b.HasIndex("UniqueIdentifier")
                        .IsUnique()
                        .HasFilter("[UniqueIdentifier] IS NOT NULL");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("ERNI.PhotoDatabase.DataAccess.DomainModel.PhotoTag", b =>
                {
                    b.HasOne("ERNI.PhotoDatabase.DataAccess.DomainModel.Photo", "Photo")
                        .WithMany("PhotoTags")
                        .HasForeignKey("PhotoId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("ERNI.PhotoDatabase.DataAccess.DomainModel.Tag", "Tag")
                        .WithMany("PhotoTags")
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
