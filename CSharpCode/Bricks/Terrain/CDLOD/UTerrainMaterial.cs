using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Terrain.CDLOD
{
    public partial class UTerrainMaterialId : IO.BaseSerializer
    {
        public override string ToString()
        {
            var result = "";
            if (TexDiffuse != null)
                result += TexDiffuse.Name + TexDiffuse.RNameType.ToString();
            if (TexNormal != null)
                result += TexNormal.Name + TexNormal.RNameType.ToString();
            result += TransitionRange;
            result += NullPlantDensity;
            foreach(var i in Plants)
            {
                result += i.ToString();
            }
            return result;
        }

        [Rtti.Meta]
        [RName.PGRName(FilterExts = RHI.CShaderResourceView.AssetExt)]
        public RName TexDiffuse { get; set; }
        [Rtti.Meta]
        [RName.PGRName(FilterExts = RHI.CShaderResourceView.AssetExt)]
        public RName TexNormal { get; set; }
        [Rtti.Meta]
        public float TransitionRange { get; set; } = 5.0f;

        List<UTerrainPlant> mPlants = new List<UTerrainPlant>();
        [Rtti.Meta]
        public List<UTerrainPlant> Plants 
        {
            get => mPlants;
            set
            {
                mPlants = value;
                UpdateTotalDensity();
            }
        }
        int mTotalDensity;
        public int TotalDensity 
        {
            get => mTotalDensity;
            set
            {
                UpdateTotalDensity();
            }
        }
        public void UpdateTotalDensity()
        {
            lock (Plants)
            {
                mTotalDensity = 0;
                if (Plants.Count > 0)
                {
                    mTotalDensity = NullPlantDensity;
                    foreach (var i in Plants)
                    {
                        mTotalDensity += i.Density;
                    }

                    Plants.Sort((x, y) =>
                    {
                        return x.Density.CompareTo(y);
                    });
                }
            }
        }
        [Rtti.Meta]
        public int NullPlantDensity { get; set; } = 100000;
        [Rtti.Meta]
        public int GetRandomPlant(int rdValue)
        {
            rdValue = rdValue % TotalDensity;
            if (rdValue < NullPlantDensity)
                return -1;
            int curTotal = NullPlantDensity;
            for (int i = 0; i < Plants.Count; i++)
            {
                curTotal += Plants[i].Density;
                if (rdValue < curTotal)
                    return i;
            }
            return -1;
        }
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
            {
                RHI.CShaderResourceView.UPicDesc txDesc = RHI.CShaderResourceView.LoadPicDesc(dftLayer.TexDiffuse);

                var desc = new ITexture2DDesc();
                desc.SetDefault();
                //desc.Format = EPixelFormat.PXF_B8G8R8A8_UNORM;
                desc.Width = (uint)txDesc.Width;
                desc.Height = (uint)txDesc.Height;
                desc.MipLevels = (uint)txDesc.MipLevel + 1;
                desc.ArraySize = (uint)MaterialIdArray.Count;

                DiffuseTextureArray = UEngine.Instance.GfxDevice.RenderContext.CreateTexture2D(in desc);
                var srvDesc = new IShaderResourceViewDesc();
                srvDesc.SetTexture2DArray();
                srvDesc.Format = desc.Format;
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
            }

            {
                RHI.CShaderResourceView.UPicDesc txDesc = RHI.CShaderResourceView.LoadPicDesc(dftLayer.TexNormal);

                var desc = new ITexture2DDesc();
                desc.SetDefault();
                //desc.Format = EPixelFormat.PXF_B8G8R8A8_UNORM;
                desc.Width = (uint)txDesc.Width;
                desc.Height = (uint)txDesc.Height;
                desc.MipLevels = (uint)txDesc.MipLevel + 1;
                desc.ArraySize = (uint)MaterialIdArray.Count;

                NormalTextureArray = UEngine.Instance.GfxDevice.RenderContext.CreateTexture2D(in desc);
                var srvDesc = new IShaderResourceViewDesc();
                srvDesc.SetTexture2DArray();
                srvDesc.Format = desc.Format;
                srvDesc.Texture2DArray.ArraySize = desc.ArraySize;
                srvDesc.Texture2DArray.FirstArraySlice = 0;
                srvDesc.Texture2DArray.MipLevels = desc.MipLevels;
                srvDesc.Texture2DArray.MostDetailedMip = 0;
                srvDesc.mGpuBuffer = NormalTextureArray.mCoreObject;
                NormalTextureArraySRV = UEngine.Instance.GfxDevice.RenderContext.CreateShaderResourceView(in srvDesc);

                unsafe
                {
                    for (int i = 0; i < MaterialIdArray.Count; i++)
                    {
                        var mipDatas = RHI.CShaderResourceView.LoadPngImageLevels(MaterialIdArray[i].TexNormal, 0, ref txDesc);
                        if (txDesc.Width != desc.Width || txDesc.Height != desc.Height || mipDatas.Length != desc.m_MipLevels)
                        {
                            continue;
                        }
                        for (int j = 0; j < desc.m_MipLevels; j++)
                        {
                            fixed (byte* pSrcData = &mipDatas[j].Data[0])
                            {
                                NormalTextureArray.GetTextureCore().UpdateMipData(cmd, (uint)i, (uint)j,
                                    pSrcData, (uint)mipDatas[j].Width, (uint)mipDatas[j].Height, (uint)(mipDatas[j].Width * sizeof(uint)));
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}
