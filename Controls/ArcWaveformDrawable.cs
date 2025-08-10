// /ProVoiceLedger/Controls/ArcWaveformDrawable.cs
namespace ProVoiceLedger.Controls
{
    public class ArcWaveformDrawable : IDrawable
    {
        public float Amplitude { get; set; }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            // Draw arc based on Amplitude
        }
    }
}
