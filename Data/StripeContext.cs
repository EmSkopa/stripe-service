using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using StripeApp.Data.Models;

namespace StripeApp.Data
{
    public class StripeContext : DbContext
    {
        public StripeContext(DbContextOptions<StripeContext> options) : base(options) {}

        public DbSet<PaymentStatus> PaymentStatus { get; set; }
        public DbSet<Payment> Payment { get; set; }
        public DbSet<PaymentUser> PaymentUser { get; set; }
        public DbSet<PaymentUserCard> PaymentUserCard { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<PaymentEvent> PaymentEvent { get; set; }

        // Automatic insertion of CreatedAt and UpdatedAt values
        public override int SaveChanges()
        {
            SetTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            SetTimestamps();
            return (await base.SaveChangesAsync(true, cancellationToken));
        }

        private void SetTimestamps()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e =>
                        e.State == EntityState.Added
                        || e.State == EntityState.Modified);

            foreach (var entityEntry in entries)
            {
                entityEntry.Property("UpdatedAt").CurrentValue = DateTime.Now;

                if (entityEntry.State == EntityState.Added)
                {
                    entityEntry.Property("CreatedAt").CurrentValue = DateTime.Now;
                }
            }
        }
    }
}
