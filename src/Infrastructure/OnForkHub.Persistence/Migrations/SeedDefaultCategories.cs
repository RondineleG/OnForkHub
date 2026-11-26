namespace OnForkHub.Persistence.Migrations;

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

using OnForkHub.Persistence.Contexts;

/// <summary>
/// Seeds the default category catalog so publishing can require a valid category end-to-end.
/// Ids are fixed GUIDs (N format) so the catalog stays stable across environments.
/// </summary>
[DbContext(typeof(EntityFrameworkDataContext))]
[Migration("20261126014500_SeedDefaultCategories")]
public sealed class SeedDefaultCategories : Migration
{
    /// <inheritdoc/>
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        var categories = new[]
        {
            ("10000000000000000000000000000001", "Gaming", "Gameplay, walkthroughs and esports"),
            ("10000000000000000000000000000002", "Music", "Music videos, covers and performances"),
            ("10000000000000000000000000000003", "Education", "Tutorials, courses and explanations"),
            ("10000000000000000000000000000004", "Entertainment", "Comedy, sketches and general entertainment"),
            ("10000000000000000000000000000005", "Sports", "Highlights, training and sports analysis"),
            ("10000000000000000000000000000006", "Technology", "Tech reviews, programming and gadgets"),
        };

        foreach (var (id, name, description) in categories)
        {
            migrationBuilder.Sql(
                $"IF NOT EXISTS (SELECT 1 FROM [Categories] WHERE [Id] = '{id}') "
                    + $"INSERT INTO [Categories] ([Id], [Name], [Description], [CreatedAt]) VALUES ('{id}', '{name}', '{description}', '2026-01-01T00:00:00.000');"
            );
        }
    }

    /// <inheritdoc/>
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DELETE FROM [Categories] WHERE [Id] LIKE '1000000000000000000000000000000_';");
    }
}
