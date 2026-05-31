using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Policy;
using System.Text;

namespace EngineNS.EGui.UIProxy
{
    public class DockProxy
    {
        public static ImGuiWindowClass MainFormDockClass = new ImGuiWindowClass()
        {
            m_ClassId = ImGuiAPI.GetID("MainEditorApplication"),
        };
        static uint GetMainFormDockId(IRootForm form, uint dockId)
        {
            if (dockId != 0 && dockId != uint.MaxValue)
                return dockId;
            if (form.DockId != 0 && form.DockId != uint.MaxValue)
                return form.DockId;
            return MainFormDockClass.m_ClassId;
        }

        // 只能dock到Main window中
        public static bool BeginMainForm(string name, IRootForm form, ImGuiWindowFlags_ flags, uint dockId = 0, bool saveLayout = true)
        {
            ImGuiAPI.SetNextWindowClass(MainFormDockClass);
            var targetDockId = GetMainFormDockId(form, dockId);
            ImGuiAPI.SetNextWindowDockID(targetDockId, form.DockCond);
            if (ImGuiAPI.IsLastFrame(name))
            {
                ImGuiAPI.SetNextWindowFocus();
            }
            if (EngineNS.TtEngine.Instance.IsBlockOperation)
                flags |= ImGuiWindowFlags_.ImGuiWindowFlags_NoInputs;
            if (!saveLayout)
                flags |= ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings;
            var vis = form.Visible;
            //var mainFlag = ImGuiWindowFlags_.ImGuiWindowFlags_None | ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar;
            //if ((flags & ImGuiWindowFlags_.ImGuiWindowFlags_NoTitleBar) == ImGuiWindowFlags_.ImGuiWindowFlags_NoTitleBar)
            //    mainFlag |= ImGuiWindowFlags_.ImGuiWindowFlags_NoTitleBar;
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_WindowPadding, Vector2.Zero);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_WindowBg, StyleConfig.Instance.WindowBackground);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_ChildBg, StyleConfig.Instance.PanelBackground);
            var retValue = ImGuiAPI.Begin(name, ref vis, flags);
            ImGuiAPI.PopStyleColor(2);
            ImGuiAPI.PopStyleVar(1);
            form.Visible = vis;
            //var presentWin = ImGuiAPI.GetWindowViewportData();
            //if (presentWin != null)
            //{
            //    form.Visible = !presentWin.IsClosed;
            //}
            if (ImGuiAPI.IsWindowDocked())
                form.DockId = ImGuiAPI.GetWindowDockID();
            DrawAssetEditorChrome(form, retValue);
            //if (retValue)
            //{
            //    ImGuiAPI.BeginChild("###name", Vector2.Zero, false, flags | ImGuiWindowFlags_.ImGuiWindowFlags_NoMove);
            //}
            return retValue;
        }
        public static bool BeginMainForm(string name, ref bool open, ref uint dockId, ImGuiWindowFlags_ flags, bool saveLayout = true)
        {
            ImGuiAPI.SetNextWindowClass(MainFormDockClass);
            ImGuiAPI.SetNextWindowDockID(MainFormDockClass.m_ClassId, ImGuiCond_.ImGuiCond_FirstUseEver);
            if (EngineNS.TtEngine.Instance.IsBlockOperation)
                flags |= ImGuiWindowFlags_.ImGuiWindowFlags_NoInputs;
            if (!saveLayout)
                flags |= ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings;
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
        public static unsafe bool BeginMainForm(string name, bool* open, ref uint dockId, ImGuiWindowFlags_ flags, bool saveLayout = true)
        {
            ImGuiAPI.SetNextWindowClass(MainFormDockClass);
            ImGuiAPI.SetNextWindowDockID(MainFormDockClass.m_ClassId, ImGuiCond_.ImGuiCond_FirstUseEver);
            if (EngineNS.TtEngine.Instance.IsBlockOperation)
                flags |= ImGuiWindowFlags_.ImGuiWindowFlags_NoInputs;
            if (!saveLayout)
                flags |= ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings;
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
        static Vector2 offset = new Vector2(10, 50);
        static void DrawAssetEditorChrome(IRootForm form, bool visible)
        {
            if (!visible)
                return;
            var editor = form as Editor.IAssetEditor;
            if (editor == null)
                return;

            var pos = ImGuiAPI.GetWindowPos();
            var size = ImGuiAPI.GetWindowSize();
            var end = pos + size;
            var focused = ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_RootAndChildWindows | ImGuiFocusedFlags_.ImGuiFocusedFlags_DockHierarchy);
            var borderColor = focused ? StyleConfig.Instance.AssetEditorActiveBorder : StyleConfig.Instance.AssetEditorInactiveBorder;
            var drawList = ImGuiAPI.GetWindowDrawList();
            drawList.AddRect(in pos, in end, borderColor, 0.0f, ImDrawFlags_.ImDrawFlags_None, focused ? 1.5f : 1.0f);

            var percent = editor.LoadingPercent;
            if (percent < 0.0f)
                percent = 0.0f;
            else if (percent > 1.0f)
                percent = 1.0f;
            if (percent < 0.999f)
            {
                var barMin = pos + new Vector2(1.0f, 1.0f);
                var barMax = new Vector2(end.X - 1.0f, pos.Y + 4.0f);
                var fillMax = new Vector2(barMin.X + (barMax.X - barMin.X) * percent, barMax.Y);
                drawList.AddRectFilled(in barMin, in barMax, StyleConfig.Instance.AssetEditorProgressBg, 0.0f, ImDrawFlags_.ImDrawFlags_None);
                drawList.AddRectFilled(in barMin, in fillMax, StyleConfig.Instance.AssetEditorProgressFill, 0.0f, ImDrawFlags_.ImDrawFlags_None);
            }
        }
        public static void EndMainForm(bool visible)
        {
            if (EngineNS.TtEngine.Instance.IsBlockOperation)
            {
                var pos = ImGuiAPI.GetWindowPos();
                var size = ImGuiAPI.GetWindowSize();

                ImGuiAPI.SetCursorPos(pos);
                ImGuiAPI.InvisibleButton("##Convert" + pos.ToString(), size, ImGuiButtonFlags_.ImGuiButtonFlags_None);
                var drawList = ImGuiAPI.GetForegroundDrawList();
                drawList.AddRectFilled(in pos, pos + size, 0x7f000000, 0.0f, ImDrawFlags_.ImDrawFlags_None);
                drawList.AddText(pos + offset, 0xffffffff, TtEngine.Instance.StopOperationInfo, null);
                TtEngine.Instance.StopOperationDrawAction?.Invoke(drawList, pos, size);
            }

            //if(visible)
            //    ImGuiAPI.EndChild();
            ImGuiAPI.End();
        }
        // 只能dock到指定window中
        public static bool BeginPanel(in ImGuiWindowClass dockClass, string name, ref bool open, ImGuiWindowFlags_ flags)
        {
            name = GetDockWindowName(name, dockClass);
            if(Editor.TtMainEditorApplication.NeedFocusPanelName == name)
            {
                ImGuiAPI.SetNextWindowFocus();
                Editor.TtMainEditorApplication.NeedFocusPanelName = null;
            }
            ImGuiAPI.SetNextWindowClass(dockClass);
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, StyleConfig.Instance.PanelFramePadding);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_WindowBg, StyleConfig.Instance.PanelBackground);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_ChildBg, UIProxy.StyleConfig.Instance.PanelBackground);
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_WindowPadding, Vector2.Zero);
            var retValue = ImGuiAPI.Begin(name, ref open, flags);
            ImGuiAPI.PopStyleVar(1);
            if(retValue)
                ImGuiAPI.BeginChild("###" + name, Vector2.Zero, ImGuiChildFlags_.ImGuiChildFlags_None, flags | ImGuiWindowFlags_.ImGuiWindowFlags_NoMove);
            return retValue;
        }
        public static unsafe bool BeginPanel(in ImGuiWindowClass dockClass, string name, bool* open, ImGuiWindowFlags_ flags)
        {
            name = GetDockWindowName(name, dockClass);
            if (Editor.TtMainEditorApplication.NeedFocusPanelName == name)
            {
                ImGuiAPI.SetNextWindowFocus();
                Editor.TtMainEditorApplication.NeedFocusPanelName = null;
            }
            ImGuiAPI.SetNextWindowClass(dockClass);
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, StyleConfig.Instance.PanelFramePadding);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_WindowBg, StyleConfig.Instance.PanelBackground);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_ChildBg, UIProxy.StyleConfig.Instance.PanelBackground);
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_WindowPadding, Vector2.Zero);
            var retValue = ImGuiAPI.Begin(name, open, flags);
            ImGuiAPI.PopStyleVar(1);
            if(retValue)
                ImGuiAPI.BeginChild("###" + name, Vector2.Zero, ImGuiChildFlags_.ImGuiChildFlags_None, flags | ImGuiWindowFlags_.ImGuiWindowFlags_NoMove);
            return retValue;
        }
        // 可以dock到任何地方
        public static bool BeginPanel(string name, ref bool open, ImGuiWindowFlags_ flags)
        {
            ImGuiAPI.SetNextWindowClass(MainFormDockClass);
            if (Editor.TtMainEditorApplication.NeedFocusPanelName == name)
            {
                ImGuiAPI.SetNextWindowFocus();
                Editor.TtMainEditorApplication.NeedFocusPanelName = null;
            }
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, StyleConfig.Instance.PanelFramePadding);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_WindowBg, StyleConfig.Instance.PanelBackground);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_ChildBg, UIProxy.StyleConfig.Instance.PanelBackground);
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_WindowPadding, Vector2.Zero);
            var retValue = ImGuiAPI.Begin(name, ref open, flags);
            ImGuiAPI.PopStyleVar(1);
            if(retValue)
                ImGuiAPI.BeginChild("###" + name, Vector2.Zero, ImGuiChildFlags_.ImGuiChildFlags_None, flags | ImGuiWindowFlags_.ImGuiWindowFlags_NoMove);
            return retValue;
        }
        public static unsafe bool BeginPanel(string name, bool* open, ImGuiWindowFlags_ flags)
        {
            ImGuiAPI.SetNextWindowClass(MainFormDockClass);
            if (Editor.TtMainEditorApplication.NeedFocusPanelName == name)
            {
                ImGuiAPI.SetNextWindowFocus();
                Editor.TtMainEditorApplication.NeedFocusPanelName = null;
            }
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, StyleConfig.Instance.PanelFramePadding);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_WindowBg, StyleConfig.Instance.PanelBackground);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_ChildBg, UIProxy.StyleConfig.Instance.PanelBackground);
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_WindowPadding, Vector2.Zero);
            var retValue = ImGuiAPI.Begin(name, open, flags);
            ImGuiAPI.PopStyleVar(1);
            if(retValue)
                ImGuiAPI.BeginChild("###" + name, Vector2.Zero, ImGuiChildFlags_.ImGuiChildFlags_None, flags | ImGuiWindowFlags_.ImGuiWindowFlags_NoMove);
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
