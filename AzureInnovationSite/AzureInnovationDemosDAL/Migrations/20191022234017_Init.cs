using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AzureInnovationDemosDAL.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DemoAssetTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemoAssetTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DemoAzureResourceTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemoAzureResourceTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DemoTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemoTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Givenname = table.Column<string>(nullable: true),
                    Surname = table.Column<string>(nullable: true),
                    AccountName = table.Column<string>(nullable: true),
                    Password = table.Column<string>(nullable: true),
                    IsDisabled = table.Column<bool>(nullable: false),
                    IsAdmin = table.Column<bool>(nullable: false),
                    IsVMAdmin = table.Column<bool>(nullable: false),
                    LastLoggin = table.Column<DateTime>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Demos",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    Categories = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Abstract = table.Column<string>(nullable: true),
                    Technologies = table.Column<string>(nullable: true),
                    Additional = table.Column<string>(nullable: true),
                    IsSharedEnvironment = table.Column<bool>(nullable: false),
                    IsDisabled = table.Column<bool>(nullable: false),
                    IsVisible = table.Column<bool>(nullable: false),
                    TypeId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Demos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Demos_DemoTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "DemoTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserDemoOrganizations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDemoOrganizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDemoOrganizations_Users_Id",
                        column: x => x.Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DemoAssets",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Value = table.Column<string>(nullable: true),
                    TypeId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemoAssets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DemoAssets_Demos_DemoId",
                        column: x => x.DemoId,
                        principalTable: "Demos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DemoAssets_DemoAssetTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "DemoAssetTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DemoAzureResources",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoId = table.Column<int>(nullable: false),
                    TypeId = table.Column<int>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Value = table.Column<string>(nullable: true),
                    AttemptCount = table.Column<int>(nullable: false),
                    RequestedAt = table.Column<DateTime>(nullable: false),
                    LockedUntil = table.Column<DateTime>(nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemoAzureResources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DemoAzureResources_Demos_DemoId",
                        column: x => x.DemoId,
                        principalTable: "Demos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DemoAzureResources_DemoAzureResourceTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "DemoAzureResourceTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DemoSharedCredentials",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoId = table.Column<int>(nullable: false),
                    DemoUser = table.Column<string>(nullable: true),
                    DemoPassword = table.Column<string>(nullable: true),
                    DemoURL = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemoSharedCredentials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DemoSharedCredentials_Demos_DemoId",
                        column: x => x.DemoId,
                        principalTable: "Demos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DemoUserEnvironments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnvironmentUser = table.Column<string>(nullable: true),
                    EnvironmentPassword = table.Column<string>(nullable: true),
                    EnvironmentURL = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    DemoId = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemoUserEnvironments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DemoUserEnvironments_Demos_DemoId",
                        column: x => x.DemoId,
                        principalTable: "Demos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DemoUserEnvironments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DemoUserResources",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    Value = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    TypeId = table.Column<int>(nullable: true),
                    DemoId = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemoUserResources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DemoUserResources_Demos_DemoId",
                        column: x => x.DemoId,
                        principalTable: "Demos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DemoUserResources_DemoAssetTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "DemoAssetTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DemoUserResources_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DemoVMs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemoId = table.Column<int>(nullable: false),
                    URL = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemoVMs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DemoVMs_Demos_DemoId",
                        column: x => x.DemoId,
                        principalTable: "Demos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRDPLogs",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false),
                    UserAccount = table.Column<string>(nullable: true),
                    DemoId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRDPLogs", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserRDPLogs_Demos_DemoId",
                        column: x => x.DemoId,
                        principalTable: "Demos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRDPLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DemoGuides",
                columns: table => new
                {
                    DemoAssetId = table.Column<int>(nullable: false),
                    GuideContent = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemoGuides", x => x.DemoAssetId);
                    table.ForeignKey(
                        name: "FK_DemoGuides_DemoAssets_DemoAssetId",
                        column: x => x.DemoAssetId,
                        principalTable: "DemoAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserDemoAzureResources",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false),
                    DemoAzureResourceId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDemoAzureResources", x => new { x.UserId, x.DemoAzureResourceId });
                    table.ForeignKey(
                        name: "FK_UserDemoAzureResources_DemoAzureResources_DemoAzureResourceId",
                        column: x => x.DemoAzureResourceId,
                        principalTable: "DemoAzureResources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserDemoAzureResources_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DemoAssets_DemoId",
                table: "DemoAssets",
                column: "DemoId");

            migrationBuilder.CreateIndex(
                name: "IX_DemoAssets_TypeId",
                table: "DemoAssets",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DemoAzureResources_DemoId",
                table: "DemoAzureResources",
                column: "DemoId");

            migrationBuilder.CreateIndex(
                name: "IX_DemoAzureResources_LockedUntil",
                table: "DemoAzureResources",
                column: "LockedUntil");

            migrationBuilder.CreateIndex(
                name: "IX_DemoAzureResources_TypeId",
                table: "DemoAzureResources",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Demos_TypeId",
                table: "Demos",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DemoSharedCredentials_DemoId",
                table: "DemoSharedCredentials",
                column: "DemoId");

            migrationBuilder.CreateIndex(
                name: "IX_DemoUserEnvironments_DemoId",
                table: "DemoUserEnvironments",
                column: "DemoId");

            migrationBuilder.CreateIndex(
                name: "IX_DemoUserEnvironments_UserId",
                table: "DemoUserEnvironments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DemoUserResources_DemoId",
                table: "DemoUserResources",
                column: "DemoId");

            migrationBuilder.CreateIndex(
                name: "IX_DemoUserResources_TypeId",
                table: "DemoUserResources",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DemoUserResources_UserId",
                table: "DemoUserResources",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DemoVMs_DemoId",
                table: "DemoVMs",
                column: "DemoId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDemoAzureResources_DemoAzureResourceId",
                table: "UserDemoAzureResources",
                column: "DemoAzureResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRDPLogs_DemoId",
                table: "UserRDPLogs",
                column: "DemoId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DemoGuides");

            migrationBuilder.DropTable(
                name: "DemoSharedCredentials");

            migrationBuilder.DropTable(
                name: "DemoUserEnvironments");

            migrationBuilder.DropTable(
                name: "DemoUserResources");

            migrationBuilder.DropTable(
                name: "DemoVMs");

            migrationBuilder.DropTable(
                name: "UserDemoAzureResources");

            migrationBuilder.DropTable(
                name: "UserDemoOrganizations");

            migrationBuilder.DropTable(
                name: "UserRDPLogs");

            migrationBuilder.DropTable(
                name: "DemoAssets");

            migrationBuilder.DropTable(
                name: "DemoAzureResources");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "DemoAssetTypes");

            migrationBuilder.DropTable(
                name: "Demos");

            migrationBuilder.DropTable(
                name: "DemoAzureResourceTypes");

            migrationBuilder.DropTable(
                name: "DemoTypes");
        }
    }
}
