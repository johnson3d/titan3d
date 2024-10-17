using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Terrain.CDLOD
{
    public class UTerrainSystem : IDisposable
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
        public NxRHI.UCommandList UpdateRvtPass;

        public int MipLevels { get; set; } = 6;
        public Graphics.Mesh.TtMeshPrimitives[] GridMipLevels;

        public TtLayerManager LayerManager { get; } = new TtLayerManager();

        public async System.Threading.Tasks.Task<bool> Initialize(int mipLevel)
        {
            MipLevels = mipLevel;
            //Material = await TtEngine.Instance.GfxDevice.MaterialManager.CreateMaterial(RName.GetRName("material/SysDft.material", RName.ERNameType.Engine));
            Material = await TtEngine.Instance.GfxDevice.MaterialManager.CreateMaterial(RName.GetRName("utest/material/terrainidmap.material"));
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

            WireFrameMaterial = await TtEngine.Instance.GfxDevice.MaterialManager.CreateMaterial(RName.GetRName("material/sysdft_color.material", RName.ERNameType.Engine));
            WireFrameMaterial.IsEditingMaterial = false;

            WaterMaterial = await TtEngine.Instance.GfxDevice.MaterialManager.CreateMaterial(RName.GetRName("utest/material/terrainwater.material"));
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

            UpdateRvtPass = TtEngine.Instance.GfxDevice.RenderContext.CreateCommandList();
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
            NormalmapRVT.TickSync(UpdateRvtPass);
            HeightmapRVT.TickSync(UpdateRvtPass);
            MaterialIdRVT.TickSync(UpdateRvtPass);
        }
    }
}
