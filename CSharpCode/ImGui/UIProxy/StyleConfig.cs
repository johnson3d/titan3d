using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.EGui.UIProxy
{
    [Rtti.Meta]
    public class StyleConfig
    {
        public static StyleConfig Instance { get; } = new StyleConfig();

        public UInt32 TransparentColor = 0x000000FF;

        // Common
        [Rtti.Meta, Category("Common")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 ItemSpacing = new Vector2(8, 4);

        [Rtti.Meta, Category("Common")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PanelBackground = 0xff242424;// new Vector4(0.14f, 0.14f, 0.14f, 1.00f);

        [Rtti.Meta, Category("Common")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 SeparatorColor = 0xFF151515;

        [Rtti.Meta, Category("Common")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 WindowsPadding = new Vector2(0, 0);

        [Rtti.Meta, Category("Common")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PopupColor = 0xFF1A1A1A;

        [Rtti.Meta, Category("Common")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 PopupWindowsPadding = new Vector2(8, 8);

        [Rtti.Meta, Category("Common")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 PopupItemSpacing = new Vector2(8, 8);

        [Rtti.Meta, Category("Common")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public float PopupBordersize = 0.0f;

        [Rtti.Meta, Category("Common")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PopupHoverColor = 0xffFFBA26;// 38, 186 new Vector4(0.15f, 0.73f, 1.00f, 1.00f)

        [Rtti.Meta, Category("Common")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 TextColor = 0xFFBFBFBF;

        [Rtti.Meta, Category("Common")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 TextHoveredColor = 0xFF000000;

        [Rtti.Meta, Category("Common")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 TextSelectedColor = 0xFF000000;

        [Rtti.Meta, Category("Common")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ItemHightlightHoveredColor = 0xFFFFD26F;

        // Menu
        [Rtti.Meta, Category("Menu")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 NamedMenuSeparatorColor = 0xff6a6a6a;// new Vector4(0.42f, 0.42f, 0.42f, 1.00f);

        [Rtti.Meta, Category("Menu")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 MenuHeaderColor = 0xffFFBA26;// 38, 186 new Vector4(0.15f, 0.73f, 1.00f, 1.00f)

        [Rtti.Meta, Category("Menu")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 MenuBG = 0xff383838;// (0.22f, 0.22f, 0.22f, 1.00f)

        [Rtti.Meta, Category("Menu")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 WindowPadding = new Vector2(0, 0);

        [Rtti.Meta, Category("Menu")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 MenuPadding = new Vector2(40, 8);

        [Rtti.Meta, Category("Menu")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 TopMenuWindowPadding = new Vector2(1, 8);

        [Rtti.Meta, Category("Menu")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 TopMenuFramePadding = new Vector2(4, 7);

        [Rtti.Meta, Category("Menu")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 TopMenuItemSpacing = new Vector2(8, 12);

        [Rtti.Meta, Category("Menu")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 MenuItemFramePadding = new Vector2(4, 4);

        [Rtti.Meta, Category("Menu")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 MenuItemSpacing = new Vector2(8, 8);

        [Rtti.Meta, Category("Menu")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public float MenuItemIndent = 15;

        // Tab
        [Rtti.Meta, Category("Tab")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 MainTabFramePadding = new Vector2(10, 10);

        // Toolbar
        [Rtti.Meta, Category("Toolbar")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public float ToolbarButtonIconTextSpacing = 4;

        [Rtti.Meta, Category("Toolbar")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ToolbarButtonTextColor = 0xFFBFBFBF;// new Vector4(0.75f, 0.75f, 0.75f, 1.00f);

        [Rtti.Meta, Category("Toolbar")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ToolbarButtonTextColor_Hover = 0xFFFFFFFF;// new Vector4(1.00f, 1.00f, 1.00f, 1.00f);

        [Rtti.Meta, Category("Toolbar")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ToolbarButtonTextColor_Press = 0xFFFFFFFF;// new Vector4(1.00f, 1.00f, 1.00f, 1.00f);

        [Rtti.Meta, Category("Toolbar")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public float ToolbarHeight = 40;

        [Rtti.Meta, Category("Toolbar")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ToolbarBG = 0xFF242424;// new Vector4(0.14f, 0.14f, 0.14f, 1.00f);

        [Rtti.Meta, Category("Toolbar")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public float ToolbarSeparatorThickness = 2;

        // PropertyGrid
        [Rtti.Meta, Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 PGNormalItemSpacing = new Vector2(0, 0);

        [Rtti.Meta, Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 PGNormalFramePadding = new Vector2(0, 10);


        [Rtti.Meta, Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 PGCheckboxFramePadding = new Vector2(2, 2);

        [Rtti.Meta, Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGItemHoveredColor = 0xFF2F2F2F;

        [Rtti.Meta, Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 PGInputFramePadding = new Vector2(8, 6);

        [Rtti.Meta, Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGCategoryBG = 0xFF2F2F2F;

        [Rtti.Meta, Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 PGCategoryPadding = new Vector2(8, 9);

        [Rtti.Meta, Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public float PGNormalFrameBorderSize = 1.0f;

        [Rtti.Meta, Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public float PGNormalFrameRounding = 3.0f;

        [Rtti.Meta, Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 PGSearchBoxFramePadding = new Vector2(30, 6);

        [Rtti.Meta, Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGSearchBoxFocusBorderColor = 0xFFFFBB26;

        [Rtti.Meta, Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGSearchBoxInfoTextColor = 0xFF636363;

        [Rtti.Meta, Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGHeadColor = 0xFF212121;

        [Rtti.Meta, Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGCellBorderInnerColor = 0xFF151515;

        [Rtti.Meta, Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGCellBorderOutterColor = 0xFF151515;

        [Rtti.Meta, Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 PGCellPadding = new Vector2(8, 4);

        [Rtti.Meta, Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 PGColorBoxSize = new Vector2(30, 15);

        [Rtti.Meta, Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public float PGColorBoxRound = 3.0f;

        [Rtti.Meta, Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public float PGItemBorderThickness = 3.0f;

        [Rtti.Meta, Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGItemBorderNormalColor = 0xFF1a1a1a;//0xFF454545;

        [Rtti.Meta, Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGItemBorderHoveredColor = 0xFF575757;

        [Rtti.Meta, Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGCreateButtonBGColor = 0xFF52a852;
        [Rtti.Meta, Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGCreateButtonBGHoverColor = 0xFF5cb95c;
        [Rtti.Meta, Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGCreateButtonBGActiveColor = 0xFF479347;

        [Rtti.Meta, Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGDeleteButtonBGColor = 0xFF5252a8;
        [Rtti.Meta, Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGDeleteButtonBGHoverColor = 0xFF5c5cb9;
        [Rtti.Meta, Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGDeleteButtonBGActiveColor = 0xFF474793;
    }
}
