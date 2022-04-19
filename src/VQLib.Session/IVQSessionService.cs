namespace VQLib.Session
{
    public interface IVQSessionService
    {
        long TenantId { get; set; }

        void SetSessionData(string key, object data);

        T GetSessionData<T>(string key, T @default = default);
    }
}