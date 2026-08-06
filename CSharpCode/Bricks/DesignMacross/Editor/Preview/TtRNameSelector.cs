using EngineNS.EGui;
using EngineNS.Rtti;
using System;

namespace EngineNS.DesignMacross.Editor.Preview
{
    /// <summary>
    /// RName选择器控件 - 参考PGRNameAttribute/TtGraphElement_RNameSelect实现,
    /// 由snapshot缩略图 + ComboBox弹出ContentBrowser + F/&lt;/- 按钮组成, 可脱离PropertyGrid独立使用
    /// </summary>
    public class TtRNameSelector
    {
        public string Name = "##RNameSelector";
        public string FilterExts;   // "ext1" / "ext1,ext2"
        public System.Type MacrossType;

        EGui.Controls.TtContentBrowser mContentBrowser;
        EGui.UIProxy.ComboBox mComboBox;

        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            mContentBrowser = EngineNS.Editor.TtEditor.NewPopupContentBrowser();
            mComboBox = new EGui.UIProxy.ComboBox()
            {
                ComboOpenAction = ComboOpenAction
            };
            await mComboBox.Initialize();
            return true;
        }
        public void Cleanup()
        {
            mComboBox?.Cleanup();
            mComboBox = null;
            mContentBrowser = null;
        }
        void ComboOpenAction(in Support.TtAnyPointer data)
        {
            mContentBrowser.OnDraw();
        }

        /// <summary>
        /// 绘制选择器, 选中新资源时返回true并输出newValue
        /// </summary>
        public bool OnDraw(RName name, out RName newValue)
        {
            newValue = name;
            if (mComboBox == null)
                return false;

            var changedValue = name;

            var drawList = ImGuiAPI.GetWindowDrawList();
            ImGuiAPI.BeginGroup();
            ImGuiAPI.PushID(Name);

            // snapshot缩略图
            var groupStart = ImGuiAPI.GetCursorScreenPos();
            var snapSize = new Vector2(64, 64);
            var snapEnd = groupStart + snapSize;
            var assetMeta = TtEngine.Instance.AssetMetaManager.GetAssetMeta(name);
            drawList.AddRectFilled(in groupStart, in snapEnd, 0xff202020, 2.0f, ImDrawFlags_.ImDrawFlags_RoundCornersAll);
            var snapStart = groupStart;
            assetMeta?.OnDrawSnapshot(in drawList, ref snapStart, ref snapEnd);
            drawList.AddRect(in groupStart, in snapEnd, EGui.UIProxy.StyleConfig.Instance.PGItemBorderNormalColor, 2.0f, ImDrawFlags_.ImDrawFlags_RoundCornersAll, 1.0f);
            ImGuiAPI.InvisibleButton("##AssetPreview", in snapSize, ImGuiButtonFlags_.ImGuiButtonFlags_MouseButtonLeft);
            if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
            {
                if (assetMeta != null)
                    assetMeta.DrawTooltip();
                else if (name != null)
                    EGui.Controls.CtrlUtility.DrawHelper(name.ToString());
            }

            // ComboBox: 点击弹出ContentBrowser选择资源
            var preViewStr = "null";
            if (name != null)
                preViewStr = name.ToString();
            var editorStart = groupStart + new Vector2(snapSize.X + 8, 0);
            ImGuiAPI.SetCursorScreenPos(in editorStart);
            ImGuiAPI.Dummy(in Vector2.Zero);
            mComboBox.Flags = ImGuiComboFlags_.ImGuiComboFlags_None | ImGuiComboFlags_.ImGuiComboFlags_NoArrowButton | ImGuiComboFlags_.ImGuiComboFlags_HeightMask_;
            mComboBox.WinFlags = ImGuiWindowFlags_.ImGuiWindowFlags_Popup |
                                 ImGuiWindowFlags_.ImGuiWindowFlags_NoTitleBar |
                                 ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings |
                                 ImGuiWindowFlags_.ImGuiWindowFlags_NoMove;
            mComboBox.Width = ImGuiAPI.GetContentRegionAvail().X;
            if (mComboBox.Width < 120)
                mComboBox.Width = 120;
            mComboBox.Name = Name;
            mComboBox.PreviewValue = preViewStr;
            var contentBrowserSize = new Vector2(500, 600);
            ImGuiAPI.SetNextWindowSize(in contentBrowserSize, ImGuiCond_.ImGuiCond_Appearing);
            mContentBrowser.ExtNames = FilterExts;
            mContentBrowser.MacrossBase = TtTypeDesc.TypeOf(MacrossType);
            mContentBrowser.SelectedAssets.Clear();
            mComboBox.OnDraw(in drawList, in Support.TtAnyPointer.Default);
            if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
            {
                EGui.Controls.CtrlUtility.DrawHelper(preViewStr);
            }
            if (mContentBrowser.SelectedAssets.Count > 0 &&
                mContentBrowser.SelectedAssets[0].GetAssetName() != name)
            {
                changedValue = mContentBrowser.SelectedAssets[0].GetAssetName();
            }

            // F(聚焦资源) / <(取全局选中) / -(清空) 按钮
            var pos = editorStart + new Vector2(0, ImGuiAPI.GetFrameHeight() + 4);
            ImGuiAPI.SetCursorScreenPos(in pos);
            var sz = new Vector2(0, 0);
            if (ImGuiAPI.Button("F", in sz))
            {
                EGui.Controls.TtContentBrowser.GlobalFocusAsset = changedValue;
            }
            ImGuiAPI.SameLine(0, 8);
            if (ImGuiAPI.Button("<", in sz))
            {
                changedValue = EGui.Controls.TtContentBrowser.GlobalSelectedAsset?.GetAssetName();
            }
            ImGuiAPI.SameLine(0, 8);
            if (ImGuiAPI.Button("-", in sz))
            {
                changedValue = null;
            }

            ImGuiAPI.PopID();
            ImGuiAPI.EndGroup();

            if (changedValue != name)
            {
                newValue = changedValue;
                return true;
            }
            return false;
        }
    }
}
