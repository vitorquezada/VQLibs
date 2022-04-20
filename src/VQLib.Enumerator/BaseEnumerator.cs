using VQLib.Util;

namespace VizePortal.Infra.Enumerators
{
    public abstract class BaseEnumerator<TEnum, TId>
        where TEnum : BaseEnumerator<TEnum, TId>
        where TId : notnull
    {
        private static object _lock = new object();
        private static readonly Dictionary<TId, TEnum> _all = new Dictionary<TId, TEnum>();

        public TId Id { get; private set; }

        public string Description { get; private set; }

        protected BaseEnumerator(TId id, string description)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Description = description ?? throw new ArgumentNullException(nameof(description));
        }

        public static IReadOnlyDictionary<TId, TEnum> GetDictionary()
        {
            lock (_lock)
            {
                if (_all.Count <= 0)
                {
                    if (_all.Count <= 0)
                    {
                        var fields = typeof(TEnum).GetFields().Where(x => x.IsStatic && x.IsPublic && x.DeclaringType == typeof(TEnum));
                        foreach (var field in fields)
                        {
                            var value = (TEnum)(field.GetValue(null) ?? throw new ArgumentNullException(field.Name));
                            _all.Add(value.Id, value);
                        }
                    }
                }

                return _all;
            }
        }

        public static IReadOnlyList<TEnum> GetList()
        {
            return GetDictionary().Values.ToList().AsReadOnly();
        }

        public bool Equals(TEnum obj)
        {
            return Id.Equals(obj.Id);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return false;
            if (obj.GetType() != this.GetType()) return false;

            return Equals((TEnum)obj);
        }

        public static bool operator ==(BaseEnumerator<TEnum, TId> x1, BaseEnumerator<TEnum, TId> x2) => Equals(x1, x2);

        public static bool operator !=(BaseEnumerator<TEnum, TId> x1, BaseEnumerator<TEnum, TId> x2) => !Equals(x1, x2);

        public static TEnum? GetById(TId id)
        {
            return GetDictionary().ContainsKey(id) ? GetDictionary()[id] : default(TEnum);
        }

        public static List<TEnum> GetByDescriptionTerm(string term)
        {
            return GetDictionary().Values.Where(x => x.Description.ContainsIgnoreCaseAndAccents(term)).ToList();
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}