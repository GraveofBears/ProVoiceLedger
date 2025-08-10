using Microsoft.Maui.Graphics;
using System;

namespace ProVoiceLedger.Graphics
{
    public class WaveformLineDrawable : IDrawable
    {
        public float Progress { get; set; } = 0f; // 0.0 to 1.0
        public int BarCount { get; set; } = 64;
        public float[] Amplitudes { get; set; } = Array.Empty<float>();

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (Amplitudes.Length != BarCount)
            {
                Amplitudes = new float[BarCount];
                var rand = new Random();
                for (int i = 0; i < BarCount; i++)
                    Amplitudes[i] = (float)rand.NextDouble();
            }

            float barWidth = dirtyRect.Width / BarCount;
            float maxHeight = dirtyRect.Height;

            for (int i = 0; i < BarCount; i++)
            {
                float x = i * barWidth;
                float height = Amplitudes[i] * maxHeight;
                float y = (maxHeight - height) / 2;

                // Color gradient: cyan to blue
                float t = (float)i / BarCount;
                Color color = Color.FromRgba(
                    Lerp(0.0f, 0.0f, t),   // R
                    Lerp(1.0f, 0.4f, t),   // G
                    Lerp(1.0f, 1.0f, t),   // B
                    1.0f                   // A
                );

                // Highlight progress
                if (i < Progress * BarCount)
                    color = color.WithAlpha(1.0f);
                else
                    color = color.WithAlpha(0.3f);

                canvas.FillColor = color;
                canvas.FillRectangle(x, y, barWidth * 0.8f, height);
            }
        }

        private float Lerp(float a, float b, float t) => a + (b - a) * t;
    }
}
