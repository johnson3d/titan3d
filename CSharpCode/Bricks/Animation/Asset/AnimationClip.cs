using EngineNS.Animation.Base;
using EngineNS.Animation.Curve;
using EngineNS.IO;
using EngineNS.Rtti;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EngineNS.Animation.Notify;
using EngineNS.Thread.Async;

namespace EngineNS.Animation.Asset
{
    [Rtti.Meta]
    public class TtAnimationClipAMeta : IO.IAssetMeta
    {
        public override string GetAssetExtType()
        {
            return TtAnimationClip.AssetExt;
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            //必须是TextureAsset
            return true;
        }
        public override string GetAssetTypeName()
        {
            return "AnimationClip";
        }
        public override async Task<IAsset> LoadAsset()
        {
            return await TtEngine.Instance.AnimationModule.AnimationClipManager.GetAnimationClip(GetAssetName());
        }
    }

    [Rtti.Meta]
    [TtAnimationClip.Import]
    [IO.AssetCreateMenu(MenuName = "Anim/Animation")]
    public partial class TtAnimationClip : IO.BaseSerializer, IAnimationAsset
    {
        #region IAnimationAsset
        public const string AssetExt = ".animclip";
        [Rtti.Meta]
        public RName AssetName
        {
            get;
            set;
        }
        [Rtti.Meta]
        public float SampleRate { get; set; } = 1.0f;
        [Rtti.Meta]
        public float Duration { get; set; } = 0.0f;
        [Rtti.Meta]
        public List<IAnimNotify> Notifies { get; set; } = new List<IAnimNotify>();
        public IAssetMeta CreateAMeta()
        {
            var result = new TtAnimationClipAMeta();
            return result;
        }

        public IAssetMeta GetAMeta()
        {
            return TtEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }

        public void UpdateAMetaReferences(IAssetMeta ameta)
        {
            ameta.RefAssetRNames.Clear();
        }
        #endregion IAnimationAsset

        #region AnimationChunk
        [Rtti.Meta]
        public RName AnimationChunkName { get; set; }
        public TtAnimationChunk AnimationChunk { get; set; } = null;

        public Dictionary<Guid, Curve.ICurve> AnimCurvesList
        {
            get
            {
                return AnimationChunk.AnimCurvesList;
            }
        }
        #endregion

        public void SaveAssetTo(RName name)
        {
            var ameta = this.GetAMeta();
            if (ameta != null)
            {
                UpdateAMetaReferences(ameta);
                ameta.SaveAMeta(this);
            }
            var typeStr = Rtti.TtTypeDescManager.Instance.GetTypeStringFromType(this.GetType());
            var xnd = new IO.TtXndHolder(typeStr, 0, 0);
            using (var attr = xnd.NewAttribute("AnimationClip", 0, 0))
            {
                using (var ar = attr.GetWriter(512))
                {
                    ar.Write(this);
                }
                xnd.RootNode.AddAttribute(attr);
            }
            using (var attr = xnd.NewAttribute("AnimationChunk", 0, 0))
            {
                using (var ar = attr.GetWriter(512))
                {
                    ar.Write(AnimationChunk);
                }
                xnd.RootNode.AddAttribute(attr);
            }

            xnd.SaveXnd(name.Address);
        }
        public static TtAnimationClip LoadXnd(TtAnimationClipManager manager, IO.TtXndHolder holder)
        {
            unsafe
            {
                IO.ISerializer result = null;
                var attr = holder.RootNode.TryGetAttribute("AnimationClip");
                if ((IntPtr)attr.CppPointer != IntPtr.Zero)
                {
                    using (var ar = attr.GetReader(manager))
                    {
                        ar.Read(out result, manager);
                    }
                }
                var clip = result as TtAnimationClip;
                if (clip != null)
                {
                    var chunk = EngineNS.TtEngine.Instance.AnimationModule.AnimationChunkManager.GetAnimationChunk(clip.AnimationChunkName);
                    if (chunk != null)
                    {
                        clip.AnimationChunk = chunk;
                    }
                    else
                    {
                        var chunkAttr = holder.RootNode.TryGetAttribute("AnimationChunk");
                        if ((IntPtr)chunkAttr.CppPointer != IntPtr.Zero)
                        {
                            using (var ar = chunkAttr.GetReader(manager))
                            {
                                ar.Read(out result, manager);
                            }
                            chunk = result as TtAnimationChunk;
                            if (chunk != null)
                            {
                                clip.AnimationChunk = chunk;
                            }
                            else
                            {
                                System.Diagnostics.Debug.Assert(false);
                            }
                        }
                    }
                    return clip;
                }
                return null;
            }
        }

