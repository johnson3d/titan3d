using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.RenderPolicyEditor
{
    [Rtti.Meta]
    public class URenderPolicyAssetAMeta : IO.IAssetMeta
    {
        public override string GetAssetExtType()
        {
            return URenderPolicyAsset.AssetExt;
        }
        public override string GetAssetTypeName()
        {
            return "RPolicy";
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
    [Rtti.Meta]
    [URenderPolicyAsset.Import]
    [IO.AssetCreateMenu(MenuName = "RenderPolicy")]
    [Editor.UAssetEditor(EditorType = typeof(UPolicyEditor))]
    public class URenderPolicyAsset : IO.IAsset
    {
        public const string AssetExt = ".rpolicy";

        public class ImportAttribute : IO.CommonCreateAttribute
        {
            
        }

        public URenderPolicyAsset()
        {
            
        }

        #region IAsset
        public IO.IAssetMeta CreateAMeta()
        {
            var result = new URenderPolicyAssetAMeta();
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

            IO.TtFileManager.SaveObjectToXml(name.Address, PolicyGraph);
        }
        [Rtti.Meta]
        public RName AssetName
        {
            get;
            set;
        }
        #endregion

        public static URenderPolicyAsset LoadAsset(RName name)
        {
            var result = new URenderPolicyAsset();

            if (IO.TtFileManager.LoadXmlToObject(name.Address, result.PolicyGraph) == false)
                return null;

            result.PolicyGraph.AssetName = name;
            return result;
        }
        [Rtti.Meta]
        public UPolicyGraph PolicyGraph { get; } = new UPolicyGraph();
        public Graphics.Pipeline.URenderPolicy CreateRenderPolicy()
        {
            var typeDesc = PolicyGraph.PolicyType;
            var policy = Rtti.UTypeDescManager.CreateInstance(typeDesc) as Graphics.Pipeline.URenderPolicy; // new Graphics.Pipeline.URenderPolicy();
            var meta = Rtti.UClassMetaManager.Instance.GetMeta(typeDesc);
            meta.CopyObjectMetaField(policy, this.PolicyGraph.RenderPolicy);
            foreach (UPolicyNode i in PolicyGraph.Nodes)
            {
                if (false == policy.RegRenderNode(i.Name, i.GraphNode))
                {
                    policy.Dispose();
                    return null;
                }
            }
            foreach (var i in PolicyGraph.Linkers)
            {
                var inNode = policy.FindNode(i.InNode.Name);
                if (inNode == null)
                    continue;
                var outNode = policy.FindNode(i.OutNode.Name);
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
            var root = policy.FindFirstNode<Graphics.Pipeline.Common.UCopy2SwapChainNode>();
            policy.RootNode = root;
            bool hasInputError = false;
            policy.BuildGraph(ref hasInputError);
            if (hasInputError)
            {
                policy.Dispose();
                return null;
            }
            return policy;
        }
    }
}
