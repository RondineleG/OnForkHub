using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnForkHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoViewCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ViewCount",
                table: "Videos",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Videos");
        }
    }
}