        #region ImprotAttribute
        public partial class ImportAttribute : IO.CommonCreateAttribute
        {
            ~ImportAttribute()
            {
                //mFileDialog.Dispose();
            }
            string mSourceFile;
            ImGui.ImGuiFileDialog mFileDialog = TtEngine.Instance.EditorInstance.FileDialog.mFileDialog;
            //EGui.Controls.PropertyGrid.PropertyGrid PGAsset = new EGui.Controls.PropertyGrid.PropertyGrid();
            public override async Thread.Async.TtTask DoCreate(RName dir, Rtti.TtTypeDesc type, string ext)
            {
                mDir = dir;
                await PGAsset.Initialize();
                //mDesc.Desc.SetDefault();
                //PGAsset.SingleTarget = mDesc;
            }
            public override unsafe bool OnDraw(EGui.Controls.UContentBrowser ContentBrowser)
            {
                //return FBXCreateCreateDraw(ContentBrowser);
                return AssimpCreateCreateDraw(ContentBrowser);
            }

            //for just create a clip as a property animation not from fbx 
            public unsafe void SimpleCreateDraw(EGui.Controls.UContentBrowser ContentBrowser)
            {

            }
            private unsafe bool SimpleImport()
            {
                return false;
            }

            public unsafe partial bool FBXCreateCreateDraw(EGui.Controls.UContentBrowser ContentBrowser);
            public unsafe partial bool AssimpCreateCreateDraw(EGui.Controls.UContentBrowser ContentBrowser);
        }
        #endregion

    }
    public class TtAnimationClipManager
    {
        Dictionary<RName, TtAnimationClip> AnimationClips = new Dictionary<RName, TtAnimationClip>();

        //for new don't see animationChunk as the asset
        public async TtTask<TtAnimationClip> GetAnimationClip(RName name)
        {
            TtAnimationClip result;
            if (AnimationClips.TryGetValue(name, out result))
                return result;

            //this is a demo for suspend&cancel operation
            Thread.Async.TtAsyncTaskToken token = null;// new Thread.Async.TtAsyncTaskToken();
            result = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                if (state.TaskToken != null)
                {
                    if (state.TaskToken.TaskState == Thread.Async.EAsyncTaskState.Suspended)
                    {
                        state.TaskState = Thread.Async.EAsyncTaskState.Suspended;
                        return null;
                    }
                    else if (state.TaskToken.TaskState == Thread.Async.EAsyncTaskState.Canceled)
                    {
                        state.TaskState = Thread.Async.EAsyncTaskState.Canceled;
                        return null;
                    }
                }
                using (var xnd = IO.TtXndHolder.LoadXnd(name.Address))
                {
                    if (xnd != null)
                    {
                        var clip = TtAnimationClip.LoadXnd(this, xnd);
                        clip.SaveAssetTo(name);
                        if (clip == null)
                            return null;

                        clip.AssetName = name;
                        return clip;
                    }
                    else
                    {
                        return null;
                    }
                }
            }, Thread.Async.EAsyncTarget.AsyncIO).WithToken(token);

            if (result != null)
            {
                AnimationClips[name] = result;
                return result;
            }
            return null;
        }
        public void Remove(RName name)
        {
            AnimationClips.Remove(name);
        }
    }
}
namespace EngineNS.Animation
{

    public partial class TtAnimationModule
    {
        public Animation.Asset.TtAnimationClipManager AnimationClipManager { get; } = new Animation.Asset.TtAnimationClipManager();
    }

}