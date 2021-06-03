using System;

namespace EngineNS
{
    public struct IntColor
    {
        public Byte R;
        public Byte G;
        public Byte B;
        public Byte A;
    };

    //
    // 摘要:
    //     指定已知的系统颜色。
    public enum KnownColor
    {
        //
        // 摘要:
        //     活动窗口边框的系统定义颜色。
        ActiveBorder = 1,
        //
        // 摘要:
        //     活动窗口标题栏背景的系统定义颜色。
        ActiveCaption = 2,
        //
        // 摘要:
        //     活动窗口标题栏中文本的系统定义颜色。
        ActiveCaptionText = 3,
        //
        // 摘要:
        //     应用程序工作区的系统定义颜色。应用程序工作区是多文档视图中未被文档占据的区域。
        AppWorkspace = 4,
        //
        // 摘要:
        //     三维元素的系统定义表面颜色。
        Control = 5,
        //
        // 摘要:
        //     三维元素的系统定义阴影颜色。阴影颜色应用于三维元素背向光源的部分。
        ControlDark = 6,
        //
        // 摘要:
        //     系统定义的颜色，是三维元素的暗阴影颜色。暗阴影颜色应用于三维元素颜色最深的部分。
        ControlDarkDark = 7,
        //
        // 摘要:
        //     系统定义的颜色，是三维元素的亮色。亮色应用于三维元素面向光源的部分。
        ControlLight = 8,
        //
        // 摘要:
        //     三维元素的系统定义高光点颜色。高光点颜色应用于三维元素的颜色最亮的部分。
        ControlLightLight = 9,
        //
        // 摘要:
        //     三维元素中文本的系统定义颜色。
        ControlText = 10,
        //
        // 摘要:
        //     桌面的系统定义颜色。
        Desktop = 11,
        //
        // 摘要:
        //     浅灰色文本的系统定义颜色。列表中已禁用的项用浅灰色文本显示。
        GrayText = 12,
        //
        // 摘要:
        //     选定项背景的系统定义颜色。选定项包括选定菜单项和选定文本。
        Highlight = 13,
        //
        // 摘要:
        //     选定项文本的系统定义颜色。
        HighlightText = 14,
        //
        // 摘要:
        //     用于指定热跟踪项的系统定义颜色。单击一个热跟踪项会执行该项。
        HotTrack = 15,
        //
        // 摘要:
        //     非活动窗口边框的系统定义颜色。
        InactiveBorder = 16,
        //
        // 摘要:
        //     非活动窗口标题栏背景的系统定义颜色。
        InactiveCaption = 17,
        //
        // 摘要:
        //     非活动窗口标题栏文本的系统定义颜色。
        InactiveCaptionText = 18,
        //
        // 摘要:
        //     工具提示背景的系统定义颜色。
        Info = 19,
        //
        // 摘要:
        //     工具提示文本的系统定义颜色。
        InfoText = 20,
        //
        // 摘要:
        //     菜单背景的系统定义颜色。
        Menu = 21,
        //
        // 摘要:
        //     菜单文本的系统定义颜色。
        MenuText = 22,
        //
        // 摘要:
        //     滚动条背景的系统定义颜色。
        ScrollBar = 23,
        //
        // 摘要:
        //     窗口的工作区中背景的系统定义颜色。
        Window = 24,
        //
        // 摘要:
        //     窗口框架的系统定义颜色。
        WindowFrame = 25,
        //
        // 摘要:
        //     窗口的工作区中文本的系统定义颜色。
        WindowText = 26,
        //
        // 摘要:
        //     系统定义的颜色。
        Transparent = 27,
        //
        // 摘要:
        //     系统定义的颜色。
        AliceBlue = 28,
        //
        // 摘要:
        //     系统定义的颜色。
        AntiqueWhite = 29,
        //
        // 摘要:
        //     系统定义的颜色。
        Aqua = 30,
        //
        // 摘要:
        //     系统定义的颜色。
        Aquamarine = 31,
        //
        // 摘要:
        //     系统定义的颜色。
        Azure = 32,
        //
        // 摘要:
        //     系统定义的颜色。
        Beige = 33,
        //
        // 摘要:
        //     系统定义的颜色。
        Bisque = 34,
        //
        // 摘要:
        //     系统定义的颜色。
        Black = 35,
        //
        // 摘要:
        //     系统定义的颜色。
        BlanchedAlmond = 36,
        //
        // 摘要:
        //     系统定义的颜色。
        Blue = 37,
        //
        // 摘要:
        //     系统定义的颜色。
        BlueViolet = 38,
        //
        // 摘要:
        //     系统定义的颜色。
        Brown = 39,
        //
        // 摘要:
        //     系统定义的颜色。
        BurlyWood = 40,
        //
        // 摘要:
        //     系统定义的颜色。
        CadetBlue = 41,
        //
        // 摘要:
        //     系统定义的颜色。
        Chartreuse = 42,
        //
        // 摘要:
        //     系统定义的颜色。
        Chocolate = 43,
        //
        // 摘要:
        //     系统定义的颜色。
        Coral = 44,
        //
        // 摘要:
        //     系统定义的颜色。
        CornflowerBlue = 45,
        //
        // 摘要:
        //     系统定义的颜色。
        Cornsilk = 46,
        //
        // 摘要:
        //     系统定义的颜色。
        Crimson = 47,
        //
        // 摘要:
        //     系统定义的颜色。
        Cyan = 48,
        //
        // 摘要:
        //     系统定义的颜色。
        DarkBlue = 49,
        //
        // 摘要:
        //     系统定义的颜色。
        DarkCyan = 50,
        //
        // 摘要:
        //     系统定义的颜色。
        DarkGoldenrod = 51,
        //
        // 摘要:
        //     系统定义的颜色。
        DarkGray = 52,
        //
        // 摘要:
        //     系统定义的颜色。
        DarkGreen = 53,
        //
        // 摘要:
        //     系统定义的颜色。
        DarkKhaki = 54,
        //
        // 摘要:
        //     系统定义的颜色。
        DarkMagenta = 55,
        //
        // 摘要:
        //     系统定义的颜色。
        DarkOliveGreen = 56,
        //
        // 摘要:
        //     系统定义的颜色。
        DarkOrange = 57,
        //
        // 摘要:
        //     系统定义的颜色。
        DarkOrchid = 58,
        //
        // 摘要:
        //     系统定义的颜色。
        DarkRed = 59,
        //
        // 摘要:
        //     系统定义的颜色。
        DarkSalmon = 60,
        //
        // 摘要:
        //     系统定义的颜色。
        DarkSeaGreen = 61,
        //
        // 摘要:
        //     系统定义的颜色。
        DarkSlateBlue = 62,
        //
        // 摘要:
        //     系统定义的颜色。
        DarkSlateGray = 63,
        //
        // 摘要:
        //     系统定义的颜色。
        DarkTurquoise = 64,
        //
        // 摘要:
        //     系统定义的颜色。
        DarkViolet = 65,
        //
        // 摘要:
        //     系统定义的颜色。
        DeepPink = 66,
        //
        // 摘要:
        //     系统定义的颜色。
        DeepSkyBlue = 67,
        //
        // 摘要:
        //     系统定义的颜色。
        DimGray = 68,
        //
        // 摘要:
        //     系统定义的颜色。
        DodgerBlue = 69,
        //
        // 摘要:
        //     系统定义的颜色。
        Firebrick = 70,
        //
        // 摘要:
        //     系统定义的颜色。
        FloralWhite = 71,
        //
        // 摘要:
        //     系统定义的颜色。
        ForestGreen = 72,
        //
        // 摘要:
        //     系统定义的颜色。
        Fuchsia = 73,
        //
        // 摘要:
        //     系统定义的颜色。
        Gainsboro = 74,
        //
        // 摘要:
        //     系统定义的颜色。
        GhostWhite = 75,
        //
        // 摘要:
        //     系统定义的颜色。
        Gold = 76,
        //
        // 摘要:
        //     系统定义的颜色。
        Goldenrod = 77,
        //
        // 摘要:
        //     系统定义的颜色。
        Gray = 78,
        //
        // 摘要:
        //     系统定义的颜色。
        Green = 79,
        //
        // 摘要:
        //     系统定义的颜色。
        GreenYellow = 80,
        //
        // 摘要:
        //     系统定义的颜色。
        Honeydew = 81,
        //
        // 摘要:
        //     系统定义的颜色。
        HotPink = 82,
        //
        // 摘要:
        //     系统定义的颜色。
        IndianRed = 83,
        //
        // 摘要:
        //     系统定义的颜色。
        Indigo = 84,
        //
        // 摘要:
        //     系统定义的颜色。
        Ivory = 85,
        //
        // 摘要:
        //     系统定义的颜色。
        Khaki = 86,
        //
        // 摘要:
        //     系统定义的颜色。
        Lavender = 87,
        //
        // 摘要:
        //     系统定义的颜色。
        LavenderBlush = 88,
        //
        // 摘要:
        //     系统定义的颜色。
        LawnGreen = 89,
        //
        // 摘要:
        //     系统定义的颜色。
        LemonChiffon = 90,
        //
        // 摘要:
        //     系统定义的颜色。
        LightBlue = 91,
        //
        // 摘要:
        //     系统定义的颜色。
        LightCoral = 92,
        //
        // 摘要:
        //     系统定义的颜色。
        LightCyan = 93,
        //
        // 摘要:
        //     系统定义的颜色。
        LightGoldenrodYellow = 94,
        //
        // 摘要:
        //     系统定义的颜色。
        LightGray = 95,
        //
        // 摘要:
        //     系统定义的颜色。
        LightGreen = 96,
        //
        // 摘要:
        //     系统定义的颜色。
        LightPink = 97,
        //
        // 摘要:
        //     系统定义的颜色。
        LightSalmon = 98,
        //
        // 摘要:
        //     系统定义的颜色。
        LightSeaGreen = 99,
        //
        // 摘要:
        //     系统定义的颜色。
        LightSkyBlue = 100,
        //
        // 摘要:
        //     系统定义的颜色。
        LightSlateGray = 101,
        //
        // 摘要:
        //     系统定义的颜色。
        LightSteelBlue = 102,
        //
        // 摘要:
        //     系统定义的颜色。
        LightYellow = 103,
        //
        // 摘要:
        //     系统定义的颜色。
        Lime = 104,
        //
        // 摘要:
        //     系统定义的颜色。
        LimeGreen = 105,
        //
        // 摘要:
        //     系统定义的颜色。
        Linen = 106,
        //
        // 摘要:
        //     系统定义的颜色。
        Magenta = 107,
        //
        // 摘要:
        //     系统定义的颜色。
        Maroon = 108,
        //
        // 摘要:
        //     系统定义的颜色。
        MediumAquamarine = 109,
        //
        // 摘要:
        //     系统定义的颜色。
        MediumBlue = 110,
        //
        // 摘要:
        //     系统定义的颜色。
        MediumOrchid = 111,
        //
        // 摘要:
        //     系统定义的颜色。
        MediumPurple = 112,
        //
        // 摘要:
        //     系统定义的颜色。
        MediumSeaGreen = 113,
        //
        // 摘要:
        //     系统定义的颜色。
        MediumSlateBlue = 114,
        //
        // 摘要:
        //     系统定义的颜色。
        MediumSpringGreen = 115,
        //
        // 摘要:
        //     系统定义的颜色。
        MediumTurquoise = 116,
        //
        // 摘要:
        //     系统定义的颜色。
        MediumVioletRed = 117,
        //
        // 摘要:
        //     系统定义的颜色。
        MidnightBlue = 118,
        //
        // 摘要:
        //     系统定义的颜色。
        MintCream = 119,
        //
        // 摘要:
        //     系统定义的颜色。
        MistyRose = 120,
        //
        // 摘要:
        //     系统定义的颜色。
        Moccasin = 121,
        //
        // 摘要:
        //     系统定义的颜色。
        NavajoWhite = 122,
        //
        // 摘要:
        //     系统定义的颜色。
        Navy = 123,
        //
        // 摘要:
        //     系统定义的颜色。
        OldLace = 124,
        //
        // 摘要:
        //     系统定义的颜色。
        Olive = 125,
        //
        // 摘要:
        //     系统定义的颜色。
        OliveDrab = 126,
        //
        // 摘要:
        //     系统定义的颜色。
        Orange = 127,
        //
        // 摘要:
        //     系统定义的颜色。
        OrangeRed = 128,
        //
        // 摘要:
        //     系统定义的颜色。
        Orchid = 129,
        //
        // 摘要:
        //     系统定义的颜色。
        PaleGoldenrod = 130,
        //
        // 摘要:
        //     系统定义的颜色。
        PaleGreen = 131,
        //
        // 摘要:
        //     系统定义的颜色。
        PaleTurquoise = 132,
        //
        // 摘要:
        //     系统定义的颜色。
        PaleVioletRed = 133,
        //
        // 摘要:
        //     系统定义的颜色。
        PapayaWhip = 134,
        //
        // 摘要:
        //     系统定义的颜色。
        PeachPuff = 135,
        //
        // 摘要:
        //     系统定义的颜色。
        Peru = 136,
        //
        // 摘要:
        //     系统定义的颜色。
        Pink = 137,
        //
        // 摘要:
        //     系统定义的颜色。
        Plum = 138,
        //
        // 摘要:
        //     系统定义的颜色。
        PowderBlue = 139,
        //
        // 摘要:
        //     系统定义的颜色。
        Purple = 140,
        //
        // 摘要:
        //     系统定义的颜色。
        Red = 141,
        //
        // 摘要:
        //     系统定义的颜色。
        RosyBrown = 142,
        //
        // 摘要:
        //     系统定义的颜色。
        RoyalBlue = 143,
        //
        // 摘要:
        //     系统定义的颜色。
        SaddleBrown = 144,
        //
        // 摘要:
        //     系统定义的颜色。
        Salmon = 145,
        //
        // 摘要:
        //     系统定义的颜色。
        SandyBrown = 146,
        //
        // 摘要:
        //     系统定义的颜色。
        SeaGreen = 147,
        //
        // 摘要:
        //     系统定义的颜色。
        SeaShell = 148,
        //
        // 摘要:
        //     系统定义的颜色。
        Sienna = 149,
        //
        // 摘要:
        //     系统定义的颜色。
        Silver = 150,
        //
        // 摘要:
        //     系统定义的颜色。
        SkyBlue = 151,
        //
        // 摘要:
        //     系统定义的颜色。
        SlateBlue = 152,
        //
        // 摘要:
        //     系统定义的颜色。
        SlateGray = 153,
        //
        // 摘要:
        //     系统定义的颜色。
        Snow = 154,
        //
        // 摘要:
        //     系统定义的颜色。
        SpringGreen = 155,
        //
        // 摘要:
        //     系统定义的颜色。
        SteelBlue = 156,
        //
        // 摘要:
        //     系统定义的颜色。
        Tan = 157,
        //
        // 摘要:
        //     系统定义的颜色。
        Teal = 158,
        //
        // 摘要:
        //     系统定义的颜色。
        Thistle = 159,
        //
        // 摘要:
        //     系统定义的颜色。
        Tomato = 160,
        //
        // 摘要:
        //     系统定义的颜色。
        Turquoise = 161,
        //
        // 摘要:
        //     系统定义的颜色。
        Violet = 162,
        //
        // 摘要:
        //     系统定义的颜色。
        Wheat = 163,
        //
        // 摘要:
        //     系统定义的颜色。
        White = 164,
        //
        // 摘要:
        //     系统定义的颜色。
        WhiteSmoke = 165,
        //
        // 摘要:
        //     系统定义的颜色。
        Yellow = 166,
        //
        // 摘要:
        //     系统定义的颜色。
        YellowGreen = 167,
        //
        // 摘要:
        //     三维元素的系统定义表面颜色。
        ButtonFace = 168,
        //
        // 摘要:
        //     系统定义的颜色，是三维元素的高光点颜色。此颜色应用于三维元素面向光源的部分。
        ButtonHighlight = 169,
        //
        // 摘要:
        //     系统定义的颜色，是三维元素的阴影颜色。此颜色应用于三维元素背向光源的部分。
        ButtonShadow = 170,
        //
        // 摘要:
        //     活动窗口标题栏的颜色渐变中最亮色的系统定义颜色。
        GradientActiveCaption = 171,
        //
        // 摘要:
        //     非活动窗口标题栏的颜色渐变中最亮色的系统定义颜色。
        GradientInactiveCaption = 172,
        //
        // 摘要:
        //     菜单栏背景的系统定义颜色。
        MenuBar = 173,
        //
        // 摘要:
        //     当出现的是展开菜单时，用于突出显示菜单项的系统定义颜色。
        MenuHighlight = 174
    }

