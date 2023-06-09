using System;
using System.Collections.Generic;
using System.Text;

namespace StbImageSharp
{
    partial class ImageResult
    {
        public static unsafe ImageResult CreateImage(int width, int height, ColorComponents comp)
        {
            var image = new ImageResult
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

        public void Clear(EngineNS.Color color)
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
        public void SetPixel(int x, int y, EngineNS.Color color)
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
        public EngineNS.Color GetPixel(int x, int y)
        {
            var color = new EngineNS.Color();
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
