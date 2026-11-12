using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceDay.API.Clean.Migrations
{
    // Adds Azure image references to the existing database tables.
    public partial class AddAzureImageReferences : Migration
    {
        // Adds the two image reference columns.
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Adds the event banner reference to the Events table.
            migrationBuilder.AddColumn<string>(
                name: "BannerImage",
                table: "Events",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            // Adds the profile picture reference to the Users table.
            migrationBuilder.AddColumn<string>(
                name: "ProfileImage",
                table: "Users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }

        // Reverses the changes if the migration is rolled back.
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Removes the event banner reference.
            migrationBuilder.DropColumn(
                name: "BannerImage",
                table: "Events");

            // Removes the profile picture reference.
            migrationBuilder.DropColumn(
                name: "ProfileImage",
                table: "Users");
        }
    }
}