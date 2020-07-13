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
using Mautom.Portunus.Entities.Configuration;
using Mautom.Portunus.Entities.Models;
using Mautom.Portunus.Shared;
using Microsoft.EntityFrameworkCore;

namespace Mautom.Portunus.Entities
{
    public sealed class RepositoryContext : DbContext
    {
        public DbSet<PublicKey> PublicKeys { get; set; } = null!;
        public DbSet<KeyIdentity> KeyIdentities { get; set; } = null!;
        
        public RepositoryContext(DbContextOptions<RepositoryContext> options) : base(options)
        {
            
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.UseSnakeCaseNamingConvention();
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            modelBuilder.Entity<PublicKey>()
                .Property((k) => k.SubmissionDate)
                .HasDefaultValueSql("now()");

            modelBuilder.Entity<PublicKey>()
                .Property(k => k.Flags)
                .HasDefaultValue(PublicKeyFlags.None);
            
            
            // seed
            modelBuilder.ApplyConfiguration(new PublicKeyConfiguration());
            modelBuilder.ApplyConfiguration(new KeyIdentityConfiguration());
        }
    }
}