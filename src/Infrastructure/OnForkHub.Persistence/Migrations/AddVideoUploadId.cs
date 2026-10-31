namespace OnForkHub.Persistence.Migrations;

using System;

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

using OnForkHub.Persistence.Contexts;

/// <summary>
/// Adds a database-enforced source upload identifier for catalog publications.
/// </summary>
[DbContext(typeof(EntityFrameworkDataContext))]
[Migration("20261029002000_AddVideoUploadId")]
public sealed class AddVideoUploadId : Migration
{
    /// <inheritdoc/>
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "UploadId",
            table: "Videos",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Videos_UploadId",
            table: "Videos",
            column: "UploadId",
            unique: true,
            filter: "[UploadId] IS NOT NULL");
    }

    /// <inheritdoc/>
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_Videos_UploadId", table: "Videos");
        migrationBuilder.DropColumn(name: "UploadId", table: "Videos");
    }
}
