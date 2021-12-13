using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Procedure.Buffer2D
{
    public class ImageConponent
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public float[] Pixels;
        public void SetSize(int w, int h, float initValue)
        {
            Width = w;
            Height = h;

            Pixels = new float[h * w];

            for (int i = 0; i < Pixels.Length; i++)
            {
                Pixels[i] = initValue;
            }
        }
        public float GetPixel(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return float.MaxValue;
            return Pixels[y * Width + x];
        }
        public void SetPixel(int x, int y, float value)
        {
            if (x >= Width || y >= Height)
                return;
            Pixels[y * Width + x] = value;
        }
        public static bool CopyData(ImageConponent src, ImageConponent dst)
        {
            if (src.Width != dst.Width ||
                src.Height != dst.Height)
                return false;
            for (int i = 0; i < src.Pixels.Length; i++)
            {
                dst.Pixels[i] = src.Pixels[i];
            }
            return true;
        }

        public void GetRange(out float minValue, out float maxValue)
        {
            minValue = float.MaxValue;
            maxValue = float.MinValue;
            for (int i = 0; i < Pixels.Length; i++)
            {
                var f = Pixels[i];
                if (f > maxValue)
                    maxValue = f;
                if (f < minValue)
                    minValue = f;
            }
        }
        public void GetMaxAbs(out float maxValue)
        {
            maxValue = float.MinValue;
            for (int i = 0; i < Pixels.Length; i++)
            {
                var f = Pixels[i];
                if (Math.Abs(f) > maxValue)
                    maxValue = f;
            }
        }

        public void LoadTexture(RName name, int xyz)
        {
            using (var stream = System.IO.File.OpenRead(name.Address))
            {
                var image = StbImageSharp.ImageResult.FromStream(stream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                SetSize(image.Height, image.Width, 0);
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        SetPixel(i, j, ((float)image.Data[Width * i * 4 + j * 4 + 1]) / 256.0f);
                    }
                }
            }
        }

        public RHI.CShaderResourceView CreateAsTexture2D(float minHeight, float maxHeight, EPixelFormat format = EPixelFormat.PXF_R32_FLOAT)
        {
            RHI.CTexture2D texture;
            unsafe
            {
                var desc = new ITexture2DDesc();
                desc.SetDefault();
                desc.Width = (uint)Width;
                desc.Height = (uint)Height;
                desc.MipLevels = 1;
                desc.Format = format;
                ImageInitData initData = new ImageInitData();
                initData.SetDefault();
                desc.InitData = &initData;
                switch (format)
                {
                    case EPixelFormat.PXF_R32_FLOAT:
                        {
                            var tarPixels = new float[Pixels.Length];
                            for (int i = 0; i < Pixels.Length; i++)
                            {
                                tarPixels[i] = Pixels[i] - minHeight;
                            }
                            fixed (float* p = &tarPixels[0])
                            {
                                initData.SysMemPitch = desc.Width * sizeof(float);
                                initData.pSysMem = p;
                                texture = UEngine.Instance.GfxDevice.RenderContext.CreateTexture2D(in desc);
                            }
                        }
                        break;
                    case EPixelFormat.PXF_R16_FLOAT:
                        {//高度信息有2的11次方级别精度高度
                            
                            var tarPixels = new Half[Pixels.Length];
                            for (int i = 0; i < Pixels.Length; i++)
                            {
                                tarPixels[i] = HalfHelper.SingleToHalf(Pixels[i] - minHeight);
                            }
                            fixed (Half* p = &tarPixels[0])
                            {
                                initData.SysMemPitch = (uint)(desc.Width * sizeof(Half));
                                initData.pSysMem = p;
                                texture = UEngine.Instance.GfxDevice.RenderContext.CreateTexture2D(in desc);
                            }
                        }
                        break;
                    case EPixelFormat.PXF_R8G8_UNORM:
                        {//如果希望表达更高的精度，而不是浪费在half上的指数位，可以RG8UNorm格式，让高度信息有65535级别
                            var tarPixels = new UInt8_2[Pixels.Length];
                            float range = maxHeight - minHeight;
                            for (int i = 0; i < Pixels.Length; i++)
                            {
                                float alt = Pixels[i] - minHeight;
                                float rate = alt / range;
                                ushort value = (ushort)(rate * (float)ushort.MaxValue);
                                tarPixels[i].x = (byte)(value & 0xFF);
                                tarPixels[i].y = (byte)((value>>8) & 0xFF);
                            }
                            fixed (UInt8_2* p = &tarPixels[0])
                            {
                                initData.SysMemPitch = (uint)(desc.Width * sizeof(UInt8_2));
                                initData.pSysMem = p;
                                texture = UEngine.Instance.GfxDevice.RenderContext.CreateTexture2D(in desc);
                            }
                        }
                        break;
                    default:
                        return null;
                }
                
                
                var rsvDesc = new IShaderResourceViewDesc();
                rsvDesc.SetTexture2D();
                rsvDesc.Type = ESrvType.ST_Texture2D;
                rsvDesc.mGpuBuffer = texture.mCoreObject.NativeSuper;
                rsvDesc.Format = format;
                rsvDesc.Texture2D.MipLevels = desc.MipLevels;
                var result = UEngine.Instance.GfxDevice.RenderContext.CreateShaderResourceView(in rsvDesc);
                return result;
            }
        }
    }
    
    public class Image
    {
        [Flags]
        public enum EImageComponent
        {
            X = 1,
            Y = 1 << 1,
            Z = 1 << 2,
            W = 1 << 3,
            All = X | Y | Z | W,
        }
        public int NumOfComponent
        {
            get
            {
                int result = 0;
                if ((Components & EImageComponent.X) != 0)
                {
                    result++;
                }
                if ((Components & EImageComponent.Y) != 0)
                {
                    result++;
                }
                if ((Components & EImageComponent.Z) != 0)
                {
                    result++;
                }
                if ((Components & EImageComponent.W) != 0)
                {
                    result++;
                }
                return result;
            }
        }
        public EImageComponent Components { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public void Initialize(int w, int h, float defaultValue, EImageComponent comps = EImageComponent.All)
        {
            Width = w;
            Height = h;
            Components = comps;
            if ((comps & EImageComponent.X) != 0)
            {
                CompX = new ImageConponent();
                CompX.SetSize(w, h, defaultValue);
            }
            if ((comps & EImageComponent.Y) != 0)
            {
                CompY = new ImageConponent();
                CompY.SetSize(w, h, defaultValue);
            }
            if ((comps & EImageComponent.Z) != 0)
            {
                CompZ = new ImageConponent();
                CompZ.SetSize(w, h, defaultValue);
            }
            if ((comps & EImageComponent.W) != 0)
            {
                CompW = new ImageConponent();
                CompW.SetSize(w, h, defaultValue);
            }
        }
        public Image Clone()
        {
            var result = new Image();
            result.Initialize(Width, Height, 0, Components);
            if (CompX != null)
            {
                ImageConponent.CopyData(CompX, result.CompX);
            }
            if (CompY != null)
            {
                ImageConponent.CopyData(CompY, result.CompY);
            }
            if (CompZ != null)
            {
                ImageConponent.CopyData(CompZ, result.CompZ);
            }
            if (CompW != null)
            {
                ImageConponent.CopyData(CompW, result.CompW);
            }
            return result;
        }
        public ImageConponent CompX;
        public ImageConponent CompY;
        public ImageConponent CompZ;
        public ImageConponent CompW;
        public ImageConponent GetComponent(EImageComponent comp)
        {
            switch (comp)
            {
                case EImageComponent.X:
                    return CompX;
                case EImageComponent.Y:
                    return CompY;
                case EImageComponent.Z:
                    return CompZ;
                case EImageComponent.W:
                    return CompW;
                default:
                    return null;
            }
        }

        public RHI.CShaderResourceView CreateRGBA8Texture2D()
        {
            RHI.CTexture2D texture;
            unsafe
            {
                var desc = new ITexture2DDesc();
                desc.SetDefault();
                desc.Width = (uint)Width;
                desc.Height = (uint)Height;
                desc.MipLevels = 1;
                desc.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
                ImageInitData initData = new ImageInitData();
                initData.SetDefault();
                desc.InitData = &initData;
                initData.SysMemPitch = desc.Width * (uint)sizeof(Byte4);
                Byte4[,] pixels = new Byte4[Height, Width];
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        Byte4 px = new Byte4();
                        if (CompX != null)
                        {
                            px.X = (byte)(CompX.GetPixel(i, j) * 255.0f);
                        }
                        if (CompY != null)
                        {
                            px.Y = (byte)(CompY.GetPixel(i, j) * 255.0f);
                        }
                        if (CompZ != null)
                        {
                            px.Z = (byte)(CompZ.GetPixel(i, j) * 255.0f);
                        }
                        if (CompW != null)
                        {
                            px.W = (byte)(CompW.GetPixel(i, j) * 255.0f);
                        }
                        pixels[i, j] = px;
                    }
                }
                fixed (Byte4* p = &pixels[0, 0])
                {
                    initData.pSysMem = p;
                    texture = UEngine.Instance.GfxDevice.RenderContext.CreateTexture2D(in desc);
                }
                var rsvDesc = new IShaderResourceViewDesc();
                rsvDesc.SetTexture2D();
                rsvDesc.Type = ESrvType.ST_Texture2D;
                rsvDesc.mGpuBuffer = texture.mCoreObject.NativeSuper;
                rsvDesc.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
                rsvDesc.Texture2D.MipLevels = desc.MipLevels;
                var result = UEngine.Instance.GfxDevice.RenderContext.CreateShaderResourceView(in rsvDesc);
                result.PicDesc = new RHI.CShaderResourceView.UPicDesc();
                return result;
            }
        }
        public RHI.CShaderResourceView CreateRGBA8Texture2DAsNormal()
        {
            RHI.CTexture2D texture;
            unsafe
            {
                var desc = new ITexture2DDesc();
                desc.SetDefault();
                desc.Width = (uint)Width;
                desc.Height = (uint)Height;
                desc.MipLevels = 1;
                desc.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
                ImageInitData initData = new ImageInitData();
                initData.SetDefault();
                desc.InitData = &initData;
                initData.SysMemPitch = desc.Width * (uint)sizeof(Byte4);
                Byte4[,] pixels = new Byte4[Height, Width];
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        Byte4 px = new Byte4();
                        if (CompX != null)
                        {
                            var v = (CompX.GetPixel(i, j) + 1.0f) / 2.0f;
                            px.X = (byte)(v * 255.0f);
                        }
                        if (CompY != null)
                        {
                            var v = (CompY.GetPixel(i, j) + 1.0f) / 2.0f;
                            px.Y = (byte)(v * 255.0f);
                        }
                        if (CompZ != null)
                        {
                            var v = (CompZ.GetPixel(i, j) + 1.0f) / 2.0f;
                            px.Z = (byte)(v * 255.0f);
                        }
                        if (CompW != null)
                        {
                            var v = (CompW.GetPixel(i, j) + 1.0f) / 2.0f;
                            px.W = (byte)(v * 255.0f);
                        }
                        pixels[i, j] = px;
                    }
                }
                fixed (Byte4* p = &pixels[0, 0])
                {
                    initData.pSysMem = p;
                    texture = UEngine.Instance.GfxDevice.RenderContext.CreateTexture2D(in desc);
                }
                var rsvDesc = new IShaderResourceViewDesc();
                rsvDesc.SetTexture2D();
                rsvDesc.Type = ESrvType.ST_Texture2D;
                rsvDesc.mGpuBuffer = texture.mCoreObject.NativeSuper;
                rsvDesc.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
                rsvDesc.Texture2D.MipLevels = desc.MipLevels;
                var result = UEngine.Instance.GfxDevice.RenderContext.CreateShaderResourceView(in rsvDesc);
                result.PicDesc = new RHI.CShaderResourceView.UPicDesc();
                return result;
            }
        }
    }
}
