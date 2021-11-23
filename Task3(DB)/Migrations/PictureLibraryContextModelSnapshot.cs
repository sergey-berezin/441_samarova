﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Task3_DB_;

namespace Task3_DB_.Migrations
{
    [DbContext(typeof(PictureLibraryContext))]
    partial class PictureLibraryContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.12");

            modelBuilder.Entity("Task3_DB_.PictureInfoDB", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TypeId")
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("image")
                        .HasColumnType("BLOB");

                    b.Property<byte[]>("rectangle")
                        .HasColumnType("BLOB");

                    b.HasKey("Id");

                    b.HasIndex("TypeId");

                    b.ToTable("Pictures");
                });

            modelBuilder.Entity("Task3_DB_.PictureTypeDB", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("TypeName")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Types");
                });

            modelBuilder.Entity("Task3_DB_.PictureInfoDB", b =>
                {
                    b.HasOne("Task3_DB_.PictureTypeDB", "Type")
                        .WithMany("Pictures")
                        .HasForeignKey("TypeId");

                    b.Navigation("Type");
                });

            modelBuilder.Entity("Task3_DB_.PictureTypeDB", b =>
                {
                    b.Navigation("Pictures");
                });
#pragma warning restore 612, 618
        }
    }
}
