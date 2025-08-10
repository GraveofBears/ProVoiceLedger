// /ProVoiceLedger/Controls/WaveformLineDrawable.cs
namespace ProVoiceLedger.Controls
{
    public class WaveformLineDrawable : IDrawable
    {
        public float Progress { get; set; }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            // Draw line based on Progress
        }
    }
}
