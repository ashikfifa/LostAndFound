// <auto-generated />
using System;
using LostAndFound.Data.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LostAndFound.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20220323084401_AddedIdOnSimilarImage")]
    partial class AddedIdOnSimilarImage
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.23")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("LostAndFound.Data.Models.Image", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("IsFaceDetected")
                        .HasColumnType("bit");

                    b.Property<bool>("IsProcessed")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("LastSearched")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Status")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserPostId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("UserPostId");

                    b.ToTable("Images");
                });

            modelBuilder.Entity("LostAndFound.Data.Models.SimilarImage", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<double>("Accuracy")
                        .HasColumnType("float");

                    b.Property<string>("FoundImageId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("LostImageId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("FoundImageId");

                    b.HasIndex("LostImageId");

                    b.ToTable("SimilarImages");
                });

            modelBuilder.Entity("LostAndFound.Data.Models.UserPost", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Comment")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsMatched")
                        .HasColumnType("bit");

                    b.Property<bool>("IsVerified")
                        .HasColumnType("bit");

                    b.Property<string>("OTP")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("OTPCreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("PhoneNo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PostType")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("UserPosts");
                });

            modelBuilder.Entity("LostAndFound.Data.Models.Image", b =>
                {
                    b.HasOne("LostAndFound.Data.Models.UserPost", "UserPost")
                        .WithMany()
                        .HasForeignKey("UserPostId");
                });

            modelBuilder.Entity("LostAndFound.Data.Models.SimilarImage", b =>
                {
                    b.HasOne("LostAndFound.Data.Models.Image", "FoundImage")
                        .WithMany()
                        .HasForeignKey("FoundImageId");

                    b.HasOne("LostAndFound.Data.Models.Image", "LostImage")
                        .WithMany()
                        .HasForeignKey("LostImageId");
                });
#pragma warning restore 612, 618
        }
    }
}
