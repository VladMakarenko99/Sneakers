using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace practice.Migrations
{
    /// <inheritdoc />
    public partial class newmigration2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_Orders_OrderId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_OrderId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "Items");

            migrationBuilder.AddColumn<string>(
                name: "ItemsJson",
                table: "Orders",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ItemsJson",
                table: "Orders");

            migrationBuilder.AddColumn<Guid>(
                name: "OrderId",
                table: "Items",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Items_OrderId",
                table: "Items",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Orders_OrderId",
                table: "Items",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id");
        }
    }
}
