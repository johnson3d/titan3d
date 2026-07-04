using Assimp.Unmanaged;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using BCnEncoder.Shared.ImageFiles;
using CommunityToolkit.HighPerformance;
using EngineNS.Bricks.ImageDecoder;
using EngineNS.EGui.Controls;
using EngineNS.EGui.Controls.PropertyGrid;
using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.IO;
using EngineNS.NxRHI;
using EngineNS.Support;
using Jither.OpenEXR;
using MathNet.Numerics.Financial;
using StbImageSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static EngineNS.EGui.Controls.PropertyGrid.PGPropertyOrderAttribute;
using static EngineNS.NxRHI.TtTextureUtility;
using static EngineNS.RName;


namespace EngineNS.NxRHI
{
    [Rtti.Meta("",NameAlias = new string[] { "EngineNS.NxRHI.USrViewAMeta@EngineCore", "EngineNS.NxRHI.USrViewAMeta" })]
    public class TtSrViewAMeta : IO.IAssetMeta
    {
        public TtSrViewAMeta()
        {
            mCreateUVAnimMenuState.Reset();
            mCreateUVAnimMenuState.HasIndent = false;
            mReImportMenuState.Reset();
            mReImportMenuState.HasIndent = false;
        }

        public override string TypeExt
        {
            get => TtSrView.AssetExt;
        }
        public override Color4b GetBorderColor()
        {
            return TtEngine.Instance.EditorInstance.Config.TextureBoderColor;
        }
        public override string GetAssetTypeName()
        {
            return "SrView";
        }
        string mOriginImageAddress = null;
        [Rtti.Meta("")]
        public string OriginImageAddress 
        {
            get
            {
                if (mOriginImageAddress == null)
                {
                    var address = AssetName.Address;
                    if (IO.TtFileManager.FileExists(address + ".png") == true)
                        mOriginImageAddress = address + ".png";
                    else if (IO.TtFileManager.FileExists(address + ".hdr") == true)
                        mOriginImageAddress = address + ".hdr";
                    else if (IO.TtFileManager.FileExists(address + ".exr") == true)
                        mOriginImageAddress = address + ".exr";
                    else
                        mOriginImageAddress = AssetName.ToString();
                }
                return mOriginImageAddress;
            }
            set
            {
                mOriginImageAddress = value;
            }
        }
        public UImageType OriginImageType
        {
            get
            {
                var ext = IO.TtFileManager.GetExtName(OriginImageAddress);
                if (ext == ".png")
                    return UImageType.PNG;
                else if (ext == ".hdr")
                    return UImageType.HDR;
                if (ext == ".exr")
                    return UImageType.EXR;
                return UImageType.Unkown;
            }
        }
        public override async Thread.Async.TtTask<IO.IAsset> GetAsset(params object[] args)
        {
            return await TtEngine.Instance.GfxDevice.TextureManager.GetTexture(GetAssetName());
        }
        public override async Thread.Async.TtTask CopyTo(string name, RName.ERNameType type)
        {
            if (mAssetName.Name == name && mAssetName.RNameType == type)
                return;
            var tarName = RName.GetRName(name, type);
            var ameta = TtEngine.Instance.AssetMetaManager.NewAMeta(tarName, typeof(TtSrViewAMeta));
            ameta.TypeStr = Rtti.TtTypeDesc.TypeOf(typeof(TtSrView)).TypeString;
            foreach (var i in this.RefAssetRNames)
            {
                ameta.RefAssetRNames.Add(i);
            }
            ameta.SaveAMeta((IAsset)null);

            var targetSnapName = TtEngine.Instance.FileManager.GetRoot2(type) + name;
            IO.TtFileManager.CopyFile(mAssetName.Address, targetSnapName);
            if (IO.TtFileManager.FileExists(targetSnapName))
                TtEngine.Instance.SourceControlModule.AddFile(targetSnapName, true);
        }
        public override async Thread.Async.TtTask MoveTo(string name, RName.ERNameType type)
        {
            if (mAssetName.Name == name && mAssetName.RNameType == type)
                return;

            if (mAssetName.Name == name && mAssetName.RNameType == type)
                return;
            IAsset asset = await GetAsset();
            List<EngineNS.IO.IAssetMeta> holders = new List<EngineNS.IO.IAssetMeta>();
            TtEngine.Instance.AssetMetaManager.GetAssetHolder(this, holders);
            List<EngineNS.IO.IAsset> holdAssets = new List<EngineNS.IO.IAsset>();
            foreach (var i in holders)
            {
                var holdAsset = await i.GetAsset();
                if (holdAsset != null)
                {
                    holdAssets.Add(holdAsset);
                }
            }

            var savedName = mAssetName.Name;
            var savedType = mAssetName.RNameType;
            var targetSnapName = TtEngine.Instance.FileManager.GetRoot2(type) + name;
            IO.TtFileManager.CopyFile(mAssetName.Address, targetSnapName);
            if (IO.TtFileManager.FileExists(targetSnapName))
                TtEngine.Instance.SourceControlModule.AddFile(targetSnapName, true);

            TtEngine.Instance.AssetMetaManager.RemoveAMeta(this);

            RNameManager.Instance.VeryDangrouseRemove(mAssetName);
            mAssetName.VeryDangrouseUpdate(name, type);
            RNameManager.Instance.VeryDangrouseAdd(mAssetName);
            this.SaveAMeta(asset);

            TtEngine.Instance.AssetMetaManager.RegAsset(this);

            foreach (var i in holdAssets)
            {
                i.SaveAssetTo(i.GetAMeta().GetAssetName());
            }

            DeleteAsset(savedName, savedType);
        }
        public override void DeleteAsset(string name, RName.ERNameType type)
        {
            var address = RName.GetAddress(type, name);

            DeleteFile(address);

            DeleteFile(address + MetaExt);
        }
        //public override void OnBeforeRenamedAsset(IAsset asset, RName name)
        //{
        //    ((TtSrView)asset).LoadOriginImageObject(this);
        //}
        //public override void OnAfterRenamedAsset(IAsset asset, RName name)
        //{
        //    ((TtSrView)asset).FreeOriginImageObject();
        //}
        Thread.Async.TtTask? EffectTask;
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            //纹理不会引用别的资产
            return false;
        }
        public override void OnShowIconTimout(int time)
        {
            if (EffectTask != null)
            {
                CoreSDK.DisposeObject(ref CmdParameters);
                EffectTask.Value.Dispose();
                EffectTask = null;
            }
        }
        protected bool mShowA = false;
        EngineNS.Editor.Forms.TtTextureViewerCmdParams CmdParameters = null;
        public NxRHI.TtSrView Srv = null;
        private async Thread.Async.TtTask BuildCmdParameters()
        {
            Srv = await TtEngine.Instance.GfxDevice.TextureManager.GetTexture(this.GetAssetName(), 1);
            if (Srv == null)
                return;
            bool isCubemap = Srv.PicDesc.CubeFaces == 6;
            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            TtGraphicsShadingEnv shading = null;
            if(isCubemap)
                shading = await Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<EngineNS.Editor.Forms.TtSlateTextureCubeViewerShading>();
            else
                shading = await Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<EngineNS.Editor.Forms.TtSlateTextureViewerShading>();
            var effect = await TtEngine.Instance.GfxDevice.EffectManager.GetGraphicEffect(shading,
                TtEngine.Instance.GfxDevice.MaterialManager.ScreenMaterial,
                new Graphics.Mesh.TtMdfStaticMesh());

            var iptDesc = new NxRHI.TtInputLayoutDesc();
            unsafe
            {
                iptDesc.mCoreObject.AddElement("POSITION", 0, EPixelFormat.PXF_R32G32_FLOAT, 0, 0, 0, 0);
                iptDesc.mCoreObject.AddElement("TEXCOORD", 0, EPixelFormat.PXF_R32G32_FLOAT, 0, (uint)sizeof(Vector2), 0, 0);
                iptDesc.mCoreObject.AddElement("COLOR", 0, EPixelFormat.PXF_R8G8B8A8_UNORM, 0, (uint)sizeof(Vector2) * 2, 0, 0);
                //iptDesc.SetShaderDesc(SlateEffect.GraphicsEffect);
                iptDesc.mCoreObject.SetShaderDesc(effect.DescVS.mCoreObject);
                var InputLayout = rc.CreateInputLayout(iptDesc); //TtEngine.Instance.GfxDevice.InputLayoutManager.GetPipelineState(rc, iptDesc);
                effect.ShaderEffect.mCoreObject.BindInputLayout(InputLayout.mCoreObject);
            }

            var cmdParams = EGui.TtImDrawCmdParameters.CreateInstance<EngineNS.Editor.Forms.TtTextureViewerCmdParams>();
            cmdParams.ColorMask = new Vector4i(1, 1, 1, 1);
            cmdParams.MipLevel = 0;
            var cbBinder = effect.ShaderEffect.FindBinder("cbShadingEnv");
            cmdParams.CBuffer = rc.CreateCBV(cbBinder);
            cmdParams.Drawcall.BindShaderEffect(effect, effect.ShadingEnv);
            cmdParams.Drawcall.BindCBV(cbBinder.mCoreObject, cmdParams.CBuffer);
            cmdParams.Drawcall.BindSRV(TtNameTable.FontTexture, Srv);
            cmdParams.Drawcall.BindSampler(TtNameTable.Samp_FontTexture, TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);

            cmdParams.IsNormalMap = 0;
            if (Srv.PicDesc.Format == EPixelFormat.PXF_BC5_UNORM || Srv.PicDesc.Format == EPixelFormat.PXF_BC5_TYPELESS || Srv.PicDesc.Format == EPixelFormat.PXF_BC5_SNORM)
            {
                cmdParams.IsNormalMap = 1;
            }

            CmdParameters = cmdParams;
        }
        public override void OnDrawSnapshot(in ImDrawList cmdlist, ref Vector2 start, ref Vector2 end)
        {
            if (EffectTask == null)
            {
                EffectTask = BuildCmdParameters();
                return;
            }
            if (EffectTask.Value.IsCompleted == false)
            {
                cmdlist.AddText(in start, 0xFFFFFFFF, "loading...", null);
                return;
            }
            if (CmdParameters != null)
            {
                unsafe
                {
                    var uv0 = new Vector2(0, 0);
                    var uv1 = new Vector2(1, 1);
                    ImTextureRef imTextureRef = new ImTextureRef();
                    imTextureRef.m__TexID = (ulong)CmdParameters.GetHandle();
                    cmdlist.AddImage(imTextureRef, in start, in end, in uv0, in uv1, 0xFFFFFFFF);
                    if (CmdParameters.IsNormalMap == 1)
                    {
                        var indiactorPos = start + new Vector2(3, 2);
                        //cmdlist.AddText(in indiactorPos, 0xFF00FF00, "N", null);
                        var font = ImGuiAPI.GetDrawListFont(cmdlist);
                        cmdlist.AddText(font, 25, &indiactorPos, 0xFF00FF00, "N", null, 2.0f, null);
                    }
                }
            }
            //cmdlist.AddText(in start, 0xFFFFFFFF, "texture", null);
        }

        public override void OnDrawIndicator(in ImDrawList cmdlist, ref Vector2 start, ref Vector2 end)
        {
        }

        protected EGui.UIProxy.MenuItemProxy.MenuState mCreateUVAnimMenuState = new EGui.UIProxy.MenuItemProxy.MenuState();
        protected EGui.UIProxy.MenuItemProxy.MenuState mReImportMenuState = new EGui.UIProxy.MenuItemProxy.MenuState();
        protected override void OnDrawPopMenu(EGui.Controls.TtContentBrowser ContentBrowser)
        {
            Support.TtAnyPointer menuData = new Support.TtAnyPointer();
            var drawList = ImGuiAPI.GetWindowDrawList();
            if(EGui.UIProxy.MenuItemProxy.MenuItem("Create UVAnim", null, false, null, in drawList, in menuData, ref mCreateUVAnimMenuState))
            {
                var anim = new EGui.TtUVAnim();
                var rname = RName.GetRName(AssetName.Name.Replace(TtSrView.AssetExt, EGui.TtUVAnim.AssetExt), AssetName.RNameType);
                anim.TextureName = AssetName;
                anim.AssetName = rname;
                var ameta = anim.CreateAMeta() as EGui.TtUVAnimAMeta;
                ameta.SetAssetName(rname);
                ameta.AssetId = Guid.NewGuid();
                ameta.TextureName = AssetName;
                ameta.TypeStr = Rtti.TtTypeDesc.TypeOf(anim.GetType()).TypeString;
                ameta.SaveAMeta((IAsset)null);
                TtEngine.Instance.AssetMetaManager.RegAsset(ameta);
                anim.SaveAssetTo(ameta.AssetName);
            }

            ImGuiAPI.Separator();
            base.OnDrawPopMenu(ContentBrowser);

            if (EGui.UIProxy.MenuItemProxy.MenuItem("ReImport", null, false, null, in drawList, in menuData, ref mReImportMenuState))
            {
                //renwind todo
            }
        }

