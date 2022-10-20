﻿using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.NxRHI
{
    [Rtti.Meta]
    public class USrViewAMeta : IO.IAssetMeta
    {
        protected override Color GetBorderColor()
        {
            return Color.LightPink;
        }
        public override string GetAssetExtType()
        {
            return USrView.AssetExt;
        }
        public override async System.Threading.Tasks.Task<IO.IAsset> LoadAsset()
        {
            return await UEngine.Instance.GfxDevice.TextureManager.GetTexture(GetAssetName());
        }
        System.Threading.Tasks.Task<USrView> SnapTask;
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
            cmdlist.AddRect(in start, in end, (uint)GetBorderColor().ToAbgr(),
                EGui.UCoreStyles.Instance.SnapRounding, ImDrawFlags_.ImDrawFlags_RoundCornersAll, EGui.UCoreStyles.Instance.SnapThinkness);

            cmdlist.AddText(in tpos, 0xFFFF00FF, name, null);
            //ImGuiAPI.PopClipRect();

            DrawPopMenu(ContentBrowser);
        }
        public override void OnShowIconTimout(int time)
        {
            if (SnapTask != null)
            {
                SnapTask = null;
            }
        }
        public unsafe override void OnDrawSnapshot(in ImDrawList cmdlist, ref Vector2 start, ref Vector2 end)
        {
            if (SnapTask == null)
            {
                var rc = UEngine.Instance.GfxDevice.RenderContext;
                SnapTask = UEngine.Instance.GfxDevice.TextureManager.GetTexture(this.GetAssetName(), 1);
                cmdlist.AddText(in start, 0xFFFFFFFF, "texture", null);
                return;
            }
            else if (SnapTask.IsCompleted == false)
            {
                cmdlist.AddText(in start, 0xFFFFFFFF, "texture", null);
                return;
            }
            unsafe
            {
                var uv0 = new Vector2(0, 0);
                var uv1 = new Vector2(1, 1);

                cmdlist.AddImage(SnapTask.Result.GetTextureHandle().ToPointer(), in start, in end, in uv0, in uv1, 0xFFFFFFFF);
            }
            cmdlist.AddText(in start, 0xFFFFFFFF, "texture", null);
        }
    }
    [Rtti.Meta]
    [USrView.Import]
    [IO.AssetCreateMenu(MenuName = "Texture")]
    public partial class USrView : AuxPtrType<NxRHI.ISrView>, IO.IAsset, IO.IStreaming
    {
        public class UPicDesc
        {
            public FPictureDesc Desc;
            public int sRGB { get => Desc.sRGB; set => Desc.sRGB = value; }
            public EEtcFormat EtcFormat { get => Desc.EtcFormat; set => Desc.EtcFormat = value; }
            public int MipLevel { get => Desc.MipLevel; set => Desc.MipLevel = value; }
            public int Width { get => Desc.Width; set => Desc.Width = value; }
            public int Height { get => Desc.Height; set => Desc.Height = value; }
            public List<Int32_2> MipSizes = new List<Int32_2>();
        }
        public EPixelFormat SrvFormat
        {
            get
            {
                return mCoreObject.GetBufferAsTexture().Desc.Format;
            }
        }
        public UPicDesc PicDesc { get; set; }
        public object TagObject;
        public static int NumOfInstance = 0;
        public static int NumOfGCHandle = 0;

        public class ImportAttribute : IO.IAssetCreateAttribute
        {
            bool bPopOpen = false;
            bool bFileExisting = false;
            RName mDir;
            string mName;
            string mSourceFile;
            public UPicDesc mDesc = new UPicDesc();
            ImGui.ImGuiFileDialog mFileDialog = UEngine.Instance.EditorInstance.FileDialog.mFileDialog;
            EGui.Controls.PropertyGrid.PropertyGrid PGAsset = new EGui.Controls.PropertyGrid.PropertyGrid();
            public override void DoCreate(RName dir, Rtti.UTypeDesc type, string ext)
            {
                mDir = dir;
                mDesc.Desc.SetDefault();
                var noused = PGAsset.Initialize();
                PGAsset.Target = mDesc;
            }
            public override unsafe void OnDraw(EGui.Controls.UContentBrowser ContentBrowser)
            {
                if (bPopOpen == false)
                    ImGuiAPI.OpenPopup($"Import SRV", ImGuiPopupFlags_.ImGuiPopupFlags_None);
                var visible = true;
                ImGuiAPI.SetNextWindowSize(new Vector2(200, 500), ImGuiCond_.ImGuiCond_FirstUseEver);
                if (ImGuiAPI.BeginPopupModal($"Import SRV", &visible, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                {
                    var sz = new Vector2(-1, 0);
                    if (ImGuiAPI.Button("Select Image", in sz))
                    {
                        mFileDialog.OpenModal("ChooseFileDlgKey", "Choose File", ".png,.PNG,.jpg,.JPG,.bmp,.BMP,.tga,.TGA", ".");
                    }
                    // display
                    if (mFileDialog.DisplayDialog("ChooseFileDlgKey"))
                    {
                        // action if OK
                        if (mFileDialog.IsOk() == true)
                        {
                            mSourceFile = mFileDialog.GetFilePathName();
                            string filePath = mFileDialog.GetCurrentPath();
                            using (var stream = System.IO.File.OpenRead(mSourceFile))
                            {
                                var image = StbImageSharp.ImageResult.FromStream(stream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                                if (image != null)
                                {
                                    mDesc.Width = image.Width;
                                    mDesc.Height = image.Height;

                                    int height = image.Height;
                                    int width = image.Width;
                                    int mipLevel = 0;
                                    do
                                    {
                                        height = height / 2;
                                        width = width / 2;
                                        mipLevel++;
                                    }
                                    while (height > 0 && width > 0);

                                    mDesc.MipLevel = mipLevel;
                                }
                                mName = IO.FileManager.GetPureName(mSourceFile);
                            }
                        }
                        // close
                        mFileDialog.CloseDialog();
                    }
                    if (bFileExisting)
                    {
                        var clr = new Vector4(1, 0, 0, 1);
                        ImGuiAPI.TextColored(in clr, $"Source:{mSourceFile}");
                    }
                    else
                    {
                        var clr = new Vector4(1, 1, 1, 1);
                        ImGuiAPI.TextColored(in clr, $"Source:{mSourceFile}");
                    }
                    ImGuiAPI.Separator();

                    using (var buffer = BigStackBuffer.CreateInstance(256))
                    {
                        buffer.SetText(mName);
                        ImGuiAPI.InputText("##in_rname", buffer.GetBuffer(), (uint)buffer.GetSize(), ImGuiInputTextFlags_.ImGuiInputTextFlags_None, null, (void*)0);
                        var name = buffer.AsText();
                        if (mName != name)
                        {
                            mName = name;
                            bFileExisting = IO.FileManager.FileExists(mDir.Address + mName + NxRHI.USrView.AssetExt);
                        }
                    }

                    sz = Vector2.Zero;
                    if (bFileExisting == false)
                    {
                        if (ImGuiAPI.Button("Create Asset", in sz))
                        {
                            if (ImportImage())
                            {
                                ImGuiAPI.CloseCurrentPopup();
                                ContentBrowser.mAssetImporter = null;
                            }
                        }
                        ImGuiAPI.SameLine(0, 20);
                    }
                    if (ImGuiAPI.Button("Cancel", in sz))
                    {
                        ImGuiAPI.CloseCurrentPopup();
                        ContentBrowser.mAssetImporter = null;
                    }

                    ImGuiAPI.Separator();

                    PGAsset.OnDraw(false, false, false);

                    ImGuiAPI.EndPopup();
                }
            }
            private bool ImportImage()
            {
                using (var stream = System.IO.File.OpenRead(mSourceFile))
                {
                    if (stream == null)
                        return false;
                    var image = StbImageSharp.ImageResult.FromStream(stream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                    if (image == null)
                        return false;

                    mDesc.Width = image.Width;
                    mDesc.Height = image.Height;

                    var rn = RName.GetRName(mDir.Name + mName + USrView.AssetExt);

                    var xnd = new IO.CXndHolder("CShaderResourceView", 0, 0);
                    USrView.SaveTexture(xnd.RootNode.mCoreObject, image, mDesc);
                    xnd.SaveXnd(rn.Address);

                    var ameta = new USrViewAMeta();
                    ameta.SetAssetName(rn);
                    ameta.AssetId = Guid.NewGuid();
                    ameta.TypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(typeof(USrView));
                    ameta.Description = $"This is a {typeof(USrView).FullName}\n";
                    ameta.SaveAMeta();

                    UEngine.Instance.AssetMetaManager.RegAsset(ameta);
                }
                return true;
            }

            public static bool ImportImage(string sourceFile, RName dir)
            {
                using (var stream = System.IO.File.OpenRead(sourceFile))
                {
                    if (stream == null)
                        return false;
                    var image = StbImageSharp.ImageResult.FromStream(stream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                    if (image == null)
                        return false;

                    var desc = new UPicDesc();

                    desc.Width = image.Width;
                    desc.Height = image.Height;
                    var name = IO.FileManager.GetPureName(sourceFile);

                    var rn = RName.GetRName(dir.Name.TrimEnd('\\').TrimEnd('/') + "/" + name + USrView.AssetExt, dir.RNameType);

                    var xnd = new IO.CXndHolder("CShaderResourceView", 0, 0);
                    USrView.SaveTexture(xnd.RootNode.mCoreObject, image, desc);
                    xnd.SaveXnd(rn.Address);

                    var ameta = new USrViewAMeta();
                    ameta.SetAssetName(rn);
                    ameta.AssetId = Guid.NewGuid();
                    ameta.TypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(typeof(USrView));
                    ameta.Description = $"This is a {typeof(USrView).FullName}\n";
                    ameta.SaveAMeta();

                    UEngine.Instance.AssetMetaManager.RegAsset(ameta);
                }
                return true;
            }
        }

        #region IAsset
        public const string AssetExt = ".srv";
        public IO.IAssetMeta CreateAMeta()
        {
            var result = new USrViewAMeta();
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
            //这里需要存盘的情况很少，正常来说srb是Image导入的时候生成的，不是保存出来的
            //mCoreObject.Save2Xnd()
            IO.FileManager.CopyFile(AssetName.Address, name.Address);
        }
        [Rtti.Meta]
        public RName AssetName
        {
            get;
            set;
        }
        #endregion

        #region TextureHandle
        IntPtr mTextureHandle = IntPtr.Zero;
        public bool IsHandle()
        {
            return mTextureHandle != IntPtr.Zero;
        }
        public IntPtr GetTextureHandle()
        {
            if (mTextureHandle == IntPtr.Zero)
            {
                mTextureHandle = System.Runtime.InteropServices.GCHandle.ToIntPtr(
                    System.Runtime.InteropServices.GCHandle.Alloc(this, System.Runtime.InteropServices.GCHandleType.Weak));
                System.Threading.Interlocked.Increment(ref NumOfGCHandle);
            }
            return mTextureHandle;
        }
        public void FreeTextureHandle()
        {
            if (mTextureHandle != IntPtr.Zero)
            {
                var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(mTextureHandle);
                handle.Free();
                mTextureHandle = IntPtr.Zero;
                System.Threading.Interlocked.Decrement(ref NumOfGCHandle);
            }
        }
        public override void Dispose()
        {
            FreeTextureHandle();
            base.Dispose();
        }
        #endregion

        #region IStreaming
        public int LevelOfDetail { get; set; }
        public int TargetLOD { get; set; }
        public int MaxLOD
        {
            get
            {
                return PicDesc.MipLevel;
            }
        }
        public System.Threading.Tasks.Task<bool> CurLoadTask { get; set; }
        public async System.Threading.Tasks.Task<bool> LoadLOD(int level)
        {
            if (level == 0)
            {
                return false;
            }
            if (level < 0 || level > MaxLOD)
                return false;
            var tex2d = await UEngine.Instance.EventPoster.Post(() =>
            {
                var xnd = IO.CXndHolder.LoadXnd(AssetName.Address);
                if (xnd == null)
                    return null;

                var tex = LoadPngTexture2DMipLevel(xnd.RootNode.mCoreObject, PicDesc, level);
                if (tex == null)
                    return null;
                return tex;
            }, Thread.Async.EAsyncTarget.AsyncIO);

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            LevelOfDetail = level;
            unsafe
            {
                if (tex2d == null)
                {
                    return this.mCoreObject.UpdateBuffer(rc.mCoreObject, new IGpuBufferData());
                }
                else
                {
                    return this.mCoreObject.UpdateBuffer(rc.mCoreObject, tex2d.mCoreObject.NativeSuper);
                }
            }
        }
        #endregion

        #region static function
        public static int CalcMipLevel(int width, int height, bool isAnyZero)
        {
            int mipLevel = 0;
            do
            {
                height = height / 2;
                width = width / 2;
                mipLevel++;

                if (isAnyZero)
                {
                    if ((height == 0 || width == 0))
                    {
                        break;
                    }
                }
                else
                {
                    if ((height == 0 && width == 0))
                    {
                        break;
                    }
                }

                if (height == 0)
                {
                    height = 1;
                }
                if (width == 0)
                {
                    width = 1;
                }
            }
            while (true);
            return mipLevel;
        }
        public static unsafe void SaveTexture(XndNode node, StbImageSharp.ImageResult image, UPicDesc desc)
        {
            using (var memStream = new System.IO.MemoryStream(image.Data.Length))
            {
                var writer = new StbImageWriteSharp.ImageWriter();
                writer.WritePng(image.Data, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, memStream);
                var pngData = memStream.ToArray();
                var attr = node.GetOrAddAttribute("Png", 0, 0);
                var ar = attr.GetWriter((ulong)memStream.Position);
                ar.WriteNoSize(pngData, (int)memStream.Position);
                attr.ReleaseWriter(ref ar);
            }

            int height = image.Height;
            int width = image.Width;
            int mipLevel = 0;
            var curImage = image;
            desc.MipSizes.Clear();
            var pngMipsNode = node.GetOrAddNode("PngMips", 0, 0);
            do
            {
                using (var memStream = new System.IO.MemoryStream(curImage.Data.Length))
                {
                    var writer = new StbImageWriteSharp.ImageWriter();
                    writer.WritePng(curImage.Data, curImage.Width, curImage.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, memStream);
                    //writer.WriteJpg(curImage.Data, curImage.Width, curImage.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, memStream, 100);
                    var pngData = memStream.ToArray();
                    var attr = pngMipsNode.GetOrAddAttribute($"PngMip{mipLevel}", 0, 0);
                    var ar = attr.GetWriter((ulong)memStream.Position);
                    ar.WriteNoSize(pngData, (int)memStream.Position);
                    attr.ReleaseWriter(ref ar);
                }
                desc.MipSizes.Add(new Int32_2() { X = width, Y = height });
                height = height / 2;
                width = width / 2;
                if ((height == 0 && width == 0))
                {
                    break;
                }
                mipLevel++;
                if (height == 0)
                    height = 1;
                if (width == 0)
                    width = 1;
                curImage = StbImageSharp.ImageProcessor.GetBoxDownSampler(curImage, width, height);
                if (desc.MipLevel > 0 && mipLevel == desc.MipLevel)
                    break;
            }
            while (true);

            {
                desc.Desc.MipLevel = mipLevel;
                var attr = node.GetOrAddAttribute("Desc", 0, 0);
                var ar = attr.GetWriter((ulong)sizeof(FPictureDesc));
                ar.Write(desc.Desc);
                ar.Write(desc.MipSizes.Count);
                for (int i = 0; i < desc.MipSizes.Count; i++)
                {
                    ar.Write(desc.MipSizes[i]);
                }
                attr.ReleaseWriter(ref ar);
            }
        }
        public static uint GetPixelByteWidth(EPixelFormat format)
        {
            switch (format)
            {
                case EPixelFormat.PXF_R8G8B8A8_UNORM:
                case EPixelFormat.PXF_R8G8B8A8_TYPELESS:
                case EPixelFormat.PXF_R8G8B8A8_SINT:
                case EPixelFormat.PXF_R8G8B8A8_UINT:
                case EPixelFormat.PXF_B8G8R8A8_UNORM:
                case EPixelFormat.PXF_B8G8R8A8_TYPELESS:
                case EPixelFormat.PXF_B8G8R8A8_UNORM_SRGB:
                    return 4;
                default:
                    return 0;
            }
        }
        public static unsafe UPicDesc LoadPicDesc(RName name)
        {
            using (var xnd = IO.CXndHolder.LoadXnd(name.Address))
            {
                var desc = new UPicDesc();
                var attr = xnd.RootNode.TryGetAttribute("Desc");
                var ar = attr.GetReader(null);

                ar.Read(out desc.Desc);
                attr.ReleaseReader(ref ar);
                return desc;
            }
        }
        public static unsafe StbImageSharp.ImageResult[] LoadPngImageLevels(RName name, uint mipLevel, ref UPicDesc desc)
        {
            using (var xnd = IO.CXndHolder.LoadXnd(name.Address))
            {
                {
                    if (desc != null)
                    {
                        var attr = xnd.RootNode.TryGetAttribute("Desc");
                        var ar = attr.GetReader(null);

                        ar.Read(out desc.Desc);
                        attr.ReleaseReader(ref ar);
                    }
                }
                var pngNode = xnd.RootNode.TryGetChildNode("PngMips");
                if (pngNode.NativePointer == IntPtr.Zero)
                    return null;

                if (mipLevel == 0)
                {
                    mipLevel = pngNode.GetNumOfAttribute();
                }
                var result = new StbImageSharp.ImageResult[mipLevel];
                for (uint i = 0; i < mipLevel; i++)
                {
                    var mipAttr = pngNode.TryGetAttribute($"PngMip{i}");
                    if (mipAttr.NativePointer == IntPtr.Zero)
                        return null;

                    var ar = mipAttr.GetReader(null);
                    byte[] data;
                    ar.ReadNoSize(out data, (int)mipAttr.GetReaderLength());
                    mipAttr.ReleaseReader(ref ar);

                    using (var memStream = new System.IO.MemoryStream(data, false))
                    {
                        result[i] = StbImageSharp.ImageResult.FromStream(memStream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                    }
                }
                return result;
            }
        }
        public static unsafe UTexture LoadPngTexture2DMipLevel(XndNode node, UPicDesc desc, int mipLevel)
        {
            if (mipLevel == 0)
                return null;
            var rc = UEngine.Instance.GfxDevice.RenderContext;

            var pngNode = node.TryGetChildNode("PngMips");
            if (pngNode.NativePointer == IntPtr.Zero)
                return null;

            var handles = stackalloc System.Runtime.InteropServices.GCHandle[mipLevel];
            try
            {
                var pInitData = stackalloc FMappedSubResource[mipLevel];

                StbImageSharp.ColorComponents colorComp = StbImageSharp.ColorComponents.RedGreenBlueAlpha;
                for (uint i = 0; i < mipLevel; i++)
                {
                    var ptr = pngNode.TryGetAttribute($"PngMip{desc.MipLevel - mipLevel + i}");
                    if (ptr.NativePointer == IntPtr.Zero)
                        return null;
                    var mipAttr = ptr;
                    var ar = mipAttr.GetReader(null);
                    byte[] data;
                    ar.ReadNoSize(out data, (int)mipAttr.GetReaderLength());
                    mipAttr.ReleaseReader(ref ar);

                    StbImageSharp.ImageResult image;
                    using (var memStream = new System.IO.MemoryStream(data, false))
                    {
                        image = StbImageSharp.ImageResult.FromStream(memStream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                    }
                    handles[i] = System.Runtime.InteropServices.GCHandle.Alloc(image.Data, System.Runtime.InteropServices.GCHandleType.Pinned);
                    //pInitData[i].SetDefault();
                    pInitData[i].m_pData = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0).ToPointer();
                    pInitData[i].m_RowPitch = (uint)image.Width * 4;
                    pInitData[i].m_DepthPitch = pInitData[i].m_RowPitch * (uint)image.Height;

                    colorComp = image.Comp;
                }

                var texDesc = new FTextureDesc();
                texDesc.SetDefault();
                texDesc.Width = (uint)desc.MipSizes[desc.MipLevel - mipLevel].X;
                texDesc.Height = (uint)desc.MipSizes[desc.MipLevel - mipLevel].Y;
                texDesc.MipLevels = (uint)mipLevel;
                texDesc.InitData = pInitData;
                switch (colorComp)
                {
                    case StbImageSharp.ColorComponents.RedGreenBlueAlpha:
                        texDesc.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
                        break;
                }

                var result = rc.CreateTexture(in texDesc);
                return result;
            }
            finally
            {
                for (uint i = 0; i < mipLevel; i++)
                {
                    if (handles[i].IsAllocated)
                        handles[i].Free();
                }
            }
        }
        public static async System.Threading.Tasks.Task<USrView> LoadSrvMipmap(RName rn, int mipLevel)
        {
            UPicDesc desc = new UPicDesc();
            var tex2d = await UEngine.Instance.EventPoster.Post(() =>
            {
                var xnd = IO.CXndHolder.LoadXnd(rn.Address);
                if (xnd == null)
                    return null;

                {
                    var attr = xnd.RootNode.TryGetAttribute("Desc");
                    var ar = attr.GetReader(null);
                    ar.Read(out desc.Desc);
                    int len;
                    ar.Read(out len);
                    for (int i = 0; i < len; i++)
                    {
                        Int32_2 tmp;
                        ar.Read(out tmp);
                        desc.MipSizes.Add(tmp);
                    }
                    attr.ReleaseReader(ref ar);
                }

                if (mipLevel == -1 || mipLevel > desc.MipLevel)
                    mipLevel = desc.MipLevel;

                var tex = LoadPngTexture2DMipLevel(xnd.RootNode.mCoreObject, desc, mipLevel);
                if (tex == null)
                    return null;

                return tex;
            }, Thread.Async.EAsyncTarget.AsyncIO);

            if (tex2d == null)
                return null;

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            var srvDesc = new FSrvDesc();
            srvDesc.SetTexture2D();
            srvDesc.Type = ESrvType.ST_Texture2D;
            srvDesc.Format = tex2d.mCoreObject.Desc.Format;
            srvDesc.Texture2D.MipLevels = (uint)mipLevel;
            
            var result = rc.CreateSRV(tex2d, in srvDesc);
            result.PicDesc = desc;
            result.LevelOfDetail = mipLevel;
            result.TargetLOD = mipLevel;
            result.AssetName = rn;
            return result;
        }
        #endregion
    }
    public class URenderTargetView : AuxPtrType<NxRHI.IRenderTargetView>
    {
        public URenderTargetView(IRenderTargetView ptr)
        {
            mCoreObject = ptr;
            mCoreObject.NativeSuper.AddRef();
        }
        public URenderTargetView()
        {

        }
    }
    public class UDepthStencilView : AuxPtrType<NxRHI.IDepthStencilView>
    {
    }

    public class UTextureManager : IO.UStreamingManager, ITickable
    {
        public UTextureManager()
        {

        }
        ~UTextureManager()
        {
            Cleanup();
            UEngine.Instance?.TickableManager.RemoveTickable(this);
        }
        public void Cleanup()
        {
            foreach (var i in StreamingAssets)
            {
                var srv = i.Value as USrView;
                if (srv == null)
                    continue;
                srv.Dispose();
            }
            StreamingAssets.Clear();
        }
        public USrView DefaultTexture;
        public async System.Threading.Tasks.Task Initialize(UEngine engine)
        {
            DefaultTexture = await GetTexture(engine.Config.DefaultTexture);
        }
        private Thread.UAwaitSessionManager<RName, USrView> mCreatingSession = new Thread.UAwaitSessionManager<RName, USrView>();
        List<RName> mWaitRemoves = new List<RName>();
        public async System.Threading.Tasks.Task<USrView> CreateTexture(string file)
        {
            if (EngineNS.IO.FileManager.FileExists(file) == false)
                return null;
            StbImageSharp.ImageResult image = await UEngine.Instance.EventPoster.Post(() =>
            {
                using (var memStream = new System.IO.FileStream(file, System.IO.FileMode.Open))
                {
                    return StbImageSharp.ImageResult.FromStream(memStream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                }
            }, Thread.Async.EAsyncTarget.AsyncIO);
            if (image == null)
            {
                return null;
            }

            var texDesc = new FTextureDesc();
            texDesc.SetDefault();
            texDesc.Width = (uint)image.Width;
            texDesc.Height = (uint)image.Height;
            texDesc.MipLevels = (uint)1;
            uint pixelWidth = 4;
            switch (image.Comp)
            {
                case StbImageSharp.ColorComponents.RedGreenBlueAlpha:
                    texDesc.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
                    pixelWidth = 4;
                    break;
            }
            unsafe
            {
                var data = new FMappedSubResource();
                data.RowPitch = texDesc.m_Width * pixelWidth;
                data.DepthPitch = data.RowPitch * texDesc.Height;
                texDesc.InitData = &data;
                fixed (byte* pData = &image.Data[0])
                {
                    data.pData = pData;

                    var rc = UEngine.Instance.GfxDevice.RenderContext;
                    var texture2d = rc.CreateTexture(in texDesc);

                    var srvDesc = new FSrvDesc();
                    srvDesc.SetTexture2D();
                    srvDesc.Type = ESrvType.ST_Texture2D;
                    srvDesc.Format = texDesc.Format;
                    srvDesc.Texture2D.MipLevels = texDesc.MipLevels;
                    var result = rc.CreateSRV(texture2d, in srvDesc);
                    //result.PicDesc.Desc = texDesc;
                    //result.LevelOfDetail = mipLevel;
                    //result.TargetLOD = mipLevel;
                    //result.AssetName = rn;

                    return result;
                }
            }
        }
        public async System.Threading.Tasks.Task<USrView> GetTexture(RName rn, int mipLevel = 1)
        {
            if (rn == null)
                return null;
            USrView srv = null;
            IO.IStreaming result;
            lock (StreamingAssets)
            {
                if (StreamingAssets.TryGetValue(rn, out result))
                {
                    srv = result as USrView;
                    if (srv == null)
                        return null;
                    srv.TargetLOD = mipLevel;
                    return srv;
                }
            }

            bool isNewSession;
            var session = mCreatingSession.GetOrNewSession(rn, out isNewSession);
            if (isNewSession == false)
            {
                return await session.Await();
            }

            try
            {
                srv = await USrView.LoadSrvMipmap(rn, mipLevel);
                if (srv == null)
                    return srv;
                lock (StreamingAssets)
                {
                    if (StreamingAssets.TryGetValue(rn, out result) == false)
                    {
                        StreamingAssets.Add(rn, srv);
                    }
                    else
                    {
                        srv = result as USrView;
                    }
                }

                return srv;
            }
            finally
            {
                if (srv != null)
                    mCreatingSession.FinishSession(rn, session, srv);
            }
        }
        public USrView TryGetTexture(RName rn)
        {
            if (rn == null)
                return null;
            USrView srv;
            IO.IStreaming result;
            if (StreamingAssets.TryGetValue(rn, out result))
            {
                srv = result as USrView;
                if (srv == null)
                    return null;
                return srv;
            }
            return null;
        }
        public unsafe override bool UpdateTargetLOD(IO.IStreaming asset)
        {
            var srv = asset as USrView;
            if (srv == null)
                return false;

            var now = UEngine.Instance.CurrentTickCount;
            var resState = srv.mCoreObject.GetResourceState();
            if (now - resState->GetAccessTime() > 15 * 1000 * 1000)
            {
                srv.TargetLOD = 1;
                //mWaitRemoves.Add(asset.AssetName);
                return true;
            }
            else
            {
                srv.TargetLOD = srv.MaxLOD;
                return true;
            }
        }
        int TickInterval = 150;
        int EllapsedRemainTime = 150;
        public void TickLogic(int ellapse)
        {
            EllapsedRemainTime -= ellapse;
            if (EllapsedRemainTime <= 0)
            {
                UpdateStreamingState();
                EllapsedRemainTime = TickInterval;
            }
            foreach (var i in mWaitRemoves)
            {
                StreamingAssets.Remove(i);
            }
            mWaitRemoves.Clear();
        }
        public void TickRender(int ellapse)
        {

        }
        public void TickSync(int ellapse)
        {

        }
    }
}