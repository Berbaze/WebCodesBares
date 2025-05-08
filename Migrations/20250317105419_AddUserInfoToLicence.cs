using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebCodesBares.Migrations
{
    /// <inheritdoc />
    public partial class AddUserInfoToLicence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Licence_Commande_CommandeId_Commande",
                table: "Licence");

            migrationBuilder.DropIndex(
                name: "IX_Licence_CommandeId_Commande",
                table: "Licence");

            migrationBuilder.RenameColumn(
                name: "CommandeId_Commande",
                table: "Licence",
                newName: "NombreUtilisateurs");

            migrationBuilder.AlterColumn<string>(
                name: "Cle",
                table: "Licence",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "Licence",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateEmission",
                table: "Licence",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Licence",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "NombreBarcodes",
                table: "Licence",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Prix",
                table: "Licence",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrixMaintenance",
                table: "Licence",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Licence",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "Licence",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Licence_Id_Commande",
                table: "Licence",
                column: "Id_Commande");

            migrationBuilder.AddForeignKey(
                name: "FK_Licence_Commande_Id_Commande",
                table: "Licence",
                column: "Id_Commande",
                principalTable: "Commande",
                principalColumn: "Id_Commande",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Licence_Commande_Id_Commande",
                table: "Licence");

            migrationBuilder.DropIndex(
                name: "IX_Licence_Id_Commande",
                table: "Licence");

            migrationBuilder.DropColumn(
                name: "Active",
                table: "Licence");

            migrationBuilder.DropColumn(
                name: "DateEmission",
                table: "Licence");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Licence");

            migrationBuilder.DropColumn(
                name: "NombreBarcodes",
                table: "Licence");

            migrationBuilder.DropColumn(
                name: "Prix",
                table: "Licence");

            migrationBuilder.DropColumn(
                name: "PrixMaintenance",
                table: "Licence");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Licence");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "Licence");

            migrationBuilder.RenameColumn(
                name: "NombreUtilisateurs",
                table: "Licence",
                newName: "CommandeId_Commande");

            migrationBuilder.AlterColumn<string>(
                name: "Cle",
                table: "Licence",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateIndex(
                name: "IX_Licence_CommandeId_Commande",
                table: "Licence",
                column: "CommandeId_Commande");

            migrationBuilder.AddForeignKey(
                name: "FK_Licence_Commande_CommandeId_Commande",
                table: "Licence",
                column: "CommandeId_Commande",
                principalTable: "Commande",
                principalColumn: "Id_Commande",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
