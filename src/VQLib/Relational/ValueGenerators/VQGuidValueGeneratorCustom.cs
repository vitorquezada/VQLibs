using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace VQLib.Relational.ValueGenerators
{
    public class VQGuidValueGeneratorCustom : ValueGenerator<string>
    {
        public override bool GeneratesTemporaryValues => false;

        public override string Next([NotNull] EntityEntry entry) => Guid.NewGuid().ToString();
    }
}