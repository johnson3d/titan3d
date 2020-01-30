using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;
using EngineNS.Bricks.Animation.AnimElement;
using EngineNS.Bricks.Animation;
using System.Threading.Tasks;
using EngineNS.Bricks.Animation.AnimNode;
using EditorCommon.ResourceInfos;

namespace EngineNS.Bricks.AssetImpExp.Creater
{
    public class CGfxAsset_AnimationCreater : CGfxAssetCreater
    {
        Dictionary<uint, CGfxAnimationElement> mElement = new Dictionary<uint, CGfxAnimationElement>();
        public CGfxAsset_AnimationCreater() : base("GfxAsset_AnimationCreater")
        {

        }
        public override void Init()
        {

        }
        //AssetImportMessageType mOperationResult = AssetImportMessageType.AMT_UnKnown;
        RName mAnimAbsFilePath = RName.EmptyName;
        public override bool CheckIfNeedImport()
        {
            mAnimAbsFilePath = RName.EditorOnly_GetRNameFromAbsFile(AssetImportOption.AbsSavePath + "/" + AssetImportOption.Name + CEngineDesc.AnimationClipExtension);
            var operationResult = AssetImportMessageType.AMT_UnKnown;
            if (EngineNS.CEngine.Instance.FileManager.FileExists(mAnimAbsFilePath.Address))
            {
                var userDesire = _OnCreaterAssetImportMessageDumping(this, AssetImportMessageType.AMT_FileExist, 0, "FileExist", ImportPercent);
                operationResult = FileOperation(userDesire, mAnimAbsFilePath);
            }
            if (operationResult == AssetImportMessageType.AMT_IgnoreFile)
            {
                ImportPercent = 1.0f;
                _OnCreaterAssetImportMessageDumping(this, AssetImportMessageType.AMT_Import, 0, "Skip", ImportPercent);
                return false;
            }
            if (operationResult == AssetImportMessageType.AMT_RenameFile)
            {
                var validName = GetValidFileName(AssetImportOption.AbsSavePath, AssetImportOption.Name, CEngineDesc.AnimationClipExtension);
                mAnimAbsFilePath = RName.EditorOnly_GetRNameFromAbsFile(validName);
                return true;
            }
            if (operationResult == AssetImportMessageType.AMT_DeleteOriginFile)
            {
                return true;
            }
            return true;
        }
        public override async Task SaveAsset()
        {
            var animOption = AssetImportOption as CGfxAnimationImportOption;
            BuildOptionsDictionary();
            var clip = new Animation.AnimNode.AnimationClipInstance();
            clip.Elements = mElement;
            uint count = 0;
            using (var it = mElement.GetEnumerator())
            {
                while (it.MoveNext())
                {
                    if(count < it.Current.Value.GetKeyCount())
                    {
                        count = it.Current.Value.GetKeyCount();
                    }
                    if (it.Current.Value.ElementType == AnimationElementType.Default)
                    {
                        clip.ElementProperties.Add(new ElementProperty(ElementPropertyType.EPT_Property, it.Current.Value.Desc.Path));
                    }
                    else
                    {
                        clip.ElementProperties.Add(new ElementProperty(ElementPropertyType.EPT_Skeleton, animOption.Skeleton.Name));
                    }
                }
            }
            
            clip.KeyFrames = count;
            clip.Duration = animOption.Duration;
            clip.SampleRate = animOption.SampleRate;
            clip.SaveAs(mAnimAbsFilePath);
            var info = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromResource(mAnimAbsFilePath.Address);
            var msRInfo = info as EditorCommon.ResourceInfos.AnimationClipResourceInfo;
            msRInfo.ElementProperties = clip.ElementProperties;
            var skeletonInfo = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromFile(msRInfo.GetElementProperty(ElementPropertyType.EPT_Skeleton),null) as SkeletonResourceInfo;
            if(skeletonInfo != null)
                msRInfo.PreViewMesh = skeletonInfo.PreViewMesh;
            //msRInfo.SkeletonAsset = asset.SkeletonAsset;
            await msRInfo.Save();
            ImportPercent = 1.0f;
            _OnCreaterAssetImportMessageDumping(this, AssetImportMessageType.AMT_Import, 0, "Save", ImportPercent);
            _OnCreaterAssetImportMessageDumping(this, AssetImportMessageType.AMT_Save, 0, "Save", 0);
            EngineNS.CEngine.Instance.AnimationInstanceManager.Remove(mAnimAbsFilePath);
        }
        public void BuildOptionsDictionary()
        {
            var count = SDK_GfxAsset_AnimationCreater_GetElementCount(CoreObject);
            for (uint i = 0; i < count; ++i)
            {
                var option = GetAnimationElement(i, SDK_GfxAsset_AnimationCreater_GetElementType(CoreObject, i));
                option.SyncNative();
                mElement.Add(option.Desc.NameHash, option);
            }
        }
        CGfxAnimationElement GetAnimationElement(uint index, AnimationElementType type)
        {
            CGfxAnimationElement element = null;
            switch (type)
            {
                case AnimationElementType.Skeleton:
                    {
                        element = new CGfxSkeletonAnimationElement(SDK_GfxAsset_AnimationCreater_GetElement(CoreObject, index));
                    }
                    break;
                case AnimationElementType.Bone:
                    {
                        element = new CGfxBoneAnimationElement(SDK_GfxAsset_AnimationCreater_GetElement(CoreObject, index));
                    }
                    break;
                case AnimationElementType.Default:
                    {
                        element = new CGfxAnimationElement(SDK_GfxAsset_AnimationCreater_GetElement(CoreObject, index));
                    }
                    break;
                default:
                    break;
            }
            return element;
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static uint SDK_GfxAsset_AnimationCreater_GetElementCount(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static AnimationElementType SDK_GfxAsset_AnimationCreater_GetElementType(NativePointer self, uint index);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CGfxAnimationElement.NativePointer SDK_GfxAsset_AnimationCreater_GetElement(NativePointer self, uint index);
        #endregion
    }
}
