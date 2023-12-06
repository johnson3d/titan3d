using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UI.Controls
{
    public partial class TtUIElement
    {
        public EGui.UIProxy.MenuItemProxy.MenuState HierachyContextMenuState;

        partial void TtUIElementConstructor_Editor()
        {
            HierachyContextMenuState.Reset();
        }
    }
}
