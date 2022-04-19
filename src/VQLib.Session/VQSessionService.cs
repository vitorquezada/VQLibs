using System.Collections.Generic;

namespace VQLib.Session
{
    public class VQSessionService : IVQSessionService
    {
        private Dictionary<string, object> _sessionData = new Dictionary<string, object>();

        public virtual long TenantId { get; set; }

        public virtual T GetSessionData<T>(string key, T @default = default)
        {
            if (_sessionData.ContainsKey(key))
                return (T)_sessionData[key];
            return @default;
        }

        public virtual void SetSessionData(string key, object data)
        {
            if (_sessionData.ContainsKey(key))
                _sessionData[key] = data;
            else
                _sessionData.Add(key, data);
        }
    }
}