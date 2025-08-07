using System;

namespace ProVoiceLedger.Core.Models
{
    public class FileTransferRequest
    {
        public string Filename { get; set; } = string.Empty;
        public byte[] FileBytes { get; set; } = Array.Empty<byte>();
        public string SessionToken { get; set; } = string.Empty;
    }
}
