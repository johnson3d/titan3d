using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Animation.Binding
{
    public static class PropertyChainCache
    {
        static Dictionary<int, System.Reflection.PropertyInfo[]> mPropertyChains = new Dictionary<int, System.Reflection.PropertyInfo[]>(); // hash, 
        public static ObjectProperty GetObjectProperty(ObjectAnimationBinding binding, object rootObj)
        {
            ObjectProperty op = new ObjectProperty();
            var hash = binding.BindingPath.GetHashCode();
            var hostInstance = rootObj;
            if (binding.Binded)
            {
                //var Properties = mPropertyChains[hash];
                ////遍历到最后的属性的上一级 以取得该属性的host对象
                //var length = Properties.Length - 1;
                //for (int i = 0; i < length; ++i)
                //{
                //    if (Properties[i] == null)
                //        continue;
                //    hostInstance = Properties[i].GetValue(hostInstance);
                //}
                //op.Object = hostInstance;
                //op.Property = Properties[length];
                //object instanceObject = null;
                for (int i = 0; i < binding.BindHierarchyObjects.Count; ++i)
                {
                    var instanceType = hostInstance.GetType();
                    var bho = binding.BindHierarchyObjects[i];
                    if (bho.BindHierarchyType == BindHierarchyObjectType.BHOT_Child)
                    {
                        var childValue = bho.InstanceProperty.GetValue(hostInstance) as List<GamePlay.Actor.GActor>;
                        for (int j = 0; j < childValue.Count; ++j)
                        {
                            if (childValue[i].SpecialName == bho.BindHierarchyName)
                            {
                                hostInstance = childValue[i];
                            }
                        }
                    }
                    if (bho.BindHierarchyType == BindHierarchyObjectType.BHOT_Property)
                    {
                        hostInstance = bho.InstanceProperty.GetValue(hostInstance);
                    }
                    if (bho.BindHierarchyType == BindHierarchyObjectType.BHOT_Target)
                    {
                        op.Object = hostInstance;
                        op.Property = bho.InstanceProperty;
                    }
                }
                return op;
            }
            else
            {
                //var strHierarchy = path.Split('/');
                var length = binding.BindHierarchyObjects.Count;
                System.Reflection.PropertyInfo[] propArray = new System.Reflection.PropertyInfo[length];
                object instanceObject = null;
                for (int i = 1; i < length; ++i)
                {
                    var instanceType = hostInstance.GetType();
                    var bho = binding.BindHierarchyObjects[i];
                    if (bho.BindHierarchyType == BindHierarchyObjectType.BHOT_Root)
                    {
                        var actor = rootObj as GamePlay.Actor.GActor;
                        if(actor.SpecialName == bho.BindHierarchyName)
                        {

                        }
                        else
                        {

                        }
                    }
                    if (bho.BindHierarchyType == BindHierarchyObjectType.BHOT_Child)
                    {
                        var childPreperty = instanceType.GetProperty("Children");
                        bho.InstanceProperty = childPreperty;
                        var childValue = childPreperty.GetValue(hostInstance) as List<GamePlay.Actor.GActor>;
                        for (int j = 0; j < childValue.Count; ++j)
                        {
                            if (childValue[i].SpecialName == bho.BindHierarchyName)
                            {
                                instanceObject = childValue[i];
                            }
                        }
                    }
                    if (bho.BindHierarchyType == BindHierarchyObjectType.BHOT_Property)
                    {
                        bho.InstanceProperty = instanceType.GetProperty(bho.BindHierarchyName);
                        instanceObject = bho.InstanceProperty.GetValue(hostInstance);
                    }
                    if (bho.BindHierarchyType == BindHierarchyObjectType.BHOT_Target)
                    {
                        bho.InstanceProperty = instanceType.GetProperty(bho.BindHierarchyName);
                        //instanceObject = bho.InstanceProperty.GetValue(hostInstance);
                        op.Object = instanceObject;
                        op.Property = bho.InstanceProperty;
                    }
                    hostInstance = instanceObject;
                }
                //mPropertyChains.Add(hash, propArray);
                //op.Object = hostInstance;
                //op.Property = propArray[length - 1];
                binding.Binded = true;
                return op;
            }
        }
    }

    public class AnimationBindingPose
    {
        List<AnimationElementBinding> mBindingElements = new List<AnimationElementBinding>();
        public List<AnimationElementBinding> BindingElements { get => mBindingElements; }
        public void Add(AnimationElementBinding aeb)
        {
            mBindingElements.Add(aeb);
        }
        public virtual void Flush()
        {
            for (int i = 0; i < mBindingElements.Count; ++i)
            {
                mBindingElements[i].Flush();
            }
        }
    }
}
