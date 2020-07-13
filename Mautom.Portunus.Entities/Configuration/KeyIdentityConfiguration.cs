// Copyright (c) 2020 Bence Horváth <horvath.bence@mautom.hu>.
//
// This file is part of Portunus OpenPGP key server 
// (see https://www.horvathb.dev/portunus).
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.
using System;
using Mautom.Portunus.Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mautom.Portunus.Entities.Configuration
{
    public class KeyIdentityConfiguration : IEntityTypeConfiguration<KeyIdentity>
    {
        public void Configure(EntityTypeBuilder<KeyIdentity> builder)
        {
            builder.HasData
            (
                new KeyIdentity
                {
                    IdentityId = -1,
                    Name = "Bence Horváth",
                    Email = "horvath.bence@muszerautomatika.hu",
                    CreationDate = new DateTime(2020, 6, 18),
                    PublicKeyFingerprint = "33EFA0592FAEEF4DD84CD8A0E4C22D9F57CBD3F0"
                },
                new KeyIdentity
                {
                    IdentityId = -2,
                    Name = "Gábor Horváth",
                    Email = "horvath.gabor@muszerautomatika.hu",
                    CreationDate = new DateTime(2020, 6, 22),
                    PublicKeyFingerprint = "1FDA0F756C0A2A78775CBC7BFA0060473ACD2360"
                        
                }
            );
        }
    }
}