        public override void DrawTooltip()
        {
            if (Srv == null)
                return;
            CtrlUtility.DrawHelper(
                "Name: " + GetAssetName().Name,
                "Desc: " + Description,
                "Address: " + GetAssetName().Address,
                "Res: " + Srv.PicDesc.Width + "X" + Srv.PicDesc.Height + "\r\n" +
                "Format: " + Srv.PicDesc.Format + "\r\n" +
                "CubeFaces: " + Srv.PicDesc.CubeFaces + "\r\n" +
                "MipLevel: " + Srv.PicDesc.MipLevel + "\r\n" +
                "IsSRGB: " + Srv.PicDesc.sRGB + "\r\n" +
                "IsNormal: " + Srv.PicDesc.IsNormal);
        }
    }
    [Rtti.Meta("",NameAlias = new string[] { "EngineNS.NxRHI.USrView@EngineCore", "EngineNS.NxRHI.USrView" })]
    [TtSrView.Import]
    [IO.AssetCreateMenu(MenuName = "Texture")]
    public partial class TtSrView : AuxPtrType<NxRHI.ISrView>, IO.IAsset, IO.IStreaming
    {
        public TtSrView()
        {
            System.Threading.Interlocked.Increment(ref NumOfInstance);
        }
        ~TtSrView()
        {
            System.Threading.Interlocked.Decrement(ref NumOfInstance);
        }
        [PGPropertyOrder(PGPropertyOrderAttribute.EPropertyOrder.DefinitionOrder)]
        public class TtPicDesc
        {
            public TtPicDesc()
            {
                Desc.SetDefault();
            }
            public FPictureDesc Desc;
            //public uint dwStructureSize { get => Desc.dwStructureSize; set => Desc.dwStructureSize = value; }
            [ReadOnly(true)]
            public ETextureCompressFormat CompressFormat { get => Desc.CompressFormat; set => Desc.CompressFormat = value; }
            [ReadOnly(true)]
            public EPixelFormat Format { get => Desc.Format; set => Desc.Format = value; }
            [Category("General")]
            public uint CubeFaces { get => Desc.CubeFaces; set => Desc.CubeFaces = value; }
            [Category("General")]
            public int MipLevel { get => Desc.MipLevel; set => Desc.MipLevel = value; }
            [Category("Dimension")]
            public int Width { get => Desc.Width; set => Desc.Width = value; }
            [Category("Dimension")]
            public int Height { get => Desc.Height; set => Desc.Height = value; }
            public byte BitNumRed { get => Desc.BitNumRed; set => Desc.BitNumRed = value; }
            public byte BitNumGreen { get => Desc.BitNumGreen; set => Desc.BitNumGreen = value; }
            public byte BitNumBlue { get => Desc.BitNumBlue; set => Desc.BitNumBlue = value; }
            public byte BitNumAlpha { get => Desc.BitNumAlpha; set => Desc.BitNumAlpha = value; }
            [Category("General")]
            public bool DontCompress 
            {
                get => Desc.DontCompress != 0 ? true : false;
                set => Desc.DontCompress = value ? 1 : 0;
            }
            [Category("General")]
            public bool sRGB
            {
                get => Desc.sRGB != 0 ? true : false;
                set => Desc.sRGB = value ? 1 : 0;
            }
            [Category("General")]
            public bool IsHdr()
            {
                switch (Desc.Format)
                {
                    case EPixelFormat.PXF_R16_FLOAT:
                    case EPixelFormat.PXF_R16G16_FLOAT:
                    case EPixelFormat.PXF_R16G16B16A16_FLOAT:
                    case EPixelFormat.PXF_R32_FLOAT:
                    case EPixelFormat.PXF_R32G32_FLOAT:
                    case EPixelFormat.PXF_R32G32B32_FLOAT:
                    case EPixelFormat.PXF_R32G32B32A32_FLOAT:
                    case EPixelFormat.PXF_BC6H_UF16:
                    case EPixelFormat.PXF_BC6H_SF16:
                    case EPixelFormat.PXF_BC6H_TYPELESS:
                        return true;
                }
                if (BitNumRed > 8 || BitNumGreen > 8 || BitNumBlue > 8)
                    return true;
                return false;
            }
            public bool StripOriginSource
            {
                get => Desc.StripOriginSource != 0 ? true : false;
                set => Desc.StripOriginSource = value ? 1 : 0;
            }
            int mDepth = 0;
            [Category("Dimension")]
            public int Depth { get => mDepth; set => mDepth = value; }
            public bool IsTexture3D { get => Depth > 0; }
            bool mAutoCheckNormal = true;
            public bool AutoCheckNormal { get => mAutoCheckNormal; set => mAutoCheckNormal = value; }
            bool mIsNormal;
            public bool IsNormal { get => mIsNormal; set => mIsNormal=value; }
            bool mIsAutoSaveSrcImage = false;
            public bool IsAutoSaveSrcImage { get => mIsAutoSaveSrcImage; set => mIsAutoSaveSrcImage = value; }
            public List<Vector3i> MipSizes { get; } = new List<Vector3i>();
            public List<Vector2i> BlockDimenstions { get; } = new List<Vector2i>();
            public int BlockSize = 0;
        }
        public EPixelFormat SrvFormat
        {
            get
            {
                return mCoreObject.GetBufferAsTexture().Desc.Format;
            }
        }
        public TtPicDesc PicDesc { get; set; }
        public TtTexture StreamingTexture { get; private set; } = null;
        public EPixelFormat Format
        {
            get
            {
                if (PicDesc!=null)
                    return PicDesc.Desc.Format;
                return mCoreObject.GetBufferAsTexture().Desc.Format;
            }
        }
        public int Width
        {
            get
            {
                if (PicDesc!=null)
                    return PicDesc.Desc.Width;
                return (int)mCoreObject.GetBufferAsTexture().Desc.Width;
            }
        }
        public int Height
        {
            get
            {
                if (PicDesc!=null)
                    return PicDesc.Desc.Height;
                return (int)mCoreObject.GetBufferAsTexture().Desc.Height;
            }
        }
        public uint CubeFaces
        {
            get
            {
                if (PicDesc!=null)
                    return PicDesc.Desc.CubeFaces;
                if (mCoreObject.Desc.Type == ESrvType.ST_TextureCube)
                    return 6;
                return 1;//mCoreObject.GetBufferAsTexture().Desc.CubeFaces;
            }
        }

        public object TagObject;
        public static int NumOfInstance = 0;
        public static int NumOfGCHandle = 0;

        public void SetDebugName(string name)
        {
            mCoreObject.NativeSuper.SetDebugName(name);
        }

        public class ImportAttribute : IO.IAssetCreateAttribute
        {
            bool bPopOpen = false;
            bool bFileExisting = false;
            public RName mDir;
            public string mName;
            public string mSourceFile;
            public TtPicDesc mDesc = new TtPicDesc();
            ImGui.ImGuiFileDialog mFileDialog = TtEngine.Instance.EditorInstance.FileDialog.mFileDialog;
            EGui.Controls.PropertyGrid.TtPropertyGrid PGAsset = new EGui.Controls.PropertyGrid.TtPropertyGrid();
            public override async Thread.Async.TtTask DoCreate(RName dir, Rtti.TtTypeDesc type, string ext)
            {
                mDir = dir;
                mDesc.Desc.SetDefault();
                var noused = PGAsset.Initialize();
                PGAsset.Target = mDesc;
            }
            public unsafe void _DumpBasicPicDesc()
            {
                using (var stream = System.IO.File.OpenRead(mSourceFile))
                {
                    var filter = mFileDialog.GetCurrentFilter();
                    if(filter == ".exr")
                    {
                        var exrFile = new Jither.OpenEXR.EXRFile(stream);
                        var part = exrFile.Parts[0];

                        mDesc.Width = part.DisplayWindow.Width;
                        mDesc.Height = part.DisplayWindow.Height;
                    }
                    else
                    {
                        var image = StbImageSharp.TtMemImage.FromStream(stream, StbImageSharp.ColorComponents.Default);
                        if (image != null)
                        {
                            mDesc.Width = image.Width;
                            mDesc.Height = image.Height;
                        }
                    }
                    mDesc.MipLevel = Math.Max(CalcMipLevel(mDesc.Width, mDesc.Height, true, 4), 1);
                }
            }
            public override unsafe bool OnDraw(EGui.Controls.TtContentBrowser ContentBrowser)
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
                            mFileDialog.OpenModal("ChooseFileDlgKey", "Choose File", ".png,.jpg,.bmp,.tga,.exr,.hdr", ".");
                        }
                        // display
                        if (mFileDialog.DisplayDialog("ChooseFileDlgKey"))
                        {
                            // action if OK
                            if (mFileDialog.IsOk() == true)
                            {
                                mSourceFile = mFileDialog.GetFilePathName();
                                mName = IO.TtFileManager.GetPureName(mSourceFile);
                                _DumpBasicPicDesc();
                            }
                            // close
                            mFileDialog.CloseDialog();
                        }
                    }
                    else if(string.IsNullOrEmpty(mSourceFile))
                    {
                        mSourceFile = ContentBrowser.CurrentImporterFile;
                        mName = IO.TtFileManager.GetPureName(mSourceFile);
                        _DumpBasicPicDesc();
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
                        var name = buffer.AsTextUtf8();
                        if (mName != name)
                        {
                            mName = name;
                            bFileExisting = IO.TtFileManager.FileExists(mDir.Address + mName + NxRHI.TtSrView.AssetExt);
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
                TtEngine.Instance.EventPoster.RunOn((state)=>
                {
                    TtEngine.Instance.BlockOperation($"ImportImage:{mSourceFile}");
                    //ImportImageImpl_Deprecated();
                    using (var stream = System.IO.File.OpenRead(mSourceFile))
                    {
                        TtSrView.ImportImage(stream, this, true);
                    }
                    TtEngine.Instance.ResumeOperation();
                    return true;
                }, Thread.Async.EAsyncTarget.AsyncIO);
                return true;
            }
            public RName ImportImageImpl_Deprecated()
            {
                using (var stream = System.IO.File.OpenRead(mSourceFile))
                {
                    return ImportImageImpl_Deprecated(stream);
                }
            }
            public unsafe RName ImportImageImpl_Deprecated(System.IO.Stream stream)
            {
                if (stream == null)
                    return null;

                var extName = IO.TtFileManager.GetExtName(mSourceFile);
                var rn = RName.GetRName(mDir.Name + mName + TtSrView.AssetExt, mDir.RNameType);
                var xnd = new IO.TtXndHolder("TtSrView", 0, 0);

                if (extName.ToLower() == ".hdr")
                {
                    var imageFloat = StbImageSharp.ImageResultFloat.FromStream(stream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                    if (imageFloat == null)
                        return null;

                    StbImageSharp.ImageResultFloat processedImage = null;
                    if (mDesc.CubeFaces == 6)
                    {
                        TtTextureHelper.GenerateBaseCubeMipFromLongitudeLatitude2D(ref processedImage, imageFloat, 512);
                    }
                    else
                        processedImage = imageFloat;

                    TtSrView.CookTextureTo(rn, xnd.RootNode.mCoreObject, processedImage, mDesc);
                }
                else if (extName.ToLower() == ".exr")
                {
                    var file = new Jither.OpenEXR.EXRFile(stream);
                    if (file.Parts.Count == 0)
                        return null;

                    TtSrView.SaveTexture(rn, xnd.RootNode.mCoreObject, file, mDesc);
                }
                else
                {
                    TtMemImage image = null;
                    image = StbImageSharp.TtMemImage.FromStream(stream, StbImageSharp.ColorComponents.Default);
                    if (image == null)
                        return null;

                    if (mDesc.AutoCheckNormal == true)
                    {
                        NormalmapChecker normalChecker = new NormalmapChecker();
                        mDesc.IsNormal = normalChecker.DoesTextureLookLikelyToBeANormalMap(image);
                    }

                    if (mDesc.MipLevel==0)
                    {
                        if (mDesc.Height < 64 && mDesc.Width < 64)
                            mDesc.MipLevel = 1;
                    }

                    TtSrView.CookTextureTo(rn, xnd.RootNode.mCoreObject, image, mDesc);
                }

                xnd.SaveXnd(rn.Address);
                TtEngine.Instance.SourceControlModule.AddFile(rn.Address, true);

                if (rn.AMeta==null)
                {
                    var ameta = new TtSrViewAMeta();
                    ameta.SetAssetName(rn);
                    ameta.AssetId = Guid.NewGuid();
                    ameta.TypeStr = Rtti.TtTypeDesc.TypeOf(typeof(TtSrView)).TypeString;
                    ameta.Description = $"This is a {typeof(TtSrView).FullName}\n";
                    ameta.OriginImageAddress = mSourceFile;
                    ameta.SaveAMeta((IAsset)null);

                    TtEngine.Instance.AssetMetaManager.RegAsset(ameta);
                }

                rn.AMeta.AddAssetFile(rn.Address);
                TtEngine.Instance.SourceControlModule.AddFile(rn.Address + IAssetMeta.MetaExt, true);

                return rn;
            }

            public static bool ImportImage_Deprecated(string sourceFile, RName dir, TtPicDesc desc)
            {
                using (var stream = System.IO.File.OpenRead(sourceFile))
                {
                    if (stream == null)
                        return false;
                    var image = StbImageSharp.TtMemImage.FromStream(stream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                    if (image == null)
                        return false;

                    var name = IO.TtFileManager.GetPureName(sourceFile);
                    var rn = RName.GetRName(dir.Name.TrimEnd('\\').TrimEnd('/') + "/" + name + TtSrView.AssetExt, dir.RNameType);

                    return SaveSrv_Deprecated(image, rn, desc);
                }
            }

            public static bool SaveSrv_Deprecated(Jither.OpenEXR.EXRFile file, RName rn, TtPicDesc desc)
            {
                var part = file.Parts[0];
                System.Diagnostics.Debug.Assert(part.DataReader != null);

                desc.Width = part.DisplayWindow.Width;
                desc.Height = part.DisplayWindow.Height;

                var xnd = new IO.TtXndHolder("USrView", 0, 0);
                TtSrView.SaveTexture(rn, xnd.RootNode.mCoreObject, file, desc);
                xnd.SaveXnd(rn.Address);

                var ameta = new TtSrViewAMeta();
                ameta.SetAssetName(rn);
                ameta.AssetId = Guid.NewGuid();
                ameta.TypeStr = Rtti.TtTypeDesc.TypeOf(typeof(TtSrView)).TypeString;
                ameta.Description = $"This is a {typeof(TtSrView).FullName}\n";
                ameta.SaveAMeta((IO.IAsset)null);

                TtEngine.Instance.AssetMetaManager.RegAsset(ameta);
                rn.AMeta.AddAssetFile(rn.Address);
                TtEngine.Instance.SourceControlModule.AddFile(rn.Address);

                return true;
            }

            public static bool SaveSrv_Deprecated(StbImageSharp.ImageResultFloat image, RName rn, TtPicDesc desc)
            {
                desc.Width = image.Width;
                desc.Height = image.Height;

                var xnd = new IO.TtXndHolder("USrView", 0, 0);
                TtSrView.CookTextureTo(rn, xnd.RootNode.mCoreObject, image, desc);
                xnd.SaveXnd(rn.Address);

                var ameta = new TtSrViewAMeta();
                ameta.SetAssetName(rn);
                ameta.AssetId = Guid.NewGuid();
                ameta.TypeStr = Rtti.TtTypeDesc.TypeOf(typeof(TtSrView)).TypeString;
                ameta.Description = $"This is a {typeof(TtSrView).FullName}\n";
                ameta.SaveAMeta((IO.IAsset)null);

                TtEngine.Instance.AssetMetaManager.RegAsset(ameta);
                rn.AMeta.AddAssetFile(rn.Address);
                TtEngine.Instance.SourceControlModule.AddFile(rn.Address);
                return true;
            }

            public static bool SaveSrv_Deprecated(StbImageSharp.TtMemImage image, RName rn, TtPicDesc desc)
            {
                desc.Width = image.Width;
                desc.Height = image.Height;

                var xnd = new IO.TtXndHolder("TtSrView", 0, 0);
                TtSrView.CookTextureTo(rn, xnd.RootNode.mCoreObject, image, desc);
                xnd.SaveXnd(rn.Address);

                var ameta = new TtSrViewAMeta();
                ameta.SetAssetName(rn);
                ameta.AssetId = Guid.NewGuid();
                ameta.TypeStr = Rtti.TtTypeDesc.TypeOf(typeof(TtSrView)).TypeString;
                ameta.Description = $"This is a {typeof(TtSrView).FullName}\n";
                ameta.SaveAMeta((IO.IAsset)null);

                TtEngine.Instance.AssetMetaManager.RegAsset(ameta);
                TtEngine.Instance.SourceControlModule.AddFile(rn.Address);
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
                System.Diagnostics.Debug.Assert(false);
                //ImportImage(sourceFile, dir, new TtPicDesc());
            }
        }

        public ITexture GetTexture()
        {
            return mCoreObject.GetBufferAsTexture();
        }
        public IBuffer GetBuffer()
        {
            return mCoreObject.GetBufferAsBuffer();
        }

        #region IAsset
        public const string AssetExt = ".srv";
        public string TypeExt { get => AssetExt; }
        public IO.IAssetMeta CreateAMeta()
        {
            var result = new TtSrViewAMeta();
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
        object mOriginImageObject = null;
        internal void LoadOriginImageObject(TtSrViewAMeta ameta)
        {
            //var ameta = GetAMeta() as TtSrViewAMeta;
            if (ameta == null)
                return;
            var imgType = ameta.OriginImageType;
            switch (imgType)
            {
                case EngineNS.Bricks.ImageDecoder.UImageType.PNG:
                    {
                        mOriginImageObject = LoadOriginPng(this.AssetName);
                    }
                    break;
                case EngineNS.Bricks.ImageDecoder.UImageType.HDR:
                    {
                        StbImageSharp.ImageResultFloat imageFloat = new StbImageSharp.ImageResultFloat();
                        LoadOriginHdr(AssetName, ref imageFloat);
                        mOriginImageObject = imageFloat;
                    }
                    break;
                case EngineNS.Bricks.ImageDecoder.UImageType.EXR:
                    {
                        System.IO.Stream outStream = null;
                        mOriginImageObject = LoadOriginExr(AssetName, ref outStream);
                    }
                    break;
                default:
                    mOriginImageObject = null;
                    break;
            }
        }
        internal void FreeOriginImageObject()
        {
            mOriginImageObject = null;
        }

        /// <summary>
        /// Load the uncompressed LDR source image from local file or .srv asset.
        /// Priority: 1) OriginImageAddress local file  2) XND "Png"/"OriginSource" embedded  3) XND "PngMips" mip0  4) XND "DxtMips" decoded mip0
        /// Returns null if no source can be found.
        /// </summary>
        public StbImageSharp.TtMemImage LoadUncompressImageLDR()
        {
            var ameta = GetAMeta() as TtSrViewAMeta;

            // 1) Try local source file
            if (ameta != null && !string.IsNullOrEmpty(ameta.OriginImageAddress)
                && System.IO.File.Exists(ameta.OriginImageAddress)
                && ameta.OriginImageType == Bricks.ImageDecoder.UImageType.PNG)
            {
                using (var stream = System.IO.File.OpenRead(ameta.OriginImageAddress))
                {
                    var image = StbImageSharp.TtMemImage.FromStream(stream, StbImageSharp.ColorComponents.Default);
                    if (image != null)
                        return image;
                }
            }

            // 2) Try reading from .srv XND file
            var rn = AssetName;
            if (rn == null || string.IsNullOrEmpty(rn.Address))
                return null;

            using (var xnd = IO.TtXndHolder.LoadXnd(rn.Address))
            {
                if (xnd == null)
                    return null;

                // 2a) New format: RawSource node
                var rawNode = xnd.RootNode.TryGetChildNode("RawSource");
                if (rawNode.IsValidPointer)
                {
                    var image = TtTextureCookManager.LoadLdrFromRawNode(rawNode);
                    if (image != null)
                        return image;
                }

                // 2b) Embedded original PNG data (legacy)
                var attr = xnd.RootNode.TryGetAttribute("OriginSource");
                if (attr.IsValidPointer == false)
                    attr = xnd.RootNode.TryGetAttribute("Png");
                if (attr.IsValidPointer)
                {
                    byte[] pngData;
                    using (var ar = attr.GetReader(null))
                    {
                        ar.ReadNoSize(out pngData, (int)attr.GetReaderLength());
                    }
                    using (var memStream = new System.IO.MemoryStream(pngData))
                    {
                        var image = StbImageSharp.TtMemImage.FromStream(memStream, StbImageSharp.ColorComponents.Default);
                        if (image != null)
                            return image;
                    }
                }

                // 2b) PngMips node — read mip 0
                var pngMipsNode = xnd.RootNode.TryGetChildNode("PngMips");
                if (pngMipsNode.IsValidPointer)
                {
                    var mip0Attr = pngMipsNode.TryGetAttribute("PngMip0");
                    if (mip0Attr.IsValidPointer)
                    {
                        byte[] data;
                        using (var ar = mip0Attr.GetReader(null))
                        {
                            ar.ReadNoSize(out data, (int)mip0Attr.GetReaderLength());
                        }
                        using (var memStream = new System.IO.MemoryStream(data))
                        {
                            var image = StbImageSharp.TtMemImage.FromStream(memStream, StbImageSharp.ColorComponents.Default);
                            if (image != null)
                                return image;
                        }
                    }
                }

                // 2c) DxtMips node — decode BC compressed mip 0
                var dxtMipsNode = xnd.RootNode.TryGetChildNode("DxtMips");
                if (dxtMipsNode.IsValidPointer)
                {
                    var image = DecodeDxtMip0FromXnd(dxtMipsNode);
                    if (image != null)
                        return image;
                }
            }

            return null;
        }

        /// <summary>
        /// Decode mip 0 from a DxtMips XND node back to uncompressed TtMemImage.
        /// Handles both legacy (DxtMip0 attribute) and new (Face0/DxtMip0) formats.
        /// </summary>
        private StbImageSharp.TtMemImage DecodeDxtMip0FromXnd(XndNode dxtMipsNode)
        {
            byte[] compressedData = null;

            // Try legacy format: DxtMip0 attribute directly on DxtMips node
            var mip0Attr = dxtMipsNode.TryGetAttribute("DxtMip0");
            if (mip0Attr.IsValidPointer)
            {
                using (var ar = mip0Attr.GetReader(null))
                {
                    ar.ReadNoSize(out compressedData, (int)mip0Attr.GetReaderLength());
                }
            }
            else
            {
                // New format: Face0/DxtMip0
                var faceNode = dxtMipsNode.TryGetChildNode("Face0");
                if (faceNode.IsValidPointer)
                {
                    mip0Attr = faceNode.TryGetAttribute("DxtMip0");
                    if (mip0Attr.IsValidPointer)
                    {
                        using (var ar = mip0Attr.GetReader(null))
                        {
                            ar.ReadNoSize(out compressedData, (int)mip0Attr.GetReaderLength());
                        }
                    }
                }
            }

            if (compressedData == null || compressedData.Length == 0)
                return null;

            var desc = PicDesc;
            if (desc == null)
                return null;

            int width = desc.Width;
            int height = desc.Height;
            var bcnFormat = GetBCnCompressionFormat(desc.Format);
            if (bcnFormat == null)
                return null;

            var decoder = new BCnEncoder.Decoder.BcDecoder();
            var decoded = decoder.DecodeRaw(compressedData, width, height, bcnFormat.Value);
            if (decoded == null || decoded.Length == 0)
                return null;

            // Convert ColorRgba32[] to TtMemImage (RGBA byte array)
            var image = new StbImageSharp.TtMemImage();
            image.Width = width;
            image.Height = height;
            image.Comp = StbImageSharp.ColorComponents.RedGreenBlueAlpha;
            image.SourceComp = StbImageSharp.ColorComponents.RedGreenBlueAlpha;
            image.Data = new byte[width * height * 4];
            for (int i = 0; i < decoded.Length && i < width * height; i++)
            {
                int idx = i * 4;
                image.Data[idx] = decoded[i].r;
                image.Data[idx + 1] = decoded[i].g;
                image.Data[idx + 2] = decoded[i].b;
                image.Data[idx + 3] = decoded[i].a;
            }
            return image;
        }

        /// <summary>
        /// Load the uncompressed HDR source image from local file or .srv asset.
        /// Priority: 1) OriginImageAddress local file (hdr/exr)  2) XND "PngMips" decoded as float  3) XND "DxtMips" BC6H decoded
        /// Returns null if no source can be found.
        /// </summary>
        public StbImageSharp.ImageResultFloat LoadUncompressImageHDR()
        {
            var ameta = GetAMeta() as TtSrViewAMeta;

            // 1) Try local source file
            if (ameta != null && !string.IsNullOrEmpty(ameta.OriginImageAddress)
                && System.IO.File.Exists(ameta.OriginImageAddress))
            {
                var imageType = ameta.OriginImageType;
                switch (imageType)
                {
                    case Bricks.ImageDecoder.UImageType.HDR:
                        using (var stream = System.IO.File.OpenRead(ameta.OriginImageAddress))
                        {
                            var result = StbImageSharp.ImageResultFloat.FromStream(stream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                            if (result != null)
                                return result;
                        }
                        break;
                    case Bricks.ImageDecoder.UImageType.EXR:
                        var exrResult = LoadExrFromFile(ameta.OriginImageAddress);
                        if (exrResult != null)
                            return exrResult;
                        break;
                }
            }

            // 2) Try reading from .srv XND file
            var rn = AssetName;
            if (rn == null || string.IsNullOrEmpty(rn.Address))
                return null;

            using (var xnd = IO.TtXndHolder.LoadXnd(rn.Address))
            {
                if (xnd == null)
                    return null;

                // 2a) New format: RawSource node
                var rawNode = xnd.RootNode.TryGetChildNode("RawSource");
                if (rawNode.IsValidPointer)
                {
                    var image = TtTextureCookManager.LoadHdrFromRawNode(rawNode);
                    if (image != null)
                        return image;
                }

                // 2b) PngMips node — decode mip 0 PNG and convert to float
                var pngMipsNode = xnd.RootNode.TryGetChildNode("PngMips");
                if (pngMipsNode.IsValidPointer)
                {
                    var mip0Attr = pngMipsNode.TryGetAttribute("PngMip0");
                    if (mip0Attr.IsValidPointer)
                    {
                        byte[] data;
                        using (var ar = mip0Attr.GetReader(null))
                        {
                            ar.ReadNoSize(out data, (int)mip0Attr.GetReaderLength());
                        }
                        using (var memStream = new System.IO.MemoryStream(data))
                        {
                            var ldrImage = StbImageSharp.TtMemImage.FromStream(memStream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                            if (ldrImage != null)
                                return ConvertLdrToFloat(ldrImage);
                        }
                    }
                }

                // 2b) DxtMips node — decode BC6H mip 0
                var dxtMipsNode = xnd.RootNode.TryGetChildNode("DxtMips");
                if (dxtMipsNode.IsValidPointer)
                {
                    var imageFloat = DecodeDxtMip0HdrFromXnd(dxtMipsNode);
                    if (imageFloat != null)
                        return imageFloat;
                }
            }

            return null;
        }

        /// <summary>
        /// Load an EXR file from disk and return as ImageResultFloat.
        /// </summary>
        internal static StbImageSharp.ImageResultFloat LoadExrFromFile(string filePath)
        {
            using (var stream = System.IO.File.OpenRead(filePath))
            {
                var exrFile = new Jither.OpenEXR.EXRFile(stream);
                if (exrFile.Parts.Count == 0)
                    return null;
                var part = exrFile.Parts[0];
                if (part.DataReader == null)
                    return null;

                int width = part.DisplayWindow.Width;
                int height = part.DisplayWindow.Height;
                int totalChannels = part.Channels.Count;
                int bytesPerChannel = part.Channels[0].Type == Jither.OpenEXR.EXRDataType.Float ? 4 : 2;
                int channelCount = Math.Min(totalChannels, 4);

                byte[] pixelData = new byte[part.DataReader.GetTotalByteCount()];
                part.DataReader.ReadInterleaved(pixelData, new[] { "R", "G", "B", "A" });

                var imageFloat = new StbImageSharp.ImageResultFloat()
                {
                    Width = width,
                    Height = height,
                    Comp = channelCount >= 4
                        ? StbImageSharp.ColorComponents.RedGreenBlueAlpha
                        : StbImageSharp.ColorComponents.RedGreenBlue,
                    Data = new float[width * height * channelCount]
                };

                for (int i = 0; i < width * height; i++)
                {
                    for (int c = 0; c < channelCount && c < totalChannels; c++)
                    {
                        int srcIdx = i * totalChannels * bytesPerChannel + c * bytesPerChannel;
                        int destIdx = i * channelCount + c;
                        imageFloat.Data[destIdx] = bytesPerChannel == 4
                            ? BitConverter.ToSingle(pixelData, srcIdx)
                            : (float)BitConverter.ToHalf(pixelData, srcIdx);
                    }
                }
                return imageFloat;
            }
        }

        /// <summary>
        /// Load an EXR file object (already parsed) into an ImageResultFloat.
        /// </summary>
        internal static StbImageSharp.ImageResultFloat LoadExrToImageFloat(Jither.OpenEXR.EXRFile exrFile)
        {
            if (exrFile.Parts.Count == 0)
                return null;
            var part = exrFile.Parts[0];
            if (part.DataReader == null)
                return null;

            int width = part.DisplayWindow.Width;
            int height = part.DisplayWindow.Height;
            int totalChannels = part.Channels.Count;
            int bytesPerChannel = part.Channels[0].Type == Jither.OpenEXR.EXRDataType.Float ? 4 : 2;
            int channelCount = Math.Min(totalChannels, 4);

            byte[] pixelData = new byte[part.DataReader.GetTotalByteCount()];
            part.DataReader.ReadInterleaved(pixelData, new[] { "R", "G", "B", "A" });

            var imageFloat = new StbImageSharp.ImageResultFloat()
            {
                Width = width,
                Height = height,
                Comp = channelCount >= 4
                    ? StbImageSharp.ColorComponents.RedGreenBlueAlpha
                    : StbImageSharp.ColorComponents.RedGreenBlue,
                Data = new float[width * height * channelCount]
            };

            for (int i = 0; i < width * height; i++)
            {
                for (int c = 0; c < channelCount && c < totalChannels; c++)
                {
                    int srcIdx = i * totalChannels * bytesPerChannel + c * bytesPerChannel;
                    int destIdx = i * channelCount + c;
                    imageFloat.Data[destIdx] = bytesPerChannel == 4
                        ? BitConverter.ToSingle(pixelData, srcIdx)
                        : (float)BitConverter.ToHalf(pixelData, srcIdx);
                }
            }
            return imageFloat;
        }

        /// <summary>
        /// Convert a LDR TtMemImage (byte RGBA) to ImageResultFloat (normalized 0~1).
        /// </summary>
        private static StbImageSharp.ImageResultFloat ConvertLdrToFloat(StbImageSharp.TtMemImage ldrImage)
        {
            int channelCount = 4;
            var imageFloat = new StbImageSharp.ImageResultFloat()
            {
                Width = ldrImage.Width,
                Height = ldrImage.Height,
                Comp = StbImageSharp.ColorComponents.RedGreenBlueAlpha,
                Data = new float[ldrImage.Width * ldrImage.Height * channelCount]
            };

            for (int i = 0; i < ldrImage.Data.Length; i++)
            {
                imageFloat.Data[i] = ldrImage.Data[i] / 255.0f;
            }
            return imageFloat;
        }

        /// <summary>
        /// Decode HDR (BC6H) mip 0 from DxtMips XND node back to ImageResultFloat.
        /// </summary>
        private StbImageSharp.ImageResultFloat DecodeDxtMip0HdrFromXnd(XndNode dxtMipsNode)
        {
            byte[] compressedData = null;

            // Try legacy format: DxtMip0 directly
            var mip0Attr = dxtMipsNode.TryGetAttribute("DxtMip0");
            if (mip0Attr.IsValidPointer)
            {
                using (var ar = mip0Attr.GetReader(null))
                {
                    ar.ReadNoSize(out compressedData, (int)mip0Attr.GetReaderLength());
                }
            }
            else
            {
                // New format: Face0/DxtMip0
                var faceNode = dxtMipsNode.TryGetChildNode("Face0");
                if (faceNode.IsValidPointer)
                {
                    mip0Attr = faceNode.TryGetAttribute("DxtMip0");
                    if (mip0Attr.IsValidPointer)
                    {
                        using (var ar = mip0Attr.GetReader(null))
                        {
                            ar.ReadNoSize(out compressedData, (int)mip0Attr.GetReaderLength());
                        }
                    }
                }
            }

            if (compressedData == null || compressedData.Length == 0)
                return null;

            var desc = PicDesc;
            if (desc == null)
                return null;

            int width = desc.Width;
            int height = desc.Height;

            var decoder = new BCnEncoder.Decoder.BcDecoder();
            var bcnHdrFormat = (desc.Format == EPixelFormat.PXF_BC6H_SF16)
                ? CompressionFormat.Bc6S : CompressionFormat.Bc6U;
            var decoded = decoder.DecodeRawHdr(compressedData, width, height, bcnHdrFormat);
            if (decoded == null || decoded.Length == 0)
                return null;

            var imageFloat = new StbImageSharp.ImageResultFloat()
            {
                Width = width,
                Height = height,
                Comp = StbImageSharp.ColorComponents.RedGreenBlueAlpha,
                Data = new float[width * height * 4]
            };

            for (int i = 0; i < decoded.Length && i < width * height; i++)
            {
                int idx = i * 4;
                imageFloat.Data[idx] = decoded[i].r;
                imageFloat.Data[idx + 1] = decoded[i].g;
                imageFloat.Data[idx + 2] = decoded[i].b;
                imageFloat.Data[idx + 3] = 1.0f;
            }
            return imageFloat;
        }

        public void SaveAssetTo_Deprecated(RName name)
        {
            //if (SaveAssetTo2(name) == true)
            //    return;
            var ameta = this.GetAMeta() as TtSrViewAMeta;
            if (mOriginImageObject != null)
            {
                if (mOriginImageObject.GetType() == typeof(TtMemImage))
                {
                    ImportAttribute.SaveSrv_Deprecated(mOriginImageObject as TtMemImage, name, this.PicDesc);
                }
                else if (mOriginImageObject.GetType() == typeof(ImageResultFloat))
                {
                    ImportAttribute.SaveSrv_Deprecated(mOriginImageObject as ImageResultFloat, name, this.PicDesc);
                }
                else if (mOriginImageObject.GetType() == typeof(EXRFile))
                {
                    ImportAttribute.SaveSrv_Deprecated(mOriginImageObject as EXRFile, name, this.PicDesc);
                }
                return;
            }

            var imgType = GetOriginImageType(AssetName);// ameta.OriginImageType;
            switch (imgType)
            {
                case EngineNS.Bricks.ImageDecoder.UImageType.PNG:
                    {
                        var image = LoadOriginPng(this.AssetName);
                        if (image == null)
                        {
                            Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Warning, $"SaveAssetTo failed: LoadOriginImage({AssetName}) = null");
                            return;
                        }
                        ImportAttribute.SaveSrv_Deprecated(image, name, this.PicDesc);
                    }
                    break;
                case EngineNS.Bricks.ImageDecoder.UImageType.HDR:
                    {
                        StbImageSharp.ImageResultFloat imageFloat = new StbImageSharp.ImageResultFloat();
                        LoadOriginHdr(AssetName, ref imageFloat);
                        ImportAttribute.SaveSrv_Deprecated(imageFloat, name, this.PicDesc);
                    }
                    break;
                case EngineNS.Bricks.ImageDecoder.UImageType.EXR:
                    {
                        System.IO.Stream outStream = null;
                        var file = LoadOriginExr(AssetName, ref outStream);
                        if(file == null)
                        {
                            Profiler.Log.WriteLine<Profiler.TtEditorGategory>(Profiler.ELogTag.Warning, $"SaveAssetTo failed: LoadOriginImage({AssetName}) = null");
                            return;
                        }
                        ImportAttribute.SaveSrv_Deprecated(file, name, this.PicDesc);
                    }
                    break;
                case UImageType.Unkown:
                    {
                        var segs = ameta.OriginImageAddress.Split(':');
                        if (segs.Length == 2)
                        {
                            var rnType = (RName.ERNameType)Support.TConvert.ToEnumValue(typeof(RName.ERNameType), segs[1]);
                            var src = RName.GetAddress(rnType, segs[0]);
                            IO.TtFileManager.CopyFile(src, name.Address);
                            name.AMeta.AddAssetFile(name.Address);
                            TtEngine.Instance.SourceControlModule.AddFile(name.Address, true);
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(false);
                        }
                    }
                    break;
            }

            if (ameta != null)
            {
                UpdateAMetaReferences(ameta);
                string saved = null;
                if (imgType == UImageType.Unkown)
                {
                    saved = ameta.OriginImageAddress;
                    ameta.OriginImageAddress = AssetName.ToString();
                }
                ameta.SaveAMeta(this);
                if (imgType == UImageType.Unkown)
                {
                    ameta.OriginImageAddress = saved;
                }
            }
        }
        RName mAssetName;
        [Rtti.Meta("")]
        public RName AssetName
        {
            get => mAssetName;
            set
            {
                mAssetName = value;
                mCoreObject.NativeSuper.SetDebugName(value.ToString());
                var tex = mCoreObject.GetBufferAsTexture();
                if (tex.IsValidPointer)
                {
                    tex.SetDebugName("Texture:" + value.ToString());
                }
            }
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
            if (CurLoadTask != null)
            {
                CurLoadTask.Value.Dispose();
                CurLoadTask = null;
            }
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
                if (PicDesc!=null)
                    return PicDesc.MipLevel;
                var tex = mCoreObject.GetBufferAsTexture();
                if (tex.IsValidPointer == false)
                    return 0;
                return (int)tex.Desc.MipLevels;
            }
        }
        [Browsable(false)]
        public Thread.Async.TtTask<bool>? CurLoadTask { get; set; }

        #region TextureStreaming
        /// <summary>
        /// 返回加载 lod 个 mip（包含最高 lod 个分辨率层级）所占的字节数。
        /// lod 语义与 LoadLOD 一致：lod 表示 mip 数量，对应 mip index 区间 [MaxLOD-lod, MaxLOD-1]。
        /// 根据每个 mip 的 Width/Height/Format 自算，以与实际 GPU 占用对齐（块压缩按 4x4 对齐）。
        /// </summary>
        public long GetStreamingBytesForLOD(int lod)
        {
            var maxLod = MaxLOD;
            if (maxLod <= 0 || lod <= 0)
                return 0;
            if (lod > maxLod)
                lod = maxLod;

            int baseWidth = Width;
            int baseHeight = Height;
            if (baseWidth <= 0 || baseHeight <= 0)
                return 0;

            var format = Format;
            bool isBlock = IsBlockCompressedFormat(format);
            uint cubeFaces = CubeFaces;
            if (cubeFaces == 0)
                cubeFaces = 1;

            long total = 0;
            // mip index 区间 [maxLod-lod, maxLod-1]
            for (int mip = maxLod - lod; mip < maxLod; mip++)
            {
                int w = System.Math.Max(1, baseWidth >> mip);
                int h = System.Math.Max(1, baseHeight >> mip);
                long mipBytes;
                if (isBlock)
                {
                    int blocksW = System.Math.Max(1, (w + 3) / 4);
                    int blocksH = System.Math.Max(1, (h + 3) / 4);
                    mipBytes = (long)blocksW * blocksH * GetBlockByteSize(format);
                }
                else
                {
                    mipBytes = (long)w * h * CoreSDK.GetPixelFormatByteWidth(format);
                }
                total += mipBytes;
            }
            return total * cubeFaces;
        }

        /// <summary>当前已驻留字节数。</summary>
        [Browsable(false)]
        public long CurrentResidentBytes => GetStreamingBytesForLOD(LevelOfDetail);
        #endregion

        public async Thread.Async.TtTask<bool> LoadLOD(int level)
        {
            if (level == 0)
            {
                return false;
            }
            if (level < 0 || level > MaxLOD)
                return false;
            var oldTexture = StreamingTexture;
            StreamingTexture = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                using (var xnd = IO.TtXndHolder.LoadXnd(AssetName.Address))
                {
                    if (xnd == null)
                        return null;

                    // New format: load from cooked cache
                    if (TtTextureCookManager.HasRawSource(xnd.RootNode))
                    {
                        TtPicDesc cookedDesc;
                        return TtTextureCookManager.LoadFromCooked(this.AssetName, level, out cookedDesc);
                    }

                    // Legacy format: load directly from .srv
                    return LoadTexture2DMipLevel(this.AssetName, xnd.RootNode, this.PicDesc, level, oldTexture);
                }
            }, Thread.Async.EAsyncTarget.AsyncIO);

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            LevelOfDetail = level;
            {
                if (StreamingTexture == null)
                {
                    //return this.mCoreObject.UpdateBuffer(rc.mCoreObject, new IGpuBufferData());
                    return false;
                }
                else
                {
                    if (AssetName != null)
                    {
                        StreamingTexture.SetDebugName("Texture:" + AssetName.ToString());
                    }

                    //var desc = this.mCoreObject.Desc;
                    //desc.Texture2D.MipLevels = tex2d.mCoreObject.Desc.MipLevels;
                    //var srv = rc.mCoreObject.CreateSRV(tex2d.mCoreObject.NativeSuper, in desc);
                    //var fp = this.mCoreObject.NativeSuper.GetFingerPrint();
                    //this.Core_Release();
                    //this.mCoreObject = srv;
                    //this.mCoreObject.NativeSuper.SetFingerPrint(fp + 1);
                    //return true;
                    return this.mCoreObject.UpdateBuffer(rc.mCoreObject, StreamingTexture.mCoreObject.NativeSuper);
                }
            }
        }
        #endregion

        private static UImageType GetOriginImageType(string address)
        {
            if (IO.TtFileManager.GetExtName(address) != ".srv")
                return UImageType.Unkown;
            using (var xnd = IO.TtXndHolder.LoadXnd(address))
            {
                if (xnd == null)
                {
                    return UImageType.Unkown;
                }
                var attr = xnd.RootNode.TryGetAttribute("Png");
                if (attr.IsValidPointer)
                    return UImageType.PNG;
                attr = xnd.RootNode.TryGetAttribute("Hdr");
                if (attr.IsValidPointer)
                    return UImageType.HDR;
                attr = xnd.RootNode.TryGetAttribute("Exr");
                if (attr.IsValidPointer)
                    return UImageType.EXR;
            }
            return UImageType.Unkown;
        }
        public static UImageType GetOriginImageType(RName name)
        {
            var meta = TtEngine.Instance.AssetMetaManager.GetAssetMeta(name) as TtSrViewAMeta;
            if (meta.OriginImageType != UImageType.Unkown)
            {
                return meta.OriginImageType;
            }

            return GetOriginImageType(name.Address);
        }

        public static Jither.OpenEXR.EXRFile LoadOriginExr(RName name, ref System.IO.Stream outStream)
        {
            //优先读真正的原始文件
            var ameta = TtEngine.Instance.AssetMetaManager.GetAssetMeta(name) as TtSrViewAMeta;
            if (ameta != null && ameta.OriginImageAddress != null)
            {
                if (ameta.OriginImageType == UImageType.EXR)
                {
                    outStream = System.IO.File.OpenRead(ameta.OriginImageAddress);
                    {
                        if (outStream == null)
                            return null;
                        return new Jither.OpenEXR.EXRFile(outStream);
                    }
                }
            }
            //尝试读保存在xnd里面的原始数据
            using (var xnd = IO.TtXndHolder.LoadXnd(name.Address))
            {
                if (xnd == null)
                {
                    var attr = xnd.RootNode.TryGetAttribute("Exr");
                    if (attr.IsValidPointer)
                    {
                        byte[] rawData;
                        using (var ar = attr.GetReader(null))
                        {
                            ar.ReadNoSize(out rawData, (int)attr.GetReaderLength());
                        }

                        outStream = new System.IO.MemoryStream(rawData);
                        {
                            var file = new Jither.OpenEXR.EXRFile(outStream);
                            return file;
                        }
                    }
                }
            }
            return null;
        }


        public static bool LoadOriginHdr(RName name, ref StbImageSharp.ImageResultFloat outImage)
        {
            //优先读真正的原始文件
            var ameta = TtEngine.Instance.AssetMetaManager.GetAssetMeta(name) as TtSrViewAMeta;
            if (ameta != null && ameta.OriginImageAddress != null)
            {
                if (ameta.OriginImageType == UImageType.HDR)
                {
                    using (var stream = System.IO.File.OpenRead(ameta.OriginImageAddress))
                    {
                        if (stream == null)
                            return false;
                        outImage = StbImageSharp.ImageResultFloat.FromStream(stream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                    }
                }
            }
            //尝试读保存在xnd里面的原始数据
            using (var xnd = IO.TtXndHolder.LoadXnd(name.Address))
            {
                if (xnd != null)
                {
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
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static StbImageSharp.TtMemImage LoadOriginPng(RName name)
        {
            //优先读真正的原始文件
            var ameta = TtEngine.Instance.AssetMetaManager.GetAssetMeta(name) as TtSrViewAMeta;
            if (ameta != null && ameta.OriginImageAddress != null)
            {
                if (ameta.OriginImageType == UImageType.PNG)
                {
                    using (var stream = System.IO.File.OpenRead(ameta.OriginImageAddress))
                    {
                        if (stream == null)
                            return null;
                        return StbImageSharp.TtMemImage.FromStream(stream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                    }
                }
            }

            //尝试读保存在xnd里面的原始数据
            using (var xnd = IO.TtXndHolder.LoadXnd(name.Address))
            {
                if (xnd != null)
                {
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
                            var image = StbImageSharp.TtMemImage.FromStream(memStream, StbImageSharp.ColorComponents.Default);
                            return image;
                        }
                    }
                }
            }

            return null;
        }
        #region static function
        public static int CalcMipLevel(int width, int height, bool isAnyZero, int Divisible)
        {
            int mipLevel = 0;
            do
            {
                height = height / 2;
                width = width / 2;

                if (height % Divisible != 0 || width % Divisible != 0)
                {
                    break;
                }

                if (isAnyZero)
                {
                    if ((height == 0 || width == 0))
                    {
                        break;
                    }
                }

                mipLevel++;
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
        public static unsafe void SaveTexture(RName assetName, XndNode node, Jither.OpenEXR.EXRFile file, TtPicDesc desc)
        {
            var part = file.Parts[0];
            System.Diagnostics.Debug.Assert(part.DataReader != null);
            System.Diagnostics.Debug.Assert(part.Channels.Count > 0);

            int width = part.DisplayWindow.Width;
            int height = part.DisplayWindow.Height;

            // 读取原始数据用于降采样
            var dataSize = part.DataReader.GetTotalByteCount();
            byte[] pixelData = new byte[dataSize];
            part.DataReader.ReadInterleaved(pixelData, new[] { "R", "G", "B", "A" });

            // 将Mip0（原始数据）转换为 ImageResultFloat
            int totalChannels = part.Channels.Count;
            int bytesPerPixel = part.Channels[0].Type.GetBytesPerPixel();
            StbImageSharp.ColorComponents colorComp = part.Channels.Count == 3 ? StbImageSharp.ColorComponents.RedGreenBlue : StbImageSharp.ColorComponents.RedGreenBlueAlpha;
            var imageFloat_Mip0 = new StbImageSharp.ImageResultFloat()
            {
                Width = width,
                Height = height,
                Comp = colorComp,
                Data = new float[width * height * (int)colorComp]
            };
            // 转换像素数据, uint/half/float->float
            for (int i = 0; i < width * height; ++i)
            {
                for (int c = 0; c < (int)colorComp && c < totalChannels; ++c)
                {
                    int srcIdx = i * totalChannels * bytesPerPixel + c * bytesPerPixel;
                    int destIdx = i * (int)colorComp + c;
                    if(part.Channels[0].Type == EXRDataType.Float)
                        imageFloat_Mip0.Data[destIdx] = BitConverter.ToSingle(pixelData, srcIdx);
                    else
                        imageFloat_Mip0.Data[destIdx] = (float)BitConverter.ToHalf(pixelData, srcIdx);
                }
            }

            // 使用ImageResultFloat的序列化
            CookTextureTo(assetName, node, imageFloat_Mip0, desc);
        }
        
        public static unsafe void CookTextureTo(RName assetName, XndNode node, StbImageSharp.ImageResultFloat image, TtPicDesc desc)
        {
            var writeComp = UStbImageUtility.ConvertColorComponent(image.Comp);
            if (desc.Depth == 0)
            {
                desc.Height = image.Height;
                desc.Width = image.Width;
            }
            else
            {

            }

            //if (desc.StripOriginSource && assetName != null)
            //{
            //    using (var memStream = new System.IO.FileStream(assetName.Address + ".hdr", System.IO.FileMode.OpenOrCreate))
            //    {
            //        var writer = new StbImageWriteSharp.ImageWriter();
            //        fixed (void* fptr = image.Data)
            //        {
            //            writer.WriteHdr(fptr, image.Width, image.Height, writeComp, memStream);
            //        }
            //    }
            //}
            //else
            //{
            //    if (desc.IsAutoSaveSrcImage == true)
            //    {
            //        using (var memStream = new System.IO.MemoryStream())
            //        {
            //            var writer = new StbImageWriteSharp.ImageWriter();
            //            fixed (void* fptr = image.Data)
            //            {
            //                writer.WriteHdr(fptr, image.Width, image.Height, writeComp, memStream);
            //            }
            //            var rawData = memStream.ToArray();

            //            var rawAttr = node.GetOrAddAttribute("Hdr", 0, 0, true);
            //            using (var ar = rawAttr.GetWriter((ulong)memStream.Position))
            //            {
            //                ar.WriteNoSize(rawData, (int)memStream.Position);
            //            }
            //        }
            //    }
            //}

            if (image.Width % 4 != 0 || image.Height % 4 != 0)
            {
                desc.DontCompress = true;
            }
            int mipLevel = 0;
            var curImage = image;
            desc.MipSizes.Clear();
            desc.CompressFormat = TtTextureHelper.SelectHdrCompressFormat(desc);
            switch (desc.CompressFormat)
            {
                case ETextureCompressFormat.TCF_None:
                    {
                        var hdrMipsNode = node.GetOrAddNode("HdrMips", 0, 0, true);
                        mipLevel = SaveHdrMips(hdrMipsNode, curImage, desc);
                    }
                    break;
                case ETextureCompressFormat.TCF_BC6:
                    {
                        var pngMipsNode = node.GetOrAddNode("DxtMips", 0, 0, true);
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
                        var pngMipsNode = node.GetOrAddNode("AstcMips", 0, 0, true);
                        //mipLevel = SaveAstcMips_ActcEncoder(pngMipsNode, curImage, desc);
                    }
                    break;
            }

            TtTextureHelper.SaveDescToNode(node, desc);
        }
        public static StbImageWriteSharp.ColorComponents GetImageWriteFormat(StbImageSharp.TtMemImage image)
        {
            switch(image.Comp)
            {
                case ColorComponents.RedGreenBlue:
                    return StbImageWriteSharp.ColorComponents.RedGreenBlue;
                case ColorComponents.RedGreenBlueAlpha:
                    return StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha;
                case ColorComponents.GreyAlpha:
                    return StbImageWriteSharp.ColorComponents.GreyAlpha;
                case ColorComponents.Grey:
                    return StbImageWriteSharp.ColorComponents.Grey;
            }
            return StbImageWriteSharp.ColorComponents.RedGreenBlue;
        }
        public static unsafe void CookTextureTo(RName assetName, XndNode node, StbImageSharp.TtMemImage image, TtPicDesc desc)
        {
            desc.Height = image.Height;
            desc.Width = image.Width;
            //if (desc.StripOriginSource && assetName != null)
            //{
            //    using (var memStream = new System.IO.FileStream(assetName.Address + ".png", System.IO.FileMode.OpenOrCreate))
            //    {
            //        var writer = new StbImageWriteSharp.ImageWriter();
            //        writer.WritePng(image.Data, image.Width, image.Height, GetImageWriteFormat(image), memStream);
            //    }
            //}
            //else
            //{
            //    if (desc.IsAutoSaveSrcImage == true)
            //    {
            //        using (var memStream = new System.IO.MemoryStream(image.Data.Length))
            //        {
            //            var writer = new StbImageWriteSharp.ImageWriter();

            //            writer.WritePng(image.Data, image.Width, image.Height, GetImageWriteFormat(image), memStream);
            //            var pngData = memStream.ToArray();

            //            var size = (uint)memStream.Length;
            //            if (size > 0)
            //            {
            //                var len = CoreSDK.CompressBound_ZSTD(size) + 5;
            //                using (var d = BigStackBuffer.CreateInstance((int)len))
            //                {
            //                    void* srcBuffer = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(pngData, 0).ToPointer();
            //                    var wSize = (uint)CoreSDK.Compress_ZSTD(d.GetBuffer(), len, srcBuffer, size, 1);
            //                }
            //            }

            //            var attr = node.GetOrAddAttribute("Png", 0, 0, true);
            //            using (var ar = attr.GetWriter((ulong)memStream.Position))
            //            {
            //                ar.WriteNoSize(pngData, (int)memStream.Position);
            //            }
            //        }
            //    }
            //}

            if (image.Width % 4 != 0 || image.Height % 4 != 0)
            {
                desc.DontCompress = true;
            }
            int mipLevel = 0;
            var curImage = image;
            if (image.Comp == ColorComponents.RedGreenBlue || image.Comp == ColorComponents.Grey)
                desc.BitNumAlpha = 0;
            desc.MipSizes.Clear();
            desc.CompressFormat = TtTextureHelper.SelectLdrCompressFormat(desc);
            switch (desc.CompressFormat)
            {
                case ETextureCompressFormat.TCF_None:
                    {
                        var pngMipsNode = node.GetOrAddNode("PngMips", 0, 0, true);
                        mipLevel = SavePngMips(pngMipsNode, curImage, desc);
                        desc.MipLevel = mipLevel;
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
                case ETextureCompressFormat.TCF_BC1:
                case ETextureCompressFormat.TCF_BC1A:
                case ETextureCompressFormat.TCF_BC2:
                case ETextureCompressFormat.TCF_BC3:
                case ETextureCompressFormat.TCF_BC4:
                case ETextureCompressFormat.TCF_BC5:
                case ETextureCompressFormat.TCF_BC6:
                case ETextureCompressFormat.TCF_BC6_FLOAT:
                case ETextureCompressFormat.TCF_BC7_UNORM:
                    {
                        var pngMipsNode = node.GetOrAddNode("DxtMips", 0, 0, true);
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
                        var pngMipsNode = node.GetOrAddNode("EtcMips", 0, 0, true);
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
                        var pngMipsNode = node.GetOrAddNode("AstcMips", 0, 0, true);
                        mipLevel = SaveAstcMips_ActcEncoder(pngMipsNode, curImage, desc);
                    }
                    break;
            }

            TtTextureHelper.SaveDescToNode(node, desc);
        }

        public static int SaveHdrMips(XndNode hdrMipsNode, StbImageSharp.ImageResultFloat curImage, TtPicDesc desc)
        {
            desc.CubeFaces = Math.Max(desc.CubeFaces, 1);
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

            // 对于 3D Texture，从 2D image 中解析 slices
            int sliceWidth, sliceHeight, sliceSize;
            int slicesPerRow = 1, slicesPerColumn = 1;
            int imageWidth = width;
            int imageHeight = height;

            if (desc.IsTexture3D && desc.Depth > 0)
            {
                // 使用 TtTextureHelper 计算 3D Texture 布局
                var layout = TtTextureHelper.Calculate3DTextureLayout(imageWidth, imageHeight, desc);
                sliceWidth = layout.SliceWidth;
                sliceHeight = layout.SliceHeight;
                slicesPerRow = layout.SlicesPerRow;
                slicesPerColumn = layout.SlicesPerColumn;

                // 重新计算MipLevel（基于单个slice的尺寸）
                TtTextureHelper.CalculateAndSetMipLevel(desc, sliceWidth, sliceHeight, true, 4);

                sliceSize = sliceWidth * sliceHeight;
            }
            else
            {
                // 2D Texture
                TtTextureHelper.CalculateAndSetMipLevel(desc, curImage.Width, curImage.Height, true, 4);
                sliceWidth = imageWidth;
                sliceHeight = imageHeight;
                sliceSize = imageWidth * imageHeight;
            }

            desc.MipSizes.Clear();

            for (uint i = 0; i < desc.Desc.CubeFaces; i++)
            {
                var faceNode = hdrMipsNode.GetOrAddNode($"Face{i}", 0, 0, true);

                // 优化：统一处理 2D 和 3D Texture 的 mipmap 生成
                if (desc.IsTexture3D && desc.Depth > 0)
                {
                    // ===== 3D Texture 处理 =====
                    var depthSlicesNode = faceNode.GetOrAddNode("DepthSlices", 0, 0, true);

                    // Step 1: 一次性提取所有原始 slice
                    StbImageSharp.ImageResultFloat[] allOriginalSlices;
                    TtTextureHelper.GenerateAllMips3DOptimized(
                        curImage.Data, imageWidth, imageHeight,
                        sliceWidth, sliceHeight, desc.Depth,
                        slicesPerRow, (int)curImage.Comp,
                        out allOriginalSlices);

                    // Step 2: 级联生成所有 mipmap 层级
                    StbImageSharp.ImageResultFloat[][] allMipLevels = new StbImageSharp.ImageResultFloat[desc.MipLevel][];
                    allMipLevels[0] = allOriginalSlices; // Mip0 就是原始数据

                    // Step 3: 从 Mip0 级联降采样生成后续各级
                    for (uint j = 1; j < desc.MipLevel; j++)
                    {
                        int prevMipIndex = (int)j - 1;
                        int mipWidth = Math.Max(1, sliceWidth >> (int)j);
                        int mipHeight = Math.Max(1, sliceHeight >> (int)j);
                        int mipDepth = Math.Max(1, desc.Depth >> (int)j);

                        allMipLevels[j] = new StbImageSharp.ImageResultFloat[mipDepth];

                        // 从上一级的每个 slice 降采样到当前级
                        for (int d = 0; d < mipDepth; d++)
                        {
                            allMipLevels[j][d] = StbImageSharp.ImageProcessor.GetBoxDownSampler(
                                allMipLevels[prevMipIndex][d], mipWidth, mipHeight);
                        }
                    }

                    // Step 4: 保存所有 mipmap 数据到 XND
                    for (uint j = 0; j < desc.MipLevel; j++)
                    {
                        var mipNode = depthSlicesNode.GetOrAddNode($"Mip{j}", 0, 0, true);
                        int mipDepth = allMipLevels[j].Length;

                        for (int d = 0; d < mipDepth; d++)
                        {
                            var downsampledSlice = allMipLevels[j][d];

                            using (var memStream = new System.IO.MemoryStream())
                            {
                                var writer = new StbImageWriteSharp.ImageWriter();
                                unsafe
                                {
                                    var writeComp = UStbImageUtility.ConvertColorComponent(downsampledSlice.Comp);
                                    fixed (void* ptr = downsampledSlice.Data)
                                    {
                                        writer.WriteHdr(ptr, downsampledSlice.Width, downsampledSlice.Height, writeComp, memStream);
                                    }
                                }

                                var hdrData = memStream.ToArray();
                                var attr = mipNode.GetOrAddAttribute($"HdrMipSlice{d}", 0, 0, true);
                                using (var ar = attr.GetWriter((ulong)memStream.Position))
                                {
                                    ar.WriteNoSize(hdrData, (int)memStream.Position);
                                }
                            }

                            // 只在第一片添加 MipSizes
                            if (i == 0 && d == 0)
                            {
                                desc.MipSizes.Add(new Vector3i()
                                {
                                    X = downsampledSlice.Width,
                                    Y = downsampledSlice.Height,
                                    Z = mipDepth
                                });
                            }
                        }
                    }
                }
                else
                {
                    // ===== 2D Texture 处理 =====
                    // 优化：级联生成所有 mipmap 层级
                    StbImageSharp.ImageResultFloat[] mipLevels = TtTextureHelper.GenerateAllMips2DOptimized(
                        curImage, (int)desc.MipLevel);

                    // 保存所有 mipmap 数据到 XND
                    for (uint j = 0; j < desc.MipLevel; j++)
                    {
                        var currentImage = mipLevels[j];

                        using (var memStream = new System.IO.MemoryStream())
                        {
                            var writer = new StbImageWriteSharp.ImageWriter();
                            unsafe
                            {
                                var writeComp = UStbImageUtility.ConvertColorComponent(currentImage.Comp);
                                fixed (void* ptr = currentImage.Data)
                                {
                                    writer.WriteHdr(ptr, currentImage.Width, currentImage.Height, writeComp, memStream);
                                }
                            }

                            var hdrData = memStream.ToArray();
                            var attr = faceNode.GetOrAddAttribute($"HdrMip{j}", 0, 0, true);
                            using (var ar = attr.GetWriter((ulong)memStream.Position))
                            {
                                ar.WriteNoSize(hdrData, (int)memStream.Position);
                            }
                        }

                        desc.MipSizes.Add(new Vector3i()
                        {
                            X = currentImage.Width,
                            Y = currentImage.Height,
                            Z = currentImage.Width * (int)currentImage.Comp * sizeof(float)
                        });
                    }
                }
            }

            return desc.MipLevel;
        }

        public static int SavePngMips(XndNode pngMipsNode, StbImageSharp.TtMemImage curImage, TtPicDesc desc)
        {
            desc.CubeFaces = Math.Max(desc.CubeFaces, 1);
            int height = curImage.Height;
            int width = curImage.Width;

            // 兼容性检查：如果是普通的2D纹理（CubeFaces=1 且 Depth<=1），使用旧的属性结构
            bool useLegacyFormat = (desc.CubeFaces == 1 && desc.Depth <= 1);

            // 对于 3D Texture，从 2D image 中解析 slices
            int sliceWidth, sliceHeight, sliceSize;
            int slicesPerRow = 1, slicesPerColumn = 1;
            int imageWidth = width;
            int imageHeight = height;

            if (desc.IsTexture3D && desc.Depth > 0)
            {
                // 使用 TtTextureHelper 计算 3D Texture 布局
                var layout = TtTextureHelper.Calculate3DTextureLayout(imageWidth, imageHeight, desc);
                sliceWidth = layout.SliceWidth;
                sliceHeight = layout.SliceHeight;
                slicesPerRow = layout.SlicesPerRow;
                slicesPerColumn = layout.SlicesPerColumn;

                // 重新计算MipLevel（基于单个slice的尺寸）
                TtTextureHelper.CalculateAndSetMipLevel(desc, sliceWidth, sliceHeight, false, 4);

                sliceSize = sliceWidth * sliceHeight;
            }
            else
            {
                // 2D Texture
                TtTextureHelper.CalculateAndSetMipLevel(desc, curImage.Width, curImage.Height, false, 4);
                sliceWidth = imageWidth;
                sliceHeight = imageHeight;
                sliceSize = imageWidth * imageHeight;
            }

            desc.MipSizes.Clear();

            // 兼容性：普通2D纹理使用旧的属性结构（PngMipsNode/PngMip0, PngMip1...）
            if (useLegacyFormat)
            {
                int mipLevel = 0;
                do
                {
                    using (var memStream = new System.IO.MemoryStream(curImage.Data.Length))
                    {
                        var writer = new StbImageWriteSharp.ImageWriter();
                        writer.WritePng(curImage.Data, curImage.Width, curImage.Height, GetImageWriteFormat(curImage), memStream);
                        var pngData = memStream.ToArray();
                        var attr = pngMipsNode.GetOrAddAttribute($"PngMip{mipLevel}", 0, 0, true);
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
                    if (curImage == null)
                        break;
                    if (desc.MipLevel > 0 && mipLevel == desc.MipLevel)
                        break;
                }
                while (true);

                return mipLevel;
            }

            // 新格式：Cube或3D纹理使用节点结构（PngMipsNode/Face0/PngMip0, PngMipsNode/Face0/DepthSlices/Mip0/...）
            for (uint i = 0; i < desc.Desc.CubeFaces; i++)
            {
                var faceNode = pngMipsNode.GetOrAddNode($"Face{i}", 0, 0, true);

                // 优化：统一处理 2D 和 3D Texture 的 mipmap 生成
                if (desc.IsTexture3D && desc.Depth > 0)
                {
                    // ===== 3D Texture 处理 =====
                    var depthSlicesNode = faceNode.GetOrAddNode("DepthSlices", 0, 0, true);

                    // Step 1: 一次性提取所有原始 slice
                    StbImageSharp.TtMemImage[] allOriginalSlices;
                    TtTextureHelper.GenerateAllMips3DOptimizedPng(
                        curImage.Data, imageWidth, imageHeight,
                        sliceWidth, sliceHeight, desc.Depth,
                        slicesPerRow, out allOriginalSlices);

                    // Step 2: 级联生成所有 mipmap 层级
                    StbImageSharp.TtMemImage[][] allMipLevels = new StbImageSharp.TtMemImage[desc.MipLevel][];
                    allMipLevels[0] = allOriginalSlices; // Mip0 就是原始数据

                    // Step 3: 从 Mip0 级联降采样生成后续各级
                    for (uint j = 1; j < desc.MipLevel; j++)
                    {
                        int prevMipIndex = (int)j - 1;
                        int mipWidth = Math.Max(1, sliceWidth >> (int)j);
                        int mipHeight = Math.Max(1, sliceHeight >> (int)j);
                        int mipDepth = Math.Max(1, desc.Depth >> (int)j);

                        allMipLevels[j] = new StbImageSharp.TtMemImage[mipDepth];

                        // 从上一级的每个 slice 降采样到当前级
                        for (int d = 0; d < mipDepth; d++)
                        {
                            allMipLevels[j][d] = StbImageSharp.ImageProcessor.GetBoxDownSampler(
                                allMipLevels[prevMipIndex][d], mipWidth, mipHeight);
                        }
                    }

                    // Step 4: 保存所有 mipmap 数据到 XND
                    for (uint j = 0; j < desc.MipLevel; j++)
                    {
                        var mipNode = depthSlicesNode.GetOrAddNode($"Mip{j}", 0, 0, true);
                        int mipDepth = allMipLevels[j].Length;

                        for (int d = 0; d < mipDepth; d++)
                        {
                            var downsampledSlice = allMipLevels[j][d];

                            using (var memStream = new System.IO.MemoryStream(downsampledSlice.Data.Length))
                            {
                                var writer = new StbImageWriteSharp.ImageWriter();
                                writer.WritePng(downsampledSlice.Data, downsampledSlice.Width, downsampledSlice.Height, GetImageWriteFormat(downsampledSlice), memStream);

                                var pngData = memStream.ToArray();
                                var attr = mipNode.GetOrAddAttribute($"PngMipSlice{d}", 0, 0, true);
                                using (var ar = attr.GetWriter((ulong)memStream.Position))
                                {
                                    ar.WriteNoSize(pngData, (int)memStream.Position);
                                }
                            }

                            // 只在第一片添加 MipSizes
                            if (i == 0 && d == 0)
                            {
                                desc.MipSizes.Add(new Vector3i()
                                {
                                    X = downsampledSlice.Width,
                                    Y = downsampledSlice.Height,
                                    Z = mipDepth
                                });
                            }
                        }
                    }
                }
                else
                {
                    // ===== 2D Texture 处理 =====
                    // 优化：级联生成所有 mipmap 层级
                    StbImageSharp.TtMemImage[] mipLevels = TtTextureHelper.GenerateAllMips2DOptimizedPng(
                        curImage, (int)desc.MipLevel);

                    // 保存所有 mipmap 数据到 XND
                    for (uint j = 0; j < desc.MipLevel; j++)
                    {
                        var currentImage = mipLevels[j];

                        using (var memStream = new System.IO.MemoryStream(currentImage.Data.Length))
                        {
                            var writer = new StbImageWriteSharp.ImageWriter();
                            writer.WritePng(currentImage.Data, currentImage.Width, currentImage.Height, GetImageWriteFormat(currentImage), memStream);

                            var pngData = memStream.ToArray();
                            var attr = faceNode.GetOrAddAttribute($"PngMip{j}", 0, 0, true);
                            using (var ar = attr.GetWriter((ulong)memStream.Position))
                            {
                                ar.WriteNoSize(pngData, (int)memStream.Position);
                            }
                        }

                        desc.MipSizes.Add(new Vector3i()
                        {
                            X = currentImage.Width,
                            Y = currentImage.Height,
                            Z = currentImage.Width * 4
                        });
                    }
                }
            }

            return desc.MipLevel;
        }
        public unsafe static int SaveDxtMips(XndNode mipsNode, StbImageSharp.TtMemImage curImage, TtPicDesc desc)
        {
            System.Diagnostics.Debug.Assert(desc.DontCompress == false);

            var srcImage = new TextureCompress.FCubeImage();
            fixed (byte* pImageData = &curImage.Data[0])
            {
                srcImage.m_Image0 = (uint*)pImageData;
                var blobResult = new Support.TtBlobObject();
                if (TextureCompress.CrunchWrap.CompressPixels(16, blobResult.mCoreObject, (uint)curImage.Width, (uint)curImage.Height, in srcImage, desc.CompressFormat, true, desc.sRGB, 0, 255))
                {
                    using (var reader = blobResult.CreateReader())
                    {
                        var ar = new IO.AuxReader<EngineNS.IO.TtMemReader>(reader, null);
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
                            var faceNode = mipsNode.GetOrAddNode($"Face{i}", 0, 0, true);
                            for (uint j = 0; j < desc.Desc.MipLevel; j++)
                            {
                                var mipSize = new Vector3i();
                                ar.Read(out mipSize);
                                desc.MipSizes.Add(mipSize);
                                uint total_face_size = 0;
                                ar.Read(out total_face_size);
                                var pixels = new byte[total_face_size];
                                ar.ReadNoSize(pixels, (int)total_face_size);

                                var attr = faceNode.GetOrAddAttribute($"DxtMip{j}", 0, 0, true);
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
        }
        public unsafe static int SaveDxtMips_BcEncoder(XndNode mipsNode, StbImageSharp.ImageResultFloat curImage, TtPicDesc desc)
        {
            System.Diagnostics.Debug.Assert(desc.DontCompress == false);

            TtTextureHelper.CalculateAndSetMipLevel(desc, curImage.Width, curImage.Height, false, 4);
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

            int imageWidth = curImage.Width;
            int imageHeight = curImage.Height;
            if (desc.CubeFaces == 6)
            {
                desc.Width = desc.Height = imageWidth = imageHeight = MathHelper.Min(imageWidth, imageHeight);
            }
            else
                desc.CubeFaces = 1;

            // 对于 3D Texture，从 2D image 中解析 slices
            // 自动计算数据行列布局（更灵活的方式）
            int sliceWidth, sliceHeight, sliceSize;
            int slicesPerRow = 1, slicesPerColumn = 1;
            if (desc.IsTexture3D && desc.Depth > 0)
            {
                // 使用 TtTextureHelper 计算 3D Texture 布局
                var layout = TtTextureHelper.Calculate3DTextureLayout(imageWidth, imageHeight, desc);
                sliceWidth = layout.SliceWidth;
                sliceHeight = layout.SliceHeight;
                slicesPerRow = layout.SlicesPerRow;
                slicesPerColumn = layout.SlicesPerColumn;

                // 重新计算MipLevel
                TtTextureHelper.CalculateAndSetMipLevel(desc, sliceWidth, sliceHeight, false, 4);

                sliceSize = sliceWidth * sliceHeight;
            }
            else
            {
                // 2D Texture
                sliceWidth = imageWidth;
                sliceHeight = imageHeight;
                sliceSize = imageWidth * imageHeight;
            }

            System.DateTime beginTime = System.DateTime.Now;
            Profiler.Log.WriteInfoSimple("Start SaveDxtMips_BcEncoder");

            // 优化：对于 3D Texture，使用 GetBoxDownSampler 预先生成所有 mipmap 级别的数据
            ColorRgbFloat[][][] allMipLevelsData = null;
            if (desc.IsTexture3D && desc.Depth > 0)
            {
                // 使用 GetBoxDownSampler 级联降采样，生成所有 mipmap 级别
                allMipLevelsData = TtTextureHelper.GenerateAllMips3DForBc6(
                    curImage.Data, imageWidth, imageHeight,
                    sliceWidth, sliceHeight, desc.Depth,
                    slicesPerRow, (int)curImage.Comp,
                    (int)desc.MipLevel);
            }

            for (uint i = 0; i < desc.Desc.CubeFaces; i++)
            {
                var faceNode = mipsNode.GetOrAddNode($"Face{i}", 0, 0, true);

                byte[][] pixelsBcnMips = null;
                // 3D Texture：创建 DepthSlices 节点存储每一层的数据
                XndNode depthSlicesNode = new XndNode();
                if (desc.IsTexture3D && desc.Depth > 0)
                {
                    depthSlicesNode = faceNode.GetOrAddNode("DepthSlices", 0, 0, true);
                }
                else
                {
                    // 2D Texture：原有逻辑
                    ColorRgbFloat[] colorDataFace = new ColorRgbFloat[sliceSize];
                    for (int iC = 0; iC < sliceSize; ++iC)
                    {
                        colorDataFace[iC].r = curImage.Data[(i * sliceSize + iC) * (int)curImage.Comp];
                        colorDataFace[iC].g = curImage.Data[(i * sliceSize + iC) * (int)curImage.Comp + Math.Min(1, (int)curImage.Comp - 1)];
                        colorDataFace[iC].b = curImage.Data[(i * sliceSize + iC) * (int)curImage.Comp + Math.Min(2, (int)curImage.Comp - 1)];
                    }
                    var memory2DFace = colorDataFace.AsMemory().AsMemory2D(sliceHeight, sliceWidth);
                    pixelsBcnMips = encoder.EncodeToRawBytesHdr(memory2DFace);
                }

                for (uint j = 0; j < desc.MipLevel; j++)
                {
                    var mipSize = new Vector3i();
                    var blockDimension = new Vector2i();

                    if (desc.IsTexture3D && desc.Depth > 0)
                    {
                        // 3D Texture：为每个 mipmap 创建一个节点，包含所有 depth 层
                        var mipNode = depthSlicesNode.GetOrAddNode($"Mip{j}", 0, 0, true);

                        // 计算当前 mipmap 的尺寸
                        int mipDepth = Math.Max(1, desc.Depth >> (int)j);
                        int mipWidth = Math.Max(1, sliceWidth >> (int)j);
                        int mipHeight = Math.Max(1, sliceHeight >> (int)j);

                        // 编码每一层并保存
                        for (int d = 0; d < mipDepth; d++)
                        {
                            var attr = mipNode.GetOrAddAttribute($"DxtMipSlice{d}", 0, 0, true);

                            // 优化：使用预先生成的 mipmap 数据（已经过级联降采样）
                            ColorRgbFloat[] colorDataSlice = allMipLevelsData[j][d];

                            var memory2DSlice = colorDataSlice.AsMemory().AsMemory2D(mipHeight, mipWidth);

                            // 编码当前层（传入 0 表示数据已经降采样）
                            var pixelsBcn = encoder.EncodeToRawBytesHdr(memory2DSlice, 0, out mipSize.X, out mipSize.Y);

                            encoder.GetBlockCount(mipSize.X, mipSize.Y, out blockDimension.X, out blockDimension.Y);
                            desc.BlockSize = encoder.GetBlockSize();

                            // 只在第一层第一片添加 MipSizes
                            if (i == 0 && d == 0)
                            {
                                mipSize.Z = mipDepth;
                                desc.MipSizes.Add(mipSize);
                                desc.BlockDimenstions.Add(blockDimension);
                            }

                            using (var ar2 = attr.GetWriter((ulong)pixelsBcn.Length))
                            {
                                ar2.WriteNoSize(pixelsBcn, (int)pixelsBcn.Length);
                            }
                        }
                    }
                    else
                    {
                        var pixelsBcn = pixelsBcnMips[j];

                        encoder.CalculateMipMapSize(sliceWidth, sliceHeight, (int)j, out mipSize.X, out mipSize.Y);
                        encoder.GetBlockCount(mipSize.X, mipSize.Y, out blockDimension.X, out blockDimension.Y);
                        desc.BlockSize = encoder.GetBlockSize();
                        if(desc.MipSizes.Count < desc.MipLevel)
                        {
                            desc.MipSizes.Add(mipSize);
                            desc.BlockDimenstions.Add(blockDimension);
                        }

                        var attr = faceNode.GetOrAddAttribute($"DxtMip{j}", 0, 0, true);
                        {
                            using (var ar2 = attr.GetWriter((ulong)pixelsBcn.Length))
                            {
                                ar2.WriteNoSize(pixelsBcn, (int)pixelsBcn.Length);
                            }
                        }
                    }
                }
            }

            System.DateTime endTime = System.DateTime.Now;
            Profiler.Log.WriteInfoSimple("End SaveDxtMips_BcEncoder");
            Profiler.Log.WriteInfoSimple("Use Time : " + endTime.Subtract(beginTime).TotalMilliseconds + " ms");

            return desc.MipLevel;
        }

        public unsafe static int SaveDxtMips_BcEncoder(XndNode mipsNode, StbImageSharp.TtMemImage curImage, TtPicDesc desc)
        {
            System.Diagnostics.Debug.Assert(desc.DontCompress == false);

            BcEncoder encoder = new BcEncoder();
            bool IsKtx = false;
            desc.CubeFaces = 1;
            if (desc.MipLevel == 0)
                desc.MipLevel = CalcMipLevel(curImage.Width, curImage.Height, true, 4);
            EPixelFormat descPixelFormat = EPixelFormat.PXF_UNKNOWN;
            switch (desc.CompressFormat)
            {
                case ETextureCompressFormat.TCF_BC1:
                    if (desc.sRGB)
                        descPixelFormat = EPixelFormat.PXF_BC1_UNORM_SRGB;
                    else
                        descPixelFormat = EPixelFormat.PXF_BC1_UNORM;
                    encoder.OutputOptions.Format = CompressionFormat.Bc1;
                    break;
                case ETextureCompressFormat.TCF_BC1A:
                    if (desc.sRGB)
                        descPixelFormat = EPixelFormat.PXF_BC1_UNORM_SRGB;
                    else
                        descPixelFormat = EPixelFormat.PXF_BC1_UNORM;
                    encoder.OutputOptions.Format = CompressionFormat.Bc1WithAlpha;
                    break;
                case ETextureCompressFormat.TCF_BC2:
                    if (desc.sRGB)
                        descPixelFormat = EPixelFormat.PXF_BC2_UNORM_SRGB;
                    else
                        descPixelFormat = EPixelFormat.PXF_BC2_UNORM;
                    encoder.OutputOptions.Format = CompressionFormat.Bc2;
                    break;
                case ETextureCompressFormat.TCF_BC3:
                    if (desc.sRGB)
                        descPixelFormat = EPixelFormat.PXF_BC3_UNORM_SRGB;
                    else
                        descPixelFormat = EPixelFormat.PXF_BC3_UNORM;
                    encoder.OutputOptions.Format = CompressionFormat.Bc3;
                    break;
                case ETextureCompressFormat.TCF_BC4:
                    descPixelFormat = EPixelFormat.PXF_BC4_UNORM;
                    encoder.OutputOptions.Format = CompressionFormat.Bc4;
                    break;
                case ETextureCompressFormat.TCF_BC5:
                        descPixelFormat = EPixelFormat.PXF_BC5_UNORM;
                    encoder.OutputOptions.Format = CompressionFormat.Bc5;
                    break;
                case ETextureCompressFormat.TCF_BC6:
                    descPixelFormat = EPixelFormat.PXF_BC6H_UF16;
                    encoder.OutputOptions.Format = CompressionFormat.Bc6U;
                    break;
                case ETextureCompressFormat.TCF_BC6_FLOAT:
                    descPixelFormat = EPixelFormat.PXF_BC6H_SF16;
                    encoder.OutputOptions.Format = CompressionFormat.Bc6S;
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
            encoder.OutputOptions.Quality = CompressionQuality.Balanced;
            //encoder.OutputOptions.Format = CompressionFormat.Bc2;

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
                System.Threading.Tasks.Task<byte[]>[] taskArray = null;
                bool isEncodeMultiThread = true;
                if (isEncodeMultiThread==true)
                {
                    taskArray = new System.Threading.Tasks.Task<byte[]>[desc.Desc.MipLevel];
                    for (uint j = 0; j < desc.Desc.MipLevel; j++)
                    {
                        taskArray[j] = encoder.EncodeToRawBytesAsync(curImage.Data, curImage.Width, curImage.Height, pixelFormat, (int)j);
                    }
                    System.Threading.Tasks.Task.WaitAll(taskArray);
                }


                var faceNode = mipsNode.GetOrAddNode($"Face{i}", 0, 0, true);
                for (uint j = 0; j < desc.Desc.MipLevel; j++)
                {
                    var mipSize = new Vector3i();
                    var blockDimension = new Vector2i();
                    byte[] pixelsBcn = null;
                    if(isEncodeMultiThread==true)
                    {
                        pixelsBcn = taskArray[j].Result;
                        encoder.CalculateMipMapSize(curImage.Width, curImage.Height, (int)j, out mipSize.X, out mipSize.Y);
                    }
                    else
                    {
                        pixelsBcn = encoder.EncodeToRawBytes(curImage.Data.AsSpan(), curImage.Width, curImage.Height, pixelFormat, (int)j, out mipSize.X, out mipSize.Y);
                    }
                    encoder.GetBlockCount(mipSize.X, mipSize.Y, out blockDimension.X, out blockDimension.Y);
                    desc.BlockSize = encoder.GetBlockSize();
                    desc.MipSizes.Add(mipSize);
                    desc.BlockDimenstions.Add(blockDimension);

                    if (IsKtx)
                    {
                        var attr = faceNode.GetOrAddAttribute($"EtcMip{j}", 0, 0, true);
                        {
                            using (var ar2 = attr.GetWriter((ulong)pixelsBcn.Length))
                            {
                                ar2.WriteNoSize(pixelsBcn, (int)pixelsBcn.Length);
                            }
                        }
                    }
                    else
                    {
                        var attr = faceNode.GetOrAddAttribute($"DxtMip{j}", 0, 0, true);
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
        public unsafe static int SaveAstcMips_ActcEncoder(XndNode mipsNode, StbImageSharp.TtMemImage curImage, TtPicDesc desc)
        {
            System.Diagnostics.Debug.Assert(desc.DontCompress == false);
            System.Diagnostics.Debug.Assert(false);
            return 0;
        }
        public static unsafe Support.TtBlobObject[] LoadPixelMipLevels(RName name, uint mipLevel, TtPicDesc desc)
        {
            using (var xnd = IO.TtXndHolder.LoadXnd(name.Address))
            {
                if (desc != null)
                {
                    TtTextureHelper.LoadPictureDesc(xnd.RootNode, desc);
                }

                var pngNode = xnd.RootNode.TryGetChildNode("PngMips");
                if (pngNode.IsValidPointer)
                {
                    if (mipLevel == 0)
                    {
                        mipLevel = pngNode.GetNumOfAttribute();
                    }
                    var result = new StbImageSharp.TtMemImage[mipLevel];
                    var blobs = new Support.TtBlobObject[mipLevel];
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
                            result[i] = StbImageSharp.TtMemImage.FromStream(memStream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                            blobs[i] = new Support.TtBlobObject();
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
                            var blobs = new Support.TtBlobObject[mipLevel];
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

                                blobs[i] = new Support.TtBlobObject();
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

        public static unsafe TtTexture LoadTexture2DMipLevel(RName rn, IO.TtXndNode node, TtPicDesc desc, int level, TtTexture oldTexture)
        {
            switch (desc.CompressFormat)
            {
                case ETextureCompressFormat.TCF_None:
                    {
                        var pngNode = node.TryGetChildNode("PngMips");
                        if (pngNode.IsValidPointer)
                            return LoadPngTexture2DMipLevel(rn, node, desc, level);
                        else
                        {
                            var hdrNode = node.TryGetChildNode("HdrMips");
                            if (hdrNode.IsValidPointer)
                                return LoadHdrTexture2DMipLevel(rn, node, desc, level);
                            else
                            {
                                var exrNode = node.TryGetChildNode("ExrMips");
                                if (exrNode.IsValidPointer)
                                {
                                    Profiler.Log.WriteLine<Profiler.TtAssetGategory>(Profiler.ELogTag.Warning, $"Exr format is not supported");
                                    return null;
                                }
                            }
                        }
                        return null;
                    }
                case ETextureCompressFormat.TCF_BC1:
                case ETextureCompressFormat.TCF_BC1A:
                case ETextureCompressFormat.TCF_BC2:
                case ETextureCompressFormat.TCF_BC3:
                case ETextureCompressFormat.TCF_BC4:
                case ETextureCompressFormat.TCF_BC5:
                case ETextureCompressFormat.TCF_BC6:
                case ETextureCompressFormat.TCF_BC6_FLOAT:
                    {
                        oldTexture = null;
                        return LoadDxtTexture2DMipLevel(rn, node, desc, level, oldTexture);
                    }
                default:
                    return null;
            }
        }
        #region Load Mips
        private static unsafe TtTexture LoadHdrTexture2DMipLevel(RName rn, TtXndNode node, TtPicDesc desc, int mipLevel)
        {
            if (mipLevel == 0)
                return null;
            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            var hdrNode = node.TryGetChildNode("HdrMips");
            if (hdrNode.NativePointer == IntPtr.Zero)
                return null;

            // 判断是否为 3D Texture
            bool isTexture3D = desc.IsTexture3D && desc.Depth > 0;

            // 确定颜色组件数
            StbImageSharp.ColorComponents colorComp = desc.Format switch
            {
                EPixelFormat.PXF_R32G32B32_FLOAT => ColorComponents.RedGreenBlue,
                _ => ColorComponents.RedGreenBlueAlpha
            };

            // 计算数据单元数量
            int totalDataUnits = isTexture3D ? mipLevel : (int)desc.CubeFaces * mipLevel;
            using var guard = new UMipmapResourceGuard(totalDataUnits);
            var pInitData = stackalloc FMappedSubResource[totalDataUnits];

                for (uint j = 0; j < desc.Desc.CubeFaces; j++)
                {
                    var faceNode = hdrNode.TryGetChildNode($"Face{j}");
                    if (faceNode.IsValidPointer == false)
                    {
                        continue;
                    }

                    if (isTexture3D)
                    {
                        // 3D Texture：从 DepthSlices 节点加载
                        var depthSlicesNode = faceNode.TryGetChildNode("DepthSlices");
                        if (depthSlicesNode.IsValidPointer == false)
                            return null;

                        for (uint i = 0; i < mipLevel; i++)
                        {
                            var realLevel = desc.MipLevel - mipLevel + i;
                            var mipNode = depthSlicesNode.TryGetChildNode($"Mip{realLevel}");
                            if (mipNode.IsValidPointer == false)
                                return null;

                            // 计算当前 mipmap level 的 depth
                            int currentDepth = Math.Max(1, desc.Depth >> (int)realLevel);

                            // 收集所有 slice 的数据
                            var allSlicesData = new List<float[]>();
                            uint totalRowPitch = 0;
                            uint totalDepthPitch = 0;

                            for (int d = 0; d < currentDepth; d++)
                            {
                                var ptr = mipNode.TryGetAttribute($"HdrMipSlice{d}");
                                if (ptr.NativePointer == IntPtr.Zero)
                                    return null;

                                var mipAttr = ptr;
                                byte[] data;
                                using (var ar = mipAttr.GetReader(null))
                                {
                                    ar.ReadNoSize(out data, (int)mipAttr.GetReaderLength());
                                }

                                // 解码 HDR 数据
                                StbImageSharp.ImageResultFloat image;
                                using (var memStream = new System.IO.MemoryStream(data, false))
                                {
                                    image = StbImageSharp.ImageResultFloat.FromStream(memStream, colorComp);
                                }

                                allSlicesData.Add(image.Data);

                                if (d == 0)
                                {
                                    // 第一个 slice 的 RowPitch
                                    totalRowPitch = (uint)image.Width * (uint)colorComp * sizeof(float);
                                    totalDepthPitch = totalRowPitch * (uint)image.Height;
                                }
                            }

                            // 将所有 slice 的数据合并成一个连续的缓冲区
                            int totalFloats = allSlicesData.Sum(d => d.Length);
                            float[] mergedData = new float[totalFloats];
                            int offset = 0;
                            foreach (var sliceData in allSlicesData)
                            {
                                Array.Copy(sliceData, 0, mergedData, offset, sliceData.Length);
                                offset += sliceData.Length;
                            }

                            int dataIndex = (int)i;
                            pInitData[dataIndex].m_pData = guard.SetDataFloat(dataIndex, mergedData);
                            pInitData[dataIndex].m_RowPitch = totalRowPitch;
                            pInitData[dataIndex].m_DepthPitch = totalDepthPitch;
                        }
                    }
                    else
                    {
                        // 2D Texture：原有逻辑
                        for (uint i = 0; i < mipLevel; i++)
                        {
                            var realLevel = desc.MipLevel - mipLevel + i;
                            var ptr = faceNode.TryGetAttribute($"HdrMip{realLevel}");
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

                            int dataIndex = (int)(j * mipLevel + i);
                            pInitData[dataIndex].m_pData = guard.SetDataFloat(dataIndex, image.Data);
                            pInitData[dataIndex].m_RowPitch = (uint)image.Width * (uint)colorComp * sizeof(float);
                            pInitData[dataIndex].m_DepthPitch = pInitData[dataIndex].m_RowPitch * (uint)image.Height;
                        }
                    }
                }

                var texDesc = new FTextureDesc();
                texDesc.SetDefault();
                texDesc.Width = (uint)desc.MipSizes[desc.MipLevel - mipLevel].X;
                texDesc.Height = (uint)desc.MipSizes[desc.MipLevel - mipLevel].Y;
                texDesc.MipLevels = (uint)mipLevel;
                texDesc.InitData = pInitData;
                texDesc.Format = desc.Format;

                if (isTexture3D)
                {
                    texDesc.Depth = (uint)desc.MipSizes[desc.MipLevel - mipLevel].Z;
                    texDesc.ArraySize = 1;
                }

                var result = rc.CreateTexture(in texDesc);
                CoreSDK.SetMemDebugText(result.mCoreObject, rn.ToString());
                if (result == null)
                    return null;
                return result;
        }

        private static unsafe TtTexture LoadPngTexture2DMipLevel(RName rn, TtXndNode node, TtPicDesc desc, int mipLevel)
        {
            if (mipLevel == 0)
                return null;
            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            var pngNode = node.TryGetChildNode("PngMips");
            if (pngNode.NativePointer == IntPtr.Zero)
                return null;

            // 使用 TtTextureHelper 检测旧格式
            bool useLegacyFormat = TtTextureHelper.DetectLegacyFormat(pngNode, desc);

            // 判断是否为 3D Texture
            bool isTexture3D = desc.IsTexture3D && desc.Depth > 0;

            // 旧格式：直接在 PngMips 节点下读取 PngMip0, PngMip1... 属性
            if (useLegacyFormat)
            {
                // 使用 UMipmapResourceGuard 自动管理 GCHandle
                using var guard = new UMipmapResourceGuard(mipLevel);
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

                    StbImageSharp.TtMemImage image;
                    using (var memStream = new System.IO.MemoryStream(data, false))
                    {
                        image = StbImageSharp.TtMemImage.FromStream(memStream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                    }
                    pInitData[i].m_pData = guard.SetData((int)i, image.Data);
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
                CoreSDK.SetMemDebugText(result.mCoreObject, rn.ToString());
                if (result == null)
                    return null;
                return result;
            }

            // 新格式：Cube或3D纹理使用节点结构（PngMips/Face0/PngMip0, PngMips/Face0/DepthSlices/Mip0/...）
            int totalDataUnits = isTexture3D ? mipLevel : (int)desc.CubeFaces * mipLevel;
            using var guardNew = new UMipmapResourceGuard(totalDataUnits);
            var pInitDataNew = stackalloc FMappedSubResource[totalDataUnits];

            StbImageSharp.ColorComponents colorCompNew = StbImageSharp.ColorComponents.RedGreenBlueAlpha;

            for (uint j = 0; j < desc.Desc.CubeFaces; j++)
            {
                var faceNode = pngNode.TryGetChildNode($"Face{j}");
                if (faceNode.IsValidPointer == false)
                {
                    continue;
                }

                if (isTexture3D)
                {
                    // 3D Texture：从 DepthSlices 节点加载
                    var depthSlicesNode = faceNode.TryGetChildNode("DepthSlices");
                    if (depthSlicesNode.IsValidPointer == false)
                        return null;

                    for (uint i = 0; i < mipLevel; i++)
                    {
                        var realLevel = desc.MipLevel - mipLevel + i;
                        var mipNode = depthSlicesNode.TryGetChildNode($"Mip{realLevel}");
                        if (mipNode.IsValidPointer == false)
                            return null;

                        // 计算当前 mipmap level 的 depth
                        int currentDepth = Math.Max(1, desc.Depth >> (int)realLevel);

                        // 收集所有 slice 的数据
                        var allSlicesData = new List<byte[]>();
                        uint totalRowPitch = 0;
                        uint totalDepthPitch = 0;

                        for (int d = 0; d < currentDepth; d++)
                        {
                            var ptr = mipNode.TryGetAttribute($"PngMipSlice{d}");
                            if (ptr.NativePointer == IntPtr.Zero)
                                return null;

                            var mipAttr = ptr;
                            byte[] data;
                            using (var ar = mipAttr.GetReader(null))
                            {
                                ar.ReadNoSize(out data, (int)mipAttr.GetReaderLength());
                            }

                            StbImageSharp.TtMemImage image;
                            using (var memStream = new System.IO.MemoryStream(data, false))
                            {
                                image = StbImageSharp.TtMemImage.FromStream(memStream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                            }
                            allSlicesData.Add(image.Data);
                            colorCompNew = image.Comp;

                            if (d == 0)
                            {
                                // 第一个 slice 的 RowPitch
                                totalRowPitch = (uint)image.Width * 4;
                                totalDepthPitch = totalRowPitch * (uint)image.Height;
                            }
                        }

                        // 将所有 slice 的数据合并成一个连续的缓冲区
                        int totalSize = allSlicesData.Sum(d => d.Length);
                        byte[] mergedData = new byte[totalSize];
                        int offset = 0;
                        foreach (var sliceData in allSlicesData)
                        {
                            Buffer.BlockCopy(sliceData, 0, mergedData, offset, sliceData.Length);
                            offset += sliceData.Length;
                        }

                        int dataIndex = (int)i;
                        pInitDataNew[dataIndex].m_pData = guardNew.SetData(dataIndex, mergedData);
                        pInitDataNew[dataIndex].m_RowPitch = totalRowPitch;
                        pInitDataNew[dataIndex].m_DepthPitch = totalDepthPitch;
                    }
                }
                else
                {
                    // 2D Texture：原有逻辑
                    for (uint i = 0; i < mipLevel; i++)
                    {
                        var realLevel = desc.MipLevel - mipLevel + i;
                        var ptr = faceNode.TryGetAttribute($"PngMip{realLevel}");
                        if (ptr.NativePointer == IntPtr.Zero)
                            return null;
                        var mipAttr = ptr;
                        byte[] data;
                        using (var ar = mipAttr.GetReader(null))
                        {
                            ar.ReadNoSize(out data, (int)mipAttr.GetReaderLength());
                        }

                        StbImageSharp.TtMemImage image;
                        using (var memStream = new System.IO.MemoryStream(data, false))
                        {
                            image = StbImageSharp.TtMemImage.FromStream(memStream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                        }

                        int dataIndex = (int)(j * mipLevel + i);
                        pInitDataNew[dataIndex].m_pData = guardNew.SetData(dataIndex, image.Data);
                        pInitDataNew[dataIndex].m_RowPitch = (uint)image.Width * 4;
                        pInitDataNew[dataIndex].m_DepthPitch = pInitDataNew[dataIndex].m_RowPitch * (uint)image.Height;

                        colorCompNew = image.Comp;
                    }
                }
            }

            var texDescNew = new FTextureDesc();
            texDescNew.SetDefault();
            texDescNew.Width = (uint)desc.MipSizes[desc.MipLevel - mipLevel].X;
            texDescNew.Height = (uint)desc.MipSizes[desc.MipLevel - mipLevel].Y;
            texDescNew.MipLevels = (uint)mipLevel;
            texDescNew.InitData = pInitDataNew;
            switch (colorCompNew)
            {
                case StbImageSharp.ColorComponents.RedGreenBlueAlpha:
                    texDescNew.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
                    break;
            }

            if (isTexture3D)
            {
                // 3D Texture
                texDescNew.Depth = (uint)desc.MipSizes[desc.MipLevel - mipLevel].Z;
                texDescNew.ArraySize = 1;
            }
            else if (desc.CubeFaces == 6)
            {
                // Cube Texture
                texDescNew.ArraySize = 6;
                texDescNew.MiscFlags = EResourceMiscFlag.RM_TEXTURECUBE;
            }

            var resultNew = rc.CreateTexture(in texDescNew);
            CoreSDK.SetMemDebugText(resultNew.mCoreObject, rn.ToString());
            if (resultNew == null)
                return null;
            return resultNew;
        }
        private static unsafe TtTexture LoadDxtTexture2DMipLevel(RName rn, TtXndNode node, TtPicDesc desc, int mipLevel)
        {
            if (mipLevel == 0)
                return null;
            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            var dxtNode = node.TryGetChildNode("DxtMips");
            if (dxtNode.NativePointer == IntPtr.Zero)
                return null;

            // 判断是否为 3D Texture
            bool isTexture3D = desc.IsTexture3D && desc.Depth > 0;

            // 计算数据单元数量
            int totalDataUnits = isTexture3D ? mipLevel : (int)desc.CubeFaces * mipLevel;
            using var guard = new UMipmapResourceGuard(totalDataUnits);
            var pInitData = stackalloc FMappedSubResource[totalDataUnits];

                for (uint j = 0; j < desc.Desc.CubeFaces; j++)
                {
                    var faceNode = dxtNode.TryGetChildNode($"Face{j}");
                    if (faceNode.IsValidPointer == false)
                    {
                        continue;
                    }

                    if (isTexture3D)
                    {
                        // 3D Texture：从 DepthSlices 节点加载
                        var depthSlicesNode = faceNode.TryGetChildNode("DepthSlices");
                        if (depthSlicesNode.IsValidPointer == false)
                            return null;

                        for (uint i = 0; i < mipLevel; i++)
                        {
                            var realLevel = desc.MipLevel - mipLevel + i;
                            var mipNode = depthSlicesNode.TryGetChildNode($"Mip{realLevel}");
                            if (mipNode.IsValidPointer == false)
                                return null;

                            // 计算当前 mipmap level 的 depth
                            int currentDepth = Math.Max(1, desc.Depth >> (int)realLevel);

                            // 收集所有 slice 的数据
                            var allSlicesData = new List<byte[]>();
                            uint totalRowPitch = 0;
                            uint totalDepthPitch = 0;

                            for (int d = 0; d < currentDepth; d++)
                            {
                                var ptr = mipNode.TryGetAttribute($"DxtMipSlice{d}");
                                if (ptr.NativePointer == IntPtr.Zero)
                                    return null;

                                var mipAttr = ptr;
                                byte[] data;
                                using (var ar = mipAttr.GetReader(null))
                                {
                                    ar.ReadNoSize(out data, (int)mipAttr.GetReaderLength());
                                }
                                allSlicesData.Add(data);

                                if (d == 0)
                                {
                                    // 第一个 slice 的 RowPitch
                                    if (desc.BlockSize == 0)
                                    {
                                        totalRowPitch = (uint)desc.MipSizes[(int)realLevel].Z;
                                    }
                                    else
                                    {
                                        var blockWidth = desc.BlockDimenstions[(int)realLevel].X;
                                        totalRowPitch = (uint)(blockWidth * desc.BlockSize);
                                    }
                                    totalDepthPitch = (uint)data.Length;
                                }
                            }

                            // 将所有 slice 的数据合并成一个连续的缓冲区
                            int totalSize = allSlicesData.Sum(d => d.Length);
                            byte[] mergedData = new byte[totalSize];
                            int offset = 0;
                            foreach (var sliceData in allSlicesData)
                            {
                                Buffer.BlockCopy(sliceData, 0, mergedData, offset, sliceData.Length);
                                offset += sliceData.Length;
                            }

                            int dataIndex = (int)i;
                            pInitData[dataIndex].m_pData = guard.SetData(dataIndex, mergedData);
                            pInitData[dataIndex].m_RowPitch = totalRowPitch;
                            pInitData[dataIndex].m_DepthPitch = totalDepthPitch;
                        }
                    }
                    else
                    {
                        // 2D Texture：原有逻辑
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

                            int dataIndex = (int)(j * mipLevel + i);
                            pInitData[dataIndex].m_pData = guard.SetData(dataIndex, data);
                            if (desc.BlockSize==0)
                            {
                                pInitData[dataIndex].m_RowPitch = (uint)desc.MipSizes[(int)realLevel].Z;
                                //pInitData[dataIndex].m_DepthPitch = pInitData[i].m_RowPitch * (uint)desc.MipSizes[(int)realLevel].Y;
                                //System.Diagnostics.Debug.Assert(data.Length>=pInitData[dataIndex].m_DepthPitch);
                                pInitData[dataIndex].m_DepthPitch = (uint)data.Length;
                            }
                            else
                            {
                                if (realLevel > (desc.BlockDimenstions.Count-1))
                                    return null;
                                var blockWidth = desc.BlockDimenstions[(int)realLevel].X;
                                var blockHeight = desc.BlockDimenstions[(int)realLevel].Y;
                                pInitData[dataIndex].m_RowPitch = (uint)(blockWidth * desc.BlockSize);
                                //pInitData[dataIndex].m_DepthPitch = pInitData[dataIndex].m_RowPitch * (uint)blockHeight;
                                //System.Diagnostics.Debug.Assert(data.Length>=pInitData[dataIndex].m_DepthPitch);
                                pInitData[dataIndex].m_DepthPitch = (uint)data.Length;
                            }
                        }
                    }
                }

                var texDesc = new FTextureDesc();
                texDesc.SetDefault();
                texDesc.Width = (uint)desc.MipSizes[desc.MipLevel - mipLevel].X;
                texDesc.Height = (uint)desc.MipSizes[desc.MipLevel - mipLevel].Y;
                texDesc.MipLevels = (uint)mipLevel;
                texDesc.InitData = pInitData;
                texDesc.Format = desc.Desc.Format;

                if (isTexture3D)
                {
                    // 3D Texture
                    texDesc.Depth = (uint)desc.MipSizes[desc.MipLevel - mipLevel].Z;
                    texDesc.ArraySize = 1;
                }
                else if (desc.CubeFaces == 6)
                {
                    // Cube Texture
                    texDesc.ArraySize = 6;
                    texDesc.MiscFlags = EResourceMiscFlag.RM_TEXTURECUBE;
                }

                var result = rc.CreateTexture(in texDesc);
                if(result!=null)
                    CoreSDK.SetMemDebugText(result.mCoreObject, rn.ToString());
                return result;
        
        }

        private static unsafe TtTexture LoadDxtTexture2DMipLevel(RName rn, TtXndNode node, TtPicDesc desc, int mipLevel, TtTexture oldTexture)
        {
            if (oldTexture == null)
            {
                return LoadDxtTexture2DMipLevel(rn, node, desc, mipLevel);
            }

            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            var pngNode = node.TryGetChildNode("DxtMips");
            if (pngNode.NativePointer == IntPtr.Zero)
                return null;

            uint oldLevel = oldTexture.mCoreObject.Desc.MipLevels;
            var texDesc = new FTextureDesc();
            texDesc.SetDefault();
            texDesc.Width = (uint)desc.MipSizes[desc.MipLevel - mipLevel].X;
            texDesc.Height = (uint)desc.MipSizes[desc.MipLevel - mipLevel].Y;
            texDesc.MipLevels = (uint)mipLevel;
            texDesc.Format = desc.Desc.Format;
            if (desc.CubeFaces == 6)
            {
                texDesc.ArraySize = 6;
                texDesc.MiscFlags = EResourceMiscFlag.RM_TEXTURECUBE;
            }

            var result = rc.CreateTexture(in texDesc);

            using (var tsCmd = new NxRHI.FTransientCmd(NxRHI.EQueueType.QU_Transfer, "Texture.LoadMip.CopyOld"))
            {
                var copyNum = Math.Min(mipLevel, oldLevel);
                for (uint j = 0; j < desc.Desc.CubeFaces; j++)
                {
                    for (uint i = 0; i < copyNum; i++)
                    {
                        var cpDraw = rc.CreateCopyDraw();
                        cpDraw.Mode = ECopyDrawMode.CDM_Texture2Texture;
                        cpDraw.BindTextureSrc(oldTexture);
                        cpDraw.BindTextureDest(result);
                        cpDraw.DestSubResource = i;
                        cpDraw.SrcSubResource = i;
                        tsCmd.CmdList.PushGpuDraw(cpDraw.mCoreObject);
                        cpDraw.Dispose();
                    }
                }
            }

            if (oldLevel < mipLevel)
            {
                using (var tsCmd = new NxRHI.FTransientCmd(NxRHI.EQueueType.QU_Transfer, "Texture.LoadMip.CopyNewMip"))
                {
                    for (uint j = 0; j < desc.Desc.CubeFaces; j++)
                    {
                        var faceNode = pngNode.TryGetChildNode($"Face{j}");
                        if (faceNode.IsValidPointer == false)
                        {
                            continue;
                        }
                        for (uint i = oldLevel; i < mipLevel; i++)
                        {
                            var ptr = faceNode.TryGetAttribute($"DxtMip{i}");
                            if (ptr.NativePointer == IntPtr.Zero)
                                continue;
                            var mipAttr = ptr;
                            byte[] data;
                            using (var ar = mipAttr.GetReader(null))
                            {
                                ar.ReadNoSize(out data, (int)mipAttr.GetReaderLength());
                            }

                            var blockWidth = desc.BlockDimenstions[(int)i].X;
                            var blockHeight = desc.BlockDimenstions[(int)i].Y;
                            fixed (byte* p = &data[0])
                            {
                                NxRHI.FTextureDesc tDesc = new FTextureDesc();
                                tDesc.SetDefault();
                                tDesc.Usage = EGpuUsage.USAGE_STAGING;
                                tDesc.CpuAccess = ECpuAccess.CAS_WRITE;
                                tDesc.MipLevels = 1;
                                tDesc.Width = (uint)desc.MipSizes[desc.MipLevel - (int)i].X;
                                tDesc.Height = (uint)desc.MipSizes[desc.MipLevel - (int)i].Y;
                                tDesc.Format = desc.Desc.Format;
                                FMappedSubResource subRes = new FMappedSubResource();
                                subRes.pData = p;
                                if (desc.BlockSize == 0)
                                {
                                    subRes.RowPitch = (uint)desc.MipSizes[(int)i].Z;
                                    subRes.DepthPitch = subRes.RowPitch * (uint)desc.MipSizes[(int)i].Y;
                                }
                                else
                                {
                                    subRes.RowPitch = (uint)(blockWidth * desc.BlockSize);
                                    subRes.DepthPitch = subRes.RowPitch * (uint)blockHeight;
                                }
                                tDesc.InitData = &subRes;
                                var tex = rc.CreateTexture(in tDesc);

                                var cpDraw = rc.CreateCopyDraw();
                                cpDraw.Mode = ECopyDrawMode.CDM_Texture2Texture;
                                cpDraw.BindTextureSrc(tex);
                                cpDraw.BindTextureDest(result);
                                cpDraw.DestSubResource = i;
                                cpDraw.SrcSubResource = 0;
                                tsCmd.CmdList.PushGpuDraw(cpDraw.mCoreObject);
                                cpDraw.Dispose();
                            }
                        }
                    }
                }
            }
            return result;
        }
        #endregion
        static StbImageWriteSharp.ColorComponents ComponentConvert(StbImageSharp.ColorComponents component)
        {
            switch(component)
            {
                case ColorComponents.Grey:
                    return StbImageWriteSharp.ColorComponents.Grey;
                case ColorComponents.GreyAlpha:
                    return StbImageWriteSharp.ColorComponents.GreyAlpha;
                case ColorComponents.RedGreenBlue:
                    return StbImageWriteSharp.ColorComponents.RedGreenBlue;
                case ColorComponents.RedGreenBlueAlpha:
                    return StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha;
            }
            return StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha;
        }
        public static unsafe void SaveOriginImage(RName rn)
        {
            if (System.IO.File.Exists(rn.Address) == false)
                return;

            var imgType = GetOriginImageType(rn);
            switch (imgType)
            {
                case EngineNS.Bricks.ImageDecoder.UImageType.PNG:
                    {
                        var image = LoadOriginPng(rn);
                        if (image == null)
                        {
                            return;
                        }
                        using (var memStream = new System.IO.FileStream(rn.Address + ".png", System.IO.FileMode.OpenOrCreate))
                        {
                            var writer = new StbImageWriteSharp.ImageWriter();
                            writer.WritePng(image.Data, image.Width, image.Height, ComponentConvert(image.Comp), memStream);
                        }
                    }
                    break;
                case EngineNS.Bricks.ImageDecoder.UImageType.HDR:
                    {
                        StbImageSharp.ImageResultFloat imageFloat = new StbImageSharp.ImageResultFloat();
                        LoadOriginHdr(rn, ref imageFloat);
                        using (var memStream = new System.IO.FileStream(rn.Address + ".hdr", System.IO.FileMode.OpenOrCreate))
                        {
                            var writer = new StbImageWriteSharp.ImageWriter();
                            fixed (void* fptr = imageFloat.Data)
                            {
                                writer.WriteHdr(fptr, imageFloat.Width, imageFloat.Height, ComponentConvert(imageFloat.Comp), memStream);
                            }
                        }
                    }
                    break;
                case EngineNS.Bricks.ImageDecoder.UImageType.EXR:
                    {
                        System.IO.Stream outStream = null;
                        var file = LoadOriginExr(rn, ref outStream);
                        if(file!=null)
                        {
                            var part = file.Parts[0];

                            byte[] pixelData = new byte[part.DataReader.GetTotalByteCount()];
                            string[] channelNames = new[] { "R", "G", "B", "A" };
                            if (part.Channels.Count == 3)
                                channelNames = new[] { "R", "G", "B" };

                            part.DataReader.ReadInterleaved(pixelData, channelNames);
                            file.Write(rn.Address + ".exr");
                            part.DataWriter.WriteInterleaved(pixelData, new[] { "R", "G", "B", "A" });
                        }
                    }
                    break;
            }
        }
        public static async Thread.Async.TtTask<TtSrView> LoadSrvMipmap(RName rn, int mipLevel, TtTexture oldTexture)
        {
            TtPicDesc desc = null;
            var tex2d = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                using (var xnd = IO.TtXndHolder.LoadXnd(rn.Address))
                {
                    if (xnd == null)
                        return null;

                    // New format: .srv has "RawSource" node → load from cooked cache
                    if (TtTextureCookManager.HasRawSource(xnd.RootNode))
                    {
                        return TtTextureCookManager.LoadOrCook(rn, xnd.RootNode, mipLevel, out desc);
                    }

                    // Legacy format: .srv has PngMips/DxtMips → load directly
                    desc = TtTextureHelper.LoadPictureDesc(xnd.RootNode);

                    if (mipLevel == -1 || mipLevel > desc.MipLevel)
                        mipLevel = desc.MipLevel;

                    return LoadTexture2DMipLevel(rn, xnd.RootNode, desc, mipLevel, oldTexture);
                }   
            }, Thread.Async.EAsyncTarget.AsyncIO);

            if (tex2d == null)
            {
                Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Warning, $"LoadSrvMipmap {rn} failed");
                if (TtEngine.Instance.PlayMode == EPlayMode.Editor)
                {
                    //SaveOriginImage(rn);
                }
                return null;
            }

            mipLevel = (int)tex2d.mCoreObject.Desc.MipLevels;
            tex2d.SetDebugName("Texture:" + rn.ToString());

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            var srvDesc = new FSrvDesc();
            srvDesc.SetTexture2D();
            if (desc.CubeFaces == 6)
            {
                srvDesc.Type = ESrvType.ST_TextureCube;
                srvDesc.TextureCube.MipLevels = (uint)mipLevel;
            }
            else if (desc.IsTexture3D)
            {
                srvDesc.Type = ESrvType.ST_Texture3D;
                srvDesc.Texture3D.MipLevels = (uint)mipLevel;
            }
            else
            {
                srvDesc.Type = ESrvType.ST_Texture2D;
                srvDesc.Texture2D.MipLevels = (uint)mipLevel;
            }
            srvDesc.Format = tex2d.mCoreObject.Desc.Format;
            
            var result = rc.CreateSRV(tex2d, in srvDesc);
            result.StreamingTexture = tex2d;
            result.PicDesc = desc;
            result.LevelOfDetail = mipLevel;
            result.TargetLOD = mipLevel;
            result.AssetName = rn;

            result.SetDebugName(rn.ToString());
            return result;
        }
        #endregion
    }
    public class TtRenderTargetView : AuxPtrType<NxRHI.IRenderTargetView>
    {
        public TtRenderTargetView(IRenderTargetView ptr)
        {
            mCoreObject = ptr;
            mCoreObject.NativeSuper.AddRef();
        }
        public TtRenderTargetView()
        {

        }
    }
    public class TtDepthStencilView : AuxPtrType<NxRHI.IDepthStencilView>
    {
    }

    public class TtTextureManager : IO.TtStreamingManager, ITickable
    {
        public int GetTickOrder()
        {
            return 0;
        }
        public TtTextureManager()
        {

        }
        ~TtTextureManager()
        {
            Cleanup();
            TtEngine.Instance?.TickableManager.RemoveTickable(this);
        }
        public void Cleanup()
        {
            foreach (var i in StreamingAssets)
            {
                var srv = i.Value as TtSrView;
                if (srv == null)
                    continue;
                srv.Dispose();
            }
            StreamingAssets.Clear();
            CoreSDK.DisposeObject(ref BlackTextureSRV);
            CoreSDK.DisposeObject(ref BlackTexture);

            CoreSDK.DisposeObject(ref WhiteTextureSRV);
            CoreSDK.DisposeObject(ref WhiteTexture);
        }
        public TtSrView DefaultTexture;

        public NxRHI.TtTexture BlackTexture;
        public TtSrView BlackTextureSRV;
        public NxRHI.TtTexture WhiteTexture;
        public TtSrView WhiteTextureSRV;
        public async System.Threading.Tasks.Task Initialize(TtEngine engine)
        {
            DefaultTexture = await GetTexture(engine.Config.DefaultTexture);
            BlackTextureSRV = CreateSolidColorTexture(0, 0, 0, 0, "BlackTexture");
            WhiteTextureSRV = CreateSolidColorTexture(255, 255, 255, 255, "WhiteTexture");
        }
        private unsafe TtSrView CreateSolidColorTexture(byte r, byte g, byte b, byte a, string debugName)
        {
            var texDesc = new FTextureDesc();
            texDesc.SetDefault();
            texDesc.Width = 1;
            texDesc.Height = 1;
            texDesc.MipLevels = 1;
            texDesc.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;

            var pixel = stackalloc byte[4] { r, g, b, a };
            var data = new FMappedSubResource();
            data.pData = pixel;
            data.RowPitch = 4;
            data.DepthPitch = 4;
            texDesc.InitData = &data;

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            BlackTexture = rc.CreateTexture(in texDesc);
            CoreSDK.SetMemDebugText(BlackTexture.mCoreObject, debugName);
            BlackTexture.SetDebugName(debugName);

            var srvDesc = new FSrvDesc();
            srvDesc.SetTexture2D();
            srvDesc.Type = ESrvType.ST_Texture2D;
            srvDesc.Format = texDesc.Format;
            srvDesc.Texture2D.MipLevels = texDesc.MipLevels;
            return rc.CreateSRV(BlackTexture, in srvDesc);
        }
        private Thread.TtAwaitSessionManager<RName, TtSrView> mCreatingSession = new Thread.TtAwaitSessionManager<RName, TtSrView>();
        List<RName> mWaitRemoves = new List<RName>();
        public async Thread.Async.TtTask<TtSrView> CreateTexture(string file)
        {
            if (EngineNS.IO.TtFileManager.FileExists(file) == false)
                return null;
            StbImageSharp.TtMemImage image = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                using (var memStream = new System.IO.FileStream(file, System.IO.FileMode.Open))
                {
                    return StbImageSharp.TtMemImage.FromStream(memStream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                }
            }, Thread.Async.EAsyncTarget.AsyncIO);
            if (image == null)
            {
                return null;
            }

            return CreateTexture(image, file);
        }
        private unsafe TtSrView CreateTexture(StbImageSharp.TtMemImage image, string file)
        {
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
            var data = new FMappedSubResource();
            data.RowPitch = texDesc.m_Width * pixelWidth;
            data.DepthPitch = data.RowPitch * texDesc.Height;
            texDesc.InitData = &data;
            fixed (byte* pData = &image.Data[0])
            {
                data.pData = pData;

                var rc = TtEngine.Instance.GfxDevice.RenderContext;
                var texture2d = rc.CreateTexture(in texDesc);
                CoreSDK.SetMemDebugText(texture2d.mCoreObject, file);
                texture2d.SetDebugName(file);

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
        public async Thread.Async.TtTask<TtSrView> GetOrNewTexture(RName rn, int mipLevel = 1)
        {
            var result = await GetTexture(rn, mipLevel);
            if (result != null)
                return result;

            return await TtSrView.LoadSrvMipmap(rn, mipLevel, null);
        }
        public async Thread.Async.TtTask<TtSrView> GetTexture(RName rn, int mipLevel = 1)
        {
            if (rn == null)
                return null;
            TtSrView srv = null;
            IO.IStreaming result;
            lock (StreamingAssets)
            {
                if (StreamingAssets.TryGetValue(rn, out result))
                {
                    srv = result as TtSrView;
                    if (srv == null)
                        return null;
                    srv.TargetLOD = mipLevel;
                    return srv;
                }
            }

            Thread.TtSemaphore smp;
            var session = mCreatingSession.GetOrNewSession(rn, out smp);
            if (smp != null)
            {
                await smp.Await();
                return session.Result;
            }

            try
            {
                srv = await TtSrView.LoadSrvMipmap(rn, mipLevel, null);
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
                        srv = result as TtSrView;
                    }
                }

                return srv;
            }
            finally
            {
                session.FinishSession(rn, srv);
            }
        }
        public TtSrView TryGetTexture(RName rn)
        {
            if (rn == null)
                return null;
            lock (StreamingAssets)
            {
                TtSrView srv;
                IO.IStreaming result;
                if (StreamingAssets.TryGetValue(rn, out result))
                {
                    srv = result as TtSrView;
                    if (srv == null)
                        return null;
                    return srv;
                }
                return null;
            }
        }
        public unsafe override bool UpdateTargetLOD(IO.IStreaming asset)
        {
            var srv = asset as TtSrView;
            if (srv == null)
                return false;

            var gfxConfig = TtEngine.Instance.GfxDevice.Config;
            // 开启 streaming 时，TargetLOD 已由 RebalanceStreaming 阶段统一决定，这里不再单独调整。
            if (gfxConfig.EnableTextureStreaming)
            {
                RebalanceStreaming();
                return true;
            }

            // Fallback：保留旧的全有/全无逻辑。
            var nowFrame = TtEngine.Instance.CurrentTickFrame;
            var resState = srv.mCoreObject.GetResourceState();
            if (nowFrame - resState->GetAccessFrame() > 15 * (uint)TtEngine.Instance.Config.TargetFps)//15 second & 60 target fps
            {
                if (srv.TargetLOD != 1)
                    srv.TargetLOD = 1;
                //mWaitRemoves.Add(asset.AssetName);
                return true;
            }
            else
            {
                if (srv.TargetLOD != srv.MaxLOD)
                    srv.TargetLOD = srv.MaxLOD;
                return true;
            }
        }

        #region TextureStreaming
        /// <summary>当前预算上限（字节）。</summary>
        public long PoolBudgetBytes { get; private set; }
        /// <summary>所有纹理当前已驻留总字节。</summary>
        public long ResidentBytes { get; private set; }
        /// <summary>预算裁剪后的期望总字节。</summary>
        public long WantedBytes { get; private set; }
        /// <summary>被预算压制而降级的纹理数量。</summary>
        public int NumThrottled { get; private set; }
        /// <summary>本轮命中屏幕需求（可见）的纹理数量。</summary>
        public int NumScreenVisible { get; private set; }

        // 屏幕尺寸模型：由 CpuCullingNode 在裁剪后推送每张纹理的屏幕 texel 需求（多 mesh/多 view 取 max）。
        private struct FScreenDemand
        {
            public float WantedTexels;
            public ulong Frame;
        }
        private readonly Dictionary<RName, FScreenDemand> mScreenDemands = new Dictionary<RName, FScreenDemand>();
        // 复用缓存：收集待清理的陈旧屏幕需求 key，避免遍历字典时修改与每次分配。
        private readonly List<RName> mScreenDemandStaleKeys = new List<RName>();

        /// <summary>
        /// 由裁剪节点（渲染线程）累积某张纹理本帧的屏幕 texel 需求。同帧多次累积取 max（覆盖多 mesh/多 view）。
        /// </summary>
        public void AccumulateScreenDemand(RName texName, float wantedTexels, ulong frame)
        {
            if (texName == null)
                return;
            lock (mScreenDemands)
            {
                if (mScreenDemands.TryGetValue(texName, out var d) && d.Frame == frame)
                {
                    if (wantedTexels > d.WantedTexels)
                    {
                        d.WantedTexels = wantedTexels;
                    }
                }
                else
                {
                    d.WantedTexels = wantedTexels;
                    d.Frame = frame;
                }
                mScreenDemands[texName] = d;
            }
        }

        /// <summary>
        /// 将屏幕需求的 texel 数反推为 wanted lod（lod=mip 数量，越大分辨率越高，合法 [clampedMin, maxLod]）。
        /// </summary>
        private int ScreenTexelsToLod(float wantedTexels, int baseWidth, int maxLod, int clampedMin)
        {
            int wanted;
            if (baseWidth <= 0 || wantedTexels >= baseWidth)
            {
                wanted = maxLod;
            }
            else
            {
                int dropped = (int)System.Math.Floor(System.Math.Log2(baseWidth / System.Math.Max(1.0f, wantedTexels)));
                if (dropped < 0)
                    dropped = 0;
                if (dropped > maxLod - 1)
                    dropped = maxLod - 1;
                wanted = maxLod - dropped;
            }
            if (wanted < clampedMin)
                wanted = clampedMin;
            if (wanted > maxLod)
                wanted = maxLod;
            return wanted;
        }

        // RebalanceStreaming 的临时工作项（Wanted/Priority 仅在调度期间有效，不污染 TtSrView）。
        private sealed class FStreamingWork
        {
            public TtSrView Srv;
            public int Wanted;
            public int InitialWanted;
            public float Priority;
        }
        private readonly List<FStreamingWork> mStreamingScratch = new List<FStreamingWork>();

        /// <summary>
        /// 阶段 A：为每张纹理计算期望 LOD，然后在全局内存预算约束下抢占式降级，
        /// 最后写入每个 srv.TargetLOD。参考 UE FRenderAssetStreamingManager。
        /// </summary>
        public unsafe void RebalanceStreaming()
        {
            var gfxConfig = TtEngine.Instance.GfxDevice.Config;
            var nowFrame = TtEngine.Instance.CurrentTickFrame;
            var targetFps = (uint)System.Math.Max(1, TtEngine.Instance.Config.TargetFps);
            ulong coldFrames = (ulong)System.Math.Max(1.0f, gfxConfig.TextureStreamingHysteresisSeconds) * targetFps;
            int minResidentLod = System.Math.Max(1, gfxConfig.TextureStreamingMinResidentLOD);
            bool useScreenSpace = gfxConfig.TextureStreamingScreenSpace;
            ulong visibilityFrames = (ulong)System.Math.Max(1, gfxConfig.TextureStreamingVisibilityFrames);
            int screenVisibleCount = 0;

            mStreamingScratch.Clear();
            long wantedTotal = 0;
            long residentTotal = 0;

            lock (StreamingAssets)
            {
                foreach (var i in StreamingAssets.Values)
                {
                    var srv = i as TtSrView;
                    if (srv == null)
                        continue;

                    var maxLod = srv.MaxLOD;
                    if (maxLod <= 0)
                        continue;

                    int clampedMin = System.Math.Min(minResidentLod, maxLod);

                    var resState = srv.mCoreObject.GetResourceState();
                    ulong lastAccess = resState->GetAccessFrame();
                    ulong idle = nowFrame >= lastAccess ? nowFrame - lastAccess : 0;
                    bool isHot = idle <= coldFrames;
                    // 优先级：越近被访问 -> 越高（越不易被降级）。
                    float recency = 1.0f / (1.0f + idle);

                    int wanted;
                    float priority;
                    if (useScreenSpace)
                    {
                        // 屏幕优先：本周期可见的纹理用屏幕需求反推 wanted；不可见的沿用冷热兜底。
                        FScreenDemand demand = default;
                        bool screenVisible = false;
                        var an = srv.AssetName;
                        if (an != null)
                        {
                            lock (mScreenDemands)
                            {
                                if (mScreenDemands.TryGetValue(an, out demand) && nowFrame - demand.Frame <= visibilityFrames)
                                    screenVisible = true;
                            }
                        }

                        if (screenVisible)
                        {
                            wanted = ScreenTexelsToLod(demand.WantedTexels, srv.Width, maxLod, clampedMin);
                            // 可见项加基数，优先级高于不可见项。
                            priority = 1000.0f + recency * 1000.0f + wanted;
                            screenVisibleCount++;
                        }
                        else
                        {
                            wanted = isHot ? maxLod : clampedMin;
                            priority = recency * 1000.0f + wanted;
                        }
                    }
                    else
                    {
                        // 热：希望升到物理全分辨率 MaxLOD；冷：降到保底常驻。
                        wanted = isHot ? maxLod : clampedMin;
                        priority = recency * 1000.0f + wanted;
                    }

                    var work = new FStreamingWork
                    {
                        Srv = srv,
                        Wanted = wanted,
                        InitialWanted = wanted,
                        Priority = priority,
                    };

                    wantedTotal += srv.GetStreamingBytesForLOD(wanted);
                    residentTotal += srv.GetStreamingBytesForLOD(srv.LevelOfDetail);
                    mStreamingScratch.Add(work);
                }
            }

            long budget = (long)(gfxConfig.TextureStreamingPoolMB * 1024L * 1024L * gfxConfig.TextureStreamingPoolTargetRatio);
            PoolBudgetBytes = budget;
            ResidentBytes = residentTotal;

            int throttled = 0;
            long finalTotal = wantedTotal;
            if (wantedTotal > budget && budget > 0)
            {
                // 按优先级升序（低优先级先被降级）。
                mStreamingScratch.Sort((a, b) => a.Priority.CompareTo(b.Priority));
                // 反复逐级下调 Wanted，直到总量不超预算或无可再降。
                while (finalTotal > budget)
                {
                    bool anyReduced = false;
                    for (int n = 0; n < mStreamingScratch.Count; n++)
                    {
                        var work = mStreamingScratch[n];
                        int clampedMin = System.Math.Min(minResidentLod, work.Srv.MaxLOD);
                        if (work.Wanted > clampedMin)
                        {
                            long before = work.Srv.GetStreamingBytesForLOD(work.Wanted);
                            work.Wanted--;
                            long after = work.Srv.GetStreamingBytesForLOD(work.Wanted);
                            finalTotal -= (before - after);
                            anyReduced = true;
                            if (finalTotal <= budget)
                                break;
                        }
                    }
                    if (!anyReduced)
                        break;
                }
            }

            // Apply：写回 TargetLOD。throttled 仅统计因预算被进一步削减的纹理。
            for (int n = 0; n < mStreamingScratch.Count; n++)
            {
                var work = mStreamingScratch[n];
                if (work.Srv.TargetLOD != work.Wanted)
                    work.Srv.TargetLOD = work.Wanted;
                if (work.Wanted < work.InitialWanted)
                    throttled++;
            }

            WantedBytes = finalTotal;
            NumThrottled = throttled;
            NumScreenVisible = screenVisibleCount;
            mStreamingScratch.Clear();

            // 清理长期不可见的屏幕需求条目，防止字典随场景累积无界增长。
            // 阈值取可见窗口的若干倍，给视角回摆留缓冲，避免误删马上又可见的项。
            if (useScreenSpace)
            {
                ulong staleFrames = visibilityFrames * 8;
                lock (mScreenDemands)
                {
                    if (mScreenDemands.Count > 0)
                    {
                        mScreenDemandStaleKeys.Clear();
                        foreach (var kv in mScreenDemands)
                        {
                            if (nowFrame >= kv.Value.Frame && nowFrame - kv.Value.Frame > staleFrames)
                                mScreenDemandStaleKeys.Add(kv.Key);
                        }
                        for (int n = 0; n < mScreenDemandStaleKeys.Count; n++)
                            mScreenDemands.Remove(mScreenDemandStaleKeys[n]);
                        mScreenDemandStaleKeys.Clear();
                    }
                }
            }
        }
        #endregion

        float EllapsedRemainTime = 150;
        [ThreadStatic]
        private static Profiler.TimeScope mScopeTick;
        private static Profiler.TimeScope ScopeTick
        {
            get
            {
                if (mScopeTick == null)
                    mScopeTick = new Profiler.TimeScope(typeof(TtTextureManager), nameof(TickLogic));
                return mScopeTick;
            }
        }
        public void TickLogic(float ellapse)
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                EllapsedRemainTime -= ellapse;
                if (EllapsedRemainTime <= 0)
                {
                    UpdateStreamingState();
                    EllapsedRemainTime = TtEngine.Instance.GfxDevice.Config.TextureStreamingUpdateIntervalMS;
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