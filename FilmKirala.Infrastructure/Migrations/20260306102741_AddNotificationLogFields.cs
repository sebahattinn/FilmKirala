using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FilmKirala.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationLogFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSent",
                table: "NotificationLogs");

            migrationBuilder.AddColumn<int>(
                name: "RetryCount",
                table: "NotificationLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "NotificationLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RetryCount",
                table: "NotificationLogs");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "NotificationLogs");

            migrationBuilder.AddColumn<bool>(
                name: "IsSent",
                table: "NotificationLogs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
