using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LostAndFound.Data.Migrations
{
    public partial class AddedUnlockingFields2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnlockOTPAt",
                table: "UserPosts");

            migrationBuilder.AddColumn<DateTime>(
                name: "UnlockOTPCreatedAt",
                table: "UserPosts",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnlockOTPCreatedAt",
                table: "UserPosts");

            migrationBuilder.AddColumn<DateTime>(
                name: "UnlockOTPAt",
                table: "UserPosts",
                type: "datetime2",
                nullable: true);
        }
    }
}
