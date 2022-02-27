namespace VQLib.Queue.Model
{
    public class QueueResponse<T> : QueueResponse
    {
        public T Data { get; set; }
    }

    public abstract class QueueResponse
    {
        public string Receipt { get; set; }
    }
}