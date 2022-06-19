using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LostAndFound.Data.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserPosts",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    PhoneNo = table.Column<string>(nullable: true),
                    Comment = table.Column<string>(nullable: true),
                    OTP = table.Column<string>(nullable: true),
                    IsVerified = table.Column<bool>(nullable: false),
                    PostType = table.Column<string>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPosts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    UserPostId = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    IsProcessed = table.Column<bool>(nullable: false),
                    LastSearched = table.Column<DateTime>(nullable: false),
                    Status = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Images_UserPosts_UserPostId",
                        column: x => x.UserPostId,
                        principalTable: "UserPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SimilarImages",
                columns: table => new
                {
                    LostImageId = table.Column<string>(nullable: true),
                    FoundImageId = table.Column<string>(nullable: true),
                    Accuracy = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_SimilarImages_Images_FoundImageId",
                        column: x => x.FoundImageId,
                        principalTable: "Images",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SimilarImages_Images_LostImageId",
                        column: x => x.LostImageId,
                        principalTable: "Images",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Images_UserPostId",
                table: "Images",
                column: "UserPostId");

            migrationBuilder.CreateIndex(
                name: "IX_SimilarImages_FoundImageId",
                table: "SimilarImages",
                column: "FoundImageId");

            migrationBuilder.CreateIndex(
                name: "IX_SimilarImages_LostImageId",
                table: "SimilarImages",
                column: "LostImageId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SimilarImages");

            migrationBuilder.DropTable(
                name: "Images");

            migrationBuilder.DropTable(
                name: "UserPosts");
        }
    }
}
