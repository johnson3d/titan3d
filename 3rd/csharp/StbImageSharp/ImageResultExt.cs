using System;
using System.Collections.Generic;
using System.Text;

namespace StbImageSharp
{
    partial class TtMemImage
    {
        public static unsafe TtMemImage CreateImage(int width, int height, ColorComponents comp)
        {
            var image = new TtMemImage
            {
                Width = width,
                Height = height,
                SourceComp = comp,
                Comp = comp
            };
            int bitWisePerPixel = 0;
            switch(comp)
            {
                case ColorComponents.Grey:
                    {
                        bitWisePerPixel = 8;
                    }
                    break;
                case ColorComponents.GreyAlpha:
                    {
                        bitWisePerPixel = 16;
                    }
                    break;
                case ColorComponents.RedGreenBlue:
                    {
                        bitWisePerPixel = 24;
                    }
                    break;
                case ColorComponents.RedGreenBlueAlpha:
                    {
                        bitWisePerPixel = 32;
                    }
                    break;
            }
            image.Data = new byte[width * height * (bitWisePerPixel / 8)];
            return image;
        }
        public static unsafe TtMemImage CreateImageRGBA(byte* pPixels, in EngineNS.NxRHI.FSubResourceFootPrint fp)
        {
            if (pPixels == null)
                throw new InvalidOperationException(StbImage.stbi__g_failure_reason);

            var image = new TtMemImage
            {
                Width = (int)fp.Width,
                Height = (int)fp.Height,
                SourceComp= ColorComponents.RedGreenBlueAlpha,
                Comp = ColorComponents.RedGreenBlueAlpha,
            };

            image.Data = new byte[image.Width * image.Height * (int)image.Comp];
            int copySize = image.Width * 4;
            int imageStride = image.Width * 4;
            fixed (byte* pTar = &image.Data[0])
            {
                switch (fp.Format)
                {
                    case EngineNS.EPixelFormat.PXF_R8G8B8A8_UNORM:
                        {
                            var dst = pTar;
                            var row = pPixels;
                            for (int i = 0; i < image.Height; i++)
                            {
                                EngineNS.CoreSDK.MemoryCopy(dst, row, (uint)copySize);
                                row += fp.RowPitch;
                                dst += imageStride;
                            }
                        }
                        break;
                    case EngineNS.EPixelFormat.PXF_B8G8R8A8_UNORM:
                        {
                            var dst = pTar;
                            var row = pPixels;
                            for (int i = 0; i < image.Height; i++)
                            {
                                for (int j = 0; j < image.Width; j++)
                                {
                                    dst[j * 4 + 3] = row[j * 4];
                                    dst[j * 4 + 2] = row[j * 4 + 1];
                                    dst[j * 4 + 1] = row[j * 4 + 2];
                                    dst[j * 4 + 0] = row[j * 4 + 3];
                                }
                                row += fp.RowPitch;
                                dst += imageStride;
                            }
                        }
                        break;
                    case EngineNS.EPixelFormat.PXF_R10G10B10A2_UNORM:
                        {
                            var dst = pTar;
                            var row = pPixels;
                            for (int i = 0; i < image.Height; i++)
                            {
                                for (int j = 0; j < image.Width; j++)
                                {
                                    uint color = *(uint*)(&row[j * 4]);
                                    uint channel = (color & 0x3FF);
                                    dst[j * 4] = (byte)(channel * 255 / 1024);

                                    channel = ((color >> 10) & 0x3FF);
                                    dst[j * 4 + 1] = (byte)(channel * 255 / 1024);

                                    channel = ((color >> 20) & 0x3FF);
                                    dst[j * 4 + 2] = (byte)(channel * 255 / 1024);

                                    channel = ((color >> 30) & 0x3);
                                    dst[j * 4 + 3] = (byte)(channel * 255 / 4);
                                }
                                row += fp.RowPitch;
                                dst += imageStride;
                            }
                        }
                        break;
                    case EngineNS.EPixelFormat.PXF_R16G16B16A16_FLOAT:
                        {
                            var dst = pTar;
                            var row = pPixels;
                            for (int i = 0; i < image.Height; i++)
                            {
                                var color = (EngineNS.Half*)row;
                                for (int j = 0; j < image.Width; j++)
                                {
                                    var f1 = (float)color[j * 4];
                                    var f2 = (float)color[j * 4 + 1];
                                    var f3 = (float)color[j * 4 + 2];
                                    var f4 = (float)color[j * 4 + 3];
                                    if (f1 > 1.0f)
                                        dst[j * 4] = 255;
                                    else
                                        dst[j * 4] = (byte)(f1 * 255.0f);
                                    if (f2 > 1.0f)
                                        dst[j * 4] = 255;
                                    else
                                        dst[j * 4 + 1] = (byte)(f2 * 255.0f);
                                    if (f3 > 1.0f)
                                        dst[j * 4] = 255;
                                    else
                                        dst[j * 4 + 2] = (byte)(f3 * 255.0f);
                                    if (f4 > 1.0f)
                                        dst[j * 4] = 255;
                                    else
                                        dst[j * 4 + 3] = (byte)(f4 * 255.0f);
                                }
                                row += fp.RowPitch;
                                dst += imageStride;
                            }
                        }
                        break;
                    default:
                        return null;
                }
            }

            return image;
        }
        public unsafe bool SavePng(string file)
        {
            using (var memStream = new System.IO.FileStream(file, System.IO.FileMode.OpenOrCreate))
            {
                if (memStream==null)
                    return false;
                var writer = new StbImageWriteSharp.ImageWriter();
                writer.WritePng(Data, Width, Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, memStream);
            }
            return true;
        }
        public void Clear(EngineNS.Color4b color)
        {
            switch (Comp)
            {
                case ColorComponents.Grey:
                    {
                        for (int y = 0; y < Height; y++)
                        {
                            for (int x = 0; x < Width; x++)
                            {
                                Data[y * Width + x] = color.R;
                            }
                        }
                    }
                    break;
                case ColorComponents.GreyAlpha:
                    {
                        for (int y = 0; y < Height; y++)
                        {
                            for (int x = 0; x < Width; x++)
                            {
                                int index =(y * Width + x) * 2;
                                Data[index] = color.R;
                                Data[index + 1] = color.A;
                            }
                        }
                    }
                    break;
                case ColorComponents.RedGreenBlue:
                    {
                        for (int y = 0; y < Height; y++)
                        {
                            for (int x = 0; x < Width; x++)
                            {
                                int index = (y * Width + x) * 3;
                                Data[index] = color.R;
                                Data[index + 1] = color.G;
                                Data[index + 2] = color.B;
                            }
                        }
                    }
                    break;
                case ColorComponents.RedGreenBlueAlpha:
                    {
                        for (int y = 0; y < Height; y++)
                        {
                            for (int x = 0; x < Width; x++)
                            {
                                int index = (y * Width + x) * 4;
                                Data[index] = color.R;
                                Data[index + 1] = color.G;
                                Data[index + 2] = color.B;
                                Data[index + 3] = color.A;
                            }
                        }
                    }
                    break;
            }
        }
        public void SetPixel(int x, int y, EngineNS.Color4b color)
        {
            switch (Comp)
            {
                case ColorComponents.Grey:
                    {
                        Data[y * Width + x] = color.R;
                    }
                    break;
                case ColorComponents.GreyAlpha:
                    {
                        var index = (y * Width + x) * 2;
                        Data[index] = color.R;
                        Data[index + 1] = color.A;
                    }
                    break;
                case ColorComponents.RedGreenBlue:
                    {
                        var index = (y * Width + x) * 3;
                        Data[index] = color.R;
                        Data[index + 1] = color.G;
                        Data[index + 2] = color.B;
                    }
                    break;
                case ColorComponents.RedGreenBlueAlpha:
                    {
                        var index = (y * Width + x) * 4;
                        Data[index] = color.R;
                        Data[index + 1] = color.G;
                        Data[index + 2] = color.B;
                        Data[index + 3] = color.A;
                    }
                    break;
            }
        }
        public EngineNS.Color4b GetPixel(int x, int y)
        {
            var color = new EngineNS.Color4b();
            switch (Comp)
            {
                case ColorComponents.Grey:
                    {
                        color.R = Data[y * Width + x];
                    }
                    break;
                case ColorComponents.GreyAlpha:
                    {
                        var index = (y * Width + x) * 2;
                        color.R = Data[index];
                        color.A = Data[index + 1];
                    }
                    break;
                case ColorComponents.RedGreenBlue:
                    {
                        var index = (y * Width + x) * 3;
                        color.R = Data[index];
                        color.G = Data[index + 1];
                        color.B = Data[index + 2];
                    }
                    break;
                case ColorComponents.RedGreenBlueAlpha:
                    {
                        var index = (y * Width + x) * 4;
                        color.R = Data[index];
                        color.G = Data[index + 1];
                        color.B = Data[index + 2];
                        color.A = Data[index + 3];
                    }
                    break;
            }
            return color;
        }
    }
}
