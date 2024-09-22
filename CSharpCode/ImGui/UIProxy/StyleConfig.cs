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
        [Category("Common")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 ItemSpacing = new Vector2(4, 4);

        [Category("Common")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 WindowBackground = 0xff141414;

        [Category("Common")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PanelBackground = 0xff242424;// new Vector4(0.14f, 0.14f, 0.14f, 1.00f);

        [Category("Common")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 SecondPanelBackground = 0xff141414;

        [Category("Common")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 PanelFramePadding = new Vector2(4, 4);

        [Category("Common")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 PGNormalItemSpacing = new Vector2(0, 0);

        [Category("Common")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 GridColor = 0xff505050;// new Vector4(0.14f, 0.14f, 0.14f, 1.00f);

        [Category("Common")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 SeparatorColor = 0xFF151515;

        [Category("Common")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 WindowsPadding = new Vector2(4, 4);

        [Category("Common")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PopupColor = 0xFF1A1A1A;

        [Category("Common")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 PopupWindowsPadding = new Vector2(8, 8);

        [Category("Common")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 PopupItemSpacing = new Vector2(8, 8);

        [Category("Common")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public float PopupBordersize = 0.0f;

        [Category("Common")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PopupHoverColor = 0xffFFBA26;// 38, 186 new Vector4(0.15f, 0.73f, 1.00f, 1.00f)

        [Category("Common")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 TextColor = 0xFFBFBFBF;

        [Category("Common")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 TextHoveredColor = 0xFF000000;

        [Category("Common")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 TextSelectedColor = 0xFF000000;

        [Category("Common")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 TextDisableColor = 0xFF929292;

        [Category("Common")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ItemHightlightHoveredColor = 0xFFFFD26F;

        [Category("Common")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ErrorStringColor = 0xFF0000FF;

        [Category("Common")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PassStringColor = 0xFF00FF00;

        [Category("Common")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 LinkStringColor = 0xffeeb081;

        // Menu
        [Category("Menu")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 NamedMenuSeparatorColor = 0xff6a6a6a;// new Vector4(0.42f, 0.42f, 0.42f, 1.00f);

        [Category("Menu")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public float NamedMenuSeparatorThickness = 1.0f;// new Vector4(0.42f, 0.42f, 0.42f, 1.00f);

        [Category("Menu")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 MenuHeaderColor = 0xFFE07000;// 38, 186 new Vector4(0.15f, 0.73f, 1.00f, 1.00f)

        [Category("Menu")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 MenuBG = 0xff383838;// (0.22f, 0.22f, 0.22f, 1.00f)

        [Category("Menu")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 WindowPadding = new Vector2(0, 0);

        [Category("Menu")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 MenuPadding = new Vector2(40, 8);

        [Category("Menu")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 TopMenuWindowPadding = new Vector2(1, 8);

        [Category("Menu")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 TopMenuFramePadding = new Vector2(4, 7);

        [Category("Menu")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 TopMenuItemSpacing = new Vector2(8, 12);

        [Category("Menu")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 MenuItemFramePadding = new Vector2(4, 4);

        [Category("Menu")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 MenuItemSpacing = new Vector2(8, 8);

        [Category("Menu")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public float MenuItemIndent = 15;

        // Tab
        [Category("Tab")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 MainTabFramePadding = new Vector2(10, 10);

        // Toolbar
        [Category("Toolbar")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public float ToolbarButtonIconTextSpacing = 4;

        [Category("Toolbar")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ToolbarButtonTextColor = 0xFFBFBFBF;// new Vector4(0.75f, 0.75f, 0.75f, 1.00f);

        [Category("Toolbar")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ToolbarButtonTextColor_Hover = 0xFFFFFFFF;// new Vector4(1.00f, 1.00f, 1.00f, 1.00f);

        [Category("Toolbar")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ToolbarButtonTextColor_Press = 0xFFFFFFFF;// new Vector4(1.00f, 1.00f, 1.00f, 1.00f);

        [Category("Toolbar")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ToolbarButtonTextColor_Disable = 0xFF6a6a6a;// new Vector4(1.00f, 1.00f, 1.00f, 1.00f);

        [Category("Toolbar")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public float ToolbarHeight = 40;

        [Category("Toolbar")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ToolbarBG = 0xFF242424;// new Vector4(0.14f, 0.14f, 0.14f, 1.00f);

        [Category("Toolbar")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public float ToolbarSeparatorThickness = 2;

        // Button
        [Category("Button")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ToolButtonTextColor = 0xFFBFBFBF;// new Vector4(0.75f, 0.75f, 0.75f, 1.00f);

        [Category("Button")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ToolButtonTextColor_Hover = 0xFFFFFFFF;// new Vector4(1.00f, 1.00f, 1.00f, 1.00f);

        [Category("Button")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ToolButtonTextColor_Press = 0xFFE8A200;// new Vector4(1.00f, 1.00f, 1.00f, 1.00f);

        // PropertyGrid
        [Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 PGWindowPadding = new Vector2(2, 2);

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 PGNormalFramePadding = new Vector2(0, 10);


        [Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 PGCheckboxFramePadding = new Vector2(2, 2);

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGItemHoveredColor = 0xFF2F2F2F;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 PGInputFramePadding = new Vector2(8, 6);

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGCategoryBG = 0xFF2F2F2F;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 PGCategoryPadding = new Vector2(8, 9);

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public float PGNormalFrameBorderSize = 1.0f;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public float PGNormalFrameRounding = 3.0f;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 PGSearchBoxFramePadding = new Vector2(30, 6);

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGSearchBoxFocusBorderColor = 0xFFFFBB26;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGSearchBoxInfoTextColor = 0xFF636363;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGHeadColor = 0xFF212121;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGCellBorderInnerColor = 0xFF151515;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGCellBorderOutterColor = 0xFF151515;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 PGCellPadding = new Vector2(8, 4);

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public Vector2 PGColorBoxSize = new Vector2(30, 15);

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public float PGColorBoxRound = 3.0f;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        public float PGItemBorderThickness = 3.0f;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGItemBorderNormalColor = 0xFF1a1a1a;//0xFF454545;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGItemBorderHoveredColor = 0xFF575757;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGCreateButtonBGColor = 0xFF52a852;
        [Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGCreateButtonBGHoverColor = 0xFF5cb95c;
        [Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGCreateButtonBGActiveColor = 0xFF479347;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGDeleteButtonBGColor = 0xFF5252a8;
        [Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGDeleteButtonBGHoverColor = 0xFF5c5cb9;
        [Category("PropertyGrid")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGDeleteButtonBGActiveColor = 0xFF474793;

        [Category("TreeView")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 TVHeader = 0xFFE07000;
        [Category("TreeView")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 TVHeaderHovered = 0xFF9E4f00;
        [Category("TreeView")]
        [Controls.PropertyGrid.PGShowInPropertyGrid]
        [Controls.PropertyGrid.UByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 TVHeaderActive = 0xFFE07000;

        public unsafe void ResetStyle()
        {
            var style = ImGuiAPI.GetStyle();
            style->Colors[(int)ImGuiCol_.ImGuiCol_Text] = new Vector4(0.86f, 0.86f, 0.86f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_TextDisabled] = new Vector4(0.57f, 0.57f, 0.57f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_WindowBg] = new Vector4(0.08f, 0.08f, 0.08f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_ChildBg] = new Vector4(0.16f, 0.16f, 0.16f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_PopupBg] = new Vector4(0.24f, 0.24f, 0.24f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_Border] = new Vector4(0.12f, 0.12f, 0.12f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_BorderShadow] = new Vector4(0.21f, 0.21f, 0.21f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_FrameBg] = new Vector4(0.0f, 0.0f, 0.0f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_FrameBgHovered] = new Vector4(0.32f, 0.32f, 0.32f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_FrameBgActive] = new Vector4(0.45f, 0.45f, 0.45f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_TitleBg] = new Vector4(0.08f, 0.08f, 0.08f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_TitleBgActive] = new Vector4(0.08f, 0.08f, 0.08f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_TitleBgCollapsed] = new Vector4(0.08f, 0.08f, 0.08f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_MenuBarBg] = new Vector4(0.08f, 0.08f, 0.08f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_ScrollbarBg] = new Vector4(0.1f, 0.1f, 0.1f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_ScrollbarGrab] = new Vector4(0.34f, 0.34f, 0.34f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_ScrollbarGrabHovered] = new Vector4(0.50f, 0.50f, 0.50f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_ScrollbarGrabActive] = new Vector4(0.50f, 0.50f, 0.50f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_CheckMark] = new Vector4(0.75f, 0.75f, 0.75f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_SliderGrab] = new Vector4(0.4f, 0.4f, 0.4f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_SliderGrabActive] = new Vector4(0.2f, 0.2f, 0.2f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_Button] = new Vector4(0.22f, 0.22f, 0.22f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_ButtonHovered] = new Vector4(0.31f, 0.31f, 0.31f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_ButtonActive] = new Vector4(0.45f, 0.45f, 0.45f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_Header] = new Vector4(0.18f, 0.18f, 0.18f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_HeaderHovered] = new Vector4(0.24f, 0.24f, 0.24f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_HeaderActive] = new Vector4(0.18f, 0.18f, 0.18f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_Separator] = new Vector4(0.08f, 0.08f, 0.08f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_SeparatorHovered] = new Vector4(0.19f, 0.19f, 0.19f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_SeparatorActive] = new Vector4(0.4f, 0.4f, 0.4f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_ResizeGrip] = new Vector4(0.2f, 0.2f, 0.2f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_ResizeGripHovered] = new Vector4(0.00f, 0.44f, 0.88f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_ResizeGripActive] = new Vector4(0.57f, 0.57f, 0.57f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_Tab] = new Vector4(0.57f, 0.57f, 0.57f, 0.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_TabHovered] = new Vector4(0.13f, 0.13f, 0.13f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_TabActive] = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_TabUnfocused] = new Vector4(0.19f, 0.19f, 0.19f, 0.0f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_TabUnfocusedActive] = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_DockingPreview] = new Vector4(0.21f, 0.21f, 0.21f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_DockingEmptyBg] = new Vector4(0.24f, 0.24f, 0.24f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_PlotLines] = new Vector4(0.18f, 0.18f, 0.18f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_PlotLinesHovered] = new Vector4(0.24f, 0.24f, 0.24f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_PlotHistogram] = new Vector4(0.0f, 0.0f, 0.0f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_PlotHistogramHovered] = new Vector4(0.0f, 0.0f, 0.0f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_TableHeaderBg] = new Vector4(0.18f, 0.18f, 0.18f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_TableBorderStrong] = new Vector4(0.17f, 0.17f, 0.17f, 1.00f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_TableBorderLight] = new Vector4(0.29f, 0.29f, 0.29f, 1.0f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_TableRowBg] = new Vector4(0.10f, 0.10f, 0.10f, 1.0f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_TableRowBgAlt] = new Vector4(0.13f, 0.13f, 0.13f, 1.0f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_TextSelectedBg] = new Vector4(0.00f, 0.44f, 0.88f, 1.0f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_DragDropTarget] = new Vector4(0.26f, 0.59f, 0.98f, 1.0f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_NavHighlight] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_NavWindowingHighlight] = new Vector4(0.8f, 0.8f, 0.8f, 1.0f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_NavWindowingDimBg] = new Vector4(0.8f, 0.8f, 0.8f, 1.0f);
            style->Colors[(int)ImGuiCol_.ImGuiCol_ModalWindowDimBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.66f);
            //style->WindowPadding = new Vector2(1, 0);

            style->Alpha = 1.0f;
            style->WindowPadding = WindowsPadding;
            style->WindowRounding = 3.0f;
            style->WindowBorderSize = 1.0f;
            style->WindowMinSize = new Vector2(32, 32);
            style->WindowTitleAlign = new Vector2(0, 0.5f);
            style->WindowMenuButtonPosition = ImGuiDir.ImGuiDir_Left;
            style->ChildRounding = 0.0f;
            style->ChildBorderSize = 1.0f;
            style->PopupRounding = 0.0f;
            style->PopupBorderSize = 0.0f;
            style->FramePadding = new Vector2(8.0f, 8.0f);
            style->FrameRounding = 3.0f;
            style->FrameBorderSize = 0.0f;
            style->ItemSpacing = ItemSpacing;
            style->ItemInnerSpacing = new Vector2(4.0f, 4.0f);
            style->CellPadding = new Vector2(4.0f, 4.0f);
            style->TouchExtraPadding = new Vector2(0, 0);
            style->IndentSpacing = 21.0f;
            style->ColumnsMinSpacing = 6.0f;
            style->ScrollbarSize = 14.0f;
            style->ScrollbarRounding = 9.0f;
            style->GrabMinSize = 10.0f;
            style->GrabRounding = 0.0f;
            style->TabRounding = 4.0f;
            style->TabBorderSize = 0.0f;
            style->ColorButtonPosition = ImGuiDir.ImGuiDir_Right;
            style->ButtonTextAlign = new Vector2(0.5f, 0.5f);
            style->SelectableTextAlign = new Vector2(0.0f, 0.0f);
            style->DisplayWindowPadding = new Vector2(19.0f, 19.0f);
            style->DisplaySafeAreaPadding = new Vector2(3.0f, 3.0f);
            style->MouseCursorScale = 1.0f;
            style->AntiAliasedLines = true;
            style->AntiAliasedFill = true;
            style->CurveTessellationTol = 1.25f;
        }
    }
}
