using EngineNS.Animation.Animatable;
using EngineNS.Animation.Command;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.Player
{
    public class TtSceneAnimationPlayer : IAnimationPlayer
    {
        public float Time { get; set; } = 0;

        public Asset.TtAnimationClip SceneAnimClip { get; set; } = null;


        public void Update(float elapse)
        {
            Time += elapse;

        }

        public void Evaluate()
        {
            //make command
            TtExtractPoseFromClipCommand command = new TtExtractPoseFromClipCommand()
            {
                Time = Time
            };

            //if(IsImmediate)
            command.Execute();
            //else Insert to pipeline
            //AnimationPiple.CommandList.Add();
        }

        //will be Imp when need
        #region Binding
        //public void Binding(Animatable.IAnimatable animatable)
        //{
        //    var animatedHierarchy = SceneAnimClip.AnimatedHierarchy;
        //    System.Diagnostics.Debug.Assert(animatedHierarchy != null);
        //    var objType = Rtti.UTypeDesc.TypeOfFullName(animatable.GetType().FullName);
        //    if (objType == animatedHierarchy.Value.ClassType)
        //    {
        //        var properties = objType.SystemType.GetProperties();
        //        for (int i = 0; i < properties.Length; ++i)
        //        {
        //            var property = properties[i];
        //            if (property.PropertyType.IsGenericType && property.PropertyType.GetInterface("IList") != null)
        //            {
        //                var genericArguments = property.PropertyType.GetGenericArguments();
        //                System.Diagnostics.Debug.Assert(genericArguments.Length != 0);


        //            }
        //            if (property.GetType() != typeof(Animatable.IAnimatable))
        //            {
        //                bool hasAnimatablePropertyAttribute = false;
        //                var atts = property.GetCustomAttributes(false);
        //                for (int j = 0; j < atts.Length; ++j)
        //                {
        //                    if (atts[j].GetType() == typeof(Animatable.AnimatablePropertyAttribute))
        //                    {
        //                        hasAnimatablePropertyAttribute = true;
        //                    }
        //                }
        //                if (hasAnimatablePropertyAttribute)
        //                {
        //                    for (int j = 0; j < animatedHierarchy.Value.Properties.Count; ++j)
        //                    {
        //                        var hierarchyNodePropertyDesc = animatedHierarchy.Value.Properties[j];
        //                        if (property.Name == hierarchyNodePropertyDesc.Name
        //                            && property.PropertyType == hierarchyNodePropertyDesc.ClassType.SystemType)
        //                        {
        //                            Animatable.UAnimatablePropertyDesc desc = new Animatable.UAnimatablePropertyDesc();
        //                            desc.ClassType = objType;
        //                            desc.PropertyType = hierarchyNodePropertyDesc.ClassType;
        //                            var func = UEngine.Instance.AnimatablePropertySetterModule.CreateInstance(desc);
        //                            func.AssignObject(animatable);
        //                            Curve.ICurve curve = null;
        //                            var isExsit = SceneAnimClip.AnimCurvesList.TryGetValue(hierarchyNodePropertyDesc.CurveId, out curve);
        //                            if (isExsit)
        //                            {
        //                                PropertySetFuncMapping.Add(func, SceneAnimClip.AnimCurvesList[hierarchyNodePropertyDesc.CurveId]);
        //                            }
        //                        }
        //                    }
        //                }

        //            }
        //            else
        //            {
        //                var childAnimatable = property.GetValue(animatable) as Animatable.IAnimatable;
        //                for (int j = 0; j < animatedHierarchy.Children.Count; ++j)
        //                {
        //                    if (animatedHierarchy.Children[j].Value.Name == property.Name)
        //                    {
        //                        BindeRecursion(childAnimatable, animatedHierarchy.Children[i]);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        var properties = objType.SystemType.GetProperties();
        //        for (int i = 0; i < properties.Length; ++i)
        //        {
        //            var property = properties[i];
        //            if (property.PropertyType.IsGenericType && property.PropertyType.GetInterface("IList") != null)
        //            {
        //                var genericArguments = property.PropertyType.GetGenericArguments();
        //                System.Diagnostics.Debug.Assert(genericArguments.Length != 0);


        //            }
        //            if (property.GetType() == typeof(Animatable.IAnimatable))
        //            {
        //                var childAnimatable = property.GetValue(animatable) as Animatable.IAnimatable;
        //                for (int j = 0; j < animatedHierarchy.Children.Count; ++j)
        //                {
        //                    if (animatedHierarchy.Children[j].Value.Name == property.Name)
        //                    {
        //                        BindeRecursion(childAnimatable, animatedHierarchy.Children[i]);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
        //void BindeRecursion(Animatable.IAnimatable animatable, Base.UAnimHierarchy instanceHierarchy)
        //{
        //    var animatedHierarchy = SceneAnimClip.AnimatedHierarchy;
        //    var objType = Rtti.UTypeDesc.TypeOfFullName(animatable.GetType().FullName);
        //    if (objType == animatedHierarchy.Value.ClassType)
        //    {
        //        var properties = objType.SystemType.GetProperties();
        //        for (int i = 0; i < properties.Length; ++i)
        //        {
        //            var property = properties[i];
        //            if (property.GetType() != typeof(Animatable.IAnimatable))
        //            {
        //                for (int j = 0; j < animatedHierarchy.Value.Properties.Count; ++j)
        //                {
        //                    var animatedPropertyDesc = animatedHierarchy.Value.Properties[j];
        //                    if (property.Name == animatedPropertyDesc.Name)
        //                    {
        //                        var atts = property.GetCustomAttributes(false);
        //                        for (int k = 0; k < atts.Length; ++k)
        //                        {
        //                            var attType = Rtti.UTypeDesc.TypeOfFullName(atts[k].GetType().FullName);
        //                            if (attType == animatedHierarchy.Value.ClassType)
        //                            {
        //                                Animatable.UAnimatablePropertyDesc desc = new Animatable.UAnimatablePropertyDesc();
        //                                desc.ClassType = objType;
        //                                desc.PropertyType = attType;
        //                                var func = UEngine.Instance.AnimatablePropertySetterModule.CreateInstance(desc);
        //                                func.AssignObject(animatable);

        //                                Curve.ICurve curve = null;
        //                                var isExsit = SceneAnimClip.AnimCurvesList.TryGetValue(animatedPropertyDesc.CurveId, out curve);
        //                                if (isExsit)
        //                                {
        //                                    PropertySetFuncMapping.Add(func, SceneAnimClip.AnimCurvesList[animatedPropertyDesc.CurveId]);
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                var childAnimatable = property.GetValue(animatable) as Animatable.IAnimatable;
        //                for (int j = 0; j < instanceHierarchy.Children.Count; ++j)
        //                {
        //                    if (instanceHierarchy.Children[j].Value.Name == property.Name)
        //                    {
        //                        BindeRecursion(childAnimatable, instanceHierarchy.Children[i]);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        #endregion Binding
    }
}
