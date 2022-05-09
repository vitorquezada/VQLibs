using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VQLib.Relational.Entity;
using VQLib.Relational.ValueGenerators;

namespace VQLib.Relational.Mapping
{
    public abstract class VQBaseTenantMapping<T, TTenant> : VQBaseMapping<T>, IEntityTypeConfiguration<T> where T : VQBaseEntityTenant where TTenant : VQBaseEntity
    {
        protected override bool ExecuteOnBaseEntityMapping => false;

        public new void Configure(EntityTypeBuilder<T> builder)
        {
            base.Configure(builder);

            builder
                .HasOne<TTenant>()
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.SetNull);

            builder
                .HasIndex(nameof(VQBaseEntityTenant.TenantId), VQDbContext.IS_DELETED_COLUMN_NAME);

            Map(builder);
        }
    }

    public abstract class VQBaseMapping<T> : IEntityTypeConfiguration<T> where T : VQBaseEntity
    {
        protected abstract string TableName { get; }

        protected virtual bool ExecuteOnBaseEntityMapping => true;

        public void Configure(EntityTypeBuilder<T> builder)
        {
            builder.ToTable(TableName);

            builder.HasQueryFilter(m => EF.Property<bool>(m, VQDbContext.IS_DELETED_COLUMN_NAME) == false);

            builder
                .HasKey(x => x.Id);
            builder
                .Property(x => x.Id)
                .IsRequired()
                .ValueGeneratedOnAdd();

            builder
                .HasIndex(x => x.Key)
                .IsUnique();
            builder
                .Property(x => x.Key)
                .IsRequired()
                .HasMaxLength(255)
                .ValueGeneratedOnAdd()
                .HasValueGenerator<VQGuidValueGeneratorCustom>();

            builder
                .Property<bool>(VQDbContext.IS_DELETED_COLUMN_NAME)
                .IsRequired()
                .HasDefaultValue(false);
            builder.HasIndex(VQDbContext.IS_DELETED_COLUMN_NAME);

            builder
                .Property(x => x.CreatedDate)
                .IsRequired()
                .ValueGeneratedOnAdd()
                .HasValueGenerator<VQDateTimeOffsetUtcGenerator>();

            var lastUpdatedAtProp = builder
                .Property(x => x.UpdatedDate)
                .IsRequired()
                .ValueGeneratedOnAddOrUpdate()
                .HasValueGenerator<VQDateTimeOffsetUtcGenerator>();
            lastUpdatedAtProp.Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Save);

            if (ExecuteOnBaseEntityMapping)
                Map(builder);
        }

        protected abstract void Map(EntityTypeBuilder<T> builder);
    }
}