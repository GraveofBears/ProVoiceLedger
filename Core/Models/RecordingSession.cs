using SQLite;

namespace ProVoiceLedger.Core.Models
{
    public class RecordingSession
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime Timestamp { get; set; }
        public string FilePath { get; set; }
    }
}
