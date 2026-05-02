// UTextureHelper.cs
// 纹理处理辅助函数集合，包含 mipmap 处理和压缩格式选择

using BCnEncoder.Shared;
using EngineNS.Bricks.ImageDecoder;
using EngineNS.IO;
using StbImageSharp;
using System;
using System.Runtime.InteropServices;
using static EngineNS.NxRHI.TtSrView;

namespace EngineNS.NxRHI
{

    public class NormalmapChecker
    {
        // These values are the threshold values for the average vector's
        // length to be considered within limits as a normal map normal
        const float NormalMapMinLengthConfidenceThreshold = 0.55f;
        const float NormalMapMaxLengthConfidenceThreshold = 1.1f;

        // This value is the threshold value for the average vector to be considered
        // to be going in the correct direction.
        const float NormalMapDeviationThreshold = 0.8f;

        // Samples from the texture will be taken in blocks of this size^2
        const int SampleTileEdgeLength = 4;

        // We sample up to this many tiles in each axis. Sampling more tiles
        // will likely be more accurate, but will take longer.
        const int MaxTilesPerAxis = 16;

        // This is used in the comparison with "mid-gray"
        const float ColorComponentNearlyZeroThreshold = (2.0f / 255.0f);

        // This is used when comparing alpha to zero to avoid picking up sprites
        const float AlphaComponentNearlyZeroThreshold = (1.0f / 255.0f);

        // These values are chosen to make the threshold colors (from uint8 textures)
        // discard the top most and bottom most two values, i.e. 0, 1, 254 and 255 on
        // the assumption that these are likely invalid values for a general normal map
        const float ColorComponentMinVectorThreshold = (2.0f / 255.0f) * 2.0f - 1.0f;
        const float ColorComponentMaxVectorThreshold = (253.0f / 255.0f) * 2.0f - 1.0f;

        // This is the threshold delta length for a vector to be considered as a unit vector
        const float NormalVectorUnitLengthDeltaThreshold = 0.45f;

        // Rejected to taken sample ratio threshold.
        const float RejectedToTakenRatioThreshold = 0.33f;

        void EvaluateSubBlock(StbImageSharp.TtMemImage image, int Left, int Top, int Width, int Height)
        {
            for (int Y = Top; Y != (Top + Height); Y++)
            {
                for (int X = Left; X != (Left + Width); X++)
                {
                    var ColorSample = image.GetPixel(X, Y).ToColor4Float();
                    if (image.Comp == ColorComponents.RedGreenBlue || image.Comp == ColorComponents.Grey)
                        ColorSample.Alpha = 1.0f;

                    // Nearly black or transparent pixels don't contribute to the calculation
                    if ((ColorSample.Alpha - AlphaComponentNearlyZeroThreshold) < MathHelper.Epsilon ||
                        ColorSample.IsAlmostBlack())
                    {
                        continue;
                    }

                    // Scale and bias, if required, to get a signed vector
                    float Vx = ColorSample.Red * 2.0f - 1.0f;
                    float Vy = ColorSample.Green * 2.0f - 1.0f;
                    float Vz = ColorSample.Blue * 2.0f - 1.0f;

                    float Length = MathHelper.Sqrt(Vx * Vx + Vy * Vy + Vz * Vz);
                    if (Length < ColorComponentNearlyZeroThreshold)
                    {
                        // mid-grey pixels representing (0,0,0) are also not considered as they may be used to denote unused areas
                        continue;
                    }

                    // If the vector is sufficiently different in length from a unit vector, consider it invalid.
                    if (MathHelper.Abs(Length - 1.0f) > NormalVectorUnitLengthDeltaThreshold)
                    {
                        NumSamplesRejected++;
                        continue;
                    }

                    // If the vector is pointing backwards then it is an invalid sample, so consider it invalid
                    if (Vz < 0.0f)
                    {
                        NumSamplesRejected++;
                        continue;
                    }

                    AverageColor = AverageColor + ColorSample;
                    NumSamplesTaken++;
                }
            }
        }

