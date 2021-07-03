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
            public string TargetPath;
            public RName.ERNameType TargetType;
        }
        public List<UAssetModifier> Modifiers { get; set; } = new List<UAssetModifier>();
        public Dictionary<RName, IAsset> mDirtyAssets = new Dictionary<RName, IAsset>();
        public void AddModifier(UAssetModifier mdf)
        {
            
        }
        public void Execute()
        {
            foreach (var i in Modifiers)
            {
                var ameta = UEngine.Instance.AssetMetaManager.GetAssetMeta(i.Source);
                if(i.Source!= ameta.GetAssetName())
                {
                    System.Diagnostics.Debug.Assert(false);
                    continue;
                }
                i.Source.Name = i.TargetPath;
                i.Source.RNameType = i.TargetType;
                foreach (var j in ameta.RefAssetRNames)
                {
                    if (mDirtyAssets.ContainsKey(j))
                        continue;
                    IAsset dirtyAsset = null;
                    mDirtyAssets.Add(j, dirtyAsset);
                }
                IAsset asset = null;// ameta.GetAssetName();
                asset.SaveAssetTo(ameta.GetAssetName());
                //IO.FileManager.DeleteAsset
            }
            foreach (var i in Modifiers)
            {
                var nameSets = RName.RNameManager.Instance.mNameSets[(int)i.Source.RNameType];
                nameSets.Remove(i.Source.Name);
                nameSets.Add(i.Source.Name, i.Source);
            }
            foreach (var i in mDirtyAssets)
            {
                i.Value.SaveAssetTo(i.Key);
            }
        }
    }
}
