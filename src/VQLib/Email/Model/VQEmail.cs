using System.Collections.Generic;

namespace VQLib.Email.Model
{
    public class VQEmail
    {
        public VQEmailUser From { get; set; }
        public List<VQEmailUser> To { get; set; } = new List<VQEmailUser>();
        public string Body { get; set; }
        public string Subject { get; set; }
        public List<VQEmailAttachment> Attachments { get; set; } = new List<VQEmailAttachment>();
    }
}