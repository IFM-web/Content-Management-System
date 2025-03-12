using Microsoft.EntityFrameworkCore.Migrations;

namespace ContentManagementSystem.Migrations
{
    public partial class ReplaceCapacityWithProcessor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CPUCapacity",
                table: "MaterialItems");

            migrationBuilder.AlterColumn<string>(
                name: "SerialNo",
                table: "MaterialItems",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Processor",
                table: "MaterialItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaterialItems_SerialNo",
                table: "MaterialItems",
                column: "SerialNo",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MaterialItems_SerialNo",
                table: "MaterialItems");

            migrationBuilder.DropColumn(
                name: "Processor",
                table: "MaterialItems");

            migrationBuilder.AlterColumn<string>(
                name: "SerialNo",
                table: "MaterialItems",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<float>(
                name: "CPUCapacity",
                table: "MaterialItems",
                type: "real",
                nullable: true);
        }
    }
}
