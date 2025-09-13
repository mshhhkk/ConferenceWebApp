using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConferenceWebApp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReferalRequestedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTransferConfirmed",
                table: "Reports",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTransferRequested",
                table: "Reports",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTransferConfirmed",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "IsTransferRequested",
                table: "Reports");
        }
    }
}
