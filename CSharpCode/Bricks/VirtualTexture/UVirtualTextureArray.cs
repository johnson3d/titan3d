using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.VirtualTexture
{
    public class UVirtualTextureArray : UVirtualTextureBase
    {
        public ITexture2DDesc ArrayDesc;
        public RHI.CTexture2D Tex2DArray;
        public RHI.CShaderResourceView TexArraySRV;
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
                mipLevels = (uint)RHI.CShaderResourceView.CalcMipLevel((int)width, (int)height, true) - 1;
            }
            IRenderContextCaps caps = new IRenderContextCaps();
            rc.mCoreObject.GetRenderContextCaps(ref caps);
            if (arraySize > (uint)caps.MaxTexture2DArray)
            {
                arraySize = (uint)caps.MaxTexture2DArray;
            }
            Textures = new UTextureLayer[arraySize];

            ArrayDesc = new ITexture2DDesc();
            ArrayDesc.SetDefault();
            ArrayDesc.Width = width;
            ArrayDesc.Height = height;
            ArrayDesc.MipLevels = mipLevels;
            ArrayDesc.ArraySize = arraySize;
            ArrayDesc.Format = format;
            Tex2DArray = rc.CreateTexture2D(in ArrayDesc);

            var srvDesc = new IShaderResourceViewDesc();
            srvDesc.SetTexture2D();
            srvDesc.Type = ESrvType.ST_Texture2D;
            srvDesc.Format = format;
            srvDesc.mGpuBuffer = Tex2DArray.mCoreObject;
            srvDesc.Texture2D.MipLevels = mipLevels;

            TexArraySRV = rc.CreateShaderResourceView(in srvDesc);
            return false;
        }        
        public unsafe UTextureLayer PushTexture2D(ICommandList cmd, RName name)
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
            RHI.CShaderResourceView.UPicDesc txDesc = null;
            StbImageSharp.ImageResult[] mipDatas = RHI.CShaderResourceView.LoadPngImageLevels(name, ArrayDesc.MipLevels, ref txDesc);
            if (mipDatas == null || mipDatas.Length == 0)
                return null;
            if (mipDatas[0].Width != ArrayDesc.Width || mipDatas[0].Height != ArrayDesc.Height ||
                ArrayDesc.MipLevels != mipDatas.Length)
                return null;

            Textures[index] = new UTextureLayer();
            Textures[index].Name = name;
            Textures[index].ArrayIndex = index;

            var pixelByte = RHI.CShaderResourceView.GetPixelByteWidth(ArrayDesc.Format);
            for (uint i = 0; i < ArrayDesc.MipLevels; i++)
            {
                fixed (byte* pSrcData = &mipDatas[i].Data[0])
                {
                    Tex2DArray.GetTextureCore().UpdateMipData(cmd, (uint)index, i, 
                        pSrcData, (uint)mipDatas[i].Width, (uint)mipDatas[i].Height, (uint)mipDatas[i].Width * pixelByte);
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
