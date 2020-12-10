using System;

namespace VQLibs.Relational.Entity
{
    public abstract class VQBaseEntity
    {
        public ulong Id { get; set; }

        public Guid Key { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
