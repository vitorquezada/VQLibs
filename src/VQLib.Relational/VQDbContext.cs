using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using VQLib.Relational.Entity;
using VQLib.Relational.ValueGenerators;

namespace VQLib.Relational
{
    public abstract class VQDbContext : DbContext
    {
        public const string IS_DELETED_COLUMN_NAME = "Deleted";

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
                SetCreateUpdatedAt(entry);
                SetTenantId(entry);
            }
        }

        protected virtual void SetTenantId(EntityEntry entry)
        {
            if (entry.CurrentValues.Properties.Any(x => x.Name == nameof(VQBaseEntityTenant.TenantId)))
            {
                var currentValue = (long?)entry.CurrentValues[nameof(VQBaseEntityTenant.TenantId)] ?? 0;
                if (currentValue == 0)
                    entry.CurrentValues[nameof(VQBaseEntityTenant.TenantId)] = GetTenantId;
            }
        }

        protected virtual void SetCreateUpdatedAt(EntityEntry entry)
        {
            var dateTime = new VQDateTimeOffsetUtcGenerator().Next(entry);
            if (entry.State == EntityState.Added)
                entry.CurrentValues[nameof(VQBaseEntity.CreatedDate)] = dateTime;

            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                entry.CurrentValues[nameof(VQBaseEntity.UpdatedDate)] = dateTime;
        }

        protected virtual void SetSoftDelete(EntityEntry entry)
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.CurrentValues[IS_DELETED_COLUMN_NAME] = false;
            }
        }
    }
}