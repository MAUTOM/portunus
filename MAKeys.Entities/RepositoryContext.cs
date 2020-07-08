using System;
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
            modelBuilder.Entity<PublicKey>().HasData(new PublicKey()
                {
                    Id = 1,
                    Name = "Horváth Bence",
                    Email = "horvath.bence@muszerautomatika.hu",
                    Armor = "noarmor",
                    ExpirationDate = DateTime.Now + TimeSpan.FromDays(365),
                    Fingerprint = "fprint"
                },
                new PublicKey()
                {
                    Id = 2,
                    Name = "Horváth Gábor",
                    Email = "horvath.gabor@muszerautomatika.hu",
                    Armor = "noarmor",
                    ExpirationDate = DateTime.Now + TimeSpan.FromDays(365),
                    Fingerprint = "fprint2"
                });
        }
    }
}