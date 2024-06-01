using System;
using EngineNS.IO;
using System.Collections.Generic;
using System.Text;
using EngineNS.Animation.SkeletonAnimation.Skeleton;

namespace EngineNS.Animation.Asset
{
    [Rtti.Meta]
    public partial class TtSkeletonAssetAMeta : IO.IAssetMeta
    {
        public override string GetAssetExtType()
        {
            return TtSkeletonAsset.AssetExt;
        }
        public override async System.Threading.Tasks.Task<IO.IAsset> LoadAsset()
        {
            return await UEngine.Instance.AnimationModule.SkeletonAssetManager.GetSkeletonAsset(GetAssetName());
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            return true;
        }
        public override string GetAssetTypeName()
        {
            return "Skeleton";
        }
    }
    [Rtti.Meta]
    //[UFullSkeleton.Import]
    public partial class TtSkeletonAsset :IO.BaseSerializer, IO.IAsset
    {
        [Rtti.Meta]
        public TtSkinSkeleton Skeleton { get; set; } = new TtSkinSkeleton();
        #region IO.IAsset
        public const string AssetExt = ".skt";
        [Rtti.Meta]
        public RName AssetName { get; set; }

        public IAssetMeta CreateAMeta()
        {
            var result = new TtSkeletonAssetAMeta();
            return result;

        }

        public IAssetMeta GetAMeta()
        {
            return UEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }

        public void SaveAssetTo(RName name)
        {
            AssetName = name;
            var typeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(this.GetType());
            var xnd = new IO.TtXndHolder(typeStr, 0, 0);
            using (var attr = xnd.NewAttribute("SkeletonAsset", 0, 0))
            {
                using (var ar = attr.GetWriter(512))
                {
                    ar.Write(this);
                }
                xnd.RootNode.AddAttribute(attr);
            }

            xnd.SaveXnd(name.Address);
            UEngine.Instance.SourceControlModule.AddFile(name.Address);
        }

        public void UpdateAMetaReferences(IAssetMeta ameta)
        {
            ameta.RefAssetRNames.Clear();
        }
        #endregion

        public static TtSkeletonAsset LoadXnd(TtSkeletonAssetManager manager, IO.TtXndNode node)
        {
            unsafe
            {
                IO.ISerializer result = null;
                var attr = node.TryGetAttribute("SkeletonAsset");
                if ((IntPtr)attr.CppPointer != IntPtr.Zero)
                {
                    using (var ar = attr.GetReader(manager))
                    {
                        ar.Read(out result, manager);
                    }
                }

                var asset = result as TtSkeletonAsset;
                if (asset != null)
                {
                    asset.Skeleton.ConstructHierarchy();
                    return asset;
                }
                return null;
            }
        }
    }

    public partial class TtSkeletonAssetManager
    {
        public Dictionary<RName, TtSkeletonAsset> SkeletonAssets { get; } = new Dictionary<RName, TtSkeletonAsset>();
        public async System.Threading.Tasks.Task<TtSkeletonAsset> GetSkeletonAsset(RName name)
        {
            TtSkeletonAsset result;
            if (SkeletonAssets.TryGetValue(name, out result))
                return result;

            result = await UEngine.Instance.EventPoster.Post((state) =>
            {
                using (var xnd = IO.TtXndHolder.LoadXnd(name.Address))
                {
                    if (xnd != null)
                    {
                        var skeletonAsset = TtSkeletonAsset.LoadXnd(this, xnd.RootNode);
                        if (skeletonAsset == null)
                            return null;

                        skeletonAsset.AssetName = name;
                        return skeletonAsset;
                    }
                    else
                    {
                        return null;
                    }
                }
            }, Thread.Async.EAsyncTarget.AsyncIO);

            if (result != null)
            {
                SkeletonAssets[name] = result;
                return result;
            }

            return null;
        }
    }
}
namespace EngineNS.Animation
{
    public partial class TtAnimationModule
    {
        public Asset.TtSkeletonAssetManager SkeletonAssetManager { get; } = new Asset.TtSkeletonAssetManager();
    }
}