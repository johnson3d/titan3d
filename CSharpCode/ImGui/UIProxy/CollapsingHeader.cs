using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui.UIProxy
{
    public class CollapsingHeaderProxy
    {
        public static bool CollapsingHeader(string label, ref bool open, ImGuiTreeNodeFlags_ flags)
        {
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Header, StyleConfig.Instance.PGCategoryBG);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_HeaderActive, StyleConfig.Instance.PGCategoryBG);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_HeaderHovered, StyleConfig.Instance.PGCategoryBG);
            var retValue = ImGuiAPI.CollapsingHeader(label, ref open, flags);
            ImGuiAPI.PopStyleColor(3);
            return retValue;
        }
        public static bool CollapsingHeader(string label, ImGuiTreeNodeFlags_ flags)
        {
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Header, StyleConfig.Instance.PGCategoryBG);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_HeaderActive, StyleConfig.Instance.PGCategoryBG);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_HeaderHovered, StyleConfig.Instance.PGCategoryBG);
            var retValue = ImGuiAPI.CollapsingHeader(label, flags);
            ImGuiAPI.PopStyleColor(3);
            return retValue;
        }
    }
}
