using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KxnPhotoStudio.Migrations
{
    /// <inheritdoc />
    public partial class ProtectClientNotificationHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientNotifications_Bookings_BookingId",
                table: "ClientNotifications");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientNotifications_Bookings_BookingId",
                table: "ClientNotifications",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "BookingId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientNotifications_Bookings_BookingId",
                table: "ClientNotifications");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientNotifications_Bookings_BookingId",
                table: "ClientNotifications",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "BookingId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
