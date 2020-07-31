using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Mautom.Portunus.Entities.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "address_verification",
                columns: table => new
                {
                    verification_id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    token = table.Column<Guid>(nullable: false, defaultValue: new Guid("399edbb9-695a-4336-a538-70e9437102da")),
                    email = table.Column<string>(nullable: false),
                    verification_code = table.Column<ushort>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_address_verification", x => x.verification_id);
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
                    flags = table.Column<int>(nullable: false, defaultValue: 0),
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

            migrationBuilder.InsertData(
                table: "public_keys",
                columns: new[] { "fingerprint", "algorithm", "armored_key", "creation_date", "expiration_date", "length", "long_key_id", "short_key_id" },
                values: new object[] { "33EFA0592FAEEF4DD84CD8A0E4C22D9F57CBD3F0", 1, @"-----BEGIN PGP PUBLIC KEY BLOCK-----

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
=Sxbn
-----END PGP PUBLIC KEY BLOCK-----
", new DateTime(2020, 6, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2021, 6, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), 4096, "E4C22D9F57CBD3F0", "57CBD3F0" });

            migrationBuilder.InsertData(
                table: "public_keys",
                columns: new[] { "fingerprint", "algorithm", "armored_key", "creation_date", "expiration_date", "length", "long_key_id", "short_key_id" },
                values: new object[] { "1FDA0F756C0A2A78775CBC7BFA0060473ACD2360", 1, @"-----BEGIN PGP PUBLIC KEY BLOCK-----

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
IMNgaG75uJaVS3Z787pEWteuiKyRb/H/JQ==
=+IcE
-----END PGP PUBLIC KEY BLOCK-----
", new DateTime(2020, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2021, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), 4096, "FA0060473ACD2360", "3ACD2360" });

            migrationBuilder.InsertData(
                table: "key_identities",
                columns: new[] { "identity_id", "comment", "creation_date", "email", "name", "public_key_fingerprint", "status", "verification_token" },
                values: new object[] { -1L, string.Empty, new DateTime(2020, 6, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "horvath.bence@muszerautomatika.hu", "Bence Horváth", "33EFA0592FAEEF4DD84CD8A0E4C22D9F57CBD3F0", 0, new Guid("00000000-0000-0000-0000-000000000000") });

            migrationBuilder.InsertData(
                table: "key_identities",
                columns: new[] { "identity_id", "comment", "creation_date", "email", "name", "public_key_fingerprint", "status", "verification_token" },
                values: new object[] { -2L, string.Empty, new DateTime(2020, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "horvath.gabor@muszerautomatika.hu", "Gábor Horváth", "1FDA0F756C0A2A78775CBC7BFA0060473ACD2360", 0, new Guid("00000000-0000-0000-0000-000000000000") });

            migrationBuilder.CreateIndex(
                name: "ix_key_identities_public_key_fingerprint",
                table: "key_identities",
                column: "public_key_fingerprint");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "address_verification");

            migrationBuilder.DropTable(
                name: "key_identities");

            migrationBuilder.DropTable(
                name: "public_keys");
        }
    }
}
