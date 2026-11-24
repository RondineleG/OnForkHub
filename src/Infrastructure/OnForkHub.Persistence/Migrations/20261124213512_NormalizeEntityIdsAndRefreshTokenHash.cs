using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnForkHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeEntityIdsAndRefreshTokenHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Existing plaintext refresh tokens are incompatible with the hashed-token model; force re-login.
            migrationBuilder.Sql("DELETE FROM [RefreshTokens];");

            // Normalize legacy prefixed entity ids (e.g. 'Video/<guid>' -> '<guid>').
            // FK constraints on the VideoCategories join table must be dropped while both sides are rewritten.
            migrationBuilder.Sql("ALTER TABLE [VideoCategories] DROP CONSTRAINT [FK_VideoCategories_Categories_CategoriesId];");
            migrationBuilder.Sql("ALTER TABLE [VideoCategories] DROP CONSTRAINT [FK_VideoCategories_Videos_VideoId];");

            foreach (var table in new[] { "Users", "Videos", "Categories", "Comments", "Notifications", "VideoUploads", "UserFavorites", "VideoRatings", "ViewHistories" })
            {
                migrationBuilder.Sql(
                    $"UPDATE [{table}] SET [Id] = SUBSTRING([Id], CHARINDEX('/', [Id]) + 1, LEN([Id])) WHERE CHARINDEX('/', [Id]) > 0;"
                );
            }

            migrationBuilder.Sql(
                "UPDATE [VideoCategories] SET [CategoriesId] = SUBSTRING([CategoriesId], CHARINDEX('/', [CategoriesId]) + 1, LEN([CategoriesId])) WHERE CHARINDEX('/', [CategoriesId]) > 0;"
            );
            migrationBuilder.Sql(
                "UPDATE [VideoCategories] SET [VideoId] = SUBSTRING([VideoId], CHARINDEX('/', [VideoId]) + 1, LEN([VideoId])) WHERE CHARINDEX('/', [VideoId]) > 0;"
            );

            migrationBuilder.Sql(
                "ALTER TABLE [VideoCategories] WITH CHECK ADD CONSTRAINT [FK_VideoCategories_Categories_CategoriesId] FOREIGN KEY ([CategoriesId]) REFERENCES [Categories] ([Id]) ON DELETE CASCADE;"
            );
            migrationBuilder.Sql(
                "ALTER TABLE [VideoCategories] WITH CHECK ADD CONSTRAINT [FK_VideoCategories_Videos_VideoId] FOREIGN KEY ([VideoId]) REFERENCES [Videos] ([Id]) ON DELETE CASCADE;"
            );

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "Token",
                table: "RefreshTokens");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "User");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "RefreshTokens",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldMaxLength: 36);

            migrationBuilder.AddColumn<string>(
                name: "TokenHash",
                table: "RefreshTokens",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TokenHash",
                table: "RefreshTokens",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_TokenHash",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TokenHash",
                table: "RefreshTokens");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "RefreshTokens",
                type: "nvarchar(36)",
                maxLength: 36,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "Token",
                table: "RefreshTokens",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);
        }
    }
}
