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
            if(UEngine.Instance.UIManager[SearchBarIconKey] == null)
            {
                var icon = new ImageProxy()
                {
                    ImageFile = RName.GetRName("icons/icons.srv", RName.ERNameType.Engine),
                    ImageSize = new Vector2(16, 16),
                    UVMin = new Vector2(3.0f / 1024, 655.0f / 1024),
                    UVMax = new Vector2(19.0f / 1024, 671.0f / 1024),
                };
                await icon.Initialize();
                UEngine.Instance.UIManager[SearchBarIconKey] = icon;
            }

            return true;
        }

        public unsafe bool OnDraw(ref ImDrawList drawList, ref Support.UAnyPointer drawData)
        {
            bool retValue = false;
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, in StyleConfig.Instance.PGSearchBoxFramePadding);
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FrameBorderSize, 1.0f);
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FrameRounding, 12.0f);
             
            if(mFocused)
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Border, EGui.UIProxy.StyleConfig.Instance.PGSearchBoxFocusBorderColor);

            var buffer = BigStackBuffer.CreateInstance(256);
            var oldValue = SearchText;
            buffer.SetText(oldValue);
            ImGuiAPI.SetNextItemWidth(Width);
            retValue = ImGuiAPI.InputText(TName.FromString2("##", "PropertyGridFilterString").ToString(), buffer.GetBuffer(), (uint)buffer.GetSize(),
                ImGuiInputTextFlags_.ImGuiInputTextFlags_None, null, (void*)0);

            var itemMin = ImGuiAPI.GetItemRectMin();
            var itemMax = ImGuiAPI.GetItemRectMax();
            var icon = UEngine.Instance.UIManager[SearchBarIconKey] as ImageProxy;
            if (icon != null)
            {
                var pos = new Vector2(itemMin.X + 6, itemMin.Y + (itemMax.Y - itemMin.Y - icon.ImageSize.Y) * 0.5f);
                icon.OnDraw(ref drawList, ref pos);
            }

            if (mFocused)
                ImGuiAPI.PopStyleColor(1);
            mFocused = ImGuiAPI.IsItemFocused();

            SearchText = buffer.AsText();            
            if (string.IsNullOrEmpty(SearchText))
            {
                var textSize = ImGuiAPI.CalcTextSize(InfoText, false, 0);
                var pos = new Vector2(itemMin.X + StyleConfig.Instance.PGSearchBoxFramePadding.X, itemMin.Y + (itemMax.Y - itemMin.Y - textSize.Y) * 0.5f);
                drawList.AddText(in pos, StyleConfig.Instance.PGSearchBoxInfoTextColor, InfoText, null);
            }
            buffer.DestroyMe();

            ImGuiAPI.PopStyleVar(3);
            return retValue;
        }
    }
}
