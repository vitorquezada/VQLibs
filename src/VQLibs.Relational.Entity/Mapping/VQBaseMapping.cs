using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VQLibs.Relational.Entity.ValueGenerators;

namespace VQLibs.Relational.Entity.Mapping
{
    public abstract class VQBaseMapping<T> : IEntityTypeConfiguration<T> where T : VQBaseEntity
    {
        public void Configure(EntityTypeBuilder<T> builder)
        {
            builder
                .HasKey(x => x.Id);
            builder
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            builder
                .HasIndex(x => x.Key)
                .IsUnique();
            builder
                .Property(x => x.Key)
                .ValueGeneratedOnAdd();

            builder
                .Property(x => x.CreatedAt)
                .ValueGeneratedOnAdd()
                .HasValueGenerator<DateTimeGenerator>();

            builder
                .Property(x => x.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasValueGenerator<DateTimeGenerator>();

            ConfigureMap(builder);
        }

        public abstract void ConfigureMap(EntityTypeBuilder<T> builder);
    }
}
