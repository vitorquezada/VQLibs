using System;

namespace VQLib.Relational.Entity
{
    public abstract class VQBaseEntityTenant : VQBaseEntity
    {
        public long TenantId { get; set; }
    }

    public abstract class VQBaseEntity
    {
        public long Id { get; set; }
        public string Key { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset UpdatedDate { get; set; }
        public bool Active { get; set; }
    }
}