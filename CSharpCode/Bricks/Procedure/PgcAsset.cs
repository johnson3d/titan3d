using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Procedure
{
    [Rtti.Meta("", NameAlias = new string[] { "EngineNS.Bricks.Procedure.UPgcAssetAMeta@EngineCore", "EngineNS.Bricks.Procedure.UPgcAssetAMeta" })]
    public class TtPgcAssetAMeta : IO.IAssetMeta
    {
        public override string TypeExt
        {
            get => TtPgcAsset.AssetExt;
        }
        public override Color4b GetBorderColor()
        {
            return TtEngine.Instance.ConfigManager.GetConfig<Editor.TtEditorConfig>().PgcBoderColor;
        }
        public override async Thread.Async.TtTask<IO.IAsset> GetAsset(params object[] args)
        {
            return TtPgcAsset.LoadAsset(GetAssetName());
        }
        public override async Thread.Async.TtTask<IO.IAsset> CreateAsset(params object[] args)
        {
            return TtPgcAsset.LoadAsset(GetAssetName());
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            return true;
        }
        public const string AssetTypeName = "PGC";
        public override string GetAssetTypeName()
        {
            return AssetTypeName;
        }
        //public unsafe override void OnDraw(in ImDrawList cmdlist, in Vector2 sz, EGui.Controls.UContentBrowser ContentBrowser)
        //{
        //    var start = ImGuiAPI.GetItemRectMin();
        //    var end = start + sz;

        //    var name = IO.FileManager.GetPureName(GetAssetName().Name);
        //    var tsz = ImGuiAPI.CalcTextSize(name, false, -1);
        //    Vector2 tpos;
        //    tpos.Y = start.Y + sz.Y - tsz.Y;
        //    tpos.X = start.X + (sz.X - tsz.X) * 0.5f;
        //    //ImGuiAPI.PushClipRect(in start, in end, true);

        //    end.Y -= tsz.Y;
        //    OnDrawSnapshot(in cmdlist, ref start, ref end);
        //    cmdlist.AddRect(in start, in end, (uint)EGui.UCoreStyles.Instance.SnapBorderColor.ToArgb(),
        //        EGui.UCoreStyles.Instance.SnapRounding, ImDrawFlags_.ImDrawFlags_RoundCornersAll, EGui.UCoreStyles.Instance.SnapThinkness);

        //    cmdlist.AddText(in tpos, 0xFFFF00FF, name, null);
        //    //ImGuiAPI.PopClipRect();

        //    DrawPopMenu(ContentBrowser);
        //}
        //public override void OnDrawSnapshot(in ImDrawList cmdlist, ref Vector2 start, ref Vector2 end)
        //{
        //    base.OnDrawSnapshot(in cmdlist, ref start, ref end);
        //    cmdlist.AddText(in start, 0xFFFFFFFF, "PGC", null);
        //}
        public override void OnShowIconTimout(int time)
        {
            base.OnShowIconTimout(time);
        }
    }
    [TtPgcAsset.Import]
    [IO.AssetCreateMenu(MenuName = "Procedure")]
    [Editor.TtAssetEditor(EditorType = typeof(TtPgcEditor))]
    [Rtti.Meta("", NameAlias = new string[] { "EngineNS.Bricks.Procedure.UPgcAsset@EngineCore", "EngineNS.Bricks.Procedure.UPgcAsset" })]
    public class TtPgcAsset : IO.IAsset
    {
        public const string AssetExt = ".pgc";
        public string TypeExt { get => AssetExt; }
        public class ImportAttribute : IO.CommonCreateAttribute
        {

        }
        #region IAsset
        public IO.IAssetMeta CreateAMeta()
        {
            var result = new TtPgcAssetAMeta();
            return result;
        }
        public IO.IAssetMeta GetAMeta()
        {
            return TtEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }
        public void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {
            ameta.RefAssetRNames.Clear();

            ameta.AddReferenceAsset(AssetGraph.ProgramName);
            foreach (var i in AssetGraph.Nodes)
            {
                var node = i as TtPgcNodeBase;
                if (node == null)
                    continue;
                node.UpdateAMetaReferences(ameta);
            }
            ameta.RefAssetRNames.Sort();
        }
        public void SaveAssetTo(RName name)
        {
            //var xml = new System.Xml.XmlDocument();
            //var xmlRoot = xml.CreateElement($"Root", xml.NamespaceURI);
            //xml.AppendChild(xmlRoot);
            //IO.SerializerHelper.WriteObjectMetaFields(xml, xmlRoot, PolicyGraph);
            //var xmlText = IO.FileManager.GetXmlText(xml);

            //IO.FileManager.WriteAllText(name.Address, xmlText);

            var ameta = this.GetAMeta();
            if (ameta == null)
            {
                var asset = TtEngine.Instance.AssetMetaManager.NewAsset<TtPgcAsset>(name);
                ameta = asset.GetAMeta();
            }
            UpdateAMetaReferences(ameta);
            ameta.SaveAMeta(this);

            AssetGraph.Version++;
            IO.TtFileManager.SaveObjectToXml(name.Address, AssetGraph);
            name.AMeta.AddAssetFile(name.Address);
            TtEngine.Instance.SourceControlModule.AddFile(name.Address);
        }
        [Rtti.Meta("")]
        public RName AssetName
        {
            get;
            set;
        }
        #endregion

        [Rtti.Meta("")]
        public TtPgcGraph AssetGraph { get; } = new TtPgcGraph();

        public static TtPgcAsset LoadAsset(RName name)
        {
            var result = new TtPgcAsset();

            if (IO.TtFileManager.LoadXmlToObject(name.Address, result.AssetGraph) == false)
                return null;

            result.AssetName = name;
            result.AssetGraph.AssetName = name;

            result.AssetGraph.Root = result.AssetGraph.FindFirstNode("RootNode", false) as Node.TtEndingNode;
            if (result.AssetGraph.Root == null)
            {
                result.AssetGraph.Root = new Node.TtEndingNode();
                result.AssetGraph.Root.Name = "RootNode";
                var nodeDef = result.AssetGraph.Root;
                var inputs = new List<Bricks.Procedure.Node.UNodePinDefine>();
                var height = new Bricks.Procedure.Node.UNodePinDefine();
                height.Name = "Height";
                inputs.Add(height);
                nodeDef.UserInputs = inputs;
                result.AssetGraph.AddNode(result.AssetGraph.Root);
            }

            return result;
        }

        public void Compile(TtPgcNodeBase root)
        {
            AssetGraph.Compile(root);
        }

        public void Compile(TtPgcNodeBase root, TtPgcExecutionContext context, bool resetCache = true)
        {
            AssetGraph.Compile(root, context, resetCache);
        }
    }
}
