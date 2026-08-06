using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Editor.Infrastructure
{
    /// <summary>
    /// 编辑器操作栈历史面板: 显示当前编辑器的命令列表与游标位置, 点击条目多步跳转。
    /// 由各编辑器OnDraw中作为一个dock窗口绘制。
    /// </summary>
    public class TtEditorHistoryPanel
    {
        public bool Visible = true;
        const uint UndoneTextColor = 0xFF808080;

        public void OnDraw(in ImGuiWindowClass dockKeyClass, string panelName, TtEditorHistory history)
        {
            var show = EGui.UIProxy.DockProxy.BeginPanel(in dockKeyClass, panelName, ref Visible, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                DrawHistoryList(history);
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        public void DrawHistoryList(TtEditorHistory history)
        {
            if (history == null)
            {
                ImGuiAPI.Text("History is disabled");
                return;
            }
            ImGuiAPI.Text($"Steps: {history.CurrentStep}/{history.Commands.Count}{(history.IsDirtyFromHistory ? " *" : "")}");
            ImGuiAPI.SameLine(0, -1);
            var btSize = Vector2.Zero;
            if (EGui.UIProxy.CustomButton.ToolButton("Clear", in btSize))
            {
                history.Clear();
            }
            ImGuiAPI.Separator();

            // 起始状态条目
            if (ImGuiAPI.Selectable("<Initial State>", history.CurrentStep == 0, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in Vector2.Zero))
            {
                history.JumpTo(0);
            }
            for (int i = 0; i < history.Commands.Count; i++)
            {
                var step = i + 1;
                var undone = step > history.CurrentStep;
                if (undone)
                    ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, UndoneTextColor);
                var name = history.Commands[i].Name;
                if (string.IsNullOrEmpty(name))
                    name = history.Commands[i].GetType().Name;
                if (ImGuiAPI.Selectable($"{step}: {name}##HistoryItem{i}", step == history.CurrentStep, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in Vector2.Zero))
                {
                    history.JumpTo(step);
                }
                if (undone)
                    ImGuiAPI.PopStyleColor(1);
            }
        }
    }
}
