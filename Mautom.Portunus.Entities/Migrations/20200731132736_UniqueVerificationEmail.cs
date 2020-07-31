using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Mautom.Portunus.Entities.Migrations
{
    public partial class UniqueVerificationEmail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "token",
                table: "address_verification",
                nullable: false,
                defaultValue: new Guid("068f637c-d109-4b76-a767-e335f90da156"),
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldDefaultValue: new Guid("399edbb9-695a-4336-a538-70e9437102da"));

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "address_verification",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext CHARACTER SET utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_address_verification_email",
                table: "address_verification",
                column: "email",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_address_verification_email",
                table: "address_verification");

            migrationBuilder.AlterColumn<Guid>(
                name: "token",
                table: "address_verification",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("399edbb9-695a-4336-a538-70e9437102da"),
                oldClrType: typeof(Guid),
                oldDefaultValue: new Guid("068f637c-d109-4b76-a767-e335f90da156"));

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "address_verification",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: false,
                oldClrType: typeof(string));
        }
    }
}
