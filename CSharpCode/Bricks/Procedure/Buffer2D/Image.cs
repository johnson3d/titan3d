using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Procedure.Buffer2D
{
    public class Image
    {
        public float[,] Pixels;
        public void SetSize(int w, int h, float initValue)
        {
            Pixels = new float[h, w];
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    Pixels[i, j] = initValue;
                }
            }
        }
        public Image Clone()
        {
            var result = new Image();
            result.Pixels = new float[Height, Width];
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    Pixels[i, j] = Pixels[i, j];
                }
            }
            return result;
        }
        public void LoadTexture(RName name)
        {
            using (var stream = System.IO.File.OpenRead(name.Address))
            {
                var image = StbImageSharp.ImageResult.FromStream(stream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                Pixels = new float[image.Height, image.Width];
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        Pixels[i, j] = ((float)image.Data[Width * i * 4 + j * 4 + 1]) / 256.0f;
                    }
                }
            }
        }
        public int Width
        {
            get
            {
                return Pixels.GetLength(1);
            }
        }
        public int Height
        {
            get
            {
                return Pixels.GetLength(0);
            }
        }
        public void GetHeghtRange(out float minHeight, out float maxHeight)
        {
            minHeight = float.MaxValue;
            maxHeight = float.MinValue;
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    var f = Pixels[i, j];
                    if (f > maxHeight)
                        maxHeight = f;
                    if (f < minHeight)
                        minHeight = f;
                }
            }
        }
        public RHI.CShaderResourceView CreateAsTexture2D()
        {
            RHI.CTexture2D texture;
            unsafe
            {
                var desc = new ITexture2DDesc();
                desc.SetDefault();
                desc.Width = (uint)Width;
                desc.Height = (uint)Height;
                desc.MipLevels = 1;
                desc.Format = EPixelFormat.PXF_R32_FLOAT;
                ImageInitData initData = new ImageInitData();
                initData.SetDefault();
                desc.InitData = &initData;
                initData.SysMemPitch = desc.Width * sizeof(float);
                fixed (float* p = &Pixels[0, 0])
                {
                    initData.pSysMem = p;
                    texture = UEngine.Instance.GfxDevice.RenderContext.CreateTexture2D(ref desc);
                }
                var rsvDesc = new IShaderResourceViewDesc();
                rsvDesc.m_pTexture2D = texture.mCoreObject.CppPointer;
                rsvDesc.mFormat = EPixelFormat.PXF_R32_FLOAT;
                return UEngine.Instance.GfxDevice.RenderContext.CreateShaderResourceView(ref rsvDesc);
            }
        }
    }
    
}
