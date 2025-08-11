using Microsoft.Maui.Graphics;
using System.Collections.Generic;

namespace ProVoiceLedger.Core.Models
{
    public class CustomTouchEventArgs : EventArgs
    {
        public List<PointF> Touches { get; }

        public CustomTouchEventArgs(List<PointF> touches)
        {
            Touches = touches ?? new List<PointF>();
        }
    }
}
