using System.Net;

namespace VQLib.Util
{
    public static class VQHttpExtensions
    {
        public static bool IsSuccessStatusCode(this int statusCode)
        {
            return statusCode >= 200 && statusCode < 300;
        }

        public static bool IsSuccessStatusCode(this HttpStatusCode statusCode)
        {
            return IsSuccessStatusCode((int)statusCode);
        }
    }
}