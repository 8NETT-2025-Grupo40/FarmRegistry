using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmRegistry.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldCropAndBoundaryPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CropName",
                table: "Fields",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "Não informado");

            migrationBuilder.CreateTable(
                name: "FieldBoundaryPoints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FieldId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldBoundaryPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FieldBoundaryPoints_Fields_FieldId",
                        column: x => x.FieldId,
                        principalTable: "Fields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FieldBoundaryPoints_FieldId",
                table: "FieldBoundaryPoints",
                column: "FieldId");

            migrationBuilder.CreateIndex(
                name: "IX_FieldBoundaryPoints_FieldId_Sequence",
                table: "FieldBoundaryPoints",
                columns: new[] { "FieldId", "Sequence" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FieldBoundaryPoints");

            migrationBuilder.DropColumn(
                name: "CropName",
                table: "Fields");
        }
    }
}
