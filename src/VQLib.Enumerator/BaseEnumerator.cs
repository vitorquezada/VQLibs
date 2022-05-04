using VQLib.Util;

namespace VQLib.Enumerator
{
    public abstract class BaseEnumerator<TEnum>
        where TEnum : BaseEnumerator<TEnum>
    {
        private static object _lock = new object();
        private static readonly Dictionary<int, TEnum> _all = new Dictionary<int, TEnum>();

        public int Id { get; private set; }

        public string Description { get; private set; }

        protected BaseEnumerator(int id, string description)
        {
            Id = id;
            Description = description ?? throw new ArgumentNullException(nameof(description));
        }

        public static IReadOnlyDictionary<int, TEnum> GetDictionary()
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
            if (obj.GetType() != GetType()) return false;

            return Equals((TEnum)obj);
        }

        public static bool operator ==(BaseEnumerator<TEnum>? x1, BaseEnumerator<TEnum>? x2) => Equals(x1, x2);

        public static bool operator !=(BaseEnumerator<TEnum>? x1, BaseEnumerator<TEnum>? x2) => !Equals(x1, x2);

        public static TEnum GetById(int id)
        {
            return GetDictionary().ContainsKey(id) ? GetDictionary()[id] : throw new IndexOutOfRangeException();
        }

        public static TEnum? GetByIdNullable(int? id)
        {
            if (id == null)
                return null;

            return GetById(id.Value);
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