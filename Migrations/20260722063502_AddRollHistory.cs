using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AniDrop.Migrations
{
    /// <inheritdoc />
    public partial class AddRollHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PoolItemNavigationId",
                table: "RollHistory",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_RollHistory_PoolItemNavigationId",
                table: "RollHistory",
                column: "PoolItemNavigationId");

            migrationBuilder.CreateIndex(
                name: "IX_RollHistory_UserId",
                table: "RollHistory",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_RollHistory_PoolItems_PoolItemNavigationId",
                table: "RollHistory",
                column: "PoolItemNavigationId",
                principalTable: "PoolItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RollHistory_Users_UserId",
                table: "RollHistory",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RollHistory_PoolItems_PoolItemNavigationId",
                table: "RollHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_RollHistory_Users_UserId",
                table: "RollHistory");

            migrationBuilder.DropIndex(
                name: "IX_RollHistory_PoolItemNavigationId",
                table: "RollHistory");

            migrationBuilder.DropIndex(
                name: "IX_RollHistory_UserId",
                table: "RollHistory");

            migrationBuilder.DropColumn(
                name: "PoolItemNavigationId",
                table: "RollHistory");
        }
    }
}
