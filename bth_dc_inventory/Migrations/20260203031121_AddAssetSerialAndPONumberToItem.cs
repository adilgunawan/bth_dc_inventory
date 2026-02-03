using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bth_dc_inventory.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetSerialAndPONumberToItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssetNumber",
                table: "Items",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SerialNumber",
                table: "Items",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssetNumber",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "SerialNumber",
                table: "Items");
        }
    }
}
