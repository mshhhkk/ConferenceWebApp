using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConferenceWebApp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRejectionCommentInReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RejectionComment",
                table: "Reports",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RejectionComment",
                table: "Reports");
        }
    }
}
