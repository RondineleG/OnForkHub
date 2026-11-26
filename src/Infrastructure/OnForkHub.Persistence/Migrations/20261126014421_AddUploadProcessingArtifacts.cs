using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnForkHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUploadProcessingArtifacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ThumbnailPath",
                table: "VideoUploads",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TranscodedVariants",
                table: "VideoUploads",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ThumbnailPath",
                table: "VideoUploads");

            migrationBuilder.DropColumn(
                name: "TranscodedVariants",
                table: "VideoUploads");
        }
    }
}
