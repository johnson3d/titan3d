using System;
using System.Collections.Generic;
using System.Text;
using BCnEncoder.Decoder;
using BCnEncoder.Shared;
using StbImageWriteSharp;

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
            var decoded = decoder.DecodeRaw(compactData, width, height, bcnFormat.Value);

            // ColorRgba32 → Color4f
            for (int i = 0; i < decoded.Length && i < layer.Pixels.Length; i++)
            {
                var c = decoded[i];
                layer.Pixels[i] = new Color4f(c.r / 255f, c.g / 255f, c.b / 255f, c.a / 255f);
            }
        }

        #endregion

        #region Readback Methods

        /// <summary>
        /// Read back a single 2D subresource from GPU and return it as a TtTex2dLayer.
        /// Handles both uncompressed and BC block-compressed formats.
        /// ASTC formats are not yet supported and will return null.
        /// </summary>
        private unsafe TtTextureUtility.TtTex2dLayer ReadbackSubresource2D(
            ITexture texture, uint subResource, int width, int height, EPixelFormat format)
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            var blob = new Support.TtBlobObject();
            bool ok = texture.FetchGpuData(rc.mCoreObject, subResource, blob.mCoreObject);
            if (!ok)
                return null;

            var layer = new TtTextureUtility.TtTex2dLayer();
            layer.Width = width;
            layer.Height = height;
            layer.Pixels = new Color4f[width * height];

            var pData = (byte*)blob.DataPointer;
            uint rowPitch = *(uint*)pData;
            pData += sizeof(uint) + sizeof(uint); // skip RowPitch + DepthPitch header

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
            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            var blob = new Support.TtBlobObject();
            bool ok = texture.FetchGpuData(rc.mCoreObject, subResource, blob.mCoreObject);
            if (!ok)
                return null;

            var layer = new TtTextureUtility.TtTex3dLayer();
            layer.Width = mipWidth;
            layer.Height = mipHeight;
            layer.Depth = mipDepth;
            layer.Pixels = new Color4f[mipWidth * mipHeight * mipDepth];

            var pData = (byte*)blob.DataPointer;
            uint rowPitch = *(uint*)pData;
            uint depthPitch = *(uint*)(pData + sizeof(uint));
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
    }
}
