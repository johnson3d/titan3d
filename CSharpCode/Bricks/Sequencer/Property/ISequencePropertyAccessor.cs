using System;
using System.Collections.Generic;

namespace EngineNS.Sequencer
{
    /// <summary>
    /// 序列器读写一个属性的唯一入口。
    ///
    /// 这里不用反射: 上一版的反射式属性 setter 注册表 (TtPropertySetterModule) 从未被
    /// 调用过就被删掉了。序列器要按名字找属性, 但"能按名字找"不等于"必须反射"——
    /// 显式访问器把可被动画的属性变成一份明确的白名单, 既避免反射开销, 也避免序列
    /// 能悄悄改到任何属性。
    ///
    /// PropertyId 是存进资产里的稳定字符串 (如 "Node.Position"), 改名等于让老资产
    /// 认不出这条轨道, 所以它一旦发布就不能改。
    /// </summary>
    public interface ISequencePropertyAccessor
    {
        string PropertyId { get; }
        /// <summary>界面上显示的名字</summary>
        string DisplayName { get; }
        /// <summary>Read/Write 走的值类型, 用于校验轨道与目标是否对得上</summary>
        Rtti.TtTypeDesc ValueType { get; }
        /// <summary>target 类型不匹配时返回 null</summary>
        object Read(object target);
        /// <summary>target 类型或 value 类型不匹配时静默跳过, 不抛</summary>
        void Write(object target, object value);
    }

    /// <summary>
    /// 显式访问器注册表。引擎内置的访问器在 TtSequencerModule.Initialize 里注册,
    /// 游戏侧可以自己往里加。
    /// </summary>
    public class TtSequencePropertyRegistry
    {
        Dictionary<string, ISequencePropertyAccessor> mAccessors = new Dictionary<string, ISequencePropertyAccessor>();

        public IReadOnlyDictionary<string, ISequencePropertyAccessor> Accessors { get => mAccessors; }

        /// <summary>
        /// 撞键时不抛异常, 只报 Warning 并保留先注册的那个: 注册发生在模块初始化期,
        /// 这里抛异常会整个引擎起不来, 而一条属性轨失效只是那条轨道不动。
        /// </summary>
        public bool Register(ISequencePropertyAccessor accessor)
        {
            if (accessor == null || string.IsNullOrEmpty(accessor.PropertyId))
                return false;
            if (mAccessors.ContainsKey(accessor.PropertyId))
            {
                Profiler.Log.WriteLine<TtSequencerCategory>(Profiler.ELogTag.Warning,
                    $"TtSequencePropertyRegistry.Register: PropertyId({accessor.PropertyId}) already registered, keep the first one");
                return false;
            }
            mAccessors.Add(accessor.PropertyId, accessor);
            return true;
        }
        /// <summary>
        /// 找不到返回 null。老资产引用了已删掉的访问器时会走到这里, 由调用方决定怎么提示。
        ///
        /// "Reflect:" 开头的 Id 是通用属性轨存的, 不可能预先注册 (属性名是编辑时才
        /// 定下来的), 第一次问到就建一个并置入 —— 反射访问器内部缓存了 PropertyInfo,
        /// 每帧新建一个等于缓存得不到。这条路径不是线程安全的, 和求值一样只跑主线程。
        /// </summary>
        public ISequencePropertyAccessor Find(string propertyId)
        {
            if (string.IsNullOrEmpty(propertyId))
                return null;
            ISequencePropertyAccessor result;
            if (mAccessors.TryGetValue(propertyId, out result))
                return result;

            var reflected = TtReflectedPropertyAccessor.CreateFromId(propertyId);
            if (reflected != null)
            {
                mAccessors.Add(propertyId, reflected);
                return reflected;
            }
            return null;
        }
    }
}
