using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManageAccount.Migrations
{
    /// <inheritdoc />
    public partial class CreateTableApp_Logs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "APP_LOGS",
                columns: table => new
                {
                    ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    LOGGED_AT = table.Column<DateTime>(type: "TIMESTAMP(6)", nullable: false, defaultValueSql: "SYSTIMESTAMP"),
                    LEVEL = table.Column<string>(type: "NVARCHAR2(16)", maxLength: 16, nullable: false),
                    LOGGER = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: true),
                    MESSAGE = table.Column<string>(type: "CLOB", nullable: true),
                    EXCEPTION = table.Column<string>(type: "CLOB", nullable: true),
                    PROPERTIES = table.Column<string>(type: "CLOB", nullable: true),
                    MACHINE_NAME = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: true),
                    APP_NAME = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APP_LOGS", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_APP_LOGS_LEVEL_LOGGED_AT",
                table: "APP_LOGS",
                columns: new[] { "LEVEL", "LOGGED_AT" });

            migrationBuilder.CreateIndex(
                name: "IX_APP_LOGS_LOGGED_AT",
                table: "APP_LOGS",
                column: "LOGGED_AT");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "APP_LOGS");
        }
    }
}
