namespace VQLib.Email.Model
{
    public class VQEmailAttachment
    {
        public string Name { get; set; }
        public byte[] Content { get; set; }
        public string Type { get; set; }
    }
}