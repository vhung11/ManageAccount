using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManageAccount.Migrations
{
    /// <inheritdoc />
    public partial class AlterTableAppLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LEVEL",
                table: "APP_LOGS",
                newName: "LOG_LEVEL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LOG_LEVEL",
                table: "APP_LOGS",
                newName: "LEVEL");
        }
    }
}
