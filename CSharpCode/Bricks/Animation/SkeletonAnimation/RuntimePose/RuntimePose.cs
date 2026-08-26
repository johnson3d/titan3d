using EngineNS.Animation.RootMotion;
using EngineNS.Animation.SkeletonAnimation.AnimatablePose;
using EngineNS.Animation.SkeletonAnimation.Skeleton;
using EngineNS.Animation.SkeletonAnimation.Skeleton.Limb;
using EngineNS.Rtti;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EngineNS.Animation.SkeletonAnimation.Runtime.Pose
{
    public interface IRuntimePose
    {
        public int HashCode { get; }
        public List<ILimbDesc> Descs { get; set; }
        public List<FTransform> Transforms { get; set; }
        /// <summary>
        /// 本帧随该Pose一起流动的RootMotion增量。采样节点写入, 混合节点按与Pose相同的权重混合。
        /// 这样不依赖BlendTree.Tick的递归结构就能拿到最终权重下的RootMotion。
        /// </summary>
        public FRootMotionData RootMotion { get; set; }
    }
    public class TtLocalSpaceRuntimePose : IRuntimePose  //or struct
    {
        public int HashCode => 0;
        public List<ILimbDesc> Descs { get; set; } = new List<ILimbDesc>();    //or array
        public List<FTransform> Transforms { get; set; } = new List<FTransform>();//or array
        public FRootMotionData RootMotion { get; set; } = FRootMotionData.Empty;
    }
    public class TtMeshSpaceRuntimePose : IRuntimePose
    {
        public int HashCode => 0;
        public List<ILimbDesc> Descs { get; set; } = new List<ILimbDesc>();//or array
        public List<FTransform> Transforms { get; set; } = new List<FTransform>();//or array
        public FRootMotionData RootMotion { get; set; } = FRootMotionData.Empty;
    }

    /// <summary>
    /// LocalSpace Position MeshSpace Rotation
    /// </summary>
    public class TtLPosMRotRuntimePose : IRuntimePose  //or struct
    {
        public int HashCode => 0;
        public List<ILimbDesc> Descs { get; set; } = new List<ILimbDesc>();    //or array
        public List<FTransform> Transforms { get; set; } = new List<FTransform>();//or array
        public FRootMotionData RootMotion { get; set; } = FRootMotionData.Empty;
    }


    public class TtRuntimePoseUtility
    {
        #region Get
        public static FTransform GetTransform(string limbName, IRuntimePose pose)
        {
            var index = GetIndex(limbName, pose);
            if (index.Value < 0)
            {
                return FTransform.Identity;
            }
            return pose.Transforms[index.Value];
        }
        public static FTransform GetTransform(uint limbNameHash, IRuntimePose pose)
        {
            var index = GetIndex(limbNameHash, pose);
            if (index.Value < 0)
            {
                return FTransform.Identity;
            }
            return pose.Transforms[index.Value];
        }
        public static ILimbDesc GetDesc(string limbName, IRuntimePose pose)
        {
            var index = GetIndex(limbName, pose);
            if (index.Value < 0)
            {
                return null;
            }
            return pose.Descs[index.Value];
        }
        public static ILimbDesc GetDesc(uint limbNameHash, IRuntimePose pose)
        {
            var index = GetIndex(limbNameHash, pose);
            if (index.Value < 0)
            {
                return null;
            }
            return pose.Descs[index.Value];
        }
        public static IndexInSkeleton GetIndex(string limbName, IRuntimePose pose)
        {
            for (int i = 0; i < pose.Descs.Count; ++i)
            {
                if (limbName == pose.Descs[i].Name)
                {
                    return new IndexInSkeleton(i);
                }
            }
            return new IndexInSkeleton(-1);
        }
        public static IndexInSkeleton GetIndex(uint limbNameHash, IRuntimePose pose)
        {
            for (int i = 0; i < pose.Descs.Count; ++i)
            {
                if (limbNameHash == pose.Descs[i].NameHash)
                {
                    return new IndexInSkeleton(i);
                }
            }
            return new IndexInSkeleton(-1);
        }
        public static List<IndexInSkeleton> GetChildren(uint limbNameHash, IRuntimePose pose)
        {
            List<IndexInSkeleton> childs = new List<IndexInSkeleton>();
            for (int i = 0; i < pose.Descs.Count; ++i)
            {
                if (limbNameHash == pose.Descs[i].ParentHash)
                {
                    childs.Add(new IndexInSkeleton(i));
                }
            }
            return childs;
        }
        public static IndexInSkeleton GetRoot(IRuntimePose pose)
        {
            for (int i = 0; i < pose.Descs.Count; ++i)
            {
                if (string.IsNullOrEmpty(pose.Descs[i].ParentName))
                {
                    return new IndexInSkeleton(i);
                }
            }
            return new IndexInSkeleton(-1);
        }
        public static ILimbDesc GetRootDesc(IRuntimePose pose)
        {
            for (int i = 0; i < pose.Descs.Count; ++i)
            {
                if (string.IsNullOrEmpty(pose.Descs[i].ParentName))
                {
                    return pose.Descs[i];
                }
            }
            return null;
        }
        #endregion get

        #region Create RuntimePose
        public static TtLocalSpaceRuntimePose CreateLocalSpaceRuntimePose(AnimatablePose.TtAnimatableSkeletonPose skeletonPose)
        {
            TtLocalSpaceRuntimePose pose = new TtLocalSpaceRuntimePose();
            if (skeletonPose != null)
            {
                for (int i = 0; i < skeletonPose.LimbPoses.Count; ++i)
                {
                    pose.Transforms.Add(skeletonPose.LimbPoses[i].Transtorm);
                    pose.Descs.Add(skeletonPose.LimbPoses[i].Desc);
                }
            }
            return pose;
        }
        //public static TtMeshSpaceRuntimePose CreateMeshSpaceRuntimePose(AnimatablePose.TtAnimatableSkeletonPose skeletonPose)
        //{
        //    TtLocalSpaceRuntimePose localSpacePose = CreateLocalSpaceRuntimePose(skeletonPose);
        //    return ConvetToMeshSpaceRuntimePose(localSpacePose);
        //}
        #endregion Create RuntimePose

        #region ConvertRuntimePose
        public static TtMeshSpaceRuntimePose ConvetToMeshSpaceRuntimePose(TtLocalSpaceRuntimePose localSpacePose)
        {
            TtMeshSpaceRuntimePose temp = new TtMeshSpaceRuntimePose();
            ConvetToMeshSpaceRuntimePose(ref temp, localSpacePose);
            return temp;
        }
        public static void ConvetToMeshSpaceRuntimePose(ref TtMeshSpaceRuntimePose desMeshSpacePose, TtLocalSpaceRuntimePose srcLocalSpacePose)
        {
            desMeshSpacePose.Transforms.Clear();
            desMeshSpacePose.Descs.Clear();
            desMeshSpacePose.Transforms.AddRange(srcLocalSpacePose.Transforms);
            desMeshSpacePose.Descs.AddRange(srcLocalSpacePose.Descs);

            var rootHash = GetRootDesc(desMeshSpacePose).NameHash;
            ConvertToMeshSpaceTransformRecursively(ref desMeshSpacePose, rootHash, srcLocalSpacePose);
        }
        static void ConvertToMeshSpaceTransformRecursively(ref TtMeshSpaceRuntimePose outPose, uint parentHash, TtLocalSpaceRuntimePose srcPose)
        {
            var parentIndex = GetIndex(parentHash, srcPose);
            var childrenIndexs = GetChildren(parentHash, srcPose);
            for (int i = 0; i < childrenIndexs.Count; ++i)
            {
                var childIndex = childrenIndexs[i].Value;
                FTransform temp;
                FTransform.Multiply(out temp, srcPose.Transforms[childIndex], outPose.Transforms[parentIndex.Value]);
                outPose.Transforms[childIndex] = temp;
                ConvertToMeshSpaceTransformRecursively(ref outPose, srcPose.Descs[childIndex].NameHash, srcPose);
            }
        }
        public static TtLocalSpaceRuntimePose ConvetToLocalSpaceRuntimePose(TtMeshSpaceRuntimePose meshSpacePose)
        {
            TtLocalSpaceRuntimePose temp = new TtLocalSpaceRuntimePose();
            ConvetToLocalSpaceRuntimePose(ref temp, meshSpacePose);
            return temp;
        }
        public static void ConvetToLocalSpaceRuntimePose(ref TtLocalSpaceRuntimePose desLocalSpacePose, TtMeshSpaceRuntimePose srcMeshSpacePose)
        {
            desLocalSpacePose.Transforms.Clear();
            desLocalSpacePose.Descs.Clear();
            desLocalSpacePose.Transforms.AddRange(srcMeshSpacePose.Transforms);
            desLocalSpacePose.Descs.AddRange(srcMeshSpacePose.Descs);

            var rootHash = GetRootDesc(desLocalSpacePose).NameHash;
            ConvertToLocalSpaceTransformRecursively(ref desLocalSpacePose, rootHash, srcMeshSpacePose);
        }
        static void ConvertToLocalSpaceTransformRecursively(ref TtLocalSpaceRuntimePose outPose, uint parentHash, TtMeshSpaceRuntimePose srcPose)
        {
            var parentIndex = GetIndex(parentHash, srcPose);
            var childrenIndexs = GetChildren(parentHash, srcPose);
            for (int i = 0; i < childrenIndexs.Count; ++i)
            {
                var childIndex = childrenIndexs[i].Value;
                ConvertToLocalSpaceTransformRecursively(ref outPose, srcPose.Descs[childIndex].NameHash, srcPose);
                FTransform temp;
                FTransform.Multiply(out temp, srcPose.Transforms[childIndex], srcPose.Transforms[parentIndex.Value].Inverse());
                outPose.Transforms[childIndex] = temp;
            }
        }

        public static void ConvetToLocalSpaceRuntimePose(ref TtLocalSpaceRuntimePose desLocalSpacePose, TtAnimatableSkeletonPose skeletonPose)
        {
            desLocalSpacePose.Transforms.Clear();
            desLocalSpacePose.Descs.Clear();
            for (int i = 0; i < skeletonPose.LimbPoses.Count; ++i)
            {
                desLocalSpacePose.Transforms.Add(skeletonPose.LimbPoses[i].Transtorm);
                desLocalSpacePose.Descs.Add(skeletonPose.LimbPoses[i].Desc);
            }
        }

        public static TtLPosMRotRuntimePose ConvetToLPosMRotRuntimePose(TtLocalSpaceRuntimePose localSpacePose)
        {
            TtLPosMRotRuntimePose temp = new TtLPosMRotRuntimePose();
            ConvetToLPosMRotRuntimePose(ref temp, localSpacePose);
            return temp;
        }
        public static void ConvetToLPosMRotRuntimePose(ref TtLPosMRotRuntimePose desLPosMRotSpacePose, TtLocalSpaceRuntimePose srcLocalSpacePose)
        {
            desLPosMRotSpacePose.Transforms.Clear();
            desLPosMRotSpacePose.Descs.Clear();
            desLPosMRotSpacePose.Transforms.AddRange(srcLocalSpacePose.Transforms);
            desLPosMRotSpacePose.Descs.AddRange(srcLocalSpacePose.Descs);

            var rootHash = GetRootDesc(desLPosMRotSpacePose).NameHash;
            ConvetToLPosMRotTransformRecursively(ref desLPosMRotSpacePose, rootHash, srcLocalSpacePose);
        }
        static void ConvetToLPosMRotTransformRecursively(ref TtLPosMRotRuntimePose outPose, uint parentHash, TtLocalSpaceRuntimePose srcPose)
        {
            var parentIndex = GetIndex(parentHash, srcPose);
            var childrenIndexs = GetChildren(parentHash, srcPose);
            for (int i = 0; i < childrenIndexs.Count; ++i)
            {
                var childIndex = childrenIndexs[i].Value;
                FTransform temp;
                temp = srcPose.Transforms[childIndex];
                temp.Quat = temp.Quat * srcPose.Transforms[parentIndex.Value].Quat;
                outPose.Transforms[childIndex] = temp;
                ConvetToLPosMRotTransformRecursively(ref outPose, srcPose.Descs[childIndex].NameHash, srcPose);
            }
        }

        public static TtLocalSpaceRuntimePose ConvetToLocalSpaceRuntimePose(TtLPosMRotRuntimePose lPosMRotRuntimePose)
        {
            TtLocalSpaceRuntimePose temp = new TtLocalSpaceRuntimePose();
            ConvetToLocalSpaceRuntimePose(ref temp, lPosMRotRuntimePose);
            return temp;
        }
        public static void ConvetToLocalSpaceRuntimePose(ref TtLocalSpaceRuntimePose desLocalSpacePose, TtLPosMRotRuntimePose srcLPosMRotRuntimePose)
        {
            desLocalSpacePose.Transforms.Clear();
            desLocalSpacePose.Descs.Clear();
            desLocalSpacePose.Transforms.AddRange(srcLPosMRotRuntimePose.Transforms);
            desLocalSpacePose.Descs.AddRange(srcLPosMRotRuntimePose.Descs);

            var rootHash = GetRootDesc(desLocalSpacePose).NameHash;
            ConvertToLocalSpaceTransformRecursively(ref desLocalSpacePose, rootHash, srcLPosMRotRuntimePose);
        }
        static void ConvertToLocalSpaceTransformRecursively(ref TtLocalSpaceRuntimePose outPose, uint parentHash, TtLPosMRotRuntimePose srcPose)
        {
            var parentIndex = GetIndex(parentHash, srcPose);
            var childrenIndexs = GetChildren(parentHash, srcPose);
            for (int i = 0; i < childrenIndexs.Count; ++i)
            {
                var childIndex = childrenIndexs[i].Value;
                ConvertToLocalSpaceTransformRecursively(ref outPose, srcPose.Descs[childIndex].NameHash, srcPose);
                FTransform temp;
                temp = srcPose.Transforms[childIndex];
                temp.Quat = temp.Quat * srcPose.Transforms[parentIndex.Value].Quat.Inverse();
                outPose.Transforms[childIndex] = temp;
            }
        }
        #endregion ConvertRuntimePose

        public static T CopyPose<T>(T pose) where T : IRuntimePose
        {
            //T temp = (T)TtTypeDescManager.CreateInstance(TtTypeDesc.TypeOf<T>());
            T temp = (T)TtTypeDescManager.CreateInstance(typeof(T));
            temp.Transforms.AddRange(pose.Transforms);
            temp.Descs.AddRange(pose.Descs);
            temp.RootMotion = pose.RootMotion;
            return temp;
        }
        public static void CopyPose<T>(ref T descPose, T srcPose) where T : IRuntimePose
        {
            if (srcPose == null)
                return;
            if (descPose == null)
            {
                //descPose = (T)TtTypeDescManager.CreateInstance(TtTypeDesc.TypeOf<T>());
                descPose = (T)TtTypeDescManager.CreateInstance(typeof(T));
            }
            descPose.Transforms.Clear();
            descPose.Descs.Clear();
            descPose.Transforms.AddRange(srcPose.Transforms);
            descPose.Descs.AddRange(srcPose.Descs);
            descPose.RootMotion = srcPose.RootMotion;
        }
        public static void CopyTransforms<T>(ref T descPose, T srcPose) where T : IRuntimePose
        {
            System.Diagnostics.Debug.Assert(descPose != null && srcPose != null);
            descPose.Transforms.Clear();
            descPose.Transforms.AddRange(srcPose.Transforms);
            descPose.RootMotion = srcPose.RootMotion;
        }
        public static void CopyTransforms(ref TtLocalSpaceRuntimePose descPose, AnimatablePose.TtAnimatableSkeletonPose srcPose)
        {
            for (int i = 0; i < srcPose.LimbPoses.Count; ++i)
            {
                descPose.Transforms[i] = srcPose.LimbPoses[i].Transtorm;
            }
        }

        public static void BlendPoses<T>(ref T outPose, T aPose, T bPose, float alpha) where T : IRuntimePose
        {
            System.Diagnostics.Debug.Assert(aPose.Transforms.Count == bPose.Transforms.Count);
            for (int i = 0; i < aPose.Transforms.Count; ++i)
            {
                var lerpedPos = DVector3.Lerp(aPose.Transforms[i].Position, bPose.Transforms[i].Position, alpha);
                var lerpedRot = Quaternion.Slerp(aPose.Transforms[i].Quat, bPose.Transforms[i].Quat, alpha);
                outPose.Transforms[i] = FTransform.CreateTransform(lerpedPos, Vector3.One, lerpedRot);
            }
            // RootMotion与Pose用同一个权重混合, 否则位移会与脑袋动画对不上
            outPose.RootMotion = TtRootMotionUtil.Blend(aPose.RootMotion, bPose.RootMotion, alpha);
        }
        /// <summary>
        /// 多 pose 加权混合。位置为加权求和, 旋转为 NLERP 加权平均(加权求和后归一化)。
        /// 权重归一化(和为 1)由调用方保证, 与 UE AccumulateWeightedTransform 语义一致。
        /// scale 统一输出 Vector3.One, 与本文件其它 pose 运算的约定保持一致。
        /// </summary>
        public static void BlendPoses<T>(ref T outPose, List<T> poses, List<float> weights) where T : IRuntimePose
        {
            System.Diagnostics.Debug.Assert(poses.Count > 0);
            if (poses.Count == 0)
                return;
            System.Diagnostics.Debug.Assert(weights.Count >= poses.Count);

            int boneCount = poses[0].Transforms.Count;
            for (int boneIndex = 0; boneIndex < boneCount; ++boneIndex)
            {
                // 累加器必须每根骨骼归零, 否则会把前面所有骨骼的结果累进来
                DVector3 pos = DVector3.Zero;
                Quaternion rot = new Quaternion(0, 0, 0, 0);
                bool hasRefQuat = false;
                Quaternion refQuat = Quaternion.Identity;

                for (int poseIndex = 0; poseIndex < poses.Count; ++poseIndex)
                {
                    float weight = weights[poseIndex];
                    var transform = poses[poseIndex].Transforms[boneIndex];
                    pos += transform.Position * weight;

                    // 四元数加权平均: 先做符号对齐(q 与 -q 表示同一旋转, 不对齐会互相抵消),
                    // 再加权求和, 最后归一化。不能用四元数乘法累积 —— 那是旋转复合而非加权平均。
                    // 参考系取第一个权重非零的 pose: 锚应当是实际参与混合的旋转。
                    var quat = transform.Quat;
                    if (!hasRefQuat)
                    {
                        if (weight != 0.0f)
                        {
                            refQuat = quat;
                            hasRefQuat = true;
                        }
                    }
                    else if (Quaternion.Dot(refQuat, quat) < 0.0f)
                    {
                        quat = Quaternion.MultiplyFloat(in quat, -1.0f);
                    }
                    rot = Quaternion.Add(in rot, Quaternion.MultiplyFloat(in quat, weight));
                }

                // 权重全为 0 或四元数相互抵消时长度近于 0, 直接 Normalize 会产生 NaN
                if (rot.LengthSquared() > 1e-8f)
                    rot = Quaternion.Normalize(rot);
                else
                    rot = Quaternion.Identity;

                outPose.Transforms[boneIndex] = FTransform.CreateTransform(pos, Vector3.One, rot);
            }

            // RootMotion按与Pose完全相同的权重做多路加权累加
            FRootMotionData rootMotion = FRootMotionData.Empty;
            bool hasRootMotionRefQuat = false;
            Quaternion rootMotionRefQuat = Quaternion.Identity;
            for (int poseIndex = 0; poseIndex < poses.Count; ++poseIndex)
            {
                TtRootMotionUtil.AccumulateWeighted(ref rootMotion, ref hasRootMotionRefQuat, ref rootMotionRefQuat, poses[poseIndex].RootMotion, weights[poseIndex]);
            }
            TtRootMotionUtil.FinishAccumulate(ref rootMotion);
            outPose.RootMotion = rootMotion;
        }
        public static void ZeroPose<T>(ref T descPose) where T : IRuntimePose
        {
            for (int i = 0; i < descPose.Transforms.Count; ++i)
            {
                descPose.Transforms[i] = FTransform.Identity;
            }
            descPose.RootMotion = FRootMotionData.Empty;
        }
        public static void ZeroPosePosition<T>(ref T descPose) where T : IRuntimePose
        {
            for (int i = 0; i < descPose.Transforms.Count; ++i)
            {
                var transform = descPose.Transforms[i];
                transform.Position = DVector3.Zero;
                descPose.Transforms[i] = transform;
            }
        }
        public static void AddPoses(ref TtLocalSpaceRuntimePose outPose, TtLocalSpaceRuntimePose lPose, TtLocalSpaceRuntimePose rPose)
        {
            System.Diagnostics.Debug.Assert(lPose.Transforms.Count == rPose.Transforms.Count);
            System.Diagnostics.Debug.Assert(lPose.Transforms.Count == outPose.Transforms.Count);
            for (int i = 0; i < outPose.Transforms.Count; ++i)
            {
                FTransform lTransform = lPose.Transforms[i];
                FTransform rTransform = rPose.Transforms[i];
                FTransform outTransform = FTransform.Identity;
                FTransform.Multiply(out outTransform, lTransform, rTransform);
                outPose.Transforms[i] = outTransform;
            }
        }
        public static void AddPoses(ref TtLPosMRotRuntimePose outPose, TtLPosMRotRuntimePose basePose, TtLPosMRotRuntimePose additivePose, float alpha)
        {
            System.Diagnostics.Debug.Assert(basePose.Transforms.Count == additivePose.Transforms.Count);
            System.Diagnostics.Debug.Assert(basePose.Transforms.Count == outPose.Transforms.Count);
            for (int i = 0; i < outPose.Transforms.Count; ++i)
            {
                FTransform baseBone = basePose.Transforms[i];
                FTransform additiveBone = additivePose.Transforms[i];
                var quat = additiveBone.Quat * baseBone.Quat;
                var pos = baseBone.Position + additiveBone.Position;
                var lerpedPos = DVector3.Lerp(baseBone.Position, pos, alpha);
                var lerpedRot = Quaternion.Slerp(baseBone.Quat, quat, alpha);
                lerpedRot.Normalize();
                outPose.Transforms[i] = FTransform.CreateTransform(lerpedPos, Vector3.One, lerpedRot);
            }
        }
        public static void MinusPoses(ref TtLPosMRotRuntimePose outPose, TtLPosMRotRuntimePose minusPose, TtLPosMRotRuntimePose minuendPose)
        {
            System.Diagnostics.Debug.Assert(minusPose.Transforms.Count == minuendPose.Transforms.Count);
            System.Diagnostics.Debug.Assert(minuendPose.Transforms.Count == outPose.Transforms.Count);
            for (int i = 0; i < outPose.Transforms.Count; ++i)
            {
                FTransform minusBone = minusPose.Transforms[i];
                FTransform minuendBone = minuendPose.Transforms[i];
                var position = minuendBone.Position - minusBone.Position;
                var rotation = minuendBone.Quat * minusBone.Quat.Inverse();
                rotation.Normalize();

                outPose.Transforms[i] = FTransform.CreateTransform(position, Vector3.One, rotation);
            }
        }

        /// <summary>
        /// RootMotion提取后将输出Pose的根骨骼锁定, 防止位移被应用两次(一次骨骼一次Actor)。
        /// firstFrameTransform仅在AnimFirstFrame模式下使用。
        /// </summary>
        public static void ApplyRootLock<T>(ref T pose, RootMotion.ERootMotionRootLock rootLock, in FTransform firstFrameTransform) where T : IRuntimePose
        {
            var rootIndex = GetRoot(pose);
            if (!rootIndex.IsValid())
                return;

            FTransform lockedTransform;
            switch (rootLock)
            {
                case RootMotion.ERootMotionRootLock.RefPose:
                    {
                        var initMatrix = pose.Descs[rootIndex.Value].InitMatrix;
                        lockedTransform = FTransform.CreateTransform(initMatrix.Translation.AsDVector(), initMatrix.Scale, initMatrix.Rotation);
                    }
                    break;
                case RootMotion.ERootMotionRootLock.AnimFirstFrame:
                    lockedTransform = firstFrameTransform;
                    break;
                default:
                    lockedTransform = FTransform.Identity;
                    break;
            }
            pose.Transforms[rootIndex.Value] = lockedTransform;
        }
    }


}
