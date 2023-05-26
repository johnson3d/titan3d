using EngineNS.EGui.Controls.PropertyGrid;
using EngineNS.UI.Bind;
using NPOI.OpenXmlFormats.Dml;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UI.Controls.Containers
{
    [Editor_UIControl("Container.Canvas", "Canvas", "")]
    public partial class TtCanvasControl : TtContainer
    {
        [Bind.AttachedProperty(Name = "AnchorMin", Category = "Layout(Canvas)")]
        static void OnChildAnchorMinChanged(IBindableObject element, TtBindableProperty property, Vector2 value)
        {
            var ui = element as TtUIElement;
            if(ui != null)
            {
                var canvas = ui.Parent as TtCanvasControl;
                canvas?.InvalidateArrange();
            }
        }

        [Bind.AttachedProperty(Name = "AnchorMax", Category = "Layout(Canvas)")]
        static void OnChildAnchorMaxChanged(IBindableObject element, TtBindableProperty property, Vector2 value)
        {
            var ui = element as TtUIElement;
            if (ui != null)
            {
                var canvas = ui.Parent as TtCanvasControl;
                canvas?.InvalidateArrange();
            }
        }

        class AnchorRectXDisplayNameAttribute : BindPropertyDisplayNameAttribute
        {
            public override string GetDisplayName(IBindableObject element)
            {
                var anchorMin = TtCanvasControl.GetAnchorMin(element);
                var anchorMax = TtCanvasControl.GetAnchorMax(element);
                if ((anchorMax.X - anchorMin.X) > MathHelper.Epsilon)
                    return "OffsetLeft";
                else
                    return "LocationX";
            }
        }
        [Bind.AttachedProperty(Name = "AnchorRectX", Category = "Layout(Canvas)")]
        [AnchorRectXDisplayName]
        static void OnChildAnchorRectXChanged(IBindableObject element, TtBindableProperty property, float value)
        {
            var ui = element as TtUIElement;
            if (ui != null)
            {
                var canvas = ui.Parent as TtCanvasControl;
                canvas?.InvalidateArrange();
            }
        }

        class AnchorRectYDisplayNameAttribute : BindPropertyDisplayNameAttribute
        {
            public override string GetDisplayName(IBindableObject element)
            {
                var anchorMin = TtCanvasControl.GetAnchorMin(element);
                var anchorMax = TtCanvasControl.GetAnchorMax(element);
                if ((anchorMax.Y - anchorMin.Y) > MathHelper.Epsilon)
                    return "OffsetTop";
                else
                    return "LocationY";
            }
        }
        [AnchorRectYDisplayName]
        [Bind.AttachedProperty(Name = "AnchorRectY", Category = "Layout(Canvas)")]
        static void OnChildAnchorRectYChanged(IBindableObject element, TtBindableProperty property, float value)
        {
            var ui = element as TtUIElement;
            if (ui != null)
            {
                var canvas = ui.Parent as TtCanvasControl;
                canvas?.InvalidateArrange();
            }
        }
        class AnchorRectZDisplayNameAttribute : BindPropertyDisplayNameAttribute
        {
            public override string GetDisplayName(IBindableObject element)
            {
                var anchorMin = TtCanvasControl.GetAnchorMin(element);
                var anchorMax = TtCanvasControl.GetAnchorMax(element);
                if ((anchorMax.X - anchorMin.X) > MathHelper.Epsilon)
                    return "OffsetRight";
                else
                    return "SizeX";
            }
        }
        [AnchorRectZDisplayName]
        [Bind.AttachedProperty(Name = "AnchorRectZ", DefaultValue = 100.0f, Category = "Layout(Canvas)")]
        static void OnChildAnchorRectZChanged(IBindableObject element, TtBindableProperty property, float value)
        {
            var ui = element as TtUIElement;
            if (ui != null)
            {
                var canvas = ui.Parent as TtCanvasControl;
                canvas?.InvalidateArrange();
            }
        }
        class AnchorRectWDisplayNameAttribute : BindPropertyDisplayNameAttribute
        {
            public override string GetDisplayName(IBindableObject element)
            {
                var anchorMin = TtCanvasControl.GetAnchorMin(element);
                var anchorMax = TtCanvasControl.GetAnchorMax(element);
                if ((anchorMax.Y - anchorMin.Y) > MathHelper.Epsilon)
                    return "OffsetBottom";
                else
                    return "SizeY";
            }
        }
        [AnchorRectWDisplayName]
        [Bind.AttachedProperty(Name = "AnchorRectW", DefaultValue = 100.0f, Category = "Layout(Canvas)")]
        static void OnChildAnchorRectWChanged(IBindableObject element, TtBindableProperty property, float value)
        {
            var ui = element as TtUIElement;
            if (ui != null)
            {
                var canvas = ui.Parent as TtCanvasControl;
                canvas?.InvalidateArrange();
            }
        }

        [Bind.AttachedProperty(Name = "AnchorCenter", Category = "Layout(Canvas)")]
        static void OnChildAnchorCenterChanged(IBindableObject element, TtBindableProperty property, Vector2 value)
        {
            var ui = element as TtUIElement;
            if (ui != null)
            {
                var canvas = ui.Parent as TtCanvasControl;
                canvas?.InvalidateArrange();
            }
        }

        protected override SizeF MeasureOverride(in SizeF availableSize)
        {
            for(int i=0; i<mChildren.Count; i++)
            {
                var childUI = mChildren[i];
                var anchorMin = TtCanvasControl.GetAnchorMin(childUI);
                var anchorMax = TtCanvasControl.GetAnchorMax(childUI);
                var anchorRectX = TtCanvasControl.GetAnchorRectX(childUI);
                var anchorRectY = TtCanvasControl.GetAnchorRectY(childUI);
                var anchorRectZ = TtCanvasControl.GetAnchorRectZ(childUI);
                var anchorRectW = TtCanvasControl.GetAnchorRectW(childUI);
                var anchorMinX = availableSize.Width * anchorMin.X;
                var anchorMinY = availableSize.Height * anchorMin.Y;
                var anchorMaxX = availableSize.Width * anchorMax.X;
                var anchorMaxY = availableSize.Height * anchorMax.Y;

                float width, height;
                if((anchorMax.X - anchorMin.X) <= MathHelper.Epsilon)
                    width = anchorRectZ;
                else
                    width = anchorMaxX - anchorMinX - anchorRectX - anchorRectZ;

                if ((anchorMax.Y - anchorMin.Y) <= MathHelper.Epsilon)
                    height = anchorRectW;
                else
                    height = anchorMaxY - anchorMinY - anchorRectY - anchorRectW;

                var childAvailableSize = new SizeF(width, height);
                childUI.Measure(in childAvailableSize);
            }
            return availableSize;
        }
        protected override void ArrangeOverride(in RectangleF arrangeSize)
        {
            for(int i=0; i<mChildren.Count; i++)
            {
                var childUI = mChildren[i];
                var anchorMin = TtCanvasControl.GetAnchorMin(childUI);
                var anchorMax = TtCanvasControl.GetAnchorMax(childUI);
                var anchorRectX = TtCanvasControl.GetAnchorRectX(childUI);
                var anchorRectY = TtCanvasControl.GetAnchorRectY(childUI);
                var anchorRectZ = TtCanvasControl.GetAnchorRectZ(childUI);
                var anchorRectW = TtCanvasControl.GetAnchorRectW(childUI);
                var anchorCenter = TtCanvasControl.GetAnchorCenter(childUI);
                var anchorMinX = arrangeSize.Width * anchorMin.X;
                var anchorMinY = arrangeSize.Height * anchorMin.Y;
                var anchorMaxX = arrangeSize.Width * anchorMax.X;
                var anchorMaxY = arrangeSize.Height * anchorMax.Y;

                float posX, posY, width, height;
                if ((anchorMax.X - anchorMin.X) <= MathHelper.Epsilon)
                {
                    width = anchorRectZ;
                    posX = arrangeSize.X + anchorMinX + anchorRectX - width * anchorCenter.X;
                }
                else
                {
                    width = anchorMaxX - anchorMinX - anchorRectX - anchorRectZ;
                    posX = arrangeSize.X + anchorMinX + anchorRectX;
                }

                if ((anchorMax.Y - anchorMin.Y) <= MathHelper.Epsilon)
                {
                    height = anchorRectW;
                    posY = arrangeSize.Y + anchorMinY + anchorRectY - height * anchorCenter.Y;
                }
                else
                {
                    height = anchorMaxY - anchorMinY - anchorRectY - anchorRectW;
                    posY = arrangeSize.Y + anchorMinY + anchorRectY;
                }

                var childArrangeSize = new RectangleF(posX, posY, width, height);
                childUI.Arrange(in childArrangeSize);
            }
        }
    }
}
