using EngineNS.Bricks.CodeBuilder.MacrossNode;
using EngineNS.UI.Controls.Containers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.UI
{
    [Rtti.Meta]
    public class TtUIAssetAMeta : IO.IAssetMeta
    {
        public override string GetAssetExtType()
        {
            return TtUIAsset.AssetExt;
        }
        public override async System.Threading.Tasks.Task<IO.IAsset> LoadAsset()
        {
            //return await TtEngine.Instance.GfxDevice.TextureManager.GetTexture(GetAssetName());
            return null;
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            return true;
        }
        public override string GetAssetTypeName()
        {
            return "UI";
        }
        public override void OnShowIconTimout(int time)
        {
            base.OnShowIconTimout(time);
        }
    }
    [Rtti.Meta]
    [TtUIAsset.Import]
    [IO.AssetCreateMenu(MenuName = "UI/UI")]
    [EngineNS.Editor.UAssetEditor(EditorType = typeof(EngineNS.UI.Editor.TtUIEditor))]
    public class TtUIAsset : IO.IAsset, IDisposable
    {
        public const string AssetExt = ".ui";
        public class ImportAttribute : IO.CommonCreateAttribute
        {

        }
        public Graphics.Mesh.TtMesh Mesh;
        public void Dispose()
        {
            CoreSDK.DisposeObject(ref Mesh);
        }
        #region IAsset
        public IO.IAssetMeta CreateAMeta()
        {
            var result = new TtUIAssetAMeta();
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
        public TtUIHost UIHost = null;
        UMacrossEditor mMacrossEditor = null;
        [Browsable(false)]
        public UMacrossEditor MacrossEditor
        {
            get
            {
                if (mMacrossEditor == null)
                {
                    mMacrossEditor = new UMacrossEditor();
                    mMacrossEditor.AssetName = AssetName;
                }
                return mMacrossEditor;
            }
        }
        public void SaveAssetTo(RName name)
        {
            var ameta = this.GetAMeta();
            if (ameta == null)
            {
                var asset = TtEngine.Instance.AssetMetaManager.NewAsset<TtUIAsset>(name);
                ameta = asset.GetAMeta();
            }
            UpdateAMetaReferences(ameta);
            ameta.SaveAMeta(this);

            MacrossEditor.AssetName = name;
            MacrossEditor.DefClass.ClassName = name.PureName;
            MacrossEditor.DefClass.Namespace = new Bricks.CodeBuilder.UNamespaceDeclaration(IO.TtFileManager.GetParentPathName(AssetName.Name).TrimEnd('/').Replace('/', '.'));
            MacrossEditor.DefClass.SupperClassNames.Add(typeof(TtUIMacrossBase).FullName);
            MacrossEditor.SaveClassGraph(AssetName);
            MacrossEditor.GenerateCode();
            MacrossEditor.CompileCode();

            if(UIHost == null)
            {
                var canvas = new TtCanvasControl();
                TtEngine.Instance.UIManager.Save(name, canvas);
            }
            else
            {
                TtEngine.Instance.UIManager.Save(name, UIHost.Children[0]);
            }
        }
        [Rtti.Meta]
        public RName AssetName
        {
            get;
            set;
        }
        #endregion
    }
}
