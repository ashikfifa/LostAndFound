using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LostAndFound.Data.Migrations
{
    public partial class AddedUnlockingFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsUnlocked",
                table: "UserPosts",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UnlockOTP",
                table: "UserPosts",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UnlockOTPAt",
                table: "UserPosts",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsUnlocked",
                table: "UserPosts");

            migrationBuilder.DropColumn(
                name: "UnlockOTP",
                table: "UserPosts");

            migrationBuilder.DropColumn(
                name: "UnlockOTPAt",
                table: "UserPosts");
        }
    }
}
