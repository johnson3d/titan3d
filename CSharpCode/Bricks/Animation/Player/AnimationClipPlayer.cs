using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.Player
{
    public class AnimationClipPlayer : IAnimationPlayer
    {
        public float Time { get; set; } = 0;

        public Asset.UAnimationClip Clip { get; set; } = null;
        //DynamicInitialize
        Dictionary<Animatable.IPropertySetter, Curve.ICurve> PropertySetFuncMapping { get; set; } = new Dictionary<Animatable.IPropertySetter, Curve.ICurve>();

        public void Update(float time)
        {
            //make command
            Pipeline.PropertiesSettingCommand command = new Pipeline.PropertiesSettingCommand();
            command.Context = new Pipeline.PropertiesSettingCommandContext(PropertySetFuncMapping, time);

            //if(IsImmediate)
            command.Excute();
            //else Insert to pipeline
            //AnimationPiple.CommandList.Add();

        }
        public void Binding(Animatable.IAnimatable animatable)
        {
            var animatedHierarchy = Clip.AnimatedHierarchy;
            var objType = Rtti.UTypeDesc.TypeOfFullName(animatable.GetType().FullName);
            if (objType == animatedHierarchy.Value.ClassType)
            {
                var properties = objType.SystemType.GetProperties();
                for (int i = 0; i < properties.Length; ++i)
                {
                    var property = properties[i];
                    if (property.GetType() != typeof(Animatable.IAnimatable))
                    {
                        bool hasAnimatablePropertyAttribute = false;
                        var atts = property.GetCustomAttributes(false);
                        for (int j = 0; j < atts.Length; ++j)
                        {
                            if (atts[j].GetType() == typeof(Animatable.AnimatablePropertyAttribute))
                            {
                                hasAnimatablePropertyAttribute = true;
                            }
                        }
                        if (hasAnimatablePropertyAttribute)
                        {
                            for (int j = 0; j < animatedHierarchy.Value.Properties.Count; ++j)
                            {
                                var hierarchyNodePropertyDesc = animatedHierarchy.Value.Properties[j];
                                if (property.Name == hierarchyNodePropertyDesc.Name.GetString()
                                    && property.PropertyType == hierarchyNodePropertyDesc.ClassType.SystemType)
                                {
                                    Animatable.AnimatablePropertyDesc desc = new Animatable.AnimatablePropertyDesc();
                                    desc.ClassType = objType;
                                    desc.PropertyType = hierarchyNodePropertyDesc.ClassType;
                                    var func = UEngine.Instance.AnimatablePropertySetterModule.CreateInstance(desc);
                                    func.SetAnimatableObject(animatable);
                                    Curve.ICurve curve = null;
                                    var isExsit = Clip.AnimCurvesList.TryGetValue(hierarchyNodePropertyDesc.CurveId, out curve);
                                    if (isExsit)
                                    {
                                        PropertySetFuncMapping.Add(func, Clip.AnimCurvesList[hierarchyNodePropertyDesc.CurveId]);
                                    }
                                }
                            }
                        }

                    }
                    else
                    {
                        var childAnimatable = property.GetValue(animatable) as Animatable.IAnimatable;
                        for (int j = 0; j < animatedHierarchy.Children.Count; ++j)
                        {
                            if (animatedHierarchy.Children[j].Value.Name.GetString() == property.Name)
                            {
                                BindeRecursion(childAnimatable, animatedHierarchy.Children[i]);
                            }
                        }
                    }
                }
            }
        }
        void BindeRecursion(Animatable.IAnimatable animatable, Base.AnimHierarchy instanceHierarchy)
        {
            var animatedHierarchy = Clip.AnimatedHierarchy;
            var objType = Rtti.UTypeDesc.TypeOfFullName(animatable.GetType().FullName);
            if (objType == animatedHierarchy.Value.ClassType)
            {
                var properties = objType.SystemType.GetProperties();
                for (int i = 0; i < properties.Length; ++i)
                {
                    var property = properties[i];
                    if (property.GetType() != typeof(Animatable.IAnimatable))
                    {
                        for (int j = 0; j < animatedHierarchy.Value.Properties.Count; ++j)
                        {
                            var animatedPropertyDesc = animatedHierarchy.Value.Properties[j];
                            if (property.Name == animatedPropertyDesc.Name.GetString())
                            {
                                var atts = property.GetCustomAttributes(false);
                                for (int k = 0; k < atts.Length; ++k)
                                {
                                    var attType = Rtti.UTypeDesc.TypeOfFullName(atts[k].GetType().FullName);
                                    if (attType == animatedHierarchy.Value.ClassType)
                                    {
                                        Animatable.AnimatablePropertyDesc desc = new Animatable.AnimatablePropertyDesc();
                                        desc.ClassType = objType;
                                        desc.PropertyType = attType;
                                        var func = UEngine.Instance.AnimatablePropertySetterModule.CreateInstance(desc);
                                        func.SetAnimatableObject(animatable);

                                        Curve.ICurve curve = null;
                                        var isExsit = Clip.AnimCurvesList.TryGetValue(animatedPropertyDesc.CurveId, out curve);
                                        if (isExsit)
                                        {
                                            PropertySetFuncMapping.Add(func, Clip.AnimCurvesList[animatedPropertyDesc.CurveId]);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        var childAnimatable = property.GetValue(animatable) as Animatable.IAnimatable;
                        for (int j = 0; j < instanceHierarchy.Children.Count; ++j)
                        {
                            if (instanceHierarchy.Children[j].Value.Name.GetString() == property.Name)
                            {
                                BindeRecursion(childAnimatable, instanceHierarchy.Children[i]);
                            }
                        }
                    }
                }
            }
        }
    }
}
