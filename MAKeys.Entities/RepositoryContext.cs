using System;
using MAKeys.Entities.Configuration;
using MAKeys.Entities.Models;
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
            modelBuilder.Entity<PublicKey>().ToTable("PublicKeys");
            modelBuilder.Entity<PublicKey>()
                .Property((k) => k.SubmissionDate)
                .HasDefaultValueSql("now()");
            
            // seed
            modelBuilder.ApplyConfiguration(new PublicKeyConfiguration());
        }
    }
}