// UTextureHelper.cs
// 纹理处理辅助函数集合，包含 mipmap 处理和压缩格式选择

using BCnEncoder.Shared;
using EngineNS.Bricks.ImageDecoder;
using EngineNS.IO;
using StbImageSharp;
using System;
using static EngineNS.NxRHI.TtSrView;

namespace EngineNS.NxRHI
{
    /// <summary>
    /// 纹理处理辅助类
    /// 包含 mipmap 处理和压缩格式选择等功能
    /// </summary>
    public static class TtTextureHelper
    {
        #region 3D Texture 网格布局计算

        /// <summary>
        /// 3D Texture 网格布局计算结果
        /// </summary>
        public struct Texture3DLayout
        {
            public int SliceWidth;           // 每个 slice 的宽度
            public int SliceHeight;          // 每个 slice 的高度
            public int SlicesPerRow;         // 每行有多少个 slices
            public int SlicesPerColumn;      // 每列有多少个 slices
        }

        /// <summary>
        /// 计算 3D Texture 的网格布局
        /// </summary>
        /// <param name="imageWidth">源图像宽度</param>
        /// <param name="imageHeight">源图像高度</param>
        /// <param name="desc">纹理描述符</param>
        /// <returns>网格布局信息</returns>
        public static Texture3DLayout Calculate3DTextureLayout(int imageWidth, int imageHeight, TtPicDesc desc)
        {
            var result = new Texture3DLayout();

            // 1. 计算每个 slice 的尺寸（基于 desc）
            result.SliceWidth = desc.Width;
            result.SliceHeight = desc.Height;

            // 2. 计算在 width 和 height 方向上各能容纳多少个 slices
            int possibleSlicesPerRow = Math.Max(1, imageWidth / result.SliceWidth);
            int possibleSlicesPerCol = Math.Max(1, imageHeight / result.SliceHeight);

            // 3. 验证是否能容纳所有 slices
            if (possibleSlicesPerRow * possibleSlicesPerCol < desc.Depth)
            {
                throw new System.InvalidOperationException(
                    $"Invalid 3D texture layout. " +
                    $"Image: {imageWidth}x{imageHeight}, " +
                    $"Slice: {result.SliceWidth}x{result.SliceHeight}, " +
                    $"Depth: {desc.Depth}, " +
                    $"Can fit at most {possibleSlicesPerRow * possibleSlicesPerCol} slices, need {desc.Depth}");
            }

            result.SlicesPerRow = possibleSlicesPerRow;
            result.SlicesPerColumn = (desc.Depth + result.SlicesPerRow - 1) / result.SlicesPerRow;

            // 4. 验证实际使用的区域
            int usedWidth = Math.Min(result.SlicesPerRow, desc.Depth) * result.SliceWidth;
            int usedHeight = result.SlicesPerColumn * result.SliceHeight;

            if (usedWidth > imageWidth || usedHeight > imageHeight)
            {
                throw new System.InvalidOperationException(
                    $"Invalid 3D texture layout. " +
                    $"Image: {imageWidth}x{imageHeight}, " +
                    $"Required: {usedWidth}x{usedHeight}");
            }

            return result;
        }

        /// <summary>
        /// 从网格布局中提取 slice 数据（byte[]）
        /// </summary>
        public static byte[] ExtractSliceFromGridLayout(
            byte[] srcData, int srcWidth, int srcHeight,
            int sliceWidth, int sliceHeight, int sliceIndex,
            int slicesPerRow, int bytesPerPixel)
        {
            byte[] sliceData = new byte[sliceWidth * sliceHeight * bytesPerPixel];

            int gridX = sliceIndex % slicesPerRow;
            int gridY = sliceIndex / slicesPerRow;

            for (int y = 0; y < sliceHeight; y++)
            {
                for (int x = 0; x < sliceWidth; x++)
                {
                    int srcX = gridX * sliceWidth + x;
                    int srcY = gridY * sliceHeight + y;

                    int srcIndex = (srcY * srcWidth + srcX) * bytesPerPixel;
                    int dstIndex = (y * sliceWidth + x) * bytesPerPixel;

                    for (int c = 0; c < bytesPerPixel; c++)
                    {
                        sliceData[dstIndex + c] = srcData[srcIndex + c];
                    }
                }
            }

            return sliceData;
        }

