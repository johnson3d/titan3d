using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Procedure
{
    [Rtti.Meta]
    public class UPgcAssetAMeta : IO.IAssetMeta
    {
        public override string GetAssetExtType()
        {
            return UPgcAsset.AssetExt;
        }
        public override async System.Threading.Tasks.Task<IO.IAsset> LoadAsset()
        {
            return await UEngine.Instance.GfxDevice.TextureManager.GetTexture(GetAssetName());
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            //纹理不会引用别的资产
            return false;
        }
        public unsafe override void OnDraw(in ImDrawList cmdlist, in Vector2 sz, EGui.Controls.ContentBrowser ContentBrowser)
        {
            var start = ImGuiAPI.GetItemRectMin();
            var end = start + sz;

            var name = IO.FileManager.GetPureName(GetAssetName().Name);
            var tsz = ImGuiAPI.CalcTextSize(name, false, -1);
            Vector2 tpos;
            tpos.Y = start.Y + sz.Y - tsz.Y;
            tpos.X = start.X + (sz.X - tsz.X) * 0.5f;
            ImGuiAPI.PushClipRect(in start, in end, true);

            end.Y -= tsz.Y;
            OnDrawSnapshot(in cmdlist, ref start, ref end);

            cmdlist.AddText(in tpos, 0xFFFF00FF, name, null);
            ImGuiAPI.PopClipRect();
        }
        public override void OnDrawSnapshot(in ImDrawList cmdlist, ref Vector2 start, ref Vector2 end)
        {
            base.OnDrawSnapshot(in cmdlist, ref start, ref end);
            cmdlist.AddText(in start, 0xFFFFFFFF, "PGC", null);
        }
        public override void OnShowIconTimout(int time)
        {

        }
    }
    [Rtti.Meta]
    [UPgcAsset.Import]
    [IO.AssetCreateMenu(MenuName = "Procedure")]
    [Editor.UAssetEditor(EditorType = typeof(UPgcEditor))]
    public class UPgcAsset : IO.IAsset
    {
        public const string AssetExt = ".pgc";
        public class ImportAttribute : IO.CommonCreateAttribute
        {

        }
        #region IAsset
        public IO.IAssetMeta CreateAMeta()
        {
            var result = new UPgcAssetAMeta();
            return result;
        }
        public IO.IAssetMeta GetAMeta()
        {
            return UEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }
        public void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {
            ameta.RefAssetRNames.Clear();
        }
        public void SaveAssetTo(RName name)
        {
            //var xml = new System.Xml.XmlDocument();
            //var xmlRoot = xml.CreateElement($"Root", xml.NamespaceURI);
            //xml.AppendChild(xmlRoot);
            //IO.SerializerHelper.WriteObjectMetaFields(xml, xmlRoot, PolicyGraph);
            //var xmlText = IO.FileManager.GetXmlText(xml);

            //IO.FileManager.WriteAllText(name.Address, xmlText);

            IO.FileManager.SaveObjectToXml(name.Address, AssetGraph);
        }
        [Rtti.Meta]
        public RName AssetName
        {
            get;
            set;
        }
        #endregion

        [Rtti.Meta]
        public UPgcGraph AssetGraph { get; } = new UPgcGraph();

        public static UPgcAsset LoadAsset(RName name)
        {
            var result = new UPgcAsset();

            if (IO.FileManager.LoadXmlToObject(name.Address, result.AssetGraph) == false)
                return null;

            result.AssetGraph.Root = result.AssetGraph.FindFirstNode("RootNode") as Node.UEndingNode;
            if (result.AssetGraph.Root == null)
            {
                result.AssetGraph.Root = new Node.UEndingNode();
                result.AssetGraph.Root.Name = "RootNode";
                result.AssetGraph.Root.PinNumber = 4;
                result.AssetGraph.AddNode(result.AssetGraph.Root);
            }

            return result;
        }

        public void Compile()
        {
            AssetGraph.BufferCache.ResetCache();
            var nodes = AssetGraph.Compile();
            foreach (var i in nodes)
            {
                i.InitProcedure(AssetGraph);
            }
            foreach (var i in nodes)
            {
                i.DoProcedure(AssetGraph);
            }
        }
    }
}
