using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.UISystem.Controls.Containers
{
    [Rtti.MetaClass]
    public class CanvasPanelInitializer : PanelInitializer
    {

    }
    [Editor_UIControlInit(typeof(CanvasPanelInitializer))]
    [Editor_UIControl("容器.Canvas", "", "Canvas.png")]
    public class CanvasPanel : Panel
    {
        public override Task<bool> Initialize(CRenderContext rc, UIElementInitializer init)
        {
            //IsLayoutIslandRoot = true;
            return base.Initialize(rc, init);
        }
        public override void AddChild(UIElement element, bool updateLayout = true)
        {
            base.AddChild(element, updateLayout);
            if(element.Slot != null)
            {
                System.Diagnostics.Debug.Assert(element.Slot.GetType() == typeof(CanvasSlot));
                element.Slot.Parent = this;
                element.Slot.Content = element;
            }
            else
            {
                var slot = new CanvasSlot();
                slot.Parent = this;
                slot.Content = element;
                element.Slot = slot;
            }
        }
        public override void InsertChild(int index, UIElement element, bool updateLayout = true)
        {
            base.InsertChild(index, element, updateLayout);
            if (element.Slot != null)
            {
                System.Diagnostics.Debug.Assert(element.Slot.GetType() == typeof(CanvasSlot));
                element.Slot.Parent = this;
                element.Slot.Content = element;
            }
            else
            {
                var slot = new CanvasSlot();
                slot.Parent = this;
                slot.Content = element;
                element.Slot = slot;
            }
        }

        public override SizeF MeasureOverride(ref SizeF availableSize)
        {
            var designRect = DesignRect;
            float maxWidth = availableSize.Width;
            float maxHeight = availableSize.Height;
            for(int i=0; i<ChildrenUIElements.Count; i++)
            {
                var child = ChildrenUIElements[i];
                child.Measure(ref availableSize);
                var slot = child.Slot as CanvasSlot;
                var desiredSize = child.DesiredSize;
                maxWidth = System.Math.Max(maxWidth, desiredSize.Width);
                maxHeight = System.Math.Max(maxHeight, desiredSize.Height);
            }
            return new SizeF(maxWidth, maxHeight);
        }
    }
}
