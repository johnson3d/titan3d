using EngineNS.EGui.Controls;
using EngineNS.IO;
using EngineNS.Rtti;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.CodeBuilder
{
    public partial class UMacrossAMeta : IO.IAssetMeta
    {
        public override bool CanRefAssetType(IAssetMeta ameta)
        {
            // macross可以引用所有类型的资源
            return true;
        }
        public override void OnDraw(ref ImDrawList cmdlist, ref Vector2 sz, ContentBrowser ContentBrowser)
        {
            base.OnDraw(ref cmdlist, ref sz, ContentBrowser);
        }
    }

    [Rtti.Meta]
    [UMacross.MacrossCreate]
    [IO.AssetCreateMenu(MenuName = "Macross")]
    [Editor.UAssetEditor(EditorType = typeof(Bricks.CodeBuilder.MacrossNode.ClassGraph))]
    public partial class UMacross : IO.IAsset
    {
        public const string AssetExt = ".macross";

        public class MacrossCreateAttribute : IO.CommonCreateAttribute
        {
            public override void DoCreate(RName dir, UTypeDesc type, string ext)
            {
                ExtName = ext;
                mName = null;
                mDir = dir;
                TypeSlt.BaseType = type;
                TypeSlt.SelectedType = type;

                mAsset = Rtti.UTypeDescManager.CreateInstance(TypeSlt.SelectedType.SystemType) as IAsset;
                PGAsset.SingleTarget = mAsset;
            }
        }

        public RName AssetName
        {
            get;
            set;
        }

        public IO.IAssetMeta CreateAMeta()
        {
            var result = new UMacrossAMeta();
            result.Icon = new EGui.UVAnim();
            return result;
        }

        public IO.IAssetMeta GetAMeta()
        {
            return UEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }

        public void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {

        }

        public void SaveAssetTo(RName name)
        {
            IO.FileManager.CreateDirectory(name.Address);

            var graph = new MacrossNode.ClassGraph();
            graph.AssetName = name;
            graph.DefClass.ClassName = name.PureName;
            graph.DefClass.NameSpace = IO.FileManager.GetParentPathName(name.Name).TrimEnd('/').Replace('/', '.');
            graph.SaveClassGraph(name);
        }
    }
}
