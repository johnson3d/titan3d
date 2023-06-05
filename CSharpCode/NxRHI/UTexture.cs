using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using EngienNS.Bricks.ImageDecoder;
using EngineNS.Bricks.CodeBuilder.MacrossNode;
using EngineNS.IO;
using Microsoft.Toolkit.HighPerformance;
using NPOI.OpenXmlFormats.Vml;
using NPOI.SS.Formula.Functions;
using NPOI.Util;
using Org.BouncyCastle.Crypto.IO;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.PixelFormats;
using StbImageSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
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
        public override string GetAssetTypeName()
        {
            return "SrView";
        }
        public override async System.Threading.Tasks.Task<IO.IAsset> LoadAsset()
        {
            return await UEngine.Instance.GfxDevice.TextureManager.GetTexture(GetAssetName());
        }
        Thread.Async.TtTask<USrView>? SnapTask;
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
        //    cmdlist.AddRect(in start, in end, (uint)GetBorderColor().ToAbgr(),
        //        EGui.UCoreStyles.Instance.SnapRounding, ImDrawFlags_.ImDrawFlags_RoundCornersAll, EGui.UCoreStyles.Instance.SnapThinkness);

        //    cmdlist.AddText(in tpos, 0xFFFF00FF, name, null);
        //    //ImGuiAPI.PopClipRect();

        //    DrawPopMenu(ContentBrowser);
        //}
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
                //cmdlist.AddText(in start, 0xFFFFFFFF, "texture", null);
                return;
            }
            else if (SnapTask.Value.IsCompleted == false)
            {
                cmdlist.AddText(in start, 0xFFFFFFFF, "loading...", null);
                return;
            }
            unsafe
            {
                var uv0 = new Vector2(0, 0);
                var uv1 = new Vector2(1, 1);

                if (SnapTask.Value.Result != null)
                    cmdlist.AddImage(SnapTask.Value.Result.GetTextureHandle().ToPointer(), in start, in end, in uv0, in uv1, 0xFFFFFFFF);
            }
            //cmdlist.AddText(in start, 0xFFFFFFFF, "texture", null);
        }
    }
    [Rtti.Meta]
    [USrView.Import]
    [IO.AssetCreateMenu(MenuName = "Texture")]
    public partial class USrView : AuxPtrType<NxRHI.ISrView>, IO.IAsset, IO.IStreaming
    {
        public class UPicDesc
        {
            public UPicDesc()
            {
                Desc.SetDefault();
            }
            public FPictureDesc Desc;
            //public uint dwStructureSize { get => Desc.dwStructureSize; set => Desc.dwStructureSize = value; }
            [ReadOnly(true)]
            public ETextureCompressFormat CompressFormat { get => Desc.CompressFormat; set => Desc.CompressFormat = value; }
            [ReadOnly(true)]
            public EPixelFormat Format { get => Desc.Format; set => Desc.Format = value; }
            [ReadOnly(true)]
            public uint CubeFaces { get => Desc.CubeFaces; set => Desc.CubeFaces = value; }
            public int MipLevel { get => Desc.MipLevel; set => Desc.MipLevel = value; }
            [ReadOnly(true)]
            public int Width { get => Desc.Width; set => Desc.Width = value; }
            [ReadOnly(true)]
            public int Height { get => Desc.Height; set => Desc.Height = value; }
            public byte BitNumRed { get => Desc.BitNumRed; set => Desc.BitNumRed = value; }
            public byte BitNumGreen { get => Desc.BitNumGreen; set => Desc.BitNumGreen = value; }
            public byte BitNumBlue { get => Desc.BitNumBlue; set => Desc.BitNumBlue = value; }
            public byte BitNumAlpha { get => Desc.BitNumAlpha; set => Desc.BitNumAlpha = value; }
            public bool DontCompress 
            {
                get => Desc.DontCompress != 0 ? true : false;
                set => Desc.DontCompress = value ? 1 : 0;
            }
            public bool sRGB
            {
                get => Desc.sRGB != 0 ? true : false;
                set => Desc.sRGB = value ? 1 : 0;
            }
            public bool StripOriginSource
            {
                get => Desc.StripOriginSource != 0 ? true : false;
                set => Desc.StripOriginSource = value ? 1 : 0;
            }
            public List<Vector3i> MipSizes { get; } = new List<Vector3i>();
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
            public override unsafe bool OnDraw(EGui.Controls.UContentBrowser ContentBrowser)
            {
                if (bPopOpen == false)
                    ImGuiAPI.OpenPopup($"Import SRV", ImGuiPopupFlags_.ImGuiPopupFlags_None);
                bool retValue = false;
                var visible = true;
                ImGuiAPI.SetNextWindowSize(new Vector2(200, 500), ImGuiCond_.ImGuiCond_FirstUseEver);
                if (ImGuiAPI.BeginPopupModal($"Import SRV", &visible, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                {
                    if(string.IsNullOrEmpty(ContentBrowser.CurrentImporterFile))
                    {
                        var sz = new Vector2(-1, 0);
                        if (ImGuiAPI.Button("Select Image", in sz))
                        {
                            mFileDialog.OpenModal("ChooseFileDlgKey", "Choose File", ".png,.PNG,.jpg,.JPG,.bmp,.BMP,.tga,.TGA,.exr,.EXR,.hdr,.HDR", ".");
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
                                    mName = IO.TtFileManager.GetPureName(mSourceFile);
                                }
                            }
                            // close
                            mFileDialog.CloseDialog();
                        }
                    }
                    else if(string.IsNullOrEmpty(mSourceFile))
                    {
                        mSourceFile = ContentBrowser.CurrentImporterFile;
                        mName = IO.TtFileManager.GetPureName(mSourceFile);
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

                    using (var buffer = BigStackBuffer.CreateInstance(128))
                    {
                        buffer.SetTextUtf8(mName);
                        ImGuiAPI.InputText("##in_rname", buffer.GetBuffer(), (uint)buffer.GetSize(), ImGuiInputTextFlags_.ImGuiInputTextFlags_None, null, (void*)0);
                        var name = buffer.AsText();
                        if (mName != name)
                        {
                            mName = name;
                            bFileExisting = IO.TtFileManager.FileExists(mDir.Address + mName + NxRHI.USrView.AssetExt);
                        }
                    }

                    var btSz = Vector2.Zero;
                    if (bFileExisting == false)
                    {
                        if (ImGuiAPI.Button("Create Asset", in btSz))
                        {
                            if (ImportImage())
                            {
                                ImGuiAPI.CloseCurrentPopup();
                                retValue = true;
                            }
                        }
                        ImGuiAPI.SameLine(0, 20);
                    }
                    if (ImGuiAPI.Button("Cancel", in btSz))
                    {
                        ImGuiAPI.CloseCurrentPopup();
                        retValue = true;
                    }

                    ImGuiAPI.Separator();

                    PGAsset.OnDraw(false, false, false);

                    ImGuiAPI.EndPopup();
                }
                if (!visible)
                    retValue = true;
                return retValue;
            }
            private unsafe bool ImportImage()
            {
                using (var stream = System.IO.File.OpenRead(mSourceFile))
                {
                    if (stream == null)
                        return false;

                    #region test hdr
                    var extName = IO.TtFileManager.GetExtName(mSourceFile);
                    if(extName == ".exr")
                    {
                        UImageDecoder imgDecoder = new UImageDecoder(mSourceFile);
                        imgDecoder.mInner.CheckFileExtention(mSourceFile, false);
                        var imageBuffer = new XImageBuffer();
                        imgDecoder.mInner.LoadImageX(imageBuffer, mSourceFile);
                        //imageBuffer.Create(imageBuffer,)
                        //imgDecoder.mInner.CheckFileContent(stream.get)
                    }
                    else if (extName == ".hdr")
                    {
                        //var imageHdr2 = StbImageSharp.ImageResultFloat.FromStream(stream, StbImageSharp.ColorComponents.RedGreenBlue);
                        //if (imageHdr2 == null)
                        //    return false;

                        //ColorRgbFloat[] colorData = new ColorRgbFloat[imageHdr2.Width * imageHdr2.Height];
                        //var fromData = imageHdr2.Data.AsSpan();
                        //var toData = colorData.AsSpan();
                        //MemoryMarshal.Cast<float, ColorRgbFloat>(fromData).CopyTo(toData);
                        //var inputMemory = colorData.AsMemory().AsMemory2D(imageHdr2.Height, imageHdr2.Width);

                        //BcEncoder encoder = new BcEncoder();
                        //encoder.OutputOptions.GenerateMipMaps = true;
                        //encoder.OutputOptions.Quality = CompressionQuality.Balanced;
                        //encoder.OutputOptions.Format = CompressionFormat.Bc6U;
                        //encoder.OutputOptions.FileFormat = OutputFileFormat.Dds; //Change to Dds for a dds file.
                        //var ddsFile = encoder.EncodeToDdsHdr(inputMemory);
                        //var fs = new System.IO.FileStream("f:\\renwind_bc6.dds", System.IO.FileMode.OpenOrCreate);
                        //ddsFile.Write(fs);
                    }
                    #endregion

                    var rn = RName.GetRName(mDir.Name + mName + USrView.AssetExt, mDir.RNameType);

                    var xnd = new IO.TtXndHolder("USrView", 0, 0);


                    if (extName == ".hdr")
                    {
                        var imageFloat = StbImageSharp.ImageResultFloat.FromStream(stream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                        if (imageFloat == null)
                            return false;

                        USrView.SaveTexture(rn, xnd.RootNode.mCoreObject, imageFloat, mDesc);
                    }
                    else
                    {
                        var image = StbImageSharp.ImageResult.FromStream(stream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                        if (image == null)
                            return false;

                        USrView.SaveTexture(rn, xnd.RootNode.mCoreObject, image, mDesc);
                    }

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

            public static bool ImportImage(string sourceFile, RName dir, UPicDesc desc)
            {
                using (var stream = System.IO.File.OpenRead(sourceFile))
                {
                    if (stream == null)
                        return false;
                    var image = StbImageSharp.ImageResult.FromStream(stream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                    if (image == null)
                        return false;

                    var name = IO.TtFileManager.GetPureName(sourceFile);
                    var rn = RName.GetRName(dir.Name.TrimEnd('\\').TrimEnd('/') + "/" + name + USrView.AssetExt, dir.RNameType);

                    return SaveSrv(image, rn, desc);
                }
            }

            public static bool SaveSrv(StbImageSharp.ImageResultFloat image, RName rn, UPicDesc desc)
            {
                desc.Width = image.Width;
                desc.Height = image.Height;

                var xnd = new IO.TtXndHolder("USrView", 0, 0);
                USrView.SaveTexture(rn, xnd.RootNode.mCoreObject, image, desc);
                xnd.SaveXnd(rn.Address);

                var ameta = new USrViewAMeta();
                ameta.SetAssetName(rn);
                ameta.AssetId = Guid.NewGuid();
                ameta.TypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(typeof(USrView));
                ameta.Description = $"This is a {typeof(USrView).FullName}\n";
                ameta.SaveAMeta();

                UEngine.Instance.AssetMetaManager.RegAsset(ameta);

                return true;
            }

            public static bool SaveSrv(StbImageSharp.ImageResult image, RName rn, UPicDesc desc)
            {
                desc.Width = image.Width;
                desc.Height = image.Height;

                var xnd = new IO.TtXndHolder("USrView", 0, 0);
                USrView.SaveTexture(rn, xnd.RootNode.mCoreObject, image, desc);
                xnd.SaveXnd(rn.Address);

                var ameta = new USrViewAMeta();
                ameta.SetAssetName(rn);
                ameta.AssetId = Guid.NewGuid();
                ameta.TypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(typeof(USrView));
                ameta.Description = $"This is a {typeof(USrView).FullName}\n";
                ameta.SaveAMeta();

                UEngine.Instance.AssetMetaManager.RegAsset(ameta);

                return true;
            }

            public override bool IsAssetSource(string fileExt)
            {
                fileExt = fileExt.TrimStart('.').ToLower();
                switch (fileExt)
                {
                    case "png":
                    case "jpg":
                    case "jpeg":
                    case "bmp":
                    case "tga":
                    case "exr":
                    case "hdr":
                        return true;
                }

                return false;
            }
            public override void ImportSource(string sourceFile, RName dir)
            {
                ImportImage(sourceFile, dir, new UPicDesc());
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
            var imgType = GetOriginImageType(this.AssetName);
            switch (imgType)
            {
                case EngienNS.Bricks.ImageDecoder.UImageType.PNG:
                    {
                        var image = LoadOriginImage(this.AssetName);
                        if (image == null)
                        {
                            Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Assets", $"SaveAssetTo failed: LoadOriginImage({AssetName}) = null");
                            return;
                        }
                        ImportAttribute.SaveSrv(image, name, this.PicDesc);
                    }
                    break;
                case EngienNS.Bricks.ImageDecoder.UImageType.HDR:
                    {
                        StbImageSharp.ImageResultFloat imageFloat = new StbImageSharp.ImageResultFloat();
                        LoadOriginImage(AssetName, ref imageFloat);
                        ImportAttribute.SaveSrv(imageFloat, name, this.PicDesc);
                    }
                    break;
            }

            var ameta = this.GetAMeta();
            if (ameta != null)
            {
                UpdateAMetaReferences(ameta);
                ameta.SaveAMeta();
            }
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
            var tex2d = await UEngine.Instance.EventPoster.Post((state) =>
            {
                var xnd = IO.TtXndHolder.LoadXnd(AssetName.Address);
                if (xnd == null)
                    return null;

                return LoadTexture2DMipLevel(xnd.RootNode, this.PicDesc, level);
            }, Thread.Async.EAsyncTarget.AsyncIO);

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            LevelOfDetail = level;
            unsafe
            {
                if (tex2d == null)
                {
                    //return this.mCoreObject.UpdateBuffer(rc.mCoreObject, new IGpuBufferData());
                    return false;
                }
                else
                {
                    //var desc = this.mCoreObject.Desc;
                    //desc.Texture2D.MipLevels = tex2d.mCoreObject.Desc.MipLevels;
                    //var srv = rc.mCoreObject.CreateSRV(tex2d.mCoreObject.NativeSuper, in desc);
                    //var fp = this.mCoreObject.NativeSuper.GetFingerPrint();
                    //this.Core_Release();
                    //this.mCoreObject = srv;
                    //this.mCoreObject.NativeSuper.SetFingerPrint(fp + 1);
                    //return true;
                    return this.mCoreObject.UpdateBuffer(rc.mCoreObject, tex2d.mCoreObject.NativeSuper);
                }
            }
        }
        #endregion

        public static UImageType GetOriginImageType(RName name)
        {
            using (var xnd = IO.TtXndHolder.LoadXnd(name.Address))
            {
                if(xnd==null)
                {
                    if (System.IO.File.Exists(name.Address + ".png") == true)
                        return UImageType.PNG;
                    else if (System.IO.File.Exists(name.Address + ".hdr") == true)
                        return UImageType.HDR;
                }
                var attr = xnd.RootNode.TryGetAttribute("Png");
                if(attr.IsValidPointer)
                    return UImageType.PNG;
                attr = xnd.RootNode.TryGetAttribute("Hdr");
                if (attr.IsValidPointer)
                    return UImageType.HDR;
            }
            return UImageType.PNG;
        }

        public static void LoadOriginImage(RName name, ref StbImageSharp.ImageResultFloat outImage)
        {
            using (var xnd = IO.TtXndHolder.LoadXnd(name.Address))
            {
                if (xnd == null)
                {
                    var sourceFile = name.Address + ".hdr";
                    using (var stream = System.IO.File.OpenRead(sourceFile))
                    {
                        if (stream == null)
                            return;
                        outImage = StbImageSharp.ImageResultFloat.FromStream(stream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                    }
                }
                var attr = xnd.RootNode.TryGetAttribute("Hdr");
                if (attr.IsValidPointer)
                {
                    byte[] rawData;
                    using (var ar = attr.GetReader(null))
                    {
                        ar.ReadNoSize(out rawData, (int)attr.GetReaderLength());
                    }

                    using (var memStream = new System.IO.MemoryStream(rawData))
                    {
                        outImage = StbImageSharp.ImageResultFloat.FromStream(memStream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                        return;
                    }
                }
            }
        }

        public static StbImageSharp.ImageResult LoadOriginImage(RName name)
        {
            using (var xnd = IO.TtXndHolder.LoadXnd(name.Address))
            {
                if (xnd == null)
                {
                    var sourceFile = name.Address + ".png";
                    using (var stream = System.IO.File.OpenRead(sourceFile))
                    {
                        if (stream == null)
                            return null;
                        return StbImageSharp.ImageResult.FromStream(stream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                    }
                }
                var attr = xnd.RootNode.TryGetAttribute("OriginSource");
                if (attr.IsValidPointer == false)
                {
                    attr = xnd.RootNode.TryGetAttribute("Png");
                }
                if (attr.IsValidPointer)
                {
                    byte[] pngData;
                    using (var ar = attr.GetReader(null))
                    {
                        ar.ReadNoSize(out pngData, (int)attr.GetReaderLength());
                    }

                    using (var memStream = new System.IO.MemoryStream(pngData))
                    {
                        var image = StbImageSharp.ImageResult.FromStream(memStream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                        return image;
                    }
                }
                else
                {
                    var sourceFile = name.Address + ".png";
                    if (System.IO.File.Exists(sourceFile) == false)
                    {
                        Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Assets", $"LoadOriginImage({name}) failed");
                        return null;
                    }
                    using (var stream = System.IO.File.OpenRead(sourceFile))
                    {
                        if (stream == null)
                            return null;
                        return StbImageSharp.ImageResult.FromStream(stream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                    }
                }
            }
        }
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
        public static unsafe void SaveTexture(RName assetName, XndNode node, StbImageSharp.ImageResultFloat image, UPicDesc desc)
        {
            var writeComp = UStbImageUtility.ConvertColorComponent(image.Comp);
            desc.Height = image.Height;
            desc.Width = image.Width;
            if (desc.StripOriginSource && assetName != null)
            {
                using (var memStream = new System.IO.FileStream(assetName.Address + ".hdr", System.IO.FileMode.OpenOrCreate))
                {
                    var writer = new StbImageWriteSharp.ImageWriter();
                    fixed (void* fptr = image.Data)
                    {
                        writer.WriteHdr(fptr, image.Width, image.Height, writeComp, memStream);
                    }
                }
            }
            else
            {
                using (var memStream = new System.IO.MemoryStream())
                {
                    var writer = new StbImageWriteSharp.ImageWriter();
                    fixed (void* fptr = image.Data)
                    {
                        writer.WriteHdr(fptr, image.Width, image.Height, writeComp, memStream);
                    }
                    var rawData = memStream.ToArray();


                    var rawAttr = node.GetOrAddAttribute("Hdr", 0, 0);
                    using (var ar = rawAttr.GetWriter((ulong)memStream.Position))
                    {
                        ar.WriteNoSize(rawData, (int)memStream.Position);
                    }
                    //var size = (uint)memStream.Length;
                    //if (size > 0)
                    //{
                    //    var len = CoreSDK.CompressBound_ZSTD(size) + 5;
                    //    using (var d = BigStackBuffer.CreateInstance((int)len))
                    //    {
                    //        void* srcBuffer = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(rawData, 0).ToPointer();
                    //        var wSize = (uint)CoreSDK.Compress_ZSTD(d.GetBuffer(), len, srcBuffer, size, 1);
                    //        rawAttr.Write(wSize);
                    //        rawAttr.Write(d.GetBuffer(), wSize);
                    //    }
                    //}
                }
            }

            if (image.Width % 4 != 0 || image.Height % 4 != 0)
            {
                desc.DontCompress = true;
            }
            int mipLevel = 0;
            var curImage = image;
            desc.MipSizes.Clear();
            if (desc.DontCompress == false)
            {
                if (UEngine.Instance.Config.CompressDxt)
                {
                    desc.CompressFormat = ETextureCompressFormat.TCF_BC6;
                }
                else if (UEngine.Instance.Config.CompressAstc)
                {
                    System.Diagnostics.Debug.Assert(false);
                    desc.CompressFormat = ETextureCompressFormat.TCF_Astc_4x4_Float;
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false);
                    desc.CompressFormat = ETextureCompressFormat.TCF_None;
                }
            }
            else
            {
                desc.CompressFormat = ETextureCompressFormat.TCF_None;
            }
            switch (desc.CompressFormat)
            {
                case ETextureCompressFormat.TCF_None:
                    {
                        var hdrMipsNode = node.GetOrAddNode("HdrMips", 0, 0);
                        mipLevel = SaveHdrMips(hdrMipsNode, curImage, desc);
                    }
                    break;
                case ETextureCompressFormat.TCF_BC6:
                    {
                        var pngMipsNode = node.GetOrAddNode("DxtMips", 0, 0);
                        mipLevel = SaveDxtMips_BcEncoder(pngMipsNode, curImage, desc);
                    }
                    break;
                case ETextureCompressFormat.TCF_Astc_4x4:
                case ETextureCompressFormat.TCF_Astc_4x4_Float:
                case ETextureCompressFormat.TCF_Astc_5x4:
                case ETextureCompressFormat.TCF_Astc_5x4_Float:
                case ETextureCompressFormat.TCF_Astc_5x5:
                case ETextureCompressFormat.TCF_Astc_5x5_Float:
                case ETextureCompressFormat.TCF_Astc_6x5:
                case ETextureCompressFormat.TCF_Astc_6x5_Float:
                case ETextureCompressFormat.TCF_Astc_6x6:
                case ETextureCompressFormat.TCF_Astc_6x6_Float:
                case ETextureCompressFormat.TCF_Astc_8x5:
                case ETextureCompressFormat.TCF_Astc_8x5_Float:
                case ETextureCompressFormat.TCF_Astc_8x6:
                case ETextureCompressFormat.TCF_Astc_8x6_Float:
                case ETextureCompressFormat.TCF_Astc_8x8:
                case ETextureCompressFormat.TCF_Astc_8x8_Float:
                case ETextureCompressFormat.TCF_Astc_10x6:
                case ETextureCompressFormat.TCF_Astc_10x6_Float:
                case ETextureCompressFormat.TCF_Astc_10x8:
                case ETextureCompressFormat.TCF_Astc_10x8_Float:
                case ETextureCompressFormat.TCF_Astc_10x10:
                case ETextureCompressFormat.TCF_Astc_10x10_Float:
                case ETextureCompressFormat.TCF_Astc_12x10:
                case ETextureCompressFormat.TCF_Astc_12x10_Float:
                case ETextureCompressFormat.TCF_Astc_12x12:
                case ETextureCompressFormat.TCF_Astc_12x12_Float:
                    {
                        var pngMipsNode = node.GetOrAddNode("AstcMips", 0, 0);
                        //mipLevel = SaveAstcMips_ActcEncoder(pngMipsNode, curImage, desc);
                    }
                    break;
            }

            {
                desc.Desc.MipLevel = mipLevel;
                desc.Desc.dwStructureSize = (uint)sizeof(FPictureDesc);
                var attr = node.GetOrAddAttribute("Desc", 0, 0);
                using (var ar = attr.GetWriter((ulong)sizeof(FPictureDesc)))
                {
                    ar.Write(desc.Desc);
                    ar.Write(desc.MipSizes.Count);
                    for (int i = 0; i < desc.MipSizes.Count; i++)
                    {
                        ar.Write(desc.MipSizes[i]);
                    }
                }
            }
        }
        public static unsafe void SaveTexture(RName assetName, XndNode node, StbImageSharp.ImageResult image, UPicDesc desc)
        {
            desc.Height = image.Height;
            desc.Width = image.Width;
            if (desc.StripOriginSource && assetName != null)
            {
                using (var memStream = new System.IO.FileStream(assetName.Address + ".png", System.IO.FileMode.OpenOrCreate))
                {
                    var writer = new StbImageWriteSharp.ImageWriter();
                    writer.WritePng(image.Data, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, memStream);
                }
            }
            else
            {
                using (var memStream = new System.IO.MemoryStream(image.Data.Length))
                {
                    var writer = new StbImageWriteSharp.ImageWriter();
                    writer.WritePng(image.Data, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, memStream);
                    var pngData = memStream.ToArray();

                    var size = (uint)memStream.Length;
                    if (size > 0)
                    {
                        var len = CoreSDK.CompressBound_ZSTD(size) + 5;
                        using (var d = BigStackBuffer.CreateInstance((int)len))
                        {
                            void *srcBuffer = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(pngData, 0).ToPointer();
                            var wSize = (uint)CoreSDK.Compress_ZSTD(d.GetBuffer(), len, srcBuffer, size, 1);
                        }
                    }

                    var attr = node.GetOrAddAttribute("Png", 0, 0);
                    using (var ar = attr.GetWriter((ulong)memStream.Position))
                    {
                        ar.WriteNoSize(pngData, (int)memStream.Position);
                    }
                }
            }

            if (image.Width % 4 != 0 || image.Height % 4 != 0)
            {
                desc.DontCompress = true;
            }
            int mipLevel = 0;
            var curImage = image;
            desc.MipSizes.Clear();
            if (desc.DontCompress == false)
            {
                if (UEngine.Instance.Config.CompressDxt)
                {
                    if (desc.BitNumAlpha == 8 || desc.BitNumAlpha == 4)
                    {
                        desc.CompressFormat = ETextureCompressFormat.TCF_Dxt3;
                    }
                    else if (desc.BitNumAlpha == 1)
                    {
                        desc.CompressFormat = ETextureCompressFormat.TCF_Dxt1a;
                    }
                    else
                    {
                        desc.CompressFormat = ETextureCompressFormat.TCF_Dxt1;
                    }
                }
                else if (UEngine.Instance.Config.CompressEtc)
                {
                    if (desc.BitNumAlpha == 8 || desc.BitNumAlpha == 4)
                    {
                        desc.CompressFormat = ETextureCompressFormat.TCF_Etc2_RGBA8;
                    }
                    else if (desc.BitNumAlpha == 1)
                    {
                        desc.CompressFormat = ETextureCompressFormat.TCF_Etc2_RGBA1;
                    }
                    else
                    {
                        desc.CompressFormat = ETextureCompressFormat.TCF_Etc2_RGB8;
                    }
                }
                else if (UEngine.Instance.Config.CompressAstc)
                {
                    System.Diagnostics.Debug.Assert(false);
                    desc.CompressFormat = ETextureCompressFormat.TCF_Etc2_RGBA8;
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false);
                    desc.CompressFormat = ETextureCompressFormat.TCF_None;
                }
            }
            else
            {
                desc.CompressFormat = ETextureCompressFormat.TCF_None;
            }
            switch (desc.CompressFormat)
            {
                case ETextureCompressFormat.TCF_None:
                    {
                        var pngMipsNode = node.GetOrAddNode("PngMips", 0, 0);
                        mipLevel = SavePngMips(pngMipsNode, curImage, desc);
                        switch (curImage.Comp)
                        {
                            case StbImageSharp.ColorComponents.RedGreenBlueAlpha:
                            case StbImageSharp.ColorComponents.RedGreenBlue:
                                desc.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
                                break;
                            case StbImageSharp.ColorComponents.Grey:
                                desc.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
                                break;
                            case StbImageSharp.ColorComponents.GreyAlpha:
                                desc.Format = EPixelFormat.PXF_R8G8_UNORM;
                                break;
                            default:
                                desc.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
                                break;
                        }
                    }
                    break;
                case ETextureCompressFormat.TCF_Dxt1://rgb:5-6-5 a:0
                case ETextureCompressFormat.TCF_Dxt1a://rgb:5-6-5 a:1
                case ETextureCompressFormat.TCF_Dxt3://rgb:5-6-5 a:8
                case ETextureCompressFormat.TCF_Dxt5://rg:8-8
                case ETextureCompressFormat.TCF_BC4:
                case ETextureCompressFormat.TCF_BC5:
                case ETextureCompressFormat.TCF_BC6:
                case ETextureCompressFormat.TCF_BC6_FLOAT:
                case ETextureCompressFormat.TCF_BC7_UNORM:
                    {
                        var pngMipsNode = node.GetOrAddNode("DxtMips", 0, 0);
                        mipLevel = SaveDxtMips_BcEncoder(pngMipsNode, curImage, desc);
                    }
                    break;
                case ETextureCompressFormat.TCF_Etc2_RGB8:
                case ETextureCompressFormat.TCF_Etc2_RGBA1:
                case ETextureCompressFormat.TCF_Etc2_RGBA8:
                case ETextureCompressFormat.TCF_Etc2_R11:
                case ETextureCompressFormat.TCF_Etc2_SIGNED_R11:
                case ETextureCompressFormat.TCF_Etc2_RG11:
                case ETextureCompressFormat.TCF_Etc2_SIGNED_RG11:
                    {
                        var pngMipsNode = node.GetOrAddNode("EtcMips", 0, 0);
                        mipLevel = SaveDxtMips_BcEncoder(pngMipsNode, curImage, desc);
                    }
                    break;
                case ETextureCompressFormat.TCF_Astc_4x4:
                case ETextureCompressFormat.TCF_Astc_4x4_Float:
                case ETextureCompressFormat.TCF_Astc_5x4:
                case ETextureCompressFormat.TCF_Astc_5x4_Float:
                case ETextureCompressFormat.TCF_Astc_5x5:
                case ETextureCompressFormat.TCF_Astc_5x5_Float:
                case ETextureCompressFormat.TCF_Astc_6x5:
                case ETextureCompressFormat.TCF_Astc_6x5_Float:
                case ETextureCompressFormat.TCF_Astc_6x6:
                case ETextureCompressFormat.TCF_Astc_6x6_Float:
                case ETextureCompressFormat.TCF_Astc_8x5:
                case ETextureCompressFormat.TCF_Astc_8x5_Float:
                case ETextureCompressFormat.TCF_Astc_8x6:
                case ETextureCompressFormat.TCF_Astc_8x6_Float:
                case ETextureCompressFormat.TCF_Astc_8x8:
                case ETextureCompressFormat.TCF_Astc_8x8_Float:
                case ETextureCompressFormat.TCF_Astc_10x6:
                case ETextureCompressFormat.TCF_Astc_10x6_Float:
                case ETextureCompressFormat.TCF_Astc_10x8:
                case ETextureCompressFormat.TCF_Astc_10x8_Float:
                case ETextureCompressFormat.TCF_Astc_10x10:
                case ETextureCompressFormat.TCF_Astc_10x10_Float:
                case ETextureCompressFormat.TCF_Astc_12x10:
                case ETextureCompressFormat.TCF_Astc_12x10_Float:
                case ETextureCompressFormat.TCF_Astc_12x12:
                case ETextureCompressFormat.TCF_Astc_12x12_Float:
                    {
                        var pngMipsNode = node.GetOrAddNode("AstcMips", 0, 0);
                        mipLevel = SaveAstcMips_ActcEncoder(pngMipsNode, curImage, desc);
                    }
                    break;
            }

            {
                desc.Desc.MipLevel = mipLevel;
                desc.Desc.dwStructureSize = (uint)sizeof(FPictureDesc);
                var attr = node.GetOrAddAttribute("Desc", 0, 0);
                using (var ar = attr.GetWriter((ulong)sizeof(FPictureDesc)))
                {
                    ar.Write(desc.Desc);
                    ar.Write(desc.MipSizes.Count);
                    for (int i = 0; i < desc.MipSizes.Count; i++)
                    {
                        ar.Write(desc.MipSizes[i]);
                    }
                }
            }
        }
        public static int SaveHdrMips(XndNode pngMipsNode, StbImageSharp.ImageResultFloat curImage, UPicDesc desc)
        {
            int mipLevel = 0;
            int height = curImage.Height;
            int width = curImage.Width;

            switch (curImage.Comp)
            {
                case StbImageSharp.ColorComponents.RedGreenBlue:
                    desc.Format = EPixelFormat.PXF_R32G32B32_FLOAT;
                    break;
                case StbImageSharp.ColorComponents.RedGreenBlueAlpha:
                    desc.Format = EPixelFormat.PXF_R32G32B32A32_FLOAT;
                    break;
                default:
                    desc.Format = EPixelFormat.PXF_R32G32B32A32_TYPELESS;
                    break;
            }

            desc.MipLevel = Math.Max(CalcMipLevel(curImage.Width, curImage.Height, true) - 3, 1);
            do
            {
                using (var memStream = new System.IO.MemoryStream())
                {
                    var writer = new StbImageWriteSharp.ImageWriter();
                    unsafe
                    {
                        var writeComp = UStbImageUtility.ConvertColorComponent(curImage.Comp);
                        fixed (void* ptr = curImage.Data)
                        {
                            writer.WriteHdr(ptr, curImage.Width, curImage.Height, writeComp, memStream);
                        }
                    }

                    var hdrData = memStream.ToArray();
                    var attr = pngMipsNode.GetOrAddAttribute($"HdrMip{mipLevel}", 0, 0);
                    using (var ar = attr.GetWriter((ulong)memStream.Position))
                    {
                        ar.WriteNoSize(hdrData, (int)memStream.Position);
                    }
                }
                desc.MipSizes.Add(new Vector3i() { X = width, Y = height, Z = width * (int)curImage.Comp * sizeof(float) });
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

            return mipLevel;
        }

        public static int SavePngMips(XndNode pngMipsNode, StbImageSharp.ImageResult curImage, UPicDesc desc)
        {
            int mipLevel = 0;
            int height = curImage.Height;
            int width = curImage.Width;
            do
            {
                using (var memStream = new System.IO.MemoryStream(curImage.Data.Length))
                {
                    var writer = new StbImageWriteSharp.ImageWriter();
                    writer.WritePng(curImage.Data, curImage.Width, curImage.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, memStream);
                    //writer.WriteJpg(curImage.Data, curImage.Width, curImage.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, memStream, 100);
                    var pngData = memStream.ToArray();
                    var attr = pngMipsNode.GetOrAddAttribute($"PngMip{mipLevel}", 0, 0);
                    using (var ar = attr.GetWriter((ulong)memStream.Position))
                    {
                        ar.WriteNoSize(pngData, (int)memStream.Position);
                    }
                }
                desc.MipSizes.Add(new Vector3i() { X = width, Y = height, Z = width * 4 });
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

            return mipLevel;
        }
        public unsafe static int SaveDxtMips(XndNode mipsNode, StbImageSharp.ImageResult curImage, UPicDesc desc)
        {
            System.Diagnostics.Debug.Assert(desc.DontCompress == false);

            var srcImage = new TextureCompress.FCubeImage();
            fixed (byte* pImageData = &curImage.Data[0])
            {
                srcImage.m_Image0 = (uint*)pImageData;
                var blobResult = new Support.UBlobObject();
                if (TextureCompress.CrunchWrap.CompressPixels(16, blobResult.mCoreObject, (uint)curImage.Width, (uint)curImage.Height, in srcImage, desc.CompressFormat, true, desc.sRGB, 0, 255))
                {
                    using (var reader = blobResult.CreateReader())
                    {
                        var ar = new IO.AuxReader<EngineNS.IO.UMemReader>(reader, null);
                        var loadDesc = new FPictureDesc();
                        ar.Read(out loadDesc);

                        System.Diagnostics.Debug.Assert(desc.Width == loadDesc.Width);
                        System.Diagnostics.Debug.Assert(desc.Height == loadDesc.Height);
                        desc.MipLevel = loadDesc.MipLevel;
                        desc.CubeFaces = loadDesc.CubeFaces;
                        desc.Format = loadDesc.Format;

                        desc.MipSizes.Clear();
                        for (uint i = 0; i < desc.Desc.CubeFaces; i++)
                        {
                            var faceNode = mipsNode.GetOrAddNode($"Face{i}", 0, 0);
                            for (uint j = 0; j < desc.Desc.MipLevel; j++)
                            {
                                var mipSize = new Vector3i();
                                ar.Read(out mipSize);
                                desc.MipSizes.Add(mipSize);
                                uint total_face_size = 0;
                                ar.Read(out total_face_size);
                                var pixels = new byte[total_face_size];
                                ar.ReadNoSize(pixels, (int)total_face_size);

                                var attr = faceNode.GetOrAddAttribute($"DxtMip{j}", 0, 0);
                                {
                                    using (var ar2 = attr.GetWriter((ulong)total_face_size))
                                    {
                                        ar2.WriteNoSize(pixels, (int)total_face_size);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return desc.MipLevel;
            //int mipLevel = 0;
            //int height = curImage.Height;
            //int width = curImage.Width;
            //do
            //{
            //    using (var memStream = new System.IO.MemoryStream(curImage.Data.Length))
            //    {
            //        var writer = new StbImageWriteSharp.ImageWriter();
            //        writer.WritePng(curImage.Data, curImage.Width, curImage.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, memStream);
            //        //writer.WriteJpg(curImage.Data, curImage.Width, curImage.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, memStream, 100);
            //        var pngData = memStream.ToArray();
            //        var attr = pngMipsNode.GetOrAddAttribute($"PngMip{mipLevel}", 0, 0);
            //        var ar = attr.GetWriter((ulong)memStream.Position);
            //        ar.WriteNoSize(pngData, (int)memStream.Position);
            //        attr.ReleaseWriter(ref ar);
            //    }
            //    desc.MipSizes.Add(new Int32_2() { X = width, Y = height });
            //    height = height / 2;
            //    width = width / 2;
            //    if ((height == 0 && width == 0))
            //    {
            //        break;
            //    }
            //    mipLevel++;
            //    if (height == 0)
            //        height = 1;
            //    if (width == 0)
            //        width = 1;
            //    curImage = StbImageSharp.ImageProcessor.GetBoxDownSampler(curImage, width, height);
            //    if (desc.MipLevel > 0 && mipLevel == desc.MipLevel)
            //        break;
            //}
            //while (true);

            //return mipLevel;
        }
        public unsafe static int SaveDxtMips_BcEncoder(XndNode mipsNode, StbImageSharp.ImageResultFloat curImage, UPicDesc desc)
        {
            System.Diagnostics.Debug.Assert(desc.DontCompress == false);


            desc.CubeFaces = 1;
            desc.MipLevel = CalcMipLevel(curImage.Width, curImage.Height, true) - 3;
            EPixelFormat descPixelFormat = EPixelFormat.PXF_UNKNOWN;
            switch (desc.CompressFormat)
            {
                case ETextureCompressFormat.TCF_BC6:
                    descPixelFormat = EPixelFormat.PXF_BC6H_UF16;
                    break;
            }
            desc.Format = descPixelFormat;
            desc.MipSizes.Clear();

            BcEncoder encoder = new BcEncoder();
            encoder.OutputOptions.GenerateMipMaps = true;
            encoder.OutputOptions.Quality = CompressionQuality.BestQuality;
            encoder.OutputOptions.Format = CompressionFormat.Bc6U;
            encoder.OutputOptions.FileFormat = OutputFileFormat.Dds;

            System.DateTime beginTime = System.DateTime.Now;
            Profiler.Log.WriteInfoSimple("Start SaveDxtMips_BcEncoder");
            for (uint i = 0; i < desc.Desc.CubeFaces; i++)
            {
                var faceNode = mipsNode.GetOrAddNode($"Face{i}", 0, 0);
                for (uint j = 0; j < desc.Desc.MipLevel; j++)
                {
                    var mipSize = new Vector3i();

                    ColorRgbFloat[] colorData = new ColorRgbFloat[curImage.Width * curImage.Height];
                    for(int iC = 0; iC < curImage.Width * curImage.Height; ++iC)
                    {
                        colorData[iC].r = curImage.Data[iC * (int)curImage.Comp];
                        colorData[iC].g = curImage.Data[iC * (int)curImage.Comp + Math.Min(1, (int)curImage.Comp-1)];
                        colorData[iC].b = curImage.Data[iC * (int)curImage.Comp + Math.Min(2, (int)curImage.Comp - 1)];
                    }
                    var inputMemory = colorData.AsMemory().AsMemory2D(curImage.Height, curImage.Width);
                    var pixelsBcn = encoder.EncodeToRawBytesHdr(inputMemory, (int)j, out mipSize.X, out mipSize.Y);

                    int blockWidth = 0;
                    int blockHeight = 0;
                    encoder.GetBlockCount(mipSize.X, mipSize.Y, out blockWidth, out blockHeight);
                    var blockSize = encoder.GetBlockSize();
                    mipSize.Z = blockWidth * blockSize;
                    desc.MipSizes.Add(mipSize);

                    var attr = faceNode.GetOrAddAttribute($"DxtMip{j}", 0, 0);
                    {
                        using (var ar2 = attr.GetWriter((ulong)pixelsBcn.Length))
                        {
                            ar2.WriteNoSize(pixelsBcn, (int)pixelsBcn.Length);
                        }
                    }
                }
            }

            System.DateTime endTime = System.DateTime.Now;
            Profiler.Log.WriteInfoSimple("End SaveDxtMips_BcEncoder");
            Profiler.Log.WriteInfoSimple("Use Time : " + endTime.Subtract(beginTime).TotalMilliseconds + " ms");

            return desc.MipLevel;
        }

        public unsafe static int SaveDxtMips_BcEncoder(XndNode mipsNode, StbImageSharp.ImageResult curImage, UPicDesc desc)
        {
            System.Diagnostics.Debug.Assert(desc.DontCompress == false);

            BcEncoder encoder = new BcEncoder();
            bool IsKtx = false;
            desc.CubeFaces = 1;
            desc.MipLevel = CalcMipLevel(curImage.Width, curImage.Height, true)-3;
            EPixelFormat descPixelFormat = EPixelFormat.PXF_UNKNOWN;
            switch (desc.CompressFormat)
            {
                case ETextureCompressFormat.TCF_Dxt1:
                    if (desc.sRGB)
                        descPixelFormat = EPixelFormat.PXF_BC1_UNORM_SRGB;
                    else
                        descPixelFormat = EPixelFormat.PXF_BC1_UNORM;
                    encoder.OutputOptions.Format = CompressionFormat.Bc1;
                    break;
                case ETextureCompressFormat.TCF_Dxt1a:
                    if (desc.sRGB)
                        descPixelFormat = EPixelFormat.PXF_BC1_UNORM_SRGB;
                    else
                        descPixelFormat = EPixelFormat.PXF_BC1_UNORM;
                    encoder.OutputOptions.Format = CompressionFormat.Bc1WithAlpha;
                    break;
                case ETextureCompressFormat.TCF_Dxt3:
                    if (desc.sRGB)
                        descPixelFormat = EPixelFormat.PXF_BC2_UNORM_SRGB;
                    else
                        descPixelFormat = EPixelFormat.PXF_BC2_UNORM;
                    encoder.OutputOptions.Format = CompressionFormat.Bc2;
                    break;
                case ETextureCompressFormat.TCF_Dxt5:
                    if (desc.sRGB)
                        descPixelFormat = EPixelFormat.PXF_BC3_UNORM_SRGB;
                    else
                        descPixelFormat = EPixelFormat.PXF_BC3_UNORM;
                    encoder.OutputOptions.Format = CompressionFormat.Bc3;
                    break;
                case ETextureCompressFormat.TCF_Etc2_RGB8:
                    if (desc.sRGB)
                        descPixelFormat = EPixelFormat.PXF_ETC2_SRGB8;
                    else
                        descPixelFormat = EPixelFormat.PXF_ETC2_RGB8;
                    encoder.OutputOptions.Format = CompressionFormat.Atc;
                    IsKtx = false;
                    break;
                case ETextureCompressFormat.TCF_Etc2_RGBA1:
                    if (desc.sRGB)
                        descPixelFormat = EPixelFormat.PXF_ETC2_SRGBA1;
                    else
                        descPixelFormat = EPixelFormat.PXF_ETC2_RGBA1;
                    encoder.OutputOptions.Format = CompressionFormat.AtcExplicitAlpha;
                    IsKtx = false; 
                    break;
                case ETextureCompressFormat.TCF_Etc2_RGBA8:
                    if (desc.sRGB)
                        descPixelFormat = EPixelFormat.PXF_ETC2_SRGBA8;
                    else
                        descPixelFormat = EPixelFormat.PXF_ETC2_RGBA8;
                    encoder.OutputOptions.Format = CompressionFormat.AtcInterpolatedAlpha;
                    IsKtx = false;
                    break;
                case ETextureCompressFormat.TCF_Etc2_RG11:
                    descPixelFormat = EPixelFormat.PXF_ETC2_RG11;
                    IsKtx = false;
                    break;
                case ETextureCompressFormat.TCF_Etc2_SIGNED_RG11:
                    descPixelFormat = EPixelFormat.PXF_ETC2_SIGNED_RG11;
                    IsKtx = false;
                    break;
                case ETextureCompressFormat.TCF_Etc2_R11:
                    descPixelFormat = EPixelFormat.PXF_ETC2_R11;
                    IsKtx = false;
                    break;
                case ETextureCompressFormat.TCF_Etc2_SIGNED_R11:
                    descPixelFormat = EPixelFormat.PXF_ETC2_SIGNED_R11;
                    IsKtx = false;
                    break;
            }
            desc.Format = descPixelFormat;
            desc.MipSizes.Clear();

            encoder.OutputOptions.FileFormat = IsKtx ? OutputFileFormat.Ktx : OutputFileFormat.Dds;
            encoder.OutputOptions.GenerateMipMaps = true;
            encoder.OutputOptions.Quality = CompressionQuality.BestQuality;
            encoder.OutputOptions.Format = CompressionFormat.Bc2;

            PixelFormat pixelFormat = PixelFormat.Rgba32;
            switch (curImage.Comp)
            {
                case StbImageSharp.ColorComponents.RedGreenBlueAlpha:
                    pixelFormat = PixelFormat.Rgba32;
                    break;
                case StbImageSharp.ColorComponents.RedGreenBlue:
                    pixelFormat = PixelFormat.Rgb24;
                    break;
            }

            for (uint i = 0; i < desc.Desc.CubeFaces; i++)
            {
                var faceNode = mipsNode.GetOrAddNode($"Face{i}", 0, 0);
                for (uint j = 0; j < desc.Desc.MipLevel; j++)
                {
                    var mipSize = new Vector3i();
                    var pixelsBcn = encoder.EncodeToRawBytes(curImage.Data.AsSpan(), curImage.Width, curImage.Height, pixelFormat, (int)j, out mipSize.X, out mipSize.Y);
                    int blockWidth = 0;
                    int blockHeight = 0;
                    encoder.GetBlockCount(mipSize.X, mipSize.Y, out blockWidth, out blockHeight);
                    var blockSize = encoder.GetBlockSize();
                    mipSize.Z = blockWidth * blockSize;
                    desc.MipSizes.Add(mipSize);

                    if (IsKtx)
                    {
                        var attr = faceNode.GetOrAddAttribute($"EtcMip{j}", 0, 0);
                        {
                            using (var ar2 = attr.GetWriter((ulong)pixelsBcn.Length))
                            {
                                ar2.WriteNoSize(pixelsBcn, (int)pixelsBcn.Length);
                            }
                        }
                    }
                    else
                    {
                        var attr = faceNode.GetOrAddAttribute($"DxtMip{j}", 0, 0);
                        {
                            using (var ar2 = attr.GetWriter((ulong)pixelsBcn.Length))
                            {
                                ar2.WriteNoSize(pixelsBcn, (int)pixelsBcn.Length);
                            }
                        }
                    }
                }
            }

            return desc.MipLevel;
        }
        public unsafe static int SaveAstcMips_ActcEncoder(XndNode mipsNode, StbImageSharp.ImageResult curImage, UPicDesc desc)
        {
            System.Diagnostics.Debug.Assert(desc.DontCompress == false);
            System.Diagnostics.Debug.Assert(false);
            return 0;
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
        public static unsafe StbImageSharp.ImageResult[] LoadImageLevels(RName name, uint mipLevel, ref UPicDesc desc)
        {
            using (var xnd = IO.TtXndHolder.LoadXnd(name.Address))
            {
                {
                    if (desc != null)
                    {
                        var attr = xnd.RootNode.TryGetAttribute("Desc");
                        using (var ar = attr.GetReader(null))
                        {
                            ar.Read(out desc.Desc);
                        }
                    }
                }
                var pngNode = xnd.RootNode.TryGetChildNode("PngMips");
                if (pngNode.IsValidPointer)
                {
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

                        byte[] data;
                        using (var ar = mipAttr.GetReader(null))
                        {
                            ar.ReadNoSize(out data, (int)mipAttr.GetReaderLength());
                        }

                        using (var memStream = new System.IO.MemoryStream(data, false))
                        {
                            result[i] = StbImageSharp.ImageResult.FromStream(memStream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                        }
                    }
                    return result;
                }
                else
                {
                    //todo
                    pngNode = xnd.RootNode.TryGetChildNode("DxtMips");
                }
                return null;
            }
        }
        public static unsafe Support.UBlobObject[] LoadPixelMipLevels(RName name, uint mipLevel, UPicDesc desc)
        {
            using (var xnd = IO.TtXndHolder.LoadXnd(name.Address))
            {
                if (desc != null)
                {
                    LoadPictureDesc(xnd.RootNode, desc);
                }

                var pngNode = xnd.RootNode.TryGetChildNode("PngMips");
                if (pngNode.IsValidPointer)
                {
                    if (mipLevel == 0)
                    {
                        mipLevel = pngNode.GetNumOfAttribute();
                    }
                    var result = new StbImageSharp.ImageResult[mipLevel];
                    var blobs = new Support.UBlobObject[mipLevel];
                    for (uint i = 0; i < mipLevel; i++)
                    {
                        var mipAttr = pngNode.TryGetAttribute($"PngMip{i}");
                        if (mipAttr.NativePointer == IntPtr.Zero)
                            return null;

                        byte[] data;
                        using (var ar = mipAttr.GetReader(null))
                        {
                            ar.ReadNoSize(out data, (int)mipAttr.GetReaderLength());
                        }

                        using (var memStream = new System.IO.MemoryStream(data, false))
                        {
                            result[i] = StbImageSharp.ImageResult.FromStream(memStream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                            blobs[i] = new Support.UBlobObject();
                            fixed (byte* p = &result[i].Data[0])
                            {
                                blobs[i].PushData(p, (uint)result[i].Data.Length);
                            }
                        }
                    }
                    return blobs;
                }
                else
                {
                    //todo
                    var dxtNode = xnd.RootNode.TryGetChildNode("DxtMips");
                    if (dxtNode.IsValidPointer)
                    {
                        for (uint j = 0; j < desc.Desc.CubeFaces; j++)
                        {
                            var faceNode = dxtNode.TryGetChildNode($"Face{j}");
                            if (faceNode.IsValidPointer == false)
                            {
                                continue;
                            }
                            if (mipLevel == 0)
                            {
                                mipLevel = faceNode.GetNumOfAttribute();
                            }
                            var blobs = new Support.UBlobObject[mipLevel];
                            for (uint i = 0; i < mipLevel; i++)
                            {
                                var realLevel = desc.MipLevel - mipLevel + i;
                                var ptr = faceNode.TryGetAttribute($"DxtMip{realLevel}");
                                if (ptr.NativePointer == IntPtr.Zero)
                                    return null;
                                var mipAttr = ptr;
                                byte[] data;
                                using (var ar = mipAttr.GetReader(null))
                                {
                                    ar.ReadNoSize(out data, (int)mipAttr.GetReaderLength());
                                }

                                blobs[i] = new Support.UBlobObject();
                                fixed (byte* p = &data[0])
                                {
                                    blobs[i].PushData(p, (uint)data.Length);
                                }
                            }
                            return blobs;
                        }
                    }
                }
                return null;
            }
        }
        public static unsafe UPicDesc LoadPictureDesc(RName name)
        {
            using (var xnd = IO.TtXndHolder.LoadXnd(name.Address))
            {
                return LoadPictureDesc(xnd.RootNode);
            }
        }
        public static unsafe void LoadPictureDesc(IO.TtXndNode node, UPicDesc desc)
        {
            var attr = node.TryGetAttribute("Desc");
            using (var ar = attr.GetReader(null))
            {
                //ar.Read(out desc.Desc);
                uint headSize = 0;
                ar.Read(out headSize);
                System.Diagnostics.Debug.Assert(headSize <= sizeof(FPictureDesc));
                fixed (FPictureDesc* pDesc = &desc.Desc)
                {
                    var pDescData = (byte*)pDesc + sizeof(uint);
                    ar.ReadPtr(pDescData, (int)headSize - sizeof(uint));
                }

                int len;
                ar.Read(out len);
                desc.MipSizes.Clear();
                for (int i = 0; i < len; i++)
                {
                    Vector3i tmp = new Vector3i();
                    if (headSize == 20)
                    {
                        ar.Read(out tmp.X);
                        ar.Read(out tmp.Y);
                        tmp.Z = tmp.X * 4;
                    }
                    else
                    {
                        ar.Read(out tmp);
                    }
                    desc.MipSizes.Add(tmp);
                }
            }
        }
        public static unsafe UPicDesc LoadPictureDesc(IO.TtXndNode node)
        {
            UPicDesc desc = new UPicDesc();
            LoadPictureDesc(node, desc);
            return desc;
        }
        public static unsafe UTexture LoadTexture2DMipLevel(IO.TtXndNode node, UPicDesc desc, int level)
        {
            switch (desc.CompressFormat)
            {
                case ETextureCompressFormat.TCF_None:
                    {
                        var pngNode = node.TryGetChildNode("PngMips");
                        if(pngNode.IsValidPointer)
                            return LoadPngTexture2DMipLevel(node.mCoreObject, desc, level);
                        else
                        {
                            var hdrNode = node.TryGetChildNode("HdrMips");
                            if (hdrNode.IsValidPointer)
                                return LoadHdrTexture2DMipLevel(node.mCoreObject, desc, level);
                        }
                        return null;
                    }
                case ETextureCompressFormat.TCF_Dxt1:
                case ETextureCompressFormat.TCF_Dxt1a:
                case ETextureCompressFormat.TCF_Dxt3:
                case ETextureCompressFormat.TCF_Dxt5:
                case ETextureCompressFormat.TCF_BC6:
                case ETextureCompressFormat.TCF_BC6_FLOAT:
                    {
                        return LoadDxtTexture2DMipLevel(node.mCoreObject, desc, level);
                    }
                default:
                    return null;
            }
        }
        #region Load Mips
        private static unsafe UTexture LoadHdrTexture2DMipLevel(XndNode node, UPicDesc desc, int mipLevel)
        {
            if (mipLevel == 0)
                return null;
            var rc = UEngine.Instance.GfxDevice.RenderContext;

            var hdrNode = node.TryGetChildNode("HdrMips");
            if (hdrNode.NativePointer == IntPtr.Zero)
                return null;

            var handles = stackalloc System.Runtime.InteropServices.GCHandle[mipLevel];
            try
            {
                var pInitData = stackalloc FMappedSubResource[mipLevel];

                StbImageSharp.ColorComponents colorComp = StbImageSharp.ColorComponents.RedGreenBlue;
                switch (desc.Format)
                {
                    case EPixelFormat.PXF_R32G32B32_FLOAT:
                        colorComp = ColorComponents.RedGreenBlue;
                        break;
                    case EPixelFormat.PXF_R32G32B32A32_FLOAT:
                        colorComp = ColorComponents.RedGreenBlueAlpha;
                        break;
                    default:
                        colorComp = ColorComponents.RedGreenBlueAlpha;
                        break;
                }
                for (uint i = 0; i < mipLevel; i++)
                {
                    var realLevel = desc.MipLevel - mipLevel + i;
                    var ptr = hdrNode.TryGetAttribute($"HdrMip{realLevel}");
                    if (ptr.NativePointer == IntPtr.Zero)
                        return null;
                    var mipAttr = ptr;
                    byte[] data;
                    using (var ar = mipAttr.GetReader(null))
                    {
                        ar.ReadNoSize(out data, (int)mipAttr.GetReaderLength());
                    }

                    StbImageSharp.ImageResultFloat image;
                    using (var memStream = new System.IO.MemoryStream(data, false))
                    {
                        image = StbImageSharp.ImageResultFloat.FromStream(memStream, colorComp);
                    }
                    handles[i] = System.Runtime.InteropServices.GCHandle.Alloc(image.Data, System.Runtime.InteropServices.GCHandleType.Pinned);
                    //pInitData[i].SetDefault();
                    pInitData[i].m_pData = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0).ToPointer();
                    pInitData[i].m_RowPitch = (uint)image.Width * (uint)colorComp * sizeof(float);
                    pInitData[i].m_DepthPitch = pInitData[i].m_RowPitch * (uint)image.Height;
                }

                var texDesc = new FTextureDesc();
                texDesc.SetDefault();
                texDesc.Width = (uint)desc.MipSizes[desc.MipLevel - mipLevel].X;
                texDesc.Height = (uint)desc.MipSizes[desc.MipLevel - mipLevel].Y;
                texDesc.MipLevels = (uint)mipLevel;
                texDesc.InitData = pInitData;
                texDesc.Format = desc.Format;

                var result = rc.CreateTexture(in texDesc);
                if (result == null)
                    return null;
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

        private static unsafe UTexture LoadPngTexture2DMipLevel(XndNode node, UPicDesc desc, int mipLevel)
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
                    var realLevel = desc.MipLevel - mipLevel + i;
                    var ptr = pngNode.TryGetAttribute($"PngMip{realLevel}");
                    if (ptr.NativePointer == IntPtr.Zero)
                        return null;
                    var mipAttr = ptr;
                    byte[] data;
                    using (var ar = mipAttr.GetReader(null))
                    {
                        ar.ReadNoSize(out data, (int)mipAttr.GetReaderLength());
                    }

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
                if (result == null)
                    return null;
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
        private static unsafe UTexture LoadDxtTexture2DMipLevel(XndNode node, UPicDesc desc, int mipLevel)
        {
            if (mipLevel == 0)
                return null;
            var rc = UEngine.Instance.GfxDevice.RenderContext;

            var pngNode = node.TryGetChildNode("DxtMips");
            if (pngNode.NativePointer == IntPtr.Zero)
                return null;

            var handles = stackalloc System.Runtime.InteropServices.GCHandle[mipLevel];
            try
            {
                var pInitData = stackalloc FMappedSubResource[mipLevel];

                for (uint j = 0; j < desc.Desc.CubeFaces; j++)
                {
                    var faceNode = pngNode.TryGetChildNode($"Face{j}");
                    if (faceNode.IsValidPointer == false)
                    {
                        continue;
                    }
                    for (uint i = 0; i < mipLevel; i++)
                    {
                        var realLevel = desc.MipLevel - mipLevel + i;
                        var ptr = faceNode.TryGetAttribute($"DxtMip{realLevel}");
                        if (ptr.NativePointer == IntPtr.Zero)
                            return null;
                        var mipAttr = ptr;
                        byte[] data;
                        using (var ar = mipAttr.GetReader(null))
                        {
                            ar.ReadNoSize(out data, (int)mipAttr.GetReaderLength());
                        }

                        handles[i] = System.Runtime.InteropServices.GCHandle.Alloc(data, System.Runtime.InteropServices.GCHandleType.Pinned);
                        //pInitData[i].SetDefault();
                        pInitData[i].m_pData = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(data, 0).ToPointer();
                        pInitData[i].m_RowPitch = (uint)desc.MipSizes[(int)realLevel].Z;
                        pInitData[i].m_DepthPitch = pInitData[i].m_RowPitch * (uint)desc.MipSizes[(int)realLevel].Y;
                    }
                }
                var texDesc = new FTextureDesc();
                texDesc.SetDefault();
                texDesc.Width = (uint)desc.MipSizes[desc.MipLevel - mipLevel].X;
                texDesc.Height = (uint)desc.MipSizes[desc.MipLevel - mipLevel].Y;
                texDesc.MipLevels = (uint)mipLevel;
                texDesc.InitData = pInitData;
                texDesc.Format = desc.Desc.Format;

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
        #endregion
        public static unsafe void SaveOriginPng(RName rn)
        {
            if (System.IO.File.Exists(rn.Address) == false)
                return;

            var imgType = GetOriginImageType(rn);
            switch (imgType)
            {
                case EngienNS.Bricks.ImageDecoder.UImageType.PNG:
                    {
                        var image = LoadOriginImage(rn);
                        if (image == null)
                        {
                            return;
                        }
                        using (var memStream = new System.IO.FileStream(rn.Address + ".png", System.IO.FileMode.OpenOrCreate))
                        {
                            var writer = new StbImageWriteSharp.ImageWriter();
                            writer.WritePng(image.Data, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, memStream);
                        }
                    }
                    break;
                case EngienNS.Bricks.ImageDecoder.UImageType.HDR:
                    {
                        StbImageSharp.ImageResultFloat imageFloat = new StbImageSharp.ImageResultFloat();
                        LoadOriginImage(rn, ref imageFloat);
                        using (var memStream = new System.IO.FileStream(rn.Address + ".hdr", System.IO.FileMode.OpenOrCreate))
                        {
                            var writer = new StbImageWriteSharp.ImageWriter();
                            fixed (void* fptr = imageFloat.Data)
                            {
                                writer.WriteHdr(fptr, imageFloat.Width, imageFloat.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, memStream);
                            }
                        }
                    }
                    break;
            }
        }
        public static async System.Threading.Tasks.Task<USrView> LoadSrvMipmap(RName rn, int mipLevel)
        {
            UPicDesc desc = null;
            var tex2d = await UEngine.Instance.EventPoster.Post((state) =>
            {
                var xnd = IO.TtXndHolder.LoadXnd(rn.Address);
                if (xnd == null)
                    return null;

                desc = LoadPictureDesc(xnd.RootNode);

                if (mipLevel == -1 || mipLevel > desc.MipLevel)
                    mipLevel = desc.MipLevel;

                return LoadTexture2DMipLevel(xnd.RootNode, desc, mipLevel);
            }, Thread.Async.EAsyncTarget.AsyncIO);

            if (tex2d == null)
            {
                if (UEngine.Instance.PlayMode == EPlayMode.Editor)
                    SaveOriginPng(rn);
                return null;
            }

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

    public class UTextureManager : IO.TtStreamingManager, ITickable
    {
        public int GetTickOrder()
        {
            return 0;
        }
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
        public async Thread.Async.TtTask<USrView> CreateTexture(string file)
        {
            if (EngineNS.IO.TtFileManager.FileExists(file) == false)
                return null;
            StbImageSharp.ImageResult image = await UEngine.Instance.EventPoster.Post((state) =>
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
        public async Thread.Async.TtTask<USrView> GetOrNewTexture(RName rn, int mipLevel = 1)
        {
            var result = await GetTexture(rn, mipLevel);
            if (result != null)
                return result;

            return await USrView.LoadSrvMipmap(rn, mipLevel);
        }
        public async Thread.Async.TtTask<USrView> GetTexture(RName rn, int mipLevel = 1)
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

            var nowFrame = UEngine.Instance.CurrentTickFrame;
            var resState = srv.mCoreObject.GetResourceState();
            if (nowFrame - resState->GetAccessFrame() > 15 * (uint)UEngine.Instance.Config.TargetFps)//15 second & 60 target fps
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
        float TickInterval = 150;
        float EllapsedRemainTime = 150;
        [ThreadStatic]
        private static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(UTextureManager), nameof(TickLogic));
        public void TickLogic(float ellapse)
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
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
        }
        public void TickRender(float ellapse)
        {

        }
        public void TickBeginFrame(float ellapse)
        {

        }
        public void TickSync(float ellapse)
        {

        }
    }
}
