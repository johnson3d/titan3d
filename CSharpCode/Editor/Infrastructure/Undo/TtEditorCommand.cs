using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using EngineNS.EGui.Controls.PropertyGrid;

namespace EngineNS.Editor.Infrastructure
{
    /// <summary>
    /// 统一Undo/Redo架构的命令基类。每个可撤销的编辑操作封装为一条命令, 由 TtEditorHistory 管理。
    /// </summary>
    public abstract class TtEditorCommand
    {
        public string Name { get; set; }
        /// <summary>
        /// 已封口的命令不再参与合并(拖拽/滑条结束后由PropertyGrid或编辑器封口)
        /// </summary>
        public bool Sealed { get; private set; } = false;
        public void Seal()
        {
            Sealed = true;
        }
        public abstract void Do();
        public abstract void Undo();
        /// <summary>
        /// 尝试把后续命令合并进当前命令(用于拖拽/滑条产生的连续修改), 成功返回true且other被丢弃
        /// </summary>
        public virtual bool TryMerge(TtEditorCommand other)
        {
            return false;
        }
    }

    /// <summary>
    /// 用do/undo委托快速包装特殊操作的通用命令
    /// </summary>
    public class TtDelegateCommand : TtEditorCommand
    {
        readonly Action mDoAction;
        readonly Action mUndoAction;
        public TtDelegateCommand(string name, Action doAction, Action undoAction)
        {
            Name = name;
            mDoAction = doAction;
            mUndoAction = undoAction;
        }
        public override void Do()
        {
            mDoAction?.Invoke();
        }
        public override void Undo()
        {
            mUndoAction?.Invoke();
        }
    }

    /// <summary>
    /// 复合命令: BeginTransaction/EndTransaction期间收集的子命令合并为一条历史记录(gizmo拖动、多选修改等)
    /// </summary>
    public class TtTransactionCommand : TtEditorCommand
    {
        public List<TtEditorCommand> Commands { get; } = new List<TtEditorCommand>();
        public TtTransactionCommand(string name)
        {
            Name = name;
        }
        public override void Do()
        {
            for (int i = 0; i < Commands.Count; i++)
            {
                Commands[i].Do();
            }
        }
        public override void Undo()
        {
            for (int i = Commands.Count - 1; i >= 0; i--)
            {
                Commands[i].Undo();
            }
        }
    }

    /// <summary>
    /// 属性修改命令: 记录宿主对象+成员名+新旧值, 通过反射写回。
    /// 由PropertyGrid写入漏斗自动创建(见 TtPropertyGrid.SetValueWithHistory), 也可供编辑器手工创建。
    /// 宿主必须是引用类型(值类型属性的修改由PGRenderer级联写回到最外层引用类型宿主时记录)。
    /// </summary>
    public class TtPropertyChangeCommand : TtEditorCommand
    {
        public List<object> Hosts { get; } = new List<object>();
        public List<object> OldValues { get; } = new List<object>();
        public string MemberName;
        public object NewValue;
        public PGProvider Provider;

        /// <summary>
        /// 在propDesc.SetValue写入之前调用, 捕获旧值。不可记录的场景(多值写入/根为值类型/值未变化)返回null
        /// </summary>
        public static TtPropertyChangeCommand TryCreate(CustomPropertyDescriptor propDesc, object target, object newValue)
        {
            if (target == null || propDesc == null)
                return null;
            // 值类型宿主的写入由PGRenderer级联到外层引用类型宿主时再记录, 避免记录改不回去的盒
            if (propDesc.ParentIsValueType)
                return null;
            // 多值(多选不同值)写入暂不记录
            if (newValue is PropertyMultiValue)
                return null;

            var cmd = new TtPropertyChangeCommand();
            cmd.MemberName = propDesc.Name;
            cmd.Name = $"Set {propDesc.Name}";
            cmd.Provider = propDesc.CustomValueEditor?.Provider;
            if (target is IEnumerable enumerable && !(target is string))
            {
                // 多选: PropertyDescCollection.SetValue会把同一个值写到每个元素
                foreach (var elem in enumerable)
                {
                    if (elem == null)
                        continue;
                    cmd.Hosts.Add(elem);
                    cmd.OldValues.Add(GetMember(elem, cmd.MemberName, cmd.Provider));
                }
            }
            else
            {
                cmd.Hosts.Add(target);
                cmd.OldValues.Add(GetMember(target, cmd.MemberName, cmd.Provider));
            }
            if (cmd.Hosts.Count == 0)
                return null;
            cmd.NewValue = newValue;
            if (cmd.Hosts.Count == 1 && object.Equals(cmd.OldValues[0], newValue))
                return null;
            return cmd;
        }

