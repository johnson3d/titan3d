using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui.UIProxy
{
    [Rtti.Meta]
    public class StyleConfig
    {
        public static UInt32 TransparentColor = 0x000000FF;

        // Common
        [Rtti.Meta]
        public static Vector2 ItemSpacing = new Vector2(8, 4);
        [Rtti.Meta]
        public static UInt32 PanelBackground = 0xff242424;// new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
        [Rtti.Meta]
        public static UInt32 SeparatorColor = 0xFF151515;
        [Rtti.Meta]
        public static Vector2 PGSearchBoxFramePadding = new Vector2(30, 6);
        [Rtti.Meta]
        public static UInt32 PGSearchBoxFocusBorderColor = 0xFFFFBB26;
        [Rtti.Meta]
        public static UInt32 PGSearchBoxInfoTextColor = 0xFF636363;

        // Menu
        [Rtti.Meta]
        public static UInt32 NamedMenuSeparatorColor = 0xff6a6a6a;// new Vector4(0.42f, 0.42f, 0.42f, 1.00f);
        [Rtti.Meta]
        public static Vector2 WindowPadding = new Vector2(8, 5);
        [Rtti.Meta]
        public static Vector2 MenuPadding = new Vector2(40, 8);
        [Rtti.Meta]
        public static Vector2 TopMenuWindowPadding = new Vector2(1, 8);
        [Rtti.Meta]
        public static Vector2 TopMenuFramePadding = new Vector2(4, 7);
        [Rtti.Meta]
        public static Vector2 TopMenuItemSpacing = new Vector2(8, 12);
        [Rtti.Meta]
        public static Vector2 MenuItemFramePadding = new Vector2(4, 4);
        [Rtti.Meta]
        public static Vector2 MenuItemSpacing = new Vector2(8, 8);
        [Rtti.Meta]
        public static float MenuItemIndent = 15;

        // Tab
        [Rtti.Meta]
        public static Vector2 MainTabFramePadding = new Vector2(10, 10);

        // Toolbar
        [Rtti.Meta]
        public static float ToolbarButtonIconTextSpacing = 4;
        [Rtti.Meta]
        public static Vector4 ToolbarButtonTextColor = new Vector4(0.75f, 0.75f, 0.75f, 1.00f);
        [Rtti.Meta]
        public static Vector4 ToolbarButtonTextColor_Hover = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
        [Rtti.Meta]
        public static Vector4 ToolbarButtonTextColor_Press = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
        [Rtti.Meta]
        public static float ToolbarHeight = 40;
        [Rtti.Meta]
        public static Vector4 ToolbarBG = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
        [Rtti.Meta]
        public static float ToolbarSeparatorThickness = 2;

        // PropertyGrid
        [Rtti.Meta]
        public static Vector2 PGNormalItemSpacing = new Vector2(0, 4);
        [Rtti.Meta]
        public static Vector2 PGNormalFramePadding = new Vector2(0, 6);
        [Rtti.Meta]
        public static Vector2 PGCheckboxFramePadding = new Vector2(2, 2);
        [Rtti.Meta]
        public static Vector4 PGItemHoveredColor = new Vector4(0.18f, 0.18f, 0.18f, 1.00f);
        [Rtti.Meta]
        public static Vector2 PGInputFramePadding = new Vector2(8, 6);

    }
}
