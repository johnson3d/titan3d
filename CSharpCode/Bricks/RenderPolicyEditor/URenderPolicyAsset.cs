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
        public override async System.Threading.Tasks.Task<IO.IAsset> LoadAsset()
        {
            return await UEngine.Instance.GfxDevice.TextureManager.GetTexture(GetAssetName());
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            //纹理不会引用别的资产
            return false;
        }
        public unsafe override void OnDraw(ref ImDrawList cmdlist, ref Vector2 sz, EGui.Controls.ContentBrowser ContentBrowser)
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
        public override void OnShowIconTimout(int time)
        {
            
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

            IO.FileManager.SaveObjectToXml(name.Address, PolicyGraph);
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

            IO.FileManager.LoadXmlToObject(name.Address, result.PolicyGraph);            

            return result;
        }
        [Rtti.Meta]
        public UPolicyGraph PolicyGraph { get; } = new UPolicyGraph();
    }
}
