using Microsoft.EntityFrameworkCore.Migrations;

namespace DotnetCampusP2PFileShare.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                "ResourceModel",
                table => new
                {
                    Id = table.Column<string>(),
                    ResourceId = table.Column<string>(nullable: true),
                    ResourceName = table.Column<string>(nullable: true),
                    LocalPath = table.Column<string>(nullable: true),
                    ResourceSign = table.Column<string>(nullable: true),
                    ResourceFileDetail = table.Column<string>(nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_ResourceModel", x => x.Id); });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "ResourceModel");
        }
    }
}