using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmRegistry.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFarmOwnerId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "Farms",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE Farms SET OwnerId = '11111111-1111-1111-1111-111111111111' WHERE OwnerId IS NULL");

            migrationBuilder.AlterColumn<Guid>(
                name: "OwnerId",
                table: "Farms",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Farms_OwnerId",
                table: "Farms",
                column: "OwnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Farms_OwnerId",
                table: "Farms");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Farms");
        }
    }
}