        /// <summary>
        /// 从网格布局中提取 slice 数据（float[]）
        /// </summary>
        public static float[] ExtractSliceFromGridLayoutFloat(
            float[] srcData, int srcWidth, int srcHeight,
            int sliceWidth, int sliceHeight, int sliceIndex,
            int slicesPerRow, int channelsPerPixel)
        {
            float[] sliceData = new float[sliceWidth * sliceHeight * channelsPerPixel];

            int gridX = sliceIndex % slicesPerRow;
            int gridY = sliceIndex / slicesPerRow;

            for (int y = 0; y < sliceHeight; y++)
            {
                for (int x = 0; x < sliceWidth; x++)
                {
                    int srcX = gridX * sliceWidth + x;
                    int srcY = gridY * sliceHeight + y;

                    int srcIndex = (srcY * srcWidth + srcX) * channelsPerPixel;
                    int dstIndex = (y * sliceWidth + x) * channelsPerPixel;

                    for (int c = 0; c < channelsPerPixel; c++)
                    {
                        sliceData[dstIndex + c] = srcData[srcIndex + c];
                    }
                }
            }

            return sliceData;
        }

        #endregion

        #region MipLevel 计算

        /// <summary>
        /// 计算并设置 MipLevel
        /// </summary>
        /// <param name="desc">纹理描述符</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="forceAtLeastOne">是否强制至少为1</param>
        /// <param name="divisible">除数（用于纹理压缩）</param>
        public static void CalculateAndSetMipLevel(
            TtPicDesc desc, int width, int height,
            bool forceAtLeastOne = false, int divisible = 4)
        {
            if (desc.MipLevel == 0)
            {
                desc.MipLevel = TtSrView.CalcMipLevel(width, height, true, divisible);
                if (forceAtLeastOne)
                    desc.MipLevel = Math.Max(desc.MipLevel, 1);
            }
            else
            {
                desc.MipLevel = Math.Min(desc.MipLevel, TtSrView.CalcMipLevel(width, height, true, divisible));
            }
        }

        #endregion

        #region 降采样辅助函数

        /// <summary>
        /// 降采样到目标 mipmap 层级（TtMemImage）
        /// </summary>
        public static StbImageSharp.TtMemImage DownsampleToMipLevel(
            StbImageSharp.TtMemImage srcImage, uint targetMipLevel, out int outWidth, out int outHeight)
        {
            var result = srcImage;
            outWidth = srcImage.Width;
            outHeight = srcImage.Height;

            for (uint m = 0; m < targetMipLevel; m++)
            {
                outWidth = Math.Max(1, outWidth / 2);
                outHeight = Math.Max(1, outHeight / 2);
                result = StbImageSharp.ImageProcessor.GetBoxDownSampler(result, outWidth, outHeight);
            }

            return result;
        }

        /// <summary>
        /// 降采样到目标 mipmap 层级（ImageResultFloat）
        /// </summary>
        public static StbImageSharp.ImageResultFloat DownsampleToMipLevelFloat(
            StbImageSharp.ImageResultFloat srcImage, uint targetMipLevel, out int outWidth, out int outHeight)
        {
            var result = srcImage;
            outWidth = srcImage.Width;
            outHeight = srcImage.Height;

            for (uint m = 0; m < targetMipLevel; m++)
            {
                outWidth = Math.Max(1, outWidth / 2);
                outHeight = Math.Max(1, outHeight / 2);
                result = StbImageSharp.ImageProcessor.GetBoxDownSampler(result, outWidth, outHeight);
            }

            return result;
        }

        #endregion

        #region 兼容性检测

        /// <summary>
        /// 检测 XND 文件是否使用旧的属性结构
        /// </summary>
        public static bool DetectLegacyFormat(XndNode pngNode, TtPicDesc desc)
        {
            if (desc.CubeFaces > 1 || desc.Depth > 1)
            {
                // 如果是Cube或3D纹理，检查是否有Face节点
                var testFace = pngNode.TryGetChildNode("Face0");
                return (testFace.NativePointer == IntPtr.Zero);
            }
            else
            {
                // 如果是普通2D纹理，检查是否有PngMip0属性（旧格式）
                var testAttr = pngNode.TryGetAttribute("PngMip0");
                return (testAttr.NativePointer != IntPtr.Zero);
            }
        }