        /**
         * DoesTextureLookLikelyToBeANormalMap
         *
         * Makes a best guess as to whether a texture represents a normal map or not.
         * Will not be 100% accurate, but aims to be as good as it can without usage
         * information or relying on naming conventions.
         *
         * The heuristic takes samples in small blocks across the texture (if the texture
         * is large enough). The assumption is that if the texture represents a normal map
         * then the average direction of the resulting vector should be somewhere near {0,0,1}.
         * It samples in a number of blocks spread out to decrease the chance of hitting a
         * single unused/blank area of texture, which could happen depending on uv layout.
         *
         * Any pixels that are black, mid-gray or have a red or green value resulting in X or Y
         * being -1 or +1 are ignored on the grounds that they are invalid values. Artists
         * sometimes fill the unused areas of normal maps with color being the {0,0,1} vector,
         * but that cannot be relied on - those areas are often black or gray instead.
         *
         * If the heuristic manages to sample enough valid pixels, the threshold being based
         * on the total number of samples it will be looking at, then it takes the average
         * vector of all the sampled pixels and checks to see if the length and direction are
         * within a specific tolerance. See the namespace at the top of the file for tolerance
         * value specifications. If the vector satisfies those tolerances then the texture is
         * considered to be a normal map.
         */
        public bool DoesTextureLookLikelyToBeANormalMap(StbImageSharp.TtMemImage image)
        {
            int TextureSizeX = image.Width;
            int TextureSizeY = image.Height;

            // Calculate the number of tiles in each axis, but limit the number
            // we interact with to a maximum of 16 tiles (4x4)
            int NumTilesX = Math.Min(TextureSizeX / SampleTileEdgeLength, MaxTilesPerAxis);
            int NumTilesY = Math.Min(TextureSizeY / SampleTileEdgeLength, MaxTilesPerAxis);

            //if (!Sampler.SetSourceTexture(Texture))
            //{
            //    return false;
            //}

            if ((NumTilesX > 0) &&
                (NumTilesY > 0))
            {
                // If texture is large enough then take samples spread out across the image
                NumSamplesThreshold = (NumTilesX * NumTilesY) * 4; // on average 4 samples per tile need to be valid...

                for (int TileY = 0; TileY < NumTilesY; TileY++)
                {
                    int Top = (TextureSizeY / NumTilesY) * TileY;

                    for (int TileX = 0; TileX < NumTilesX; TileX++)
                    {
                        int Left = (TextureSizeX / NumTilesX) * TileX;

                        EvaluateSubBlock(image, Left, Top, SampleTileEdgeLength, SampleTileEdgeLength);
                    }
                }
            }
            else
            {
                NumSamplesThreshold = (TextureSizeX * TextureSizeY) / 4;

                // Texture is small enough to sample all texels
                EvaluateSubBlock(image, 0, 0, TextureSizeX, TextureSizeY);
            }

            // if we managed to take a reasonable number of samples then we can evaluate the result
            if (NumSamplesTaken >= NumSamplesThreshold)
            {
                float RejectedToTakenRatio = (float)(NumSamplesRejected) / (float)(NumSamplesTaken);
                if (RejectedToTakenRatio >= RejectedToTakenRatioThreshold)
                {
                    // Too many invalid samples, probably not a normal map
                    return false;
                }

                AverageColor = AverageColor * (1.0f / (float)NumSamplesTaken);

                // See if the resulting vector lies anywhere near the {0,0,1} vector
                float Vx = AverageColor.Red * 2.0f - 1.0f;
                float Vy = AverageColor.Green * 2.0f - 1.0f;
                float Vz = AverageColor.Blue * 2.0f - 1.0f;

                float Magnitude = MathHelper.Sqrt(Vx * Vx + Vy * Vy + Vz * Vz);

                // The normalized value of the Z component tells us how close to {0,0,1} the average vector is
                float NormalizedZ = Vz / Magnitude;

                // if the average vector is longer than or equal to the min length, shorter than the max length
                // and the normalized Z value means that the vector is close enough to {0,0,1} then we consider
                // this a normal map
                return ((Magnitude >= NormalMapMinLengthConfidenceThreshold) &&
                        (Magnitude < NormalMapMaxLengthConfidenceThreshold) &&
                        (NormalizedZ >= NormalMapDeviationThreshold));
            }

            // Not enough samples, don't trust the result at all
            return false;
        }

