using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace VQLib.Relational.ValueGenerators
{
    public class VQDateTimeOffsetUtcGenerator : ValueGenerator<DateTimeOffset>
    {
        public override bool GeneratesTemporaryValues => false;

        public override DateTimeOffset Next(EntityEntry entry) => DateTimeOffset.UtcNow;

        protected override object NextValue(EntityEntry entry) => DateTimeOffset.UtcNow;

        public override ValueTask<DateTimeOffset> NextAsync(EntityEntry entry, CancellationToken cancellationToken = default) => ValueTask.FromResult(DateTimeOffset.UtcNow);

        protected override ValueTask<object> NextValueAsync(EntityEntry entry, CancellationToken cancellationToken = default) => ValueTask.FromResult<object>(DateTimeOffset.UtcNow);
    }
}