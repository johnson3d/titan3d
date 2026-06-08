using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using BCnEncoder.Shared.ImageFiles;
using CommunityToolkit.HighPerformance;
using EngineNS.Bricks.ImageDecoder;
using EngineNS.IO;
using Mono.CompilerServices.SymbolWriter;
using StbImageSharp;
using StbImageWriteSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace EngineNS.NxRHI
{
    public class TtTextureUtility
    {
        public class TtTex3dLayer
        {
            public Color4f[] Pixels;
            public int Width;
            public int Height;
            public int Depth;
            public unsafe NxRHI.TtTexture CreateTexture3D(EPixelFormat format = EPixelFormat.PXF_R8G8B8A8_UNORM, int MaxLayer = 1)
            {
                var sourceLayer = this;
                List<NxRHI.TtTextureUtility.TtTex3dLayer> mipDatas = new List<NxRHI.TtTextureUtility.TtTex3dLayer>();
                mipDatas.Add(sourceLayer);
                int w = Width/2;
                int h = Height/2;
                int d = Depth/2;
                while (w>=1 && h>=1 && d>=1)
                {
                    if (mipDatas.Count>=MaxLayer)
                        break;
                    var next = NxRHI.TtTextureUtility.GenerateMipLayer3D(sourceLayer, w, h, d);
                    mipDatas.Add(next);
                    sourceLayer = next;
                    w = w/2;
                    h = h/2;
                    d = d/2;
                }

                NxRHI.FMappedSubResource* initData = stackalloc NxRHI.FMappedSubResource[mipDatas.Count];
                try
                {
                    for (int i = 0; i<mipDatas.Count; i++)
                    {
                        switch (format)
                        {
                            case EPixelFormat.PXF_R8G8B8A8_UNORM:
                                initData[i].RowPitch = (uint)(mipDatas[i].Width * sizeof(uint));
                                initData[i].pData = mipDatas[i].CreateColorR8G8B8A8();
                                break;
                            case EPixelFormat.PXF_R16_FLOAT:
                                initData[i].RowPitch = (uint)(mipDatas[i].Width * sizeof(Half));
                                initData[i].pData = mipDatas[i].CreateRHalf();
                                break;
                        }
                        initData[i].DepthPitch = (uint)(initData[i].RowPitch * mipDatas[i].Height);
                    }

                    var texDesc = new NxRHI.FTextureDesc();
                    texDesc.SetDefault();
                    texDesc.Width = (uint)Width;
                    texDesc.Height = (uint)Height;
                    texDesc.Depth = (uint)Depth;
                    texDesc.Format = format;
                    texDesc.MipLevels = (uint)mipDatas.Count;
                    texDesc.InitData = initData;
                    return TtEngine.Instance.GfxDevice.RenderContext.CreateTexture(in texDesc);
                }
                finally
                {
                    for (int i = 0; i<mipDatas.Count; i++)
                    {
                        mipDatas[i].DesctroyPixels(initData[i].pData);
                    }
                }
            }
            public unsafe uint* CreateColorR8G8B8A8()
            {
                uint* result = (uint*)CoreSDK.Alloc((uint)(sizeof(uint) * Pixels.Length), null, 0);
                for (int i = 0; i<Pixels.Length; i++)
                {
                    result[i] = Pixels[i].ToColor4b().ToR8G8B8A8();
                }
                return result;
            }
            public unsafe Half* CreateRHalf()
            {
                Half* result = (Half*)CoreSDK.Alloc((uint)(sizeof(Half) * Pixels.Length), null, 0);
                for (int i = 0; i<Pixels.Length; i++)
                {
                    result[i] = new Half(Pixels[i].r);
                }
                return result;
            }
            public unsafe void DesctroyPixels(void* pixel)
            {
                CoreSDK.Free(pixel);
            }
        }
        public static TtTex3dLayer GenerateMipLayer3D(TtTex3dLayer sourceLayer, int w, int h, int d)
        {
            var target = new Color4f[w * h * d];
            TtTex3dLayer targetLayer = new TtTex3dLayer();
            targetLayer.Pixels = target;
            targetLayer.Width = w;
            targetLayer.Height = h;
            targetLayer.Depth = d;

            // 简单的3D盒式滤波（box filter）
            for (int z = 0; z < targetLayer.Depth; z++)
            {
                for (int y = 0; y < targetLayer.Height; y++)
                {
                    for (int x = 0; x < targetLayer.Width; x++)
                    {
                        // 采样2x2x2立方体的平均值
                        var sum = new Color4f();

                        for (int dz = 0; dz < 2; dz++)
                        {
                            for (int dy = 0; dy < 2; dy++)
                            {
                                for (int dx = 0; dx < 2; dx++)
                                {
                                    int sx = x * 2 + dx;
                                    int sy = y * 2 + dy;
                                    int sz = z * 2 + dz;

                                    int index = sx + sy * sourceLayer.Width + sz * sourceLayer.Width * sourceLayer.Height;
                                    sum += sourceLayer.Pixels[index];
                                }
                            }
                        }

                        int targetIndex = x + y * targetLayer.Width + z * targetLayer.Width * targetLayer.Height;
                        target[targetIndex].r = sum.r / 8f;
                        target[targetIndex].g = sum.g / 8f;
                        target[targetIndex].b = sum.b / 8f;
                        target[targetIndex].a = sum.a / 8f;
                    }
                }
            }

            return targetLayer;
        }

        public class TtTex2dLayer
        {
            public Color4f[] Pixels;
            public int Width;
            public int Height;
            public void NormalizeLayer()
            {
                Color4f cmin = new Color4f(float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue);
                Color4f cmax = new Color4f(float.MinValue, float.MinValue, float.MinValue, float.MinValue);
                foreach (var color in Pixels)
                {
                    if (color.Red<cmin.Red)
                        cmin.Red = color.Red;
                    if (color.Red>cmax.Red)
                        cmax.Red = color.Red;

                    if (color.Green<cmin.Green)
                        cmin.Green = color.Green;
                    if (color.Green>cmax.Green)
                        cmax.Green = color.Green;

                    if (color.Blue<cmin.Blue)
                        cmin.Blue = color.Blue;
                    if (color.Blue>cmax.Blue)
                        cmax.Blue = color.Blue;

                    if (color.Alpha<cmin.Alpha)
                        cmin.Alpha = color.Alpha;
                    if (color.Alpha>cmax.Alpha)
                        cmax.Alpha = color.Alpha;
                }
                Color4f delta = cmax - cmin;
                for (int i = 0; i<Pixels.Length; i++)
                {
                    Pixels[i].Red = (Pixels[i].Red - cmin.Red)/delta.Red;
                    Pixels[i].Green = (Pixels[i].Green - cmin.Green)/delta.Green;
                    Pixels[i].Blue = (Pixels[i].Blue - cmin.Blue)/delta.Blue;
                    Pixels[i].Alpha = (Pixels[i].Alpha - cmin.Alpha)/delta.Alpha;
                }
            }
            public unsafe NxRHI.TtTexture CreateTexture2D(EPixelFormat format = EPixelFormat.PXF_R8G8B8A8_UNORM, int MaxLayer = 1)
            {
                var sourceLayer = this;
                List<NxRHI.TtTextureUtility.TtTex2dLayer> mipDatas = new List<NxRHI.TtTextureUtility.TtTex2dLayer>();
                mipDatas.Add(sourceLayer);
                int w = Width/2;
                int h = Height/2;
                while (w>=1 && h>=1)
                {
                    if (mipDatas.Count>=MaxLayer)
                        break;
                    var next = NxRHI.TtTextureUtility.GenerateMipLayer2D(sourceLayer, w, h);
                    mipDatas.Add(next);
                    sourceLayer = next;
                    w = w/2;
                    h = h/2;
                }

                NxRHI.FMappedSubResource* initData = stackalloc NxRHI.FMappedSubResource[mipDatas.Count];
                try
                {
                    for (int i = 0; i<mipDatas.Count; i++)
                    {
                        switch(format)
                        {
                            case EPixelFormat.PXF_R8G8B8A8_UNORM:
                                initData[i].RowPitch = (uint)(mipDatas[i].Width * sizeof(uint));
                                initData[i].pData = mipDatas[i].CreateColorR8G8B8A8();
                                break;
                            case EPixelFormat.PXF_R16_FLOAT:
                                initData[i].RowPitch = (uint)(mipDatas[i].Width * sizeof(Half));
                                initData[i].pData = mipDatas[i].CreateRHalf();
                                break;
                        }
                        initData[i].DepthPitch = (uint)(initData[i].RowPitch * mipDatas[i].Height);
                    }

                    var texDesc = new NxRHI.FTextureDesc();
                    texDesc.SetDefault();
                    texDesc.Width = (uint)Width;
                    texDesc.Height = (uint)Height;
                    texDesc.Depth = (uint)0;
                    texDesc.Format = format;
                    texDesc.MipLevels = (uint)mipDatas.Count;
                    texDesc.InitData = initData;
                    return TtEngine.Instance.GfxDevice.RenderContext.CreateTexture(in texDesc);
                }
                finally
                {
                    for (int i = 0; i<mipDatas.Count; i++)
                    {
                        mipDatas[i].DesctroyPixels(initData[i].pData);
                    }
                }
            }
            public Color4f GetPixel(int x, int y)
            {
                if (x>Width||y>Height)
                    throw new ArgumentOutOfRangeException();
                return Pixels[y*Width + x];
            }
            public unsafe uint* CreateColorR8G8B8A8()
            {
                uint* result = (uint*)CoreSDK.Alloc((uint)(sizeof(uint) * Pixels.Length), null, 0);
                for (int i = 0; i<Pixels.Length; i++)
                {
                    result[i] = Pixels[i].ToColor4b().ToR8G8B8A8();
                }
                return result;
            }
            public unsafe Half* CreateRHalf()
            {
                Half* result = (Half*)CoreSDK.Alloc((uint)(sizeof(Half) * Pixels.Length), null, 0);
                for (int i = 0; i<Pixels.Length; i++)
                {
                    result[i] = new Half(Pixels[i].r);
                }
                return result;
            }
            public unsafe void DesctroyPixels(void* pixel)
            {
                CoreSDK.Free(pixel);
            }

            /// <summary>
            /// 将纹理层保存为PNG文件
            /// </summary>
            /// <param name="filePath">保存路径（绝对路径）</param>
            /// <param name="quality">PNG压缩级别（0-9，9为最高质量/最慢）</param>
            public bool SaveToPNG(string filePath, int quality = 9)
            {
                if (Pixels == null || Width <= 0 || Height <= 0)
                    return false;

                try
                {
                    // 准备像素数据（RGBA格式）
                    byte[] pixelData = new byte[Pixels.Length * 4];
                    for (int i = 0; i < Pixels.Length; i++)
                    {
                        int idx = i * 4;
                        pixelData[idx] = (byte)(Pixels[i].Red * 255.0f);   // R
                        pixelData[idx + 1] = (byte)(Pixels[i].Green * 255.0f); // G
                        pixelData[idx + 2] = (byte)(Pixels[i].Blue * 255.0f);  // B
                        pixelData[idx + 3] = (byte)(Pixels[i].Alpha * 255.0f); // A
                    }

                    using (var memStream = new System.IO.FileStream(filePath, System.IO.FileMode.OpenOrCreate))
                    {
                        if (memStream == null)
                            return false;
                        var writer = new StbImageWriteSharp.ImageWriter();
                        writer.WritePng(pixelData, Width, Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, memStream);
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"SaveToPNG failed: {ex.Message}");
                    return false;
                }
            }
        }
        public static TtTex2dLayer GenerateMipLayer2D(TtTex2dLayer sourceLayer, int w, int h)
        {
            var target = new Color4f[w * h];
            TtTex2dLayer targetLayer = new TtTex2dLayer();
            targetLayer.Pixels = target;
            targetLayer.Width = w;
            targetLayer.Height = h;

            // 简单的3D盒式滤波（box filter）
            for (int y = 0; y < targetLayer.Height; y++)
            {
                for (int x = 0; x < targetLayer.Width; x++)
                {
                    // 采样2x2正方形的平均值
                    var sum = new Color4f();

                    for (int dy = 0; dy < 2; dy++)
                    {
                        for (int dx = 0; dx < 2; dx++)
                        {
                            int sx = x * 2 + dx;
                            int sy = y * 2 + dy;

                            int index = sx + sy * sourceLayer.Width;
                            sum += sourceLayer.Pixels[index];
                        }
                    }

                    int targetIndex = x + y * targetLayer.Width;
                    target[targetIndex].r = sum.r / 4f;
                    target[targetIndex].g = sum.g / 4f;
                    target[targetIndex].b = sum.b / 4f;
                    target[targetIndex].a = sum.a / 4f;
                }
            }

            return targetLayer;
        }
    }

    /// <summary>
    /// Readback utilities for TtSrView — reads GPU texture data back to CPU-side TtTex2dLayer / TtTex3dLayer.
    /// Supports uncompressed formats and BC block-compressed formats; ASTC is reserved for future implementation.
    /// </summary>
    public partial class TtSrView
    {
        #region Pixel Format Helpers

        /// <summary>
        /// Whether the given pixel format is a BC block-compressed format.
        /// </summary>
        private static bool IsBlockCompressedFormat(EPixelFormat format)
        {
            switch (format)
            {
                case EPixelFormat.PXF_BC1_UNORM:
                case EPixelFormat.PXF_BC1_UNORM_SRGB:
                case EPixelFormat.PXF_BC1_TYPELESS:
                case EPixelFormat.PXF_BC2_UNORM:
                case EPixelFormat.PXF_BC2_UNORM_SRGB:
                case EPixelFormat.PXF_BC2_TYPELESS:
                case EPixelFormat.PXF_BC3_UNORM:
                case EPixelFormat.PXF_BC3_UNORM_SRGB:
                case EPixelFormat.PXF_BC3_TYPELESS:
                case EPixelFormat.PXF_BC4_UNORM:
                case EPixelFormat.PXF_BC4_SNORM:
                case EPixelFormat.PXF_BC4_TYPELESS:
                case EPixelFormat.PXF_BC5_UNORM:
                case EPixelFormat.PXF_BC5_SNORM:
                case EPixelFormat.PXF_BC5_TYPELESS:
                case EPixelFormat.PXF_BC6H_UF16:
                case EPixelFormat.PXF_BC6H_SF16:
                case EPixelFormat.PXF_BC6H_TYPELESS:
                case EPixelFormat.PXF_BC7_UNORM:
                case EPixelFormat.PXF_BC7_UNORM_SRGB:
                case EPixelFormat.PXF_BC7_TYPELESS:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Map EPixelFormat to BCnEncoder CompressionFormat.
        /// Returns null if the format is not a recognized BC format.
        /// </summary>
        private static CompressionFormat? GetBCnCompressionFormat(EPixelFormat format)
        {
            switch (format)
            {
                case EPixelFormat.PXF_BC1_UNORM:
                case EPixelFormat.PXF_BC1_UNORM_SRGB:
                case EPixelFormat.PXF_BC1_TYPELESS:
                    return CompressionFormat.Bc1;
                case EPixelFormat.PXF_BC2_UNORM:
                case EPixelFormat.PXF_BC2_UNORM_SRGB:
                case EPixelFormat.PXF_BC2_TYPELESS:
                    return CompressionFormat.Bc2;
                case EPixelFormat.PXF_BC3_UNORM:
                case EPixelFormat.PXF_BC3_UNORM_SRGB:
                case EPixelFormat.PXF_BC3_TYPELESS:
                    return CompressionFormat.Bc3;
                case EPixelFormat.PXF_BC4_UNORM:
                case EPixelFormat.PXF_BC4_SNORM:
                case EPixelFormat.PXF_BC4_TYPELESS:
                    return CompressionFormat.Bc4;
                case EPixelFormat.PXF_BC5_UNORM:
                case EPixelFormat.PXF_BC5_SNORM:
                case EPixelFormat.PXF_BC5_TYPELESS:
                    return CompressionFormat.Bc5;
                case EPixelFormat.PXF_BC6H_UF16:
                case EPixelFormat.PXF_BC6H_SF16:
                case EPixelFormat.PXF_BC6H_TYPELESS:
                    return CompressionFormat.Bc6U;
                case EPixelFormat.PXF_BC7_UNORM:
                case EPixelFormat.PXF_BC7_UNORM_SRGB:
                case EPixelFormat.PXF_BC7_TYPELESS:
                    return CompressionFormat.Bc7;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Get the byte size of one compressed block (4x4 texels) for a given BC format.
        /// </summary>
        private static int GetBlockByteSize(EPixelFormat format)
        {
            switch (format)
            {
                case EPixelFormat.PXF_BC1_UNORM:
                case EPixelFormat.PXF_BC1_UNORM_SRGB:
                case EPixelFormat.PXF_BC1_TYPELESS:
                case EPixelFormat.PXF_BC4_UNORM:
                case EPixelFormat.PXF_BC4_SNORM:
                case EPixelFormat.PXF_BC4_TYPELESS:
                    return 8;
                default:
                    return 16;
            }
        }

        /// <summary>
        /// Decode a single uncompressed pixel to Color4f.
        /// </summary>
        private static unsafe Color4f DecodePixelToColor4f(byte* pixel, EPixelFormat format)
        {
            switch (format)
            {
                case EPixelFormat.PXF_R8G8B8A8_UNORM:
                case EPixelFormat.PXF_R8G8B8A8_UNORM_SRGB:
                    return new Color4f(pixel[0] / 255f, pixel[1] / 255f, pixel[2] / 255f, pixel[3] / 255f);
                case EPixelFormat.PXF_B8G8R8A8_UNORM:
                case EPixelFormat.PXF_B8G8R8A8_UNORM_SRGB:
                    return new Color4f(pixel[2] / 255f, pixel[1] / 255f, pixel[0] / 255f, pixel[3] / 255f);
                case EPixelFormat.PXF_R16G16B16A16_FLOAT:
                    {
                        var halves = (Half*)pixel;
                        return new Color4f((float)halves[0], (float)halves[1], (float)halves[2], (float)halves[3]);
                    }
                case EPixelFormat.PXF_R32G32B32A32_FLOAT:
                    {
                        var floats = (float*)pixel;
                        return new Color4f(floats[0], floats[1], floats[2], floats[3]);
                    }
                case EPixelFormat.PXF_R16_FLOAT:
                    return new Color4f((float)(*(Half*)pixel), 0f, 0f, 1f);
                case EPixelFormat.PXF_R32_FLOAT:
                    return new Color4f(*(float*)pixel, 0f, 0f, 1f);
                case EPixelFormat.PXF_R16G16_FLOAT:
                    {
                        var halves = (Half*)pixel;
                        return new Color4f((float)halves[0], (float)halves[1], 0f, 1f);
                    }
                case EPixelFormat.PXF_R32G32_FLOAT:
                    {
                        var floats = (float*)pixel;
                        return new Color4f(floats[0], floats[1], 0f, 1f);
                    }
                case EPixelFormat.PXF_R8_UNORM:
                    return new Color4f(pixel[0] / 255f, 0f, 0f, 1f);
                case EPixelFormat.PXF_R8G8_UNORM:
                    return new Color4f(pixel[0] / 255f, pixel[1] / 255f, 0f, 1f);
                default:
                    return new Color4f(0f, 0f, 0f, 1f);
            }
        }

        #endregion

        #region BC Block Decoding (via BCnEncoder.Net)

        /// <summary>
        /// Collect block-compressed rows (which may have GPU rowPitch padding)
        /// into a contiguous byte[] that BcDecoder.DecodeRaw expects.
        /// </summary>
        private static unsafe byte[] CopyCompactBlockData(byte* srcData, uint rowPitch,
            int width, int height, EPixelFormat format)
        {
            int blockWidth = (width + 3) / 4;
            int blockHeight = (height + 3) / 4;
            int blockByteSize = GetBlockByteSize(format);
            int compactRowBytes = blockWidth * blockByteSize;
            var compactData = new byte[compactRowBytes * blockHeight];

            fixed (byte* dst = compactData)
            {
                for (int by = 0; by < blockHeight; by++)
                {
                    Buffer.MemoryCopy(
                        srcData + (long)by * rowPitch,
                        dst + by * compactRowBytes,
                        compactRowBytes, compactRowBytes);
                }
            }
            return compactData;
        }

        /// <summary>
        /// Decode a block-compressed subresource into a TtTex2dLayer
        /// using BCnEncoder.Net's BcDecoder (supports BC1–BC7).
        /// </summary>
        private static unsafe void DecodeBlockCompressedToLayer(
            byte* srcData, uint rowPitch,
            int width, int height, EPixelFormat format,
            TtTextureUtility.TtTex2dLayer layer)
        {
            var bcnFormat = GetBCnCompressionFormat(format);
            if (bcnFormat == null)
                return;

            var compactData = CopyCompactBlockData(srcData, rowPitch, width, height, format);
            var decoder = new BcDecoder();

            bool isBc6 = (bcnFormat.Value == CompressionFormat.Bc6U || bcnFormat.Value == CompressionFormat.Bc6S);
            if (isBc6)
            {
                // BC6H is HDR — must use DecodeRawHdr which returns ColorRgbFloat[]
                var decoded = decoder.DecodeRawHdr(compactData, width, height, bcnFormat.Value);
                for (int i = 0; i < decoded.Length && i < layer.Pixels.Length; i++)
                {
                    var c = decoded[i];
                    layer.Pixels[i] = new Color4f(c.r, c.g, c.b, 1.0f);
                }
            }
            else
            {
                // BC1–BC5, BC7: LDR decode returns ColorRgba32[]
                var decoded = decoder.DecodeRaw(compactData, width, height, bcnFormat.Value);
                for (int i = 0; i < decoded.Length && i < layer.Pixels.Length; i++)
                {
                    var c = decoded[i];
                    layer.Pixels[i] = new Color4f(c.r / 255f, c.g / 255f, c.b / 255f, c.a / 255f);
                }
            }
        }

        #endregion

        #region Readback Methods

        /// <summary>
        /// Synchronously copy a GPU texture subresource to a CPU-readable staging buffer
        /// and fetch its data into a blob. This is the standard readback pattern:
        /// CreateReadable → submit cpDraw → flush → FetchGpuData.
        /// The returned FSubResourceFootPrint contains RowPitch, Width, Height, Depth, Format
        /// as reported by the GPU driver, which should be used for correct stride-aware reading.
        /// </summary>
        /// <param name="texture">Source GPU texture.</param>
        /// <param name="subResource">Subresource index to read back.</param>
        /// <param name="blob">Output blob that will contain the raw pixel data
        /// (prefixed with RowPitch + DepthPitch as two uint32).</param>
        /// <param name="footPrint">Output footprint filled by CreateReadable, contains
        /// RowPitch / Width / Height / Depth / Format / TotalSize.</param>
        /// <returns>True if readback succeeded.</returns>
        public static bool FetchTextureSubresource(ITexture texture, int subResource,
            Support.TtBlobObject blob, out FSubResourceFootPrint footPrint)
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            var cpDraw = rc.CreateCopyDraw();
            var readable = texture.CreateReadable(rc.mCoreObject, subResource, cpDraw.mCoreObject);
            footPrint = cpDraw.FootPrint;

            using (var cmd = new FTransientCmd(EQueueType.QU_Default, "FetchTexSub"))
            {
                cmd.CmdList.PushGpuDraw(cpDraw.mCoreObject);
            }
            rc.GpuQueue.Flush(EQueueType.QU_Default);
            cpDraw.Dispose();

            bool ok = readable.FetchGpuData(rc.mCoreObject, 0, blob.mCoreObject);
            readable.Dispose();
            return ok;
        }

        /// <summary>
        /// Read back a single 2D subresource from GPU and return it as a TtTex2dLayer.
        /// Handles both uncompressed and BC block-compressed formats.
        /// </summary>
        private unsafe TtTextureUtility.TtTex2dLayer ReadbackSubresource2D(
            ITexture texture, uint subResource, int width, int height, EPixelFormat format)
        {
            var blob = new Support.TtBlobObject();
            if (!FetchTextureSubresource(texture, (int)subResource, blob, out var footPrint))
                return null;

            var layer = new TtTextureUtility.TtTex2dLayer();
            layer.Width = width;
            layer.Height = height;
            layer.Pixels = new Color4f[width * height];

            var pData = (byte*)blob.DataPointer;
            uint rowPitch = footPrint.RowPitch;
            // blob data starts after RowPitch + DepthPitch header (2 x uint32)
            pData += sizeof(uint) + sizeof(uint);

            if (IsBlockCompressedFormat(format))
            {
                DecodeBlockCompressedToLayer(pData, rowPitch, width, height, format, layer);
            }
            else
            {
                int pixelByteWidth = CoreSDK.GetPixelFormatByteWidth(format);
                for (int y = 0; y < height; y++)
                {
                    byte* rowStart = pData + (uint)y * rowPitch;
                    for (int x = 0; x < width; x++)
                    {
                        byte* pixel = rowStart + x * pixelByteWidth;
                        layer.Pixels[y * width + x] = DecodePixelToColor4f(pixel, format);
                    }
                }
            }

            return layer;
        }

        /// <summary>
        /// Readback all array slices of a Tex2DArray at the specified mip level.
        /// Each array slice becomes one TtTex2dLayer.
        /// </summary>
        public unsafe List<TtTextureUtility.TtTex2dLayer> ReadbackTex2DArray(int mipLevel)
        {
            var texture = GetTexture();
            if (!texture.IsValidPointer)
                return null;

            var desc = texture.Desc;
            uint mipLevels = desc.MipLevels;
            uint arraySize = desc.ArraySize;
            if (mipLevel < 0 || mipLevel >= (int)mipLevels)
                return null;

            int mipWidth = Math.Max(1, (int)desc.Width >> mipLevel);
            int mipHeight = Math.Max(1, (int)desc.Height >> mipLevel);
            var format = desc.Format;

            var result = new List<TtTextureUtility.TtTex2dLayer>((int)arraySize);
            for (uint slice = 0; slice < arraySize; slice++)
            {
                uint subResource = slice * mipLevels + (uint)mipLevel;
                var layer = ReadbackSubresource2D(texture, subResource, mipWidth, mipHeight, format);
                if (layer == null)
                    return null;
                result.Add(layer);
            }
            return result;
        }

        /// <summary>
        /// Readback a Tex3D at the specified mip level.
        /// The entire 3D volume is returned as a single TtTex3dLayer.
        /// </summary>
        public unsafe TtTextureUtility.TtTex3dLayer ReadbackTex3D(int mipLevel)
        {
            var texture = GetTexture();
            if (!texture.IsValidPointer)
                return null;

            var desc = texture.Desc;
            uint mipLevels = desc.MipLevels;
            if (mipLevel < 0 || mipLevel >= (int)mipLevels)
                return null;

            int mipWidth = Math.Max(1, (int)desc.Width >> mipLevel);
            int mipHeight = Math.Max(1, (int)desc.Height >> mipLevel);
            int mipDepth = Math.Max(1, (int)desc.Depth >> mipLevel);
            var format = desc.Format;

            uint subResource = (uint)mipLevel;
            var blob = new Support.TtBlobObject();
            if (!FetchTextureSubresource(texture, (int)subResource, blob, out var footPrint))
                return null;

            var layer = new TtTextureUtility.TtTex3dLayer();
            layer.Width = mipWidth;
            layer.Height = mipHeight;
            layer.Depth = mipDepth;
            layer.Pixels = new Color4f[mipWidth * mipHeight * mipDepth];

            var pData = (byte*)blob.DataPointer;
            uint rowPitch = footPrint.RowPitch;
            uint depthPitch = rowPitch * footPrint.Height;
            // blob data starts after RowPitch + DepthPitch header (2 x uint32)
            pData += sizeof(uint) + sizeof(uint);

            if (IsBlockCompressedFormat(format))
            {
                // for 3D BC textures, decode each depth slice independently
                for (int z = 0; z < mipDepth; z++)
                {
                    byte* sliceStart = pData + (uint)z * depthPitch;
                    var sliceLayer = new TtTextureUtility.TtTex2dLayer();
                    sliceLayer.Width = mipWidth;
                    sliceLayer.Height = mipHeight;
                    sliceLayer.Pixels = new Color4f[mipWidth * mipHeight];
                    DecodeBlockCompressedToLayer(sliceStart, rowPitch, mipWidth, mipHeight, format, sliceLayer);
                    Array.Copy(sliceLayer.Pixels, 0, layer.Pixels, z * mipWidth * mipHeight, mipWidth * mipHeight);
                }
            }
            else
            {
                int pixelByteWidth = CoreSDK.GetPixelFormatByteWidth(format);
                for (int z = 0; z < mipDepth; z++)
                {
                    byte* sliceStart = pData + (uint)z * depthPitch;
                    for (int y = 0; y < mipHeight; y++)
                    {
                        byte* rowStart = sliceStart + (uint)y * rowPitch;
                        for (int x = 0; x < mipWidth; x++)
                        {
                            byte* pixel = rowStart + x * pixelByteWidth;
                            layer.Pixels[x + y * mipWidth + z * mipWidth * mipHeight] =
                                DecodePixelToColor4f(pixel, format);
                        }
                    }
                }
            }

            return layer;
        }

        /// <summary>
        /// Readback a CubeArray texture at the specified mip level.
        /// Returns one TtTex3dLayer per cube (Depth=6, one per face).
        /// </summary>
        public unsafe List<TtTextureUtility.TtTex3dLayer> ReadbackTexCubeArray(int mipLevel)
        {
            var texture = GetTexture();
            if (!texture.IsValidPointer)
                return null;

            var desc = texture.Desc;
            uint mipLevels = desc.MipLevels;
            uint arraySize = desc.ArraySize;
            if (mipLevel < 0 || mipLevel >= (int)mipLevels)
                return null;
            if (arraySize < 6 || arraySize % 6 != 0)
                return null;

            int mipWidth = Math.Max(1, (int)desc.Width >> mipLevel);
            int mipHeight = Math.Max(1, (int)desc.Height >> mipLevel);
            var format = desc.Format;
            uint cubeCount = arraySize / 6;

            var result = new List<TtTextureUtility.TtTex3dLayer>((int)cubeCount);
            for (uint cubeIndex = 0; cubeIndex < cubeCount; cubeIndex++)
            {
                var cubeLayer = new TtTextureUtility.TtTex3dLayer();
                cubeLayer.Width = mipWidth;
                cubeLayer.Height = mipHeight;
                cubeLayer.Depth = 6;
                cubeLayer.Pixels = new Color4f[mipWidth * mipHeight * 6];

                for (uint face = 0; face < 6; face++)
                {
                    uint arraySlice = cubeIndex * 6 + face;
                    uint subResource = arraySlice * mipLevels + (uint)mipLevel;
                    var faceLayer = ReadbackSubresource2D(texture, subResource, mipWidth, mipHeight, format);
                    if (faceLayer == null)
                        return null;

                    int facePixelCount = mipWidth * mipHeight;
                    Array.Copy(faceLayer.Pixels, 0, cubeLayer.Pixels, (int)face * facePixelCount, facePixelCount);
                }

                result.Add(cubeLayer);
            }
            return result;
        }

        #endregion

        #region SaveAssetTo2 — Readback-based texture asset saving

        /// <summary>
        /// Whether the given pixel format stores floating-point (HDR) data.
        /// </summary>
        private static bool IsHdrPixelFormat(EPixelFormat format)
        {
            switch (format)
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
                default:
                    return false;
            }
        }

        /// <summary>
        /// Convert a Color4f pixel array into a LDR TtMemImage (RGBA8, byte[]).
        /// </summary>
        private static StbImageSharp.TtMemImage Color4fLayerToMemImage(Color4f[] pixels, int width, int height)
        {
            var image = new StbImageSharp.TtMemImage();
            image.Width = width;
            image.Height = height;
            image.Comp = StbImageSharp.ColorComponents.RedGreenBlueAlpha;
            image.SourceComp = StbImageSharp.ColorComponents.RedGreenBlueAlpha;
            image.Data = new byte[width * height * 4];
            for (int i = 0; i < pixels.Length; i++)
            {
                int idx = i * 4;
                image.Data[idx] = (byte)MathHelper.Clamp(pixels[i].Red * 255f, 0f, 255f);
                image.Data[idx + 1] = (byte)MathHelper.Clamp(pixels[i].Green * 255f, 0f, 255f);
                image.Data[idx + 2] = (byte)MathHelper.Clamp(pixels[i].Blue * 255f, 0f, 255f);
                image.Data[idx + 3] = (byte)MathHelper.Clamp(pixels[i].Alpha * 255f, 0f, 255f);
            }
            return image;
        }

        /// <summary>
        /// Convert a Color4f pixel array into a HDR ImageResultFloat (RGBA float[]).
        /// </summary>
        private static StbImageSharp.ImageResultFloat Color4fLayerToImageFloat(Color4f[] pixels, int width, int height)
        {
            var image = new StbImageSharp.ImageResultFloat();
            image.Width = width;
            image.Height = height;
            image.Comp = StbImageSharp.ColorComponents.RedGreenBlueAlpha;
            image.SourceComp = StbImageSharp.ColorComponents.RedGreenBlueAlpha;
            image.Data = new float[width * height * 4];
            for (int i = 0; i < pixels.Length; i++)
            {
                int idx = i * 4;
                image.Data[idx] = pixels[i].Red;
                image.Data[idx + 1] = pixels[i].Green;
                image.Data[idx + 2] = pixels[i].Blue;
                image.Data[idx + 3] = pixels[i].Alpha;
            }
            return image;
        }

        /// <summary>
        /// Flatten a CubeArray readback (6 faces) into a single horizontal-strip image
        /// suitable for SaveTexture (width = faceWidth * 6, height = faceHeight).
        /// Each face is placed side by side: +X -X +Y -Y +Z -Z.
        /// </summary>
        private static void FlattenCubeFacesToStrip(
            TtTextureUtility.TtTex3dLayer cubeLayer, bool isHdr,
            out StbImageSharp.TtMemImage ldrImage,
            out StbImageSharp.ImageResultFloat hdrImage)
        {
            ldrImage = null;
            hdrImage = null;
            int faceWidth = cubeLayer.Width;
            int faceHeight = cubeLayer.Height;
            int facePixelCount = faceWidth * faceHeight;

            if (isHdr)
            {
                // Each face side by side: total width = faceWidth, height = faceHeight
                // CubeFaces = 6, SaveTexture(HDR) handles CubeFaces via desc.CubeFaces
                // We save each face as a separate image and let the existing pipeline handle it
                // Actually, the existing SaveTexture uses desc.CubeFaces to iterate faces
                // and expects the image to be a square (faceWidth x faceHeight) per face.
                // So we flatten to faceWidth x (faceHeight * 6) vertical strip approach is wrong.
                // Instead, we save the first face as the image and iterate faces in the save loop.
                // However, SaveTexture doesn't accept per-face data...
                // Best approach: create a faceWidth x faceHeight image and desc.CubeFaces = 6
                // The existing SaveHdrMips/SaveDxtMips iterate by CubeFaces but always use the same image.
                // For readback, we already have per-face data; we need to serialize each face individually.
                // We'll handle this in SaveAssetTo2 directly rather than using SaveTexture.
                hdrImage = Color4fLayerToImageFloat(cubeLayer.Pixels, faceWidth, faceHeight * 6);
                hdrImage.Width = faceWidth;
                hdrImage.Height = faceHeight;
            }
            else
            {
                ldrImage = Color4fLayerToMemImage(cubeLayer.Pixels, faceWidth, faceHeight * 6);
                ldrImage.Width = faceWidth;
                ldrImage.Height = faceHeight;
            }
        }

        /// <summary>
        /// Setup PicDesc based on the current SRV's texture properties and engine compress configuration.
        /// </summary>
        private TtPicDesc BuildPicDescFromTexture(EPixelFormat format, int width, int height, int depth, uint cubeFaces)
        {
            var desc = new TtPicDesc();
            desc.Width = width;
            desc.Height = height;
            desc.Depth = depth;
            desc.CubeFaces = cubeFaces;
            desc.Format = format;
            desc.BitNumAlpha = 8;
            desc.BitNumRed = 8;
            desc.BitNumGreen = 8;
            desc.BitNumBlue = 8;

            // Determine compression based on engine config
            var compressType = TtEngine.Instance.GfxDevice.Config.TextureAssetCompressType;
            switch (compressType)
            {
                case Graphics.Pipeline.TtGfxDeviceConfig.ETextureAssetCompressType.None:
                    desc.DontCompress = true;
                    break;
                case Graphics.Pipeline.TtGfxDeviceConfig.ETextureAssetCompressType.DXT:
                    desc.DontCompress = false;
                    break;
                case Graphics.Pipeline.TtGfxDeviceConfig.ETextureAssetCompressType.ASTC:
                    desc.DontCompress = false;
                    break;
                case Graphics.Pipeline.TtGfxDeviceConfig.ETextureAssetCompressType.ETC2:
                    desc.DontCompress = false;
                    break;
            }

            // Non-4-aligned dimensions cannot be block-compressed
            if (width % 4 != 0 || height % 4 != 0)
                desc.DontCompress = true;

            return desc;
        }
        /// <summary>
        /// 选择 DXT 压缩格式
        /// </summary>
        /// <param name="desc">纹理描述符</param>
        /// <returns>DXT 压缩格式</returns>
        public static ETextureCompressFormat SelectDxtFormat(TtPicDesc desc)
        {
            if (IsHdrFormat(desc))
            {
                return ETextureCompressFormat.TCF_BC6;
            }
            // 法线贴图使用 BC5
            if (desc.IsNormal)
            {
                return ETextureCompressFormat.TCF_BC5;
            }
            else if(desc.BitNumRed == 8 && desc.BitNumGreen ==0 && desc.BitNumBlue ==0 && desc.BitNumAlpha == 0)
            {
                return ETextureCompressFormat.TCF_BC4;
            }
            else if (desc.BitNumAlpha > 1)
            {
                return ETextureCompressFormat.TCF_BC3;
            }
            else if (desc.BitNumAlpha == 1)
            {
                return ETextureCompressFormat.TCF_BC1A;
            }
            else
            {
                return ETextureCompressFormat.TCF_BC1;
            }
        }
        public static bool IsHdrFormat(TtPicDesc desc)
        {
            if(desc.BitNumRed > 8 || desc.BitNumGreen > 8 || desc.BitNumBlue > 8)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 选择 ETC2 压缩格式
        /// </summary>
        /// <param name="desc">纹理描述符</param>
        /// <returns>ETC2 压缩格式</returns>
        public static ETextureCompressFormat SelectEtc2Format(TtPicDesc desc)
        {
            // 根据 Alpha 位数选择格式
            if (desc.BitNumAlpha == 8 || desc.BitNumAlpha == 4)
            {
                return ETextureCompressFormat.TCF_Etc2_RGBA8;
            }
            else if (desc.BitNumAlpha == 1)
            {
                return ETextureCompressFormat.TCF_Etc2_RGBA1;
            }
            else
            {
                return ETextureCompressFormat.TCF_Etc2_RGB8;
            }
        }

        /// <summary>
        /// 选择 ASTC 压缩格式
        /// </summary>
        /// <param name="desc">纹理描述符</param>
        /// <returns>ASTC 压缩格式</returns>
        public static ETextureCompressFormat SelectAstcFormat(TtPicDesc desc)
        {
            // 注意：ASTC 格式需要更多配置参数
            // 当前实现返回默认格式
            System.Diagnostics.Debug.Assert(false, "ASTC compression needs more parameters");

            // 根据 Alpha 位数选择基础格式
            if (desc.BitNumAlpha == 8 || desc.BitNumAlpha == 4)
            {
                return ETextureCompressFormat.TCF_Etc2_RGBA8;  // 临时回退到 ETC2
            }
            else if (desc.BitNumAlpha == 1)
            {
                return ETextureCompressFormat.TCF_Etc2_RGBA1;
            }
            else
            {
                return ETextureCompressFormat.TCF_Etc2_RGB8;
            }
        }
        public static List<ETextureCompressFormat> SelectLdrCompressFormats(TtPicDesc desc)
        {
            List<ETextureCompressFormat> result = new List<ETextureCompressFormat>();
            // 如果不压缩，返回 None
            if (desc.DontCompress)
                return result;

            // 检查引擎配置
            var config = TtEngine.Instance?.GfxDevice.Config;
            if (config == null)
                return result;

            if (config.TextureAssetCompressType.HasFlag(Graphics.Pipeline.TtGfxDeviceConfig.ETextureAssetCompressType.DXT))
            {
                result.Add(SelectDxtFormat(desc));
            }
            if (config.TextureAssetCompressType.HasFlag(Graphics.Pipeline.TtGfxDeviceConfig.ETextureAssetCompressType.ASTC))
            {
                result.Add(SelectAstcFormat(desc));
            }
            if (config.TextureAssetCompressType.HasFlag(Graphics.Pipeline.TtGfxDeviceConfig.ETextureAssetCompressType.ETC2))
            {
                result.Add(SelectEtc2Format(desc));
            }
            return result;
        }
        /// <summary>
        /// Save a 2D layer array (including single 2D texture) into XND with mip + compression.
        /// Each layer becomes a face node; compression follows engine config.
        /// </summary>
        private void SaveLayersToXnd(
            XndNode node,
            List<TtTextureUtility.TtTex2dLayer> layers,
            TtPicDesc desc,
            bool isHdr,
            RName assetName)
        {
            desc.CubeFaces = (uint)layers.Count;
            desc.MipSizes.Clear();
            desc.BlockDimenstions.Clear();

            if (isHdr)
            {
                desc.CompressFormat = TtTextureHelper.SelectHdrCompressFormat(desc);
                switch (desc.CompressFormat)
                {
                    case ETextureCompressFormat.TCF_None:
                        {
                            var hdrMipsNode = node.GetOrAddNode("HdrMips", 0, 0, true);
                            SaveHdrMipsFromLayers(hdrMipsNode, layers, desc);
                        }
                        break;
                    case ETextureCompressFormat.TCF_BC6:
                        {
                            var dxtMipsNode = node.GetOrAddNode("DxtMips", 0, 0, true);
                            SaveDxtMipsFromLayers(dxtMipsNode, layers, desc, isHdr: true);
                        }
                        break;
                    default:
                        {
                            // Fallback to uncompressed HDR
                            desc.CompressFormat = ETextureCompressFormat.TCF_None;
                            var hdrMipsNode = node.GetOrAddNode("HdrMips", 0, 0, true);
                            SaveHdrMipsFromLayers(hdrMipsNode, layers, desc);
                        }
                        break;
                }
            }
            else
            {
                desc.CompressFormat = TtTextureHelper.SelectLdrCompressFormat(desc);
                switch (desc.CompressFormat)
                {
                    case ETextureCompressFormat.TCF_None:
                        {
                            var pngMipsNode = node.GetOrAddNode("PngMips", 0, 0, true);
                            SavePngMipsFromLayers(pngMipsNode, layers, desc);
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
                        {
                            // DXT / ETC2 compressed
                            var dxtMipsNode = node.GetOrAddNode("DxtMips", 0, 0, true);
                            SaveDxtMipsFromLayers(dxtMipsNode, layers, desc, isHdr: false);
                        }
                        break;
                    default:
                        System.Diagnostics.Debug.Assert(false, $"Unsupported compression format {desc.CompressFormat}");
                        break;
                }
            }

            TtTextureHelper.SaveDescToNode(node, desc);
        }

        /// <summary>
        /// Save PNG mip chain from readback layers (LDR, uncompressed).
        /// </summary>
        private void SavePngMipsFromLayers(
            XndNode mipsNode,
            List<TtTextureUtility.TtTex2dLayer> layers,
            TtPicDesc desc)
        {
            desc.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
            if (desc.MipLevel == 0)
                desc.MipLevel = CalcMipLevel(layers[0].Width, layers[0].Height, true, 4);

            for (int faceIdx = 0; faceIdx < layers.Count; faceIdx++)
            {
                var faceNode = mipsNode.GetOrAddNode($"Face{faceIdx}", 0, 0, true);
                var curImage = Color4fLayerToMemImage(layers[faceIdx].Pixels, layers[faceIdx].Width, layers[faceIdx].Height);

                for (int mip = 0; mip < desc.MipLevel; mip++)
                {
                    using (var memStream = new System.IO.MemoryStream(curImage.Data.Length))
                    {
                        var writer = new StbImageWriteSharp.ImageWriter();
                        writer.WritePng(curImage.Data, curImage.Width, curImage.Height,
                            StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, memStream);
                        var pngData = memStream.ToArray();
                        var attr = faceNode.GetOrAddAttribute($"PngMip{mip}", 0, 0, true);
                        using (var ar = attr.GetWriter((ulong)memStream.Position))
                        {
                            ar.WriteNoSize(pngData, (int)memStream.Position);
                        }
                    }

                    if (faceIdx == 0)
                    {
                        desc.MipSizes.Add(new Vector3i()
                        {
                            X = curImage.Width,
                            Y = curImage.Height,
                            Z = curImage.Width * 4
                        });
                    }

                    int nextWidth = Math.Max(1, curImage.Width / 2);
                    int nextHeight = Math.Max(1, curImage.Height / 2);
                    if (nextWidth == curImage.Width && nextHeight == curImage.Height)
                        break;
                    curImage = StbImageSharp.ImageProcessor.GetBoxDownSampler(curImage, nextWidth, nextHeight);
                    if (curImage == null)
                        break;
                }
            }
        }

        /// <summary>
        /// Save HDR mip chain from readback layers (uncompressed float).
        /// </summary>
        private unsafe void SaveHdrMipsFromLayers(
            XndNode mipsNode,
            List<TtTextureUtility.TtTex2dLayer> layers,
            TtPicDesc desc)
        {
            desc.Format = EPixelFormat.PXF_R32G32B32A32_FLOAT;
            if (desc.MipLevel == 0)
                desc.MipLevel = CalcMipLevel(layers[0].Width, layers[0].Height, true, 4);
            desc.MipLevel = Math.Max(desc.MipLevel, 1);

            for (int faceIdx = 0; faceIdx < layers.Count; faceIdx++)
            {
                var faceNode = mipsNode.GetOrAddNode($"Face{faceIdx}", 0, 0, true);
                var curImage = Color4fLayerToImageFloat(layers[faceIdx].Pixels, layers[faceIdx].Width, layers[faceIdx].Height);

                for (int mip = 0; mip < desc.MipLevel; mip++)
                {
                    using (var memStream = new System.IO.MemoryStream())
                    {
                        var writer = new StbImageWriteSharp.ImageWriter();
                        var writeComp = UStbImageUtility.ConvertColorComponent(curImage.Comp);
                        fixed (void* ptr = curImage.Data)
                        {
                            writer.WriteHdr(ptr, curImage.Width, curImage.Height, writeComp, memStream);
                        }

                        var hdrData = memStream.ToArray();
                        var attr = faceNode.GetOrAddAttribute($"HdrMip{mip}", 0, 0, true);
                        using (var ar = attr.GetWriter((ulong)memStream.Position))
                        {
                            ar.WriteNoSize(hdrData, (int)memStream.Position);
                        }
                    }

                    if (faceIdx == 0)
                    {
                        desc.MipSizes.Add(new Vector3i()
                        {
                            X = curImage.Width,
                            Y = curImage.Height,
                            Z = curImage.Width * (int)curImage.Comp * sizeof(float)
                        });
                    }

                    int nextWidth = Math.Max(1, curImage.Width / 2);
                    int nextHeight = Math.Max(1, curImage.Height / 2);
                    if (nextWidth == curImage.Width && nextHeight == curImage.Height)
                        break;
                    curImage = StbImageSharp.ImageProcessor.GetBoxDownSampler(curImage, nextWidth, nextHeight);
                }
            }
        }

        /// <summary>
        /// Save DXT/ETC2 compressed mip chain from readback layers (BC-encoded).
        /// </summary>
        private void SaveDxtMipsFromLayers(
            XndNode mipsNode,
            List<TtTextureUtility.TtTex2dLayer> layers,
            TtPicDesc desc,
            bool isHdr)
        {
            if (isHdr)
            {
                // HDR BC6 compression using EncodeToRawBytesHdr (ColorRgbFloat[])
                desc.Format = EPixelFormat.PXF_BC6H_UF16;
                if (desc.MipLevel == 0)
                    desc.MipLevel = CalcMipLevel(layers[0].Width, layers[0].Height, true, 4);

                var encoder = new BcEncoder();
                encoder.OutputOptions.GenerateMipMaps = true;
                encoder.OutputOptions.Quality = CompressionQuality.BestQuality;
                encoder.OutputOptions.Format = CompressionFormat.Bc6U;
                encoder.OutputOptions.FileFormat = OutputFileFormat.Dds;

                for (int faceIdx = 0; faceIdx < layers.Count; faceIdx++)
                {
                    var layer = layers[faceIdx];
                    int sliceSize = layer.Width * layer.Height;
                    var colorData = new ColorRgbFloat[sliceSize];
                    for (int i = 0; i < sliceSize; i++)
                    {
                        colorData[i].r = layer.Pixels[i].Red;
                        colorData[i].g = layer.Pixels[i].Green;
                        colorData[i].b = layer.Pixels[i].Blue;
                    }
                    var memory2D = colorData.AsMemory().AsMemory2D(layer.Height, layer.Width);
                    var pixelsBcnMips = encoder.EncodeToRawBytesHdr(memory2D);

                    var faceNode = mipsNode.GetOrAddNode($"Face{faceIdx}", 0, 0, true);
                    for (int mip = 0; mip < desc.MipLevel && mip < pixelsBcnMips.Length; mip++)
                    {
                        var pixelsBcn = pixelsBcnMips[mip];
                        var mipSize = new Vector3i();
                        var blockDimension = new Vector2i();
                        encoder.CalculateMipMapSize(layer.Width, layer.Height, mip, out mipSize.X, out mipSize.Y);
                        encoder.GetBlockCount(mipSize.X, mipSize.Y, out blockDimension.X, out blockDimension.Y);

                        if (faceIdx == 0)
                        {
                            desc.BlockSize = encoder.GetBlockSize();
                            desc.MipSizes.Add(mipSize);
                            desc.BlockDimenstions.Add(blockDimension);
                        }

                        var attr = faceNode.GetOrAddAttribute($"DxtMip{mip}", 0, 0, true);
                        using (var ar = attr.GetWriter((ulong)pixelsBcn.Length))
                        {
                            ar.WriteNoSize(pixelsBcn, pixelsBcn.Length);
                        }
                    }
                }
            }
            else
            {
                // LDR DXT/ETC2 compression
                var encoder = new BCnEncoder.Encoder.BcEncoder();
                encoder.OutputOptions.GenerateMipMaps = true;
                encoder.OutputOptions.Quality = CompressionQuality.Balanced;
                encoder.OutputOptions.FileFormat = OutputFileFormat.Dds;

                bool isKtx = false;
                switch (desc.CompressFormat)
                {
                    case ETextureCompressFormat.TCF_BC1:
                        desc.Format = desc.sRGB ? EPixelFormat.PXF_BC1_UNORM_SRGB : EPixelFormat.PXF_BC1_UNORM;
                        encoder.OutputOptions.Format = CompressionFormat.Bc1;
                        break;
                    case ETextureCompressFormat.TCF_BC1A:
                        desc.Format = desc.sRGB ? EPixelFormat.PXF_BC1_UNORM_SRGB : EPixelFormat.PXF_BC1_UNORM;
                        encoder.OutputOptions.Format = CompressionFormat.Bc1WithAlpha;
                        break;
                    case ETextureCompressFormat.TCF_BC2:
                        desc.Format = desc.sRGB ? EPixelFormat.PXF_BC2_UNORM_SRGB : EPixelFormat.PXF_BC2_UNORM;
                        encoder.OutputOptions.Format = CompressionFormat.Bc2;
                        break;
                    case ETextureCompressFormat.TCF_BC3:
                        desc.Format = desc.sRGB ? EPixelFormat.PXF_BC3_UNORM_SRGB : EPixelFormat.PXF_BC3_UNORM;
                        encoder.OutputOptions.Format = CompressionFormat.Bc3;
                        break;
                    case ETextureCompressFormat.TCF_BC4:
                        desc.Format = EPixelFormat.PXF_BC4_UNORM;
                        encoder.OutputOptions.Format = CompressionFormat.Bc4;
                        break;
                    case ETextureCompressFormat.TCF_BC5:
                        desc.Format = EPixelFormat.PXF_BC5_UNORM;
                        encoder.OutputOptions.Format = CompressionFormat.Bc5;
                        break;
                    case ETextureCompressFormat.TCF_BC6:
                        desc.Format = EPixelFormat.PXF_BC6H_UF16;
                        encoder.OutputOptions.Format = CompressionFormat.Bc6U;
                        break;
                    case ETextureCompressFormat.TCF_BC6_FLOAT:
                        desc.Format = EPixelFormat.PXF_BC6H_SF16;
                        encoder.OutputOptions.Format = CompressionFormat.Bc6S;
                        break;
                    case ETextureCompressFormat.TCF_Etc2_RGB8:
                        desc.Format = desc.sRGB ? EPixelFormat.PXF_ETC2_SRGB8 : EPixelFormat.PXF_ETC2_RGB8;
                        encoder.OutputOptions.Format = CompressionFormat.Atc;
                        isKtx = false;
                        break;
                    case ETextureCompressFormat.TCF_Etc2_RGBA1:
                        desc.Format = desc.sRGB ? EPixelFormat.PXF_ETC2_SRGBA1 : EPixelFormat.PXF_ETC2_RGBA1;
                        encoder.OutputOptions.Format = CompressionFormat.AtcExplicitAlpha;
                        isKtx = false;
                        break;
                    case ETextureCompressFormat.TCF_Etc2_RGBA8:
                        desc.Format = desc.sRGB ? EPixelFormat.PXF_ETC2_SRGBA8 : EPixelFormat.PXF_ETC2_RGBA8;
                        encoder.OutputOptions.Format = CompressionFormat.AtcInterpolatedAlpha;
                        isKtx = false;
                        break;
                    default:
                        desc.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
                        encoder.OutputOptions.Format = CompressionFormat.Bc1;
                        break;
                }

                if (desc.MipLevel == 0)
                    desc.MipLevel = CalcMipLevel(layers[0].Width, layers[0].Height, true, 4);

                for (int faceIdx = 0; faceIdx < layers.Count; faceIdx++)
                {
                    var curImage = Color4fLayerToMemImage(layers[faceIdx].Pixels, layers[faceIdx].Width, layers[faceIdx].Height);
                    var faceNode = mipsNode.GetOrAddNode($"Face{faceIdx}", 0, 0, true);

                    var pixelFormat = PixelFormat.Rgba32;
                    var taskArray = new System.Threading.Tasks.Task<byte[]>[desc.MipLevel];
                    for (int mip = 0; mip < desc.MipLevel; mip++)
                    {
                        taskArray[mip] = encoder.EncodeToRawBytesAsync(
                            curImage.Data, curImage.Width, curImage.Height, pixelFormat, mip);
                    }
                    System.Threading.Tasks.Task.WaitAll(taskArray);

                    for (int mip = 0; mip < desc.MipLevel; mip++)
                    {
                        var pixelsBcn = taskArray[mip].Result;
                        var mipSize = new Vector3i();
                        var blockDimension = new Vector2i();
                        encoder.CalculateMipMapSize(curImage.Width, curImage.Height, mip, out mipSize.X, out mipSize.Y);
                        encoder.GetBlockCount(mipSize.X, mipSize.Y, out blockDimension.X, out blockDimension.Y);

                        if (faceIdx == 0)
                        {
                            desc.BlockSize = encoder.GetBlockSize();
                            desc.MipSizes.Add(mipSize);
                            desc.BlockDimenstions.Add(blockDimension);
                        }

                        string attrName = isKtx ? $"EtcMip{mip}" : $"DxtMip{mip}";
                        var attr = faceNode.GetOrAddAttribute(attrName, 0, 0, true);
                        using (var ar = attr.GetWriter((ulong)pixelsBcn.Length))
                        {
                            ar.WriteNoSize(pixelsBcn, pixelsBcn.Length);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Save 3D texture layers into XND with per-slice mip chains.
        /// </summary>
        private unsafe void Save3DLayerToXnd(
            XndNode node,
            TtTextureUtility.TtTex3dLayer layer3d,
            TtPicDesc desc,
            bool isHdr)
        {
            desc.MipSizes.Clear();
            desc.BlockDimenstions.Clear();

            if (desc.MipLevel == 0)
                desc.MipLevel = CalcMipLevel(
                    Math.Min(layer3d.Width, Math.Min(layer3d.Height, layer3d.Depth)),
                    Math.Min(layer3d.Width, Math.Min(layer3d.Height, layer3d.Depth)),
                    true, 4);
            desc.MipLevel = Math.Max(desc.MipLevel, 1);

            if (isHdr)
            {
                desc.CompressFormat = TtTextureHelper.SelectHdrCompressFormat(desc);
                if (desc.CompressFormat == ETextureCompressFormat.TCF_None)
                {
                    desc.Format = EPixelFormat.PXF_R32G32B32A32_FLOAT;
                    var hdrMipsNode = node.GetOrAddNode("HdrMips", 0, 0, true);
                    var faceNode = hdrMipsNode.GetOrAddNode("Face0", 0, 0, true);
                    var depthSlicesNode = faceNode.GetOrAddNode("DepthSlices", 0, 0, true);

                    // Generate mip chains: Mip0 from original, subsequent mips from downsampled 3D
                    var sourceMip = layer3d;
                    for (int mip = 0; mip < desc.MipLevel; mip++)
                    {
                        int mipDepth = sourceMip.Depth;
                        var mipNode = depthSlicesNode.GetOrAddNode($"Mip{mip}", 0, 0, true);

                        for (int d = 0; d < mipDepth; d++)
                        {
                            int slicePixelCount = sourceMip.Width * sourceMip.Height;
                            var slicePixels = new Color4f[slicePixelCount];
                            Array.Copy(sourceMip.Pixels, d * slicePixelCount, slicePixels, 0, slicePixelCount);
                            var sliceImage = Color4fLayerToImageFloat(slicePixels, sourceMip.Width, sourceMip.Height);

                            using (var memStream = new System.IO.MemoryStream())
                            {
                                var writer = new StbImageWriteSharp.ImageWriter();
                                var writeComp = UStbImageUtility.ConvertColorComponent(sliceImage.Comp);
                                fixed (void* ptr = sliceImage.Data)
                                {
                                    writer.WriteHdr(ptr, sliceImage.Width, sliceImage.Height, writeComp, memStream);
                                }
                                var hdrData = memStream.ToArray();
                                var attr = mipNode.GetOrAddAttribute($"HdrMipSlice{d}", 0, 0, true);
                                using (var ar = attr.GetWriter((ulong)memStream.Position))
                                {
                                    ar.WriteNoSize(hdrData, (int)memStream.Position);
                                }
                            }

                            if (d == 0)
                            {
                                desc.MipSizes.Add(new Vector3i()
                                {
                                    X = sourceMip.Width,
                                    Y = sourceMip.Height,
                                    Z = mipDepth
                                });
                            }
                        }

                        // Generate next mip level
                        int nextW = Math.Max(1, sourceMip.Width / 2);
                        int nextH = Math.Max(1, sourceMip.Height / 2);
                        int nextD = Math.Max(1, sourceMip.Depth / 2);
                        if (nextW == sourceMip.Width && nextH == sourceMip.Height && nextD == sourceMip.Depth)
                            break;
                        sourceMip = TtTextureUtility.GenerateMipLayer3D(sourceMip, nextW, nextH, nextD);
                    }
                }
                else
                {
                    // BC6 compressed 3D texture: save per-slice as DXT using EncodeToRawBytesHdr
                    desc.Format = EPixelFormat.PXF_BC6H_UF16;
                    var dxtMipsNode = node.GetOrAddNode("DxtMips", 0, 0, true);
                    var faceNode = dxtMipsNode.GetOrAddNode("Face0", 0, 0, true);
                    var depthSlicesNode = faceNode.GetOrAddNode("DepthSlices", 0, 0, true);

                    var encoder = new BcEncoder();
                    encoder.OutputOptions.GenerateMipMaps = false;
                    encoder.OutputOptions.Quality = CompressionQuality.BestQuality;
                    encoder.OutputOptions.Format = CompressionFormat.Bc6U;
                    encoder.OutputOptions.FileFormat = OutputFileFormat.Dds;

                    var sourceMip = layer3d;
                    for (int mip = 0; mip < desc.MipLevel; mip++)
                    {
                        int mipDepth = sourceMip.Depth;
                        var mipNode = depthSlicesNode.GetOrAddNode($"Mip{mip}", 0, 0, true);

                        for (int d = 0; d < mipDepth; d++)
                        {
                            int slicePixelCount = sourceMip.Width * sourceMip.Height;
                            var colorData = new ColorRgbFloat[slicePixelCount];
                            int baseOffset = d * slicePixelCount;
                            for (int i = 0; i < slicePixelCount; i++)
                            {
                                colorData[i].r = sourceMip.Pixels[baseOffset + i].Red;
                                colorData[i].g = sourceMip.Pixels[baseOffset + i].Green;
                                colorData[i].b = sourceMip.Pixels[baseOffset + i].Blue;
                            }
                            var memory2D = colorData.AsMemory().AsMemory2D(sourceMip.Height, sourceMip.Width);
                            var pixelsBcn = encoder.EncodeToRawBytesHdr(memory2D, 0,
                                out int mipW, out int mipH);

                            var attr = mipNode.GetOrAddAttribute($"DxtMipSlice{d}", 0, 0, true);
                            using (var ar = attr.GetWriter((ulong)pixelsBcn.Length))
                            {
                                ar.WriteNoSize(pixelsBcn, pixelsBcn.Length);
                            }

                            if (d == 0)
                            {
                                var blockDimension = new Vector2i();
                                encoder.GetBlockCount(mipW, mipH,
                                    out blockDimension.X, out blockDimension.Y);
                                desc.BlockSize = encoder.GetBlockSize();
                                desc.MipSizes.Add(new Vector3i()
                                {
                                    X = sourceMip.Width,
                                    Y = sourceMip.Height,
                                    Z = mipDepth
                                });
                                desc.BlockDimenstions.Add(blockDimension);
                            }
                        }

                        int nextW = Math.Max(1, sourceMip.Width / 2);
                        int nextH = Math.Max(1, sourceMip.Height / 2);
                        int nextD = Math.Max(1, sourceMip.Depth / 2);
                        if (nextW == sourceMip.Width && nextH == sourceMip.Height && nextD == sourceMip.Depth)
                            break;
                        sourceMip = TtTextureUtility.GenerateMipLayer3D(sourceMip, nextW, nextH, nextD);
                    }
                }
            }
            else
            {
                // LDR 3D texture
                desc.CompressFormat = TtTextureHelper.SelectLdrCompressFormat(desc);
                if (desc.CompressFormat == ETextureCompressFormat.TCF_None)
                {
                    desc.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
                    var pngMipsNode = node.GetOrAddNode("PngMips", 0, 0, true);
                    var faceNode = pngMipsNode.GetOrAddNode("Face0", 0, 0, true);
                    var depthSlicesNode = faceNode.GetOrAddNode("DepthSlices", 0, 0, true);

                    var sourceMip = layer3d;
                    for (int mip = 0; mip < desc.MipLevel; mip++)
                    {
                        int mipDepth = sourceMip.Depth;
                        var mipNode = depthSlicesNode.GetOrAddNode($"Mip{mip}", 0, 0, true);

                        for (int d = 0; d < mipDepth; d++)
                        {
                            int slicePixelCount = sourceMip.Width * sourceMip.Height;
                            var slicePixels = new Color4f[slicePixelCount];
                            Array.Copy(sourceMip.Pixels, d * slicePixelCount, slicePixels, 0, slicePixelCount);
                            var sliceImage = Color4fLayerToMemImage(slicePixels, sourceMip.Width, sourceMip.Height);

                            using (var memStream = new System.IO.MemoryStream(sliceImage.Data.Length))
                            {
                                var writer = new StbImageWriteSharp.ImageWriter();
                                writer.WritePng(sliceImage.Data, sliceImage.Width, sliceImage.Height,
                                    StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, memStream);
                                var pngData = memStream.ToArray();
                                var attr = mipNode.GetOrAddAttribute($"PngMipSlice{d}", 0, 0, true);
                                using (var ar = attr.GetWriter((ulong)memStream.Position))
                                {
                                    ar.WriteNoSize(pngData, (int)memStream.Position);
                                }
                            }

                            if (d == 0)
                            {
                                desc.MipSizes.Add(new Vector3i()
                                {
                                    X = sourceMip.Width,
                                    Y = sourceMip.Height,
                                    Z = mipDepth
                                });
                            }
                        }

                        int nextW = Math.Max(1, sourceMip.Width / 2);
                        int nextH = Math.Max(1, sourceMip.Height / 2);
                        int nextD = Math.Max(1, sourceMip.Depth / 2);
                        if (nextW == sourceMip.Width && nextH == sourceMip.Height && nextD == sourceMip.Depth)
                            break;
                        sourceMip = TtTextureUtility.GenerateMipLayer3D(sourceMip, nextW, nextH, nextD);
                    }
                }
                else
                {
                    // Block-compressed LDR 3D: save per-slice with DXT
                    var dxtMipsNode = node.GetOrAddNode("DxtMips", 0, 0, true);
                    var faceNode = dxtMipsNode.GetOrAddNode("Face0", 0, 0, true);
                    var depthSlicesNode = faceNode.GetOrAddNode("DepthSlices", 0, 0, true);

                    // Set pixel format based on compress format
                    switch (desc.CompressFormat)
                    {
                        case ETextureCompressFormat.TCF_BC1:
                            desc.Format = desc.sRGB ? EPixelFormat.PXF_BC1_UNORM_SRGB : EPixelFormat.PXF_BC1_UNORM;
                            break;
                        case ETextureCompressFormat.TCF_BC1A:
                            desc.Format = desc.sRGB ? EPixelFormat.PXF_BC1_UNORM_SRGB : EPixelFormat.PXF_BC1_UNORM;
                            break;
                        case ETextureCompressFormat.TCF_BC2:
                            desc.Format = desc.sRGB ? EPixelFormat.PXF_BC2_UNORM_SRGB : EPixelFormat.PXF_BC2_UNORM;
                            break;
                        case ETextureCompressFormat.TCF_BC3:
                            desc.Format = desc.sRGB ? EPixelFormat.PXF_BC3_UNORM_SRGB : EPixelFormat.PXF_BC3_UNORM;
                            break;
                        case ETextureCompressFormat.TCF_BC4:
                            desc.Format = EPixelFormat.PXF_BC4_UNORM;
                            break;
                        case ETextureCompressFormat.TCF_BC5:
                            desc.Format = EPixelFormat.PXF_BC5_UNORM;
                            break;
                        case ETextureCompressFormat.TCF_BC6:
                            desc.Format = EPixelFormat.PXF_BC6H_UF16;
                            break;
                        case ETextureCompressFormat.TCF_BC6_FLOAT:
                            desc.Format = EPixelFormat.PXF_BC6H_SF16;
                            break;
                        default:
                            desc.Format = desc.sRGB ? EPixelFormat.PXF_BC1_UNORM_SRGB : EPixelFormat.PXF_BC1_UNORM;
                            break;
                    }

                    var encoder = new BCnEncoder.Encoder.BcEncoder();
                    encoder.OutputOptions.GenerateMipMaps = false;
                    encoder.OutputOptions.Quality = CompressionQuality.Balanced;
                    switch (desc.CompressFormat)
                    {
                        case ETextureCompressFormat.TCF_BC1:
                            encoder.OutputOptions.Format = CompressionFormat.Bc1;
                            break;
                        case ETextureCompressFormat.TCF_BC1A:
                            encoder.OutputOptions.Format = CompressionFormat.Bc1WithAlpha;
                            break;
                        case ETextureCompressFormat.TCF_BC2:
                            encoder.OutputOptions.Format = CompressionFormat.Bc2;
                            break;
                        case ETextureCompressFormat.TCF_BC3:
                            encoder.OutputOptions.Format = CompressionFormat.Bc3;
                            break;
                        case ETextureCompressFormat.TCF_BC4:
                            encoder.OutputOptions.Format = CompressionFormat.Bc4;
                            break;
                        case ETextureCompressFormat.TCF_BC5:
                            encoder.OutputOptions.Format = CompressionFormat.Bc5;
                            break;
                        case ETextureCompressFormat.TCF_BC6:
                            encoder.OutputOptions.Format = CompressionFormat.Bc6U;
                            break;
                        case ETextureCompressFormat.TCF_BC6_FLOAT:
                            encoder.OutputOptions.Format = CompressionFormat.Bc6S;
                            break;
                        default:
                            encoder.OutputOptions.Format = CompressionFormat.Bc1;
                            break;
                    }

                    var sourceMip = layer3d;
                    for (int mip = 0; mip < desc.MipLevel; mip++)
                    {
                        int mipDepth = sourceMip.Depth;
                        var mipNode = depthSlicesNode.GetOrAddNode($"Mip{mip}", 0, 0, true);

                        for (int d = 0; d < mipDepth; d++)
                        {
                            int slicePixelCount = sourceMip.Width * sourceMip.Height;
                            var slicePixels = new Color4f[slicePixelCount];
                            Array.Copy(sourceMip.Pixels, d * slicePixelCount, slicePixels, 0, slicePixelCount);
                            var sliceImage = Color4fLayerToMemImage(slicePixels, sourceMip.Width, sourceMip.Height);

                            var pixelsBcn = encoder.EncodeToRawBytes(
                                sliceImage.Data.AsSpan(), sliceImage.Width, sliceImage.Height,
                                PixelFormat.Rgba32, 0,
                                out int mipW, out int mipH);

                            var attr = mipNode.GetOrAddAttribute($"DxtMipSlice{d}", 0, 0, true);
                            using (var ar = attr.GetWriter((ulong)pixelsBcn.Length))
                            {
                                ar.WriteNoSize(pixelsBcn, pixelsBcn.Length);
                            }

                            if (d == 0)
                            {
                                var blockDimension = new Vector2i();
                                encoder.GetBlockCount(sourceMip.Width, sourceMip.Height,
                                    out blockDimension.X, out blockDimension.Y);
                                desc.BlockSize = encoder.GetBlockSize();
                                desc.MipSizes.Add(new Vector3i()
                                {
                                    X = sourceMip.Width,
                                    Y = sourceMip.Height,
                                    Z = mipDepth
                                });
                                desc.BlockDimenstions.Add(blockDimension);
                            }
                        }

                        int nextW = Math.Max(1, sourceMip.Width / 2);
                        int nextH = Math.Max(1, sourceMip.Height / 2);
                        int nextD = Math.Max(1, sourceMip.Depth / 2);
                        if (nextW == sourceMip.Width && nextH == sourceMip.Height && nextD == sourceMip.Depth)
                            break;
                        sourceMip = TtTextureUtility.GenerateMipLayer3D(sourceMip, nextW, nextH, nextD);
                    }
                }
            }

            TtTextureHelper.SaveDescToNode(node, desc);
        }

        /// <summary>
        /// Save this SRV's texture data as a texture asset by reading back from GPU.
        /// Supports 2D, 2DArray, 3D, and CubeMap textures.
        /// Compression is determined by TtEngine.Instance.GfxDevice.Config.TextureAssetCompressType.
        /// Intended to replace the old SaveAssetTo that requires original source images.
        /// </summary>
        /// <param name="name">The RName for the asset to save.</param>
        /// <returns>True if save succeeded.</returns>
        public void SaveAssetTo(RName name)
        {
            var ameta = this.GetAMeta() as TtSrViewAMeta;
            if (ameta != null)
            {
                UpdateAMetaReferences(ameta);
                ameta.SaveAMeta(this);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, "Failed to save asset: AMeta is null or of wrong type.");
            }

            var texture = GetTexture();
            if (!texture.IsValidPointer)
            {
                Profiler.Log.WriteLine<Profiler.TtAssetGategory>(Profiler.ELogTag.Error, $"Failed to save asset{name}: texture is null or invalid.");
                return;
            }

            var texDesc = texture.Desc;
            var format = texDesc.Format;
            int width = (int)texDesc.Width;
            int height = (int)texDesc.Height;
            int depth = (int)texDesc.Depth;
            bool isHdr = IsHdrPixelFormat(format);

            var srvType = mCoreObject.Desc.Type;

            using (var xnd = new IO.TtXndHolder("TtSrView", 0, 0))
            {
                bool saveResult = false;

                switch (srvType)
                {
                    case ESrvType.ST_TextureCube:
                        saveResult = SaveCubeAsset(xnd.RootNode.mCoreObject, format, width, height, isHdr, name);
                        break;

                    case ESrvType.ST_Texture3D:
                        saveResult = Save3DAsset(xnd.RootNode.mCoreObject, format, width, height, depth, isHdr, name);
                        break;

                    default: // ST_Texture2D — may be single 2D or 2DArray
                        saveResult = Save2DAsset(xnd.RootNode.mCoreObject, format, width, height, texDesc.ArraySize, isHdr, name);
                        break;
                }

                if (!saveResult)
                {
                    Profiler.Log.WriteLine<Profiler.TtAssetGategory>(Profiler.ELogTag.Error, $"Failed to save asset{name}: saveResult is false.");
                    return;
                }

                xnd.SaveXnd(name.Address);
                TtEngine.Instance.AssetMetaManager.RegAsset(ameta);
                TtEngine.Instance.SourceControlModule.AddFile(name.Address);
            }
        }

        /// <summary>
        /// Re-compress and save this texture asset from uncompressed source data.
        /// Useful when switching target platform (e.g. DXT → ASTC) or changing quality settings.
        /// Loads source image via LoadUncompressImageLDR/HDR, then calls SaveTexture with current engine config.
        /// </summary>
        /// <summary>
        /// Re-cook this texture asset for the current platform.
        /// Invalidates the existing .txc cache and regenerates compressed data.
        /// For new-format .srv (with RawSource), uses TtTextureCookManager directly.
        /// For legacy .srv, falls back to LoadUncompressImage + SaveTexture.
        /// </summary>
        public bool CookAsset(RName name)
        {
            // Try the new CookManager path (works for new-format .srv with RawSource)
            if (TtTextureCookManager.IsCookValid(name))
                return true;
            if (TtTextureCookManager.Cook(name))
                return true;

            // Fallback for legacy .srv (no RawSource node): use LoadUncompressImage → SaveTexture
            var desc = this.PicDesc;
            if (desc == null)
                return false;

            desc.CompressFormat = SelectCompressFormat(desc);
            var cookedPath = TtTextureCookManager.GetCookedPath(name);
            if (cookedPath == null)
                return false;

            using (var xnd = new IO.TtXndHolder("CookedTexture", 0, 0))
            {
                if (desc.IsHdr())
                {
                    var imageFloat = LoadUncompressImageHDR();
                    if (imageFloat == null)
                    {
                        Profiler.Log.WriteLine<Profiler.TtAssetGategory>(Profiler.ELogTag.Warning,
                            $"CookAsset({name}): failed to load HDR source image");
                        return false;
                    }

                    if (desc.CubeFaces == 6)
                    {
                        StbImageSharp.ImageResultFloat cubeImage = null;
                        TtTextureHelper.GenerateBaseCubeMipFromLongitudeLatitude2D(ref cubeImage, imageFloat, 512);
                        imageFloat = cubeImage;
                    }

                    CookTextureTo(name, xnd.RootNode.mCoreObject, imageFloat, desc);
                }
                else
                {
                    var ldrImage = LoadUncompressImageLDR();
                    if (ldrImage == null)
                    {
                        Profiler.Log.WriteLine<Profiler.TtAssetGategory>(Profiler.ELogTag.Warning,
                            $"CookAsset({name}): failed to load LDR source image");
                        return false;
                    }

                    CookTextureTo(name, xnd.RootNode.mCoreObject, ldrImage, desc);
                }

                // Write platform and timestamp metadata
                var platformAttr = xnd.RootNode.mCoreObject.GetOrAddAttribute("Platform", 0, 0, true);
                using (var aw = platformAttr.GetWriter(64))
                {
                    aw.Write(TtEngine.Instance.GfxDevice.Config.TextureAssetCompressType.ToString());
                }
                var tsAttr = xnd.RootNode.mCoreObject.GetOrAddAttribute("SourceTimestamp", 0, 0, true);
                using (var aw = tsAttr.GetWriter(8))
                {
                    long ts = System.IO.File.Exists(name.Address)
                        ? System.IO.File.GetLastWriteTimeUtc(name.Address).Ticks : 0;
                    aw.Write(ts);
                }

                xnd.SaveXnd(cookedPath);
            }

            return true;
        }

        private bool SaveCubeAsset(XndNode node, EPixelFormat format, int width, int height, bool isHdr, RName assetName)
        {
            var cubeList = ReadbackTexCubeArray(0);
            if (cubeList == null || cubeList.Count == 0)
                return false;

            // Take first cube (most common case; CubeArray with multiple cubes is rare for asset saving)
            var cubeLayer = cubeList[0];

            // Convert 6-face cube into a list of 6 TtTex2dLayer (one per face)
            int facePixelCount = cubeLayer.Width * cubeLayer.Height;
            var faceLayers = new List<TtTextureUtility.TtTex2dLayer>(6);
            for (int face = 0; face < 6; face++)
            {
                var faceLayer = new TtTextureUtility.TtTex2dLayer();
                faceLayer.Width = cubeLayer.Width;
                faceLayer.Height = cubeLayer.Height;
                faceLayer.Pixels = new Color4f[facePixelCount];
                Array.Copy(cubeLayer.Pixels, face * facePixelCount, faceLayer.Pixels, 0, facePixelCount);
                faceLayers.Add(faceLayer);
            }

            var desc = BuildPicDescFromTexture(format, width, height, 0, 6);
            SaveLayersToXnd(node, faceLayers, desc, isHdr, assetName);
            return true;
        }

        private bool Save3DAsset(XndNode node, EPixelFormat format, int width, int height, int depth, bool isHdr, RName assetName)
        {
            var layer3d = ReadbackTex3D(0);
            if (layer3d == null)
                return false;

            var desc = BuildPicDescFromTexture(format, width, height, depth, 1);
            Save3DLayerToXnd(node, layer3d, desc, isHdr);
            return true;
        }

        private bool Save2DAsset(XndNode node, EPixelFormat format, int width, int height, uint arraySize, bool isHdr, RName assetName)
        {
            var layers = ReadbackTex2DArray(0);
            if (layers == null || layers.Count == 0)
                return false;

            int cubeFaces = (int)arraySize;
            var desc = BuildPicDescFromTexture(format, width, height, 0, (uint)cubeFaces);
            desc.IsNormal = this.PicDesc.IsNormal;
            SaveLayersToXnd(node, layers, desc, isHdr, assetName);
            return true;
        }

        #endregion

        internal static ETextureCompressFormat SelectCompressFormat(TtPicDesc desc)
        {
            if (desc.DontCompress)
            {
                return ETextureCompressFormat.TCF_None;
            }

            var config = TtEngine.Instance?.GfxDevice.Config;
            switch (config.TextureAssetCompressType)
            {
                case Graphics.Pipeline.TtGfxDeviceConfig.ETextureAssetCompressType.DXT:
                    return SelectDxtFormat(desc);
                case Graphics.Pipeline.TtGfxDeviceConfig.ETextureAssetCompressType.ETC2:
                    return SelectEtc2Format(desc);
                case Graphics.Pipeline.TtGfxDeviceConfig.ETextureAssetCompressType.ASTC:
                    return SelectAstcFormat(desc);
                default:
                    return ETextureCompressFormat.TCF_None;
            }
        }

        public static unsafe TtSrView ImportImage(System.IO.Stream stream, ImportAttribute importer, bool bSaveAsset)
        {
            var extName = IO.TtFileManager.GetExtName(importer.mSourceFile).ToLower();
            var rn = RName.GetRName(importer.mDir.Name + importer.mName + TtSrView.AssetExt, importer.mDir.RNameType);
            var desc = importer.mDesc;
            desc.CompressFormat = SelectCompressFormat(desc);

            var ameta = rn.AMeta as TtSrViewAMeta;
            if (ameta == null)
            {
                ameta = new TtSrViewAMeta();
                ameta.SetAssetName(rn);
                ameta.AssetId = Guid.NewGuid();
                ameta.TypeStr = Rtti.TtTypeDesc.TypeOf(typeof(TtSrView)).TypeString;
                ameta.Description = $"This is a {typeof(TtSrView).FullName}\n";
                ameta.OriginImageAddress = importer.mSourceFile;

                TtEngine.Instance.AssetMetaManager.RegAsset(ameta, true);
            }

            // Save directly from source pixels (avoids GPU readback → re-compress lossy roundtrip).
            // The old SaveAssetTo path does GPU readback → decode → re-encode which introduces
            // artifacts and data corruption for block-compressed textures.
            if (SaveAssetDirect(importer, rn, desc, ameta) == false)
            {
                return null;
            }

            TtEngine.Instance.GfxDevice.TextureManager.UnsafeRemove(rn);
            //TtEngine.Instance.GfxDevice.TextureManager.UnsafeAdd(rn, result);
            var result = TtEngine.Instance.GfxDevice.TextureManager.GetTexture(rn, desc.Desc.MipLevel).GetResultUntilCompleted();

            return result;
        }

        /// <summary>
        /// Save imported texture asset: .srv stores only raw source data (RawSource node + Desc).
        /// Then triggers a cook to generate the platform-compressed .txc cache.
        /// </summary>
        private static bool SaveAssetDirect(ImportAttribute importer, RName rn, TtPicDesc desc, TtSrViewAMeta ameta)
        {
            using (var xnd = new IO.TtXndHolder("TtSrView", 0, 0))
            {
                // Save raw source data to "RawSource" node (no BC compression)
                if (System.IO.File.Exists(importer.mSourceFile) == false)
                {
                    Profiler.Log.WriteLine<Profiler.TtAssetGategory>(Profiler.ELogTag.Warning, $"Failed to save asset{rn}: source file {importer.mSourceFile} does not exist.");
                    return false;
                }
                try
                {
                    using (var srcStream = System.IO.File.OpenRead(importer.mSourceFile))
                    {
                        var extName = IO.TtFileManager.GetExtName(importer.mSourceFile).ToLower();
                        if (extName == ".hdr")
                        {
                            var imageFloat = StbImageSharp.ImageResultFloat.FromStream(srcStream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                            desc.Width = imageFloat.Width;
                            desc.Height = imageFloat.Height;
                            TtTextureCookManager.SaveHdrToRawNode(xnd.RootNode.mCoreObject, imageFloat);
                        }
                        else if (extName == ".exr")
                        {
                            var exrBytes = new byte[srcStream.Length];
                            srcStream.Read(exrBytes, 0, exrBytes.Length);
                            srcStream.Position = 0;
                            var exrFile = new Jither.OpenEXR.EXRFile(srcStream);
                            int w = exrFile.Parts[0].DisplayWindow.Width;
                            int h = exrFile.Parts[0].DisplayWindow.Height;
                            desc.Width = w;
                            desc.Height = h;
                            TtTextureCookManager.SaveExrToRawNode(xnd.RootNode.mCoreObject, exrBytes, w, h);
                        }
                        else
                        {
                            var image = StbImageSharp.TtMemImage.FromStream(srcStream, StbImageSharp.ColorComponents.Default);
                            if (image != null)
                            {
                                desc.Width = image.Width;
                                desc.Height = image.Height;
                                TtTextureCookManager.SaveLdrToRawNode(xnd.RootNode.mCoreObject, image);
                            }
                        }
                    }

                    // Save PicDesc metadata
                    TtTextureHelper.SaveDescToNode(xnd.RootNode.mCoreObject, desc);

                    xnd.SaveXnd(rn.Address);
                    TtEngine.Instance.SourceControlModule.AddFile(rn.Address);
                }
                catch (Exception ex)
                {
                    Profiler.Log.WriteException(ex);
                    return false;
                }
                
            }

            if (ameta != null)
            {
                rn.AMeta?.AddAssetFile(rn.Address);
                ameta.SaveAMeta((IAsset)null);
            }

            // Trigger cook to generate .txc cache for current platform
            return TtTextureCookManager.Cook(rn);
        }
    }
}
