using BCnEncoder.Shared;
using EngineNS;
using MathNet.Numerics;
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

        public static ImageResultFloat[] GetBoxDownSampler3D(ImageResultFloat[] srcSlices, int srcWidth, int srcHeight,
            int srcDepth, int targetWidth, int targetHeight, int targetDepth)
        {
            if (srcSlices == null)
                throw new ArgumentNullException(nameof(srcSlices));
            if (srcDepth <= 0 || targetDepth <= 0 || targetWidth <= 0 || targetHeight <= 0)
                throw new ArgumentOutOfRangeException(nameof(targetDepth));
            if (srcSlices.Length < srcDepth)
                throw new ArgumentException("Source slice count is smaller than srcDepth.", nameof(srcSlices));

            var resizedSlices = new ImageResultFloat[srcDepth];
            for (int z = 0; z < srcDepth; z++)
            {
                var slice = srcSlices[z];
                if (slice == null)
                    throw new ArgumentException("Source slices cannot contain null entries.", nameof(srcSlices));

                resizedSlices[z] = (slice.Width == targetWidth && slice.Height == targetHeight)
                    ? CloneFloatImage(slice)
                    : GetBoxDownSampler(slice, targetWidth, targetHeight);
            }

            var result = new ImageResultFloat[targetDepth];
            var scaleZ = (float)srcDepth / (float)targetDepth;
            var pixelCount = targetWidth * targetHeight;
            const int channels = 4;

            for (int z = 0; z < targetDepth; z++)
            {
                var zStart = (int)Math.Floor(z * scaleZ);
                var zEnd = (int)Math.Ceiling((z + 1) * scaleZ);
                if (zStart < 0)
                    zStart = 0;
                if (zStart >= srcDepth)
                    zStart = srcDepth - 1;
                if (zEnd <= zStart)
                    zEnd = zStart + 1;
                if (zEnd > srcDepth)
                    zEnd = srcDepth;

                var data = new float[pixelCount * channels];
                for (int srcZ = zStart; srcZ < zEnd; srcZ++)
                {
                    var srcData = resizedSlices[srcZ].Data;
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] += srcData[i];
                    }
                }

                var invCount = 1.0f / (float)(zEnd - zStart);
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] *= invCount;
                }

                result[z] = new ImageResultFloat
                {
                    Width = targetWidth,
                    Height = targetHeight,
                    SourceComp = resizedSlices[zStart].SourceComp,
                    Comp = resizedSlices[zStart].Comp,
                    Data = data
                };
            }

            return result;
        }

        private static ImageResultFloat CloneFloatImage(ImageResultFloat src)
        {
            return new ImageResultFloat
            {
                Width = src.Width,
                Height = src.Height,
                SourceComp = src.SourceComp,
                Comp = src.Comp,
                Data = (float[])src.Data.Clone()
            };
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
