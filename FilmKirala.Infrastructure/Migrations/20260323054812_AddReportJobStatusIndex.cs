using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FilmKirala.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReportJobStatusIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "JobId",
                table: "ReportJobs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_ReportJobs_Status_CreatedAt",
                table: "ReportJobs",
                columns: new[] { "Status", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReportJobs_Status_CreatedAt",
                table: "ReportJobs");

            migrationBuilder.AlterColumn<string>(
                name: "JobId",
                table: "ReportJobs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }
    }
}
