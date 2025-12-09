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
            public Color4b[] ToColor4b()
            {
                Color4b[] result = new Color4b[Pixels.Length];
                for (int i = 0; i<Pixels.Length; i++)
                {
                    result[i] = Pixels[i].ToColor4b();
                }
                return result;
            }
            public unsafe Color4b* CreateColor4b()
            {
                Color4b* result = (Color4b*)CoreSDK.Alloc((uint)(sizeof(Color4b) * Pixels.Length), null, 0);
                for (int i = 0; i<Pixels.Length; i++)
                {
                    result[i] = Pixels[i].ToColor4b();
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
            public Color4b[] ToColor4b()
            {
                Color4b[] result = new Color4b[Pixels.Length];
                for (int i = 0; i<Pixels.Length; i++)
                {
                    result[i] = Pixels[i].ToColor4b();
                }
                return result;
            }
            public unsafe Color4b* CreateColor4b()
            {
                Color4b* result = (Color4b*)CoreSDK.Alloc((uint)(sizeof(Color4b) * Pixels.Length), null, 0);
                for (int i = 0; i<Pixels.Length; i++)
                {
                    result[i] = Pixels[i].ToColor4b();
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