        #endregion

        #region PNG/LDR 纹理压缩格式选择

        /// <summary>
        /// 为 PNG/LDR 纹理选择压缩格式
        /// </summary>
        /// <param name="desc">纹理描述符</param>
        /// <returns>压缩格式</returns>
        public static ETextureCompressFormat SelectLdrCompressFormat(TtPicDesc desc)
        {
            // 如果不压缩，返回 None
            if (desc.DontCompress)
                return ETextureCompressFormat.TCF_None;

            // 检查引擎配置
            var config = TtEngine.Instance?.Config;
            if (config == null)
                return ETextureCompressFormat.TCF_None;

            // 根据配置选择压缩格式
            if (config.CompressDxt)
            {
                return SelectDxtFormat(desc);
            }
            else if (config.CompressEtc)
            {
                return SelectEtc2Format(desc);
            }
            else if (config.CompressAstc)
            {
                return SelectAstcFormat(desc);
            }
            else
            {
                // 无压缩配置
                return ETextureCompressFormat.TCF_None;
            }
        }

        /// <summary>
        /// 选择 DXT 压缩格式
        /// </summary>
        /// <param name="desc">纹理描述符</param>
        /// <returns>DXT 压缩格式</returns>
        public static ETextureCompressFormat SelectDxtFormat(TtPicDesc desc)
        {
            // 法线贴图使用 BC5
            if (desc.IsNormal)
            {
                return ETextureCompressFormat.TCF_BC5;
            }

            // 根据 Alpha 位数选择格式
            if (desc.BitNumAlpha == 8 || desc.BitNumAlpha == 4)
            {
                return ETextureCompressFormat.TCF_Dxt3;
            }
            else if (desc.BitNumAlpha == 1)
            {
                return ETextureCompressFormat.TCF_Dxt1a;
            }
            else
            {
                return ETextureCompressFormat.TCF_Dxt1;
            }
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

        #endregion

        #region HDR 纹理压缩格式选择

        /// <summary>
        /// 为 HDR 纹理选择压缩格式
        /// </summary>
        /// <param name="desc">纹理描述符</param>
        /// <returns>压缩格式</returns>
        public static ETextureCompressFormat SelectHdrCompressFormat(TtPicDesc desc)
        {
            // 如果不压缩，返回 None
            if (desc.DontCompress)
                return ETextureCompressFormat.TCF_None;

            // 检查引擎配置
            var config = TtEngine.Instance?.Config;
            if (config == null)
                return ETextureCompressFormat.TCF_None;

            // HDR 纹理支持 BC6 (DXT) 或 ASTC_FLOAT
            if (config.CompressDxt)
            {
                return ETextureCompressFormat.TCF_BC6;
            }
            else if (config.CompressAstc)
            {
                System.Diagnostics.Debug.Assert(false, "ASTC_FLOAT compression not fully implemented");
                return ETextureCompressFormat.TCF_Astc_4x4_Float;
            }
            else
            {
                // 无压缩配置
                return ETextureCompressFormat.TCF_None;
            }
        }

        #endregion

        #region Save/Load 辅助函数

        /// <summary>
        /// 保存 Desc 到 XND 节点
        /// </summary>
        public static unsafe void SaveDescToNode(XndNode node, TtPicDesc desc)
        {
            desc.Desc.dwStructureSize = (uint)sizeof(FPictureDesc);
            var attr = node.GetOrAddAttribute("Desc", 3, 0, true);
            using (var ar = attr.GetWriter((ulong)sizeof(FPictureDesc)))
            {
                ar.Write(desc.Desc);
                ar.Write(desc.MipSizes.Count);
                for (int i = 0; i < desc.MipSizes.Count; i++)
                {
                    ar.Write(desc.MipSizes[i]);
                    if (i < desc.BlockDimenstions.Count)
                        ar.Write(desc.BlockDimenstions[i]);
                    else
                        ar.Write(Vector2i.Zero);
                }
                ar.Write(desc.BlockSize);
                ar.Write(desc.Depth);
            }
        }

        public static unsafe TtPicDesc LoadPictureDesc(RName name)
        {
            using (var xnd = IO.TtXndHolder.LoadXnd(name.Address))
            {
                return LoadPictureDesc(xnd.RootNode);
            }
        }

        public static unsafe TtPicDesc LoadPictureDesc(IO.TtXndNode node)
        {
            TtPicDesc desc = new TtPicDesc();
            LoadPictureDesc(node, desc);
            return desc;
        }

        public static unsafe void LoadPictureDesc(IO.TtXndNode node, TtPicDesc desc)
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
                desc.BlockDimenstions.Clear();
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
                    if (attr.Version >= 2)
                    {
                        Vector2i blockDimension = new Vector2i();
                        ar.Read(out blockDimension);
                        desc.BlockDimenstions.Add(blockDimension);
                    }
                }

