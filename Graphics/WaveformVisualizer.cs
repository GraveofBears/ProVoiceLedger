// File: Graphics/WaveformVisualizer.cs
using System;
using System.IO;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using ProVoiceLedger.Graphics;

namespace ProVoiceLedger.Graphics
{
    /// <summary>
    /// Handles waveform rendering using amplitude data and a drawable overlay.
    /// </summary>
    public class WaveformVisualizer
    {
        private readonly GraphicsView _canvas;
        private readonly AmplitudeBuffer _buffer;
        private readonly ArcWaveformDrawable _drawable;

        public WaveformVisualizer(GraphicsView canvas)
        {
            _canvas = canvas;
            _buffer = new AmplitudeBuffer();
            _drawable = new ArcWaveformDrawable(_buffer);
            _canvas.Drawable = _drawable;
        }

        /// <summary>
        /// Resets the waveform by pushing zeroed amplitudes.
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < 64; i++)
                _buffer.Push(0f);

            _canvas.Invalidate();
        }

        /// <summary>
        /// Loads waveform data from a file (stubbed for now).
        /// </summary>
        public void LoadFromFile(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                Clear();
                return;
            }

            // 🔮 Stubbed waveform generation — replace with real decoding later
            var rand = new Random();
            for (int i = 0; i < 64; i++)
            {
                float amplitude = (float)(rand.NextDouble() * 0.8 + 0.2); // Simulated spectral pulse
                _buffer.Push(amplitude);
            }

            _canvas.Invalidate();
        }

        /// <summary>
        /// Adds a live amplitude sample (e.g., during recording).
        /// </summary>
        public void AddLiveSample(float amplitude)
        {
            _buffer.Push(amplitude);
            _canvas.Invalidate();
        }
    }
}
