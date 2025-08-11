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

        /// <summary>📁 Path to the saved audio file</summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>📝 Display title for the recording</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>🕒 UTC timestamp when recording was saved</summary>
        public DateTime Timestamp { get; set; }

        /// <summary>🧭 Session name this recording belongs to</summary>
        public string SessionName { get; set; } = string.Empty;

        /// <summary>⏱️ Duration in seconds</summary>
        public double Duration { get; set; }

        /// <summary>🧠 Serialized metadata (e.g., device, tags)</summary>
        public string MetadataJson { get; set; } = "{}";

        /// <summary>🧩 Deserialized metadata dictionary</summary>
        [Ignore]
        public Dictionary<string, string> Metadata
        {
            get => string.IsNullOrWhiteSpace(MetadataJson)
                ? new Dictionary<string, string>()
                : JsonSerializer.Deserialize<Dictionary<string, string>>(MetadataJson) ?? new();
            set => MetadataJson = JsonSerializer.Serialize(value);
        }

        [Ignore]
        public float[] EditedWaveform { get; private set; } = Array.Empty<float>();

        public void SaveEditedWaveform(float[] amplitudes)
        {
            EditedWaveform = amplitudes;
            // TODO: Persist to file or database if needed
        }

        /// <summary>🗓️ Optional override for recorded timestamp</summary>
        public DateTime? RecordedAtOverride { get; set; }

        /// <summary>📱 Optional override for device name</summary>
        public string? DeviceUsedOverride { get; set; }

        /// <summary>🗓️ Alias for recorded timestamp (fallback to Timestamp)</summary>
        [Ignore]
        public DateTime RecordedAt => RecordedAtOverride ?? Timestamp;

        /// <summary>📱 Alias for device name (fallback to metadata)</summary>
        [Ignore]
        public string DeviceUsed =>
            !string.IsNullOrWhiteSpace(DeviceUsedOverride)
                ? DeviceUsedOverride
                : Metadata.TryGetValue("Device", out var device) ? device : "Unknown";

        /// <summary>🕓 Optional start time of recording</summary>
        public DateTime? StartedAt { get; set; }

        /// <summary>🕔 Optional stop time of recording</summary>
        public DateTime? StoppedAt { get; set; }

        /// <summary>📄 Filename derived from FilePath</summary>
        [Ignore]
        public string Filename => System.IO.Path.GetFileName(FilePath);

        /// <summary>✅ Indicates if the clip has a valid file path and duration</summary>
        [Ignore]
        public bool IsValid => !string.IsNullOrWhiteSpace(FilePath) && Duration > 0;

        public RecordedClipInfo() { }

        public RecordedClipInfo(
            string filePath,
            double duration,
            string sessionName,
            DateTime timestamp,
            Dictionary<string, string>? metadata = null,
            DateTime? recordedAtOverride = null,
            string? deviceUsedOverride = null,
            DateTime? startedAt = null,
            DateTime? stoppedAt = null)
        {
            FilePath = filePath;
            Duration = duration;
            SessionName = sessionName;
            Timestamp = timestamp;
            Metadata = metadata ?? new();
            RecordedAtOverride = recordedAtOverride;
            DeviceUsedOverride = deviceUsedOverride;
            StartedAt = startedAt;
            StoppedAt = stoppedAt;
        }
    }
}
