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
        public unsafe override void OnDraw(in ImDrawList cmdlist, in Vector2 sz, EGui.Controls.UContentBrowser ContentBrowser)
        {
            var start = ImGuiAPI.GetItemRectMin();
            var end = start + sz;

            var name = IO.FileManager.GetPureName(GetAssetName().Name);
            var tsz = ImGuiAPI.CalcTextSize(name, false, -1);
            Vector2 tpos;
            tpos.Y = start.Y + sz.Y - tsz.Y;
            tpos.X = start.X + (sz.X - tsz.X) * 0.5f;
            //ImGuiAPI.PushClipRect(in start, in end, true);

            end.Y -= tsz.Y;
            OnDrawSnapshot(in cmdlist, ref start, ref end);
            cmdlist.AddRect(in start, in end, (uint)EGui.UCoreStyles.Instance.SnapBorderColor.ToArgb(),
                EGui.UCoreStyles.Instance.SnapRounding, ImDrawFlags_.ImDrawFlags_RoundCornersAll, EGui.UCoreStyles.Instance.SnapThinkness);

            cmdlist.AddText(in tpos, 0xFFFF00FF, name, null);
            //ImGuiAPI.PopClipRect();

            DrawPopMenu(ContentBrowser);
        }
        public override void OnDrawSnapshot(in ImDrawList cmdlist, ref Vector2 start, ref Vector2 end)
        {
            base.OnDrawSnapshot(in cmdlist, ref start, ref end);
            cmdlist.AddText(in start, 0xFFFFFFFF, "PGC", null);
        }
        public override void OnShowIconTimout(int time)
        {
            base.OnShowIconTimout(time);
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

            ameta.AddReferenceAsset(AssetGraph.ProgramName);
            foreach (UPgcNodeBase i in AssetGraph.Nodes)
            {
                i.UpdateAMetaReferences(ameta);
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
            if (ameta != null)
            {
                UpdateAMetaReferences(ameta);
                ameta.SaveAMeta();
            }

            AssetGraph.Version++;
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

            result.AssetName = name;
            result.AssetGraph.AssetName = name;

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
            //AssetGraph.IsTryCacheBuffer = true;
            AssetGraph.BufferCache.ResetCache();
            var nodes = AssetGraph.Compile();
            int NumOfLayer = 0;
            foreach (var i in nodes)
            {
                if (i.RootDistance >= NumOfLayer)
                    NumOfLayer = i.RootDistance;
            }
            NumOfLayer += 1;
            List<UPgcNodeBase>[] Layers = new List<UPgcNodeBase>[NumOfLayer];
            for (int i = 0; i < NumOfLayer; i++)
            {
                Layers[i] = new List<UPgcNodeBase>();
            }
            
            foreach (var i in nodes)
            {
                i.InitProcedure(AssetGraph);
                
                Layers[i.RootDistance].Add(i);
            }
            foreach (var i in nodes)
            {
                AssetGraph.McProgram?.Get().OnNodeInitialized(AssetGraph, i);
            }

            for (int i = Layers.Length - 1; i >= 0; i--)
            {
                foreach (var j in Layers[i])
                {
                    //AssetGraph.BufferCache .... clear
                    //AssetGraph.BufferCache .... SaveBuffferToTempFile
                    var t1 = Support.Time.HighPrecision_GetTickCount();
                    j.DoProcedure(AssetGraph);
                    AssetGraph.McProgram?.Get().OnNodeProcedureFinished(AssetGraph, j);
                    var t2 = Support.Time.HighPrecision_GetTickCount();
                    Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Procedure", $"Node:{j.Name} = {(t2 - t1) / 1000000.0f}");
                }
            }
            //foreach (var i in nodes)
            //{
            //    i.DoProcedure(AssetGraph);
            //}
        }
    }
}
