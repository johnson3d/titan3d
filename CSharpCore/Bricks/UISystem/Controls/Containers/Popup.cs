using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UISystem.Controls.Containers
{
    [Rtti.MetaClass]
    public class PopupInitializer : Containers.BorderInitializer
    {

    }
    [Editor_UIControlInit(typeof(UserControlInitializer))]
    public class Popup : Containers.Border
    {
        public bool IsPointIn(ref PointF pt)
        {
            if (Content == null)
                return false;
            if (Content.DesignRect.Contains(ref pt))
                return true;
            return false;
        }
    }
}
