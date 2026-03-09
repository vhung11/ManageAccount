using ManageAccount.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ManageAccount.Infrastructure.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<AccountBalance> AccountBalances { get; set; }
        public DbSet<InterestType> InterestTypes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false)
                    .Build();

                var connectionString = configuration.GetConnectionString("OracleConnection");
                optionsBuilder.UseOracle(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Account>()
                .HasMany(a => a.AccountBalances)
                .WithOne(ab => ab.Account)
                .HasForeignKey(ab => ab.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AccountBalance>()
                .HasOne(ab => ab.InterestType)
                .WithMany(it => it.AccountBalances)
                .HasForeignKey(ab => ab.InterestTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình precision cho decimal properties (Oracle yêu cầu)
            modelBuilder.Entity<AccountBalance>()
                .Property(ab => ab.Balance)
                .HasPrecision(18, 2);

            modelBuilder.Entity<InterestType>()
                .Property(it => it.Rate)
                .HasPrecision(5, 4);
        }
    }
}