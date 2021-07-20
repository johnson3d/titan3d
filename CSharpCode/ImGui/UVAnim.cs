using System;
using System.Collections.Generic;
using EngineNS;

namespace EngineNS.EGui
{
    [Rtti.Meta]
    public partial class UVAnimAMeta : IO.IAssetMeta
    {
        public override async System.Threading.Tasks.Task<IO.IAsset> LoadAsset()
        {
            await Thread.AsyncDummyClass.DummyFunc();
            return null;
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            //必须是TextureAsset
            return true;
        }
        public override void OnDraw(ref ImDrawList cmdlist, ref Vector2 sz, EGui.Controls.ContentBrowser ContentBrowser)
        {
            base.OnDraw(ref cmdlist, ref sz, ContentBrowser);
        }
    }
    [Rtti.Meta]
    [UVAnim.Import]
    [IO.AssetCreateMenu(MenuName = "UVAnim")]
    public partial class UVAnim : IO.IAsset
    {
        public const string AssetExt = ".uvanim";

        public class ImportAttribute : IO.CommonCreateAttribute
        {
        }
        public UVAnim(UInt32 clr, float sz)
        {
            Size = new Vector2(sz, sz);
            Color = clr;
        }
        public UVAnim()
        {

        }
        public IO.IAssetMeta CreateAMeta()
        {
            var result = new UVAnimAMeta();
            result.Icon = new UVAnim();
            return result;
        }
        public IO.IAssetMeta GetAMeta()
        {
            return UEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }
        public void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {
            ameta.RefAssetRNames.Clear();
            if (TextureName != null)
                ameta.RefAssetRNames.Add(TextureName);
        }
        public void SaveAssetTo(RName name)
        {
            var ameta = this.GetAMeta();
            if (ameta != null)
            {
                UpdateAMetaReferences(ameta);
                ameta.SaveAMeta();
            }
            IO.FileManager.SaveObjectToXml(name.Address, this);
        }
        [Rtti.Meta]
        public RName AssetName
        {
            get;
            set;
        }
        [Rtti.Meta]
        public Vector2 Size { get; set; } = new Vector2(50, 50);
        [Rtti.Meta]
        public RName TextureName { get; set; }
        [Rtti.Meta]
        public Vector2 UVCoord { get; set; } = new Vector2(0, 0);
        [Rtti.Meta]
        public Vector2 UVSize { get; set; } = new Vector2(1, 1);
        [EGui.Controls.PropertyGrid.UByte4ToColor4PickerEditor]
        [Rtti.Meta]
        public UInt32 Color { get; set; } = 0xFFFFFFFF;
        public void OnDraw(ref ImDrawList cmdlist, ref Vector2 rectMin, ref Vector2 rectMax)
        {
            cmdlist.AddRectFilled(in rectMin, in rectMax, Color, 0, ImDrawFlags_.ImDrawFlags_RoundCornersAll);
        }
    }
}
