using Microsoft.Maui.Graphics;
using System;

namespace ProVoiceLedger.Graphics
{
    public class ArcWaveformDrawable : IDrawable
    {
        private readonly AmplitudeBuffer _buffer;
        private float _fade = 0f;

        /// <summary>
        /// Controls whether the waveform should fade in or out.
        /// Set externally for visual testing.
        /// </summary>
        public bool IsRecording { get; set; } = false;

        public ArcWaveformDrawable(AmplitudeBuffer buffer)
        {
            _buffer = buffer;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            float centerX = dirtyRect.Center.X;
            float centerY = dirtyRect.Center.Y;
            float maxBarLength = 16f + 30f; // base + max extension
            float radius = MathF.Min(dirtyRect.Width, dirtyRect.Height) / 2f - maxBarLength - 4f;

            var samples = _buffer.GetSamples();
            int barCount = samples.Length;

            // 🎚 Fade logic
            float targetFade = IsRecording ? 1f : 0f;
            _fade = Lerp(_fade, targetFade, 0.08f);

            if (_fade < 0.01f) return;

            // 🧼 Transparent background
            canvas.FillColor = Colors.Transparent;
            canvas.FillRectangle(dirtyRect);

            // 🌀 Center pulse
            canvas.FillColor = Colors.White.WithAlpha(0.03f * _fade);
            canvas.FillCircle(centerX, centerY, 10f);

            // 🌓 Arc range: horseshoe shape (top half)
            float startAngle = 160f;
            float endAngle = 20f;
            float arcSpan = endAngle - startAngle;
            if (arcSpan < 0) arcSpan += 360f;

            for (int i = 0; i < barCount; i++)
            {
                float t = (float)i / barCount;
                float angleDeg = startAngle + t * arcSpan;
                float angleRad = angleDeg * (MathF.PI / 180f);

                // 🎵 Base pattern: long-short rhythm
                float baseLength = (i % 2 == 0) ? 16f : 8f;

                // 🔊 Amplitude-driven extension
                float amplitude = samples[i];
                float extension = amplitude * 30f;
                float barLength = baseLength + extension;

                float thickness = 2f;

                // 🔁 Reversed direction: grow outward
                float x1 = centerX + MathF.Cos(angleRad) * radius;
                float y1 = centerY + MathF.Sin(angleRad) * radius;
                float x2 = centerX + MathF.Cos(angleRad) * (radius + barLength);
                float y2 = centerY + MathF.Sin(angleRad) * (radius + barLength);

                // 🌈 Cyan to violet gradient
                float hue = 200f + t * 80f;
                float alpha = _fade * (0.3f + amplitude * 0.5f);
                var strokeColor = Color.FromHsla(hue / 360f, 0.9f, 0.6f, alpha);

                // ✨ Glow trail
                canvas.StrokeColor = strokeColor.WithAlpha(alpha * 0.2f);
                canvas.StrokeSize = thickness + 4f;
                canvas.DrawLine(x1, y1, x2, y2);

                // 🔷 Core line
                canvas.StrokeColor = strokeColor;
                canvas.StrokeSize = thickness;
                canvas.DrawLine(x1, y1, x2, y2);

                // 💫 Tip glow (optional)
                canvas.FillColor = strokeColor.WithAlpha(0.3f);
                canvas.FillCircle(x2, y2, 2f);
            }
        }

        private float Lerp(float a, float b, float t) => a + (b - a) * t;
    }
}
