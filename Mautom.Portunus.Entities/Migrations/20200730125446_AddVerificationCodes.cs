using Microsoft.EntityFrameworkCore.Migrations;

namespace Mautom.Portunus.Entities.Migrations
{
    public partial class AddVerificationCodes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ushort>(
                name: "verification_secret",
                table: "key_identities",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "verification_secret",
                table: "key_identities");
        }
    }
}
