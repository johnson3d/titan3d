using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Animation.Binding
{
    public class AnimationElementBinding
    {
        string mBindingPath = "";
        public string BindingPath
        {
            get => mBindingPath;
            set
            {
                mBindingPath = value;
                if (string.IsNullOrEmpty(value) || value == "null")
                    return;
                var strHierarchy = value.Split('/');
                var length = strHierarchy.Length;
                for (int i = 0; i < length - 1; ++i)
                {
                    BindHierarchyObject bho = new BindHierarchyObject();
                    var pair = strHierarchy[i].Split(':');
                    if (pair[0] == BindHierarchyObjectType.BHOT_Root.ToString())
                    {
                        bho.BindHierarchyType = BindHierarchyObjectType.BHOT_Root;
                        bho.BindHierarchyName = pair[1];
                    }
                    if (pair[0] == BindHierarchyObjectType.BHOT_Child.ToString())
                    {
                        bho.BindHierarchyType = BindHierarchyObjectType.BHOT_Child;
                        bho.BindHierarchyName = pair[1];
                    }
                    if (pair[0] == BindHierarchyObjectType.BHOT_Property.ToString())
                    {
                        bho.BindHierarchyType = BindHierarchyObjectType.BHOT_Property;
                        bho.BindHierarchyName = pair[1];
                    }
                    BindHierarchyObjects.Add(bho);
                }
                BindHierarchyObject target = new BindHierarchyObject();
                target.BindHierarchyType = BindHierarchyObjectType.BHOT_Target;
                target.BindHierarchyName = strHierarchy[length - 1];
                BindHierarchyObjects.Add(target);
            }
        }
        public List<BindHierarchyObject> BindHierarchyObjects { get; set; } = new List<BindHierarchyObject>();
        //WeightedElement Element { get; }
        public uint AnimationElementHash { get; set; } = 0;
        public float Weight { get; set; } = 1.0f;
        public CurveResult Value { get; set; }
        public virtual void Flush() { }
    }
    public class ObjectProperty
    {
        public object Object;
        public System.Reflection.PropertyInfo Property;
    }
    public class ObjectAnimationBinding : AnimationElementBinding
    {
        public object RootObject;
        public ObjectProperty BindedProperty;
        public bool Binded { get; set; } = false;

        public FDoSetValue DoSetValue = Default_DoSetValue;

        public override void Flush()
        {
            DoSetValue(this);
        }
        public delegate void FDoSetValue(ObjectAnimationBinding bind);
        protected static void Default_DoSetValue(ObjectAnimationBinding bind)
        {
            var prop = PropertyChainCache.GetObjectProperty(bind, bind.RootObject);

            switch (bind.Value.Type)
            {
                case CurveType.CT_Bool:
                    {
                        prop.Property.SetValue(prop.Object, bind.Value.BoolResult);
                    }
                    break;
                case CurveType.CT_Int:
                    {
                        prop.Property.SetValue(prop.Object, bind.Value.BoolResult);
                    }
                    break;
                case CurveType.CT_Float:
                    {
                        prop.Property.SetValue(prop.Object, bind.Value.FloatResult);
                    }
                    break;
                case CurveType.CT_Vector2:
                    {
                        prop.Property.SetValue(prop.Object, bind.Value.Vector2Result);
                    }
                    break;
                case CurveType.CT_Vector3:
                    {
                        prop.Property.SetValue(prop.Object, bind.Value.Vector3Result);
                    }
                    break;
                case CurveType.CT_Vector4:
                    {
                        //prop.Property.SetValue(prop.Object, bind.Value.ve);
                    }
                    break;
                case CurveType.CT_Quaternion:
                    {
                        prop.Property.SetValue(prop.Object, bind.Value.QuaternionResult);
                    }
                    break;
            }
            //BindedObject.(Property.Name) = Element.Value.IntResult;

            //bind.BindedProperty.Property.SetValue(bind.BindedProperty.Object, bind.Element.Value.IntResult);
        }
    }
    public class BoneAnimationBinding : AnimationElementBinding
    {
        public Pose.CGfxBonePose Bone;
        public Skeleton.CGfxMotionState MotionData;
        public override void Flush()
        {
            if (Bone == null)
                return;
            Skeleton.CGfxBoneTransform transform = new Skeleton.CGfxBoneTransform();
            transform.Position = Value.BoneSRTResult.Position;
            transform.Rotation = Value.BoneSRTResult.Rotation;
            transform.Scale = Value.BoneSRTResult.Scale;
            //euqal Bone.LoaclSpaceTransform();

            //var result = new Skeleton.CGfxTransform();
            //temp.Position = relativeTransform.Position + relativeTransform.Rotation * transform.Position;
            //temp.Rotation = transform.Rotation;
            //temp.Scale = Vector3.Modulate(relativeTransform.Scale, transform.Scale);
            Bone.MotionData = MotionData;
            Bone.Transform = transform;
        }
        public void Flush(Pose.CGfxSkeletonPose pose)
        {
            //var bone = pose.FindBone(Bone.BoneDesc.NameHash);
            var bone = pose.FindBonePose(AnimationElementHash);
            Skeleton.CGfxBoneTransform transform = new Skeleton.CGfxBoneTransform();
            transform.Position = Value.BoneSRTResult.Position;
            transform.Rotation = Value.BoneSRTResult.Rotation;
            transform.Scale = Value.BoneSRTResult.Scale;
            Bone.MotionData = MotionData;
            bone.Transform = transform;
        }
    }
    public class SkeletonAnimationBinding : AnimationElementBinding
    {
        //public Pose.AnimationPose 
        public Pose.CGfxSkeletonPose Pose { get; set; }
        List<BoneAnimationBinding> mBoneBindings = new List<BoneAnimationBinding>();
        public List<BoneAnimationBinding> BoneBindings { get => mBoneBindings; }
        public void Add(BoneAnimationBinding boneBinding)
        {
            mBoneBindings.Add(boneBinding);
        }
        public override void Flush()
        {
            for (int i = 0; i < mBoneBindings.Count; ++i)
            {
                mBoneBindings[i].Flush(Pose);
            }
            //Animation.Runtime.CGfxAnimationRuntime.CalculateCharacterSpacePose(Pose);
        }
        public static SkeletonAnimationBinding Bind(Pose.CGfxSkeletonPose pose, AnimElement.CGfxSkeletonAnimationElement skeletonAnimationElement)
        {
            //生成全骨骼绑定，不知动画里面的，然后link时才能对
            var skeletonBinding = new SkeletonAnimationBinding();
            skeletonBinding.Pose = pose;
            using (var it = skeletonAnimationElement.BoneAnimationElements.GetEnumerator())
            {
                while (it.MoveNext())
                {
                    var boneElement = it.Current.Value;
                    var bone = pose.FindBonePose(boneElement.Desc.NameHash);
                    if (bone != null)
                    {
                        var boneBind = new BoneAnimationBinding();
                        boneBind.Bone = bone;
                        boneBind.AnimationElementHash = boneElement.Desc.NameHash;
                        skeletonBinding.Add(boneBind);
                    }
                }
            }
            return skeletonBinding;
        }
    }
}
