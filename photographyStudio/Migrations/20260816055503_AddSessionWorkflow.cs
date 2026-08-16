using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KxnPhotoStudio.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SessionWorkflows",
                columns: table => new
                {
                    SessionWorkflowId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    EditingStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EditingStartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EditingCompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveryStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GalleryUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DeliveryNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionWorkflows", x => x.SessionWorkflowId);
                    table.ForeignKey(
                        name: "FK_SessionWorkflows_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "BookingId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SessionWorkflows_BookingId",
                table: "SessionWorkflows",
                column: "BookingId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SessionWorkflows");
        }
    }
}