    public struct Color
    {
        public Byte R
        {
            get;
            set;
        }
        public Byte G
        {
            get;
            set;
        }
        public Byte B
        {
            get;
            set;
        }
        public Byte A
        {
            get;
            set;
        }

        public string Name
        {
            get
            {
                return ToArgb().ToString("X");
            }
        }

        public static Color FromArgb(Int32 r, Int32 g, Int32 b)
        {
            var ret = new Color();
            ret.A = (byte)255;
            ret.R = (byte)r;
            ret.G = (byte)g;
            ret.B = (byte)b;
            return ret;
        }

        public static Color FromArgb(Int32 a, Int32 r, Int32 g, Int32 b)
        {
            var ret = new Color();
            ret.A = (byte)a;
            ret.R = (byte)r;
            ret.G = (byte)g;
            ret.B = (byte)b;
            return ret;
        }

        public static Color FromArgb( int alpha, Color baseColor )
        {
            var ret = new Color();
            ret.A = (byte)alpha;
            ret.R = baseColor.R;
            ret.G = baseColor.G;
            ret.B = baseColor.B;
            return ret;

        }

        public static Color FromArgb(int value)
        {
            var ret = new Color();
            ret.A = (byte)(value >> 24);
            ret.R = (byte)(value >> 16);
            ret.G = (byte)(value >> 8);
            ret.B = (byte)(value);
            return ret;
        }
        public int ToArgb()
        {
            return (((int)(B)) | ((int)(G) << 8) | ((int)(R << 16)) | ((int)(A << 24)));
        }
        [Rtti.Meta]
        public Color4 ToColor4Float()
        {
            var result = new Color4();
            result.Alpha = (float)A / 255.0f;
            result.Red = (float)R / 255.0f;
            result.Green = (float)G / 255.0f;
            result.Blue = (float)B / 255.0f;
            return result;
        }
        /// <summary>
        /// 重载ToString方法在写入文件时使用
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return R + "," + G + "," + B + "," + A;
        }
        public static Color FromString(string val)
        {
            var ret = Color.White;
            var segs = val.Split(',');
            if (segs.Length < 4)
                return ret;
            ret.R = System.Convert.ToByte(segs[0]);
            ret.G = System.Convert.ToByte(segs[1]);
            ret.B = System.Convert.ToByte(segs[2]);
            ret.A = System.Convert.ToByte(segs[3]);
            return ret;
        }