        int NumSamplesTaken;
        int NumSamplesRejected;
        int NumSamplesThreshold;
        Color4f AverageColor;

    }

    /// <summary>
    /// Mipmap 资源守卫
    /// 使用 RAII 模式自动管理 GCHandle 生命周期,防止内存泄漏
    /// </summary>
    public unsafe ref struct UMipmapResourceGuard
    {
        private GCHandle[] _handles;
        private int _count;
        private bool _disposed;

        /// <summary>
        /// 创建资源守卫
        /// </summary>
        /// <param name="count">mipmap 资源数量</param>
        public UMipmapResourceGuard(int count)
        {
            if (count <= 0)
                throw new ArgumentException("Count must be greater than 0", nameof(count));

            _count = count;
            _handles = new GCHandle[count];
            _disposed = false;
        }

        /// <summary>
        /// 设置字节型 mipmap 数据并获取指针
        /// </summary>
        /// <param name="index">mipmap 索引</param>
        /// <param name="data">像素数据</param>
        /// <returns>固定的数据指针</returns>
        public void* SetData(int index, byte[] data)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(UMipmapResourceGuard));

            if (index < 0 || index >= _count)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            // 释放之前句柄(如果存在)
            if (_handles[index].IsAllocated)
            {
                _handles[index].Free();
            }

