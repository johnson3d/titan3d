using System;
using EngineNS.IO;
using System.Collections.Generic;
using System.Text;
using EngineNS.Animation.SkeletonAnimation.Skeleton;

namespace EngineNS.Animation.Asset
{
    [Rtti.Meta("")]
    public partial class TtSkeletonAssetAMeta : IO.IAssetMeta
    {
        public override string TypeExt
        {
            get => TtSkeletonAsset.AssetExt;
        }
        public override async Thread.Async.TtTask<IO.IAsset> LoadAsset(params object[] args)
        {
            return await TtEngine.Instance.AnimationModule.SkeletonAssetManager.GetSkeletonAsset(GetAssetName());
        }
        public override async Thread.Async.TtTask<IO.IAsset> CreateAsset(params object[] args)
        {
            return await TtEngine.Instance.AnimationModule.SkeletonAssetManager.CreateSkeletonAsset(GetAssetName());
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
    [Rtti.Meta("")]
    //[UFullSkeleton.Import]
    public partial class TtSkeletonAsset :IO.BaseSerializer, IO.IAsset
    {
        [Rtti.Meta("")]
        public TtSkinSkeleton Skeleton { get; set; } = new TtSkinSkeleton();
        #region IO.IAsset
        public const string AssetExt = ".skt";
        public string TypeExt { get => AssetExt; }
        [Rtti.Meta("")]
        public RName AssetName { get; set; }

        public IAssetMeta CreateAMeta()
        {
            var result = new TtSkeletonAssetAMeta();
            return result;

        }

        public IAssetMeta GetAMeta()
        {
            return TtEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }

        public void SaveAssetTo(RName name)
        {
            AssetName = name;
            var typeStr = Rtti.TtTypeDesc.TypeOf(this.GetType()).TypeString;
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
            name.AMeta.AddAssetFile(name.Address);
            TtEngine.Instance.SourceControlModule.AddFile(name.Address);
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
        public async Thread.Async.TtTask<TtSkeletonAsset> GetSkeletonAsset(RName name)
        {
            TtSkeletonAsset result;
            if (SkeletonAssets.TryGetValue(name, out result))
                return result;

            result = await CreateSkeletonAsset(name);

            if (result != null)
            {
                SkeletonAssets[name] = result;
                return result;
            }

            return null;
        }
        public async Thread.Async.TtTask<TtSkeletonAsset> CreateSkeletonAsset(RName name)
        {
            var result = await TtEngine.Instance.EventPoster.Post((state) =>
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

            return result;
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