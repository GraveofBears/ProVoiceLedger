using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProVoiceLedger.Core.Models;

namespace ProVoiceLedger.Core.Services
{
    public interface IRecordingService
    {
        // 🎙️ Recording
        Task StartRecordingAsync(string sessionName, Dictionary<string, string> metadata);
        Task<RecordedClipInfo> StopRecordingAsync();
        Task FinishRecordingAsync(); // ✅ Add this line
        Task SaveRecordingAsync(RecordedClipInfo clip);

        // ▶️ Playback
        Task PlayRecordingAsync(string filePath);
        void PausePlayback();
        void StopPlayback();
        void SeekBackward(TimeSpan amount);
        void SeekForward(TimeSpan amount);
        void SeekTo(TimeSpan position);

        // 📦 Retrieval
        Task<RecordedClipInfo?> GetLastRecordingAsync();
        Task<IList<RecordedClipInfo>> GetAllRecordingsAsync();
        Task<double> GetLastRecordingDurationAsync();
        RecordedClipInfo? GetLastClipInfo();

        // 👤 User session
        void SetCurrentUser(User user);
        User? GetCurrentUser();
        Task<User?> TryRestoreUserAsync();
    }
}
