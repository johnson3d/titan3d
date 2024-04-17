using EngineNS.UI.Bind;
using EngineNS.UI.Controls.Containers;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UI.Controls
{
    // hide in logical tree and visual tree
    public partial class TtContentsPresenter : TtContainer
    {
        //object mContent;
        //[UI.Bind.BindProperty]
        //public object Content
        //{
        //    get => mContent;
        //    set
        //    {
        //        mContent = value;
        //        OnValueChange(value);
        //    }
        //}
        string mContentSource;
        [UI.Bind.BindProperty, Rtti.Meta]
        public string ContentSource
        {
            get => mContentSource;
            set
            {
                OnValueChange(in value, in mContentSource);
                mContentSource = value;
                mContentSourceHash = Standart.Hash.xxHash.xxHash64.ComputeHash(mContentSource);
            }
        }
        UInt64 mContentSourceHash = 0;
        public UInt64 ContentSourceHash => mContentSourceHash;

        //public override Visibility Visibility 
        //{ 
        //    get => base.Visibility;
        //    set
        //    {
        //        base.Visibility = value;
        //        for(int i=this.Children.mChildren.Count - 1; i>=0; i--)
        //        {
        //            var child = Children.mChildren[i];
        //            child.Visibility = value;
        //        }
        //    }
        //}

        protected override SizeF MeasureOverride(in SizeF availableSize)
        {
            // measure with parent measure rule
            return SizeF.Empty;
        }
        protected override void ArrangeOverride(in RectangleF arrangeSize)
        {
        }
        public override void OnChildDesiredSizeChanged(TtUIElement child)
        {
            var parent = VisualTreeHelper.GetParent(this);
            if (parent != null && !parent.MeasureInProgress)
                parent.OnChildDesiredSizeChanged(child);
        }
    }
}
