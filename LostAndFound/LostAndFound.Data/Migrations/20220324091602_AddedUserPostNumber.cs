using Microsoft.EntityFrameworkCore.Migrations;

namespace LostAndFound.Data.Migrations
{
    public partial class AddedUserPostNumber : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserPostNumber",
                table: "UserPosts",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserPostNumber",
                table: "UserPosts");
        }
    }
}
