using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AniDrop.Migrations
{
    /// <inheritdoc />
    public partial class RemoveExtraFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RollHistory_PoolItems_PoolAnimeItemId",
                table: "RollHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_RollHistory_Users_UserId",
                table: "RollHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_TitlePools_Users_UserId",
                table: "TitlePools");

            migrationBuilder.DropIndex(
                name: "IX_TitlePools_UserId",
                table: "TitlePools");

            migrationBuilder.DropIndex(
                name: "IX_RollHistory_PoolAnimeItemId",
                table: "RollHistory");

            migrationBuilder.DropIndex(
                name: "IX_RollHistory_UserId",
                table: "RollHistory");

            migrationBuilder.DropColumn(
                name: "PoolAnimeItemId",
                table: "RollHistory");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PoolAnimeItemId",
                table: "RollHistory",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_TitlePools_UserId",
                table: "TitlePools",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RollHistory_PoolAnimeItemId",
                table: "RollHistory",
                column: "PoolAnimeItemId");

            migrationBuilder.CreateIndex(
                name: "IX_RollHistory_UserId",
                table: "RollHistory",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_RollHistory_PoolItems_PoolAnimeItemId",
                table: "RollHistory",
                column: "PoolAnimeItemId",
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

            migrationBuilder.AddForeignKey(
                name: "FK_TitlePools_Users_UserId",
                table: "TitlePools",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
