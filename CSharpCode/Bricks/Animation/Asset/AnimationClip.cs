using EngineNS.IO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation
{
    public partial class UAnimationClip : IO.BaseSerializer, IAnimationAsset
    {
        Data.UAnimationData AnimationData = null;
        public Base.AnimHierarchy AnimatedHierarchy
        {
            get
            {
                return AnimationData.AnimatedHierarchy;
            }
        }
        public List<Curve.ICurve> AnimCurvesList
        {
            get
            {
                return AnimationData.AnimCurvesList;
            }
        }
        //DynamicInitialize
        Dictionary<Animatable.IPropertySetter, int> PropertySetFuncMapping { get; set; } = new Dictionary<Animatable.IPropertySetter, int>();
        public void Update(float time)
        {
            //loop repead pingpong
            //time = ......
            ///

            Evaluate(time);
        }
        public void Evaluate(float time)
        {
            var it = PropertySetFuncMapping.GetEnumerator();
            while (it.MoveNext())
            {
                it.Current.Key.SetValue(AnimCurvesList[it.Current.Value], time);
            }

        }
        public void Binding(Animatable.IAnimatable animatable)
        {
            var objType = Rtti.UTypeDesc.TypeOfFullName(animatable.GetType().FullName);
            if (objType == AnimatedHierarchy.Value.ClassType)
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
                            for (int j = 0; j < AnimatedHierarchy.Value.Properties.Count; ++j)
                            {
                                var hierarchyNodePropertyDesc = AnimatedHierarchy.Value.Properties[j];
                                if (property.Name == hierarchyNodePropertyDesc.Name.ToString()
                                    && property.PropertyType == hierarchyNodePropertyDesc.ClassType.SystemType)
                                {
                                    Animatable.AnimatablePropertyDesc desc = new Animatable.AnimatablePropertyDesc();
                                    desc.ClassType = objType;
                                    desc.PropertyType = hierarchyNodePropertyDesc.ClassType;
                                    var func = UEngine.Instance.AnimatablePropertySetterModule.CreateInstance(desc);
                                    func.SetAnimatableObject(animatable);
                                    PropertySetFuncMapping.Add(func, hierarchyNodePropertyDesc.CurveIndex);
                                }
                            }
                        }

                    }
                    else
                    {
                        var childAnimatable = property.GetValue(animatable) as Animatable.IAnimatable;
                        for (int j = 0; j < AnimatedHierarchy.Children.Count; ++j)
                        {
                            if (AnimatedHierarchy.Children[j].Value.Name.ToString() == property.Name)
                            {
                                BindeRecursion(childAnimatable, AnimatedHierarchy.Children[i]);
                            }
                        }
                    }
                }
            }
        }
        void BindeRecursion(Animatable.IAnimatable animatable, Base.AnimHierarchy instanceHierarchy)
        {
            var objType = Rtti.UTypeDesc.TypeOfFullName(animatable.GetType().FullName);
            if (objType == AnimatedHierarchy.Value.ClassType)
            {
                var properties = objType.SystemType.GetProperties();
                for (int i = 0; i < properties.Length; ++i)
                {
                    var property = properties[i];
                    if (property.GetType() != typeof(Animatable.IAnimatable))
                    {
                        for (int j = 0; j < AnimatedHierarchy.Value.Properties.Count; ++j)
                        {
                            var animatedPropertyDesc = AnimatedHierarchy.Value.Properties[j];
                            if (property.Name == animatedPropertyDesc.Name.ToString())
                            {
                                var atts = property.GetCustomAttributes(false);
                                for (int k = 0; k < atts.Length; ++k)
                                {
                                    var attType = Rtti.UTypeDesc.TypeOfFullName(atts[k].GetType().FullName);
                                    if (attType == AnimatedHierarchy.Value.ClassType)
                                    {
                                        Animatable.AnimatablePropertyDesc desc = new Animatable.AnimatablePropertyDesc();
                                        desc.ClassType = objType;
                                        desc.PropertyType = attType;
                                        var func = UEngine.Instance.AnimatablePropertySetterModule.CreateInstance(desc);
                                        func.SetAnimatableObject(animatable);
                                        PropertySetFuncMapping.Add(func, animatedPropertyDesc.CurveIndex);
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
                            if (instanceHierarchy.Children[j].Value.Name.ToString() == property.Name)
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
