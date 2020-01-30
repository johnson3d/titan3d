using EngineNS.Graphics.Mesh;
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using EditorCommon.ResourceInfos;

namespace EngineNS.Bricks.AssetImpExp.Creater
{
    public class CGfxAsset_MeshCreater : CGfxAssetCreater
    {
        CGfxMeshPrimitives mMeshPrimitives = null;
        public CGfxMeshPrimitives MeshPrimitives
        {
            get
            {
                return mMeshPrimitives;
            }
            set
            {
                mMeshPrimitives = value;
                //native set
                SDK_GfxAsset_MeshCreater_SetMeshPrimitives(CoreObject, value.CoreObject);
            }
        }

        public CGfxAsset_MeshCreater() : base("GfxAsset_MeshCreater")
        {

        }
        public override void Init()
        {
            var option = AssetImportOption as CGfxMeshImportOption;
            var name = option.Name;
            MeshPrimitives = new EngineNS.Graphics.Mesh.CGfxMeshPrimitives();
            var fullPath = RName.EditorOnly_GetRNameFromAbsFile(AssetImportOption.AbsSavePath + "/" + name + CEngineDesc.MeshSourceExtension);
            MeshPrimitives.Init(CEngine.Instance.RenderContext, fullPath, option.RenderAtom);


        }
        AssetImportMessageType mOperationResult = AssetImportMessageType.AMT_UnKnown;
        RName mVmsAbsFilePath = RName.EmptyName;
        RName mGmsAbsFilePath = RName.EmptyName;
        RName mPhyAbsFilePath = RName.EmptyName;
        bool CheckPhyGeomIfNeedImport(CGfxMeshImportOption option, string phyGeomType)
        {
            mPhyAbsFilePath = RName.EditorOnly_GetRNameFromAbsFile(CEngine.Instance.FileManager.RemoveExtension(MeshPrimitives.Name.Address) + phyGeomType);
            if (EngineNS.CEngine.Instance.FileManager.FileExists(mPhyAbsFilePath.Address))
            {
                var userDesire = _OnCreaterAssetImportMessageDumping(this, AssetImportMessageType.AMT_FileExist, 0, "Save", ImportPercent);
                mOperationResult = FileOperation(userDesire, mPhyAbsFilePath);
            }
            if (mOperationResult == AssetImportMessageType.AMT_IgnoreFile)
            {
                ImportPercent = 1.0f;
                _OnCreaterAssetImportMessageDumping(this, AssetImportMessageType.AMT_Import, 0, "Skip", ImportPercent);
                return false;
            }
            if (mOperationResult == AssetImportMessageType.AMT_RenameFile)
            {
                int index = GetValidFileNameSuffix(AssetImportOption.AbsSavePath, option.Name, phyGeomType);
                mVmsAbsFilePath = RName.EditorOnly_GetRNameFromAbsFile(AssetImportOption.AbsSavePath + "/" + option.Name + index.ToString() + phyGeomType);
                mGmsAbsFilePath = RName.EditorOnly_GetRNameFromAbsFile(AssetImportOption.AbsSavePath + "/" + option.Name + index.ToString() + phyGeomType);
                return true;
            }
            if (mOperationResult == AssetImportMessageType.AMT_DeleteOriginFile)
            {
                return true;
            }
            return true;
        }
        public override bool CheckIfNeedImport()
        {
            var option = AssetImportOption as CGfxMeshImportOption;

            if (option.AsPhyGemoConvex || option.AsPhyGemoTri)
            {
                if (option.AsPhyGemoConvex)
                {
                    return CheckPhyGeomIfNeedImport(option, CEngineDesc.PhyConvexGeom);
                }
                else if (option.AsPhyGemoTri)
                {
                    return CheckPhyGeomIfNeedImport(option, CEngineDesc.PhyTriangleMeshGeom);
                }
                else if (option.AsPhyGemoHeightField)
                {
                    return CheckPhyGeomIfNeedImport(option, CEngineDesc.PhyHeightFieldGeom);
                }
            }
            else
            {
                mVmsAbsFilePath = MeshPrimitives.Name;
                mGmsAbsFilePath = RName.EditorOnly_GetRNameFromAbsFile(AssetImportOption.AbsSavePath + "/" + option.Name + CEngineDesc.MeshExtension);
                if (EngineNS.CEngine.Instance.FileManager.FileExists(mVmsAbsFilePath.Address))
                {
                    var userDesire = _OnCreaterAssetImportMessageDumping(this, AssetImportMessageType.AMT_FileExist, 0, "Save", ImportPercent);
                    mOperationResult = FileOperation(userDesire, mVmsAbsFilePath);
                    if (option.CreateGms)
                        FileOperation(userDesire, mGmsAbsFilePath);
                }
                if (mOperationResult == AssetImportMessageType.AMT_IgnoreFile)
                {
                    ImportPercent = 1.0f;
                    _OnCreaterAssetImportMessageDumping(this, AssetImportMessageType.AMT_Import, 0, "Skip", ImportPercent);
                    return false;
                }
                if (mOperationResult == AssetImportMessageType.AMT_RenameFile)
                {
                    int index = GetValidFileNameSuffix(AssetImportOption.AbsSavePath, option.Name, CEngineDesc.MeshSourceExtension);
                    mVmsAbsFilePath = RName.EditorOnly_GetRNameFromAbsFile(AssetImportOption.AbsSavePath + "/" + option.Name + index.ToString() + CEngineDesc.MeshSourceExtension);
                    mGmsAbsFilePath = RName.EditorOnly_GetRNameFromAbsFile(AssetImportOption.AbsSavePath + "/" + option.Name + index.ToString() + CEngineDesc.MeshExtension);
                    return true;
                }
                if (mOperationResult == AssetImportMessageType.AMT_DeleteOriginFile)
                {
                    return true;
                }
            }
            return true;
        }
        public async System.Threading.Tasks.Task SavePhyConvexAsset()
        {
            var option = AssetImportOption as CGfxMeshImportOption;
            var rc = CEngine.Instance.RenderContext;
            MeshPrimitives.CookAndSavePhyiscsGeomAsConvex(CEngine.Instance.RenderContext, EngineNS.CEngine.Instance.PhyContext, mPhyAbsFilePath.Address);
            var info = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromResource(mPhyAbsFilePath.Address);
            await info.Save();

        }
        public async System.Threading.Tasks.Task SavePhyTriMeshAsset()
        {
            var option = AssetImportOption as CGfxMeshImportOption;
            var rc = CEngine.Instance.RenderContext;
            MeshPrimitives.CookAndSavePhyiscsGeomAsTriMesh(CEngine.Instance.RenderContext, EngineNS.CEngine.Instance.PhyContext, mPhyAbsFilePath.Address);
            var info = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromResource(mPhyAbsFilePath.Address);
            await info.Save();

        }
        public async System.Threading.Tasks.Task SavePhyHeightFieldAsset()
        {
            return;
            //var option = AssetImportOption as CGfxMeshImportOption;
            //var rc = CEngine.Instance.RenderContext;
            //MeshPrimitives.CookAndSavePhyiscsGeomAsTriMesh(CEngine.Instance.RenderContext, EngineNS.CEngine.Instance.PhyContext, mPhyAbsFilePath.Address);
            //var info = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromResource(mPhyAbsFilePath.Address);
            //await info.Save();

        }
        public override async System.Threading.Tasks.Task SaveAsset()
        {
            var option = AssetImportOption as CGfxMeshImportOption;
            var rc = CEngine.Instance.RenderContext;
            if (option.AsPhyGemoConvex || option.AsPhyGemoTri|| option.AsPhyGemoHeightField)
            {
                if (option.AsPhyGemoConvex)
                    await SavePhyConvexAsset();
                else if (option.AsPhyGemoTri)
                    await SavePhyTriMeshAsset();
                else if (option.AsPhyGemoHeightField)
                    await SavePhyHeightFieldAsset();
            }
            else
            {
                MeshPrimitives.MdfQueue.SyncNativeModifiers();
                string skeletonAssetName = "";
                if (option.HaveSkin)
                {
                    var skinModifier = MeshPrimitives.MdfQueue.FindModifier<CGfxSkinModifier>();
                    if (skinModifier != null)
                    {
                        if (option.Skeleton != RName.EmptyName)
                        {
                            skinModifier.SkeletonAsset = option.Skeleton.Name;
                            skeletonAssetName = option.Skeleton.Name;
                        }
                        else
                        {
                            var pureName = CEngine.Instance.FileManager.GetPureFileFromFullName(mVmsAbsFilePath.Name, false);
                            var path = AssetImportOption.AbsSavePath + "/" + pureName + CEngineDesc.SkeletonExtension;
                            var assetName = RName.EditorOnly_GetRNameFromAbsFile(path);
                            EngineNS.CEngine.Instance.SkeletonAssetManager.Remove(assetName);
                            var skeletonAsset = new Animation.Skeleton.CGfxSkeleton(SDK_GfxAsset_MeshCreater_GetFullSkeleton(CoreObject));
                            CEngine.Instance.SkeletonAssetManager.CreateSkeletonAsset(CEngine.Instance.RenderContext, assetName,skeletonAsset);
                            skinModifier.SkeletonAsset = assetName.Name;
                            skeletonAssetName = assetName.Name;
                            var skeletonRInfo = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromResource(assetName.Address);
                            var skeInfo = skeletonRInfo as SkeletonResourceInfo;
                            skeInfo.PreViewMesh = mGmsAbsFilePath.Name;
                            await skeletonRInfo.Save();
                        }
                    }
                }
                CEngine.Instance.MeshPrimitivesManager.RemoveMeshPimitives(mVmsAbsFilePath);
                MeshPrimitives.SaveMesh(mVmsAbsFilePath.Address);
                var info = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromResource(mVmsAbsFilePath.Address);
                var msRInfo = info as EditorCommon.ResourceInfos.MeshSourceResourceInfo;
                msRInfo.SkeletonAsset = skeletonAssetName;
                await info.Save();

                if (option.CreateGms)
                {
                    //OnResourceImportCheck(this, AsseetImportType.AIT_Import, "CreateMesh");
                    var mesh = CEngine.Instance.MeshManager.CreateMesh(rc, mGmsAbsFilePath, MeshPrimitives);
                    CEngine.Instance.MeshManager.RemoveMesh(mGmsAbsFilePath);
                    mesh.Init(rc, mGmsAbsFilePath, MeshPrimitives);
                    var mtl = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, RName.GetRName("Material/defaultmaterial.instmtl"));
                    for (int i = 0; i < mesh.MtlMeshArray.Length; ++i)
                    {
                        await mesh.SetMaterialInstanceAsync(rc, (uint)i, mtl, CEngine.Instance.PrebuildPassData.DefaultShadingEnvs);
                    }
                    mesh.SaveMesh();
                    info = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromResource(mGmsAbsFilePath.Address);
                    var meshRInfo = info as EditorCommon.ResourceInfos.MeshResourceInfo;
                    meshRInfo.SkeletonAsset = skeletonAssetName;
                    await info.Save();
                }
                ImportPercent = 1.0f;
                _OnCreaterAssetImportMessageDumping(this, AssetImportMessageType.AMT_Import, 0, "Save", ImportPercent);
                _OnCreaterAssetImportMessageDumping(this, AssetImportMessageType.AMT_Save, 0, "Save", 0);
            }
        }

        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxAsset_MeshCreater_SetMeshPrimitives(NativePointer self, CGfxMeshPrimitives.NativePointer meshPrimitives);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static Animation.Skeleton.CGfxSkeleton.NativePointer SDK_GfxAsset_MeshCreater_GetFullSkeleton(NativePointer self);
        #endregion

    }
}
