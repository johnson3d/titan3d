using EngineNS.UI.Canvas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.UI.Controls.Containers
{
    //public abstract class TtUIContainerSlot : IO.ISerializer
    //{
    //    public TtUIElement Parent;
    //    public TtUIElement Content;

    //    public abstract EngineNS.SizeF Measure(ref EngineNS.SizeF availableSize);
    //    public abstract void Arrange(ref EngineNS.RectangleF arrangeSize);
    //    public abstract void ProcessSetContentDesignRect(ref EngineNS.RectangleF tagRect);
    //    public abstract bool NeedUpdateLayoutWhenChildDesiredSizeChanged(TtUIElement child);
    //    public virtual Type GetSlotOperatorType() { return null; }

    //    public void OnPreRead(object tagObject, object hostObject, bool fromXml) {}
    //    public void OnPropertyRead(object tagObject, PropertyInfo prop, bool fromXml) {}
    //}

    //public class TtUIDefaultSlot : TtUIContainerSlot
    //{
    //    SizeF mContentDesiredSize;
    //    public override void Arrange(ref RectangleF arrangeSize)
    //    {
    //        var rect = Parent.DesignRect;
    //        rect.Width = (mContentDesiredSize.Width == 0) ? rect.Width : mContentDesiredSize.Width;
    //        rect.Height = (mContentDesiredSize.Height == 0) ? rect.Height : mContentDesiredSize.Height;
    //        Content.ArrangeOverride(ref rect);
    //    }

    //    public override SizeF Measure(ref SizeF availableSize)
    //    {
    //        var size = Parent.DesignRect.Size;
    //        mContentDesiredSize = Content.MeasureOverride(ref size);
    //        return mContentDesiredSize;
    //    }

    //    public override bool NeedUpdateLayoutWhenChildDesiredSizeChanged(TtUIElement child)
    //    {
    //        return false;
    //    }

    //    public override void ProcessSetContentDesignRect(ref RectangleF tagRect)
    //    {
    //        Content.SetDesignRect(ref tagRect);
    //    }
    //}

    public class TtContainer : TtUIElement
    {
        protected List<TtUIElement> mChildren = new List<TtUIElement>();
        public List<TtUIElement> Children => mChildren;

        public override void Cleanup()
        {
            for(int i=0; i<mChildren.Count; i++)
            {
                mChildren[i].Cleanup();
            }
            base.Cleanup();
        }

        public override TtUIElement GetPointAtElement(in Point2f pt, bool onlyClipped = true)
        {
            // todo: inv transform
            if (onlyClipped)
            {
                if (!DesignRect.Contains(in pt))
                    return null;
                for (int i = mChildren.Count - 1; i >= 0; i--)
                {
                    var child = mChildren[i];
                    if (!child.DesignRect.Contains(in pt))
                        continue;

                    var container = child as TtContainer;
                    if (container != null)
                    {
                        var retVal = container.GetPointAtElement(in pt, onlyClipped);
                        if (retVal != null)
                            return retVal;
                    }
                    else
                        return child;
                }
                return this;
            }
            else
            {
                for (int i = mChildren.Count - 1; i >= 0; i--)
                {
                    var child = mChildren[i];

                    var container = child as TtContainer;
                    if (container != null)
                    {
                        var retVal = container.GetPointAtElement(in pt, onlyClipped);
                        if (retVal != null)
                            return retVal;
                    }
                    else if(child.DesignRect.Contains(in pt))
                        return child;
                }
                if (DesignRect.Contains(in pt))
                    return this;
                return null;
            }
        }

        public virtual void ClearChildren(bool updateLayout = true)
        {
            for(int i=mChildren.Count - 1; i >= 0; i--)
            {
                mChildren[i].RemoveFromParent();
            }
            mChildren.Clear();
            if (updateLayout)
                UpdateLayout();
        }
        public virtual void AddChild(TtUIElement element, bool updateLayout = true)
        {
            if (element == null)
                return;
            element.RootUIHost = RootUIHost;
            mChildren.Add(element);
            element.Parent = this;
            SetAttachedProperties(element);
            if (updateLayout)
                UpdateLayout();
        }
        public virtual void InsertChild(int index, TtUIElement element, bool updateLayout = true)
        {
            if (element == null)
                return;
            element.RootUIHost = RootUIHost;
            if (index >= mChildren.Count)
                mChildren.Add(element);
            else
                mChildren.Insert(index, element);
            element.Parent = this;
            SetAttachedProperties(element);
            if (updateLayout) 
                UpdateLayout();
        }
        public virtual void RemoveChild(TtUIElement element, bool updateLayout = true) 
        {
            if(element != null)
            {
                element.RemoveFromParent();
                if(updateLayout)
                    UpdateLayout();
            }
        }
        public virtual bool NeedUpdateLayoutWhenChildDesiredSizeChanged(TtUIElement child)
        {
            return false;
        }
        public virtual void OnChildDesiredSizeChanged(TtUIElement child)
        {
            if (IsMeasureValid)
            {
                if (NeedUpdateLayoutWhenChildDesiredSizeChanged(child))
                    InvalidateMeasure();
            }
        }

        public override void Draw(TtCanvas canvas, TtCanvasDrawBatch batch)
        {
            for(int i=0; i<Children.Count; i++)
            {
                Children[i].Draw(canvas, batch);
            }
        }
    }
}
