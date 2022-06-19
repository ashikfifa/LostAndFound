using Microsoft.EntityFrameworkCore.Migrations;

namespace LostAndFound.Data.Migrations
{
    public partial class AddedIsFaceDetectedColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFaceDetected",
                table: "Images",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFaceDetected",
                table: "Images");
        }
    }
}
