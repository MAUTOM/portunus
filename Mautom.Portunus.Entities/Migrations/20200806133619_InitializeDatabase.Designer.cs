﻿// <auto-generated />
using System;
using Mautom.Portunus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Mautom.Portunus.Entities.Migrations
{
    [DbContext(typeof(RepositoryContext))]
    [Migration("20200806133619_InitializeDatabase")]
    partial class InitializeDatabase
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Mautom.Portunus.Entities.Models.AddressVerification", b =>
                {
                    b.Property<int>("VerificationId")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("verification_id")
                        .HasColumnType("int");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnName("email")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<Guid>("Token")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("token")
                        .HasColumnType("char(36)")
                        .HasDefaultValue(new Guid("93f047eb-2d03-46ae-8e8f-6be05573ee0b"));

                    b.Property<ushort>("VerificationCode")
                        .HasColumnName("verification_code")
                        .HasColumnType("smallint unsigned");

                    b.HasKey("VerificationId")
                        .HasName("pk_address_verifications");

                    b.HasIndex("Email")
                        .IsUnique()
                        .HasName("ix_address_verifications_email");

                    b.ToTable("address_verifications");
                });

            modelBuilder.Entity("Mautom.Portunus.Entities.Models.KeyIdentity", b =>
                {
                    b.Property<long>("IdentityId")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("identity_id")
                        .HasColumnType("bigint");

                    b.Property<string>("Comment")
                        .HasColumnName("comment")
                        .HasColumnType("varchar(200) CHARACTER SET utf8mb4")
                        .HasMaxLength(200);

                    b.Property<DateTime>("CreationDate")
                        .HasColumnName("creation_date")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnName("email")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("name")
                        .HasColumnType("varchar(50) CHARACTER SET utf8mb4")
                        .HasMaxLength(50);

                    b.Property<string>("PublicKeyFingerprint")
                        .IsRequired()
                        .HasColumnName("public_key_fingerprint")
                        .HasColumnType("varchar(40)");

                    b.Property<int>("Status")
                        .HasColumnName("status")
                        .HasColumnType("int");

                    b.Property<Guid>("VerificationToken")
                        .HasColumnName("verification_token")
                        .HasColumnType("char(36)");

                    b.HasKey("IdentityId")
                        .HasName("pk_key_identities");

                    b.HasIndex("PublicKeyFingerprint")
                        .HasName("ix_key_identities_public_key_fingerprint");

                    b.ToTable("key_identities");
                });

            modelBuilder.Entity("Mautom.Portunus.Entities.Models.PublicKey", b =>
                {
                    b.Property<string>("Fingerprint")
                        .HasColumnName("fingerprint")
                        .HasColumnType("varchar(40)");

                    b.Property<int>("Algorithm")
                        .HasColumnName("algorithm")
                        .HasColumnType("int");

                    b.Property<string>("ArmoredKey")
                        .IsRequired()
                        .HasColumnName("armored_key")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<DateTime>("CreationDate")
                        .HasColumnName("creation_date")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime?>("ExpirationDate")
                        .HasColumnName("expiration_date")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("Flags")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("flags")
                        .HasColumnType("int")
                        .HasDefaultValue(1);

                    b.Property<int>("Length")
                        .HasColumnName("length")
                        .HasColumnType("int");

                    b.Property<string>("LongKeyId")
                        .IsRequired()
                        .HasColumnName("long_key_id")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("ShortKeyId")
                        .IsRequired()
                        .HasColumnName("short_key_id")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<DateTime>("SubmissionDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("submission_date")
                        .HasColumnType("datetime(6)")
                        .HasDefaultValueSql("now()");

                    b.HasKey("Fingerprint")
                        .HasName("pk_public_keys");

                    b.ToTable("public_keys");
                });

            modelBuilder.Entity("Mautom.Portunus.Entities.Models.KeyIdentity", b =>
                {
                    b.HasOne("Mautom.Portunus.Entities.Models.PublicKey", "PublicKey")
                        .WithMany("KeyIdentities")
                        .HasForeignKey("PublicKeyFingerprint")
                        .HasConstraintName("fk_key_identities_public_keys_public_key_fingerprint")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}