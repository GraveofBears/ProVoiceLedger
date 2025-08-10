using Microsoft.Maui.Graphics;
using System;

namespace ProVoiceLedger.Graphics
{
    public class VisualizerDrawable : IDrawable
    {
        private readonly AmplitudeBuffer _buffer;
        private float _pulsePhase = 0;

        public VisualizerDrawable(AmplitudeBuffer buffer)
        {
            _buffer = buffer;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            var samples = _buffer.GetSamples();
            int barCount = samples.Length;

            float baseRadius = dirtyRect.Width / 2;
            float centerX = dirtyRect.Center.X;
            float centerY = dirtyRect.Center.Y;

            // Pulse effect: smooth breathing motion
            _pulsePhase += 0.05f;
            float pulseOffset = (float)Math.Sin(_pulsePhase) * 10;
            float radius = baseRadius + pulseOffset;

            for (int i = 0; i < barCount; i++)
            {
                double angle = i * (360.0 / barCount);
                float amplitude = samples[i];
                float length = amplitude * 60f; // scale amplitude
                float thickness = 4;

                var radians = (float)(angle * Math.PI / 180);
                var x1 = centerX + (float)Math.Cos(radians) * (radius - length);
                var y1 = centerY + (float)Math.Sin(radians) * (radius - length);
                var x2 = centerX + (float)Math.Cos(radians) * radius;
                var y2 = centerY + (float)Math.Sin(radians) * radius;

                // Gradient stroke: cyan → magenta
                float t = (float)i / barCount;
                var strokeColor = Color.FromRgba(
                    (1 - t) * 0 + t * 255,   // R: cyan → magenta
                    (1 - t) * 255 + t * 0,   // G: cyan → magenta
                    255,                    // B: constant
                    200                     // Alpha
                );

                // Main stroke
                canvas.StrokeColor = strokeColor;
                canvas.StrokeSize = thickness;
                canvas.DrawLine(x1, y1, x2, y2);

                // Glow layer
                canvas.StrokeColor = strokeColor.WithAlpha(0.2f);
                canvas.StrokeSize = thickness + 6;
                canvas.DrawLine(x1, y1, x2, y2);
            }
        }
    }
}
