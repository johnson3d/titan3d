using System;
using System.Reflection;

namespace EngineNS.EGui.Controls.PropertyGrid
{
    /// <summary>
    /// PropertyGrid 自定义编辑器: 把属性画成一个按钮, 点击弹出 popup 窗口, popup 内容由宿主
    /// 对象上的一个公开/非公开无参方法每帧绘制 (方法里直接写 ImGuiAPI 即可, 不要自己
    /// 调 BeginPopup/EndPopup, 由本特性包裹)。
    ///
    /// 与 TtPGButtonEditor 的分工: 后者点击就执行一次性动作; 本特性用于"需要跨帧交互"的
    /// 面板 (表格/网格/多步操作), 这些 UI 必须活在 popup 里, 不能塞进 PG 单行。
    ///
    /// 用法:
    ///   [TtPGPopupButtonEditor(ButtonText = "Levels", DrawMethodName = nameof(DrawLevelsGrid))]
    ///   public int LevelsAction { get; set; }     // 属性本身的值不使用, 仅作为承载位
    ///   void DrawLevelsGrid() { ... }             // 宿主对象上的无参方法, 在 popup 内绘制
    ///
    /// 说明: DrawMethodName 用字符串而非委托, 因为 attribute 的具名参数必须是编译期常量。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class TtPGPopupButtonEditorAttribute : TtPGCustomValueEditorAttribute
    {
        /// <summary> 按钮上显示的文字。 </summary>
        public string ButtonText = "Open";

        /// <summary> 在 popup 内绘制内容的宿主对象无参方法名。 </summary>
        public string DrawMethodName = null;

        /// <summary> 悬浮提示(可空)。 </summary>
        public string Tooltip = null;

        /// <summary>
        /// popup 窗口标题。留空时用属性名派生, 保证同一宿主上的多个按钮互不冲突。
        /// 标题会显示在 popup 的标题栏上, 可拖动。
        /// </summary>
        public string PopupTitle = null;

        public ImGuiWindowFlags_ PopupFlags = ImGuiWindowFlags_.ImGuiWindowFlags_AlwaysAutoResize;

        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
        {
            newValue = info.Value;

            if (info.Readonly)
            {
                ImGuiAPI.Text(ButtonText);
                return false;
            }

            var title = string.IsNullOrEmpty(PopupTitle)
                ? $"###TtPGPopup_{info.Name}"
                : PopupTitle;

            var btSize = Vector2.Zero;
            if (EGui.UIProxy.CustomButton.ToolButton(ButtonText, in btSize))
            {
                ImGuiAPI.OpenPopup(title, ImGuiPopupFlags_.ImGuiPopupFlags_None);
            }
            if (!string.IsNullOrEmpty(Tooltip) && ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                EGui.Controls.CtrlUtility.DrawHelper(Tooltip);

            // popup 内容在后续帧继续绘制: BeginPopup 返回 false 时不进内容,
            // 关闭由窗口自身的关闭按钮/Esc/点击外部处理。
            if (ImGuiAPI.BeginPopup(title, PopupFlags))
            {
                InvokeOnHost(info.FirstObjectInstance);
                ImGuiAPI.EndPopup();
            }

            // 按钮不改变属性值, 不报告变更(避免污染脏标记/Undo)
            return false;
        }

        void InvokeOnHost(object host)
        {
            if (host == null || string.IsNullOrEmpty(DrawMethodName))
                return;

            // 宿主类型可能是 private 嵌套类型, 所以要允许非公开查找
            var method = host.GetType().GetMethod(DrawMethodName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                null, Type.EmptyTypes, null);
            if (method == null)
            {
                Profiler.Log.WriteLine<Profiler.TtEditorGategory>(Profiler.ELogTag.Warning, "PropertyGrid",
                    $"TtPGPopupButtonEditor: 在 {host.GetType().Name} 上找不到无参方法 {DrawMethodName}");
                return;
            }

            try
            {
                method.Invoke(host, null);
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteException(ex);
            }
        }
    }
}