            // 固定数组并返回指针
            _handles[index] = GCHandle.Alloc(data, GCHandleType.Pinned);
            return _handles[index].AddrOfPinnedObject().ToPointer();
        }

        /// <summary>
        /// 设置浮点型 mipmap 数据并获取指针
        /// </summary>
        /// <param name="index">mipmap 索引</param>
        /// <param name="data">浮点像素数据</param>
        /// <returns>固定的数据指针</returns>
        public void* SetDataFloat(int index, float[] data)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(UMipmapResourceGuard));

            if (index < 0 || index >= _count)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            // 释放之前句柄(如果存在)
            if (_handles[index].IsAllocated)
            {
                _handles[index].Free();
            }

            // 固定数组并返回指针
            _handles[index] = GCHandle.Alloc(data, GCHandleType.Pinned);
            return _handles[index].AddrOfPinnedObject().ToPointer();
        }

        /// <summary>
        /// 释放所有 GCHandle
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            for (int i = 0; i < _count; i++)
            {
                if (_handles[i].IsAllocated)
                {
                    _handles[i].Free();
                }
            }

            _handles = null;
            _disposed = true;
        }
    }

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
        public static ETextureCompressFormat SelectCompressFormat(TtPicDesc desc)
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
        /// 批量生成 3D Texture 的所有 mipmap 层级（用于 BC6 压缩）
        /// 优化：使用 GetBoxDownSampler 级联降采样，避免手动实现降采样逻辑
        /// </summary>
        public static unsafe ColorRgbFloat[][][] GenerateAllMips3DForBc6(
            float[] srcData, int srcWidth, int srcHeight,
            int sliceWidth, int sliceHeight, int depth,
            int slicesPerRow, int channelsPerPixel,
            int mipLevel)
        {
            // allMipLevels[j][d] = mipLevel j, slice d 的 ColorRgbFloat[] 数据
            ColorRgbFloat[][][] allMipLevels = new ColorRgbFloat[mipLevel][][];

            // Step 1: 提取所有原始 slice (Mip0) 并转换为 ImageResultFloat
            var mip0Slices = new StbImageSharp.ImageResultFloat[depth];
            for (int d = 0; d < depth; d++)
            {
                float[] sliceDataFloat = ExtractSliceFromGridLayoutFloat(
                    srcData, srcWidth, srcHeight,
                    sliceWidth, sliceHeight, d,
                    slicesPerRow, channelsPerPixel);

                // 转换为 ImageResultFloat
                mip0Slices[d] = new StbImageSharp.ImageResultFloat();
                mip0Slices[d].Width = sliceWidth;
                mip0Slices[d].Height = sliceHeight;
                mip0Slices[d].Comp = StbImageSharp.ColorComponents.RedGreenBlueAlpha;
                mip0Slices[d].SourceComp = StbImageSharp.ColorComponents.RedGreenBlueAlpha;

                // 将 float[] 转换为 RGBA 格式 (添加 Alpha=1)
                int pixelCount = sliceWidth * sliceHeight;
                mip0Slices[d].Data = new float[pixelCount * 4];
                for (int i = 0; i < pixelCount; i++)
                {
                    int floatIndex = i * channelsPerPixel;
                    mip0Slices[d].Data[i * 4] = sliceDataFloat[floatIndex];         // R
                    mip0Slices[d].Data[i * 4 + 1] = sliceDataFloat[floatIndex + 1]; // G
                    mip0Slices[d].Data[i * 4 + 2] = sliceDataFloat[floatIndex + 2]; // B
                    mip0Slices[d].Data[i * 4 + 3] = 1.0f;                            // A
                }
            }

            // Step 2: 级联降采样生成所有 mipmap 级别
            allMipLevels[0] = new ColorRgbFloat[depth][];
            for (int d = 0; d < depth; d++)
            {
                int sliceSize = sliceWidth * sliceHeight;
                allMipLevels[0][d] = new ColorRgbFloat[sliceSize];
                for (int i = 0; i < sliceSize; i++)
                {
                    allMipLevels[0][d][i].r = mip0Slices[d].Data[i * 4];
                    allMipLevels[0][d][i].g = mip0Slices[d].Data[i * 4 + 1];
                    allMipLevels[0][d][i].b = mip0Slices[d].Data[i * 4 + 2];
                }
            }

            // 从 Mip1 开始级联降采样
            // 使用临时数组保存上一级的 slice 数据
            var prevSlices = new StbImageSharp.ImageResultFloat[depth];
            for (int d = 0; d < depth; d++)
            {
                prevSlices[d] = mip0Slices[d];
            }

            for (int j = 1; j < mipLevel; j++)
            {
                int prevMipDepth = Math.Max(1, depth >> (j - 1));
                int curMipDepth = Math.Max(1, depth >> j);
                int prevMipWidth = Math.Max(1, sliceWidth >> (j - 1));
                int prevMipHeight = Math.Max(1, sliceHeight >> (j - 1));
                int curMipWidth = Math.Max(1, sliceWidth >> j);
                int curMipHeight = Math.Max(1, sliceHeight >> j);

                allMipLevels[j] = new ColorRgbFloat[curMipDepth][];

                // 使用 GetBoxDownSampler3D 降采样（同时考虑 x, y, z 三个维度）
                var downsampledSlices = StbImageSharp.ImageProcessor.GetBoxDownSampler3D(
                    prevSlices, prevMipWidth, prevMipHeight, prevMipDepth,
                    curMipWidth, curMipHeight, curMipDepth);

                // 转换回 ColorRgbFloat[]
                for (int d = 0; d < curMipDepth; d++)
                {
                    var downsampled = downsampledSlices[d];

                    int curSliceSize = curMipWidth * curMipHeight;
                    allMipLevels[j][d] = new ColorRgbFloat[curSliceSize];
                    for (int i = 0; i < curSliceSize; i++)
                    {
                        allMipLevels[j][d][i].r = downsampled.Data[i * 4];
                        allMipLevels[j][d][i].g = downsampled.Data[i * 4 + 1];
                        allMipLevels[j][d][i].b = downsampled.Data[i * 4 + 2];
                    }
                }

                // 更新 prevSlices 为当前级的数据，用于下一级降采样
                prevSlices = downsampledSlices;
            }

            return allMipLevels;
        }


        /// <summary>
        /// 创建纹理描述符
        /// </summary>
        public static FTextureDesc CreateTextureDesc(TtPicDesc desc, int mipLevel, EPixelFormat format)
        {
            var texDesc = new FTextureDesc();
            texDesc.SetDefault();
            texDesc.Width = (uint)desc.MipSizes[desc.MipLevel - mipLevel].X;
            texDesc.Height = (uint)desc.MipSizes[desc.MipLevel - mipLevel].Y;
            texDesc.MipLevels = (uint)mipLevel;
            texDesc.Format = format;

            // 处理 3D Texture
            if (desc.IsTexture3D && desc.Depth > 0)
            {
                texDesc.Depth = (uint)desc.MipSizes[desc.MipLevel - mipLevel].Z;
                texDesc.ArraySize = 1;
            }
            else if (desc.CubeFaces == 6)
            {
                // Cube Texture
                texDesc.ArraySize = 6;
                texDesc.MiscFlags = EResourceMiscFlag.RM_TEXTURECUBE;
            }

            return texDesc;
        }

        #endregion

        #region Cubemap
        // transform world space vector to a space relative to the face
        static Vector3 TransformSideToWorldSpace(uint CubemapFace, Vector3 InDirection)
        {
            float x = InDirection.X, y = InDirection.Y, z = InDirection.Z;

            Vector3 Ret = new Vector3(0, 0, 0);

            // see http://msdn.microsoft.com/en-us/library/bb204881(v=vs.85).aspx
            switch (CubemapFace)
            {
                case 0: Ret = new Vector3(+z, -y, -x); break;
                case 1: Ret = new Vector3(-z, -y, +x); break;
                case 2: Ret = new Vector3(+x, +z, +y); break;
                case 3: Ret = new Vector3(+x, -z, -y); break;
                case 4: Ret = new Vector3(+x, -y, +z); break;
                case 5: Ret = new Vector3(-x, -y, -z); break;
            }

            // this makes it with the Unreal way (z and y are flipped)
            return Ret;
        }

        // transform vector relative to the face to world space
        static Vector3 TransformWorldToSideSpace(uint CubemapFace, Vector3 InDirection)
        {
            // undo Unreal way (z and y are flipped)
            float x = InDirection.X, y = InDirection.Z, z = InDirection.Y;

            Vector3 Ret = new Vector3(0, 0, 0);

            // see http://msdn.microsoft.com/en-us/library/bb204881(v=vs.85).aspx
            switch (CubemapFace)
            {
                case 0: Ret = new Vector3(-z, -y, +x); break;
                case 1: Ret = new Vector3(+z, -y, -x); break;
                case 2: Ret = new Vector3(+x, +z, +y); break;
                case 3: Ret = new Vector3(+x, -z, -y); break;
                case 4: Ret = new Vector3(+x, -y, +z); break;
                case 5: Ret = new Vector3(-x, -y, -z); break;
            }

            return Ret;
        }

        static Vector3 ComputeSSCubeDirectionAtTexelCenter(uint x, uint y, float InvSideExtent)
        {
            // center of the texels
            Vector3 DirectionSS = new Vector3((x + 0.5f) * InvSideExtent * 2 - 1, (y + 0.5f) * InvSideExtent * 2 - 1, 1);
            DirectionSS.Normalize();
            return DirectionSS;
        }

        static Vector3 ComputeWSCubeDirectionAtTexelCenter(uint CubemapFace, uint x, uint y, float InvSideExtent)
        {
            Vector3 DirectionSS = ComputeSSCubeDirectionAtTexelCenter(x, y, InvSideExtent);
            Vector3 DirectionWS = TransformSideToWorldSpace(CubemapFace, DirectionSS);
            return DirectionWS;
        }

        static int ComputeLongLatCubemapExtents(int SrcImageWidth, int MaxCubemapTextureResolution)
        {
            int width = 1 << (int)MathHelper.ILog2Const((uint)SrcImageWidth / 2);
            return MathHelper.Clamp(width, 32, MaxCubemapTextureResolution);
        }

        /**
            * View in to an image that allows access by converting a direction to longitude and latitude.
        */
        struct ImageViewLongLat
        {
            /** Image colors. */
            Vector4[] ImageColors;
            /** Width of the image. */
            int SizeX;
            /** Height of the image. */
            int SizeY;

            /** Initialization constructor. */
            public ImageViewLongLat(ImageResultFloat Image)
            {
                SizeX = Image.Width;
                SizeY = Image.Height;
                ImageColors = new Vector4[Image.Width * Image.Height];
                if (Image.Comp == ColorComponents.RedGreenBlueAlpha)
                {
                    for (int i = 0; i < ImageColors.Length; ++i)
                    {
                        ImageColors[i].X = Image.Data[i * 4 + 0];
                        ImageColors[i].Y = Image.Data[i * 4 + 1];
                        ImageColors[i].Z = Image.Data[i * 4 + 2];
                        ImageColors[i].W = Image.Data[i * 4 + 3];
                    }
                }
            }

            /** Wraps X around W. */
            static void WrapTo(ref int X, int W)
            {
                X = X % W;

                if (X < 0)
                {
                    X += W;
                }
            }

            /** Const access to a texel. */
            Vector4 Access(int X, int Y)
            {
                return ImageColors[X + Y * SizeX];
            }

            /** Makes a filtered lookup. */
            Vector4 LookupFiltered(float X, float Y)
            {
                int X0 = (int)MathHelper.Floor(X);
                int Y0 = (int)MathHelper.Floor(Y);

                float FracX = X - X0;
                float FracY = Y - Y0;

                int X1 = X0 + 1;
                int Y1 = Y0 + 1;

                WrapTo(ref X0, SizeX);
                WrapTo(ref X1, SizeX);
                Y0 = MathHelper.Clamp(Y0, 0, (int)(SizeY - 1));
                Y1 = MathHelper.Clamp(Y1, 0, (int)(SizeY - 1));

                Vector4 CornerRGB00 = Access(X0, Y0);
                Vector4 CornerRGB10 = Access(X1, Y0);
                Vector4 CornerRGB01 = Access(X0, Y1);
                Vector4 CornerRGB11 = Access(X1, Y1);

                Vector4 CornerRGB0 = Vector4.Lerp(CornerRGB00, CornerRGB10, FracX);
                Vector4 CornerRGB1 = Vector4.Lerp(CornerRGB01, CornerRGB11, FracX);

                return Vector4.Lerp(CornerRGB0, CornerRGB1, FracY);
            }

            /** Makes a filtered lookup using a direction. */
            public Vector4 LookupLongLat(Vector3 NormalizedDirection)
            {
                // see http://gl.ict.usc.edu/Data/HighResProbes
                // latitude-longitude panoramic format = equirectangular mapping
                float X = (1 + MathHelper.Atan2(NormalizedDirection.X, NormalizedDirection.Z) / MathHelper.PI) / 2 * SizeX;
                float Y = MathHelper.Acos(NormalizedDirection.Y) / MathHelper.PI * SizeY;

                return LookupFiltered(X, Y);
            }
        };

        static void CopyFaceToCubemapContinus(Vector4[] faceData, Vector4[] cubeMapExpand, int faceStartIndex, int Extent)
        {
            var faceDataSize = Extent * Extent;
            for (int y = 0; y < Extent; ++y)
            {
                for (int x = 0; x < Extent; ++x)
                {
                    cubeMapExpand[faceStartIndex + x + y * 4 * Extent] = faceData[x + y * Extent];
                }
            }
        }

        /**
         * Generates the base cubemap mip from a longitude-latitude 2D image.
         * @param OutMip - The output mip.
         * @param SrcImage - The source longlat image.
         */
        public static void GenerateBaseCubeMipFromLongitudeLatitude2D(ref StbImageSharp.ImageResultFloat OutMip, StbImageSharp.ImageResultFloat LongLatImage, int MaxCubemapTextureResolution)
        {
            ImageViewLongLat LongLatView = new ImageViewLongLat(LongLatImage);

            // TODO_TEXTURE: Expose target size to user.
            int Extent = ComputeLongLatCubemapExtents(LongLatImage.Width, MaxCubemapTextureResolution);
            float InvExtent = 1.0f / Extent;

            Vector4[] faceDataContinus = new Vector4[6 * Extent * Extent];
            Vector4[][] faceDatas = new Vector4[6][];
            for (uint Face = 0; Face < 6; ++Face)
            {
                faceDatas[Face] = new Vector4[Extent * Extent];
                //Vector4[] faceData = new Vector4[Extent * Extent];
                for (int y = 0; y < Extent; ++y)
                {
                    for (int x = 0; x < Extent; ++x)
                    {
                        Vector3 DirectionWS = ComputeWSCubeDirectionAtTexelCenter(Face, (uint)x, (uint)y, InvExtent);
                        faceDatas[Face][x + y * Extent] = LongLatView.LookupLongLat(DirectionWS);
                    }
                }

                faceDatas[Face].CopyTo(faceDataContinus, Face * Extent * Extent);
            }

            float[] floatArray = faceDataContinus.SelectMany(vector => new float[] { vector.X, vector.Y, vector.Z, vector.W }).ToArray();
            unsafe
            {
                fixed (float* floatPtr = floatArray)
                {
                    OutMip = StbImageSharp.ImageResultFloat.FromResult(floatPtr, Extent, 6 * Extent, ColorComponents.RedGreenBlueAlpha, ColorComponents.RedGreenBlueAlpha);
                }

                #region Debug
                bool bDebug = false;
                if (bDebug)
                {
                    var faceDataSize = Extent * Extent;
                    Vector4[] cubeMapExpand = new Vector4[4 * 3 * faceDataSize];

                    CopyFaceToCubemapContinus(faceDatas[0], cubeMapExpand, 2 * Extent + 4 * faceDataSize, Extent);
                    CopyFaceToCubemapContinus(faceDatas[1], cubeMapExpand, 4 * faceDataSize, Extent);
                    CopyFaceToCubemapContinus(faceDatas[2], cubeMapExpand, 1 * Extent, Extent);
                    CopyFaceToCubemapContinus(faceDatas[3], cubeMapExpand, 1 * Extent + 8 * faceDataSize, Extent);
                    CopyFaceToCubemapContinus(faceDatas[4], cubeMapExpand, 1 * Extent + 4 * faceDataSize, Extent);
                    CopyFaceToCubemapContinus(faceDatas[5], cubeMapExpand, 3 * Extent + 4 * faceDataSize, Extent);

                    float[] floatArrayDebug = cubeMapExpand.SelectMany(vector => new float[] { vector.X, vector.Y, vector.Z, vector.W }).ToArray();

                    fixed (float* floatPtrDebug = floatArrayDebug)
                    {
                        OutMip = StbImageSharp.ImageResultFloat.FromResult(floatPtrDebug, 4 * Extent, 3 * Extent, ColorComponents.RedGreenBlueAlpha, ColorComponents.RedGreenBlueAlpha);

                        var sourceFile = "F:/CubeFaces" + ".hdr";
                        using (var stream = System.IO.File.Create(sourceFile))
                        {
                            var writer = new StbImageWriteSharp.ImageWriter();
                            writer.WriteHdr(floatPtrDebug, 4 * Extent, 3 * Extent, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
                        }
                    }
                }
                #endregion
            }

        }
        #endregion

    }




}
