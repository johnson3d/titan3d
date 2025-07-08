using EngineNS.Support;
using NPOI.SS.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui.Slate
{
    public class UBaseRenderer
    {
        public Graphics.Pipeline.Shader.TtEffect SlateEffect;

        public NxRHI.TtInputLayout InputLayout { get; private set; }
        public NxRHI.TtSampler SamplerState;
        public NxRHI.TtTexture FontTexture;
        public NxRHI.TtSrView FontSRV;
        public async System.Threading.Tasks.Task Initialize()
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            SlateEffect = await TtEngine.Instance.GfxDevice.EffectManager.GetGraphicEffect(
                await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<Graphics.Pipeline.Shader.CommanShading.USlateGUIShading>(),
                TtEngine.Instance.GfxDevice.MaterialManager.ScreenMaterial, new Graphics.Mesh.TtMdfStaticMesh());

            var iptDesc = new NxRHI.TtInputLayoutDesc();
            unsafe
            {
                iptDesc.mCoreObject.AddElement("POSITION", 0, EPixelFormat.PXF_R32G32_FLOAT, 0, 0, 0, 0);
                iptDesc.mCoreObject.AddElement("TEXCOORD", 0, EPixelFormat.PXF_R32G32_FLOAT, 0, (uint)sizeof(Vector2), 0, 0);
                iptDesc.mCoreObject.AddElement("COLOR", 0, EPixelFormat.PXF_R8G8B8A8_UNORM, 0, (uint)sizeof(Vector2) * 2, 0, 0);
                //iptDesc.SetShaderDesc(SlateEffect.GraphicsEffect);
            }
            iptDesc.mCoreObject.SetShaderDesc(SlateEffect.DescVS.mCoreObject);
            InputLayout = TtEngine.Instance.GfxDevice.RenderContext.CreateInputLayout(iptDesc); //TtEngine.Instance.GfxDevice.InputLayoutManager.GetPipelineState(rc, iptDesc);

            SlateEffect.ShaderEffect.mCoreObject.BindInputLayout(InputLayout.mCoreObject);

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
            FontSRV?.Dispose();
            FontSRV = null;
            FontTexture?.Dispose();
            FontTexture = null;

            for(int i=0; i< mFontDataList.Count; ++i)
            {
                mFontDataList[i].Dispose();
                mFontDataList[i].FontSRV?.Dispose();
                mFontDataList[i].FontSRV = null;
                mFontDataList[i].FontTexture?.Dispose();
                mFontDataList[i].FontTexture = null;
            }
            mFontDataList.Clear();
        }

        public enum enFont
        {
            Font_15px        = 0,
            Font_Bold_13px,
            Font_13px,
            Font_Icon,
        }

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
            using (var ranges = TtNativeArray<Wchar16>.CreateInstance())
            {
                // Basic Latin + Latin Supplement
                ranges.Add(new Wchar16(0x0020));
                ranges.Add(new Wchar16(0x00FF));
                // General Punctuation
                ranges.Add(new Wchar16(0x2000));
                ranges.Add(new Wchar16(0x206F));
                // CJK Symbols and Punctuations, Hiragana, Katakana
                ranges.Add(new Wchar16(0x3000));
                ranges.Add(new Wchar16(0x30FF));
                // Katakana Phonetic Extensions
                ranges.Add(new Wchar16(0x31F0));
                ranges.Add(new Wchar16(0x31FF));
                // Half-width characters
                ranges.Add(new Wchar16(0xFF00));
                ranges.Add(new Wchar16(0xFFEF));
                // CJK Ideograms
                //switch (TtEngine.Instance.Config.EditorLanguage)
                //{
                //    case "Chinese":
                        {
                            ranges.Add(new Wchar16(0x4e00));
                            ranges.Add(new Wchar16(0x9FAF));
                        }
                //        break;
                //}
                //push end
                ranges.Add(new Wchar16(0));

                var io = ImGuiAPI.GetIO();
                CreateFont(TtEngine.Instance.Config.EditorFont, 20.0f, (ImFontConfig*)0, ranges.UnsafeGetElementAddress(0));

                CreateFont(TtEngine.Instance.Config.EditorSmallFont, 15.0f, (ImFontConfig*)0, io.Fonts.GetGlyphRangesDefault());

                CreateFont(TtEngine.Instance.Config.EditorFont, 15.0f, (ImFontConfig*)0, ranges.UnsafeGetElementAddress(0));

                var iconRange = stackalloc Wchar16[3];
                iconRange[0] = new Wchar16(0xe005);
                iconRange[1] = new Wchar16(0xf8ff);
                iconRange[2] = new Wchar16(0);
                CreateFont(TtEngine.Instance.Config.EditorEffectFont, 20.0f, (ImFontConfig*)0, iconRange);

                NxRHI.TtSrView srv;
                NxRHI.TtTexture tex;
                CreateFontTexture(out srv, out tex);
                foreach(var i in mFontDataList)
                {
                    i.FontTexture = tex;
                    i.FontSRV = srv;
                    io.Fonts.SetTexID((ulong)i.SRCGCHandle);
                }
            }

            var ModuleEnd = Support.TtTime.HighPrecision_GetTickCount();
            Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, $"RecreateFontDeviceTexture:{(ModuleEnd - ModuleStart) / 1000} ms");
        }

        unsafe void CreateFont(RName rn, float size_pixels, ImFontConfig* fontConfig, Wchar16* glyph_ranges)
        {
            var io = ImGuiAPI.GetIO();

            var fontData = new FontDatas();
            fontData.Font = io.Fonts.AddFontFromFileTTF(rn.Address, size_pixels, fontConfig, glyph_ranges);

            mFontDataList.Add(fontData);
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
