using System;

namespace EngineNS.Sequencer
{
    /// <summary>
    /// 引擎内置的节点 Transform 访问器。三个属性 Id 是存进资产的稳定字符串, 不能改。
    ///
    /// 注意 TtNode.Placement 本身是只读属性 (返回 NodeData.Placement), 写入是改
    /// Placement 对象上的字段, 所以这里不需要能 set Placement。
    /// </summary>
    public static class TtNodeTransformAccessors
    {
        public const string PositionId = "Node.Position";
        public const string ScaleId = "Node.Scale";
        public const string QuatId = "Node.Quat";

        public class TtPositionAccessor : ISequencePropertyAccessor
        {
            public string PropertyId { get => PositionId; }
            public string DisplayName { get => "Position"; }
            public Rtti.TtTypeDesc ValueType { get => Rtti.TtTypeDesc.TypeOf(typeof(DVector3)); }
            public object Read(object target)
            {
                var node = target as GamePlay.Scene.TtNode;
                if (node == null || node.Placement == null)
                    return null;
                return node.Placement.Position;
            }
            public void Write(object target, object value)
            {
                var node = target as GamePlay.Scene.TtNode;
                if (node == null || node.Placement == null || value is DVector3 == false)
                    return;
                node.Placement.Position = (DVector3)value;
            }
        }
        public class TtScaleAccessor : ISequencePropertyAccessor
        {
            public string PropertyId { get => ScaleId; }
            public string DisplayName { get => "Scale"; }
            public Rtti.TtTypeDesc ValueType { get => Rtti.TtTypeDesc.TypeOf(typeof(Vector3)); }
            public object Read(object target)
            {
                var node = target as GamePlay.Scene.TtNode;
                if (node == null || node.Placement == null)
                    return null;
                return node.Placement.Scale;
            }
            public void Write(object target, object value)
            {
                var node = target as GamePlay.Scene.TtNode;
                if (node == null || node.Placement == null || value is Vector3 == false)
                    return;
                node.Placement.Scale = (Vector3)value;
            }
        }
        public class TtQuatAccessor : ISequencePropertyAccessor
        {
            public string PropertyId { get => QuatId; }
            public string DisplayName { get => "Rotation"; }
            public Rtti.TtTypeDesc ValueType { get => Rtti.TtTypeDesc.TypeOf(typeof(Quaternion)); }
            public object Read(object target)
            {
                var node = target as GamePlay.Scene.TtNode;
                if (node == null || node.Placement == null)
                    return null;
                return node.Placement.Quat;
            }
            public void Write(object target, object value)
            {
                var node = target as GamePlay.Scene.TtNode;
                if (node == null || node.Placement == null || value is Quaternion == false)
                    return;
                node.Placement.Quat = (Quaternion)value;
            }
        }

        public static void RegisterAll(TtSequencePropertyRegistry registry)
        {
            registry.Register(new TtPositionAccessor());
            registry.Register(new TtScaleAccessor());
            registry.Register(new TtQuatAccessor());
        }
    }
}
