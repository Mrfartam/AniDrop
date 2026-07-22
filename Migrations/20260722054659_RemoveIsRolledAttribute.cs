using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AniDrop.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIsRolledAttribute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRolled",
                table: "PoolItems");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRolled",
                table: "PoolItems",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
