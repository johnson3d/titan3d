using EngineNS.IO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.Asset
{
    [Rtti.Meta]
    public class UAnimationClipAMeta : IO.IAssetMeta
    {
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
    [UAnimationClip.Import]
    [IO.AssetCreateMenu(MenuName = "Animation")]
    public partial class UAnimationClip : IO.BaseSerializer, IAnimationAsset
    {
        #region IAnimationAsset
        public const string AssetExt = ".animclip";
        [Rtti.Meta]
        public RName AssetName
        {
            get;
            set;
        }

        public IAssetMeta CreateAMeta()
        {
            var result = new UAnimationClipAMeta();
            return result;
        }

        public IAssetMeta GetAMeta()
        {
            return UEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }

        public void UpdateAMetaReferences(IAssetMeta ameta)
        {
            ameta.RefAssetRNames.Clear();
        }

        public void SaveAssetTo(RName name)
        {
            throw new NotImplementedException();
        }
        #endregion IAnimationAsset

        #region AnimationChunk
        UAnimationChunk AnimationChunk = null;
        public Base.AnimHierarchy AnimatedHierarchy
        {
            get
            {
                return AnimationChunk.AnimatedHierarchy;
            }
        }
        public Dictionary<Guid, Curve.ICurve> AnimCurvesList
        {
            get
            {
                return AnimationChunk.AnimCurvesList;
            }
        }
        #endregion

        #region ImprotAttribute
        public partial class ImportAttribute : IO.CommonCreateAttribute
        {
            ~ImportAttribute()
            {
                mFileDialog.Dispose();
            }
            string mSourceFile;
            ImGui.ImGuiFileDialog mFileDialog = ImGui.ImGuiFileDialog.CreateInstance();
            //EGui.Controls.PropertyGrid.PropertyGrid PGAsset = new EGui.Controls.PropertyGrid.PropertyGrid();
            public override void DoCreate(RName dir, Rtti.UTypeDesc type, string ext)
            {
                mDir = dir;
                //mDesc.Desc.SetDefault();
                //PGAsset.SingleTarget = mDesc;
            }
            public override unsafe void OnDraw(EGui.Controls.ContentBrowser ContentBrowser)
            {
                FBXCreateCreateDraw(ContentBrowser);
            }

            //for just create a clip as a property animation not from fbx 
            public unsafe void SimpleCreateDraw(EGui.Controls.ContentBrowser ContentBrowser)
            {

            }
            private unsafe bool SimpleImport()
            {
                return false;
            }

            public unsafe partial void FBXCreateCreateDraw(EGui.Controls.ContentBrowser ContentBrowser);
        }
        #endregion

    }
}
