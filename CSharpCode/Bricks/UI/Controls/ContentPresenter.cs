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

        protected override SizeF MeasureOverride(in SizeF availableSize)
        {
            // measure with parent measure rule
            //this.Children
            return base.MeasureOverride(availableSize);
        }
        protected override void ArrangeOverride(in RectangleF arrangeSize)
        {
            base.ArrangeOverride(arrangeSize);
        }
    }
}
