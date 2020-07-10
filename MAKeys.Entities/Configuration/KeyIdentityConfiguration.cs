using System;
using MAKeys.Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MAKeys.Entities.Configuration
{
    public class KeyIdentityConfiguration : IEntityTypeConfiguration<KeyIdentity>
    {
        public void Configure(EntityTypeBuilder<KeyIdentity> builder)
        {
            builder.HasData
            (
                new KeyIdentity
                {
                    Id = -1,
                    Name = "Bence Horváth",
                    Email = "horvath.bence@muszerautomatika.hu",
                    CreationDate = new DateTime(2020, 6, 18),
                    PublicKeyFingerprint = "33EFA0592FAEEF4DD84CD8A0E4C22D9F57CBD3F0"
                },
                new KeyIdentity
                {
                    Id = -2,
                    Name = "Gábor Horváth",
                    Email = "horvath.gabor@muszerautomatika.hu",
                    CreationDate = new DateTime(2020, 6, 22),
                    PublicKeyFingerprint = "1FDA0F756C0A2A78775CBC7BFA0060473ACD2360"
                        
                }
            );
        }
    }
}