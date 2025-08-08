using System;
using System.Timers;
using ProVoiceLedger.Core.Models;
using ProVoiceLedger.Core.Services;

namespace ProVoiceLedger.Core.Audio
{
    public class PlaybackController
    {
        private readonly IRecordingService _recordingService;
        private readonly System.Timers.Timer _playbackTimer;
        private RecordedClipInfo? _currentClip;
        private DateTime _playbackStartTime;
        private bool _isPlaying;

        /// <summary>🎧 Raised every 500ms with current playback position</summary>
        public event Action<TimeSpan>? PlaybackProgressUpdated;

        public PlaybackController(IRecordingService recordingService)
        {
            _recordingService = recordingService ?? throw new ArgumentNullException(nameof(recordingService));
            _playbackTimer = new System.Timers.Timer(500); // ✅ Explicitly System.Timers.Timer
            _playbackTimer.Elapsed += OnPlaybackTimerElapsed;
        }

        /// <summary>▶️ Begin playback of a recorded clip</summary>
        public async void Play(RecordedClipInfo clip)
        {
            if (string.IsNullOrWhiteSpace(clip.FilePath))
                return;

            _currentClip = clip;
            _playbackStartTime = DateTime.UtcNow;
            _isPlaying = true;

            await _recordingService.PlayRecordingAsync(clip.FilePath);
            _playbackTimer.Start();
        }

        /// <summary>⏸️ Pause playback</summary>
        public void Pause()
        {
            if (!_isPlaying) return;

            _isPlaying = false;
            _playbackTimer.Stop();
            _recordingService.PausePlayback();
        }

        /// <summary>⏹️ Stop playback</summary>
        public void Stop()
        {
            if (!_isPlaying) return;

            _isPlaying = false;
            _playbackTimer.Stop();
            _recordingService.StopPlayback();
        }

        /// <summary>⏪ Rewind by a given amount</summary>
        public void Rewind(TimeSpan amount) => SeekBackward(amount);

        /// <summary>⏩ Fast-forward by a given amount</summary>
        public void FastForward(TimeSpan amount) => SeekForward(amount);

        /// <summary>🔙 Seek backward</summary>
        public void SeekBackward(TimeSpan amount) => _recordingService.SeekBackward(amount);

        /// <summary>🔜 Seek forward</summary>
        public void SeekForward(TimeSpan amount) => _recordingService.SeekForward(amount);

        /// <summary>⏮️ Seek to start of clip</summary>
        public void SeekToStart() => _recordingService.SeekTo(TimeSpan.Zero);

        /// <summary>⏭️ Seek to end of clip</summary>
        public void SeekToEnd()
        {
            if (_currentClip?.Duration is double duration)
            {
                var timeSpan = TimeSpan.FromSeconds(duration); // ✅ Convert double to TimeSpan
                _recordingService.SeekTo(timeSpan);
            }
        }

        /// <summary>⏱️ Timer tick: update playback progress</summary>
        private void OnPlaybackTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            if (!_isPlaying || _currentClip == null)
                return;

            var elapsed = DateTime.UtcNow - _playbackStartTime;
            PlaybackProgressUpdated?.Invoke(elapsed);
        }
    }
}
