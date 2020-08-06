using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Mautom.Portunus.Entities.Migrations
{
    public partial class InitializeDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "address_verifications",
                columns: table => new
                {
                    verification_id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    token = table.Column<Guid>(nullable: false, defaultValue: Guid.NewGuid()),
                    email = table.Column<string>(nullable: false),
                    verification_code = table.Column<ushort>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_address_verifications", x => x.verification_id);
                });

            migrationBuilder.CreateTable(
                name: "public_keys",
                columns: table => new
                {
                    fingerprint = table.Column<string>(type: "varchar(40)", nullable: false),
                    long_key_id = table.Column<string>(nullable: false),
                    short_key_id = table.Column<string>(nullable: false),
                    armored_key = table.Column<string>(nullable: false),
                    submission_date = table.Column<DateTime>(nullable: false, defaultValueSql: "now()"),
                    creation_date = table.Column<DateTime>(nullable: false),
                    expiration_date = table.Column<DateTime>(nullable: true),
                    flags = table.Column<int>(nullable: false, defaultValue: 1),
                    algorithm = table.Column<int>(nullable: false),
                    length = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_public_keys", x => x.fingerprint);
                });

            migrationBuilder.CreateTable(
                name: "key_identities",
                columns: table => new
                {
                    identity_id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(maxLength: 50, nullable: false),
                    email = table.Column<string>(nullable: false),
                    comment = table.Column<string>(maxLength: 200, nullable: true),
                    creation_date = table.Column<DateTime>(nullable: false),
                    status = table.Column<int>(nullable: false),
                    verification_token = table.Column<Guid>(nullable: false),
                    public_key_fingerprint = table.Column<string>(type: "varchar(40)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_key_identities", x => x.identity_id);
                    table.ForeignKey(
                        name: "fk_key_identities_public_keys_public_key_fingerprint",
                        column: x => x.public_key_fingerprint,
                        principalTable: "public_keys",
                        principalColumn: "fingerprint",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_address_verifications_email",
                table: "address_verifications",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_key_identities_public_key_fingerprint",
                table: "key_identities",
                column: "public_key_fingerprint");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "address_verifications");

            migrationBuilder.DropTable(
                name: "key_identities");

            migrationBuilder.DropTable(
                name: "public_keys");
        }
    }
}
