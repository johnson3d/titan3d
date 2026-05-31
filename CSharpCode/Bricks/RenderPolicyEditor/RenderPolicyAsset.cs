using EngineNS.Graphics.Pipeline;
using NPOI.HPSF;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.RenderPolicyEditor
{
    //[Rtti.Meta("",NameAlias = new string[] { "EngineNS.Bricks.RenderPolicyEditor.URenderPolicyAssetAMeta@EngineCore" })]
    public class TtRenderPolicyAssetAMeta : IO.IAssetMeta
    {
        public override string TypeExt
        {
            get => TtRenderPolicyAsset.AssetExt;
        }
        public override string GetAssetTypeName()
        {
            return "RPolicy";
        }
        public override Color4b GetBorderColor()
        {
            return TtEngine.Instance.EditorInstance.Config.RenderPolicyBoderColor;
        }
        public override async Thread.Async.TtTask<IO.IAsset> LoadAsset(params object[] args)
        {
            return TtRenderPolicyAsset.LoadAsset(GetAssetName());
        }
        public override async Thread.Async.TtTask<IO.IAsset> CreateAsset(params object[] args)
        {
            return TtRenderPolicyAsset.LoadAsset(GetAssetName());
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            //纹理不会引用别的资产
            return false;
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
        //    cmdlist.AddText(in start, 0xFFFFFFFF, "RPolicy", null);
        //}
        public override void OnShowIconTimout(int time)
        {
            base.OnShowIconTimout(time);
        }
    }
    [TtRenderPolicyAsset.Import]
    [IO.AssetCreateMenu(MenuName = "FX/RenderPolicy")]
    [Editor.UAssetEditor(EditorType = typeof(TtPolicyEditor))]
    //[Rtti.Meta("",NameAlias = new string[] { "EngineNS.Bricks.RenderPolicyEditor.URenderPolicyAsset@EngineCore" })]
    public class TtRenderPolicyAsset : IO.IAsset
    {
        public const string AssetExt = ".rpolicy";
        public string TypeExt { get => AssetExt; }
        public class ImportAttribute : IO.CommonCreateAttribute
        {
            
        }

        public TtRenderPolicyAsset()
        {
            
        }

        #region IAsset
        public IO.IAssetMeta CreateAMeta()
        {
            var result = new TtRenderPolicyAssetAMeta();
            return result;
        }
        public IO.IAssetMeta GetAMeta()
        {
            return TtEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
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
            var ameta = this.GetAMeta();
            if (ameta != null)
            {
                UpdateAMetaReferences(ameta);
                ameta.SaveAMeta(this);
            }

            IO.TtFileManager.SaveObjectToXml(name.Address, PolicyGraph);
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

        public static TtRenderPolicyAsset LoadAsset(RName name)
        {
            var result = new TtRenderPolicyAsset();

            if (IO.TtFileManager.LoadXmlToObject(name.Address, result.PolicyGraph) == false)
                return null;

            result.AssetName = name;
            result.PolicyGraph.AssetName = name;
            return result;
        }
        [Rtti.Meta("")]
        public string EndingNode
        {
            get;
            set;
        }
        [Rtti.Meta("")]
        public TtPolicyGraph PolicyGraph { get; } = new TtPolicyGraph();
        /// <summary>
        /// 从指定 rpolicy 资源创建一个完全独立的 RenderPolicy 实例.
        /// 内部每次重新 LoadAsset, 保证 RenderGraph 节点实例不会被多个 RP 共享
        /// (共享会导致 node.RenderGraph 引用被最后一个 RP 覆盖, Tick 时断言失败).
        /// </summary>
        public static Graphics.Pipeline.TtRenderPolicy CreateRenderPolicy(RName rPolicyName, IRenderViewport viewport, string endingName = "Copy2SwapChainNode")
        {
            var rpAsset = LoadAsset(rPolicyName);
            if (rpAsset == null)
            {
                Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Error, $"CreateRenderPolicy: LoadAsset failed for {rPolicyName}");
                return null;
            }
            return rpAsset.CreateRenderPolicyFromSelf(rPolicyName, viewport, endingName);
        }

        /// <summary>
        /// 从当前 asset 实例创建 RenderPolicy. 注意: 多次调用会共享 PolicyGraph
        /// 中的节点实例, 导致 node.RenderGraph 被最后一个 RP 覆盖.
        /// 除非你明确知道只会创建一个 RP (例如编辑器预览), 否则请使用静态方法
        /// CreateRenderPolicy(RName, IRenderViewport, string).
        /// </summary>
        private Graphics.Pipeline.TtRenderPolicy CreateRenderPolicyFromSelf(RName rPolicyName, IRenderViewport viewport, string endingName = "Copy2SwapChainNode")
        {
            var typeDesc = PolicyGraph.PolicyType;
            var policy = Rtti.TtTypeDescManager.CreateInstance(typeDesc) as Graphics.Pipeline.TtRenderPolicy;
            var meta = Rtti.TtClassMetaManager.Instance.GetMeta(typeDesc);
            meta.CopyObjectMetaField(policy, this.PolicyGraph.RenderPolicy);
            policy.RenderViewport = viewport;
            foreach (TtPolicyNode i in PolicyGraph.Nodes)
            {
                if (false == policy.RegRenderNode(i.NodeId, i.GraphNode))
                {
                    policy.Dispose();
                    return null;
                }
            }
            foreach (var i in PolicyGraph.Linkers)
            {
                if (i.InNode == null || i.OutNode == null)
                    continue;
                var inNode = policy.FindNode(i.InNode.NodeId);
                if (inNode == null)
                    continue;
                var outNode = policy.FindNode(i.OutNode.NodeId);
                if (outNode == null)
                    continue;
                var inPin = inNode.FindInput(i.InPin.Name);
                if (inPin == null)
                    continue;
                var outPin = outNode.FindOutput(i.OutPin.Name);
                if (outPin == null)
                    continue;
                policy.AddLinker(outPin, inPin);
            }
            if (endingName == null)
            {
                endingName = this.EndingNode;
            }

            Graphics.Pipeline.Common.TtEndingNode root = null;
            foreach (var i in policy.GraphNodes)
            {
                if (i.Value is Graphics.Pipeline.Common.TtEndingNode enode && enode.IsMainRoot)
                {
                    root = enode;
                }
            }
            if (root == null)
            {
                Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Error, $"Can't find root node in policy {rPolicyName}");
                policy.Dispose();
                return null;
            }
            policy.RootNode = root;
            bool hasInputError = false;
            policy.BuildGraph(ref hasInputError);
            if (hasInputError)
            {
                policy.Dispose();
                return null;
            }
            policy.RPolicyName = rPolicyName;
            return policy;
        }
    }
}