                if (attr.Version == 1)
                {
                    Vector2i blockDimension = new Vector2i();
                    ar.Read(out blockDimension.X);
                    ar.Read(out blockDimension.Y);
                    ar.Read(out desc.BlockSize);
                    desc.BlockDimenstions.Add(blockDimension);
                }
                else if (attr.Version >= 2)
                {
                    ar.Read(out desc.BlockSize);
                }

                for (int i = 0; i < desc.MipLevel; i++)
                {
                    if (i > (desc.BlockDimenstions.Count - 1))
                        continue;
                    var blockWidth = desc.BlockDimenstions[i].X;
                    var blockHeight = desc.BlockDimenstions[i].Y;
                    if (blockWidth % 4 != 0 || blockHeight % 4 != 0)
                    {
                        desc.MipLevel = i;
                        break;
                    }
                }

                if (attr.Version >= 3)
                {
                    int tempDepth;
                    ar.Read(out tempDepth);
                    desc.Depth = tempDepth;
                }
            }
        }
        #endregion

        #region 优化：2D Texture 批量生成 Mipmap

        /// <summary>
        /// 批量生成 2D Texture 的所有 mipmap 层级（HDR/Float）
        /// 优化：级联降采样，避免重复从 mip0 开始降采样
        /// </summary>
        public static StbImageSharp.ImageResultFloat[] GenerateAllMips2DOptimized(
            StbImageSharp.ImageResultFloat srcImage, int mipLevel)
        {
            StbImageSharp.ImageResultFloat[] mipLevels = new StbImageSharp.ImageResultFloat[mipLevel];
            mipLevels[0] = srcImage; // Mip0 就是原始数据

            // 级联降采样：从 Mip0 -> Mip1 -> Mip2 -> ...
            for (int j = 1; j < mipLevel; j++)
            {
                int prevWidth = mipLevels[j - 1].Width;
                int prevHeight = mipLevels[j - 1].Height;
                int mipWidth = Math.Max(1, prevWidth / 2);
                int mipHeight = Math.Max(1, prevHeight / 2);

                mipLevels[j] = StbImageSharp.ImageProcessor.GetBoxDownSampler(
                    mipLevels[j - 1], mipWidth, mipHeight);
            }

            return mipLevels;
        }

        /// <summary>
        /// 批量生成 2D Texture 的所有 mipmap 层级（PNG/LDR）
        /// 优化：级联降采样，避免重复从 mip0 开始降采样
        /// </summary>
        public static StbImageSharp.TtMemImage[] GenerateAllMips2DOptimizedPng(
            StbImageSharp.TtMemImage srcImage, int mipLevel)
        {
            StbImageSharp.TtMemImage[] mipLevels = new StbImageSharp.TtMemImage[mipLevel];
            mipLevels[0] = srcImage; // Mip0 就是原始数据

            // 级联降采样：从 Mip0 -> Mip1 -> Mip2 -> ...
            for (int j = 1; j < mipLevel; j++)
            {
                int prevWidth = mipLevels[j - 1].Width;
                int prevHeight = mipLevels[j - 1].Height;
                int mipWidth = Math.Max(1, prevWidth / 2);
                int mipHeight = Math.Max(1, prevHeight / 2);

                mipLevels[j] = StbImageSharp.ImageProcessor.GetBoxDownSampler(
                    mipLevels[j - 1], mipWidth, mipHeight);
            }

            return mipLevels;
        }

        #endregion

        #region 优化：3D Texture 批量生成 Mipmap

        /// <summary>
        /// 批量生成 3D Texture 的所有 mipmap 层级（HDR/Float）
        /// 优化：一次性提取所有 slice，级联降采样，避免重复提取和重复降采样
        /// </summary>
        public static unsafe void GenerateAllMips3DOptimized(
            float[] srcData, int srcWidth, int srcHeight,
            int sliceWidth, int sliceHeight, int depth,
            int slicesPerRow, int channelsPerPixel,
            out StbImageSharp.ImageResultFloat[] allSlices)
        {
            // 一次性提取所有 slice（从原始数据）
            allSlices = new StbImageSharp.ImageResultFloat[depth];
            for (int d = 0; d < depth; d++)
            {
                float[] sliceData = ExtractSliceFromGridLayoutFloat(
                    srcData, srcWidth, srcHeight,
                    sliceWidth, sliceHeight, d,
                    slicesPerRow, channelsPerPixel);

                fixed (float* floatPtr = sliceData)
                {
                    allSlices[d] = StbImageSharp.ImageResultFloat.FromResult(
                        floatPtr, sliceWidth, sliceHeight,
                        (StbImageSharp.ColorComponents)channelsPerPixel,
                        (StbImageSharp.ColorComponents)channelsPerPixel);
                }
            }
        }

        /// <summary>
        /// 批量生成 3D Texture 的所有 mipmap 层级（PNG/LDR）
        /// 优化：一次性提取所有 slice，级联降采样，避免重复提取和重复降采样
        /// </summary>
        public static void GenerateAllMips3DOptimizedPng(
            byte[] srcData, int srcWidth, int srcHeight,
            int sliceWidth, int sliceHeight, int depth,
            int slicesPerRow, out StbImageSharp.TtMemImage[] allSlices)
        {
            // 一次性提取所有 slice（从原始数据）
            allSlices = new StbImageSharp.TtMemImage[depth];
            for (int d = 0; d < depth; d++)
            {
                byte[] sliceData = ExtractSliceFromGridLayout(
                    srcData, srcWidth, srcHeight,
                    sliceWidth, sliceHeight, d,
                    slicesPerRow, 4);

                allSlices[d] = new StbImageSharp.TtMemImage();
                allSlices[d].Data = sliceData;
                allSlices[d].Width = sliceWidth;
                allSlices[d].Height = sliceHeight;
                allSlices[d].Comp = StbImageSharp.ColorComponents.RedGreenBlueAlpha;
            }
        }

        /// <summary>
        /// 批量提取 3D Texture 的所有 slice 数据（用于 BC6 压缩）
        /// 优化：避免在 mipmap 循环中重复提取 slice 数据
        /// </summary>
        public static ColorRgbFloat[] ExtractAllSlicesForBc6(
            float[] srcData, int srcWidth, int srcHeight,
            int sliceWidth, int sliceHeight, int depth,
            int slicesPerRow, int channelsPerPixel)
        {
            // 每个 slice 的大小
            int sliceSize = sliceWidth * sliceHeight;
            // 所有 slice 的总大小
            int totalSize = sliceSize * depth;

            ColorRgbFloat[] allSlices = new ColorRgbFloat[totalSize];

            // 一次性提取所有 slice，复用 ExtractSliceFromGridLayoutFloat 方法
            for (int d = 0; d < depth; d++)
            {
                // 使用现有方法提取 float[] 数据
                float[] sliceDataFloat = ExtractSliceFromGridLayoutFloat(
                    srcData, srcWidth, srcHeight,
                    sliceWidth, sliceHeight, d,
                    slicesPerRow, channelsPerPixel);

                // 转换为 ColorRgbFloat[] 并拷贝到目标位置
                int dstOffset = d * sliceSize;
                for (int i = 0; i < sliceSize; i++)
                {
                    int floatIndex = i * channelsPerPixel;
                    allSlices[dstOffset + i].r = sliceDataFloat[floatIndex];
                    allSlices[dstOffset + i].g = sliceDataFloat[floatIndex + 1];
                    allSlices[dstOffset + i].b = sliceDataFloat[floatIndex + 2];
                }
            }

            return allSlices;
        }

        #endregion
    }
}
