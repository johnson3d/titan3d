using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using EngineNS.IO;

namespace EngineNS.Graphics.Mesh
{

    public class CGfxSkinModifier : CGfxModifier
    {
        public const UInt64 CoreClassId = 0x5fdefa164ffbc429;

        private string mSkeletonAsset;
        [EngineNS.Editor.Editor_PackDataAttribute]
        public string SkeletonAsset { get => mSkeletonAsset; set => mSkeletonAsset = value; }
        public CGfxSkinModifier()
        {
            mCoreObject = NewNativeObjectByNativeName<NativePointer>("GfxSkinModifier");
            Name = Rtti.RttiHelper.GetTypeSaveString(typeof(CGfxSkinModifier));
            ShaderModuleName = RName.GetRName("Shaders/Modifier/SkinModifier");

            SkinSkeleton = new Bricks.Animation.Skeleton.CGfxSkeleton();
        }
        public CGfxSkinModifier(NativePointer self) : base(self)
        {
            mCoreObject = self;
            Core_AddRef();
            Name = Rtti.RttiHelper.GetTypeSaveString(typeof(CGfxSkinModifier));
            ShaderModuleName = RName.GetRName("Shaders/Modifier/SkinModifier");
            //native本身自带 skeleton，在设置会顶替掉
            //Skeleton = new Bricks.Animation.Skeleton.CGfxSkeleton();
            mSkinSkeleton = new Bricks.Animation.Skeleton.CGfxSkeleton(SDK_GfxSkinModifier_GetSkeleton(self));
        }
        public override CGfxModifier CloneModifier(CRenderContext rc)
        {
            //var ptr = SDK_GfxModifier_CloneModifier(CoreObject, rc.CoreObject);
            var result = new CGfxSkinModifier();
            result.mSkeletonAsset = mSkeletonAsset;
            result.SkinSkeleton = mSkinSkeleton?.Clone();
            result.AnimationPose = mAnimationPose?.Clone();
            result.mMeshSpaceAnimPose = mMeshSpaceAnimPose?.Clone();
            result.mMeshSpaceSkeletonPose = mMeshSpaceSkeletonPose?.Clone();
            result.Name = Name;
            return result;
        }

        public override void Save2Xnd(XndNode node)
        {
            var sktAssetAtt = node.AddAttrib("SkeletonAssetName");
            sktAssetAtt.BeginWrite();
            sktAssetAtt.Write(SkeletonAsset);
            sktAssetAtt.EndWrite();
            var subSkeNode = node.AddNode("SubSkeleton", 0, 0);
            mSkinSkeleton.Save(subSkeNode);
            ///skeleton 也放在这里做保存
        }
        void InitPose()
        {

        }
        public override bool LoadXnd(IO.XndNode node)
        {
            var result = base.LoadXnd(node);
            if (result == false)
                return false;

            var attrib = node.FindAttrib("SkeletonAssetName");
            if (attrib != null)
            {
                attrib.BeginRead();
                attrib.Read(out mSkeletonAsset);
                attrib.EndRead();

                if (!string.IsNullOrEmpty(mSkeletonAsset))
                {
                    var skeleton = CEngine.Instance.SkeletonAssetManager.GetSkeleton(CEngine.Instance.RenderContext, RName.GetRName(mSkeletonAsset));
                    mAnimationPose = skeleton.CreateSkeletonPose();
                    mMeshSpaceAnimPose = mAnimationPose.Clone();
                }
            }
            var subSkeNode = node.FindNode("SubSkeleton");
            if (mSkinSkeleton.Load(subSkeNode, true))
            {
                if (mAnimationPose == null)
                {
                    mAnimationPose = mSkinSkeleton.CreateSkeletonPose();
                    mMeshSpaceAnimPose = mAnimationPose.Clone();
                }
            }
            return true;
        }
        public override async System.Threading.Tasks.Task<bool> LoadXndAsync(IO.XndNode node)
        {
            var ret = await CEngine.Instance.EventPoster.Post(() =>
            {
                var result = base.LoadXnd(node);
                if (result == false)
                    return false;

                bool skinSkeleton = false;
                Bricks.Animation.Pose.CGfxSkeletonPose skinPose = null;
                var subSkeNode = node.FindNode("SubSkeleton");
                if (mSkinSkeleton.Load(subSkeNode, true))
                {
                    skinSkeleton = true;
                    skinPose = mSkinSkeleton.CreateSkeletonPose();
                }

                var attrib = node.FindAttrib("SkeletonAssetName");
                if (attrib != null)
                {
                    attrib.BeginRead();
                    attrib.Read(out mSkeletonAsset);
                    attrib.EndRead();
                    if (!string.IsNullOrEmpty(mSkeletonAsset))
                    {
                        if (skinSkeleton)
                        {
                            var skeleton = CEngine.Instance.SkeletonAssetManager.GetSkeleton(CEngine.Instance.RenderContext, RName.GetRName(mSkeletonAsset));
                            mAnimationPose = skeleton.CreateSkeletonPose();
                            CorrectAnimationPose(mAnimationPose, skinPose);
                        }
                        else
                        {
                            mAnimationPose = skinPose;
                        }
                        mMeshSpaceAnimPose = mAnimationPose.Clone();
                    }
                }


                return true;
            });
            return ret;
        }
        public void CorrectAnimationPose(Bricks.Animation.Pose.CGfxSkeletonPose skeletonPose, Bricks.Animation.Pose.CGfxSkeletonPose skinPose)
        {
            if (skinPose == null)
                return;
            for (int i = 0; i < skinPose.BoneNumber; ++i)
            {
                var bonePose = skeletonPose.FindBonePose(skinPose.Bones[i].NameHash);
                if (bonePose != null)
                {
                    bonePose.Transform = skinPose.Bones[i].Transform;
                }
            }
        }
        protected Bricks.Animation.Pose.CGfxSkeletonPose mAnimationPose = null;
        public Bricks.Animation.Pose.CGfxSkeletonPose AnimationPose
        {
            get
            {
                return mAnimationPose;
            }
            set
            {
                mAnimationPose = value;
            }
        }

