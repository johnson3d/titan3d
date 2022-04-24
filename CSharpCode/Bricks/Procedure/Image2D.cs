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
        public void Initialize(int w, int h, float defaultValue, EImageComponent comps = EImageComponent.All)
        {
            Width = w;
            Height = h;
            Components = comps;
            if ((comps & EImageComponent.X) != 0)
            {
                CompX = new UBufferConponent();
                CompX.SetSize(w, h, defaultValue);
            }
            if ((comps & EImageComponent.Y) != 0)
            {
                CompY = new UBufferConponent();
                CompY.SetSize(w, h, defaultValue);
            }
            if ((comps & EImageComponent.Z) != 0)
            {
                CompZ = new UBufferConponent();
                CompZ.SetSize(w, h, defaultValue);
            }
            if ((comps & EImageComponent.W) != 0)
            {
                CompW = new UBufferConponent();
                CompW.SetSize(w, h, defaultValue);
            }
        }
        public void Initialize(int w, int h, UBufferConponent xComp, UBufferConponent yComp, UBufferConponent zComp, UBufferConponent wComp)
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
                UBufferConponent.CopyData(CompX, result.CompX);
            }
            if (CompY != null)
            {
                UBufferConponent.CopyData(CompY, result.CompY);
            }
            if (CompZ != null)
            {
                UBufferConponent.CopyData(CompZ, result.CompZ);
            }
            if (CompW != null)
            {
                UBufferConponent.CopyData(CompW, result.CompW);
            }
            return result;
        }
        public UBufferConponent CompX;
        public UBufferConponent CompY;
        public UBufferConponent CompZ;
        public UBufferConponent CompW;
        public UBufferConponent GetComponent(EImageComponent comp)
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

        public RHI.CShaderResourceView CreateRGBA8Texture2D(bool bNormalized = true)
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
                                px.X = (byte)((CompX.GetPixel(j, i) - minX) / rangeX * 255.0f);
                            }
                            if (CompY != null)
                            {
                                px.Y = (byte)((CompY.GetPixel(j, i) - minY) / rangeY * 255.0f);
                            }
                            if (CompZ != null)
                            {
                                px.Z = (byte)((CompZ.GetPixel(j, i) - minZ) / rangeZ * 255.0f);
                            }
                            if (CompW != null)
                            {
                                px.W = (byte)((CompW.GetPixel(j, i) - minW) / rangeW * 255.0f);
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
                                px.X = (byte)CompX.GetPixel(j, i);
                            }
                            if (CompY != null)
                            {
                                px.Y = (byte)CompY.GetPixel(j, i);
                            }
                            if (CompZ != null)
                            {
                                px.Z = (byte)CompZ.GetPixel(j, i);
                            }
                            if (CompW != null)
                            {
                                px.W = (byte)CompW.GetPixel(j, i);
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
                    initData.pSysMem = p;
                    texture = UEngine.Instance.GfxDevice.RenderContext.CreateTexture2D(in desc);
                }
                var rsvDesc = new IShaderResourceViewDesc();
                rsvDesc.SetTexture2D();
                rsvDesc.Type = ESrvType.ST_Texture2D;
                rsvDesc.mGpuBuffer = texture.mCoreObject;
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
                            var v = (CompX.GetPixel(j, i) + 1.0f) / 2.0f;
                            px.X = (byte)(v * 255.0f);
                        }
                        if (CompY != null)
                        {
                            var v = (CompY.GetPixel(j, i) + 1.0f) / 2.0f;
                            px.Y = (byte)(v * 255.0f);
                        }
                        if (CompZ != null)
                        {
                            var v = (CompZ.GetPixel(j, i) + 1.0f) / 2.0f;
                            px.Z = (byte)(v * 255.0f);
                        }
                        if (CompW != null)
                        {
                            var v = (CompW.GetPixel(j, i) + 1.0f) / 2.0f;
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
                rsvDesc.mGpuBuffer = texture.mCoreObject;
                rsvDesc.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
                rsvDesc.Texture2D.MipLevels = desc.MipLevels;
                var result = UEngine.Instance.GfxDevice.RenderContext.CreateShaderResourceView(in rsvDesc);
                result.PicDesc = new RHI.CShaderResourceView.UPicDesc();
                return result;
            }
        }
    }
}
