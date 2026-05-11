using BCnEncoder.Shared;
using EngineNS;
using System;
using System.Collections.Generic;
using System.Text;

namespace StbImageSharp
{
    public class ImageProcessor
    {
        public static unsafe TtMemImage GetCenterSquare(TtMemImage src)
        {
            var size = Math.Min(src.Width, src.Height);
            TtMemImage result = new TtMemImage();
            var x = (src.Width - size) / 2;
            var y = (src.Height - size) / 2;
            result.Width = size;
            result.Height = size;
            result.SourceComp = src.SourceComp;
            result.Comp = src.Comp;
            switch (src.Comp)
            {
                case ColorComponents.RedGreenBlueAlpha:
                    {
                        result.Data = new byte[size * size * 4];
                        fixed (byte* pSrc = &src.Data[0])
                        fixed (byte* pTar = &result.Data[0])
                        {
                            var pS = (uint*)pSrc;
                            var pT = (uint*)pTar;
                            for (int i = 0; i < size; i++)
                            {
                                for (int j = 0; j < size; j++)
                                {
                                    pT[i * result.Width + j] = pS[(i + y) * src.Width + j + x];
                                }
                            }
                        }
                    }
                    break;
                case ColorComponents.RedGreenBlue:
                    {
                        result.Data = new byte[size * size * 3];
                        fixed (byte* pSrc = &src.Data[0])
                        fixed (byte* pTar = &result.Data[0])
                        {
                            for (int i = 0; i < size; i++)
                            {
                                for (int j = 0; j < size; j++)
                                {
                                    pTar[(i * result.Width + j) * 3] = pSrc[((i + y) * src.Width + j + x) * 3];
                                    pTar[(i * result.Width + j) * 3 + 1] = pSrc[((i + y) * src.Width + j + x) * 3 + 1];
                                    pTar[(i * result.Width + j) * 3 + 2] = pSrc[((i + y) * src.Width + j + x) * 3 + 2];
                                }
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
            
            return result;
        }
        public static unsafe TtMemImage GetCenterLeft(TtMemImage src)
        {
            var size = Math.Min(src.Width, src.Height);
            TtMemImage result = new TtMemImage();
            var x = 0;
            var y = (src.Height - size) / 2;
            result.Width = size;
            result.Height = size;
            result.SourceComp = src.SourceComp;
            result.Comp = src.Comp;
            switch (src.Comp)
            {
                case ColorComponents.RedGreenBlueAlpha:
                    {
                        result.Data = new byte[size * size * 4];
                        fixed (byte* pSrc = &src.Data[0])
                        fixed (byte* pTar = &result.Data[0])
                        {
                            var pS = (uint*)pSrc;
                            var pT = (uint*)pTar;
                            for (int i = 0; i < size; i++)
                            {
                                for (int j = 0; j < size; j++)
                                {
                                    pT[i * result.Width + j] = pS[(i + y) * src.Width + j + x];
                                }
                            }
                        }
                    }
                    break;
                case ColorComponents.RedGreenBlue:
                    {
                        result.Data = new byte[size * size * 3];
                        fixed (byte* pSrc = &src.Data[0])
                        fixed (byte* pTar = &result.Data[0])
                        {
                            for (int i = 0; i < size; i++)
                            {
                                for (int j = 0; j < size; j++)
                                {
                                    pTar[(i * result.Width + j) * 3] = pSrc[((i + y) * src.Width + j + x) * 3];
                                    pTar[(i * result.Width + j) * 3 + 1] = pSrc[((i + y) * src.Width + j + x) * 3 + 1];
                                    pTar[(i * result.Width + j) * 3 + 2] = pSrc[((i + y) * src.Width + j + x) * 3 + 2];
                                }
                            }
                        }
                    }
                    break;
                default:
                    break;
            }

            return result;
        }
        public static unsafe TtMemImage GetCenterRight(TtMemImage src)
        {
            var size = Math.Min(src.Width, src.Height);
            TtMemImage result = new TtMemImage();
            var x = src.Width - size;
            var y = (src.Height - size) / 2;
            result.Width = size;
            result.Height = size;
            result.SourceComp = src.SourceComp;
            result.Comp = src.Comp;
            switch (src.Comp)
            {
                case ColorComponents.RedGreenBlueAlpha:
                    {
                        result.Data = new byte[size * size * 4];
                        fixed (byte* pSrc = &src.Data[0])
                        fixed (byte* pTar = &result.Data[0])
                        {
                            var pS = (uint*)pSrc;
                            var pT = (uint*)pTar;
                            for (int i = 0; i < size; i++)
                            {
                                for (int j = 0; j < size; j++)
                                {
                                    pT[i * result.Width + j] = pS[(i + y) * src.Width + j + x];
                                }
                            }
                        }
                    }
                    break;
                case ColorComponents.RedGreenBlue:
                    {
                        result.Data = new byte[size * size * 3];
                        fixed (byte* pSrc = &src.Data[0])
                        fixed (byte* pTar = &result.Data[0])
                        {
                            for (int i = 0; i < size; i++)
                            {
                                for (int j = 0; j < size; j++)
                                {
                                    pTar[(i * result.Width + j) * 3] = pSrc[((i + y) * src.Width + j + x) * 3];
                                    pTar[(i * result.Width + j) * 3 + 1] = pSrc[((i + y) * src.Width + j + x) * 3 + 1];
                                    pTar[(i * result.Width + j) * 3 + 2] = pSrc[((i + y) * src.Width + j + x) * 3 + 2];
                                }
                            }
                        }
                    }
                    break;
                default:
                    break;
            }

            return result;
        }
        public static unsafe TtMemImage GetBoxDownSampler(TtMemImage src, int targetWidth, int targetHeight)
        {
            switch (src.Comp)
            {
                case ColorComponents.RedGreenBlueAlpha:
                    return GetBoxDownSampler_rgba(src, targetWidth, targetHeight);
                case ColorComponents.RedGreenBlue:
                    return GetBoxDownSampler_rgb(src, targetWidth, targetHeight);
                default:
                    break;
            }
            return null;
        }
        public static unsafe TtMemImage GetBoxDownSampler_rgba(TtMemImage src, int targetWidth, int targetHeight)
        {
            System.Diagnostics.Debug.Assert(src.Comp == ColorComponents.RedGreenBlueAlpha);
            TtMemImage result = new TtMemImage();
            int hW = targetWidth;
            int hH = targetHeight;
            float scaleX = (float)src.Width / (float)hW;
            float scaleY = (float)src.Height / (float)hH;
            result.Width = hW;
            result.Height = hH;
            result.SourceComp = src.SourceComp;
            result.Comp = src.Comp;
            result.Data = new byte[hW * hH * 4];
            fixed (byte* pSrc = &src.Data[0])
            fixed (byte* pTar = &result.Data[0])
            {
                byte* curTar = pTar;
                for (int i = 0; i < hH; i++)
                {
                    for (int j = 0; j < hW; j++)
                    {
                        uint color = GetSamplerStride(pSrc, src.Width, src.Height, (int)((float)j * scaleX), (int)((float)i * scaleY), 4);
                        var c = Color4b.FromB8G8R8A8((int)color);
                        c.A = 255;
                        ((uint*)curTar)[j] = c.ToB8G8R8A8();
                    }
                    curTar += result.Width * 4;
                }
            }
            return result;
        }
        public static unsafe TtMemImage GetBoxDownSampler_rgb(TtMemImage src, int targetWidth, int targetHeight)
        {
            System.Diagnostics.Debug.Assert(src.Comp == ColorComponents.RedGreenBlue);
            TtMemImage result = new TtMemImage();
            int hW = targetWidth;
            int hH = targetHeight;
            float scaleX = (float)src.Width / (float)hW;
            float scaleY = (float)src.Height / (float)hH;
            result.Width = hW;
            result.Height = hH;
            result.SourceComp = ColorComponents.RedGreenBlueAlpha;
            result.Comp = src.Comp;
            result.Data = new byte[hW * hH * 4];
            fixed (byte* pSrc = &src.Data[0])
            fixed (byte* pTar = &result.Data[0])
            {
                byte* curTar = pTar;
                for (int i = 0; i < hH; i++)
                {
                    for (int j = 0; j < hW; j++)
                    {
                        uint color = GetSamplerStride(pSrc, src.Width, src.Height, (int)((float)j * scaleX), (int)((float)i * scaleY), 3);
                        ((uint*)curTar)[j] = color;
                    }
                    curTar += result.Width * 4;
                }
            }
            return result;
        }
        public static unsafe ImageResultFloat GetBoxDownSampler(ImageResultFloat src, int targetWidth, int targetHeight)
        {
            System.Diagnostics.Debug.Assert(src.Comp == ColorComponents.RedGreenBlueAlpha);
            ImageResultFloat result = new ImageResultFloat();
            int hW = targetWidth;
            int hH = targetHeight;
            float scaleX = (float)src.Width / (float)hW;
            float scaleY = (float)src.Height / (float)hH;
            result.Width = hW;
            result.Height = hH;
            result.SourceComp = src.SourceComp;
            result.Comp = src.Comp;
            result.Data = new float[hW * hH * 4];

            fixed (float* pSrc = &src.Data[0])
            fixed (float* pTar = &result.Data[0])
            {
                float* curTar = pTar;
                for (int i = 0; i < hH; i++)
                {
                    for (int j = 0; j < hW; j++)
                    {
                        float r, g, b, a;
                        GetSamplerStride4(pSrc, src.Width, src.Height, (int)((float)j * scaleX), (int)((float)i * scaleY), &b, &g, &r, &a);
                        curTar[j * 4] = b;
                        curTar[j * 4 + 1] = g;
                        curTar[j * 4 + 2] = r;
                        curTar[j * 4 + 3] = a;
                    }
                    curTar += result.Width * 4;
                }
            }
            return result;
        }

        /// <summary>
        /// 3D 盒式降采样：2x2x2 像素平均
        /// 对 3D Texture 的每个 slice 进行降采样，同时考虑深度方向的像素平均
        /// 支持 RGB 和 RGBA 两种格式
        /// </summary>
        /// <param name="srcSlices">源 slice 数组</param>
        /// <param name="srcWidth">源宽度</param>
        /// <param name="srcHeight">源高度</param>
        /// <param name="srcDepth">源深度（slice 数量）</param>
        /// <param name="targetWidth">目标宽度</param>
        /// <param name="targetHeight">目标高度</param>
        /// <param name="targetDepth">目标深度</param>
        /// <returns>降采样后的 slice 数组</returns>
        public static unsafe ImageResultFloat[] GetBoxDownSampler3D(
            ImageResultFloat[] srcSlices,
            int srcWidth, int srcHeight, int srcDepth,
            int targetWidth, int targetHeight, int targetDepth)
        {
            var result = new ImageResultFloat[targetDepth];

            // 获取源图像的通道数（RGB = 3, RGBA = 4）
            var srcComp = srcSlices[0].Comp;
            int srcChannels = (srcComp == ColorComponents.RedGreenBlue) ? 3 : 4;
            System.Diagnostics.Debug.Assert(srcComp == ColorComponents.RedGreenBlue || srcComp == ColorComponents.RedGreenBlueAlpha);

            // 计算缩放比例（用于定位源像素）
            float scaleX = (float)srcWidth / (float)targetWidth;
            float scaleY = (float)srcHeight / (float)targetHeight;
            float scaleZ = (float)srcDepth / (float)targetDepth;

            for (int d = 0; d < targetDepth; d++)
            {
                result[d] = new ImageResultFloat();
                result[d].Width = targetWidth;
                result[d].Height = targetHeight;
                result[d].Comp = srcComp;
                result[d].SourceComp = srcComp;
                result[d].Data = new float[targetWidth * targetHeight * srcChannels];

                // 计算当前目标 slice 对应的源 z 坐标
                int srcZ = (int)(d * scaleZ);

                fixed (float* pTar = &result[d].Data[0])
                {
                    // 遍历每个像素
                    for (int y = 0; y < targetHeight; y++)
                    {
                        int srcY = (int)(y * scaleY);

                        for (int x = 0; x < targetWidth; x++)
                        {
                            int srcX = (int)(x * scaleX);

                            // 2x2x2 盒式降采样：平均 8 个像素
                            // 计算当前目标像素对应的 2x2x2 源像素区域
                            float sumR = 0, sumG = 0, sumB = 0, sumA = 0;
                            int count = 0;

                            // 遍历 2x2x2 的源像素
                            for (int dz = 0; dz < 2; dz++)
                            {
                                int curZ = srcZ + dz;
                                if (curZ >= srcDepth) continue;

                                fixed (float* pSrc = &srcSlices[curZ].Data[0])
                                {
                                    for (int dy = 0; dy < 2; dy++)
                                    {
                                        int curY = srcY + dy;
                                        if (curY >= srcHeight) continue;

                                        for (int dx = 0; dx < 2; dx++)
                                        {
                                            int curX = srcX + dx;
                                            if (curX >= srcWidth) continue;

                                            // 读取源像素（根据通道数）
                                            int srcIndex = (curY * srcWidth + curX) * srcChannels;
                                            sumR += pSrc[srcIndex];
                                            sumG += pSrc[srcIndex + 1];
                                            sumB += pSrc[srcIndex + 2];
                                            if (srcChannels == 4)
                                                sumA += pSrc[srcIndex + 3];
                                            count++;
                                        }
                                    }
                                }
                            }

                            // 平均值
                            float avgR = count > 0 ? sumR / count : 0;
                            float avgG = count > 0 ? sumG / count : 0;
                            float avgB = count > 0 ? sumB / count : 0;
                            float avgA = count > 0 ? sumA / count : 0;

                            // 写入目标像素（根据通道数）
                            int tarIndex = (y * targetWidth + x) * srcChannels;
                            pTar[tarIndex] = avgR;
                            pTar[tarIndex + 1] = avgG;
                            pTar[tarIndex + 2] = avgB;
                            if (srcChannels == 4)
                                pTar[tarIndex + 3] = avgA;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 2D 盒式降采样：2x2 像素平均
        /// 支持任意缩放比例，通过加权平均实现高质量降采样
        /// 支持 RGB 和 RGBA 两种格式
        /// </summary>
        /// <param name="src">源图像</param>
        /// <param name="targetWidth">目标宽度</param>
        /// <param name="targetHeight">目标高度</param>
        /// <returns>降采样后的图像</returns>
        public static unsafe ImageResultFloat GetBoxDownSampler2D(ImageResultFloat src, int targetWidth, int targetHeight)
        {
            // 获取源图像的通道数（RGB = 3, RGBA = 4）
            var srcComp = src.Comp;
            int srcChannels = (srcComp == ColorComponents.RedGreenBlue) ? 3 : 4;
            System.Diagnostics.Debug.Assert(srcComp == ColorComponents.RedGreenBlue || srcComp == ColorComponents.RedGreenBlueAlpha);

            // 创建目标图像
            ImageResultFloat result = new ImageResultFloat();
            result.Width = targetWidth;
            result.Height = targetHeight;
            result.Comp = srcComp;
            result.SourceComp = srcComp;
            result.Data = new float[targetWidth * targetHeight * srcChannels];

            // 预计算缩放比例
            float scaleX = (float)src.Width / (float)targetWidth;
            float scaleY = (float)src.Height / (float)targetHeight;

            fixed (float* pSrc = &src.Data[0])
            fixed (float* pTar = &result.Data[0])
            {
                // 遍历目标图像的每个像素
                for (int tarY = 0; tarY < targetHeight; tarY++)
                {
                    // 计算源图像的 Y 坐标范围
                    float srcY = tarY * scaleY;
                    int y0 = (int)srcY;
                    int y1 = Math.Min(y0 + 1, src.Height - 1);
                    float yFrac = srcY - y0;

                    for (int tarX = 0; tarX < targetWidth; tarX++)
                    {
                        // 计算源图像的 X 坐标范围
                        float srcX = tarX * scaleX;
                        int x0 = (int)srcX;
                        int x1 = Math.Min(x0 + 1, src.Width - 1);
                        float xFrac = srcX - x0;

                        // 双线性插值：对 2x2 区域的 4 个像素进行加权平均
                        float sumR = 0, sumG = 0, sumB = 0, sumA = 0;
                        float totalWeight = 0;

                        // 2x2 采样
                        for (int dy = 0; dy < 2; dy++)
                        {
                            int srcYCur = Math.Min(y0 + dy, src.Height - 1);

                            for (int dx = 0; dx < 2; dx++)
                            {
                                int srcXCur = Math.Min(x0 + dx, src.Width - 1);

                                // 计算权重（双线性插值）
                                float weightX = (dx == 0) ? (1.0f - xFrac) : xFrac;
                                float weightY = (dy == 0) ? (1.0f - yFrac) : yFrac;
                                float weight = weightX * weightY;

                                // 读取源像素
                                int srcIndex = (srcYCur * src.Width + srcXCur) * srcChannels;
                                sumR += pSrc[srcIndex] * weight;
                                sumG += pSrc[srcIndex + 1] * weight;
                                sumB += pSrc[srcIndex + 2] * weight;
                                if (srcChannels == 4)
                                    sumA += pSrc[srcIndex + 3] * weight;
                                totalWeight += weight;
                            }
                        }

                        // 归一化并写入目标像素
                        int tarIndex = (tarY * targetWidth + tarX) * srcChannels;
                        pTar[tarIndex] = (totalWeight > 0) ? sumR / totalWeight : 0;
                        pTar[tarIndex + 1] = (totalWeight > 0) ? sumG / totalWeight : 0;
                        pTar[tarIndex + 2] = (totalWeight > 0) ? sumB / totalWeight : 0;
                        if (srcChannels == 4)
                            pTar[tarIndex + 3] = (totalWeight > 0) ? sumA / totalWeight : 0;
                    }
                }
            }

            return result;
        }

        public static unsafe TtMemImage StretchBlt(uint targetWidth, uint targetHeight, TtMemImage src, uint SrcX, uint SrcY, uint SrcW, uint SrcH)
        {
            System.Diagnostics.Debug.Assert(src.Comp == ColorComponents.RedGreenBlueAlpha);
            TtMemImage result = new TtMemImage();
            uint hW = targetWidth;
            uint hH = targetHeight;
            if (SrcX >= src.Width)
            {
                SrcX = (uint)src.Width - 1;
            }
            if (SrcY >= src.Height)
            {
                SrcY = (uint)src.Height - 1;
            }
            if (SrcX + SrcW >= src.Width)
            {
                SrcW = (uint)src.Width - SrcX;
            }
            if (SrcY + SrcH >= src.Height)
            {
                SrcH = (uint)src.Height - SrcY;
            }
            float scaleX = (float)SrcW / (float)hW;
            float scaleY = (float)SrcH / (float)hH;
            result.Width = (int)hW;
            result.Height = (int)hH;
            result.SourceComp = src.SourceComp;
            result.Comp = src.Comp;
            result.Data = new byte[hW * hH * 4];
            fixed (byte* pSrc = &src.Data[0])
            fixed (byte* pTar = &result.Data[0])
            {
                byte* curTar = pTar;
                for (int i = 0; i < hH; i++)
                {
                    for (int j = 0; j < hW; j++)
                    {
                        uint color = GetSamplerStride(pSrc, src.Width, src.Height, (int)SrcX + (int)((float)j * scaleX), (int)SrcY + (int)((float)i * scaleY), 4);
                        ((uint*)curTar)[j] = color;
                    }
                    curTar += result.Width * 4;
                }
            }
            return result;
        }
        private static int[,] sampler = new int[3, 3]
        {
                { 10, 20, 10},
                { 20, 80, 20},
                { 10, 20, 10}
        };
        private static unsafe uint GetSamplerStride(byte* pBuffer, int w, int h, int x, int y, int pixelBitwise = 4)
        {
            x = x - 1;
            y = y - 1;

            uint tb = 0;
            uint tg = 0;
            uint tr = 0;
            uint ta = 0;
            uint value = 0;
            uint stride = (uint)(w * pixelBitwise);
            for (int i = 0; i < 3; i++)
            {
                int sy = y + i;
                if (sy < 0 || sy >= h)
                    continue;
                for (int j = 0; j < 3; j++)
                {
                    int sx = x + j;
                    if (sx < 0 || sx >= w)
                        continue;
                    int start = sy * (int)stride + sx * pixelBitwise;
                    int b = pBuffer[start];
                    int g = pBuffer[start + 1];
                    int r = pBuffer[start + 2];
                    int a = pBuffer[start + 3];

                    tb += (uint)(b * sampler[i, j]);
                    tg += (uint)(g * sampler[i, j]);
                    tr += (uint)(r * sampler[i, j]);
                    ta += (uint)(a * sampler[i, j]);
                    value += (uint)sampler[i, j];
                }
            }
            if (value == 0)
                return 0;
            tb = tb / value;
            tg = tg / value;
            tr = tr / value;
            ta = ta / value;

            uint color = (tb & 0xFF) | ((tg & 0xFF) << 8) | ((tr & 0xFF) << 16) | ((ta & 0xFF) << 24);
            return color;
        }
        private static unsafe void GetSamplerStride4(float* pFloatBuffer, int w, int h, int x, int y, float *outB, float *outG, float *outR, float *outA)
        {
            *outB = 0;
            *outG = 0;
            *outR = 0;
            *outA = 0;

            x = x - 1;
            y = y - 1;

            uint tb = 0;
            uint tg = 0;
            uint tr = 0;
            uint ta = 0;
            uint value = 0;
            uint stride = (uint)w * 4;
            for (int i = 0; i < 3; i++)
            {
                int sy = y + i;
                if (sy < 0 || sy >= h)
                    continue;
                for (int j = 0; j < 3; j++)
                {
                    int sx = x + j;
                    if (sx < 0 || sx >= w)
                        continue;
                    int start = sy * (int)stride + sx * 4;
                    int b = (int)(pFloatBuffer[start]*255);
                    int g = (int)(pFloatBuffer[start + 1]*255);
                    int r = (int)(pFloatBuffer[start + 2]*255);
                    int a = (int)(pFloatBuffer[start + 3]*255);

                    tb += (uint)(b * sampler[i, j]);
                    tg += (uint)(g * sampler[i, j]);
                    tr += (uint)(r * sampler[i, j]);
                    ta += (uint)(a * sampler[i, j]);
                    value += (uint)sampler[i, j];
                }
            }
            if (value == 0)
                return;
            tb = tb / value;
            tg = tg / value;
            tr = tr / value;
            ta = ta / value;

            *outB = (float)tb / (float)255.0;
            *outG = (float)tg / (float)255.0;
            *outR = (float)tr / (float)255.0;
            *outA = (float)ta / (float)255.0;

            return;
        }

    }
}
