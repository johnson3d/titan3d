using System;
using System.Collections.Generic;
using System.ComponentModel;
using EngineNS;

namespace EngineNS.EGui
{
    [Rtti.Meta]
    public partial class UVAnimAMeta : IO.IAssetMeta
    {
        [Rtti.Meta]
        public RName TextureName { get; set; }
        [Rtti.Meta]
        public Vector2 SnapUVStart { get; set; }
        [Rtti.Meta]
        public Vector2 SnapUVEnd { get; set; } = new Vector2(1, 1);
        public override string GetAssetExtType()
        {
            return UUvAnim.AssetExt;
        }
        public override string GetAssetTypeName()
        {
            return "UVAnim";
        }
        public override async System.Threading.Tasks.Task<IO.IAsset> LoadAsset()
        {
            await Thread.TtAsyncDummyClass.DummyFunc();
            return null;
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            if (ameta.GetAssetExtType() == NxRHI.USrView.AssetExt)
                return true;
            //必须是TextureAsset
            return false;
        }
        public override void OnShowIconTimout(int time)
        {
            if (SnapTask != null)
            {
                SnapTask = null;
            }
        }
        public override void OnDrawSnapshot(in ImDrawList cmdlist, ref Vector2 start, ref Vector2 end)
        {
            if (TextureName == null)
            {
                cmdlist.AddText(in start, 0xFFFFFFFF, "UVAnim", null);
                return;
            }
            if (SnapTask == null)
            {
                var rc = UEngine.Instance.GfxDevice.RenderContext;
                SnapTask = UEngine.Instance.GfxDevice.TextureManager.GetTexture(TextureName, 1);
                cmdlist.AddText(in start, 0xFFFFFFFF, "UVAnim", null);
                return;
            }
            else if (SnapTask.Value.IsCompleted == false)
            {
                cmdlist.AddText(in start, 0xFFFFFFFF, "UVAnim", null);
                return;
            }
            unsafe
            {
                if (SnapTask.Value.Result != null)
                {
                    var uv0 = SnapUVStart;
                    var uv1 = SnapUVEnd;
                    cmdlist.AddImage(SnapTask.Value.Result.GetTextureHandle().ToPointer(), in start, in end, in uv0, in uv1, 0xFFFFFFFF);
                }
                else
                {
                    //missing 
                }
            }

            //cmdlist.AddText(in start, 0xFFFFFFFF, "UVAnim", null);
        }
        Thread.Async.TtTask<NxRHI.USrView>? SnapTask;
    }
    [Rtti.Meta]
    [UUvAnim.Import]
    [Editor.UAssetEditor(EditorType = typeof(UUvAnimEditor))]
    [IO.AssetCreateMenu(MenuName = "UI/UVAnim")]
    public partial class UUvAnim : IO.IAsset, IO.ISerializer
    {
        public const string AssetExt = ".uvanim";
        #region ISerializer
        public void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {

        }
        public void OnPropertyRead(object tagObject, System.Reflection.PropertyInfo prop, bool fromXml)
        {

        }
        #endregion
        public class ImportAttribute : IO.CommonCreateAttribute
        {
        }
        public UUvAnim(UInt32 clr, float sz)
        {
            Size = new Vector2(sz, sz);
            Color = clr;

            Vector4 uv = new Vector4(0,0,1,1);
            FrameUVs.Add(uv);
        }
        public UUvAnim()
        {
            Vector4 uv = new Vector4(0, 0, 1, 1);
            FrameUVs.Add(uv);
        }
        #region IAsset
        public IO.IAssetMeta CreateAMeta()
        {
            var result = new UVAnimAMeta();
            result.Icon = new UUvAnim();
            return result;
        }
        public IO.IAssetMeta GetAMeta()
        {
            return UEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }
        public void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {
            var uvAnimAMeta = ameta as UVAnimAMeta;
            if (uvAnimAMeta != null)
            {
                uvAnimAMeta.TextureName = TextureName;
                Vector2 uvStart, uvEnd;
                this.GetUV(0, out uvStart, out uvEnd);
                uvAnimAMeta.SnapUVStart = uvStart;
                uvAnimAMeta.SnapUVEnd = uvEnd;
            }
            ameta.RefAssetRNames.Clear();
            if (TextureName != null)
                ameta.RefAssetRNames.Add(TextureName);
        }
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
            using (var attr = xnd.NewAttribute("UVAnim", 0, 0))
            {
                using (var ar = attr.GetWriter(512))
                {
                    ar.Write(this);
                }
                xnd.RootNode.AddAttribute(attr);
            }

