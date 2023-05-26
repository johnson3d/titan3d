using EngineNS.UI.Controls;
using EngineNS.UI.Controls.Containers;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UI
{
    public partial class TtUIHost : Controls.Containers.TtContainer
    {
        float mDPIScale = 1.0f;
        public float DPIScale => mDPIScale;

        SizeF mWindowSize;
        public SizeF WindowSize
        {
            get => mWindowSize;
            set
            {
                if (mWindowSize.Equals(in value))
                    return;

                mWindowSize = value;
                SizeF tagDesignSize;
                mDPIScale = UEngine.Instance.UIManager.Config.GetDPIScaleAndDesignSize(mWindowSize.Width, mWindowSize.Height, out tagDesignSize);
                var newRect = new RectangleF(0, 0, tagDesignSize.Width, tagDesignSize.Height);
                SetDesignRect(in newRect, true);
                for(int i=0; i<mChildren.Count; i++)
                {
                    mChildren[i].UpdateLayout();
                }
            }
        }

        public TtUIHost()
        {
            WindowSize = new SizeF(1920, 1080);
            BypassLayoutPolicies = true;
        }

        protected override SizeF MeasureOverride(in SizeF availableSize)
        {
            return mWindowSize;
        }
        protected override void ArrangeOverride(in RectangleF arrangeSize)
        {
            for(int i=0; i<mChildren.Count; i++)
            {
                mChildren[i].Arrange(in arrangeSize);
            }
            mMeshDirty = true;
        }
        public override void AddChild(TtUIElement element, bool updateLayout = true)
        {
            element.RootUIHost = this;
            base.AddChild(element, updateLayout);
            element.UpdateLayout();
        }
        public override void InsertChild(int index, TtUIElement element, bool updateLayout = true)
        {
            element.RootUIHost = this;
            base.InsertChild(index, element, updateLayout);
            element.UpdateLayout();
        }
        public override Vector2 GetPointWith2DSpacePoint(in Vector2 pt)
        {
            var retPt = Vector2.Zero;
            retPt.X = pt.X / mDPIScale - DesignRect.X;
            retPt.Y = pt.Y / mDPIScale - DesignRect.Y;
            return retPt;
        }
    }
}
