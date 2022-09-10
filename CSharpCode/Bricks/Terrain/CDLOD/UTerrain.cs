using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Terrain.CDLOD
{
    public class UTerrainSystem
    {
        public enum EShowMode
        {
            Normal,
            WireFrame,
            Both,
        }
        public EShowMode ShowMode { get; set; } = EShowMode.Normal;
        public bool IsShowWater { get; set; } = false;
        public Graphics.Pipeline.Shader.UMaterial Material;
        public Graphics.Pipeline.Shader.UMaterial WaterMaterial;
        public Graphics.Pipeline.Shader.UMaterial WireFrameMaterial;
        
        public int MipLevels { get; set; } = 6;
        public Graphics.Mesh.UMeshPrimitives[] GridMipLevels;

        public async System.Threading.Tasks.Task<bool> Initialize(int mipLevel)
        {
            MipLevels = mipLevel;
            //Material = await UEngine.Instance.GfxDevice.MaterialManager.CreateMaterial(RName.GetRName("material/SysDft.material", RName.ERNameType.Engine));
            Material = await UEngine.Instance.GfxDevice.MaterialManager.CreateMaterial(RName.GetRName("utest/material/terrainidmap.material"));
            Material.IsEditingMaterial = false;

            WireFrameMaterial = await UEngine.Instance.GfxDevice.MaterialManager.CreateMaterial(RName.GetRName("material/sysdft_color.material", RName.ERNameType.Engine));
            WireFrameMaterial.IsEditingMaterial = false;

            WaterMaterial = await UEngine.Instance.GfxDevice.MaterialManager.CreateMaterial(RName.GetRName("utest/material/terrainwater.material"));
            WaterMaterial.IsEditingMaterial = false;

            var rast = WireFrameMaterial.Rasterizer;
            rast.FillMode = NxRHI.EFillMode.FMD_WIREFRAME;
            WireFrameMaterial.Rasterizer = rast;

            GridMipLevels = new Graphics.Mesh.UMeshPrimitives[mipLevel];
            for (int i = 0; i < GridMipLevels.Length; i++)
            {
                var size = (ushort)Math.Pow(2, GridMipLevels.Length - i - 1);

                GridMipLevels[i] = Graphics.Mesh.UMeshDataProvider.MakeGridForTerrain(size, size).ToMesh();
            }
            //Parameters.SureMiplevels(mipLevel);
            //Parameters.LODLevelCount = mipLevel;
            //Parameters.VisibilityDistance = 128.0f;
            //Parameters.LODDistanceRatio = 0.6f;
            //Parameters.MorphStartRatio = 0.3f;

            
            return true;
        }
        public void Cleanup()
        {
            for (int i = 0; i < GridMipLevels.Length; i++)
            {
                GridMipLevels[i].Dispose();
            }
            GridMipLevels = null;
        }        
        public void Tick()
        {
            
        }
    }
}
