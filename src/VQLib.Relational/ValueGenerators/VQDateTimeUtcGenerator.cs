using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace VQLib.Relational.ValueGenerators
{
    public class VQDateTimeUtcGenerator : ValueGenerator<DateTime>
    {
        public override bool GeneratesTemporaryValues => false;

        public override DateTime Next(EntityEntry entry) => DateTime.UtcNow;

        protected override object NextValue(EntityEntry entry) => DateTime.UtcNow;

        public override ValueTask<DateTime> NextAsync(EntityEntry entry, CancellationToken cancellationToken = default) => ValueTask.FromResult(DateTime.UtcNow);

        protected override ValueTask<object> NextValueAsync(EntityEntry entry, CancellationToken cancellationToken = default) => ValueTask.FromResult<object>(DateTime.UtcNow);
    }
}