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

        public static Vector4 ToColor4(UInt32 abgr)
        {
            const float inv = 1.0f / 255.0f;
            return new Vector4(
                (abgr & 0xFF) * inv,
                ((abgr >> 8) & 0xFF) * inv,
                ((abgr >> 16) & 0xFF) * inv,
                ((abgr >> 24) & 0xFF) * inv);
        }

        public void PushPopupStyle()
        {
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_PopupBg, PopupColor);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Border, BorderColor);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Header, MenuHeaderColor);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_HeaderHovered, PopupHoverColor);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_HeaderActive, MenuHeaderActiveColor);
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_WindowPadding, in PopupWindowsPadding);
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_ItemSpacing, in PopupItemSpacing);
        }

        public void PopPopupStyle()
        {
            ImGuiAPI.PopStyleVar(2);
            ImGuiAPI.PopStyleColor(5);
        }

        public void PushPanelChildStyle(UInt32 background)
        {
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_ChildBg, background);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Border, BorderColor);
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_ChildRounding, 3.0f);
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_ItemSpacing, in ItemSpacing);
        }

        public void PopPanelChildStyle()
        {
            ImGuiAPI.PopStyleVar(2);
            ImGuiAPI.PopStyleColor(2);
        }

        // Titan Pro Dark tokens. Colors are stored in ImGui ABGR order.
        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 AccentColor = 0xFFCEA34F;

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 AccentHoveredColor = 0xFFE2BD6C;

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 AccentActiveColor = 0xFFB8882F;

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 BorderColor = 0xFF332920;

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 BorderActiveColor = 0xFF76624A;

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 FrameBackground = 0xFF261F18;

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 FrameBackgroundHovered = 0xFF332A20;

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 FrameBackgroundActive = 0xFF433627;

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 TableRowColor = 0xFF17130F;

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 TableRowAltColor = 0xFF1D1812;

        // Common
        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 ItemSpacing = new Vector2(6, 5);

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 WindowBackground = 0xFF15110F;

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PanelBackground = 0xFF221C18;// new Vector4(0.14f, 0.14f, 0.14f, 1.00f);

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 SecondPanelBackground = 0xFF1B1511;

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 PanelFramePadding = new Vector2(6, 5);

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 PGNormalItemSpacing = new Vector2(0, 2);

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 GridColor = 0xFF40332A;// new Vector4(0.14f, 0.14f, 0.14f, 1.00f);

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 SeparatorColor = 0xFF30261F;

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 WindowsPadding = new Vector2(6, 6);

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PopupColor = 0xFF211A15;

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 PopupWindowsPadding = new Vector2(8, 7);

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 PopupItemSpacing = new Vector2(6, 4);

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public float PopupBordersize = 1.0f;

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PopupHoverColor = 0xFF332A20;// 38, 186 new Vector4(0.15f, 0.73f, 1.00f, 1.00f)

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 TextColor = 0xFFE8DED6;

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
        public UInt32 TextDisableColor = 0xFF998A7F;

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ItemHightlightHoveredColor = 0xFF42362B;

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 WarningStringColor = 0xFF4CC9F2;

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ErrorStringColor = 0xFF6670F9;

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PassStringColor = 0xFF83D445;

        [Category("Common")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 LinkStringColor = 0xFFD6A76B;

        // Menu
        [Category("Menu")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 NamedMenuSeparatorColor = 0xff8E8072;// new Vector4(0.42f, 0.42f, 0.42f, 1.00f);

        [Category("Menu")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public float NamedMenuSeparatorThickness = 1.0f;// new Vector4(0.42f, 0.42f, 0.42f, 1.00f);

        [Category("Menu")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 MenuHeaderColor = 0xFF2D251D;// 38, 186 new Vector4(0.15f, 0.73f, 1.00f, 1.00f)

        [Category("Menu")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 MenuHeaderHoveredColor = 0xFF3A3025;

        [Category("Menu")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 MenuHeaderActiveColor = 0xFF4A3B2B;

        [Category("Menu")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 MenuBG = 0xFF211A15;// (0.22f, 0.22f, 0.22f, 1.00f)

        [Category("Menu")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 WindowPadding = new Vector2(0, 0);

        [Category("Menu")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 MenuPadding = new Vector2(32, 7);

        [Category("Menu")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 TopMenuWindowPadding = new Vector2(4, 5);

        [Category("Menu")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 TopMenuFramePadding = new Vector2(8, 5);

        [Category("Menu")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 TopMenuItemSpacing = new Vector2(8, 6);

        [Category("Menu")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 MenuItemFramePadding = new Vector2(8, 5);

        [Category("Menu")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 MenuItemSpacing = new Vector2(8, 5);

        [Category("Menu")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public float MenuItemIndent = 12;

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
        public UInt32 ToolbarButtonTextColor = 0xFFCFC4BA;// new Vector4(0.75f, 0.75f, 0.75f, 1.00f);

        [Category("Toolbar")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ToolbarButtonTextColor_Hover = 0xFFFFFFFF;// new Vector4(1.00f, 1.00f, 1.00f, 1.00f);

        [Category("Toolbar")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ToolbarButtonTextColor_Press = 0xFFFFFFFF;// new Vector4(1.00f, 1.00f, 1.00f, 1.00f);

        [Category("Toolbar")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ToolbarButtonTextColor_Disable = 0xFF7D7065;// new Vector4(1.00f, 1.00f, 1.00f, 1.00f);

        [Category("Toolbar")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public float ToolbarHeight = 40;

        [Category("Toolbar")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ToolbarBG = 0xFF1F1914;// new Vector4(0.14f, 0.14f, 0.14f, 1.00f);

        [Category("Toolbar")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public float ToolbarSeparatorThickness = 1;

        // Content Browser
        [Category("ContentBrowser")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ContentBrowserFolderBg = 0xFF19130F;

        [Category("ContentBrowser")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ContentBrowserAssetPaneBg = 0xFF16110D;

        [Category("ContentBrowser")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ContentBrowserAssetTileBg = 0xFF211A15;

        [Category("ContentBrowser")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ContentBrowserAssetTileHoveredBg = 0xFF2C241B;

        [Category("ContentBrowser")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ContentBrowserAssetTileSelectedBg = 0xFF392E22;

        [Category("ContentBrowser")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ContentBrowserAssetTileBorder = 0xFF30261F;

        [Category("ContentBrowser")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ContentBrowserAssetTileSelectedBorder = 0xFFE2BD6C;

        [Category("ContentBrowser")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ContentBrowserStatusBg = 0xFF211A15;

        [Category("ContentBrowser")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ContentBrowserStatusBorder = 0xFF3E3226;

        [Category("ContentBrowser")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 ContentBrowserChipPadding = new Vector2(8, 3);

        [Category("ContentBrowser")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public float ContentBrowserTileRounding = 4.0f;

        // Asset Editor
        [Category("AssetEditor")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 AssetEditorActiveBorder = 0xFFE2BD6C;

        [Category("AssetEditor")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 AssetEditorInactiveBorder = 0xFF30261F;

        [Category("AssetEditor")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 AssetEditorProgressBg = 0xFF211A15;

        [Category("AssetEditor")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 AssetEditorProgressFill = 0xFF83D445;

        // Button
        [Category("Button")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ToolButtonTextColor = 0xFFCFC4BA;// new Vector4(0.75f, 0.75f, 0.75f, 1.00f);

        [Category("Button")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ToolButtonTextColor_Hover = 0xFFFFFFFF;// new Vector4(1.00f, 1.00f, 1.00f, 1.00f);

        [Category("Button")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 ToolButtonTextColor_Press = 0xFFE2BD6C;// new Vector4(1.00f, 1.00f, 1.00f, 1.00f);

        // PropertyGrid
        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 PGWindowPadding = new Vector2(4, 4);

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 PGNormalFramePadding = new Vector2(4, 6);


        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 PGCheckboxFramePadding = new Vector2(2, 2);

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGItemHoveredColor = 0xFF2E251D;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 PGInputFramePadding = new Vector2(8, 5);

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGCategoryBG = 0xFF261F18;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 PGCategoryPadding = new Vector2(8, 6);

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public float PGNormalFrameBorderSize = 1.0f;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public float PGNormalFrameRounding = 3.0f;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 PGSearchBoxFramePadding = new Vector2(30, 6);

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGSearchBoxFocusBorderColor = 0xFFE2BD6C;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGSearchBoxInfoTextColor = 0xFF817468;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGHeadColor = 0xFF211A15;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGCellBorderInnerColor = 0xFF30261F;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGCellBorderOutterColor = 0xFF30261F;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 PGCellPadding = new Vector2(8, 4);

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public Vector2 PGColorBoxSize = new Vector2(30, 15);

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public float PGColorBoxRound = 3.0f;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        public float PGItemBorderThickness = 3.0f;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGItemBorderNormalColor = 0xFF30261F;//0xFF454545;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGItemBorderHoveredColor = 0xFF76624A;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGCreateButtonBGColor = 0xFF73A63A;
        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGCreateButtonBGHoverColor = 0xFF83D445;
        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGCreateButtonBGActiveColor = 0xFF5F8A2F;

        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGDeleteButtonBGColor = 0xFF6670F9;
        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGDeleteButtonBGHoverColor = 0xFF8088FF;
        [Category("PropertyGrid")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 PGDeleteButtonBGActiveColor = 0xFF525CD0;

        [Category("TreeView")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 TVHeader = 0xFF2D251D;
        [Category("TreeView")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 TVHeaderHovered = 0xFF3A3025;
        [Category("TreeView")]
        [Controls.PropertyGrid.TtShowInPropertyGrid]
        [Controls.PropertyGrid.TtByte4ToColor4PickerEditor(IsABGR = true)]
        public UInt32 TVHeaderActive = 0xFF4A3B2B;

        public unsafe void ResetStyle()
        {
            var style = ImGuiAPI.GetStyle();
            var colors = style->Colors;

            colors[(int)ImGuiCol_.ImGuiCol_Text] = ToColor4(TextColor);
            colors[(int)ImGuiCol_.ImGuiCol_TextDisabled] = ToColor4(TextDisableColor);
            colors[(int)ImGuiCol_.ImGuiCol_WindowBg] = ToColor4(WindowBackground);
            colors[(int)ImGuiCol_.ImGuiCol_ChildBg] = ToColor4(PanelBackground);
            colors[(int)ImGuiCol_.ImGuiCol_PopupBg] = ToColor4(PopupColor);
            colors[(int)ImGuiCol_.ImGuiCol_Border] = ToColor4(BorderColor);
            colors[(int)ImGuiCol_.ImGuiCol_BorderShadow] = new Vector4(0, 0, 0, 0);
            colors[(int)ImGuiCol_.ImGuiCol_FrameBg] = ToColor4(FrameBackground);
            colors[(int)ImGuiCol_.ImGuiCol_FrameBgHovered] = ToColor4(FrameBackgroundHovered);
            colors[(int)ImGuiCol_.ImGuiCol_FrameBgActive] = ToColor4(FrameBackgroundActive);
            colors[(int)ImGuiCol_.ImGuiCol_TitleBg] = ToColor4(WindowBackground);
            colors[(int)ImGuiCol_.ImGuiCol_TitleBgActive] = ToColor4(PanelBackground);
            colors[(int)ImGuiCol_.ImGuiCol_TitleBgCollapsed] = ToColor4(WindowBackground);
            colors[(int)ImGuiCol_.ImGuiCol_MenuBarBg] = ToColor4(WindowBackground);
            colors[(int)ImGuiCol_.ImGuiCol_ScrollbarBg] = ToColor4(WindowBackground);
            colors[(int)ImGuiCol_.ImGuiCol_ScrollbarGrab] = ToColor4(BorderColor);
            colors[(int)ImGuiCol_.ImGuiCol_ScrollbarGrabHovered] = ToColor4(BorderActiveColor);
            colors[(int)ImGuiCol_.ImGuiCol_ScrollbarGrabActive] = ToColor4(AccentActiveColor);
            colors[(int)ImGuiCol_.ImGuiCol_CheckMark] = ToColor4(AccentHoveredColor);
            colors[(int)ImGuiCol_.ImGuiCol_SliderGrab] = ToColor4(AccentColor);
            colors[(int)ImGuiCol_.ImGuiCol_SliderGrabActive] = ToColor4(AccentHoveredColor);
            colors[(int)ImGuiCol_.ImGuiCol_Button] = ToColor4(FrameBackground);
            colors[(int)ImGuiCol_.ImGuiCol_ButtonHovered] = ToColor4(FrameBackgroundHovered);
            colors[(int)ImGuiCol_.ImGuiCol_ButtonActive] = ToColor4(FrameBackgroundActive);
            colors[(int)ImGuiCol_.ImGuiCol_Header] = ToColor4(MenuHeaderColor);
            colors[(int)ImGuiCol_.ImGuiCol_HeaderHovered] = ToColor4(MenuHeaderHoveredColor);
            colors[(int)ImGuiCol_.ImGuiCol_HeaderActive] = ToColor4(MenuHeaderActiveColor);
            colors[(int)ImGuiCol_.ImGuiCol_Separator] = ToColor4(SeparatorColor);
            colors[(int)ImGuiCol_.ImGuiCol_SeparatorHovered] = ToColor4(BorderActiveColor);
            colors[(int)ImGuiCol_.ImGuiCol_SeparatorActive] = ToColor4(AccentColor);
            colors[(int)ImGuiCol_.ImGuiCol_ResizeGrip] = ToColor4(BorderColor);
            colors[(int)ImGuiCol_.ImGuiCol_ResizeGripHovered] = ToColor4(AccentColor);
            colors[(int)ImGuiCol_.ImGuiCol_ResizeGripActive] = ToColor4(AccentHoveredColor);
            colors[(int)ImGuiCol_.ImGuiCol_Tab] = ToColor4(WindowBackground);
            colors[(int)ImGuiCol_.ImGuiCol_TabHovered] = ToColor4(MenuHeaderHoveredColor);
            colors[(int)ImGuiCol_.ImGuiCol_TabActive] = ToColor4(PanelBackground);
            colors[(int)ImGuiCol_.ImGuiCol_TabUnfocused] = ToColor4(WindowBackground);
            colors[(int)ImGuiCol_.ImGuiCol_TabUnfocusedActive] = ToColor4(SecondPanelBackground);
            colors[(int)ImGuiCol_.ImGuiCol_DockingPreview] = ToColor4(AccentColor);
            colors[(int)ImGuiCol_.ImGuiCol_DockingEmptyBg] = ToColor4(WindowBackground);
            colors[(int)ImGuiCol_.ImGuiCol_PlotLines] = ToColor4(AccentColor);
            colors[(int)ImGuiCol_.ImGuiCol_PlotLinesHovered] = ToColor4(AccentHoveredColor);
            colors[(int)ImGuiCol_.ImGuiCol_PlotHistogram] = ToColor4(PassStringColor);
            colors[(int)ImGuiCol_.ImGuiCol_PlotHistogramHovered] = ToColor4(AccentHoveredColor);
            colors[(int)ImGuiCol_.ImGuiCol_TableHeaderBg] = ToColor4(PGHeadColor);
            colors[(int)ImGuiCol_.ImGuiCol_TableBorderStrong] = ToColor4(BorderColor);
            colors[(int)ImGuiCol_.ImGuiCol_TableBorderLight] = ToColor4(SeparatorColor);
            colors[(int)ImGuiCol_.ImGuiCol_TableRowBg] = ToColor4(TableRowColor);
            colors[(int)ImGuiCol_.ImGuiCol_TableRowBgAlt] = ToColor4(TableRowAltColor);
            colors[(int)ImGuiCol_.ImGuiCol_TextSelectedBg] = ToColor4(AccentActiveColor);
            colors[(int)ImGuiCol_.ImGuiCol_DragDropTarget] = ToColor4(AccentHoveredColor);
            colors[(int)ImGuiCol_.ImGuiCol_NavHighlight] = ToColor4(AccentHoveredColor);
            colors[(int)ImGuiCol_.ImGuiCol_NavWindowingHighlight] = ToColor4(AccentHoveredColor);
            colors[(int)ImGuiCol_.ImGuiCol_NavWindowingDimBg] = new Vector4(0.05f, 0.07f, 0.09f, 0.70f);
            colors[(int)ImGuiCol_.ImGuiCol_ModalWindowDimBg] = new Vector4(0.02f, 0.03f, 0.04f, 0.72f);

            style->Alpha = 1.0f;
            style->WindowPadding = WindowsPadding;
            style->WindowRounding = 4.0f;
            style->WindowBorderSize = 1.0f;
            style->WindowMinSize = new Vector2(32, 32);
            style->WindowTitleAlign = new Vector2(0, 0.5f);
            style->WindowMenuButtonPosition = ImGuiDir.ImGuiDir_Left;
            style->ChildRounding = 3.0f;
            style->ChildBorderSize = 1.0f;
            style->PopupRounding = 5.0f;
            style->PopupBorderSize = PopupBordersize;
            style->FramePadding = new Vector2(8.0f, 5.0f);
            style->FrameRounding = 3.0f;
            style->FrameBorderSize = 1.0f;
            style->ItemSpacing = ItemSpacing;
            style->ItemInnerSpacing = new Vector2(5.0f, 4.0f);
            style->CellPadding = new Vector2(6.0f, 4.0f);
            style->TouchExtraPadding = new Vector2(0, 0);
            style->IndentSpacing = 18.0f;
            style->ColumnsMinSpacing = 8.0f;
            style->ScrollbarSize = 13.0f;
            style->ScrollbarRounding = 8.0f;
            style->GrabMinSize = 10.0f;
            style->GrabRounding = 3.0f;
            style->TabRounding = 4.0f;
            style->TabBorderSize = 0.0f;
            style->ColorButtonPosition = ImGuiDir.ImGuiDir_Right;
            style->ButtonTextAlign = new Vector2(0.5f, 0.5f);
            style->SelectableTextAlign = new Vector2(0.0f, 0.0f);
            style->DisplayWindowPadding = new Vector2(16.0f, 16.0f);
            style->DisplaySafeAreaPadding = new Vector2(4.0f, 4.0f);
            style->MouseCursorScale = 1.0f;
            style->AntiAliasedLines = true;
            style->AntiAliasedFill = true;
            style->CurveTessellationTol = 1.25f;
        }
    }
}
