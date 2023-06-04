using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rinkudesu.Services.Links.Migrations
{
    /// <inheritdoc />
    public partial class StoreSearchableLinkUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SearchableUrl",
                table: "Links",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                computedColumnSql: "upper(regexp_replace(\"LinkUrl\", '^https?:\\/\\/', ''))",
                stored: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SearchableUrl",
                table: "Links");
        }
    }
}
