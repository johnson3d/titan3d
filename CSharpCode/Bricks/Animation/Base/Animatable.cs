using EngineNS.Animation.Asset;
using EngineNS.Animation.Base;
using EngineNS.Animation.Curve;
using EngineNS.Animation.SkeletonAnimation;
using EngineNS.Animation.SkeletonAnimation.AnimatablePose;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.Animatable
{
    public class TtBindedCurveUtil
    {
        static TtAnimatedObjectDescription FindAnimatedObjectDesc(String name, Asset.TtAnimationClip animationClip)
        {
            if (animationClip.AnimationChunk.AnimatedObjectDescs.ContainsKey(name))
            {
                return animationClip.AnimationChunk.AnimatedObjectDescs[name];
            }
            return null;
        }
        public static List<TtCurveBindedObject> BindingCurves(TtAnimationClip animationClip, TtAnimatableSkeletonPose animatableSkeletonPose)
        {
            List<TtCurveBindedObject> bindedObjects = new List<TtCurveBindedObject>();
            for (int i = 0; i < animatableSkeletonPose.LimbPoses.Count; i++)
            {
                var AnimatedObjectDesc = FindAnimatedObjectDesc(animatableSkeletonPose.LimbPoses[i].Name, animationClip);
                if (AnimatedObjectDesc == null)
                    continue;
                
                TtCurveBindedObject curveBindedObject = new TtCurveBindedObject();
                curveBindedObject.AnimatableObject = animatableSkeletonPose.LimbPoses[i];
                Curve.ICurve curve = null;
                if(AnimatedObjectDesc.TranslationProperty != null && animationClip.AnimCurvesList.TryGetValue(AnimatedObjectDesc.TranslationProperty.CurveId, out curve))
                {
                    TtCurveBindedProperty bindedProperty = new TtCurveBindedProperty();
                    bindedProperty.Curve = curve;
                    bindedProperty.PropertySetter = new TtBonePosePositionSetter();
                    bindedProperty.PropertySetter.AssignObject(animatableSkeletonPose.LimbPoses[i]);
                    curveBindedObject.CurveBindedProperties.Add(bindedProperty);
                }
                if(AnimatedObjectDesc.RotationProperty != null && animationClip.AnimCurvesList.TryGetValue(AnimatedObjectDesc.RotationProperty.CurveId, out curve))
                {
                    TtCurveBindedProperty bindedProperty = new TtCurveBindedProperty();
                    bindedProperty.Curve = curve;
                    bindedProperty.PropertySetter = new TtBonePoseRotationSetter();
                    bindedProperty.PropertySetter.AssignObject(animatableSkeletonPose.LimbPoses[i]);
                    curveBindedObject.CurveBindedProperties.Add(bindedProperty);
                }
                bindedObjects.Add(curveBindedObject);
            }
            return bindedObjects;
        }
    }
    public class TtCurveBindedProperty
    {
        public ICurve Curve;
        public IPropertySetter PropertySetter;
        public void Evaluate(IAnimatable AnimatableObject, float time)
        {
            PropertySetter.SetPropertyValue(AnimatableObject, Curve, time);
        }
    }
    public class TtCurveBindedObject
    {
        public IAnimatable AnimatableObject;
        public List<TtCurveBindedProperty> CurveBindedProperties = new List<TtCurveBindedProperty>();
        
        public void Evaluate(float time)
        {
            foreach(var bindedProperty in CurveBindedProperties)
            {
                //bindedProperty.PropertySetter.AssignObject(AnimatableObject);
                bindedProperty.Evaluate(AnimatableObject, time);
            }
        }
    }
    [AttributeUsage(AttributeTargets.Class)]
    public class AnimatableObjectAttribute : Attribute
    {

    }

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
        //public virtual void Binding(in IAnimatable animatableObject, in Asset.TtAnimationClip animationClip, ref TtAnimationPropertiesSetter animationPropertiesSetter)
        //{

        //}
    }

    public interface IPropertySetter
    {

        public void AssignObject(Animatable.IAnimatable obj);
        public void SetPropertyValue(IAnimatable obj, Curve.ICurve curve, float time);
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
    public class TtAnimatablePropertyDesc
    {
        public Rtti.UTypeDesc ClassType { get; set; } = new Rtti.UTypeDesc();
        public Rtti.UTypeDesc PropertyType { get; set; } = new Rtti.UTypeDesc();
        public string PropertyName { get; set; }
        public override bool Equals(object obj)
        {
            if (!(obj is TtAnimatablePropertyDesc))
                return false;
            var other = (TtAnimatablePropertyDesc)obj;
            return ClassType == other.ClassType && PropertyType == other.PropertyType && PropertyName == other.PropertyName;
        }
        public override int GetHashCode()
        {
            return ClassType.GetHashCode() + PropertyType.GetHashCode() + PropertyName.GetHashCode();
        }
    }

    //public class TtAnimationPropertiesSetter
    //{
    //    public Dictionary<Animatable.IPropertySetter, Curve.ICurve> PropertySetFuncMapping { get; set; } = new Dictionary<Animatable.IPropertySetter, Curve.ICurve>();
    //    public void Evaluate(float time) // async
    //    {
    //        var it = PropertySetFuncMapping.GetEnumerator();
    //        while (it.MoveNext())
    //        {
    //            it.Current.Key.SetProperty(it.Current.Value, time);
    //        }
    //    }

    //    //DynamicInitialize will be refactoring in next version
    //    public static TtAnimationPropertiesSetter Binding(in Asset.TtAnimationClip animationClip, SkeletonAnimation.AnimatablePose.IAnimatableLimbPose bindedPose)
    //    {
    //        System.Diagnostics.Debug.Assert(bindedPose != null);
    //        System.Diagnostics.Debug.Assert(animationClip != null);

    //        var attrs = bindedPose.GetType().GetCustomAttributes(typeof(IAnimatableClassBindingAttribute), true);
    //        TtAnimationPropertiesSetter propertiesSetter = new TtAnimationPropertiesSetter();
    //        if (attrs.Length == 0)
    //        {
    //            var binder = new CommonAnimatableBingAttribute();
    //            binder.Binding(bindedPose, animationClip, ref propertiesSetter);
    //        }
    //        else
    //        {
    //            var binder = attrs[0] as IAnimatableClassBindingAttribute;
    //            binder.Binding(bindedPose, animationClip, ref propertiesSetter);
    //        }
    //        return propertiesSetter;
    //    }
    //}

    public partial class TtPropertySetterModule : EngineNS.UModule<EngineNS.TtEngine>
    {
        //maybe can use hashcode to replace the AnimatablePropertyDesc as the key
        Dictionary<TtAnimatablePropertyDesc, Rtti.UTypeDesc> ObjectPropertySetFuncDic { get; set; } = new Dictionary<TtAnimatablePropertyDesc, Rtti.UTypeDesc>();
        bool bInitialized = false;
        public override Task<bool> Initialize(TtEngine host)
        {
            foreach (var i in Rtti.UTypeDescManager.Instance.Services)
            {
                foreach (var j in i.Value.Types)
                {
                    if (j.Value.SystemType.IsAssignableTo(typeof(IPropertySetter)) && !j.Value.SystemType.IsInterface)
                    {
                        TtAnimatablePropertyDesc desc = Rtti.UTypeDescManager.CreateInstance(typeof(TtAnimatablePropertyDesc)) as TtAnimatablePropertyDesc;
                        
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
        public override void TickModule(TtEngine host)
        {
            if (!bInitialized)
                return;

            base.TickModule(host);

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
        public TtPropertySetterModule()
        {

        }
        public IPropertySetter CreateInstance(TtAnimatablePropertyDesc objProperty)
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
        [Animatable.PropertyTypeAssign(typeof(FNullableVector3))]
        [Animatable.PropertyNameAssign("Position")]
        public class TtBonePosePositionSetter : Animatable.IPropertySetter
        {
            public AnimatablePose.TtAnimatableBonePose AnimatableObject { get; set; }


            public void AssignObject(Animatable.IAnimatable obj)
            {
                AnimatableObject = (AnimatablePose.TtAnimatableBonePose)obj;
            }
            public void SetPropertyValue(Animatable.IAnimatable obj, ICurve curve, float time)
            {
                AnimatableObject.Position = curve.Evaluate(time).Vector3Value;
            }
        }

        [Animatable.PropertyTypeAssign(typeof(FNullableVector3))]
        [Animatable.PropertyNameAssign("Rotation")]
        public class TtBonePoseRotationSetter : Animatable.IPropertySetter
        {
            public AnimatablePose.TtAnimatableBonePose AnimatableObject { get; set; }
    

            public void AssignObject(Animatable.IAnimatable obj)
            {
                AnimatableObject = (AnimatablePose.TtAnimatableBonePose)obj;
            }
            public void SetPropertyValue(Animatable.IAnimatable obj, ICurve curve, float time)
            {
               AnimatableObject.Rotation = curve.Evaluate(time).Vector3Value;
            }
        }
       
    }


    partial class TtEngine
    {
        public Animation.Animatable.TtPropertySetterModule AnimatablePropertySetterModule { get; } = new Animation.Animatable.TtPropertySetterModule();
    }
}