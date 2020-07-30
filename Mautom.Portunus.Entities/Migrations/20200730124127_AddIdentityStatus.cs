using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Mautom.Portunus.Entities.Migrations
{
    public partial class AddIdentityStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "key_identities",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "verification_token",
                table: "key_identities",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                table: "key_identities");

            migrationBuilder.DropColumn(
                name: "verification_token",
                table: "key_identities");
        }
    }
}
