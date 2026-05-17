using EngineNS.Support;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui.Slate
{
    public class TtBaseRenderer
    {
        public Graphics.Pipeline.Shader.TtGraphicsEffect SlateEffect;

        public NxRHI.TtInputLayout InputLayout { get; private set; }
        public NxRHI.TtSampler SamplerState;
        public NxRHI.TtTexture FontTexture;
        public NxRHI.TtSrView FontSRV;
        public async Thread.Async.TtTask Initialize()
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            SlateEffect = await TtEngine.Instance.GfxDevice.EffectManager.GetGraphicEffect(
                await Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<Graphics.Pipeline.Shader.CommanShading.USlateGUIShading>(),
                TtEngine.Instance.GfxDevice.MaterialManager.ScreenMaterial, new Graphics.Mesh.TtMdfStaticMesh());

            var iptDesc = new NxRHI.TtInputLayoutDesc();
            unsafe
            {
                iptDesc.mCoreObject.AddElement("POSITION", 0, EPixelFormat.PXF_R32G32_FLOAT, 0, 0, 0, 0);
                iptDesc.mCoreObject.AddElement("TEXCOORD", 0, EPixelFormat.PXF_R32G32_FLOAT, 0, (uint)sizeof(Vector2), 0, 0);
                iptDesc.mCoreObject.AddElement("COLOR", 0, EPixelFormat.PXF_R8G8B8A8_UNORM, 0, (uint)sizeof(Vector2) * 2, 0, 0);
                //iptDesc.SetShaderDesc(SlateEffect.GraphicsEffect);
            }
            iptDesc.SetShaderDesc(SlateEffect.DescVS);
            InputLayout = TtEngine.Instance.GfxDevice.RenderContext.CreateInputLayout(iptDesc); //TtEngine.Instance.GfxDevice.InputLayoutManager.GetPipelineState(rc, iptDesc);

            SlateEffect.ShaderEffect.BindInputLayout(InputLayout);

            var splDesc = new NxRHI.FSamplerDesc();
            splDesc.SetDefault();
            splDesc.Filter = NxRHI.ESamplerFilter.SPF_MIN_MAG_MIP_LINEAR;
            splDesc.AddressU = NxRHI.EAddressMode.ADM_WRAP;
            splDesc.AddressV = NxRHI.EAddressMode.ADM_WRAP;
            splDesc.AddressW = NxRHI.EAddressMode.ADM_WRAP;
            splDesc.MipLODBias = 0;
            splDesc.MaxAnisotropy = 0;
            splDesc.CmpMode = NxRHI.EComparisionMode.CMP_ALWAYS;
            SamplerState = TtEngine.Instance.GfxDevice.SamplerStateManager.GetPipelineState(rc, in splDesc);
        }
        public void Cleanup()
        {
            SamplerState = null;
            ReleaseFontTexture();

            for(int i=0; i< mFontDataList.Count; ++i)
            {
                mFontDataList[i].Dispose();
                mFontDataList[i].FontSRV = null;
                mFontDataList[i].FontTexture = null;
            }
            mFontDataList.Clear();

            mFontGlyphRanges.Dispose();
            mFontGlyphRanges = default;
            mIconGlyphRanges.Dispose();
            mIconGlyphRanges = default;
            TtImDrawDataRHI.DisposeAllImGuiTextureBindings();
        }

        public enum enFont
        {
            Font_15px        = 0,
            Font_Bold_13px,
            Font_13px,
            Font_Icon,
        }
        const uint ImGuiFreeTypeLoaderFlags_LightHinting = 1u << 3;
        const float TextRasterizerMultiply = 1.05f;

        [System.Runtime.InteropServices.DllImport(EngineNS.CoreSDK.CoreModule, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        static extern int TitanImGui_UseFreeTypeFontLoader(IntPtr fontAtlas);

        class FontDatas : IDisposable
        {
            ~FontDatas()
            {
                Dispose();
                FontTexture = null;
                mFontSRV = null;
            }
            public void Dispose()
            {
                if (SRCGCHandle != IntPtr.Zero)
                {
                    System.Runtime.InteropServices.GCHandle.FromIntPtr(SRCGCHandle).Free();
                    mSRCGCHandle = IntPtr.Zero;
                }
            }
            public ImFont Font;
            public NxRHI.TtTexture FontTexture;
            NxRHI.TtSrView mFontSRV;
            public NxRHI.TtSrView FontSRV
            {
                get => mFontSRV;
                set
                {
                    Dispose();
                    mFontSRV = value;
                    if (mFontSRV != null)
                    {
                        mSRCGCHandle = System.Runtime.InteropServices.GCHandle.ToIntPtr(System.Runtime.InteropServices.GCHandle.Alloc(mFontSRV));
                    }
                }
            }
            IntPtr mSRCGCHandle;
            public IntPtr SRCGCHandle
            {
                get => mSRCGCHandle;
            }
        }
        List<FontDatas> mFontDataList = new List<FontDatas>();
        Support.TtNativeArray<Wchar16> mFontGlyphRanges;
        Support.TtNativeArray<Wchar16> mIconGlyphRanges;

        public unsafe void RecreateFontDeviceTexture()
        {
            //var io = ImGuiAPI.GetIO();
            //ImFontConfig fontConfig = new ImFontConfig();
            //fontConfig.UnsafeCallConstructor();
            //fontConfig.MergeMode = true;
            ////io.FontsWrapper.AddFontDefault(ref mFontConfig);
            //Font_15px = io.Fonts.AddFontFromFileTTF(TtEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Engine) + "fonts/Roboto-Regular.ttf", 15.0f, (ImFontConfig*)0, io.Fonts.GetGlyphRangesDefault());
            //Font_Bold_13px = io.Fonts.AddFontFromFileTTF(TtEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Engine) + "fonts/Roboto-Bold.ttf", 13.0f, &fontConfig, io.Fonts.GetGlyphRangesDefault());
            //Font_13px = io.Fonts.AddFontFromFileTTF(TtEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Engine) + "fonts/Roboto-Regular.ttf", 13.0f, &fontConfig, io.Fonts.GetGlyphRangesDefault());
            //// Build
            //byte* pixels;
            //int width = 0, height = 0, bytesPerPixel = 0;
            //io.Fonts.GetTexDataAsRGBA32(&pixels, ref width, ref height, ref bytesPerPixel);
            //// Store our identifier
            //io.Fonts.SetTexID((void*)0);

            //ImageInitData initData;
            //initData.pSysMem = pixels;
            //initData.SysMemPitch = (uint)(width * bytesPerPixel);

            //var rc = TtEngine.Instance.GfxDevice.RenderContext;
            //var txDesc = new NxRHI.FTextureDesc();
            //txDesc.SetDefault();
            //txDesc.Width = (uint)width;
            //txDesc.Height = (uint)height;
            //txDesc.MipLevels = 1;
            //txDesc.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
            //txDesc.InitData = &initData;
            //FontTexture = rc.CreateTexture2D(ref txDesc);

            //var srvDesc = new IShaderResourceViewDesc();
            //srvDesc.mFormat = txDesc.Format;
            //srvDesc.m_pTexture2D = FontTexture.mCoreObject.Ptr;
            //FontSRV = rc.CreateShaderResourceView(ref srvDesc);

            //io.Fonts.ClearTexData();
            var ModuleStart = Support.TtTime.HighPrecision_GetTickCount();

            ReleaseFontTexture();
            for (int i = 0; i < mFontDataList.Count; ++i)
            {
                mFontDataList[i].Dispose();
            }
            mFontDataList.Clear();

            var io = ImGuiAPI.GetIO();
            var rendererHasTextures = (io.BackendFlags & ImGuiBackendFlags_.ImGuiBackendFlags_RendererHasTextures) != 0;
            var fontAtlas = io.Fonts;
            fontAtlas.Clear();
            fontAtlas.RendererHasTextures = rendererHasTextures;

            var isFreeTypeLoader = TitanImGui_UseFreeTypeFontLoader(fontAtlas.NativePointer) != 0;
            fontAtlas.FontLoaderFlags = ImGuiFreeTypeLoaderFlags_LightHinting;

            var style = ImGuiAPI.GetStyle();
            style->FontSizeBase = 15.0f;
            style->FontScaleMain = 1.0f;
            if (style->FontScaleDpi <= 0.0f)
                style->FontScaleDpi = 1.0f;

            var textRanges = RebuildTextGlyphRanges();
            var iconRanges = RebuildIconGlyphRanges();
            var editorFont = ResolveFont(TtEngine.Instance.Config.EditorFont, "fonts/NotoSansSC-Regular.otf");
            var smallFont = ResolveFont(TtEngine.Instance.Config.EditorSmallFont, "fonts/Roboto-Regular.ttf");
            var boldFont = ResolveFont(null, "fonts/Roboto-Bold.ttf");
            var iconFont = ResolveFont(TtEngine.Instance.Config.EditorEffectFont, "fonts/fa-solid-900.ttf");

            var defaultFont = AddDefaultVectorFont(fontAtlas, 15.0f);
            MergeFontInto(fontAtlas, defaultFont, editorFont, 15.0f, textRanges);
            MergeFontInto(fontAtlas, defaultFont, iconFont, 15.0f, iconRanges, true);
            mFontDataList.Add(new FontDatas() { Font = defaultFont });

            var boldFontSlot = CreateFontSlot(fontAtlas, boldFont, 13.0f, textRanges);
            MergeFontInto(fontAtlas, boldFontSlot, editorFont, 13.0f, textRanges);

            var smallFontSlot = CreateFontSlot(fontAtlas, smallFont, 13.0f, textRanges);
            MergeFontInto(fontAtlas, smallFontSlot, editorFont, 13.0f, textRanges);

            CreateFontSlot(fontAtlas, iconFont, 18.0f, iconRanges);

            if (rendererHasTextures == false)
            {
                CreateFontTexture(out FontSRV, out FontTexture);
                if (mFontDataList.Count > 0)
                {
                    mFontDataList[0].FontTexture = FontTexture;
                    mFontDataList[0].FontSRV = FontSRV;
                    fontAtlas.SetTexID((ulong)mFontDataList[0].SRCGCHandle);
                }
            }

            Profiler.Log.WriteLine<Profiler.TtCoreGategory>(
                Profiler.ELogTag.Info,
                $"ImGui font loader:{fontAtlas.FontLoaderName}, FreeType={isFreeTypeLoader}, Flags=0x{fontAtlas.FontLoaderFlags:X}, VectorDefault={defaultFont.IsValidPointer}, RendererHasTextures={rendererHasTextures}");

            var ModuleEnd = Support.TtTime.HighPrecision_GetTickCount();
            Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, $"RecreateFontDeviceTexture:{(ModuleEnd - ModuleStart) / 1000} ms");
        }

        private unsafe Wchar16* RebuildTextGlyphRanges()
        {
            mFontGlyphRanges.Dispose();
            mFontGlyphRanges = TtNativeArray<Wchar16>.CreateInstance();

            mFontGlyphRanges.Add(new Wchar16(0x0020));
            mFontGlyphRanges.Add(new Wchar16(0x00FF));
            mFontGlyphRanges.Add(new Wchar16(0x2000));
            mFontGlyphRanges.Add(new Wchar16(0x206F));
            mFontGlyphRanges.Add(new Wchar16(0x3000));
            mFontGlyphRanges.Add(new Wchar16(0x30FF));
            mFontGlyphRanges.Add(new Wchar16(0x31F0));
            mFontGlyphRanges.Add(new Wchar16(0x31FF));
            mFontGlyphRanges.Add(new Wchar16(0xFF00));
            mFontGlyphRanges.Add(new Wchar16(0xFFEF));
            mFontGlyphRanges.Add(new Wchar16(0x4e00));
            mFontGlyphRanges.Add(new Wchar16(0x9FAF));
            mFontGlyphRanges.Add(new Wchar16(0));
            return mFontGlyphRanges.UnsafeGetElementAddress(0);
        }

        private unsafe Wchar16* RebuildIconGlyphRanges()
        {
            mIconGlyphRanges.Dispose();
            mIconGlyphRanges = TtNativeArray<Wchar16>.CreateInstance();
            mIconGlyphRanges.Add(new Wchar16(0xe005));
            mIconGlyphRanges.Add(new Wchar16(0xf8ff));
            mIconGlyphRanges.Add(new Wchar16(0));
            return mIconGlyphRanges.UnsafeGetElementAddress(0);
        }

        private RName ResolveFont(RName preferred, string fallback)
        {
            if (preferred != null && string.IsNullOrWhiteSpace(preferred.Address) == false && IO.TtFileManager.FileExists(preferred.Address))
                return preferred;
            return RName.GetRName(fallback, RName.ERNameType.Engine);
        }

        private static unsafe ImFontConfig CreateImGuiFontConfig(float sizePixels, float rasterizerMultiply)
        {
            var fontConfig = new ImFontConfig();
            fontConfig.UnsafeCallConstructor();
            fontConfig.SizePixels = sizePixels;
            fontConfig.PixelSnapH = false;
            fontConfig.FontLoaderFlags = ImGuiFreeTypeLoaderFlags_LightHinting;
            fontConfig.RasterizerMultiply = rasterizerMultiply;
            fontConfig.RasterizerDensity = 1.0f;
            return fontConfig;
        }

        private unsafe ImFont AddDefaultVectorFont(ImFontAtlas fontAtlas, float sizePixels)
        {
            var fontConfig = CreateImGuiFontConfig(sizePixels, TextRasterizerMultiply);
            var font = fontAtlas.AddFontDefaultVector(&fontConfig);
            if (font.IsValidPointer == false)
                font = fontAtlas.AddFontDefault(&fontConfig);
            return font;
        }

        private unsafe ImFont CreateFontSlot(ImFontAtlas fontAtlas, RName rn, float sizePixels, Wchar16* glyphRanges)
        {
            var fontData = new FontDatas();
            fontData.Font = AddFontFromFileOrDefault(fontAtlas, rn, sizePixels, glyphRanges);

            mFontDataList.Add(fontData);
            return fontData.Font;
        }

        private unsafe ImFont AddFontFromFileOrDefault(ImFontAtlas fontAtlas, RName rn, float sizePixels, Wchar16* glyphRanges)
        {
            var fontFile = rn?.Address;
            if (string.IsNullOrWhiteSpace(fontFile) || IO.TtFileManager.FileExists(fontFile) == false)
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(
                    Profiler.ELogTag.Warning,
                    $"ImGui font asset is missing: {rn}");
                return AddDefaultVectorFont(fontAtlas, sizePixels);
            }

            var fontConfig = CreateImGuiFontConfig(sizePixels, TextRasterizerMultiply);
            var font = fontAtlas.AddFontFromFileTTF(fontFile, sizePixels, &fontConfig, glyphRanges);
            if (font.IsValidPointer)
                return font;

            Profiler.Log.WriteLine<Profiler.TtCoreGategory>(
                Profiler.ELogTag.Warning,
                $"ImGui failed to load font: {fontFile}");
            return AddDefaultVectorFont(fontAtlas, sizePixels);
        }

        private unsafe void MergeFontInto(ImFontAtlas fontAtlas, ImFont dstFont, RName rn, float sizePixels, Wchar16* glyphRanges, bool iconFont = false)
        {
            if (dstFont.IsValidPointer == false)
                return;

            var fontFile = rn?.Address;
            if (string.IsNullOrWhiteSpace(fontFile) || IO.TtFileManager.FileExists(fontFile) == false)
                return;

            var fontConfig = CreateImGuiFontConfig(sizePixels, iconFont ? 1.0f : TextRasterizerMultiply);
            fontConfig.MergeMode = true;
            fontConfig.DstFont = dstFont;
            if (iconFont)
            {
                fontConfig.PixelSnapH = true;
                fontConfig.GlyphMinAdvanceX = sizePixels;
                fontConfig.GlyphMaxAdvanceX = sizePixels;
            }
            fontAtlas.AddFontFromFileTTF(fontFile, sizePixels, &fontConfig, glyphRanges);
        }

        private void ReleaseFontTexture()
        {
            CoreSDK.DisposeObject(ref FontSRV);
            CoreSDK.DisposeObject(ref FontTexture);
        }

        unsafe void CreateFontTexture(out NxRHI.TtSrView srv, out NxRHI.TtTexture tex)
        {
            var io = ImGuiAPI.GetIO();
            int width = 0, height = 0, bytesPerPixel = 0;
            byte* pixels;
            io.Fonts.GetTexDataAsRGBA32(&pixels, ref width, ref height, ref bytesPerPixel);
            var initData = new NxRHI.FMappedSubResource();
            initData.pData = pixels;
            initData.RowPitch = (uint)(width * bytesPerPixel);
            initData.DepthPitch = (uint)(initData.RowPitch * height);

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            var txDesc = new NxRHI.FTextureDesc();
            txDesc.SetDefault();
            txDesc.Width = (uint)width;
            txDesc.Height = (uint)height;
            txDesc.MipLevels = 1;
            txDesc.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
            txDesc.InitData = &initData;
            tex = rc.CreateTexture(in txDesc);

            var srvDesc = new NxRHI.FSrvDesc();
            srvDesc.SetTexture2D();
            srvDesc.Type = NxRHI.ESrvType.ST_Texture2D;
            srvDesc.Format = txDesc.Format;
            srvDesc.Texture2D.MipLevels = 1;
            srv = rc.CreateSRV(tex, in srvDesc);

            io.Fonts.ClearTexData();
        }

        public unsafe void PushFont(int fontIdx)
        {
            if (fontIdx < 0 || (int)fontIdx >= mFontDataList.Count)
                return;

            if (mFontDataList[(int)fontIdx] == null)
                return;

            ImGuiAPI.PushFont(mFontDataList[(int)fontIdx].Font);
        }
        public void PopFont()
        {
            ImGuiAPI.PopFont();
        }
    }
}
