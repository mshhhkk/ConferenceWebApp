using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConferenceWebApp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPersonalAccountEnums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Degree",
                table: "UserProfile",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Position",
                table: "UserProfile",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Degree",
                table: "UserProfile");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "UserProfile");
        }
    }
}
