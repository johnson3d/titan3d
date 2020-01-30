using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.UISystem.Style
{
    [Rtti.MetaClass]
    public class ButtonStyle : UIStyle
    {
        [Rtti.MetaData]
        public Brush NormalBrush { get; set; } = new Brush();
        [Rtti.MetaData]
        public Brush HoveredBrush { get; set; } = new Brush();
        [Rtti.MetaData]
        public Brush PressedBrush { get; set; } = new Brush();
        [Rtti.MetaData]
        public Brush DisabledBrush { get; set; } = new Brush();

        public override async Task Initialize(CRenderContext rc, UIElement hostElement)
        {
            await NormalBrush.Initialize(rc, hostElement);
            await HoveredBrush.Initialize(rc, hostElement);
            await PressedBrush.Initialize(rc, hostElement);
            await DisabledBrush.Initialize(rc, hostElement);
        }

        public Vector2 GetImageSize()
        {
            if (NormalBrush != null)
                return NormalBrush.ImageSize;
            else if (HoveredBrush != null)
                return HoveredBrush.ImageSize;
            else if (PressedBrush != null)
                return PressedBrush.ImageSize;
            else if (DisabledBrush != null)
                return DisabledBrush.ImageSize;
            return Vector2.Zero;
        }
    }
}
