using EngineNS.IO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.Base
{
    /// <summary>
    /// the unit for animated object like bone, box. it has many animated properties which type is base like vector, bool, int...
    /// 被动画化的对象，作为最小可分割单位，该对象可包含多个被动画化的属性，该属性一般为基础数据类型
    /// 采用扁平的方式管理动画化的对象，如果一个对象的子对象要动画化那么他们在同一个级别
    /// </summary>
    public class TtAnimatedObjectDescription : IO.BaseSerializer
    {
        public Rtti.TtTypeDesc ClassType
        {
            get { return Rtti.TtTypeDesc.TypeOf(TypeName); }
            set { TypeName =value.TypeString; }
        }

        [Rtti.Meta]
        public string TypeName { get; set; } = null;
        [Rtti.Meta]
        public string Name { get; set; } = null;
        [Rtti.Meta]
        public string ParentName { get; set; } = null;
        [Rtti.Meta]
        public TtAnimatedPropertyDescription TranslationProperty { get; set; }

        [Rtti.Meta]
        public TtAnimatedPropertyDescription RotationProperty { get; set; }

        [Rtti.Meta]
        public TtAnimatedPropertyDescription ScaleProperty { get; set; }

        [Rtti.Meta]
        public List<TtAnimatedPropertyDescription> Properties { get; set; } = new List<TtAnimatedPropertyDescription>();

        public TtAnimatedObjectDescription()
        {

        }
    }
    /// <summary>
    /// 被动画化的属性，一般为基础数据类型
    /// </summary>
    public class TtAnimatedPropertyDescription : IO.BaseSerializer
    {
        public Rtti.TtTypeDesc ClassType
        {
            get { return Rtti.TtTypeDesc.TypeOf(TypeName); }
            set { TypeName = value.TypeString; }
        }
        [Rtti.Meta]
        public string TypeName { get; set; }
        [Rtti.Meta]
        public string Name { get; set; }
        [Rtti.Meta]
        public Guid CurveId { get; set; }
    }
}
