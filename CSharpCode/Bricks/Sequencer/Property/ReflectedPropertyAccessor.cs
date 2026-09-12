using System;
using System.Collections.Generic;

namespace EngineNS.Sequencer
{
    /// <summary>
    /// 按属性名读写目标对象上的 [Rtti.Meta] 属性, 让通用属性轨不必为每条属性手写一个访问器。
    ///
    /// ISequencePropertyAccessor 上"这里不用反射"那段注释针对的是老的 TtPropertySetterModule
    /// —— 它把所有 public 属性自动暴露成可 K, 里面大量属性写进去要么无效要么直接崩。这个
    /// 类不推翻那个结论: 一条属性要能被序列驱动仍然要过四道关 ——
    ///   1. 有 [Rtti.Meta] 标注 (即引擎认为它是可序列化的数据, 不是临时状态)
    ///   2. 有公开的 get 和 set, 且不是索引器
    ///   3. 值类型有对应的 ISequenceValueAdapter
    ///   4. 用户在编辑器里显式给它建了一条轨道
    /// 反射在这里只省掉"每个属性抄一遍访问器类"的重复代码, 白名单的性质没变, 判据见 CanAnimate。
    ///
    /// 目标类型不写进 PropertyId (只写属性名), 与 UE 的 FMovieScenePropertyBinding 一致:
    /// 一条序列可能绑到不同类型的节点上, 把类型名写进 Id 会让换个派生类就断。代价是不同
    /// 类型上的同名属性会被当成同一条属性, 这在实践中正是想要的行为。
    /// </summary>
    public class TtReflectedPropertyAccessor : ISequencePropertyAccessor
    {
        /// <summary>PropertyId 的前缀, 用来跟显式注册的访问器 (如 "Node.Position") 区分开</summary>
        public const string IdPrefix = "Reflect:";

        /// <summary>
        /// 每个目标类型解析一次 PropertyInfo。同一条轨道每帧都要读写, 每帧走一次
        /// GetProperty 太贵; 解析失败 (null) 也缓存, 免得每帧重试。
        /// </summary>
        Dictionary<Type, System.Reflection.PropertyInfo> mCache = new Dictionary<Type, System.Reflection.PropertyInfo>();
        Rtti.TtTypeDesc mValueType = null;

        /// <summary>属性名。目前只支持单级, 不支持 "A.B" 下钻 —— 见 ResolveProperty。</summary>
        public string PropertyPath { get; private set; }
        public string PropertyId { get; private set; }
        public string DisplayName { get; private set; }
        /// <summary>
        /// 第一次成功解析到属性之后才有值, 在那之前是 null。
        ///
        /// 反射访问器在拿到目标对象之前不可能知道值类型, 所以它不能像显式访问器那样把
        /// 类型写死。求值路径不依赖这个属性 —— TtPropertySection 用自己存的 AdapterId
        /// 找适配器, 就是为了在目标还没解析出来时也能把轨道画出来。
        /// </summary>
        public Rtti.TtTypeDesc ValueType { get => mValueType; }

        public TtReflectedPropertyAccessor(string propertyPath, string displayName = null)
        {
            PropertyPath = propertyPath != null ? propertyPath : "";
            PropertyId = IdPrefix + PropertyPath;
            DisplayName = string.IsNullOrEmpty(displayName) ? PropertyPath : displayName;
        }
        /// <summary>从 "Reflect:Xxx" 形式的 PropertyId 反建一个访问器, 前缀不对时返回 null</summary>
        public static TtReflectedPropertyAccessor CreateFromId(string propertyId)
        {
            if (string.IsNullOrEmpty(propertyId) || propertyId.StartsWith(IdPrefix) == false)
                return null;
            var path = propertyId.Substring(IdPrefix.Length);
            if (string.IsNullOrEmpty(path))
                return null;
            return new TtReflectedPropertyAccessor(path);
        }
        /// <summary>
        /// 一条属性能不能被序列驱动。编辑器列可选属性时也要用这个判据, 保证"列出来的都能用"
        /// —— 列出一条点了没反应的属性比不列出来更糟。
        /// </summary>
        public static bool CanAnimate(System.Reflection.PropertyInfo prop)
        {
            if (prop == null || prop.CanRead == false || prop.CanWrite == false)
                return false;
            if (prop.GetIndexParameters().Length > 0)
                return false;
            if (prop.GetSetMethod(false) == null || prop.GetGetMethod(false) == null)
                return false;
            if (Attribute.IsDefined(prop, typeof(Rtti.MetaAttribute)) == false)
                return false;
            return TtSequenceValueAdapters.FindByValueType(prop.PropertyType) != null;
        }
        /// <summary>
        /// 列出一个类型上所有能被驱动的属性, 供编辑器做"加属性轨道"的候选列表。
        /// 含继承来的属性 —— 用户看到的是节点的完整属性面板, 不该在这里少一半。
        /// </summary>
        public static void GatherAnimatableProperties(Type targetType, List<System.Reflection.PropertyInfo> result)
        {
            if (targetType == null || result == null)
                return;
            var props = targetType.GetProperties(System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.FlattenHierarchy);
            for (int i = 0; i < props.Length; ++i)
            {
                if (CanAnimate(props[i]))
                    result.Add(props[i]);
            }
        }

        /// <summary>
        /// 解析 target 类型上的属性。
        /// 只查单级属性名: 多级下钻 ("Placement.Position") 需要把中间那层读出来、改完再写回去,
        /// 中间层是值类型还是引用类型决定了写不写得回, 这个复杂度等真有需求再加。
        /// </summary>
        System.Reflection.PropertyInfo ResolveProperty(object target)
        {
            if (target == null || string.IsNullOrEmpty(PropertyPath))
                return null;
            var type = target.GetType();
            System.Reflection.PropertyInfo prop;
            if (mCache.TryGetValue(type, out prop))
                return prop;

            prop = type.GetProperty(PropertyPath, System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.FlattenHierarchy);
            if (prop != null && CanAnimate(prop) == false)
                prop = null;
            mCache.Add(type, prop);
            if (prop != null && mValueType == null)
                mValueType = Rtti.TtTypeDesc.TypeOf(prop.PropertyType);
            return prop;
        }
        public object Read(object target)
        {
            var prop = ResolveProperty(target);
            if (prop == null)
                return null;
            return prop.GetValue(target);
        }
        public void Write(object target, object value)
        {
            var prop = ResolveProperty(target);
            if (prop == null)
                return;
            // 类型不匹配静默跳过 (接口的约定)。反射的 SetValue 类型不对会抛, 而序列驱动
            // 是每帧都跑的, 抛在这里等于一帧一次异常。
            if (value != null && prop.PropertyType.IsInstanceOfType(value) == false)
                return;
            prop.SetValue(target, value);
        }
    }
}
