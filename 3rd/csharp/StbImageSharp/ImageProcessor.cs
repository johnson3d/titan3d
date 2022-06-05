using System;
using System.Collections.Generic;
using System.Text;

namespace StbImageSharp
{
    public class ImageProcessor
    {
        public static unsafe ImageResult GetBoxDownSampler(ImageResult src, int targetWidth, int targetHeight)
        {
            System.Diagnostics.Debug.Assert(src.Comp == ColorComponents.RedGreenBlueAlpha);
            ImageResult result = new ImageResult();
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
                        uint color = GetSamplerStride4(pSrc, src.Width, src.Height, (int)((float)j * scaleX), (int)((float)i * scaleY));
                        ((uint*)curTar)[j] = color;
                    }
                    curTar += result.Width * 4;
                }
            }
            return result;
        }
        public static unsafe ImageResult StretchBlt(uint targetWidth, uint targetHeight, ImageResult src, uint SrcX, uint SrcY, uint SrcW, uint SrcH)
        {
            System.Diagnostics.Debug.Assert(src.Comp == ColorComponents.RedGreenBlueAlpha);
            ImageResult result = new ImageResult();
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
                        uint color = GetSamplerStride4(pSrc, src.Width, src.Height, (int)SrcX + (int)((float)j * scaleX), (int)SrcY + (int)((float)i * scaleY));
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
        private static unsafe uint GetSamplerStride4(byte* pBuffer, int w, int h, int x, int y)
        {
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
    }
}
