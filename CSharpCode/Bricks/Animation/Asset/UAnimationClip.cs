using EngineNS.Animation.Base;
using EngineNS.Animation.Curve;
using EngineNS.IO;
using EngineNS.Rtti;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EngineNS.Animation.Notify;

namespace EngineNS.Animation.Asset
{
    [Rtti.Meta]
    public class UAnimationClipAMeta : IO.IAssetMeta
    {
        public override string GetAssetExtType()
        {
            return UAnimationClip.AssetExt;
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
        [Rtti.Meta]
        public float SampleRate { get; set; } = 1.0f;
        [Rtti.Meta]
        public float Duration { get; set; } = 0.0f;
        [Rtti.Meta]
        public List<IAnimNotify> Notifies { get; set; } = new List<IAnimNotify>();
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
        #endregion IAnimationAsset

        #region AnimationChunk
        [Rtti.Meta]
        public RName AnimationChunkName { get; set; }
        UAnimationChunk AnimationChunk = null;
        public Base.UAnimHierarchy AnimatedHierarchy
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

        public void SaveAssetTo(RName name)
        {
            var ameta = this.GetAMeta();
            if (ameta != null)
            {
                UpdateAMetaReferences(ameta);
                ameta.SaveAMeta();
            }
            var typeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(this.GetType());
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
        public static UAnimationClip LoadXnd(UAnimationClipManager manager, IO.TtXndHolder holder)
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
                var clip = result as UAnimationClip;
                if (clip != null)
                {
                    var chunk = EngineNS.UEngine.Instance.AnimationModule.AnimationChunkManager.GetAnimationChunk(clip.AnimationChunkName);
                    if (chunk != null)
                    {
                        ConstructAnimHierarchy(chunk.AnimatedHierarchy, null, chunk.AnimatedHierarchy.Children);
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
                            chunk = result as UAnimationChunk;
                            if (chunk != null)
                            {
                                ConstructAnimHierarchy(chunk.AnimatedHierarchy, null, chunk.AnimatedHierarchy.Children);
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

        public static void ConstructAnimHierarchy(UAnimHierarchy root, UAnimHierarchy parent, List<UAnimHierarchy> children)
        {
            if (root == null)
            {
                foreach (var property in root.Node.Properties)
                {
                    if (property.ClassType == UTypeDesc.TypeOf<Vector3>())
                        property.TypeStr = UTypeDesc.TypeOf<NullableVector3>().TypeString;
                }
            }
            if (parent != null)
            {
                foreach (var property in parent.Node.Properties)
                {
                    if (property.ClassType == UTypeDesc.TypeOf<Vector3>())
                        property.TypeStr = UTypeDesc.TypeOf<NullableVector3>().TypeString;
                }
            }
            foreach (var child in children)
            {
                child.Root = root;
                child.Parent = parent;
                ConstructAnimHierarchy(root,child,child.Children);
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
            ImGui.ImGuiFileDialog mFileDialog = UEngine.Instance.EditorInstance.FileDialog.mFileDialog;
            //EGui.Controls.PropertyGrid.PropertyGrid PGAsset = new EGui.Controls.PropertyGrid.PropertyGrid();
            public override void DoCreate(RName dir, Rtti.UTypeDesc type, string ext)
            {
                mDir = dir;
                var noused = PGAsset.Initialize();
                //mDesc.Desc.SetDefault();
                //PGAsset.SingleTarget = mDesc;
            }
            public override unsafe bool OnDraw(EGui.Controls.UContentBrowser ContentBrowser)
            {
                return FBXCreateCreateDraw(ContentBrowser);
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
        }
        #endregion

    }
    public class UAnimationClipManager
    {
        Dictionary<RName, UAnimationClip> AnimationClips = new Dictionary<RName, UAnimationClip>();

        //for new don't see animationChunk as the asset
        public async System.Threading.Tasks.Task<UAnimationClip> GetAnimationClip(RName name)
        {
            UAnimationClip result;
            if (AnimationClips.TryGetValue(name, out result))
                return result;

            //this is a demo for suspend&cancel operation
            Thread.Async.TtAsyncTaskToken token = null;// new Thread.Async.TtAsyncTaskToken();
            result = await UEngine.Instance.EventPoster.Post((state) =>
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
                        var clip = UAnimationClip.LoadXnd(this, xnd);
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
    }
}
namespace EngineNS.Animation
{

    public partial class UAnimationModule
    {
        public Animation.Asset.UAnimationClipManager AnimationClipManager { get; } = new Animation.Asset.UAnimationClipManager();
    }

}