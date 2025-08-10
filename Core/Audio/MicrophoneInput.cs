using System;
using System.Timers;

namespace ProVoiceLedger.Core.Audio
{
    public class MicrophoneInput
    {
        private System.Timers.Timer _timer;
        private readonly Random _rand = new();

        public event Action<float>? AmplitudeReceived;

        public void Start()
        {
            _timer = new System.Timers.Timer(33);
            _timer.Elapsed += (s, e) =>
            {
                float amplitude = (float)_rand.NextDouble(); // Simulated mic input
                AmplitudeReceived?.Invoke(amplitude);
            };
            _timer.Start();
        }

        public void Stop()
        {
            _timer?.Stop();
            _timer?.Dispose();
        }
    }
}
