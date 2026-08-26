using EngineNS.NxRHI;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Terrain.CDLOD
{
    public class TtTerrainSystem : IDisposable
    {
        public enum EShowMode
        {
            Normal,
            WireFrame,
            Both,
        }
        public EShowMode ShowMode { get; set; } = EShowMode.Normal;
        public bool IsShowWater { get; set; } = false;
        public Graphics.Pipeline.Shader.TtMaterial Material;
        public Graphics.Pipeline.Shader.TtMaterial WaterMaterial;
        public Graphics.Pipeline.Shader.TtMaterial WireFrameMaterial;
        public VirtualTexture.TtVirtualTextureArray HeightmapRVT;
        public VirtualTexture.TtVirtualTextureArray NormalmapRVT;
        public VirtualTexture.TtVirtualTextureArray MaterialIdRVT;
        
        public int MipLevels { get; set; } = 6;
        public Graphics.Mesh.TtMeshPrimitives[] GridMipLevels;

        public TtLayerManager LayerManager { get; } = new TtLayerManager();

        public async Thread.Async.TtTask<bool> Initialize(RName terrainMaterial, int mipLevel)
        {
            MipLevels = mipLevel;
            //Material = await TtEngine.Instance.GfxDevice.MaterialManager.CreateMaterial(RName.GetRName("material/SysDft.material", RName.ERNameType.Engine));
            Material = await terrainMaterial.CreateAsset<Graphics.Pipeline.Shader.TtMaterial>();
            Material.IsEditingMaterial = false;

            if (TtEngine.Instance.Config.Feature_UseRVT)
            {
                HeightmapRVT = new VirtualTexture.TtVirtualTextureArray();
                HeightmapRVT.Initialize(EPixelFormat.PXF_R16_FLOAT, 1024, 1, 32);
                NormalmapRVT = new VirtualTexture.TtVirtualTextureArray();
                NormalmapRVT.Initialize(EPixelFormat.PXF_R8G8B8A8_UNORM, 1024, 1, 32);
                MaterialIdRVT = new VirtualTexture.TtVirtualTextureArray();
                MaterialIdRVT.Initialize(EPixelFormat.PXF_R8G8B8A8_UNORM, 1024, 1, 32);
            }

            WireFrameMaterial = await RName.GetRName("material/sysdft_color.material", RName.ERNameType.Engine).CreateAsset<Graphics.Pipeline.Shader.TtMaterial>();
            WireFrameMaterial.IsEditingMaterial = false;

            WaterMaterial = await RName.GetRName("utest/material/terrainwater.material").CreateAsset<Graphics.Pipeline.Shader.TtMaterial>();
            WaterMaterial.IsEditingMaterial = false;

            var rast = WireFrameMaterial.Rasterizer;
            rast.FillMode = NxRHI.EFillMode.FMD_WIREFRAME;
            WireFrameMaterial.Rasterizer = rast;

            GridMipLevels = new Graphics.Mesh.TtMeshPrimitives[mipLevel];
            for (int i = 0; i < GridMipLevels.Length; i++)
            {
                var size = (ushort)Math.Pow(2, GridMipLevels.Length - i - 1);

                GridMipLevels[i] = Graphics.Mesh.TtMeshDataProvider.MakeGridForTerrain(size, size).ToMesh();
            }
            //Parameters.SureMiplevels(mipLevel);
            //Parameters.LODLevelCount = mipLevel;
            //Parameters.VisibilityDistance = 128.0f;
            //Parameters.LODDistanceRatio = 0.6f;
            //Parameters.MorphStartRatio = 0.3f;

            return true;
        }
        public void Dispose()
        {
            for (int i = 0; i < GridMipLevels.Length; i++)
            {
                GridMipLevels[i].Dispose();
            }
            GridMipLevels = null;
        }        
        public void TickSync()
        {
            var cmdlist = TtCommandList.GetCmdList();
            using (new NxRHI.TtCmdListScope(cmdlist, "RVT"))
            {
                NormalmapRVT.UpdateGpu(cmdlist);
                HeightmapRVT.UpdateGpu(cmdlist);
                MaterialIdRVT.UpdateGpu(cmdlist);
                cmdlist.FlushDraws();
            }
            
            TtEngine.Instance.GfxDevice.RenderQueue.QueueCmdlist(cmdlist, "RVT.UpdateData", NxRHI.EQueueType.QU_Transfer);
        }
    }
}
