using Microsoft.EntityFrameworkCore.Migrations;

namespace ContentManagementSystem.Migrations
{
    public partial class AddedMaterialOut01 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MSOfficeKey",
                table: "MaterialItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WindowsKey",
                table: "MaterialItems",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MSOfficeKey",
                table: "MaterialItems");

            migrationBuilder.DropColumn(
                name: "WindowsKey",
                table: "MaterialItems");
        }
    }
}
