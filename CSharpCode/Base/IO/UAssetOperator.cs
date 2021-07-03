using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.IO
{
    public class UAssetOperator
    {
        public struct UAssetModifier
        {
            public RName Source;
            public string SourcePath;
            public RName.ERNameType SourceType;
            public string TargetPath;
            public RName.ERNameType TargetType;
        }
        public List<UAssetModifier> Modifiers { get; set; } = new List<UAssetModifier>();
        public Dictionary<RName, IAsset> mDirtyAssets = new Dictionary<RName, IAsset>();
        public bool AddModifier(RName source, string targetPath, RName.ERNameType targetType)
        {
            foreach(var i in Modifiers)
            {
                if (i.Source == source)
                    return false;
            }

            if (source.Name == targetPath && source.RNameType == targetType)
                return false;

            var address = RName.GetAddress(targetType, targetPath) + ".ameta";
            if (IO.FileManager.FileExists(address))
            {//不支持覆盖
                return false;
            }

            UAssetModifier mdf = new UAssetModifier();
            mdf.Source = source;
            mdf.SourcePath = source.Name;
            mdf.SourceType = source.RNameType;
            mdf.TargetPath = targetPath;
            mdf.TargetType = targetType;

            Modifiers.Add(mdf);
            return true;
        }
        public async System.Threading.Tasks.Task Execute()
        {
            foreach (var i in Modifiers)
            {
                var ameta = UEngine.Instance.AssetMetaManager.GetAssetMeta(i.Source);
                if(i.Source!= ameta.GetAssetName())
                {
                    System.Diagnostics.Debug.Assert(false);
                    continue;
                }
                foreach (var j in ameta.RefAssetRNames)
                {
                    if (mDirtyAssets.ContainsKey(j))
                        continue;

                    var ameta_dirty = UEngine.Instance.AssetMetaManager.GetAssetMeta(j);

                    IAsset dirtyAsset = await ameta_dirty.LoadAsset();
                    mDirtyAssets.Add(j, dirtyAsset);
                }
            }
            foreach (var i in Modifiers)
            {
                var ameta = UEngine.Instance.AssetMetaManager.GetAssetMeta(i.Source);
                IAsset asset = await ameta.LoadAsset();
                
                var nameSets = RName.RNameManager.Instance.mNameSets[(int)i.Source.RNameType];
                nameSets.Remove(i.Source.Name);
                i.Source.Name = i.TargetPath;
                i.Source.RNameType = i.TargetType;
                nameSets.Add(i.Source.Name, i.Source);

                ameta.SaveAMeta();
                asset.SaveAssetTo(ameta.GetAssetName());
            }
            foreach (var i in mDirtyAssets)
            {
                i.Value.SaveAssetTo(i.Key);
            }
            foreach (var i in Modifiers)
            {
                var ameta = UEngine.Instance.AssetMetaManager.GetAssetMeta(i.Source);
                ameta.DeleteAsset(i.SourcePath, i.SourceType);
            }
            Modifiers.Clear();
            mDirtyAssets.Clear();
        }
    }
}