        public override void Do()
        {
            for (int i = 0; i < Hosts.Count; i++)
            {
                SetMember(Hosts[i], MemberName, NewValue, Provider);
            }
        }
        public override void Undo()
        {
            for (int i = 0; i < Hosts.Count; i++)
            {
                SetMember(Hosts[i], MemberName, OldValues[i], Provider);
            }
        }
        public override bool TryMerge(TtEditorCommand other)
        {
            if (Sealed)
                return false;
            var pc = other as TtPropertyChangeCommand;
            if (pc == null || pc.MemberName != MemberName || pc.Hosts.Count != Hosts.Count)
                return false;
            for (int i = 0; i < Hosts.Count; i++)
            {
                if (object.ReferenceEquals(Hosts[i], pc.Hosts[i]) == false)
                    return false;
            }
            NewValue = pc.NewValue;
            return true;
        }

        // 与 CustomPropertyDescriptor.GetValue/SetValue 的单对象语义保持一致(IPropertyCustomization/Provider/属性/字段)
        internal static object GetMember(object owner, string name, PGProvider provider)
        {
            if (owner == null)
                return null;
            var custom = owner as IPropertyCustomization;
            if (custom != null)
            {
                return custom.GetPropertyValue(name);
            }
            var type = owner.GetType();
            var propertyInfo = type.GetProperty(name);
            if (propertyInfo != null)
            {
                var value = propertyInfo.GetValue(owner);
                return provider != null ? provider.GetValue(value) : value;
            }
            var fieldInfo = type.GetField(name);
            if (fieldInfo != null)
            {
                var value = fieldInfo.GetValue(owner);
                return provider != null ? provider.GetValue(value) : value;
            }
            return null;
        }
        internal static void SetMember(object owner, string name, object value, PGProvider provider)
        {
            if (owner == null)
                return;
            var custom = owner as IPropertyCustomization;
            if (custom != null)
            {
                custom.SetPropertyValue(name, value);
                return;
            }
            var preChecker = owner as IPropertySetPreChecker;
            if (preChecker != null && preChecker.CanSetPropertyValue(name, value) == false)
                return;
            var type = owner.GetType();
            var propertyInfo = type.GetProperty(name);
            if (propertyInfo != null)
            {
                if (provider != null)
                {
                    var memberIns = propertyInfo.GetValue(owner);
                    provider.SetValue(memberIns, Support.TConvert.ToObject(propertyInfo.PropertyType, value));
                }
                else if (propertyInfo.CanWrite)
                {
                    propertyInfo.SetValue(owner, Support.TConvert.ToObject(propertyInfo.PropertyType, value));
                }
            }
            var fieldInfo = type.GetField(name);
            if (fieldInfo != null)
            {
                if (provider != null)
                {
                    var memberIns = fieldInfo.GetValue(owner);
                    provider.SetValue(memberIns, Support.TConvert.ToObject(fieldInfo.FieldType, value));
                }
                else
                {
                    fieldInfo.SetValue(owner, Support.TConvert.ToObject(fieldInfo.FieldType, value));
                }
            }
        }
    }
}
