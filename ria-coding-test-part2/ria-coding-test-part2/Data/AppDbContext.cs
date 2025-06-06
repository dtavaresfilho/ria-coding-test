using Microsoft.EntityFrameworkCore;
using ria_coding_test_part2.Model;

namespace ria_coding_test_part2.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<CustomerStorage> CustomerStorage { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CustomerStorage>()
                .ToTable("CustomerStorage")
                .Property(c => c.Data)
                .HasColumnType("jsonb");
        }
    }
}
