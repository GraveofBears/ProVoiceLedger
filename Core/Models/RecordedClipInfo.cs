using SQLite;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace ProVoiceLedger.Core.Models
{
    public class RecordedClipInfo
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string FilePath { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string SessionName { get; set; } = string.Empty;

        /// <summary>
        /// Serialized metadata for SQLite storage.
        /// </summary>
        public string MetadataJson { get; set; } = "{}";

        /// <summary>
        /// Deserialized metadata dictionary (ignored by SQLite).
        /// </summary>
        [Ignore]
        public Dictionary<string, string> Metadata
        {
            get => string.IsNullOrWhiteSpace(MetadataJson)
                ? new Dictionary<string, string>()
                : JsonSerializer.Deserialize<Dictionary<string, string>>(MetadataJson) ?? new();
            set => MetadataJson = JsonSerializer.Serialize(value);
        }

        public RecordedClipInfo() { }
    }
}
