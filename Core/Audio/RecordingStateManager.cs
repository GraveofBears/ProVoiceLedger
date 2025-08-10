using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProVoiceLedger.Core.Audio
{
    public enum RecordingState
    {
        Idle,
        Recording,
        Paused,
        Playing
    }

    /// <summary>
    /// Centralized manager for tracking recording/playback state and emitting UI sync events.
    /// </summary>
    public class RecordingStateManager
    {
        public static RecordingStateManager StateManager { get; } = new();

        public RecordingState CurrentState { get; private set; } = RecordingState.Idle;

        public event Action<RecordingState>? OnStateChanged;
        public event Action<TimeSpan>? OnTimeUpdated;
        public event Action<float>? OnAmplitudeUpdated;

        private CancellationTokenSource? _timerCts;

        private RecordingStateManager() { }

        public void SetState(RecordingState newState)
        {
            if (newState == CurrentState) return;

            CurrentState = newState;
            OnStateChanged?.Invoke(newState);

            // 🔁 Force initial amplitude tick for visuals
            if (newState == RecordingState.Recording || newState == RecordingState.Playing)
            {
                OnAmplitudeUpdated?.Invoke(AudioManager.Instance.Amplitude);
            }

            HandleTimerLoop(newState);
        }

        private void HandleTimerLoop(RecordingState state)
        {
            _timerCts?.Cancel();

            if (state == RecordingState.Recording || state == RecordingState.Playing)
            {
                _timerCts = new CancellationTokenSource();
                var token = _timerCts.Token;

                _ = Task.Run(async () =>
                {
                    var time = TimeSpan.Zero;
                    while (!token.IsCancellationRequested)
                    {
                        time += TimeSpan.FromMilliseconds(100);
                        OnTimeUpdated?.Invoke(time);
                        OnAmplitudeUpdated?.Invoke(AudioManager.Instance.Amplitude);
                        await Task.Delay(100);
                    }
                });
            }
        }

        public void Reset()
        {
            _timerCts?.Cancel();
            SetState(RecordingState.Idle);
        }
    }
}
