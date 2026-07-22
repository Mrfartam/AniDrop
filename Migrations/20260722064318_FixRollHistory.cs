using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AniDrop.Migrations
{
    /// <inheritdoc />
    public partial class FixRollHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RollHistory_PoolItems_PoolItemNavigationId",
                table: "RollHistory");

            migrationBuilder.DropIndex(
                name: "IX_RollHistory_PoolItemNavigationId",
                table: "RollHistory");

            migrationBuilder.DropColumn(
                name: "PoolItemNavigationId",
                table: "RollHistory");

            migrationBuilder.CreateIndex(
                name: "IX_RollHistory_PoolItem",
                table: "RollHistory",
                column: "PoolItem");

            migrationBuilder.AddForeignKey(
                name: "FK_RollHistory_PoolItems_PoolItem",
                table: "RollHistory",
                column: "PoolItem",
                principalTable: "PoolItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RollHistory_PoolItems_PoolItem",
                table: "RollHistory");

            migrationBuilder.DropIndex(
                name: "IX_RollHistory_PoolItem",
                table: "RollHistory");

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

            migrationBuilder.AddForeignKey(
                name: "FK_RollHistory_PoolItems_PoolItemNavigationId",
                table: "RollHistory",
                column: "PoolItemNavigationId",
                principalTable: "PoolItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
