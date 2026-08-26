using System;
using System.Reflection;

namespace EngineNS.EGui.Controls.PropertyGrid
{
    /// <summary>
    /// PropertyGrid 自定义编辑器: 把一个属性显示成一个按钮, 点击时调用宿主对象上的一个无参方法。
    ///
    /// 用途: 让"执行某个动作"这件事也能放进属性面板里, 而不必在 PropertyGrid 外面另画控件 ——
    /// TtPropertyGrid.OnDraw 内部用 Vector2.MinusOne 开 child(即占满剩余空间), 在它之后画的
    /// 控件会被挤出可见区, 所以动作类 UI 放进 PG 内部才是可靠做法。
    ///
    /// 用法:
    ///   [TtPGButtonEditor(ButtonText = "Do Something", MethodName = nameof(HostType.DoSomething))]
    ///   public int SomeAction { get; set; }     // 属性本身的值不使用, 仅作为承载位
    ///   public void DoSomething() { ... }        // 宿主对象上的公开无参方法
    ///
    /// 说明: MethodName 用字符串而非委托, 因为 attribute 的具名参数必须是编译期常量。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class TtPGButtonEditorAttribute : TtPGCustomValueEditorAttribute
    {
        /// <summary> 按钮上显示的文字。 </summary>
        public string ButtonText = "Execute";

        /// <summary> 要调用的宿主对象公开无参方法名。 </summary>
        public string MethodName = null;

        /// <summary> 悬浮提示(可空)。 </summary>
        public string Tooltip = null;

        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
        {
            newValue = info.Value;

            if (info.Readonly)
            {
                ImGuiAPI.Text(ButtonText);
                return false;
            }

            var btSize = Vector2.Zero;
            if (EGui.UIProxy.CustomButton.ToolButton(ButtonText, in btSize))
            {
                InvokeOnHost(info.FirstObjectInstance);
            }
            if (!string.IsNullOrEmpty(Tooltip) && ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                EGui.Controls.CtrlUtility.DrawHelper(Tooltip);

            // 按钮不改属性值, 因此不报告变更(避免污染脏标记/Undo)
            return false;
        }

        void InvokeOnHost(object host)
        {
            if (host == null || string.IsNullOrEmpty(MethodName))
                return;

            // 宿主类型可能是 private 嵌套类型, 所以要允许非公开查找
            var method = host.GetType().GetMethod(MethodName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                null, Type.EmptyTypes, null);
            if (method == null)
            {
                Profiler.Log.WriteLine<Profiler.TtEditorGategory>(Profiler.ELogTag.Warning, "PropertyGrid",
                    $"TtPGButtonEditor: 在 {host.GetType().Name} 上找不到无参方法 {MethodName}");
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
