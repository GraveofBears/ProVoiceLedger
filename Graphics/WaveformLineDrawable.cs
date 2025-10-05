using Microsoft.Maui.Graphics;
using System;

namespace ProVoiceLedger.Graphics
{
    public class WaveformLineDrawable : IDrawable
    {
        public float Progress { get; set; }
        private readonly float[] _waveformData;

        public WaveformLineDrawable()
        {
            // Generate some sample waveform data
            _waveformData = new float[100];
            var rand = new Random();
            for (int i = 0; i < _waveformData.Length; i++)
            {
                _waveformData[i] = (float)(rand.NextDouble() * 0.6 + 0.2);
            }
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (dirtyRect.Width <= 0 || dirtyRect.Height <= 0)
                return;

            float centerY = dirtyRect.Height / 2;
            float width = dirtyRect.Width;
            float segmentWidth = width / _waveformData.Length;

            // Draw waveform segments
            for (int i = 0; i < _waveformData.Length - 1; i++)
            {
                float x1 = i * segmentWidth;
                float x2 = (i + 1) * segmentWidth;
                float y1 = centerY - (_waveformData[i] * centerY);
                float y2 = centerY - (_waveformData[i + 1] * centerY);

                // Color based on progress
                float segmentProgress = (float)i / _waveformData.Length;
                if (segmentProgress <= Progress)
                {
                    canvas.StrokeColor = Colors.Cyan;
                }
                else
                {
                    canvas.StrokeColor = Colors.Gray.WithAlpha(0.5f);
                }

                canvas.StrokeSize = 2;
                canvas.DrawLine(x1, y1, x2, y2);
            }

            // Draw progress indicator line
            float progressX = Progress * width;
            canvas.StrokeColor = Colors.Red;
            canvas.StrokeSize = 3;
            canvas.DrawLine(progressX, 0, progressX, dirtyRect.Height);
        }

        public void SetWaveformData(float[] data)
        {
            if (data != null && data.Length > 0)
            {
                Array.Copy(data, _waveformData, Math.Min(data.Length, _waveformData.Length));
            }
        }
    }
}