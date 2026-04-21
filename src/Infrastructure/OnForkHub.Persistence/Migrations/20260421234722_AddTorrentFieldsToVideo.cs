using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnForkHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTorrentFieldsToVideo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTorrentEnabled",
                table: "Videos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MagnetUri",
                table: "Videos",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "VideoUploads",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StoragePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ProgressPercentage = table.Column<int>(type: "int", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TotalChunks = table.Column<int>(type: "int", nullable: false),
                    ReceivedChunks = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoUploads", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VideoUploads_Status",
                table: "VideoUploads",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_VideoUploads_UserId",
                table: "VideoUploads",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VideoUploads");

            migrationBuilder.DropColumn(
                name: "IsTorrentEnabled",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "MagnetUri",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Users");
        }
    }
}
