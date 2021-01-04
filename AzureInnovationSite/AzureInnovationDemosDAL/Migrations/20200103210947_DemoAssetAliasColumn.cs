using Microsoft.EntityFrameworkCore.Migrations;

namespace AzureInnovationDemosDAL.Migrations
{
    public partial class DemoAssetAliasColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Alias",
                table: "DemoAssets",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Alias",
                table: "DemoAssets");
        }
    }
}
