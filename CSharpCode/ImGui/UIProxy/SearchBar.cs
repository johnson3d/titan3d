using NPOI.HSSF.Record.AutoFilter;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui.UIProxy
{
    public class SearchBarProxy : IUIProxyBase
    {
        public string SearchText;
        public string InfoText;
        public float Width = 60;

        //ImageProxy mIcon;
        bool mFocused = false;

        public void Cleanup()
        {
            //mIcon?.Dispose();
            //mIcon = null;
        }

        static string SearchBarIconKey = "SearchBarIcon";

        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            if(UEngine.Instance.UIProxyManager[SearchBarIconKey] == null)
            {
                var icon = new ImageProxy()
                {
                    ImageFile = RName.GetRName("icons/icons.srv", RName.ERNameType.Engine),
                    ImageSize = new Vector2(16, 16),
                    UVMin = new Vector2(3.0f / 1024, 655.0f / 1024),
                    UVMax = new Vector2(19.0f / 1024, 671.0f / 1024),
                };
                await icon.Initialize();
                UEngine.Instance.UIProxyManager[SearchBarIconKey] = icon;
            }

            return true;
        }

        public unsafe bool OnDraw(in ImDrawList drawList, in Support.UAnyPointer drawData)
        {
            return OnDraw(ref mFocused, in drawList, InfoText, ref SearchText, Width);
        }

        public unsafe static bool OnDraw(ref bool focused, in ImDrawList drawList, string infoText, ref string searchText, float width)
        {
            bool retValue = false;
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, in StyleConfig.Instance.PGSearchBoxFramePadding);
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FrameBorderSize, 1.0f);
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FrameRounding, 12.0f);

            if (focused)
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Border, EGui.UIProxy.StyleConfig.Instance.PGSearchBoxFocusBorderColor);

            ImGuiAPI.SetNextItemWidth(width);
            using (var buffer = BigStackBuffer.CreateInstance(64))
            {
                buffer.SetTextUtf8("Ch:中");
                ImGuiAPI.TextAsPointer((sbyte*)buffer.GetBuffer());
            }
            retValue = ImGuiAPI.InputText(TName.FromString2("##", "PropertyGridFilterString").ToString(), ref searchText);

            var itemMin = ImGuiAPI.GetItemRectMin();
            var itemMax = ImGuiAPI.GetItemRectMax();
            var icon = UEngine.Instance.UIProxyManager[SearchBarIconKey] as ImageProxy;
            if (icon != null)
            {
                var pos = new Vector2(itemMin.X + 6, itemMin.Y + (itemMax.Y - itemMin.Y - icon.ImageSize.Y) * 0.5f);
                icon.OnDraw(in drawList, in pos);
            }

            if (focused)
                ImGuiAPI.PopStyleColor(1);
            focused = ImGuiAPI.IsItemFocused();

            if (string.IsNullOrEmpty(searchText))
            {
                var textSize = ImGuiAPI.CalcTextSize(infoText, false, 0);
                var pos = new Vector2(itemMin.X + StyleConfig.Instance.PGSearchBoxFramePadding.X, itemMin.Y + (itemMax.Y - itemMin.Y - textSize.Y) * 0.5f);
                drawList.AddText(in pos, StyleConfig.Instance.PGSearchBoxInfoTextColor, infoText, null);
            }

            ImGuiAPI.PopStyleVar(3);
            return retValue;
        }
    }
}
