using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Task1.Migrations
{
    public partial class m4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Brands_BrandName",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Stores_StoreName",
                table: "Products");

            migrationBuilder.AlterColumn<string>(
                name: "StoreName",
                table: "Products",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "BrandName",
                table: "Products",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Brands_BrandName",
                table: "Products",
                column: "BrandName",
                principalTable: "Brands",
                principalColumn: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Stores_StoreName",
                table: "Products",
                column: "StoreName",
                principalTable: "Stores",
                principalColumn: "Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Brands_BrandName",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Stores_StoreName",
                table: "Products");

            migrationBuilder.AlterColumn<string>(
                name: "StoreName",
                table: "Products",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BrandName",
                table: "Products",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Brands_BrandName",
                table: "Products",
                column: "BrandName",
                principalTable: "Brands",
                principalColumn: "Name",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Stores_StoreName",
                table: "Products",
                column: "StoreName",
                principalTable: "Stores",
                principalColumn: "Name",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
