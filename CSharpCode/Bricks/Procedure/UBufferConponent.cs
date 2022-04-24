using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Procedure
{
    public class UBufferConponent
    {
        public int LifeCount { get; set; } = 0;
        public int Width { get; private set; }
        public int Height { get; private set; }
        public float[] Pixels;
        public void SetSize(int w, int h, float initValue = 0)
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
        public static bool CopyData(UBufferConponent src, UBufferConponent dst)
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
        public RHI.CShaderResourceView CreateAsHeightMapTexture2D(float minHeight, float maxHeight, EPixelFormat format = EPixelFormat.PXF_R32_FLOAT, bool bNomalized = false)
        {
            RHI.CTexture2D texture;
            if (minHeight >= 0 && minHeight <= 1 && maxHeight >= 0 && maxHeight <= 1)
            {
                minHeight = 0;
                maxHeight = 1;
            }
            float hRange = maxHeight - minHeight;
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
                                if (bNomalized)
                                {
                                    tarPixels[i] = tarPixels[i] / hRange;
                                }
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
                                //tarPixels[i] = HalfHelper.SingleToHalf(Pixels[i]);
                                if (bNomalized)
                                {
                                    tarPixels[i] = HalfHelper.SingleToHalf((Pixels[i] - minHeight) / hRange);
                                }
                                else
                                {
                                    tarPixels[i] = HalfHelper.SingleToHalf(Pixels[i] - minHeight);
                                }
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
                                tarPixels[i].y = (byte)((value >> 8) & 0xFF);
                            }
                            fixed (UInt8_2* p = &tarPixels[0])
                            {
                                initData.SysMemPitch = (uint)(desc.Width * sizeof(UInt8_2));
                                initData.pSysMem = p;
                                texture = UEngine.Instance.GfxDevice.RenderContext.CreateTexture2D(in desc);
                            }
                        }
                        break;
                    case EPixelFormat.PXF_R8_UINT:
                        {//如果希望表达更高的精度，而不是浪费在half上的指数位，可以RG8UNorm格式，让高度信息有65535级别
                            var tarPixels = new Byte[Pixels.Length];
                            float range = maxHeight - minHeight;
                            for (int i = 0; i < Pixels.Length; i++)
                            {
                                var alt = (Byte)Pixels[i];
                            }
                            fixed (Byte* p = &tarPixels[0])
                            {
                                initData.SysMemPitch = (uint)(desc.Width * sizeof(Byte));
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
                rsvDesc.mGpuBuffer = texture.mCoreObject;
                rsvDesc.Format = format;
                rsvDesc.Texture2D.MipLevels = desc.MipLevels;
                var result = UEngine.Instance.GfxDevice.RenderContext.CreateShaderResourceView(in rsvDesc);
                return result;
            }
        }
    }

    public class UPgcBufferCache
    {
        public Dictionary<NodePin, UBufferConponent> CachedBuffers { get; } = new Dictionary<NodePin, UBufferConponent>();
        public void ResetCache()
        {
            CachedBuffers.Clear();
        }
        public UBufferConponent FindBuffer(NodePin pin)
        {
            var node = pin.HostNode as UPgcNodeBase;
            UBufferConponent buffer;
            if (CachedBuffers.TryGetValue(pin, out buffer))
                return buffer;
            var oPin = pin as PinOut;
            if (oPin != null)
            {
                buffer = new UBufferConponent();
                var sz = node.GetOutPinSize(oPin);
                buffer.SetSize(sz.x, sz.y, 0);
                buffer.LifeCount = pin.HostNode.ParentGraph.GetNumOfOutLinker(oPin);
                CachedBuffers.Add(pin, buffer);
                return buffer;
            }
            else
            {
                var linker = pin.HostNode.ParentGraph.FindInLinkerSingle(pin as PinIn);
                if (linker != null)
                {
                    if (CachedBuffers.TryGetValue(linker.OutPin, out buffer))
                        return buffer;
                }
            }
            return null;
        }
        public UBufferConponent RegBuffer(PinOut pin, UBufferConponent buffer)
        {
            UBufferConponent result = null;
            if (CachedBuffers.TryGetValue(pin, out result))
                return result;
            buffer.LifeCount = pin.HostNode.ParentGraph.GetNumOfOutLinker(pin);
            CachedBuffers[pin] = buffer;
            return buffer;
        }
    }
}