        #region 颜色

        public static Color AliceBlue
        {
            get { return FromArgb(unchecked((int)(0xFFF0F8FF))); }
        }
        public static Color AntiqueWhite
        {
            get { return FromArgb(unchecked((int)(0xFFFAEBD7))); }
        }
        public static Color Aqua
        {
            get { return FromArgb(unchecked((int)(0xFF00FFFF))); }
        }
        public static Color Aquamarine
        {
            get { return FromArgb(unchecked((int)(0xFF7FFFD4))); }
        }
        public static Color Azure
        {
            get { return FromArgb(unchecked((int)(0xFFF0FFFF))); }
        }
        public static Color Bisque
        {
            get { return FromArgb(unchecked((int)(0xFFFFE4C4))); }
        }
        public static Color Black
        {
            get { return FromArgb(unchecked((int)(0xFF000000))); }
        }
        public static Color BlanchedAlmond
        {
            get { return FromArgb(unchecked((int)(0xFFFFEBCD))); }
        }
        public static Color Blue
        {
            get { return FromArgb(unchecked((int)(0xFF0000FF))); }
        }
        public static Color BlueViolet
        {
            get { return FromArgb(unchecked((int)(0xFF8A2BE2))); }
        }
        public static Color Brown
        {
            get { return FromArgb(unchecked((int)(0xFFA52A2A))); }
        }
        public static Color BurlyWood
        {
            get { return FromArgb(unchecked((int)(0xFFDEB887))); }
        }
        public static Color CadetBlue
        {
            get { return FromArgb(unchecked((int)(0xFF5F9EA0))); }
        }
        public static Color Chartreuse
        {
            get { return FromArgb(unchecked((int)(0xFF7FFF00))); }
        }
        public static Color Chocolate
        {
            get { return FromArgb(unchecked((int)(0xFFD2691E))); }
        }
        public static Color Coral
        {
            get { return FromArgb(unchecked((int)(0xFFFF7F50))); }
        }
        public static Color CornflowerBlue
        {
            get { return FromArgb(unchecked((int)(0xFF6495ED))); }
        }
        public static Color Cornsilk
        {
            get { return FromArgb(unchecked((int)(0xFFFFF8DC))); }
        }
        public static Color Crimson
        {
            get { return FromArgb(unchecked((int)(0xFFDC143C))); }
        }
        public static Color Cyan
        {
            get { return FromArgb(unchecked((int)(0xFF00FFFF))); }
        }
        public static Color DarkBlue
        {
            get { return FromArgb(unchecked((int)(0xFF00008B))); }
        }
        public static Color DarkCyan
        {
            get { return FromArgb(unchecked((int)(0xFF008B8B))); }
        }
        public static Color DarkGoldenrod
        {
            get { return FromArgb(unchecked((int)(0xFFB8860B))); }
        }
        public static Color DarkGray
        {
            get { return FromArgb(unchecked((int)(0xFFA9A9A9))); }
        }
        public static Color DarkGreen
        {
            get { return FromArgb(unchecked((int)(0xFF006400))); }
        }
        public static Color DarkKhaki
        {
            get { return FromArgb(unchecked((int)(0xFFBDB76B))); }
        }
        public static Color DarkMagenta
        {
            get { return FromArgb(unchecked((int)(0xFF8B008B))); }
        }
        public static Color DarkOliveGreen
        {
            get { return FromArgb(unchecked((int)(0xFF556B2F))); }
        }
        public static Color DarkOrange
        {
            get { return FromArgb(unchecked((int)(0xFFFF8C00))); }
        }
        public static Color DarkOrchid
        {
            get { return FromArgb(unchecked((int)(0xFF9932CC))); }
        }
        public static Color DarkRed
        {
            get { return FromArgb(unchecked((int)(0xFF8B0000))); }
        }
        public static Color DarkSalmon
        {
            get { return FromArgb(unchecked((int)(0xFFE9967A))); }
        }
        public static Color DarkSeaGreen
        {
            get { return FromArgb(unchecked((int)(0xFF8FBC8F))); }
        }
        public static Color DarkSlateBlue
        {
            get { return FromArgb(unchecked((int)(0xFF483D8B))); }
        }
        public static Color DarkSlateGray
        {
            get { return FromArgb(unchecked((int)(0xFF2F4F4F))); }
        }
        public static Color DarkTurquoise
        {
            get { return FromArgb(unchecked((int)(0xFF00CED1))); }
        }
        public static Color DarkViolet
        {
            get { return FromArgb(unchecked((int)(0xFF9400D3))); }
        }
        public static Color DeepPink
        {
            get { return FromArgb(unchecked((int)(0xFFFF1493))); }
        }
        public static Color DeepSkyBlue
        {
            get { return FromArgb(unchecked((int)(0xFF00BFFF))); }
        }
        public static Color DimGray
        {
            get { return FromArgb(unchecked((int)(0xFF696969))); }
        }
        public static Color DodgerBlue
        {
            get { return FromArgb(unchecked((int)(0xFF1E90FF))); }
        }
        public static Color Firebrick
        {
            get { return FromArgb(unchecked((int)(0xFFB22222))); }
        }
        public static Color FloralWhite
        {
            get { return FromArgb(unchecked((int)(0xFFFFFAF0))); }
        }
        public static Color ForestGreen
        {
            get { return FromArgb(unchecked((int)(0xFF228B22))); }
        }
        public static Color Fuchsia
        {
            get { return FromArgb(unchecked((int)(0xFFFF00FF))); }
        }
        public static Color Gainsboro
        {
            get { return FromArgb(unchecked((int)(0xFFDCDCDC))); }
        }
        public static Color GhostWhite
        {
            get { return FromArgb(unchecked((int)(0xFFF8F8FF))); }
        }
        public static Color Gold
        {
            get { return FromArgb(unchecked((int)(0xFFFFD700))); }
        }
        public static Color Goldenrod
        {
            get { return FromArgb(unchecked((int)(0xFFDAA520))); }
        }
        public static Color Gray
        {
            get { return FromArgb(unchecked((int)(0xFF808080))); }
        }
        public static Color Green
        {
            get { return FromArgb(unchecked((int)(0xFF008000))); }
        }
        public static Color GreenYellow
        {
            get { return FromArgb(unchecked((int)(0xFFADFF2F))); }
        }
        public static Color Honeydew
        {
            get { return FromArgb(unchecked((int)(0xFFF0FFF0))); }
        }
        public static Color HotPink
        {
            get { return FromArgb(unchecked((int)(0xFFFF69B4))); }
        }
        public static Color IndianRed
        {
            get { return FromArgb(unchecked((int)(0xFFCD5C5C))); }
        }
        public static Color Indigo
        {
            get { return FromArgb(unchecked((int)(0xFF4B0082))); }
        }
        public static Color Ivory
        {
            get { return FromArgb(unchecked((int)(0xFFFFFFF0))); }
        }
        public static Color Khaki
        {
            get { return FromArgb(unchecked((int)(0xFFF0E68C))); }
        }
        public static Color Lavender
        {
            get { return FromArgb(unchecked((int)(0xFFE6E6FA))); }
        }
        public static Color LavenderBlush
        {
            get { return FromArgb(unchecked((int)(0xFFFFF0F5))); }
        }
        public static Color LawnGreen
        {
            get { return FromArgb(unchecked((int)(0xFF7CFC00))); }
        }
        public static Color LemonChiffon
        {
            get { return FromArgb(unchecked((int)(0xFFFFFACD))); }
        }
        public static Color LightBlue
        {
            get { return FromArgb(unchecked((int)(0xFFADD8E6))); }
        }
        public static Color LightCoral
        {
            get { return FromArgb(unchecked((int)(0xFFF08080))); }
        }
        public static Color LightCyan
        {
            get { return FromArgb(unchecked((int)(0xFFE0FFFF))); }
        }
        public static Color LightGoldenrodYellow
        {
            get { return FromArgb(unchecked((int)(0xFFFAFAD2))); }
        }
        public static Color LightGray
        {
            get { return FromArgb(unchecked((int)(0xFFD3D3D3))); }
        }
        public static Color LightGreen
        {
            get { return FromArgb(unchecked((int)(0xFF90EE90))); }
        }
        public static Color LightPink
        {
            get { return FromArgb(unchecked((int)(0xFFFFB6C1))); }
        }
        public static Color LightSalmon
        {
            get { return FromArgb(unchecked((int)(0xFFFFA07A))); }
        }
        public static Color LightSeaGreen
        {
            get { return FromArgb(unchecked((int)(0xFF20B2AA))); }
        }
        public static Color LightSkyBlue
        {
            get { return FromArgb(unchecked((int)(0xFF87CEFA))); }
        }
        public static Color LightSlateGray
        {
            get { return FromArgb(unchecked((int)(0xFF778899))); }
        }
        public static Color LightSteelBlue
        {
            get { return FromArgb(unchecked((int)(0xFFB0C4DE))); }
        }
        public static Color LightYellow
        {
            get { return FromArgb(unchecked((int)(0xFFFFFFE0))); }
        }
        public static Color Lime
        {
            get { return FromArgb(unchecked((int)(0xFF00FF00))); }
        }
        public static Color LimeGreen
        {
            get { return FromArgb(unchecked((int)(0xFF32CD32))); }
        }
        public static Color Linen
        {
            get { return FromArgb(unchecked((int)(0xFFFAF0E6))); }
        }
        public static Color Magenta
        {
            get { return FromArgb(unchecked((int)(0xFFFF00FF))); }
        }
        public static Color Maroon
        {
            get { return FromArgb(unchecked((int)(0xFF800000))); }
        }
        public static Color MediumAquamarine
        {
            get { return FromArgb(unchecked((int)(0xFF66CDAA))); }
        }
        public static Color MediumBlue
        {
            get { return FromArgb(unchecked((int)(0xFF0000CD))); }
        }
        public static Color MediumOrchid
        {
            get { return FromArgb(unchecked((int)(0xFFBA55D3))); }
        }
        public static Color MediumPurple
        {
            get { return FromArgb(unchecked((int)(0xFF9370DB))); }
        }
        public static Color MediumSeaGreen
        {
            get { return FromArgb(unchecked((int)(0xFF3CB371))); }
        }
        public static Color MediumSlateBlue
        {
            get { return FromArgb(unchecked((int)(0xFF7B68EE))); }
        }
        public static Color MediumSpringGreen
        {
            get { return FromArgb(unchecked((int)(0xFF00FA9A))); }
        }
        public static Color MediumTurquoise
        {
            get { return FromArgb(unchecked((int)(0xFF48D1CC))); }
        }
        public static Color MediumVioletRed
        {
            get { return FromArgb(unchecked((int)(0xFFC71585))); }
        }
        public static Color MidnightBlue
        {
            get { return FromArgb(unchecked((int)(0xFF191970))); }
        }
        public static Color MintCream
        {
            get { return FromArgb(unchecked((int)(0xFFF5FFFA))); }
        }
        public static Color MistyRose
        {
            get { return FromArgb(unchecked((int)(0xFFFFE4E1))); }
        }
        public static Color Moccasin
        {
            get { return FromArgb(unchecked((int)(0xFFFFE4B5))); }
        }
        public static Color NavajoWhite
        {
            get { return FromArgb(unchecked((int)(0xFFFFDEAD))); }
        }
        public static Color Navy
        {
            get { return FromArgb(unchecked((int)(0xFF000080))); }
        }
        public static Color OldLace
        {
            get { return FromArgb(unchecked((int)(0xFFFDF5E6))); }
        }
        public static Color Olive
        {
            get { return FromArgb(unchecked((int)(0xFF808000))); }
        }
        public static Color OliveDrab
        {
            get { return FromArgb(unchecked((int)(0xFF6B8E23))); }
        }
        public static Color Orange
        {
            get { return FromArgb(unchecked((int)(0xFFFFA500))); }
        }
        public static Color OrangeRed
        {
            get { return FromArgb(unchecked((int)(0xFFFF4500))); }
        }
        public static Color Orchid
        {
            get { return FromArgb(unchecked((int)(0xFFDA70D6))); }
        }
        public static Color PaleGoldenrod
        {
            get { return FromArgb(unchecked((int)(0xFFEEE8AA))); }
        }
        public static Color PaleGreen
        {
            get { return FromArgb(unchecked((int)(0xFF98FB98))); }
        }
        public static Color PaleTurquoise
        {
            get { return FromArgb(unchecked((int)(0xFFAFEEEE))); }
        }
        public static Color PaleVioletRed
        {
            get { return FromArgb(unchecked((int)(0xFFDB7093))); }
        }
        public static Color PapayaWhip
        {
            get { return FromArgb(unchecked((int)(0xFFFFEFD5))); }
        }
        public static Color PeachPuff
        {
            get { return FromArgb(unchecked((int)(0xFFFFDAB9))); }
        }
        public static Color Peru
        {
            get { return FromArgb(unchecked((int)(0xFFCD853F))); }
        }
        public static Color Pink
        {
            get { return FromArgb(unchecked((int)(0xFFFFC0CB))); }
        }
        public static Color Plum
        {
            get { return FromArgb(unchecked((int)(0xFFDDA0DD))); }
        }
        public static Color PowderBlue
        {
            get { return FromArgb(unchecked((int)(0xFFB0E0E6))); }
        }
        public static Color Purple
        {
            get { return FromArgb(unchecked((int)(0xFF800080))); }
        }
        public static Color Red
        {
            get { return FromArgb(unchecked((int)(0xFFFF0000))); }
        }
        public static Color RosyBrown
        {
            get { return FromArgb(unchecked((int)(0xFFBC8F8F))); }
        }
        public static Color RoyalBlue
        {
            get { return FromArgb(unchecked((int)(0xFF4169E1))); }
        }
        public static Color SaddleBrown
        {
            get { return FromArgb(unchecked((int)(0xFF8B4513))); }
        }
        public static Color Salmon
        {
            get { return FromArgb(unchecked((int)(0xFFFA8072))); }
        }
        public static Color SandyBrown
        {
            get { return FromArgb(unchecked((int)(0xFFF4A460))); }
        }
        public static Color SeaGreen
        {
            get { return FromArgb(unchecked((int)(0xFF2E8B57))); }
        }
        public static Color SeaShell
        {
            get { return FromArgb(unchecked((int)(0xFFFFF5EE))); }
        }
        public static Color Sienna
        {
            get { return FromArgb(unchecked((int)(0xFFA0522D))); }
        }
        public static Color Silver
        {
            get { return FromArgb(unchecked((int)(0xFFC0C0C0))); }
        }
        public static Color SkyBlue
        {
            get { return FromArgb(unchecked((int)(0xFF87CEEB))); }
        }
        public static Color SlateBlue
        {
            get { return FromArgb(unchecked((int)(0xFF6A5ACD))); }
        }
        public static Color SlateGray
        {
            get { return FromArgb(unchecked((int)(0xFF708090))); }
        }
        public static Color Snow
        {
            get { return FromArgb(unchecked((int)(0xFFFFFAFA))); }
        }
        public static Color SpringGreen
        {
            get { return FromArgb(unchecked((int)(0xFF00FF7F))); }
        }
        public static Color SteelBlue
        {
            get { return FromArgb(unchecked((int)(0xFF4682B4))); }
        }
        public static Color Tan
        {
            get { return FromArgb(unchecked((int)(0xFFD2B48C))); }
        }
        public static Color Teal
        {
            get { return FromArgb(unchecked((int)(0xFF008080))); }
        }
        public static Color Thistle
        {
            get { return FromArgb(unchecked((int)(0xFFD8BFD8))); }
        }
        public static Color Tomato
        {
            get { return FromArgb(unchecked((int)(0xFFFF6347))); }
        }
        public static Color Turquoise
        {
            get { return FromArgb(unchecked((int)(0xFF40E0D0))); }
        }
        public static Color Violet
        {
            get { return FromArgb(unchecked((int)(0xFFEE82EE))); }
        }
        public static Color Wheat
        {
            get { return FromArgb(unchecked((int)(0xFFF5DEB3))); }
        }
        public static Color White
        {
            get { return FromArgb(unchecked((int)(0xFFFFFFFF))); }
        }
        public static Color WhiteSmoke
        {
            get { return FromArgb(unchecked((int)(0xFFF5F5F5))); }
        }
        public static Color Yellow
        {
            get { return FromArgb(unchecked((int)(0xFFFFFF00))); }
        }
        public static Color YellowGreen
        {
            get { return FromArgb(unchecked((int)(0xFF9ACD32))); }
        }

        #endregion
    }
}
