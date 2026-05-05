using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.EGui.UIProxy
{
    [Rtti.Meta("")]
    public class StyleConfig
    {
        public static StyleConfig Instance { get; } = new StyleConfig();

        public UInt32 TransparentColor = 0x000000FF;

        // Common
        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 ItemSpacing = new Vector2(6, 4);

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 WindowBackground = 0xFF1F1A18;

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PanelBackground = 0xFF292320;

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 SecondPanelBackground = 0xFF312A25;

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 PanelFramePadding = new Vector2(6, 5);

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 PGNormalItemSpacing = new Vector2(0, 0);

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 GridColor = 0xFF443A34;

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 SeparatorColor = 0xFF443A34;

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 WindowsPadding = new Vector2(6, 6);

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PopupColor = 0xFF292320;

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 PopupWindowsPadding = new Vector2(8, 8);

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 PopupItemSpacing = new Vector2(8, 8);

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public float PopupBordersize = 1.0f;

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PopupHoverColor = 0xFF3D3630;

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 TextColor = 0xFFE2DCD7;

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 TextHoveredColor = 0xFFFFFFFF;

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 TextSelectedColor = 0xFFFFFFFF;

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 TextDisableColor = 0xFF968B84;

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ItemHightlightHoveredColor = 0xFF463E35;

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 WarningStringColor = 0xFF5AB4E5;

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ErrorStringColor = 0xFF756CE0;

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PassStringColor = 0xFF8ACB7D;

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 LinkStringColor = 0xFFE8B776;

        // Menu
        [Category("Menu")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 NamedMenuSeparatorColor = 0xFF443A34;

        [Category("Menu")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public float NamedMenuSeparatorThickness = 1.0f;// new Vector4(0.42f, 0.42f, 0.42f, 1.00f);

        [Category("Menu")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 MenuHeaderColor = 0xFFCF8C5E;

        [Category("Menu")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 MenuBG = 0xFF292320;

        [Category("Menu")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 WindowPadding = new Vector2(0, 0);

        [Category("Menu")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 MenuPadding = new Vector2(40, 8);

        [Category("Menu")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 TopMenuWindowPadding = new Vector2(1, 8);

        [Category("Menu")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 TopMenuFramePadding = new Vector2(6, 5);

        [Category("Menu")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 TopMenuItemSpacing = new Vector2(8, 6);

        [Category("Menu")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 MenuItemFramePadding = new Vector2(4, 4);

        [Category("Menu")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 MenuItemSpacing = new Vector2(8, 8);

        [Category("Menu")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public float MenuItemIndent = 15;

        // Tab
        [Category("Tab")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 MainTabFramePadding = new Vector2(10, 6);

        // Toolbar
        [Category("Toolbar")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public float ToolbarButtonIconTextSpacing = 4;

        [Category("Toolbar")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ToolbarButtonTextColor = 0xFFE2DCD7;

        [Category("Toolbar")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ToolbarButtonTextColor_Hover = 0xFFFFFFFF;

        [Category("Toolbar")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ToolbarButtonTextColor_Press = 0xFFCF8C5E;

        [Category("Toolbar")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ToolbarButtonTextColor_Disable = 0xFF7B736B;

        [Category("Toolbar")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public float ToolbarHeight = 32;

        [Category("Toolbar")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ToolbarBG = 0xFF292320;

        [Category("Toolbar")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public float ToolbarSeparatorThickness = 1;

        // Button
        [Category("Button")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ToolButtonTextColor = 0xFFE2DCD7;

        [Category("Button")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ToolButtonTextColor_Hover = 0xFFFFFFFF;// new Vector4(1.00f, 1.00f, 1.00f, 1.00f);

        [Category("Button")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ToolButtonTextColor_Press = 0xFFCF8C5E;

        // PropertyGrid
        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 PGWindowPadding = new Vector2(4, 4);

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 PGNormalFramePadding = new Vector2(0, 6);


        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 PGCheckboxFramePadding = new Vector2(2, 2);

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGItemHoveredColor = 0xFF383029;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 PGInputFramePadding = new Vector2(7, 4);

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGCategoryBG = 0xFF312A25;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 PGCategoryPadding = new Vector2(8, 6);

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public float PGNormalFrameBorderSize = 0.0f;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public float PGNormalFrameRounding = 4.0f;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 PGSearchBoxFramePadding = new Vector2(28, 4);

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGSearchBoxFocusBorderColor = 0xFFCF8C5E;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGSearchBoxInfoTextColor = 0xFF968B84;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGHeadColor = 0xFF292320;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGCellBorderInnerColor = 0xFF443A34;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGCellBorderOutterColor = 0xFF443A34;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 PGCellPadding = new Vector2(8, 3);

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 PGColorBoxSize = new Vector2(30, 15);

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public float PGColorBoxRound = 3.0f;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public float PGItemBorderThickness = 1.0f;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGItemBorderNormalColor = 0xFF443A34;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGItemBorderHoveredColor = 0xFF5A4F45;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGCreateButtonBGColor = 0xFF688D5B;
        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGCreateButtonBGHoverColor = 0xFF78A56A;
        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGCreateButtonBGActiveColor = 0xFF587C4F;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGDeleteButtonBGColor = 0xFF7D5C67;
        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGDeleteButtonBGHoverColor = 0xFF916A76;
        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGDeleteButtonBGActiveColor = 0xFF6C4F59;

        [Category("TreeView")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 TVHeader = 0xFF292320;
        [Category("TreeView")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 TVHeaderHovered = 0xFF383029;
        [Category("TreeView")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 TVHeaderActive = 0xFFCF8C5E;

        public unsafe void ResetStyle()
        {
            var style = ImGuiAPI.GetStyle();
            Vector4 C(byte r, byte g, byte b, byte a = 255)
            {
                const float inv = 1.0f / 255.0f;
                return new Vector4(r * inv, g * inv, b * inv, a * inv);
            }

            var bg = C(0x18, 0x1A, 0x1F);
            var panel = C(0x20, 0x23, 0x29);
            var panel2 = C(0x25, 0x2A, 0x31);
            var panel3 = C(0x2B, 0x31, 0x39);
            var border = C(0x34, 0x3A, 0x44);
            var text = C(0xD7, 0xDC, 0xE2);
            var textMuted = C(0x84, 0x8B, 0x96);
            var accent = C(0x5E, 0x8C, 0xCF);
            var accentHover = C(0x6E, 0xA0, 0xE8);
            var accentSoft = C(0x35, 0x45, 0x5F);

            style->Colors[(int)ImGuiCol_.ImGuiCol_Text] = text;
            style->Colors[(int)ImGuiCol_.ImGuiCol_TextDisabled] = textMuted;
            style->Colors[(int)ImGuiCol_.ImGuiCol_WindowBg] = bg;
            style->Colors[(int)ImGuiCol_.ImGuiCol_ChildBg] = panel;
            style->Colors[(int)ImGuiCol_.ImGuiCol_PopupBg] = C(0x22, 0x25, 0x2C);
            style->Colors[(int)ImGuiCol_.ImGuiCol_Border] = border;
            style->Colors[(int)ImGuiCol_.ImGuiCol_BorderShadow] = C(0, 0, 0, 0);
            style->Colors[(int)ImGuiCol_.ImGuiCol_FrameBg] = C(0x24, 0x28, 0x30);
            style->Colors[(int)ImGuiCol_.ImGuiCol_FrameBgHovered] = C(0x2D, 0x34, 0x3E);
            style->Colors[(int)ImGuiCol_.ImGuiCol_FrameBgActive] = C(0x33, 0x3C, 0x49);
            style->Colors[(int)ImGuiCol_.ImGuiCol_TitleBg] = bg;
            style->Colors[(int)ImGuiCol_.ImGuiCol_TitleBgActive] = panel;
            style->Colors[(int)ImGuiCol_.ImGuiCol_TitleBgCollapsed] = bg;
            style->Colors[(int)ImGuiCol_.ImGuiCol_MenuBarBg] = panel;
            style->Colors[(int)ImGuiCol_.ImGuiCol_ScrollbarBg] = C(0x1B, 0x1D, 0x23);
            style->Colors[(int)ImGuiCol_.ImGuiCol_ScrollbarGrab] = C(0x3A, 0x41, 0x4C);
            style->Colors[(int)ImGuiCol_.ImGuiCol_ScrollbarGrabHovered] = C(0x48, 0x52, 0x60);
            style->Colors[(int)ImGuiCol_.ImGuiCol_ScrollbarGrabActive] = C(0x55, 0x63, 0x76);
            style->Colors[(int)ImGuiCol_.ImGuiCol_CheckMark] = accentHover;
            style->Colors[(int)ImGuiCol_.ImGuiCol_SliderGrab] = accent;
            style->Colors[(int)ImGuiCol_.ImGuiCol_SliderGrabActive] = accentHover;
            style->Colors[(int)ImGuiCol_.ImGuiCol_Button] = C(0x2A, 0x2F, 0x37);
            style->Colors[(int)ImGuiCol_.ImGuiCol_ButtonHovered] = C(0x33, 0x3B, 0x47);
            style->Colors[(int)ImGuiCol_.ImGuiCol_ButtonActive] = accentSoft;
            style->Colors[(int)ImGuiCol_.ImGuiCol_Header] = C(0x26, 0x2B, 0x33);
            style->Colors[(int)ImGuiCol_.ImGuiCol_HeaderHovered] = C(0x2F, 0x37, 0x43);
            style->Colors[(int)ImGuiCol_.ImGuiCol_HeaderActive] = accentSoft;
            style->Colors[(int)ImGuiCol_.ImGuiCol_Separator] = border;
            style->Colors[(int)ImGuiCol_.ImGuiCol_SeparatorHovered] = C(0x48, 0x55, 0x66);
            style->Colors[(int)ImGuiCol_.ImGuiCol_SeparatorActive] = accent;
            style->Colors[(int)ImGuiCol_.ImGuiCol_ResizeGrip] = C(0x34, 0x3A, 0x44, 120);
            style->Colors[(int)ImGuiCol_.ImGuiCol_ResizeGripHovered] = C(0x5E, 0x8C, 0xCF, 170);
            style->Colors[(int)ImGuiCol_.ImGuiCol_ResizeGripActive] = accentHover;
            style->Colors[(int)ImGuiCol_.ImGuiCol_InputTextCursor] = accentHover;
            style->Colors[(int)ImGuiCol_.ImGuiCol_Tab] = C(0x20, 0x23, 0x29);
            style->Colors[(int)ImGuiCol_.ImGuiCol_TabHovered] = C(0x2F, 0x37, 0x43);
            style->Colors[(int)ImGuiCol_.ImGuiCol_TabSelected] = C(0x25, 0x2A, 0x31);
            style->Colors[(int)ImGuiCol_.ImGuiCol_TabSelectedOverline] = accent;
            style->Colors[(int)ImGuiCol_.ImGuiCol_TabDimmed] = C(0x1C, 0x1F, 0x25);
            style->Colors[(int)ImGuiCol_.ImGuiCol_TabDimmedSelected] = panel;
            style->Colors[(int)ImGuiCol_.ImGuiCol_TabDimmedSelectedOverline] = C(0x4A, 0x63, 0x89);
            style->Colors[(int)ImGuiCol_.ImGuiCol_DockingPreview] = C(0x5E, 0x8C, 0xCF, 90);
            style->Colors[(int)ImGuiCol_.ImGuiCol_DockingEmptyBg] = bg;
            style->Colors[(int)ImGuiCol_.ImGuiCol_PlotLines] = accent;
            style->Colors[(int)ImGuiCol_.ImGuiCol_PlotLinesHovered] = accentHover;
            style->Colors[(int)ImGuiCol_.ImGuiCol_PlotHistogram] = C(0x8A, 0xA8, 0x6D);
            style->Colors[(int)ImGuiCol_.ImGuiCol_PlotHistogramHovered] = C(0xA6, 0xC4, 0x86);
            style->Colors[(int)ImGuiCol_.ImGuiCol_TableHeaderBg] = panel2;
            style->Colors[(int)ImGuiCol_.ImGuiCol_TableBorderStrong] = border;
            style->Colors[(int)ImGuiCol_.ImGuiCol_TableBorderLight] = C(0x2D, 0x33, 0x3C);
            style->Colors[(int)ImGuiCol_.ImGuiCol_TableRowBg] = C(0x1C, 0x1F, 0x25);
            style->Colors[(int)ImGuiCol_.ImGuiCol_TableRowBgAlt] = C(0x20, 0x24, 0x2B);
            style->Colors[(int)ImGuiCol_.ImGuiCol_TextLink] = C(0x76, 0xB7, 0xE8);
            style->Colors[(int)ImGuiCol_.ImGuiCol_TextSelectedBg] = C(0x5E, 0x8C, 0xCF, 105);
            style->Colors[(int)ImGuiCol_.ImGuiCol_TreeLines] = C(0x42, 0x49, 0x55);
            style->Colors[(int)ImGuiCol_.ImGuiCol_DragDropTarget] = accentHover;
            style->Colors[(int)ImGuiCol_.ImGuiCol_DragDropTargetBg] = C(0x5E, 0x8C, 0xCF, 70);
            style->Colors[(int)ImGuiCol_.ImGuiCol_UnsavedMarker] = C(0xE5, 0xB4, 0x5A);
            style->Colors[(int)ImGuiCol_.ImGuiCol_NavCursor] = accentHover;
            style->Colors[(int)ImGuiCol_.ImGuiCol_NavWindowingHighlight] = C(0xD7, 0xDC, 0xE2, 170);
            style->Colors[(int)ImGuiCol_.ImGuiCol_NavWindowingDimBg] = C(0x0B, 0x0C, 0x0F, 140);
            style->Colors[(int)ImGuiCol_.ImGuiCol_ModalWindowDimBg] = C(0x0B, 0x0C, 0x0F, 170);

            style->Alpha = 1.0f;
            style->FontSizeBase = 15.0f;
            style->FontScaleMain = 1.0f;
            if (style->FontScaleDpi <= 0.0f)
                style->FontScaleDpi = 1.0f;
            style->WindowPadding = WindowsPadding;
            style->WindowRounding = 4.0f;
            style->WindowBorderSize = 1.0f;
            style->WindowBorderHoverPadding = 4.0f;
            style->WindowMinSize = new Vector2(32, 32);
            style->WindowTitleAlign = new Vector2(0, 0.5f);
            style->WindowMenuButtonPosition = ImGuiDir.ImGuiDir_Left;
            style->ChildRounding = 4.0f;
            style->ChildBorderSize = 1.0f;
            style->PopupRounding = 5.0f;
            style->PopupBorderSize = 1.0f;
            style->FramePadding = new Vector2(7.0f, 4.0f);
            style->FrameRounding = 4.0f;
            style->FrameBorderSize = 0.0f;
            style->ItemSpacing = ItemSpacing;
            style->ItemInnerSpacing = new Vector2(6.0f, 4.0f);
            style->CellPadding = new Vector2(6.0f, 3.0f);
            style->TouchExtraPadding = new Vector2(0, 0);
            style->IndentSpacing = 18.0f;
            style->ColumnsMinSpacing = 6.0f;
            style->ScrollbarSize = 12.0f;
            style->ScrollbarRounding = 6.0f;
            style->ScrollbarPadding = 2.0f;
            style->GrabMinSize = 10.0f;
            style->GrabRounding = 4.0f;
            style->ImageRounding = 3.0f;
            style->ImageBorderSize = 0.0f;
            style->TabRounding = 4.0f;
            style->TabBorderSize = 0.0f;
            style->TabMinWidthBase = 42.0f;
            style->TabMinWidthShrink = 24.0f;
            style->TabBarBorderSize = 1.0f;
            style->TabBarOverlineSize = 1.0f;
            style->TreeLinesFlags = (int)ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_DrawLinesNone;
            style->TreeLinesSize = 1.0f;
            style->TreeLinesRounding = 0.0f;
            style->DockingNodeHasCloseButton = true;
            style->DockingSeparatorSize = 2.0f;
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
