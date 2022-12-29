using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EngineNS.UI.Controls
{
    public enum HorizontalAlignment : sbyte
    {
        Left = 0,
        Center = 1,
        Right = 2,
        Stretch = 3,
    }
    public enum VerticalAlignment : sbyte
    {
        Top = 0,
        Center = 1,
        Bottom = 2,
        Stretch = 3,
    }
    public enum Visibility : sbyte
    {
        Visible = 0,
        Hidden,
        Collapsed,
    }

    public partial class UIElement : IO.ISerializer
    {
        UIElement mParent;
        public UIElement Parent
        {
            get => mParent;
            set
            {
                if (mParent != null) 
                {
                    RemoveFromParent();
                }
                mParent = value;
            }
        }
        internal void RemoveFromParent()
        {
            var panel = mParent as Containers.UContainer;
            if (panel != null)
            {
                panel.Children.Remove(this);
            }
            mParent = null;
        }

        public virtual void Cleanup()
        {

        }

        public void OnPreRead(object tagObject, object hostObject, bool fromXml) { }

        public void OnPropertyRead(object tagObject, PropertyInfo prop, bool fromXml) { }

        Visibility mVisibility = Visibility.Visible;
        public Visibility Visibility
        {
            get => mVisibility;
            set
            {
                mVisibility = value;
            }
        }

        RectangleF mDesignRect;
        public RectangleF DesignRect
        {
            get => mDesignRect;
            protected set
            {
                mDesignRect = value;
            }
        }
        public void SetDesignRect(ref RectangleF rect, bool updateClipRect = false)
        {
            mDesignRect = rect;
            if (updateClipRect)
                UpdateDesignClipRect();
            UpdateLayout();
        }
        RectangleF mDesignClipRect;
        public RectangleF DesignClipRect
        {
            get => mDesignClipRect;
            protected set
            {
                mDesignClipRect = value;
            }
        }
        protected void UpdateDesignClipRect()
        {
            var oldClipRect = DesignClipRect;
            if (Parent != null)
            {
                var parentClipRect = Parent.DesignClipRect;
                DesignClipRect = DesignRect.Intersect(ref parentClipRect);
            }
            else
            {
                DesignClipRect = new RectangleF(DesignRect.Left, DesignRect.Top, System.Math.Abs(DesignRect.Width), System.Math.Abs(DesignRect.Height));
            }
        }
    }
}
