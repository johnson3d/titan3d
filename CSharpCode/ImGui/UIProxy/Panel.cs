using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui.UIProxy
{
    public class DockPanelProxy
    {
        // 只能dock到指定window中
        public static bool Begin(in ImGuiWindowClass dockClass, string name, ref bool open, ImGuiWindowFlags_ flags)
        {
            ImGuiAPI.SetNextWindowClass(dockClass);
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, StyleConfig.Instance.PanelFramePadding);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_WindowBg, StyleConfig.Instance.PanelBackground);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_ChildBg, UIProxy.StyleConfig.Instance.PanelBackground);
            var retValue = ImGuiAPI.Begin(name, ref open, flags);
            return retValue;
        }
        public static unsafe bool Begin(in ImGuiWindowClass dockClass, string name, bool* open, ImGuiWindowFlags_ flags)
        {
            ImGuiAPI.SetNextWindowClass(dockClass);
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, StyleConfig.Instance.PanelFramePadding);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_WindowBg, StyleConfig.Instance.PanelBackground);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_ChildBg, UIProxy.StyleConfig.Instance.PanelBackground);
            var retValue = ImGuiAPI.Begin(name, open, flags);
            return retValue;
        }
        // 可以dock到任何地方
        public static bool Begin(string name, ref bool open, ImGuiWindowFlags_ flags)
        {
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, StyleConfig.Instance.PanelFramePadding);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_WindowBg, StyleConfig.Instance.PanelBackground);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_ChildBg, UIProxy.StyleConfig.Instance.PanelBackground);
            var retValue = ImGuiAPI.Begin(name, ref open, flags);
            return retValue;
        }
        public static unsafe bool Begin(string name, bool* open, ImGuiWindowFlags_ flags)
        {
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, StyleConfig.Instance.PanelFramePadding);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_WindowBg, StyleConfig.Instance.PanelBackground);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_ChildBg, UIProxy.StyleConfig.Instance.PanelBackground);
            var retValue = ImGuiAPI.Begin(name, open, flags);
            return retValue;
        }
        public static void End()
        {
            ImGuiAPI.PopStyleVar(1);
            ImGuiAPI.PopStyleColor(2);
            ImGuiAPI.End();
        }
    }
}