        protected Bricks.Animation.Skeleton.CGfxSkeleton mSkinSkeleton = null;
        //self Skeleton
        public Bricks.Animation.Skeleton.CGfxSkeleton SkinSkeleton
        {
            get
            {
                return mSkinSkeleton;
            }
            set
            {
                mSkinSkeleton = value;
                SDK_GfxSkinModifier_SetSkeleton(CoreObject, value.CoreObject);
            }
        }
        protected Bricks.Animation.Pose.CGfxSkeletonPose mMeshSpaceSkeletonPose = null;
        protected Bricks.Animation.Pose.CGfxSkeletonPose mMeshSpaceAnimPose = null;
        public Bricks.Animation.Pose.CGfxSkeletonPose MeshSpaceAnimPose
        {
            get => mMeshSpaceAnimPose;
            set => mMeshSpaceAnimPose = value;
        }
        public override string FunctionName
        {
            get { return "DoSkinModifierVS"; }
        }


        public override void TickLogic(CRenderContext rc, CGfxMesh mesh, Int64 time)
        {
            base.TickLogic(rc, mesh, time);
            SetToRenderStream(mesh);
        }

        static int AbsBonePosId = -1;
        static int AbsBoneQuatId = -1;
        public void SetToRenderStream(CGfxMesh mesh)
        {
            var cbuffer = mesh.CBuffer;
            if (AbsBonePosId == -1)
                AbsBonePosId = cbuffer.FindVar("AbsBonePos");
            if (AbsBoneQuatId == -1)
                AbsBoneQuatId = cbuffer.FindVar("AbsBoneQuat");
            if (!Bricks.Animation.Runtime.CGfxAnimationRuntime.IsZeroPose(mAnimationPose))
            {
                if (mMeshSpaceAnimPose == null)
                    mMeshSpaceAnimPose = mAnimationPose.Clone();
                Bricks.Animation.Runtime.CGfxAnimationRuntime.CopyPoseAndConvertMeshSpace(mMeshSpaceAnimPose, mAnimationPose);
                SDK_GfxSkinModifier_SetToRenderStream(CoreObject, cbuffer.CoreObject, AbsBonePosId, AbsBoneQuatId, mMeshSpaceAnimPose.CoreObject);
            }
            else
            {
                if (mMeshSpaceSkeletonPose == null)
                {
                    mMeshSpaceSkeletonPose = mSkinSkeleton.CreateSkeletonPose();
                    Bricks.Animation.Runtime.CGfxAnimationRuntime.ConvertToMeshSpace(mMeshSpaceSkeletonPose);
                }
                SDK_GfxSkinModifier_SetToRenderStream(CoreObject, cbuffer.CoreObject, AbsBonePosId, AbsBoneQuatId, mMeshSpaceSkeletonPose.CoreObject);
            }
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static Bricks.Animation.Skeleton.CGfxSkeleton.NativePointer SDK_GfxSkinModifier_GetSkeleton(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxSkinModifier_SetSkeleton(NativePointer self, Bricks.Animation.Skeleton.CGfxSkeleton.NativePointer skeleton);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxSkinModifier_SetToRenderStream(NativePointer self, CConstantBuffer.NativePointer cb, int AbsBonePos, int AbsBoneQuat, Bricks.Animation.Pose.CGfxSkeletonPose.NativePointer pose);
        #endregion
    }
}
