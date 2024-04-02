using System;
using System.Collections.Generic;
using EngineNS.UI.Canvas;
using EngineNS.Rtti;

namespace EngineNS.UI.Controls.Containers
{
    public partial class TtTabItem : TtContainer
    {
        public TtContainer TabContent;
    }

    [Editor_UIControl("Container.TabControl", "TabControl", "")]
    public partial class TtTabControl : TtContainer
    {
        ELayout_Orientation mOrientation = ELayout_Orientation.Horizontal;
        [Meta]
        public ELayout_Orientation Orientation
        {
            get => mOrientation;
            set
            {
                OnValueChange(value, mOrientation);
                mOrientation = value;
            }
        }
        public TtTabControl()
        {
            mBorderThickness = new Thickness(1, 1, 1, 1);
        }
        public TtTabControl(TtContainer parent)
            : base(parent)
        {
            mBorderThickness = new Thickness(1, 1, 1, 1);
        }
        public int CurrentTab = 0;
    }
}
