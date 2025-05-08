using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebCodesBares.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLicenceRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Licence",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Licence_UserId",
                table: "Licence",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Licence_AspNetUsers_UserId",
                table: "Licence",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Licence_AspNetUsers_UserId",
                table: "Licence");

            migrationBuilder.DropIndex(
                name: "IX_Licence_UserId",
                table: "Licence");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Licence");
        }
    }
}
