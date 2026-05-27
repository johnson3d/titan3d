using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui
{
    public class UCoreStyles
    {
        public static UCoreStyles Instance { get; } = new UCoreStyles();
        public Color4b SnapBorderColor { get; set; } = Color4b.Green;
        public int SnapRounding { get; set; } = 3;
        public int SnapThinkness { get; set; } = 1;
        public Color4b LogInfoColor { get; set; } = Color4b.FromRgb(79, 163, 206);
        public Color4b LogWarningColor { get; set; } = Color4b.FromRgb(242, 201, 76);
        public Color4b LogErrorColor { get; set; } = Color4b.FromRgb(249, 112, 102);
        public Color4b LogFatalColor { get; set; } = Color4b.FromRgb(239, 68, 68);
    }
}
