using System;

namespace ProVoiceLedger.Graphics
{
    public class AmplitudeBuffer
    {
        private readonly float[] _samples;
        private int _index;

        private readonly Random _random = new();
        private float _lastAmplitude = 0f;
        private float _pulsePhase = 0f;

        private readonly object _lock = new(); // 🔒 Thread safety lock

        /// <summary>
        /// Indicates whether audio is actively being recorded.
        /// Used for visual fade-in/out.
        /// </summary>
        public bool IsRecording { get; set; } = false;

        public AmplitudeBuffer(int size = 64)
        {
            _samples = new float[size];
            _index = 0;
        }

        /// <summary>
        /// Push a new amplitude value into the buffer.
        /// </summary>
        public void Push(float amplitude)
        {
            lock (_lock)
            {
                _samples[_index] = amplitude;
                _index = (_index + 1) % _samples.Length;
                _lastAmplitude = amplitude;
            }
        }

        /// <summary>
        /// Get samples in chronological order.
        /// </summary>
        public float[] GetSamples()
        {
            lock (_lock)
            {
                var result = new float[_samples.Length];
                for (int i = 0; i < _samples.Length; i++)
                {
                    int idx = (_index + i) % _samples.Length;
                    result[i] = _samples[idx];
                }
                return result;
            }
        }

        /// <summary>
        /// Simulate or decay amplitude over time.
        /// Call this in animation loop if no live input.
        /// </summary>
        public void Update()
        {
            // 🎚️ Decay last amplitude
            _lastAmplitude *= 0.92f;

            // 🌊 Smooth synthetic pulse using sine wave
            _pulsePhase += 0.1f;
            if (_pulsePhase > MathF.PI * 2f) _pulsePhase -= MathF.PI * 2f;

            float wave = MathF.Sin(_pulsePhase) * 0.5f + 0.5f;
            float noise = (float)_random.NextDouble() * 0.2f;
            float amplitude = MathF.Max(_lastAmplitude, wave * 0.4f + noise);

            Push(amplitude);
        }

        /// <summary>
        /// Public method to update buffer during recording.
        /// Replace this with real mic input later.
        /// </summary>
        public void UpdateFromMic(float amplitude)
        {
            Push(amplitude);
        }
    }
}
