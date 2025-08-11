using Microsoft.Maui.Graphics;
using System;

namespace ProVoiceLedger.Graphics
{
    public class WaveformEditorDrawable : IDrawable
    {
        public float[] Amplitudes { get; set; } = Array.Empty<float>();
        public float Progress { get; set; } = 0f; // 0.0 to 1.0
        public float? SelectionStart { get; set; } = null;
        public float? SelectionEnd { get; set; } = null;

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (Amplitudes.Length < 2)
                return;

            float width = dirtyRect.Width;
            float height = dirtyRect.Height;
            float centerY = height / 2;
            float xStep = width / (Amplitudes.Length - 1);
            float amplitudeScale = height / 2;

            // 🎧 Waveform polyline
            canvas.StrokeColor = Colors.Cyan;
            canvas.StrokeSize = 2;

            for (int i = 1; i < Amplitudes.Length; i++)
            {
                float x1 = (i - 1) * xStep;
                float y1 = centerY - Amplitudes[i - 1] * amplitudeScale;
                float x2 = i * xStep;
                float y2 = centerY - Amplitudes[i] * amplitudeScale;
                canvas.DrawLine(x1, y1, x2, y2);
            }

            // 🔴 Playback cursor
            float cursorX = Progress * width;
            canvas.StrokeColor = Colors.Red;
            canvas.StrokeSize = 2;
            canvas.DrawLine(cursorX, 0, cursorX, height);

            // ✂️ Selection highlight
            if (SelectionStart.HasValue && SelectionEnd.HasValue)
            {
                float selX = SelectionStart.Value * width;
                float selWidth = (SelectionEnd.Value - SelectionStart.Value) * width;
                canvas.FillColor = Colors.Cyan.WithAlpha(0.2f);
                canvas.FillRectangle(selX, 0, selWidth, height);
            }
        }

        public void OnTap(Point tapPoint, float canvasWidth)
        {
            float percent = (float)(tapPoint.X / canvasWidth);
            Progress = Math.Clamp(percent, 0f, 1f);
            SelectionStart = Math.Clamp(Progress - 0.05f, 0f, 1f);
            SelectionEnd = Math.Clamp(Progress + 0.05f, 0f, 1f);
        }

        public void ClearSelection()
        {
            SelectionStart = null;
            SelectionEnd = null;
        }

        public void SetProgress(float percent)
        {
            Progress = Math.Clamp(percent, 0f, 1f);
        }

        public void SetSelection(float startPercent, float endPercent)
        {
            SelectionStart = Math.Clamp(startPercent, 0f, 1f);
            SelectionEnd = Math.Clamp(endPercent, 0f, 1f);
        }
        public void ShiftProgress(float delta)
        {
            Progress = Math.Clamp(Progress + delta, 0f, 1f);
        }

        public void CutSelection()
        {
            if (!SelectionStart.HasValue || !SelectionEnd.HasValue || Amplitudes.Length < 2)
                return;

            int startIndex = (int)(SelectionStart.Value * Amplitudes.Length);
            int endIndex = (int)(SelectionEnd.Value * Amplitudes.Length);
            startIndex = Math.Clamp(startIndex, 0, Amplitudes.Length - 1);
            endIndex = Math.Clamp(endIndex, startIndex + 1, Amplitudes.Length);

            var newAmplitudes = new float[Amplitudes.Length - (endIndex - startIndex)];
            Array.Copy(Amplitudes, 0, newAmplitudes, 0, startIndex);
            Array.Copy(Amplitudes, endIndex, newAmplitudes, startIndex, Amplitudes.Length - endIndex);
            Amplitudes = newAmplitudes;

            ClearSelection();
        }
    }
}
