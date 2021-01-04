using Microsoft.EntityFrameworkCore.Migrations;

namespace AzureInnovationDemosDAL.Migrations
{
    public partial class EnvironmentUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EnvironmentDescription",
                table: "DemoUserEnvironments",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnvironmentProvisioned",
                table: "DemoUserEnvironments",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnvironmentDescription",
                table: "DemoUserEnvironments");

            migrationBuilder.DropColumn(
                name: "EnvironmentProvisioned",
                table: "DemoUserEnvironments");
        }
    }
}
