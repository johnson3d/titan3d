using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.Animatable
{
    [AttributeUsage(AttributeTargets.Property)]
    public class AnimatablePropertyAttribute :Attribute
    {

    }
    //the class which can be animatable
    public interface IAnimatable
    {

    }

    public interface IPropertySetter
    {
        public AnimatablePropertyDesc Desc { get; set; }
        public void SetAnimatableObject(IAnimatable obj);
        public void SetValue(Curve.ICurve curve, float time);
    }
    public class AnimatablePropertyDesc
    {
        public Rtti.UTypeDesc ClassType { get; set; } = new Rtti.UTypeDesc();
        public Rtti.UTypeDesc PropertyType { get; set; } = new Rtti.UTypeDesc();
        public override bool Equals(object obj)
        {
            if (!(obj is AnimatablePropertyDesc))
                return false;
            var other = (AnimatablePropertyDesc)obj;
            return ClassType == other.ClassType && PropertyType == other.PropertyType;
        }
        public override int GetHashCode()
        {
            return ClassType.GetHashCode() + PropertyType.GetHashCode();
        }
    }

    public partial class UPropertySetterModule : EngineNS.UModule<EngineNS.UEngine>
    {
        //maybe can use hashcode to replace the AnimatablePropertyDesc as the key
        Dictionary<AnimatablePropertyDesc, Rtti.UTypeDesc> ObjectPropertySetFuncDic { get; set; } = new Dictionary<AnimatablePropertyDesc, Rtti.UTypeDesc>();
        bool bInitialized = false;
        public override Task<bool> Initialize(UEngine host)
        {
            foreach (var i in Rtti.UTypeDescManager.Instance.Services)
            {
                foreach (var j in i.Value.Types)
                {
                    if (j.Value.SystemType.IsAssignableTo(typeof(IPropertySetter)) && !j.Value.SystemType.IsInterface)
                    {
                        var obj = j.Value.SystemType.GetProperty("AnimatableObject");
                        var property = j.Value.SystemType.GetProperty("AnimatableObjectProperty");
                        AnimatablePropertyDesc desc = Rtti.UTypeDescManager.CreateInstance(typeof(AnimatablePropertyDesc)) as AnimatablePropertyDesc;
                        desc.ClassType = Rtti.UTypeDesc.TypeOfFullName(obj.PropertyType.FullName);
                        desc.PropertyType = Rtti.UTypeDesc.TypeOfFullName(property.PropertyType.FullName);
                        ObjectPropertySetFuncDic.Add(desc, j.Value);
                    }
                }
            }
            bInitialized = true;
            return base.Initialize(host);
        }
        public override void Tick(UEngine host)
        {
            if (!bInitialized)
                return;

            base.Tick(host);

            ////test
            //AnimatedInstanceHierarchy.InstanceHierarchyNode node = new AnimatedInstanceHierarchy.InstanceHierarchyNode();
            //node.Current = new AnimatedInstanceHierarchy.InstanceClassDesc();
            //node.Current.ClassTypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(typeof(TestObject));
            //node.Current.Properties.Add(new AnimatedInstanceHierarchy.InstancePropertyDesc() { PropertyTypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(typeof(Vector3)), PropertyName = "Pos", CurveIndex = 0 });
            //UAnimationClip clip = new UAnimationClip() { InstanceHierarchy = node };
            //clip.AnimCurvesList.Add(new Vector3Curve());
            //TestObject aObject = new TestObject();
            //clip.Binding(aObject);
            //clip.Evaluate(0);
            //////////
        }
        public UPropertySetterModule()
        {
            
        }
        public IPropertySetter CreateInstance(AnimatablePropertyDesc objProperty)
        {
            Rtti.UTypeDesc type;
            if (ObjectPropertySetFuncDic.TryGetValue(objProperty, out type))
            {
                return Rtti.UTypeDescManager.CreateInstance(type) as IPropertySetter;
            }
            System.Diagnostics.Debug.Assert(false);
            return null;

            
        }
    }

}

namespace EngineNS
{
    namespace Animation
    {
        public class TestObjectPosSetter : Animatable.IPropertySetter
        {
            public TestObject AnimatableObject { get; set; }
            public Vector3 AnimatableObjectProperty { get; set; }
            public Animatable.AnimatablePropertyDesc Desc { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public void SetAnimatableObject(Animatable.IAnimatable obj)
            {
                AnimatableObject = (TestObject)obj;
            }

            public void SetValue(Curve.ICurve curve, float time)
            {
                AnimatableObject.Pos = ((Curve.Vector3Curve)curve).Evaluate(time);
            }
        }
    }
    partial class UEngine
    {
        public Animation.Animatable.UPropertySetterModule AnimatablePropertySetterModule { get; } = new Animation.Animatable.UPropertySetterModule();
    }

    public class TestObject : Animation.Animatable.IAnimatable
    {
        [EngineNS.Animation.Animatable.AnimatableProperty]
        public Vector3 Pos { get; set; }
    }
}