using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AniDrop.Migrations
{
    /// <inheritdoc />
    public partial class RenameRollHistoryPoolItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RollHistory_PoolItems_PoolId",
                table: "RollHistory");

            migrationBuilder.RenameColumn(
                name: "PoolId",
                table: "RollHistory",
                newName: "PoolAnimeItemId");

            migrationBuilder.RenameIndex(
                name: "IX_RollHistory_PoolId",
                table: "RollHistory",
                newName: "IX_RollHistory_PoolAnimeItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_RollHistory_PoolItems_PoolAnimeItemId",
                table: "RollHistory",
                column: "PoolAnimeItemId",
                principalTable: "PoolItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RollHistory_PoolItems_PoolAnimeItemId",
                table: "RollHistory");

            migrationBuilder.RenameColumn(
                name: "PoolAnimeItemId",
                table: "RollHistory",
                newName: "PoolId");

            migrationBuilder.RenameIndex(
                name: "IX_RollHistory_PoolAnimeItemId",
                table: "RollHistory",
                newName: "IX_RollHistory_PoolId");

            migrationBuilder.AddForeignKey(
                name: "FK_RollHistory_PoolItems_PoolId",
                table: "RollHistory",
                column: "PoolId",
                principalTable: "PoolItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
