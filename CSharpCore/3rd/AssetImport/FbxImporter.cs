using EngineNS;
using EngineNS.Bricks.Animation.AnimNode;
using EngineNS.Bricks.Animation.Skeleton;
using EngineNS.Graphics.Mesh;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
namespace AssetImport
{
    public class FbxImporter : ImporterBase
    {
        public static new string Ext = "fbx";
        public FbxImporter()
        {
            mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::FbxImportor");
        }
        public override async System.Threading.Tasks.Task<bool> Import(/*RName fileName, RName savePath, ImportOption option, uint flags*/)
        {
            var rc = EngineNS.CEngine.Instance.RenderContext;
            for (uint i = 0; i < mMeshNum; i++)
            {
                await CreateVmsAndGms(rc, i);
            }
            for (uint i = 0; i < mActionsNum; i++)
            {
                await CreateAnimationSequence(rc, i);
            }
            for (uint i = 0; i < mMaterialsNum; i++)
            {

            }
            OnResourceImportCheck(this, AsseetImportType.AIT_Import, "ImportComplete");
            return true;
        }

        protected override async System.Threading.Tasks.Task CreateVmsAndGms(CRenderContext rc, uint meshIndex)
        {
            if (!mImportOption.ImportMesh)
                return;
            //导入不经过MeshPrimitivesManager
            //var meshPrimitive = CEngine.Instance.MeshPrimitivesManager.NewMeshPrimitives(rc,, 1);
            //meshPrimitive.Name

            var namePtr = SDK_AssetImporter_GetMeshNameByIndex(mCoreObject, meshIndex);
            var ansiString = Marshal.PtrToStringAnsi(namePtr);
            if (MeshPrimitives.ContainsKey(ansiString))
            {
                ansiString += meshIndex.ToString();
            }
            var atom = SDK_AssetImporter_GetMesAtomByIndex(mCoreObject, meshIndex);
            var meshPrimitive = new EngineNS.Graphics.Mesh.CGfxMeshPrimitives();
            var fullPath = RName.EditorOnly_GetRNameFromAbsFile(mAbsSavePath + "/" + ansiString + CEngineDesc.MeshSourceExtension);
            CEngine.Instance.MeshPrimitivesManager.RemoveMeshPimitives(fullPath);
            if (Async)
            {
                await CEngine.Instance.EventPoster.Post(async () =>
                {
                    OnResourceImportCheck(this, AsseetImportType.AIT_Import, "CreateMeshPrimitive");
                    meshPrimitive.Init(rc, fullPath, atom);
                    SDK_AssetImporter_GetMeshPrimitiveByIndex(mCoreObject, rc.CoreObject, meshIndex, meshPrimitive.CoreObject);
                    var skinNativePointer = SDK_AssetImporter_BuildMeshPrimitiveSkinModifierByIndex(mCoreObject, rc.CoreObject, meshIndex, meshPrimitive.CoreObject);
                    if (skinNativePointer.GetPointer() != IntPtr.Zero)
                    {
                        var skinModifier = new CGfxSkinModifier(skinNativePointer);
                        var nativeSkeleton = CGfxSkinModifier.SDK_GfxSkinModifier_GetSkeleton(skinNativePointer);
                        CGfxSkeleton skeleton = new CGfxSkeleton(nativeSkeleton);
                        EngineNS.Bricks.Animation.Pose.CGfxSkeletonPose pose = new EngineNS.Bricks.Animation.Pose.CGfxSkeletonPose();
                        for (uint i = 0; i < skeleton.BoneTab.BoneNumberNative; ++i)
                        {
                            pose.NewBone(skeleton.BoneTab.GetBoneNative(i).BoneDescNative);
                        }
                        CGfxSkeleton csSkeleton = new CGfxSkeleton();
                        csSkeleton.BoneTab = pose;
                        skinModifier.Skeleton = csSkeleton;
                        meshPrimitive.MdfQueue.AddModifier(skinModifier);
                        if (mImportOption.SkeletonAssetName != null && mImportOption.SkeletonAssetName != RName.EmptyName)
                        {
                            var skeletonAsset = EngineNS.CEngine.Instance.SkeletonAssetManager.GetSkeleton(rc, mImportOption.SkeletonAssetName, false);
                            skinModifier.SkeletonAsset = mImportOption.SkeletonAssetName.Name;
                            mSkeletonAsset = mImportOption.SkeletonAssetName.Name;
                        }
                        else
                        {
                            OnResourceImportCheck(this, AsseetImportType.AIT_Import, "CreateSkeleton");
                            var pureName = CEngine.Instance.FileManager.GetPureFileFromFullName(mAbsFileName, false);
                            var path = mAbsSavePath + "/" + pureName + CEngineDesc.SkeletonExtension;
                            var assetName = RName.EditorOnly_GetRNameFromAbsFile(path);
                            var skeletonAsset = EngineNS.CEngine.Instance.SkeletonAssetManager.CreateSkeletonAsset(rc, assetName, skinModifier);
                            skinModifier.SkeletonAsset = assetName.Name;
                            mSkeletonAsset = assetName.Name;
                            if (!mSkeletonNeedInfo.ContainsKey(pureName))
                                mSkeletonNeedInfo.Add(pureName, assetName);
                        }
                    }
                    MeshPrimitives.Add(ansiString, meshPrimitive);
                    if (mImportOption.AsPhyGemoConvex || mImportOption.AsPhyGemoTri)
                    {
                        foreach (var i in MeshPrimitives.Values)
                        {
                            CGfxMeshPrimitives meshprimitive = i as CGfxMeshPrimitives;
                            if (mImportOption.AsPhyGemoConvex)
                                meshprimitive.CookAndSavePhyiscsGeomAsConvex(CEngine.Instance.RenderContext, EngineNS.CEngine.Instance.PhyContext);
                            else if (mImportOption.AsPhyGemoTri)
                                meshprimitive.CookAndSavePhyiscsGeomAsTriMesh(CEngine.Instance.RenderContext, EngineNS.CEngine.Instance.PhyContext);
                        }
                    }
                    else
                    {
                        await CEngine.Instance.EventPoster.Post(() =>
                        {
                            meshPrimitive.SaveMesh();
                            return true;
                        }, EngineNS.Thread.Async.EAsyncTarget.Render);
                        //gms
                        if (mImportOption.CreatedGms)
                        {
                            OnResourceImportCheck(this, AsseetImportType.AIT_Import, "CreateMesh");
                            var meshRName = RName.EditorOnly_GetRNameFromAbsFile(mAbsSavePath + "/" + ansiString + CEngineDesc.MeshExtension);
                            var mesh = CEngine.Instance.MeshManager.CreateMesh(rc, meshRName, meshPrimitive);
                            CEngine.Instance.MeshManager.RemoveMesh(meshRName);
                            mesh.Init(rc, meshRName, meshPrimitive);
                            var mtl = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, RName.GetRName("Material/defaultmaterial.instmtl"));
                            for (int i = 0; i < mesh.MtlMeshArray.Length; ++i)
                            {
                                await mesh.SetMaterialInstanceAsync(rc, (uint)i, mtl, CEngine.Instance.PrebuildPassData.DefaultShadingEnvs);
                            }
                            Meshes.Add(ansiString, mesh);
                            mesh.SaveMesh();
                        }
                    }
                    return true;
                }, EngineNS.Thread.Async.EAsyncTarget.AsyncEditor);
            }
            else
            {
                OnResourceImportCheck(this, AsseetImportType.AIT_Import, "CreateMeshPrimitive");
                meshPrimitive.Init(rc, fullPath, atom);
                SDK_AssetImporter_GetMeshPrimitiveByIndex(mCoreObject, rc.CoreObject, meshIndex, meshPrimitive.CoreObject);
                var skinNativePointer = SDK_AssetImporter_BuildMeshPrimitiveSkinModifierByIndex(mCoreObject, rc.CoreObject, meshIndex, meshPrimitive.CoreObject);
                if (skinNativePointer.GetPointer() != IntPtr.Zero)
                {
                    var skinModifier = new CGfxSkinModifier(skinNativePointer);
                    meshPrimitive.MdfQueue.AddModifier(skinModifier);
                    if (mImportOption.SkeletonAssetName != null)
                    {
                        var skeletonAsset = EngineNS.CEngine.Instance.SkeletonAssetManager.GetSkeleton(rc, mImportOption.SkeletonAssetName, false);
                        skinModifier.SkeletonAsset = mImportOption.SkeletonAssetName.Name;
                    }
                    else
                    {

                        var pureName = CEngine.Instance.FileManager.GetPureFileFromFullName(mAbsFileName, false);
                        var path = mAbsSavePath + "/" + pureName + CEngineDesc.SkeletonExtension;
                        var assetName = RName.EditorOnly_GetRNameFromAbsFile(path);
                        var skeletonAsset = EngineNS.CEngine.Instance.SkeletonAssetManager.CreateSkeletonAsset(rc, assetName, skinModifier);
                        skinModifier.SkeletonAsset = assetName.Name;
                        if (!mSkeletonNeedInfo.ContainsKey(pureName))
                            mSkeletonNeedInfo.Add(pureName, assetName);
                    }
                }
                MeshPrimitives.Add(ansiString, meshPrimitive);
                meshPrimitive.SaveMesh();

                //gms
                if (mImportOption.CreatedGms)
                {
                    OnResourceImportCheck(this, AsseetImportType.AIT_Import, "CreateMes");
                    var meshRName = RName.EditorOnly_GetRNameFromAbsFile(mAbsSavePath + "/" + ansiString + CEngineDesc.MeshExtension);
                    var mesh = CEngine.Instance.MeshManager.CreateMesh(rc, meshRName, meshPrimitive);
                    CEngine.Instance.MeshManager.RemoveMesh(meshRName);
                    mesh.Init(rc, meshRName, meshPrimitive);
                    var mtl = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, RName.GetRName("Material/defaultmaterial.instmtl"));
                    for (int i = 0; i < mesh.MtlMeshArray.Length; ++i)
                    {
                        await mesh.SetMaterialInstanceAsync(rc, (uint)i, mtl, CEngine.Instance.PrebuildPassData.DefaultShadingEnvs);
                    }
                    Meshes.Add(ansiString, mesh);
                    mesh.SaveMesh();
                }
            }
            //foreach(var mp in MeshPrimitives)
            //{
            //    mp.Value.SaveMesh();
            //}
            //foreach(var mesh in Meshes)
            //{
            //    mesh.Value.SaveMesh();
            //}
            //var info = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromResource(fullPath.Address);
            //info.Save();
            return;
        }

        protected override async System.Threading.Tasks.Task CreateAnimationSequence(CRenderContext rc, uint actionIndex)
        {
            if (!mImportOption.ImportAnimation)
                return;
            var namePtr = SDK_AssetImporter_GetActionNameByIndex(mCoreObject, actionIndex);
            var ansiString = Marshal.PtrToStringAnsi(namePtr);
            ansiString = CEngine.Instance.FileManager.GetPureFileFromFullName(mAbsFileName, false);
            //var skeletonAction = new EngineNS.Graphics.Mesh.Skeleton.CGfxSkeletonAction();
            //skeletonAction.Init(rc, RName.GetRName(savePath.Name + "/" + ansiString + CEngineDesc.AnimationSegementExtension));
            //SDK_AssetImporter_GetSkeletonActionByIndex(mCoreObject, rc.CoreObject, i, skeletonAction.CoreObject);

            var animSegement = new CGfxAnimationSequence();
            if (mImportOption.SkeletonAssetName != null && mImportOption.SkeletonAssetName != RName.EmptyName)
            {
                mSkeletonAsset = mImportOption.SkeletonAssetName.Name;
            }
            if (Async)
            {
                await CEngine.Instance.EventPoster.Post(() =>
                {
                    OnResourceImportCheck(this, AsseetImportType.AIT_Import, "CreateAnimationSequence");
                    animSegement.SkeletonAction = new CGfxSkeletonAction();
                    SDK_AssetImporter_GetSkeletonActionByIndex(mCoreObject, rc.CoreObject, actionIndex, animSegement.SkeletonAction.CoreObject);
                    var name = RName.EditorOnly_GetRNameFromAbsFile(mAbsSavePath + "/" + ansiString + CEngineDesc.AnimationSequenceExtension);
                    animSegement.SkeletonAssetName = mImportOption.SkeletonAssetName;
                    var skeleton = EngineNS.CEngine.Instance.SkeletonAssetManager.GetSkeleton(rc, animSegement.SkeletonAssetName);
                    //animSegement.SkeletonAction.FixBoneTree(skeleton);
                    //animSegement.SkeletonAction.FixBoneAnimPose(skeleton);
                    SDK_AssetImporter_CaculateSkeletonActionMotionData(mCoreObject, rc.CoreObject, animSegement.SkeletonAction.CoreObject, skeleton.BoneTab.Clone().CoreObject);
                    animSegement.SaveAs(name);
                    Actions.Add(ansiString, animSegement);
                    return true;
                }, EngineNS.Thread.Async.EAsyncTarget.AsyncEditor);
            }
            else
            {
                OnResourceImportCheck(this, AsseetImportType.AIT_Import, "CreateAnimationSequence");
                animSegement.SkeletonAction = new CGfxSkeletonAction();
                SDK_AssetImporter_GetSkeletonActionByIndex(mCoreObject, rc.CoreObject, actionIndex, animSegement.SkeletonAction.CoreObject);
                var name = RName.EditorOnly_GetRNameFromAbsFile(mAbsSavePath + "/" + ansiString + CEngineDesc.AnimationSequenceExtension);
                animSegement.SkeletonAssetName = mImportOption.SkeletonAssetName;
                var skeleton = EngineNS.CEngine.Instance.SkeletonAssetManager.GetSkeleton(rc, animSegement.SkeletonAssetName);
                SDK_AssetImporter_CaculateSkeletonActionMotionData(mCoreObject, rc.CoreObject, animSegement.SkeletonAction.CoreObject, skeleton.BoneTab.Clone().CoreObject);
                animSegement.SkeletonAction.FixBoneTree(skeleton);
                animSegement.SkeletonAction.FixBoneAnimPose(skeleton);
                animSegement.SaveAs(name);
                Actions.Add(ansiString, animSegement);
            }
            return;
        }
    }
}
