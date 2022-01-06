using Microsoft.EntityFrameworkCore.Migrations;
#pragma warning disable 1591

#nullable disable

namespace Rinkudesu.Services.Links.Migrations
{
    public partial class AddShareableKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShareableKey",
                table: "Links",
                type: "character varying(51)",
                maxLength: 51,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShareableKey",
                table: "Links");
        }
    }
}
