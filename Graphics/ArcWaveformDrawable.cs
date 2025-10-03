using System;
using Microsoft.Maui.Graphics;

namespace ProVoiceLedger.Graphics
{
    public class ArcWaveformDrawable : IDrawable
    {
        private readonly AmplitudeBuffer _buffer;

        public bool IsRecording { get; set; }

        /// <summary>
        /// Constructor that accepts an AmplitudeBuffer for real-time visualization
        /// </summary>
        public ArcWaveformDrawable(AmplitudeBuffer buffer)
        {
            _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (!IsRecording)
                return;

            var samples = _buffer.GetSamples();
            var centerX = dirtyRect.Width / 2;
            var centerY = dirtyRect.Height / 2;
            var radius = Math.Min(dirtyRect.Width, dirtyRect.Height) / 2 - 20;

            // Draw arc visualization
            canvas.StrokeColor = Colors.Cyan;
            canvas.StrokeSize = 3;
            canvas.StrokeLineCap = LineCap.Round;

            // Draw the arc waveform
            for (int i = 0; i < samples.Length - 1; i++)
            {
                // Calculate angles for arc position (semi-circle from left to right)
                float angle1 = (float)(Math.PI * 1.0 + (i / (float)samples.Length) * Math.PI);
                float angle2 = (float)(Math.PI * 1.0 + ((i + 1) / (float)samples.Length) * Math.PI);

                // Vary radius based on amplitude
                float r1 = radius + samples[i] * 40;
                float r2 = radius + samples[i + 1] * 40;

                // Calculate points on the arc
                float x1 = centerX + (float)Math.Cos(angle1) * r1;
                float y1 = centerY + (float)Math.Sin(angle1) * r1;
                float x2 = centerX + (float)Math.Cos(angle2) * r2;
                float y2 = centerY + (float)Math.Sin(angle2) * r2;

                canvas.DrawLine(x1, y1, x2, y2);
            }

            // Optional: Draw center circle for visual reference
            canvas.StrokeColor = Colors.Purple;
            canvas.StrokeSize = 2;
            canvas.DrawCircle(centerX, centerY, radius * 0.3f);
        }
    }
}