using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace EngineNS.UI.Controls.Containers
{
    public abstract class UIContainerSlot : IO.ISerializer
    {
        public UIElement Parent;

        public UIElement Content;

        public abstract EngineNS.SizeF Measure(ref EngineNS.SizeF availableSize);
        public abstract void Arrange(ref EngineNS.RectangleF arrangeSize);
        public abstract void ProcessSetContentDesignRect(ref EngineNS.RectangleF tagRect);
        public abstract bool NeedUpdateLayoutWhenChildDesiredSizeChanged(UIElement child);
        public virtual Type GetSlotOperatorType() { return null; }

        public void OnPreRead(object tagObject, object hostObject, bool fromXml) {}
        public void OnPropertyRead(object tagObject, PropertyInfo prop, bool fromXml) {}
    }

    public class UContainer : UIElement
    {
        protected List<UIElement> mChildren = new List<UIElement>();
        public List<UIElement> Children => mChildren;

        public override void Cleanup()
        {
            for(int i=0; i<mChildren.Count; i++)
            {
                mChildren[i].Cleanup();
            }
            base.Cleanup();
        }
    }
}
