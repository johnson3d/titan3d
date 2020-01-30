using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UISystem.Editor
{
    public class SelectedData
    {
        public EngineNS.UISystem.UIElement UI;
        public EngineNS.UISystem.Controls.Image ShowRect;
        public EngineNS.RectangleF StartRect;
    }

    public enum enCursors
    {
        None,
        ScrollSW,
        ScrollNE,
        ScrollNW,
        ScrollE ,
        ScrollW ,
        ScrollS ,
        ScrollN ,
        ScrollAll,
        ScrollWE ,
        ScrollNS ,
        Pen ,
        Hand,
        Wait,
        UpArrow ,
        SizeWE ,
        SizeNWSE,
        SizeNS ,
        SizeNESW,
        SizeAll,
        IBeam,
        Help ,
        Cross,
        AppStarting,
        Arrow,
        No,
        ScrollSE,
        ArrowCD,
    }
}
