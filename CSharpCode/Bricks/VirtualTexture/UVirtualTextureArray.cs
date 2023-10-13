using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.VirtualTexture
{
    public class UVirtualTextureArray : UVirtualTextureBase
    {
        public NxRHI.FTextureDesc ArrayDesc;
        public NxRHI.UTexture Tex2DArray;
        public NxRHI.USrView TexArraySRV;
        public class UTextureLayer
        {
            public RName Name;
            public int ArrayIndex;
        }
        public UTextureLayer[] Textures;
        public bool CreateRVT(uint width, uint height, uint mipLevels, EPixelFormat format, uint arraySize)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            if (mipLevels == uint.MaxValue)
            {
                mipLevels = (uint)NxRHI.USrView.CalcMipLevel((int)width, (int)height, true) - 1;
            }
            //IRenderContextCaps caps = new IRenderContextCaps();
            //rc.mCoreObject.GetRenderContextCaps(ref caps);
            //if (arraySize > (uint)caps.MaxTexture2DArray)
            //{
            //    arraySize = (uint)caps.MaxTexture2DArray;
            //}
            //Textures = new UTextureLayer[arraySize];

            //ArrayDesc = new NxRHI.FTextureDesc();
            //ArrayDesc.SetDefault();
            //ArrayDesc.Width = width;
            //ArrayDesc.Height = height;
            //ArrayDesc.MipLevels = mipLevels;
            //ArrayDesc.ArraySize = arraySize;
            //ArrayDesc.Format = format;
            //Tex2DArray = rc.CreateTexture2D(in ArrayDesc);

            //var srvDesc = new IShaderResourceViewDesc();
            //srvDesc.SetTexture2D();
            //srvDesc.Type = ESrvType.ST_Texture2D;
            //srvDesc.Format = format;
            //srvDesc.mGpuBuffer = Tex2DArray.mCoreObject;
            //srvDesc.Texture2D.MipLevels = mipLevels;

            //TexArraySRV = rc.CreateShaderResourceView(in srvDesc);
            return false;
        }        
        public unsafe UTextureLayer PushTexture2D(NxRHI.UCommandList cmd, RName name)
        {
            int index = -1;
            for (int i = 0; i < Textures.Length; i++)
            {
                if (Textures[i] == null)
                {
                    if (index == -1)
                    {
                        index = i;
                    }
                    continue;
                }
                else if (Textures[i].Name == name)
                {
                    return Textures[i];
                }
            }
            if (index == -1)
                return null;
            NxRHI.USrView.UPicDesc txDesc = null;
            StbImageSharp.ImageResult[] mipDatas = NxRHI.USrView.LoadImageLevels(name, ArrayDesc.MipLevels, ref txDesc);
            if (mipDatas == null || mipDatas.Length == 0)
                return null;
            if (mipDatas[0].Width != ArrayDesc.Width || mipDatas[0].Height != ArrayDesc.Height ||
                ArrayDesc.MipLevels != mipDatas.Length)
                return null;

            Textures[index] = new UTextureLayer();
            Textures[index].Name = name;
            Textures[index].ArrayIndex = index;

            var pixelByte = NxRHI.USrView.GetPixelByteWidth(ArrayDesc.Format);
            for (uint i = 0; i < ArrayDesc.MipLevels; i++)
            {
                fixed (byte* pSrcData = &mipDatas[i].Data[0])
                {
                    var fp = new NxRHI.FSubResourceFootPrint();
                    fp.SetDefault();
                    fp.Width = (uint)mipDatas[i].Width;
                    fp.Height = (uint)mipDatas[i].Height;
                    fp.Format = ArrayDesc.Format;
                    fp.RowPitch = (uint)(mipDatas[i].Width * sizeof(uint));
                    fp.TotalSize = (uint)(mipDatas[i].Width * mipDatas[i].Height * pixelByte);

                    Tex2DArray.UpdateGpuData(cmd.mCoreObject, (uint)i, pSrcData, &fp);
                }
            }
            return Textures[index];
        }
        public void PopTexture2D(UTextureLayer layer)
        {
            if (layer == null)
                return;
            if (layer.ArrayIndex < 0)
                return;
            Textures[layer.ArrayIndex] = null;
            layer.ArrayIndex = -1;
        }
    }
}
