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

    public interface IPropertySetter
    {

        public void AssignObject(Animatable.IAnimatable obj);
        public void SetPropertyValue(IAnimatable obj, Curve.ICurve curve, float time);
    }

    // 这里原先有一套「反射式属性 setter 注册表」：PropertyTypeAssignAttribute /
    // PropertyNameAssignAttribute / TtAnimatablePropertyDesc / TtPropertySetterModule，
    // 由 TtPropertySetterModule.Initialize 扫全仓 IPropertySetter 实现建 ObjectPropertySetFuncDic，
    // 再用 CreateInstance(desc) 按「(宿主类, 属性类型, 属性名)」取出 setter 实例。
    // 已整套删除, 原因: 那个字典建完从来没被查过 —— CreateInstance 全仓活代码零调用,
    // 唯一的两处引用在同批删掉的 Player\SceneAnimationPlayer.cs 注释里。真正在跑的绑定路径是下方
    // TtBindedCurveUtil.BindingCurves, 它直接 new 具体 setter, 完全绕过反射。
    // 因为从未执行过, 那套机制还带着三个未暴露的缺陷(无条件 GetProperty("AnimatableObject")
    // 会 NRE、裸 Dictionary.Add 撞键即抛、缺特性时静默留缺省值导致键错位), 留着只会误导后来者
    // 以为「通用属性绑定已经有了」。
    // 场景里「任意节点任意属性驱动」由 Bricks\Sequencer 的显式访问器注册表
    // (TtSequencePropertyRegistry) 承担, 走白名单而不是反射, 见 Documents/design/Sequencer.Plan.md。

}

namespace EngineNS
{
    namespace Animation.SkeletonAnimation
    {
        // 只写 TtAnimatableBonePose.Position, 曲线值取 FNullableVector3。
        // 原先带的 [PropertyTypeAssign] / [PropertyNameAssign] 已随反射注册表一同删除
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

        // 只写 TtAnimatableBonePose.Rotation, 曲线值取 FNullableVector3(此处当作欧拉角用)。
        // 原先带的 [PropertyTypeAssign] / [PropertyNameAssign] 已随反射注册表一同删除
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
}