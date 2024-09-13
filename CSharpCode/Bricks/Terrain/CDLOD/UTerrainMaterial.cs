﻿using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using static EngineNS.NxRHI.TtSrView;

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
            //foreach(var i in Grasses)
            //{
            //    result += i.ToString();
            //}
            return result;
        }

        [Rtti.Meta]
        [RName.PGRName(FilterExts = NxRHI.TtSrView.AssetExt)]
        public RName TexDiffuse { get; set; }
        [Rtti.Meta]
        [RName.PGRName(FilterExts = NxRHI.TtSrView.AssetExt)]
        public RName TexNormal { get; set; }
        [Rtti.Meta]
        public float TransitionRange { get; set; } = 5.0f;

        List<UTerrainPlant> mPlants = new List<UTerrainPlant>();
        [Rtti.Meta(Order = 1)]
        public List<UTerrainPlant> Plants 
        {
            get => mPlants;
            set
            {
                mPlants = value;
                UpdateTotalPlantDensity();
            }
        }
        int mTotalPlantDensity;
        public int TotalPlantDensity 
        {
            get => mTotalPlantDensity;
            set
            {
                UpdateTotalPlantDensity();
            }
        }
        public void UpdateTotalPlantDensity()
        {
            lock (Plants)
            {
                mTotalPlantDensity = 0;
                if (Plants.Count > 0)
                {
                    mTotalPlantDensity = NullPlantDensity;
                    foreach (var i in Plants)
                    {
                        mTotalPlantDensity += i.Density;
                    }

                    Plants.Sort((x, y) =>
                    {
                        return x.Density.CompareTo(y.Density);
                    });
                }
            }
        }
        int mNullPlantDensity = 100000;
        [Rtti.Meta(Order = 0)]
        public int NullPlantDensity 
        {
            get => mNullPlantDensity;
            set
            {
                mNullPlantDensity = value;
                UpdateTotalPlantDensity();
            }
        }
        //List<UTerrainGrass> mGrasses = new List<UTerrainGrass>();
        //[Rtti.Meta]
        //public List<UTerrainGrass> Grasses
        //{
        //    get => mGrasses;
        //    set
        //    {
        //        mGrasses = value;
        //        UpdateTotalGrassDensity();
        //    }
        //}
        //int mTotalGrassDensity;
        //public int TotalGrassDensity
        //{
        //    get => mTotalGrassDensity;
        //    set
        //    {
        //        mTotalGrassDensity = value;
        //        UpdateTotalGrassDensity();
        //    }
        //}
        //public void UpdateTotalGrassDensity()
        //{
        //    lock(Grasses)
        //    {
        //        mTotalGrassDensity = 0;
        //        if(Grasses.Count > 0)
        //        {
        //            mTotalGrassDensity = NullGrassDensity;
        //            foreach(var i in Grasses)
        //            {
        //                mTotalGrassDensity += i.Density;
        //            }

        //            Grasses.Sort((x, y) =>
        //            {
        //                return x.Density.CompareTo(y.Density);
        //            });
        //        }
        //    }
        //}
        //int mNullGrassDensity = 10;
        //[Rtti.Meta]
        //public int NullGrassDensity
        //{
        //    get => mNullGrassDensity;
        //    set
        //    {
        //        mNullGrassDensity = value;
        //        UpdateTotalGrassDensity();
        //    }
        //}
        [Rtti.Meta]
        public int GetRandomPlant(int rdValue)
        {
            rdValue = rdValue % TotalPlantDensity;
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

        //public int GetRandomGrass(int rdValue)
        //{
        //    rdValue = rdValue % TotalGrassDensity;
        //    if (rdValue < NullGrassDensity)
        //        return -1;
        //    int curTotal = NullGrassDensity;
        //    for(int i=0; i < Grasses.Count; i++)
        //    {
        //        curTotal += Grasses[i].Density;
        //        if (rdValue < curTotal)
        //            return i;
        //    }
        //    return -1;
        //}
    }
    public class UTerrainMaterialIdManager : IO.BaseSerializer
    {
        [Rtti.Meta]
        public List<Terrain.CDLOD.UTerrainMaterialId> MaterialIdArray { get; set; } = new List<Terrain.CDLOD.UTerrainMaterialId>();
        public NxRHI.TtTexture DiffuseTextureArray;
        public NxRHI.TtTexture NormalTextureArray;
        public NxRHI.TtSrView DiffuseTextureArraySRV;
        public NxRHI.TtSrView NormalTextureArraySRV;
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
        public bool BuildSRV(NxRHI.ICommandList cmd)
        {
            Cleanup();

            if (MaterialIdArray.Count == 0)
                return false;
            var dftLayer = MaterialIdArray[0];
            {
                var txDesc = NxRHI.TtSrView.LoadPictureDesc(dftLayer.TexDiffuse);

                var desc = new NxRHI.FTextureDesc();
                desc.SetDefault();
                if (TtEngine.Instance.CurrentPlatform == EPlatformType.PLTF_Windows)
                {
                    desc.Format = EPixelFormat.PXF_BC1_UNORM;
                }
                //desc.Format = EPixelFormat.PXF_B8G8R8A8_UNORM;
                desc.Width = (uint)txDesc.Width;
                desc.Height = (uint)txDesc.Height;
                //desc.MipLevels = (uint)txDesc.MipLevel + 1;
                desc.MipLevels = (uint)txDesc.MipLevel;
                desc.ArraySize = (uint)MaterialIdArray.Count;

                DiffuseTextureArray = TtEngine.Instance.GfxDevice.RenderContext.CreateTexture(in desc);
                var srvDesc = new NxRHI.FSrvDesc();
                srvDesc.SetTexture2DArray();
                srvDesc.Format = desc.Format;
                srvDesc.Texture2DArray.ArraySize = desc.ArraySize;
                srvDesc.Texture2DArray.FirstArraySlice = 0;
                srvDesc.Texture2DArray.MipLevels = desc.MipLevels;
                srvDesc.Texture2DArray.MostDetailedMip = 0;
                DiffuseTextureArraySRV = TtEngine.Instance.GfxDevice.RenderContext.CreateSRV(DiffuseTextureArray, in srvDesc);

                unsafe
                {
                    for (int i = 0; i < MaterialIdArray.Count; i++)
                    {
                        var mipDatas = NxRHI.TtSrView.LoadPixelMipLevels(MaterialIdArray[i].TexDiffuse, 0, txDesc);
                        if (txDesc.Width != desc.Width || txDesc.Height != desc.Height || mipDatas.Length != desc.m_MipLevels)
                        {
                            continue;
                        }
                        //System.Diagnostics.Debug.Assert(txDesc.Format == desc.Format);
                        for (int j = 0; j < desc.m_MipLevels; j++)
                        {
                            var subRes = DiffuseTextureArray.mCoreObject.GetSubResource((uint)j, (uint)i, 0);
                            //DiffuseTextureArray.UpdateGpuData(0, mipDatas[j].mCoreObject.GetData(),
                            //    (uint)(txDesc.MipSizes[j].Z * txDesc.MipSizes[j].Y), subRes);
                            var fp = new NxRHI.FSubResourceFootPrint();
                            fp.SetDefault();
                            fp.Format = desc.Format;
                            fp.Width = (uint)txDesc.MipSizes[j].X;
                            fp.Height = (uint)txDesc.MipSizes[j].Y;
                            fp.Depth = 1;
                            fp.RowPitch = (uint)txDesc.MipSizes[j].Z;
                            fp.TotalSize = fp.RowPitch * fp.Height;
                            DiffuseTextureArray.UpdateGpuData(cmd, subRes, mipDatas[j].mCoreObject.GetData(), &fp);
                        }
                    }
                }
            }

            if (dftLayer.TexNormal != null)
            {
                var txDesc = NxRHI.TtSrView.LoadPictureDesc(dftLayer.TexNormal);

                var desc = new NxRHI.FTextureDesc();
                desc.SetDefault();
                if (TtEngine.Instance.CurrentPlatform == EPlatformType.PLTF_Windows)
                {
                    desc.Format = EPixelFormat.PXF_BC1_UNORM;
                }
                //desc.Format = EPixelFormat.PXF_B8G8R8A8_UNORM;
                desc.Width = (uint)txDesc.Width;
                desc.Height = (uint)txDesc.Height;
                desc.MipLevels = (uint)txDesc.MipLevel;
                desc.ArraySize = (uint)MaterialIdArray.Count;

                NormalTextureArray = TtEngine.Instance.GfxDevice.RenderContext.CreateTexture(in desc);
                var srvDesc = new NxRHI.FSrvDesc();
                srvDesc.SetTexture2DArray();
                srvDesc.Format = desc.Format;
                srvDesc.Texture2DArray.ArraySize = desc.ArraySize;
                srvDesc.Texture2DArray.FirstArraySlice = 0;
                srvDesc.Texture2DArray.MipLevels = desc.MipLevels;
                srvDesc.Texture2DArray.MostDetailedMip = 0;
                NormalTextureArraySRV = TtEngine.Instance.GfxDevice.RenderContext.CreateSRV(NormalTextureArray, in srvDesc);

                unsafe
                {
                    for (int i = 0; i < MaterialIdArray.Count; i++)
                    {
                        //var mipDatas = NxRHI.USrView.LoadImageLevels(MaterialIdArray[i].TexNormal, 0, ref txDesc);
                        var mipDatas = NxRHI.TtSrView.LoadPixelMipLevels(MaterialIdArray[i].TexNormal, 0, txDesc);
                        if (txDesc.Width != desc.Width || txDesc.Height != desc.Height || mipDatas.Length != desc.m_MipLevels)
                        {
                            continue;
                        }
                        //System.Diagnostics.Debug.Assert(txDesc.Format == desc.Format);
                        for (int j = 0; j < desc.m_MipLevels; j++)
                        {
                            var subRes = NormalTextureArray.mCoreObject.GetSubResource((uint)j, (uint)i, 0);
                            //NormalTextureArray.UpdateGpuData(0, mipDatas[j].mCoreObject.GetData(), 
                            //    (uint)(txDesc.MipSizes[j].Z * txDesc.MipSizes[j].Y), subRes);

                            var fp = new NxRHI.FSubResourceFootPrint();
                            fp.SetDefault();
                            fp.Format = desc.Format;
                            fp.Width = (uint)txDesc.MipSizes[j].X;
                            fp.Height = (uint)txDesc.MipSizes[j].Y;
                            fp.Depth = 1;
                            fp.RowPitch = (uint)txDesc.MipSizes[j].Z;
                            fp.TotalSize = fp.RowPitch * fp.Height;
                            NormalTextureArray.UpdateGpuData(cmd, subRes, mipDatas[j].mCoreObject.GetData(), &fp);
                        }
                    }
                }
            }

            return true;
        }
    }
}
