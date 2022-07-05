using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Task1.Migrations
{
    public partial class m10 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Product_Brands_BrandName",
                table: "Product");

            migrationBuilder.DropForeignKey(
                name: "FK_Product_Stores_StoreName",
                table: "Product");

            migrationBuilder.DropForeignKey(
                name: "FK_Product_Users_UserId",
                table: "Product");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Product",
                table: "Product");

            migrationBuilder.RenameTable(
                name: "Product",
                newName: "Products");

            migrationBuilder.RenameIndex(
                name: "IX_Product_UserId",
                table: "Products",
                newName: "IX_Products_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Product_StoreName",
                table: "Products",
                newName: "IX_Products_StoreName");

            migrationBuilder.RenameIndex(
                name: "IX_Product_BrandName",
                table: "Products",
                newName: "IX_Products_BrandName");

            migrationBuilder.AddColumn<bool>(
                name: "deleted",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Products",
                table: "Products",
                column: "Id");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Users_UserId",
                table: "Products",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Brands_BrandName",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Stores_StoreName",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Users_UserId",
                table: "Products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Products",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "deleted",
                table: "Products");

            migrationBuilder.RenameTable(
                name: "Products",
                newName: "Product");

            migrationBuilder.RenameIndex(
                name: "IX_Products_UserId",
                table: "Product",
                newName: "IX_Product_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Products_StoreName",
                table: "Product",
                newName: "IX_Product_StoreName");

            migrationBuilder.RenameIndex(
                name: "IX_Products_BrandName",
                table: "Product",
                newName: "IX_Product_BrandName");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Product",
                table: "Product",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Brands_BrandName",
                table: "Product",
                column: "BrandName",
                principalTable: "Brands",
                principalColumn: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Stores_StoreName",
                table: "Product",
                column: "StoreName",
                principalTable: "Stores",
                principalColumn: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Users_UserId",
                table: "Product",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
