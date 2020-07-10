using System;
using MAKeys.Entities.Configuration;
using MAKeys.Entities.Models;
using MAKeys.Shared;
using Microsoft.EntityFrameworkCore;

namespace MAKeys.Entities
{
    public sealed class RepositoryContext : DbContext
    {
        public DbSet<PublicKey> PublicKeys { get; set; } = null!;
        
        public RepositoryContext(DbContextOptions<RepositoryContext> options) : base(options)
        {
            
        }

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