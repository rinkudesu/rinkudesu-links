using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rinkudesu.Services.Links.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueUrlIndexForUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Links_LinkUrl_CreatingUserId",
                table: "Links",
                columns: new[] { "LinkUrl", "CreatingUserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Links_LinkUrl_CreatingUserId",
                table: "Links");
        }
    }
}
