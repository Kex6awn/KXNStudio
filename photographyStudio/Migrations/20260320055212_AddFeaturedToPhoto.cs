using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KxnPhotoStudio.Migrations
{
    /// <inheritdoc />
    public partial class AddFeaturedToPhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "Photos",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "Photos");
        }
    }
}
