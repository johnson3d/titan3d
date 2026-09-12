using System;
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

        [Rtti.Meta("")]
        [RName.PGRName(FilterExts = NxRHI.TtSrView.AssetExt)]
        public RName TexDiffuse { get; set; }
        [Rtti.Meta("")]
        [RName.PGRName(FilterExts = NxRHI.TtSrView.AssetExt)]
        public RName TexNormal { get; set; }
        [Rtti.Meta("")]
        public float TransitionRange { get; set; } = 5.0f;

        List<UTerrainPlant> mPlants = new List<UTerrainPlant>();
        [Rtti.Meta("",Order = 1)]
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
        [Rtti.Meta("",Order = 0)]
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
        //[Rtti.Meta("")]
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
        //[Rtti.Meta("")]
        //public int NullGrassDensity
        //{
        //    get => mNullGrassDensity;
        //    set
        //    {
        //        mNullGrassDensity = value;
        //        UpdateTotalGrassDensity();
        //    }
        //}
        [Rtti.Meta("")]
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
    /// <summary>
    /// 地形材质贴图的场景级覆盖项, 按下标与 UTerrainMaterialIdManager.MaterialIdArray 对齐。
    /// RName 为 null 表示该槽沿用 PGC 图表里的原值。
    /// 只覆盖"表现"(贴图), 不覆盖"生成参数"(TransitionRange / Plants) —— 后者是 PGC 生成 ID 图与撒植被的输入,
    /// 且计入 UMaterialIdMapNode.GetOutBufferHash, 改了会失效 level 缓存, 必须回 PGC 编辑器改。
    /// </summary>
    public partial class TtTerrainMaterialTextureOverride : IO.BaseSerializer
    {
        public override string ToString()
        {
            var result = "";
            if (TexDiffuse != null)
                result += TexDiffuse.Name;
            if (TexNormal != null)
                result += TexNormal.Name;
            return result;
        }
        [Rtti.Meta("")]
        [RName.PGRName(FilterExts = NxRHI.TtSrView.AssetExt)]
        public RName TexDiffuse { get; set; }
        [Rtti.Meta("")]
        [RName.PGRName(FilterExts = NxRHI.TtSrView.AssetExt)]
        public RName TexNormal { get; set; }
    }
    public class UTerrainMaterialIdManager : IO.BaseSerializer
    {
        [Rtti.Meta("")]
        public List<Terrain.CDLOD.UTerrainMaterialId> MaterialIdArray { get; set; } = new List<Terrain.CDLOD.UTerrainMaterialId>();
        /// <summary>
        /// 由宿主 TtTerrainNode 注入的场景级贴图覆盖 (见 TtTerrainNode.EnsureMaterialTextures)。
        /// manager 本体属于 PGC 图表节点, 这里只是让 BuildSRV 能取到"最终生效"的贴图;
        /// 不参与序列化, PGC 编辑器里预览时为 null, 取的就是图表原值。
        /// </summary>
        [System.ComponentModel.Browsable(false)]
        public IReadOnlyList<TtTerrainMaterialTextureOverride> TextureOverrides { get; set; }
        /// <summary>
        /// 取下标 idx 最终生效的 diffuse 贴图: 有场景覆盖用覆盖值, 否则用 PGC 图表里的原值。
        /// </summary>
        public RName GetEffectiveTexDiffuse(int idx)
        {
            var ov = (TextureOverrides != null && idx < TextureOverrides.Count) ? TextureOverrides[idx] : null;
            return ov?.TexDiffuse ?? MaterialIdArray[idx].TexDiffuse;
        }
        /// <summary>
        /// 取下标 idx 最终生效的 normal 贴图, 规则同 GetEffectiveTexDiffuse。
        /// </summary>
        public RName GetEffectiveTexNormal(int idx)
        {
            var ov = (TextureOverrides != null && idx < TextureOverrides.Count) ? TextureOverrides[idx] : null;
            return ov?.TexNormal ?? MaterialIdArray[idx].TexNormal;
        }
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
        public unsafe bool BuildSRV(NxRHI.ICommandList cmd)
        {
            Cleanup();

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            if (MaterialIdArray.Count == 0)
                return false;
            // 整个贴图数组的尺寸 / mip 数以下标 0 为基准, 后面尺寸不符的 slice 会被跳过
            {
                var txDesc = NxRHI.TtTextureHelper.LoadPictureDesc(GetEffectiveTexDiffuse(0));

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

                for (int i = 0; i < MaterialIdArray.Count; i++)
                {
                    var mipDatas = NxRHI.TtSrView.LoadPixelMipLevels(GetEffectiveTexDiffuse(i), 0, txDesc);
                    if (txDesc.Width != desc.Width || txDesc.Height != desc.Height || mipDatas.Length != desc.m_MipLevels)
                    {
                        continue;
                    }
                    //System.Diagnostics.Debug.Assert(txDesc.Format == desc.Format);
                    for (int j = 0; j < desc.m_MipLevels; j++)
                    {
                        var subRes = DiffuseTextureArray.mCoreObject.GetSubResource((uint)j, (uint)i, 0);
                        var fp = new NxRHI.FSubResourceFootPrint();
                        fp.SetDefault();
                        fp.Format = desc.Format;
                        fp.Width = (uint)txDesc.MipSizes[j].X;
                        fp.Height = (uint)txDesc.MipSizes[j].Y;
                        fp.Depth = 1;
                        //fp.RowPitch = (uint)txDesc.MipSizes[j].Z;
                        if (txDesc.BlockSize == 0)
                        {
                            fp.RowPitch = (uint)txDesc.MipSizes[(int)j].Z;
                        }
                        else
                        {
                            var blockWidth = txDesc.BlockDimenstions[(int)j].X;
                            fp.RowPitch = (uint)(blockWidth * txDesc.BlockSize);
                        }
                        fp.TotalSize = mipDatas[j].Size;
                        DiffuseTextureArray.UpdateGpuData(cmd, subRes, mipDatas[j].DataPointer, &fp);
                    }
                }
            }

            var dftNormal = GetEffectiveTexNormal(0);
            if (dftNormal != null)
            {
                var txDesc = NxRHI.TtTextureHelper.LoadPictureDesc(dftNormal);

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

                for (int i = 0; i < MaterialIdArray.Count; i++)
                {
                    //var mipDatas = NxRHI.USrView.LoadImageLevels(MaterialIdArray[i].TexNormal, 0, ref txDesc);
                    var mipDatas = NxRHI.TtSrView.LoadPixelMipLevels(GetEffectiveTexNormal(i), 0, txDesc);
                    if (txDesc.Width != desc.Width || txDesc.Height != desc.Height || mipDatas.Length != desc.m_MipLevels)
                    {
                        continue;
                    }
                    //System.Diagnostics.Debug.Assert(txDesc.Format == desc.Format);
                    for (int j = 0; j < desc.m_MipLevels; j++)
                    {
                        var subRes = NormalTextureArray.mCoreObject.GetSubResource((uint)j, (uint)i, 0);

                        var fp = new NxRHI.FSubResourceFootPrint();
                        fp.SetDefault();
                        fp.Format = desc.Format;
                        fp.Width = (uint)txDesc.MipSizes[j].X;
                        fp.Height = (uint)txDesc.MipSizes[j].Y;
                        fp.Depth = 1;
                        if (txDesc.BlockSize == 0)
                        {
                            fp.RowPitch = (uint)txDesc.MipSizes[(int)j].Z;
                        }
                        else
                        {
                            var blockWidth = txDesc.BlockDimenstions[(int)j].X;
                            fp.RowPitch = (uint)(blockWidth * txDesc.BlockSize);
                        }
                        fp.TotalSize = mipDatas[j].Size;
                        NormalTextureArray.UpdateGpuData(cmd, subRes, mipDatas[j].DataPointer, &fp);
                    }
                }
            }

            return true;
        }
    }
}


#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace EngineNS.Bricks.Terrain.CDLOD
{
	partial class UTerrainMaterialId
	{
		public unsafe int macross_GetRandomPlant (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, int rdValue) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = GetRandomPlant(rdValue);
			return _return_value;
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross