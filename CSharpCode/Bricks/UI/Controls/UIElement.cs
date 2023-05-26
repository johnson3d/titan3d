using EngineNS.Rtti;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    [Bind.BindableObject]
    public partial class TtUIElement : IO.ISerializer
    {
        public TtUIElement()
        {
            NeverMeasured = true;
            NeverArranged = true;
        }

        TtUIHost mRootUIHost;
        [Browsable(false)]
        public TtUIHost RootUIHost
        {
            get
            {
                if(mRootUIHost == null)
                {
                    var parent = this;
                    while(parent != null)
                    {
                        if(parent is TtUIHost)
                        {
                            mRootUIHost = parent as TtUIHost;
                            break;
                        }
                        parent = parent.Parent;
                    }
                }
                return mRootUIHost;
            }
            set { mRootUIHost = value; }
        }

        UI.Controls.Containers.TtContainer mParent;
        [Browsable(false)]
        public UI.Controls.Containers.TtContainer Parent
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
            var panel = mParent as Containers.TtContainer;
            if (panel != null)
            {
                RemoveAttachedProperties(panel.GetType());
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
        FTransform mTransform = new FTransform();
        public FTransform Transform { get => mTransform; set => mTransform = value; }
        FTransform mAbsTransform = new FTransform();
        public virtual bool IsPointIn(in Point2f pt)
        {
            // todo: inv transform
            return DesignRect.Contains(in pt);
        }
        public virtual TtUIElement GetPointAtElement(in Point2f pt, bool onlyClipped = true)
        {
            if(IsPointIn(in pt))
                return this;
            return null;
        }

        protected RectangleF mDesignRect;
        public RectangleF DesignRect
        {
            get => mDesignRect;
            protected set
            {
                if(!mDesignRect.Equals(in value))
                {
                    mDesignRect = value;
                    mClipRectDirty = true;
                }
            }
        }
        public void SetDesignRect(in RectangleF rect, bool updateClipRect = false)
        {
            mDesignRect = rect;
            if (updateClipRect)
                UpdateDesignClipRect();
            UpdateLayout();
        }
        protected RectangleF mDesignClipRect = new RectangleF(-float.PositiveInfinity, -float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        public RectangleF DesignClipRect
        {
            get
            {
                if (mClipRectDirty)
                    UpdateDesignClipRect();
                return mDesignClipRect;
            }
            protected set
            {
                mDesignClipRect = value;
            }
        }
        bool mClipRectDirty = false;
        protected void UpdateDesignClipRect()
        {
            var oldClipRect = mDesignClipRect;
            if (Parent != null)
            {
                var parentClipRect = Parent.DesignClipRect;
                mDesignClipRect = DesignRect.Intersect(in parentClipRect);
            }
            else
            {
                mDesignClipRect = new RectangleF(DesignRect.Left, DesignRect.Top, System.Math.Abs(DesignRect.Width), System.Math.Abs(DesignRect.Height));
            }
            mClipRectDirty = false;
        }

        public virtual Vector2 GetPointWith2DSpacePoint(in Vector2 pt)
        {
            return new Vector2(pt.X - DesignRect.X, pt.Y - DesignRect.Y);
        }
    }
}
