using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Threading;
using System.Threading.Tasks;
using VQLib.Relational.Entity;
using VQLib.Relational.ValueGenerators;
using VQLib.Util;

namespace VQLib.Relational
{
    public abstract class VQDbContext : DbContext
    {
        protected abstract long GetTenantId { get; }

        public VQDbContext() : base()
        {
        }

        public VQDbContext(DbContextOptions options) : base(options)
        {
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            CustomBehaviorsOnSaveChanges();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            CustomBehaviorsOnSaveChanges();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void CustomBehaviorsOnSaveChanges()
        {
            var entries = ChangeTracker.Entries();

            foreach (var entry in entries)
            {
                SetSoftDelete(entry);
                //SetCreateUpdatedAt(entry);
                SetTenantId(entry);
            }
        }

        protected virtual void SetTenantId(EntityEntry entry)
        {
            if (entry.CurrentValues.Properties.ListHasItem(x => x.Name == nameof(VQBaseEntityTenant.TenantId))
                && ((long)entry.CurrentValues[nameof(VQBaseEntityTenant.TenantId)]) == 0)
                entry.CurrentValues[nameof(VQBaseEntityTenant.TenantId)] = GetTenantId;
        }

        protected static void SetCreateUpdatedAt(EntityEntry entry)
        {
            var dateTime = new VQDateTimeUtcGenerator().Next(entry);
            if (entry.State == EntityState.Added)
                entry.CurrentValues[nameof(VQBaseEntity.CreatedDate)] = dateTime;

            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                entry.CurrentValues[nameof(VQBaseEntity.CreatedDate)] = dateTime;
        }

        protected static void SetSoftDelete(EntityEntry entry)
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.CurrentValues[nameof(VQBaseEntity.Active)] = false;
            }
        }
    }
}