using SQLite;
using System;
using ProVoiceLedger.Core.Models;
using ProVoiceLedger.Core.Services;

namespace ProVoiceLedger.Core.Models
{
    public class Session
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public DateTime StartTime { get; set; } = DateTime.Now;
        public string Notes { get; set; } = string.Empty;
        public string TaggedKeywords { get; set; } = string.Empty;

        // SQLite-safe backing field for Duration
        public double DurationSeconds
        {
            get => Duration.TotalSeconds;
            set => Duration = TimeSpan.FromSeconds(value);
        }

        [Ignore]
        public TimeSpan Duration { get; set; } = TimeSpan.Zero;

        public string DeviceUsed { get; set; } = string.Empty;

        public Session() { }
    }
}
