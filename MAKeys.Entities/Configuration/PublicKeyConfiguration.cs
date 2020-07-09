using System;
using MAKeys.Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MAKeys.Entities.Configuration
{
    public class PublicKeyConfiguration : IEntityTypeConfiguration<PublicKey>
    {
        public void Configure(EntityTypeBuilder<PublicKey> builder)
        {
            builder.HasData
            (
                new PublicKey
                {
                    Name = "Horváth Bence",
                    Email = "horvath.bence@muszerautomatika.hu",
                    ArmoredKey = "noarmor",
                    ExpirationDate = DateTime.Now + TimeSpan.FromDays(365),
                    Fingerprint = "33EFA0592FAEEF4DD84CD8A0E4C22D9F57CBD3F0"
                },
                new PublicKey
                {
                    Name = "Horváth Gábor",
                    Email = "horvath.gabor@muszerautomatika.hu",
                    ArmoredKey = "noarmor",
                    ExpirationDate = DateTime.Now + TimeSpan.FromDays(365),
                    Fingerprint = "1FDA0F756C0A2A78775CBC7BFA0060473ACD2360"
                }
            );
        }
    }
}