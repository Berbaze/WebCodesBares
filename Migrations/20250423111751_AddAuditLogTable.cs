using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebCodesBares.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DatePause",
                table: "Licence",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EstSuspendue",
                table: "Licence",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DatePause",
                table: "Licence");

            migrationBuilder.DropColumn(
                name: "EstSuspendue",
                table: "Licence");
        }
    }
}
