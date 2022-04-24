using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Terrain.CDLOD
{
    public class UTerrainMaterialId : IO.BaseSerializer
    {
        [Rtti.Meta]
        [RName.PGRName(FilterExts = RHI.CShaderResourceView.AssetExt)]
        public RName TexDiffuse { get; set; }
        [Rtti.Meta]
        [RName.PGRName(FilterExts = RHI.CShaderResourceView.AssetExt)]
        public RName TexNormal { get; set; }
    }
    public class UTerrainMaterialIdManager : IO.BaseSerializer
    {
        [Rtti.Meta]
        public List<Terrain.CDLOD.UTerrainMaterialId> MaterialIdArray { get; set; } = new List<Terrain.CDLOD.UTerrainMaterialId>();
        public RHI.CTexture2D DiffuseTextureArray;
        public RHI.CTexture2D NormalTextureArray;
        public RHI.CShaderResourceView DiffuseTextureArraySRV;
        public RHI.CShaderResourceView NormalTextureArraySRV;
        public void Cleanup()
        {
            DiffuseTextureArraySRV?.Dispose();
            DiffuseTextureArraySRV = null;

            NormalTextureArraySRV?.Dispose();
            NormalTextureArraySRV = null;

            DiffuseTextureArray?.Dispose();
            DiffuseTextureArray = null;

            NormalTextureArray?.Dispose();
            NormalTextureArray = null;
        }
        public bool BuildSRV(ICommandList cmd)
        {
            Cleanup();

            if (MaterialIdArray.Count == 0)
                return false;
            var dftLayer = MaterialIdArray[0];
            RHI.CShaderResourceView.UPicDesc txDesc = RHI.CShaderResourceView.LoadPicDesc(dftLayer.TexDiffuse);

            var desc = new ITexture2DDesc();
            desc.SetDefault();
            desc.Format = EPixelFormat.PXF_B8G8R8A8_UNORM;
            desc.Width = (uint)txDesc.Width;
            desc.Height = (uint)txDesc.Height;
            desc.MipLevels = (uint)txDesc.MipLevel + 1;
            desc.ArraySize = (uint)MaterialIdArray.Count;
            
            DiffuseTextureArray = UEngine.Instance.GfxDevice.RenderContext.CreateTexture2D(in desc);
            var srvDesc = new IShaderResourceViewDesc();
            srvDesc.SetTexture2DArray();
            srvDesc.Texture2DArray.ArraySize = desc.ArraySize;
            srvDesc.Texture2DArray.FirstArraySlice = 0;
            srvDesc.Texture2DArray.MipLevels = desc.MipLevels;
            srvDesc.Texture2DArray.MostDetailedMip = 0;
            srvDesc.mGpuBuffer = DiffuseTextureArray.mCoreObject;
            DiffuseTextureArraySRV = UEngine.Instance.GfxDevice.RenderContext.CreateShaderResourceView(in srvDesc);

            unsafe
            {
                for (int i = 0; i < MaterialIdArray.Count; i++)
                {
                    
                    var mipDatas = RHI.CShaderResourceView.LoadPngImageLevels(MaterialIdArray[i].TexDiffuse, 0, ref txDesc);
                    if (txDesc.Width != desc.Width || txDesc.Height != desc.Height || mipDatas.Length != desc.m_MipLevels)
                    {
                        continue;
                    }
                    for (int j = 0; j < desc.m_MipLevels; j++)
                    {
                        fixed (byte* pSrcData = &mipDatas[j].Data[0])
                        {
                            DiffuseTextureArray.GetTextureCore().UpdateMipData(cmd, (uint)i, (uint)j, 
                                pSrcData, (uint)mipDatas[j].Width, (uint)mipDatas[j].Height, (uint)(mipDatas[j].Width * sizeof(uint)));
                        }
                    }
                }
            }

            return true;
        }
    }
}
