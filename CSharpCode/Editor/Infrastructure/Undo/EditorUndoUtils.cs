using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Editor.Infrastructure
{
    /// <summary>
    /// 统一Undo/Redo的编辑器接线工具: 工具栏按钮绘制与快捷键处理, 供各编辑器一行调用。
    /// </summary>
    public static class EditorUndoUtils
    {
        /// <summary>
        /// 绘制工具栏Undo/Redo按钮(自带前置SameLine由调用方控制, 两按钮之间自动SameLine)
        /// </summary>
        public static void DrawUndoRedoButtons(TtEditorHistory history)
        {
            var btSize = Vector2.Zero;
            if (EGui.UIProxy.CustomButton.ToolButton("Undo", in btSize))
            {
                history?.Undo();
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Redo", in btSize))
            {
                history?.Redo();
            }
        }
        /// <summary>
        /// 处理Ctrl+Z/Ctrl+Y/Ctrl+Shift+Z快捷键。
        /// 必须在编辑器主窗口Begin/End作用域内每帧调用(如DrawToolBar中), 依赖窗口焦点判定分发到当前编辑器
        /// </summary>
        public static void HandleUndoShortcut(TtEditorHistory history)
        {
            if (history == null)
                return;
            if (ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_RootAndChildWindows) == false)
                return;
            // 文本框等控件激活时把Ctrl+Z留给控件自身
            if (ImGuiAPI.IsAnyItemActive())
                return;
            var input = TtEngine.Instance.InputSystem;
            if (input.IsCtrlKeyDown() == false)
                return;
            if (input.IsKeyPressed(Bricks.Input.Keycode.KEY_z))
            {
                if (input.IsShiftKeyDown())
                    history.Redo();
                else
                    history.Undo();
            }
            else if (input.IsKeyPressed(Bricks.Input.Keycode.KEY_y))
            {
                history.Redo();
            }
        }
    }
}
