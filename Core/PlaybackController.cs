using System;
using System.Timers;
using ProVoiceLedger.Core.Models;
using ProVoiceLedger.Core.Services;

namespace ProVoiceLedger.Core.Audio
{
    public class PlaybackController
    {
        private readonly IRecordingService _recordingService;
        private RecordedClipInfo _currentClip;
        private System.Timers.Timer _playbackTimer;
        private DateTime _playbackStartTime;
        private bool _isPlaying;

        public event Action<TimeSpan> PlaybackProgressUpdated;

        public PlaybackController(IRecordingService recordingService)
        {
            _recordingService = recordingService ?? throw new ArgumentNullException(nameof(recordingService));

            _playbackTimer = new System.Timers.Timer(500);
            _playbackTimer.Elapsed += OnPlaybackTimerElapsed;
        }

        public async void Play(RecordedClipInfo clip)
        {
            if (clip == null || string.IsNullOrEmpty(clip.FilePath))
                return;

            _currentClip = clip;
            _playbackStartTime = DateTime.UtcNow;
            _isPlaying = true;

            await _recordingService.PlayRecordingAsync(clip.FilePath);
            _playbackTimer.Start();
        }

        public void Pause()
        {
            _isPlaying = false;
            _playbackTimer.Stop();
            _recordingService.PausePlayback(); // Optional: implement in service
        }

        public void Stop()
        {
            _isPlaying = false;
            _playbackTimer.Stop();
            _recordingService.StopPlayback(); // Optional: implement in service
        }

        public void Rewind(TimeSpan amount)
        {
            _recordingService.SeekBackward(amount); // Optional: implement in service
        }

        public void FastForward(TimeSpan amount)
        {
            _recordingService.SeekForward(amount); // Optional: implement in service
        }

        private void OnPlaybackTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (!_isPlaying || _currentClip == null)
                return;

            var elapsed = DateTime.UtcNow - _playbackStartTime;
            PlaybackProgressUpdated?.Invoke(elapsed);
        }
    }
}
