using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace EngineNS.EGui.UIProxy
{
    public class DockProxy
    {
        public static ImGuiWindowClass MainFormDockClass = new ImGuiWindowClass()
        {
            m_ClassId = ImGuiAPI.GetID("MainEditorApplication"),
        };
        // 只能dock到Main window中
        public static bool BeginMainForm(string name, IRootForm form, ImGuiWindowFlags_ flags)
        {
            ImGuiAPI.SetNextWindowClass(MainFormDockClass);
            ImGuiAPI.SetNextWindowDockID(MainFormDockClass.m_ClassId, ImGuiCond_.ImGuiCond_FirstUseEver);
            var vis = form.Visible;
            //var mainFlag = ImGuiWindowFlags_.ImGuiWindowFlags_None | ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar;
            //if ((flags & ImGuiWindowFlags_.ImGuiWindowFlags_NoTitleBar) == ImGuiWindowFlags_.ImGuiWindowFlags_NoTitleBar)
            //    mainFlag |= ImGuiWindowFlags_.ImGuiWindowFlags_NoTitleBar;
                ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_WindowPadding, Vector2.Zero);
            var retValue = ImGuiAPI.Begin(name, ref vis, flags);
                ImGuiAPI.PopStyleVar(1);
            form.Visible = vis;
            //var presentWin = ImGuiAPI.GetWindowViewportData();
            //if (presentWin != null)
            //{
            //    form.Visible = !presentWin.IsClosed;
            //}
            if (ImGuiAPI.IsWindowDocked())
                form.DockId = ImGuiAPI.GetWindowDockID();
            //if (retValue)
            //{
            //    ImGuiAPI.BeginChild("###name", Vector2.Zero, false, flags | ImGuiWindowFlags_.ImGuiWindowFlags_NoMove);
            //}
            return retValue;
        }
        public static bool BeginMainForm(string name, ref bool open, ref uint dockId, ImGuiWindowFlags_ flags)
        {
            ImGuiAPI.SetNextWindowClass(MainFormDockClass);
            ImGuiAPI.SetNextWindowDockID(MainFormDockClass.m_ClassId, ImGuiCond_.ImGuiCond_FirstUseEver);
            //var mainFlag = ImGuiWindowFlags_.ImGuiWindowFlags_None | ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar;
            //if ((flags & ImGuiWindowFlags_.ImGuiWindowFlags_NoTitleBar) == ImGuiWindowFlags_.ImGuiWindowFlags_NoTitleBar)
            //    mainFlag |= ImGuiWindowFlags_.ImGuiWindowFlags_NoTitleBar;
                ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_WindowPadding, Vector2.Zero);
            var retValue = ImGuiAPI.Begin(name, ref open, flags);
                ImGuiAPI.PopStyleVar(1);
            //var presentWin = ImGuiAPI.GetWindowViewportData();
            //if (presentWin != null)
            //{
            //    open = !presentWin.IsClosed;
            //}
            if (ImGuiAPI.IsWindowDocked())
                dockId = ImGuiAPI.GetWindowDockID();
            //if (retValue)
            //{
            //    ImGuiAPI.BeginChild("###name", Vector2.Zero, false, flags | ImGuiWindowFlags_.ImGuiWindowFlags_NoMove);
            //}
            return retValue;
        }
        public static unsafe bool BeginMainForm(string name, bool* open, ref uint dockId, ImGuiWindowFlags_ flags)
        {
            ImGuiAPI.SetNextWindowClass(MainFormDockClass);
            ImGuiAPI.SetNextWindowDockID(MainFormDockClass.m_ClassId, ImGuiCond_.ImGuiCond_FirstUseEver);
            //var mainFlag = ImGuiWindowFlags_.ImGuiWindowFlags_None | ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar;
            //if ((flags & ImGuiWindowFlags_.ImGuiWindowFlags_NoTitleBar) == ImGuiWindowFlags_.ImGuiWindowFlags_NoTitleBar)
            //    mainFlag |= ImGuiWindowFlags_.ImGuiWindowFlags_NoTitleBar;
                ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_WindowPadding, Vector2.Zero);
            var retValue = ImGuiAPI.Begin(name, open, flags);
                ImGuiAPI.PopStyleVar(1);
            //var presentWin = ImGuiAPI.GetWindowViewportData();
            //if (presentWin != null && open != null)
            //{
            //    *open = !presentWin.IsClosed;
            //}
            if (ImGuiAPI.IsWindowDocked())
                dockId = ImGuiAPI.GetWindowDockID();
            //if (retValue)
            //{
            //    ImGuiAPI.BeginChild("###name", Vector2.Zero, false, flags | ImGuiWindowFlags_.ImGuiWindowFlags_NoMove);
            //}
            return retValue;
        }
        public static void EndMainForm(bool visible)
        {
            //if(visible)
            //    ImGuiAPI.EndChild();
            ImGuiAPI.End();
        }
        // 只能dock到指定window中
        public static bool BeginPanel(in ImGuiWindowClass dockClass, string name, ref bool open, ImGuiWindowFlags_ flags)
        {
            name = GetDockWindowName(name, dockClass);
            ImGuiAPI.SetNextWindowClass(dockClass);
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, StyleConfig.Instance.PanelFramePadding);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_WindowBg, StyleConfig.Instance.PanelBackground);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_ChildBg, UIProxy.StyleConfig.Instance.PanelBackground);
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_WindowPadding, Vector2.Zero);
            var retValue = ImGuiAPI.Begin(name, ref open, flags);
            ImGuiAPI.PopStyleVar(1);
            if(retValue)
                ImGuiAPI.BeginChild("###" + name, Vector2.Zero, false, flags | ImGuiWindowFlags_.ImGuiWindowFlags_NoMove);
            return retValue;
        }
        public static unsafe bool BeginPanel(in ImGuiWindowClass dockClass, string name, bool* open, ImGuiWindowFlags_ flags)
        {
            name = GetDockWindowName(name, dockClass);
            ImGuiAPI.SetNextWindowClass(dockClass);
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, StyleConfig.Instance.PanelFramePadding);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_WindowBg, StyleConfig.Instance.PanelBackground);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_ChildBg, UIProxy.StyleConfig.Instance.PanelBackground);
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_WindowPadding, Vector2.Zero);
            var retValue = ImGuiAPI.Begin(name, open, flags);
            ImGuiAPI.PopStyleVar(1);
            if(retValue)
                ImGuiAPI.BeginChild("###" + name, Vector2.Zero, false, flags | ImGuiWindowFlags_.ImGuiWindowFlags_NoMove);
            return retValue;
        }
        // 可以dock到任何地方
        public static bool BeginPanel(string name, ref bool open, ImGuiWindowFlags_ flags)
        {
            ImGuiAPI.SetNextWindowClass(MainFormDockClass);
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, StyleConfig.Instance.PanelFramePadding);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_WindowBg, StyleConfig.Instance.PanelBackground);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_ChildBg, UIProxy.StyleConfig.Instance.PanelBackground);
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_WindowPadding, Vector2.Zero);
            var retValue = ImGuiAPI.Begin(name, ref open, flags);
            ImGuiAPI.PopStyleVar(1);
            if(retValue)
                ImGuiAPI.BeginChild("###" + name, Vector2.Zero, false, flags | ImGuiWindowFlags_.ImGuiWindowFlags_NoMove);
            return retValue;
        }
        public static unsafe bool BeginPanel(string name, bool* open, ImGuiWindowFlags_ flags)
        {
            ImGuiAPI.SetNextWindowClass(MainFormDockClass);
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, StyleConfig.Instance.PanelFramePadding);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_WindowBg, StyleConfig.Instance.PanelBackground);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_ChildBg, UIProxy.StyleConfig.Instance.PanelBackground);
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_WindowPadding, Vector2.Zero);
            var retValue = ImGuiAPI.Begin(name, open, flags);
            ImGuiAPI.PopStyleVar(1);
            if(retValue)
                ImGuiAPI.BeginChild("###" + name, Vector2.Zero, false, flags | ImGuiWindowFlags_.ImGuiWindowFlags_NoMove);
            return retValue;
        }
        public static void EndPanel(bool show)
        {
            if(show)
                ImGuiAPI.EndChild();
            ImGuiAPI.PopStyleVar(1);
            ImGuiAPI.PopStyleColor(2);
            ImGuiAPI.End();
        }

        public static string GetDockWindowName(string name, in ImGuiWindowClass dockKeyClass)
        {
            return name + "##" + dockKeyClass.m_ClassId;
        }
    }
}
