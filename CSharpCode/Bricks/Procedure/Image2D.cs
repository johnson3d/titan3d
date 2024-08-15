using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Procedure
{
    public class UImage2D
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
        public unsafe void Initialize(int w, int h, float defaultValue, EImageComponent comps = EImageComponent.All)
        {
            Width = w;
            Height = h;
            Components = comps;
            if ((comps & EImageComponent.X) != 0)
            {
                var creator = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(w, h, 1);
                CompX = UBufferComponent.CreateInstance(in creator) as USuperBuffer<float, FFloatOperator>;
            }
            if ((comps & EImageComponent.Y) != 0)
            {
                var creator = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(w, h, 1);
                CompY = UBufferComponent.CreateInstance(in creator) as USuperBuffer<float, FFloatOperator>;
            }
            if ((comps & EImageComponent.Z) != 0)
            {
                var creator = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(w, h, 1);
                CompZ = UBufferComponent.CreateInstance(in creator) as USuperBuffer<float, FFloatOperator>;
            }
            if ((comps & EImageComponent.W) != 0)
            {
                var creator = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(w, h, 1);
                CompW = UBufferComponent.CreateInstance(in creator) as USuperBuffer<float, FFloatOperator>;
            }
        }
        public void Initialize(int w, int h,
            USuperBuffer<Vector3, FFloat3Operator> xyzComp,
            USuperBuffer<float, FFloatOperator> wComp, int slice = 0)
        {
            Width = w;
            Height = h;

            Components = EImageComponent.X | EImageComponent.Y | EImageComponent.Z;
            if (wComp != null)
            {
                CompW = wComp;
                Components |= EImageComponent.W;
            }

            var creator = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(w, h, 1);
            CompX = UBufferComponent.CreateInstance(in creator) as USuperBuffer<float, FFloatOperator>;
            CompY = UBufferComponent.CreateInstance(in creator) as USuperBuffer<float, FFloatOperator>;
            CompZ = UBufferComponent.CreateInstance(in creator) as USuperBuffer<float, FFloatOperator>;

            slice = slice % xyzComp.Slice;

            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    
                    var src = xyzComp.GetPixel<Vector3>(j, i, slice);
                    CompX.SetPixel<float>(j, i, src.X);
                    CompY.SetPixel<float>(j, i, src.Y);
                    CompZ.SetPixel<float>(j, i, src.Z);
                }
            }
        }
        public void Initialize(int w, int h, 
            USuperBuffer<float, FFloatOperator> xComp, 
            USuperBuffer<float, FFloatOperator> yComp, 
            USuperBuffer<float, FFloatOperator> zComp, 
            USuperBuffer<float, FFloatOperator> wComp)
        {
            Width = w;
            Height = h;
            Components = 0;
            if (xComp != null)
            {
                CompX = xComp;
                Components |= EImageComponent.X;
            }
            if (yComp != null)
            {
                CompY = yComp;
                Components |= EImageComponent.Y;
            }
            if (zComp != null)
            {
                CompZ = zComp;
                Components |= EImageComponent.Z;
            }
            if (wComp != null)
            {
                CompW = wComp;
                Components |= EImageComponent.W;
            }
        }
        public UImage2D Clone()
        {
            var result = new UImage2D();
            result.Initialize(Width, Height, 0, Components);
            if (CompX != null)
            {
                UBufferComponent.CopyData(CompX, result.CompX);
            }
            if (CompY != null)
            {
                UBufferComponent.CopyData(CompY, result.CompY);
            }
            if (CompZ != null)
            {
                UBufferComponent.CopyData(CompZ, result.CompZ);
            }
            if (CompW != null)
            {
                UBufferComponent.CopyData(CompW, result.CompW);
            }
            return result;
        }
        public USuperBuffer<float, FFloatOperator> CompX;
        public USuperBuffer<float, FFloatOperator> CompY;
        public USuperBuffer<float, FFloatOperator> CompZ;
        public USuperBuffer<float, FFloatOperator> CompW;
        public USuperBuffer<float, FFloatOperator> GetComponent(EImageComponent comp)
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

        public NxRHI.USrView CreateRGBA8Texture2D(bool bNormalized = true)
        {
            NxRHI.UTexture texture;
            unsafe
            {
                var desc = new NxRHI.FTextureDesc();
                desc.SetDefault();
                desc.Width = (uint)Width;
                desc.Height = (uint)Height;
                desc.MipLevels = 1;
                desc.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
                var initData = new NxRHI.FMappedSubResource();
                initData.SetDefault();
                desc.InitData = &initData;
                initData.m_RowPitch = desc.Width * (uint)sizeof(Byte4);

                float minX = 0, maxX = 0;
                if (CompX != null)
                {
                    CompX.GetRange(out minX, out maxX);
                }
                if (minX >= 0 && minX <= 1 && maxX >= 0 && maxX <= 1)
                {
                    minX = 0;
                    maxX = 1;
                }
                float rangeX = maxX - minX;                
                float minY = 0, maxY = 0;
                if (CompY != null)
                {
                    CompY.GetRange(out minY, out maxY);
                }
                if (minY >= 0 && minY <= 1 && maxY >= 0 && maxY <= 1)
                {
                    minY = 0;
                    maxY = 1;
                }
                float rangeY = maxY - minY;
                float minZ = 0, maxZ = 0;
                if (CompZ != null)
                {
                    CompZ.GetRange(out minZ, out maxZ);
                }
                if (minZ >= 0 && minZ <= 1 && maxZ >= 0 && maxZ <= 1)
                {
                    minZ = 0;
                    maxZ = 1;
                }
                float rangeZ = maxZ - minZ;
                float minW = 0, maxW = 0;
                if (CompW != null)
                {
                    CompW.GetRange(out minW, out maxW);
                }
                if (minW >= 0 && minW <= 1 && maxW >= 0 && maxW <= 1)
                {
                    minW = 0;
                    maxW = 1;
                }
                float rangeW = maxW - minW;
                Byte4[,] pixels = new Byte4[Height, Width];
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        Byte4 px = new Byte4();
                        if (bNormalized)
                        {
                            if (CompX != null)
                            {
                                px.X = (byte)((CompX.GetPixel<float>(j, i) - minX) / rangeX * 255.0f);
                            }
                            if (CompY != null)
                            {
                                px.Y = (byte)((CompY.GetPixel<float>(j, i) - minY) / rangeY * 255.0f);
                            }
                            if (CompZ != null)
                            {
                                px.Z = (byte)((CompZ.GetPixel<float>(j, i) - minZ) / rangeZ * 255.0f);
                            }
                            if (CompW != null)
                            {
                                px.W = (byte)((CompW.GetPixel<float>(j, i) - minW) / rangeW * 255.0f);
                            }
                            else
                            {
                                px.W = 255;
                            }
                        }
                        else
                        {
                            if (CompX != null)
                            {
                                px.X = (byte)CompX.GetPixel<float>(j, i);
                            }
                            if (CompY != null)
                            {
                                px.Y = (byte)CompY.GetPixel<float>(j, i);
                            }
                            if (CompZ != null)
                            {
                                px.Z = (byte)CompZ.GetPixel<float>(j, i);
                            }
                            if (CompW != null)
                            {
                                px.W = (byte)CompW.GetPixel<float>(j, i);
                            }
                            else
                            {
                                px.W = 255;
                            }
                        }
                        pixels[i, j] = px;
                    }
                }
                fixed (Byte4* p = &pixels[0, 0])
                {
                    initData.pData = p;
                    texture = TtEngine.Instance.GfxDevice.RenderContext.CreateTexture(in desc);
                }
                var rsvDesc = new NxRHI.FSrvDesc();
                rsvDesc.SetTexture2D();
                rsvDesc.Type = NxRHI.ESrvType.ST_Texture2D;
                rsvDesc.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
                rsvDesc.Texture2D.MipLevels = desc.MipLevels;
                var result = TtEngine.Instance.GfxDevice.RenderContext.CreateSRV(texture, in rsvDesc);
                result.PicDesc = new NxRHI.USrView.UPicDesc();
                return result;
            }
        }
        public NxRHI.USrView CreateRGBA8Texture2DAsNormal(out NxRHI.UTexture texture)
        {
            unsafe
            {
                var desc = new NxRHI.FTextureDesc();
                desc.SetDefault();
                desc.Width = (uint)Width;
                desc.Height = (uint)Height;
                desc.MipLevels = 1;
                desc.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
                var initData = new NxRHI.FMappedSubResource();
                initData.SetDefault();
                desc.InitData = &initData;
                initData.m_RowPitch = desc.Width * (uint)sizeof(Byte4);
                Byte4[,] pixels = new Byte4[Height, Width];
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        Byte4 px = new Byte4();
                        if (CompX != null)
                        {
                            var v = CompX.GetPixel<float>(j, i);
                            v = (v + 1.0f) / 2.0f;
                            System.Diagnostics.Debug.Assert(v >= 0 && v <= 1.0f);
                            px.X = (byte)(v * 255.0f);
                        }
                        if (CompY != null)
                        {
                            var v = CompY.GetPixel<float>(j, i);
                            v = (v + 1.0f) / 2.0f;
                            System.Diagnostics.Debug.Assert(v >= 0 && v <= 1.0f);
                            px.Y = (byte)(v * 255.0f);
                        }
                        if (CompZ != null)
                        {
                            var v = CompZ.GetPixel<float>(j, i);
                            v = (v + 1.0f) / 2.0f;
                            System.Diagnostics.Debug.Assert(v >= 0 && v <= 1.0f);
                            px.Z = (byte)(v * 255.0f);
                        }
                        if (CompW != null)
                        {
                            var v = CompW.GetPixel<float>(j, i);
                            v = (v + 1.0f) / 2.0f;
                            px.W = (byte)(v * 255.0f);
                        }
                        pixels[i, j] = px;
                    }
                }
                fixed (Byte4* p = &pixels[0, 0])
                {
                    initData.pData = p;
                    texture = TtEngine.Instance.GfxDevice.RenderContext.CreateTexture(in desc);
                }
                var rsvDesc = new NxRHI.FSrvDesc();
                rsvDesc.SetTexture2D();
                rsvDesc.Type = NxRHI.ESrvType.ST_Texture2D;
                rsvDesc.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
                rsvDesc.Texture2D.MipLevels = desc.MipLevels;
                var result = TtEngine.Instance.GfxDevice.RenderContext.CreateSRV(texture, in rsvDesc);
                if (result != null)
                    result.PicDesc = new NxRHI.USrView.UPicDesc();
                return result;
            }
        }
    }
}
