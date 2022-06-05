using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui
{
    public class UCoreStyles
    {
        public static UCoreStyles Instance { get; } = new UCoreStyles();
        public Color SnapBorderColor { get; set; } = Color.Green;
        public int SnapRounding { get; set; } = 10;
        public int SnapThinkness { get; set; } = 1;
        public Color LogInfoColor { get; set; } = Color.Green;
        public Color LogWarningColor { get; set; } = Color.DarkRed;
        public Color LogErrorColor { get; set; } = Color.Red;
        public Color LogFatalColor { get; set; } = Color.Gold;
    }
}
