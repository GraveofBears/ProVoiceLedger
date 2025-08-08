using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using ProVoiceLedger.AudioBackup;
using ProVoiceLedger.Core.Models;

namespace ProVoiceLedger.Core.Services
{
    public class RecordingService : IRecordingService
    {
        private readonly IAudioCaptureService _audioCapture;
        private readonly SessionDatabase _sessionDatabase;
        private RecordedClipInfo? _lastClip;

        public User? CurrentUser { get; private set; }

        public RecordingService(IAudioCaptureService audioCapture, SessionDatabase sessionDatabase)
        {
            ArgumentNullException.ThrowIfNull(audioCapture);
            ArgumentNullException.ThrowIfNull(sessionDatabase);

            _audioCapture = audioCapture;
            _sessionDatabase = sessionDatabase;
        }

        // 👤 Set the current authenticated user and persist to secure storage
        public void SetCurrentUser(User user)
        {
            ArgumentNullException.ThrowIfNull(user);
            CurrentUser = user;

            var json = JsonSerializer.Serialize(user);
            SecureStorage.SetAsync("current_user", json);
        }

        public User? GetCurrentUser() => CurrentUser;

        public async Task<User?> TryRestoreUserAsync()
        {
            try
            {
                var json = await SecureStorage.GetAsync("current_user");
                if (string.IsNullOrWhiteSpace(json)) return null;

                var user = JsonSerializer.Deserialize<User>(json);
                CurrentUser = user;
                return user;
            }
            catch
            {
                return null;
            }
        }

        // 🎙️ Start a new recording session
        public async Task StartRecordingAsync(string sessionName, Dictionary<string, string> metadata)
        {
            if (CurrentUser is null)
                throw new InvalidOperationException("No authenticated user set.");

            metadata ??= new Dictionary<string, string>
            {
                ["username"] = CurrentUser.Username,
                ["userId"] = CurrentUser.Id
            };

            var started = await _audioCapture.StartRecordingAsync(sessionName, metadata);
            if (!started)
                throw new InvalidOperationException("Failed to start recording.");
        }

        // 🛑 Stop recording and persist the clip
        public async Task<RecordedClipInfo> StopRecordingAsync()
        {
            var clip = await _audioCapture.StopRecordingAsync();
            if (clip is null)
                throw new InvalidOperationException("No recording was active.");

            _lastClip = clip;
            await SaveRecordingAsync(clip);
            return clip;
        }

        public async Task SaveRecordingAsync(RecordedClipInfo clip)
        {
            ArgumentNullException.ThrowIfNull(clip);
            await _sessionDatabase.SaveRecordingAsync(clip);
        }

        public async Task<RecordedClipInfo?> GetLastRecordingAsync()
        {
            _lastClip = await _sessionDatabase.GetLastRecordingAsync();
            return _lastClip;
        }

        public async Task<IList<RecordedClipInfo>> GetAllRecordingsAsync()
        {
            return await _sessionDatabase.GetAllRecordingsAsync();
        }

        public async Task PlayRecordingAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Invalid file path.", nameof(filePath));

            await _audioCapture.PlayAudioAsync(filePath);
        }

        // ▶️ Stub: Pause playback
        public void PausePlayback()
        {
            Console.WriteLine("PausePlayback not yet implemented.");
            // TODO: Add actual pause logic if supported
        }

        // ⏹️ Stub: Stop playback
        public void StopPlayback()
        {
            Console.WriteLine("StopPlayback not yet implemented.");
            // TODO: Add actual stop logic if supported
        }

        // ⏪ Stub: Seek backward
        public void SeekBackward(TimeSpan amount)
        {
            Console.WriteLine($"SeekBackward by {amount.TotalSeconds} seconds not yet implemented.");
            // TODO: Add actual seek logic if supported
        }

        // ⏩ Stub: Seek forward
        public void SeekForward(TimeSpan amount)
        {
            Console.WriteLine($"SeekForward by {amount.TotalSeconds} seconds not yet implemented.");
            // TODO: Add actual seek logic if supported
        }

        public async Task<double> GetLastRecordingDurationAsync()
        {
            if (string.IsNullOrWhiteSpace(_lastClip?.FilePath))
                return 0;

            return await _audioCapture.GetDurationAsync(_lastClip.FilePath);
        }

        public RecordedClipInfo? GetLastClipInfo() => _lastClip;
    }
}
