using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UI.Controls
{
    public partial class TtUIElement
    {
        private bool IsRenderable()
        {
            if (NeverMeasured || NeverArranged)
                return false;
            if (Visibility == Visibility.Collapsed)
                return false;
            return IsMeasureValid && IsArrangeValid;
        }

        public virtual void Draw(Canvas.TtCanvas canvas, Canvas.TtCanvasDrawBatch batch)
        {

        }
    }
}
