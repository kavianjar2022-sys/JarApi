using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JarApi.Migrations
{
    /// <inheritdoc />
    public partial class AddEducationDegree : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EducationDegreeId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EducationDegrees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationDegrees", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_EducationDegreeId",
                table: "AspNetUsers",
                column: "EducationDegreeId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_EducationDegrees_EducationDegreeId",
                table: "AspNetUsers",
                column: "EducationDegreeId",
                principalTable: "EducationDegrees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_EducationDegrees_EducationDegreeId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "EducationDegrees");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_EducationDegreeId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EducationDegreeId",
                table: "AspNetUsers");
        }
    }
}
