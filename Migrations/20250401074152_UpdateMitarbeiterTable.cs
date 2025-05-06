using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebCodesBares.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMitarbeiterTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Adresse",
                table: "Mitarbeiter");

            migrationBuilder.DropColumn(
                name: "DateInscription",
                table: "Mitarbeiter");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Mitarbeiter");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Mitarbeiter");

            migrationBuilder.DropColumn(
                name: "Telephone",
                table: "Mitarbeiter");

            migrationBuilder.RenameColumn(
                name: "Id_Mitarbeiter",
                table: "Mitarbeiter",
                newName: "Id");

            migrationBuilder.AddColumn<string>(
                name: "AdminId",
                table: "Mitarbeiter",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "LicenceId",
                table: "Mitarbeiter",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Mitarbeiter",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "BarcodesRestants",
                table: "Licence",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Mitarbeiter_AdminId",
                table: "Mitarbeiter",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Mitarbeiter_LicenceId",
                table: "Mitarbeiter",
                column: "LicenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Mitarbeiter_UserId",
                table: "Mitarbeiter",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Mitarbeiter_AspNetUsers_AdminId",
                table: "Mitarbeiter",
                column: "AdminId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Mitarbeiter_AspNetUsers_UserId",
                table: "Mitarbeiter",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Mitarbeiter_Licence_LicenceId",
                table: "Mitarbeiter",
                column: "LicenceId",
                principalTable: "Licence",
                principalColumn: "Id_Licence",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mitarbeiter_AspNetUsers_AdminId",
                table: "Mitarbeiter");

            migrationBuilder.DropForeignKey(
                name: "FK_Mitarbeiter_AspNetUsers_UserId",
                table: "Mitarbeiter");

            migrationBuilder.DropForeignKey(
                name: "FK_Mitarbeiter_Licence_LicenceId",
                table: "Mitarbeiter");

            migrationBuilder.DropIndex(
                name: "IX_Mitarbeiter_AdminId",
                table: "Mitarbeiter");

            migrationBuilder.DropIndex(
                name: "IX_Mitarbeiter_LicenceId",
                table: "Mitarbeiter");

            migrationBuilder.DropIndex(
                name: "IX_Mitarbeiter_UserId",
                table: "Mitarbeiter");

            migrationBuilder.DropColumn(
                name: "AdminId",
                table: "Mitarbeiter");

            migrationBuilder.DropColumn(
                name: "LicenceId",
                table: "Mitarbeiter");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Mitarbeiter");

            migrationBuilder.DropColumn(
                name: "BarcodesRestants",
                table: "Licence");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Mitarbeiter",
                newName: "Id_Mitarbeiter");

            migrationBuilder.AddColumn<string>(
                name: "Adresse",
                table: "Mitarbeiter",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateInscription",
                table: "Mitarbeiter",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Mitarbeiter",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Mitarbeiter",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Telephone",
                table: "Mitarbeiter",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
