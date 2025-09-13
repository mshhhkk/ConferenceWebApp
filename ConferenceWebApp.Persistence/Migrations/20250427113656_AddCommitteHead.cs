using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConferenceWebApp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCommitteHead : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHead",
                table: "Committee",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHead",
                table: "Committee");
        }
    }
}