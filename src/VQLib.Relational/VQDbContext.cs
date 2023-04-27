using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using VQLib.Relational.Entity;
using VQLib.Relational.ValueGenerators;

namespace VQLib.Relational
{
    public abstract class VQDbContext : DbContext
    {
        public const string IS_DELETED_COLUMN_NAME = "Deleted";

        public VQDbContext() : base()
        {
        }

        public VQDbContext(DbContextOptions options) : base(options)
        {
        }

        protected abstract long GetTenantId { get; }

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

        protected virtual bool HasProperty(EntityEntry entry, string propertyName)
        {
            // TODO: Melhorar método com cache
            return entry.Properties.Any(x => x.Metadata.Name == propertyName);
        }

        protected virtual void SetCreateUpdatedAt(EntityEntry entry)
        {
            var dateTime = new VQDateTimeOffsetUtcGenerator().Next(entry);

            if (entry.State == EntityState.Added && HasProperty(entry, nameof(VQBaseEntity.CreatedDate)))
                entry.CurrentValues[nameof(VQBaseEntity.CreatedDate)] = dateTime;

            if ((entry.State == EntityState.Added || entry.State == EntityState.Modified) && HasProperty(entry, nameof(VQBaseEntity.UpdatedDate)))
                entry.CurrentValues[nameof(VQBaseEntity.UpdatedDate)] = dateTime;
        }

        protected virtual void SetSoftDelete(EntityEntry entry)
        {
            if (entry.State == EntityState.Deleted)
            {
                var hasDeletedColumn = HasProperty(entry, IS_DELETED_COLUMN_NAME);
                if (hasDeletedColumn)
                {
                    entry.State = EntityState.Modified;
                    foreach (var prop in entry.Properties)
                    {
                        var isDeletedColumn = prop.Metadata.Name == IS_DELETED_COLUMN_NAME;
                        prop.IsModified = isDeletedColumn;
                        if (isDeletedColumn)
                            prop.CurrentValue = true;
                    }
                }
            }
        }

        protected virtual void SetTenantId(EntityEntry entry)
        {
            if (HasProperty(entry, nameof(VQBaseEntityTenant.TenantId)))
            {
                var currentValue = (long?)entry.CurrentValues[nameof(VQBaseEntityTenant.TenantId)] ?? 0;
                if (currentValue == 0)
                    entry.CurrentValues[nameof(VQBaseEntityTenant.TenantId)] = GetTenantId;
            }
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
    }
}