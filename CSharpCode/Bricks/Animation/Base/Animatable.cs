using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.Animatable
{
    [AttributeUsage(AttributeTargets.Property)]
    public class AnimatablePropertyAttribute : Attribute
    {

    }
    //the class which can be animatable
    public interface IAnimatable
    {
        //for now we use this, maybe refactoring in next version by make a bind system like serialize
        public string Name { get; }
    }

    public class IAnimatableClassBindingAttribute : Attribute
    {
        public virtual void Binding(in IAnimatable animatableObject, in Base.UAnimHierarchy animHierarchy, in Asset.UAnimationClip animationClip, ref UAnimationPropertiesSetter animationPropertiesSetter)
        {

        }
    }

    public class CommonAnimatableBingAttribute : IAnimatableClassBindingAttribute
    {
        //AnimatableObject
        //  |-AnimatableProperty
        //  |-AnimatableObject
        //  |   |-AnimatableProperty
        //  |   |-AnimatableProperty
        //  |       |-AnimatableObject
        //  |           |-AnimatableProperty
        //  |-AnimatableObject
        //  |   |-AnimatableProperty
        //  |   |-List<AnimatableObject>

        public override void Binding(in IAnimatable animatableObject, in Base.UAnimHierarchy animHierarchy, in Asset.UAnimationClip animationClip, ref UAnimationPropertiesSetter animationPropertiesSetter)
        {
            System.Diagnostics.Debug.Assert(animHierarchy != null);
            var objType = Rtti.UTypeDesc.TypeOfFullName(animatableObject.GetType().FullName);
            if(objType == animHierarchy.Node.ClassType)
            {
                // 是否考虑全属性匹配？
                var properties = objType.SystemType.GetProperties();

                for (int i = 0; i < properties.Length; ++i)
                {
                    var property = properties[i];
                    var atts = property.GetCustomAttributes(typeof(Animatable.AnimatablePropertyAttribute), true);
                    if (atts.Length > 0)
                    {
                        if (property.PropertyType.IsGenericType)
                        {
                            if (property.PropertyType.GetInterface("IList") != null)
                            {
                                var genericArguments = property.PropertyType.GetGenericArguments();
                                var list = property.GetValue(animatableObject) as IEnumerable<Object>;
                                System.Diagnostics.Debug.Assert(list != null);
                                foreach (var arg in list)
                                {
                                    System.Diagnostics.Debug.Assert(arg is IAnimatable);
                                    var child = arg as IAnimatable;
                                    foreach(var childInAnimHierarchy in animHierarchy.Children)
                                    {
                                        if(child.Name == childInAnimHierarchy.Node.Name)
                                        {
                                            Binding(child, childInAnimHierarchy, animationClip, ref animationPropertiesSetter);
                                        }
                                    }
                                }
                            }
                        }
                        else if (property.GetType() != typeof(Animatable.IAnimatable))
                        {
                            var propertyObject = property.GetValue(animatableObject);
                            for (int j = 0; j < animHierarchy.Node.Properties.Count; ++j)
                            {
                                var hierarchyNodePropertyDesc = animHierarchy.Node.Properties[j];
                                if (property.Name == hierarchyNodePropertyDesc.Name
                                    && property.PropertyType == hierarchyNodePropertyDesc.ClassType.SystemType)
                                {
                                    Animatable.UAnimatablePropertyDesc desc = new Animatable.UAnimatablePropertyDesc();
                                    desc.ClassType = objType;
                                    desc.PropertyType = hierarchyNodePropertyDesc.ClassType;
                                    desc.PropertyName = hierarchyNodePropertyDesc.Name;
                                    var func = UEngine.Instance.AnimatablePropertySetterModule.CreateInstance(desc);
                                    func.AssignObject(animatableObject);
                                    Curve.ICurve curve = null;
                                    var isExsit = animationClip.AnimCurvesList.TryGetValue(hierarchyNodePropertyDesc.CurveId, out curve);
                                    if (isExsit)
                                    {
                                        animationPropertiesSetter.PropertySetFuncMapping.Add(func, animationClip.AnimCurvesList[hierarchyNodePropertyDesc.CurveId]);
                                    }
                                }
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(false);
                        }
                    }
                }
            }
            else
            {
                //在Properties里面找
                var properties = objType.SystemType.GetProperties();
                foreach(var property in properties)
                {
                    if(property.PropertyType == animHierarchy.Node.ClassType.SystemType 
                        ||property.PropertyType.IsAssignableTo(animHierarchy.Node.ClassType.SystemType)
                        || property.PropertyType.IsAssignableFrom(animHierarchy.Node.ClassType.SystemType))
                    {
                        Binding(property.GetValue(animatableObject) as IAnimatable, animHierarchy, animationClip, ref animationPropertiesSetter);
                    }
                }
            }
        }
    }

    public interface IPropertySetter
    {
        public void AssignObject(IAnimatable obj);
        public void SetProperty(Curve.ICurve curve, float time);
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class PropertyTypeAssignAttribute : Attribute
    {
        public Type PropertyType { get;}
        public PropertyTypeAssignAttribute(Type type)
        {
            PropertyType = type;
        }
    }
    [AttributeUsage(AttributeTargets.Class)]
    public class PropertyNameAssignAttribute : Attribute
    {
        public string PropertyName { get; }
        public PropertyNameAssignAttribute(string name)
        {
            PropertyName = name;
        }
    }
    public class UAnimatablePropertyDesc
    {
        public Rtti.UTypeDesc ClassType { get; set; } = new Rtti.UTypeDesc();
        public Rtti.UTypeDesc PropertyType { get; set; } = new Rtti.UTypeDesc();
        public string PropertyName { get; set; }
        public override bool Equals(object obj)
        {
            if (!(obj is UAnimatablePropertyDesc))
                return false;
            var other = (UAnimatablePropertyDesc)obj;
            return ClassType == other.ClassType && PropertyType == other.PropertyType && PropertyName == other.PropertyName;
        }
        public override int GetHashCode()
        {
            return ClassType.GetHashCode() + PropertyType.GetHashCode() + PropertyName.GetHashCode();
        }
    }

    public class UAnimationPropertiesSetter
    {
        public Dictionary<Animatable.IPropertySetter, Curve.ICurve> PropertySetFuncMapping { get; set; } = new Dictionary<Animatable.IPropertySetter, Curve.ICurve>();
        public void Evaluate(float time) // async
        {
            var it = PropertySetFuncMapping.GetEnumerator();
            while (it.MoveNext())
            {
                it.Current.Key.SetProperty(it.Current.Value, time);
            }
        }

        //DynamicInitialize will be refactoring in next version
        public static UAnimationPropertiesSetter Binding(in Asset.UAnimationClip animationClip, SkeletonAnimation.AnimatablePose.IAnimatableLimbPose bindedPose)
        {
            System.Diagnostics.Debug.Assert(bindedPose != null);
            System.Diagnostics.Debug.Assert(animationClip != null);

            var attrs = bindedPose.GetType().GetCustomAttributes(typeof(IAnimatableClassBindingAttribute), true);
            UAnimationPropertiesSetter propertiesSetter = new UAnimationPropertiesSetter();
            if (attrs.Length == 0)
            {
                var binder = new CommonAnimatableBingAttribute();
                binder.Binding(bindedPose, animationClip.AnimatedHierarchy, animationClip, ref propertiesSetter);
            }
            else
            {
                var binder = attrs[0] as IAnimatableClassBindingAttribute;
                binder.Binding(bindedPose, animationClip.AnimatedHierarchy, animationClip, ref propertiesSetter);
            }
            return propertiesSetter;
        }
    }

    public partial class UPropertySetterModule : EngineNS.UModule<EngineNS.UEngine>
    {
        //maybe can use hashcode to replace the AnimatablePropertyDesc as the key
        Dictionary<UAnimatablePropertyDesc, Rtti.UTypeDesc> ObjectPropertySetFuncDic { get; set; } = new Dictionary<UAnimatablePropertyDesc, Rtti.UTypeDesc>();
        bool bInitialized = false;
        public override Task<bool> Initialize(UEngine host)
        {
            foreach (var i in Rtti.UTypeDescManager.Instance.Services)
            {
                foreach (var j in i.Value.Types)
                {
                    if (j.Value.SystemType.IsAssignableTo(typeof(IPropertySetter)) && !j.Value.SystemType.IsInterface)
                    {
                        UAnimatablePropertyDesc desc = Rtti.UTypeDescManager.CreateInstance(typeof(UAnimatablePropertyDesc)) as UAnimatablePropertyDesc;
                        
                        var obj = j.Value.SystemType.GetProperty("AnimatableObject");
                        {
                            var assignAttrs = j.Value.SystemType.GetCustomAttributes(typeof(PropertyNameAssignAttribute), true);
                            if (assignAttrs.Length > 0)
                            {
                                var assign = assignAttrs[0] as PropertyNameAssignAttribute;
                                desc.PropertyName = assign.PropertyName;
                            }
                        }
                        {
                            var assignAttrs = j.Value.SystemType.GetCustomAttributes(typeof(PropertyTypeAssignAttribute), true);
                            if (assignAttrs.Length > 0)
                            {
                                var assign = assignAttrs[0] as PropertyTypeAssignAttribute;
                                desc.PropertyType = Rtti.UTypeDesc.TypeOf(assign.PropertyType);
                            }
                        }
                        desc.ClassType = Rtti.UTypeDesc.TypeOfFullName(obj.PropertyType.FullName);
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
        public IPropertySetter CreateInstance(UAnimatablePropertyDesc objProperty)
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
    namespace Animation.SkeletonAnimation
    {
        [Animatable.PropertyTypeAssign(typeof(Vector3))]
        [Animatable.PropertyNameAssign("Position")]
        public class UBonePosePositionSetter : Animatable.IPropertySetter
        {
            public AnimatablePose.UAnimatableBonePose AnimatableObject { get; set; }


            public void AssignObject(Animatable.IAnimatable obj)
            {
                AnimatableObject = (AnimatablePose.UAnimatableBonePose)obj;
            }

            public void SetProperty(Curve.ICurve curve, float time)
            {
                AnimatableObject.Position = curve.Evaluate(time).Vector3Value;
            }
        }

        [Animatable.PropertyTypeAssign(typeof(Vector3))]
        [Animatable.PropertyNameAssign("Rotation")]
        public class UBonePoseRotationSetter : Animatable.IPropertySetter
        {
            public AnimatablePose.UAnimatableBonePose AnimatableObject { get; set; }
    

            public void AssignObject(Animatable.IAnimatable obj)
            {
                AnimatableObject = (AnimatablePose.UAnimatableBonePose)obj;
            }

            public void SetProperty(Curve.ICurve curve, float time)
            {
                AnimatableObject.Rotation = curve.Evaluate(time).Vector3Value;
            }
        }
       
    }


    partial class UEngine
    {
        public Animation.Animatable.UPropertySetterModule AnimatablePropertySetterModule { get; } = new Animation.Animatable.UPropertySetterModule();
    }
}