            xnd.SaveXnd(name.Address);
            UEngine.Instance.SourceControlModule.AddFile(name.Address);
        }
        public static UUvAnim LoadXnd(UUvAnimManager manager, IO.TtXndNode node)
        {
            UUvAnim result = new UUvAnim();
            if (ReloadXnd(result, manager, node) == false)
                return null;
            return result;
        }
        public static bool ReloadXnd(UUvAnim material, UUvAnimManager manager, IO.TtXndNode node)
        {
            var attr = node.TryGetAttribute("UVAnim");
            if (attr.NativePointer != IntPtr.Zero)
            {
                using (var ar = attr.GetReader(null))
                {
                    try
                    {
                        ar.ReadTo(material, null);
                    }
                    catch (Exception ex)
                    {
                        Profiler.Log.WriteException(ex);
                    }
                }
            }
            return true;
        }
        [Rtti.Meta]
        [ReadOnly(true)]
        public RName AssetName
        {
            get;
            set;
        }
        #endregion
        [Rtti.Meta]
        public Vector2 Size { get; set; } = new Vector2(50, 50);
        RName mTextureName;
        [Rtti.Meta]
        [RName.PGRName(FilterExts = NxRHI.USrView.AssetExt)]
        public RName TextureName 
        { 
            get=> mTextureName; 
            set
            {
                mTextureName = value;
                if (value == null)
                    mTextureTask = null;
                else
                {
                    mTextureTask = UEngine.Instance.GfxDevice.TextureManager.GetTexture(value);
                }
            }
        }
        Thread.Async.TtTask<NxRHI.USrView>? mTextureTask;
        [Browsable(false)]
        public NxRHI.USrView Texture
        {
            get
            {
                if (mTextureTask == null)
                    return null;
                if (mTextureTask.Value.IsCompleted == false)
                    return null;
                return mTextureTask.Value.Result;
            }
        }

        [Rtti.Meta]
        public List<Vector4> FrameUVs { get; set; } = new List<Vector4>();
        public void GetUV(int frame, out Vector2 min, out Vector2 max)
        {
            if (FrameUVs.Count == 0)
            {
                min = Vector2.Zero;
                max = Vector2.One;
                return;
            }
            if (frame >= FrameUVs.Count || frame < 0)
            {
                frame = 0;
            }
            min.X = FrameUVs[frame].X;
            min.Y = FrameUVs[frame].Y;
            max.X = FrameUVs[frame].X + FrameUVs[frame].Z;
            max.Y = FrameUVs[frame].Y + FrameUVs[frame].W;
        }
        [EGui.Controls.PropertyGrid.UByte4ToColor4PickerEditor]
        [Rtti.Meta]
        public UInt32 Color { get; set; } = 0xFFFFFFFF;
        [Rtti.Meta]
        public float Duration { get; set; } = 1000.0f;
        public bool IsReadyToDraw()
        {
            if (mTextureTask == null)
                return true;
            else if (mTextureTask.Value.IsCompleted == false)
                return false;
            return true;
        }
        public void OnDraw(in ImDrawList cmdlist, in Vector2 rectMin, in Vector2 rectMax, int frame)
        {
            if (!IsReadyToDraw())
                return;
            if (mTextureTask != null)
            {
                Vector2 uvMin;
                Vector2 uvMax;
                this.GetUV(frame, out uvMin, out uvMax);
                unsafe
                {
                    cmdlist.AddImage(mTextureTask.Value.Result.GetTextureHandle().ToPointer(), in rectMin, in rectMax, in uvMin, in uvMax, Color);
                }
            }
            else
            {
                cmdlist.AddRectFilled(in rectMin, in rectMax, Color, 0, ImDrawFlags_.ImDrawFlags_RoundCornersAll);
            }
        }
    }

    public class UUvAnimManager
    {
        public void Cleanup()
        {
            UVAnims.Clear();
        }
        public Dictionary<RName, UUvAnim> UVAnims { get; } = new Dictionary<RName, UUvAnim>();
        public async Thread.Async.TtTask<UUvAnim> GetUVAnim(RName rn)
        {
            if (rn == null)
                return null;

            UUvAnim result;
            if (UVAnims.TryGetValue(rn, out result))
                return result;

            result = await UEngine.Instance.EventPoster.Post((state) =>
            {
                using (var xnd = IO.TtXndHolder.LoadXnd(rn.Address))
                {
                    if (xnd != null)
                    {
                        var material = UUvAnim.LoadXnd(this, xnd.RootNode);
                        if (material == null)
                            return null;

                        material.AssetName = rn;
                        return material;
                    }
                    else
                    {
                        return null;
                    }
                }
            }, Thread.Async.EAsyncTarget.AsyncIO);

            if (result != null)
            {
                UVAnims[rn] = result;
                return result;
            }

            return null;
        }
        public async Thread.Async.TtTask<UUvAnim> CreateUVAnim(RName rn)
        {
            UUvAnim result;
            result = await UEngine.Instance.EventPoster.Post((state) =>
            {
                using (var xnd = IO.TtXndHolder.LoadXnd(rn.Address))
                {
                    if (xnd != null)
                    {
                        var material = UUvAnim.LoadXnd(this, xnd.RootNode);
                        if (material == null)
                            return null;

                        material.AssetName = rn;
                        return material;
                    }
                    else
                    {
                        return null;
                    }
                }
            }, Thread.Async.EAsyncTarget.AsyncIO);
            return result;
        }
        public async Thread.Async.TtTask<bool> ReloadUVAnim(RName rn)
        {
            UUvAnim result;
            if (UVAnims.TryGetValue(rn, out result) == false)
                return true;

            var ok = await UEngine.Instance.EventPoster.Post((state) =>
            {
                using (var xnd = IO.TtXndHolder.LoadXnd(rn.Address))
                {
                    if (xnd != null)
                    {
                        return UUvAnim.ReloadXnd(result, this, xnd.RootNode);
                    }
                    else
                    {
                        return false;
                    }
                }
            }, Thread.Async.EAsyncTarget.AsyncIO);
            return ok;
        }
    }
}

namespace EngineNS.Graphics.Pipeline
{
    partial class UGfxDevice
    {
        public EGui.UUvAnimManager UvAnimManager { get; } = new EGui.UUvAnimManager();
    }
}