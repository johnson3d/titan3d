using System;
using System.Collections.Generic;
using System.Text;

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
}
