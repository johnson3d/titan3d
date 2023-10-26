using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Terrain.CDLOD
{
    public class UPatch : IDisposable
    {
        public int IndexX;
        public int IndexZ;
        public int XInLevel;
        public int ZInLevel;
        public DBoundingBox AABB;

        public UTerrainLevelData Level;
        public Graphics.Mesh.UMesh[] TerrainMesh;
        public Graphics.Mesh.UMesh[] WaterMesh;
        public Graphics.Mesh.UMesh[] WireFrameTerrainMesh;
        //public Graphics.Pipeline.Shader.UMaterialInstance Material;
        //public Graphics.Pipeline.Shader.UMaterialInstance WaterMaterial;
        public NxRHI.UCbView PatchCBuffer;

        public UTerrainGrassManager GrassManager;

        public Vector3 StartPosition = new Vector3(0);
        public Vector2 TexUVOffset;        
        int mCurrentLOD;
        public int CurrentLOD
        {
            get => mCurrentLOD;
            set
            {
                mCurrentLOD = value;
            }
        }
        ~UPatch()
        {
            Dispose();
        }
        public void Dispose()
        {
            CoreSDK.DisposeObject(ref PatchCBuffer);
            if (TerrainMesh != null)
            {
                foreach(var i in TerrainMesh)
                {
                    i.Dispose();
                }
                TerrainMesh = null;
            }
            if (WaterMesh != null)
            {
                foreach (var i in WaterMesh)
                {
                    i.Dispose();
                }
                WaterMesh = null;
            }
            if (WireFrameTerrainMesh != null)
            {
                foreach (var i in WireFrameTerrainMesh)
                {
                    i.Dispose();
                }
                WireFrameTerrainMesh = null;
            }
            CoreSDK.DisposeObject(ref GrassManager);
        }
        public void Initialize(UTerrainLevelData level, int x, int z, Bricks.Procedure.UBufferConponent HeightMap)
        {
            Level = level;

            if (x == 16 || z == 16)
            {
                return;
            }
            XInLevel = x;
            ZInLevel = z;
            var terrain = level.GetTerrainNode().Terrain;
            IndexX = x + level.Level.LevelX * level.GetTerrainNode().PatchSide;
            IndexZ = z + level.Level.LevelZ * level.GetTerrainNode().PatchSide;

            //Material = Graphics.Pipeline.Shader.UMaterialInstance.CreateMaterialInstance(terrain.Material);
            //WaterMaterial = Graphics.Pipeline.Shader.UMaterialInstance.CreateMaterialInstance(terrain.WaterMaterial);
            //var srv = Material.FindSRV("Diffuse");
            //if (srv != null)
            //{
            //    //srv.Value = RName.GetRName("");
            //}

            var mdfType = Rtti.UTypeDesc.TypeOf(typeof(UTerrainMdfQueue));
            var tMaterials = new Graphics.Pipeline.Shader.UMaterial[1];
            tMaterials[0] = terrain.Material;

            var tWireFrameMaterials = new Graphics.Pipeline.Shader.UMaterial[1];
            tWireFrameMaterials[0] = (terrain as UTerrainSystem).WireFrameMaterial;

            var twMaterials = new Graphics.Pipeline.Shader.UMaterial[1];
            twMaterials[0] = terrain.WaterMaterial;

            TerrainMesh = new Graphics.Mesh.UMesh[terrain.GridMipLevels.Length];
            WaterMesh = new Graphics.Mesh.UMesh[terrain.GridMipLevels.Length];
            WireFrameTerrainMesh = new Graphics.Mesh.UMesh[terrain.GridMipLevels.Length];
            
            for (int i = 0; i < terrain.GridMipLevels.Length; i++)
            {
                TerrainMesh[i] = new Graphics.Mesh.UMesh();
                TerrainMesh[i].Initialize(terrain.GridMipLevels[i], tMaterials, mdfType);
                var trMdfQueue = TerrainMesh[i].MdfQueue as UTerrainMdfQueue;                
                trMdfQueue.MdfDatas = this;
                trMdfQueue.Dimension = (int)Math.Pow(2, terrain.GridMipLevels.Length - i - 1);

                WireFrameTerrainMesh[i] = new Graphics.Mesh.UMesh();
                WireFrameTerrainMesh[i].Initialize(terrain.GridMipLevels[i], tWireFrameMaterials, mdfType);
                trMdfQueue = WireFrameTerrainMesh[i].MdfQueue as UTerrainMdfQueue;
                trMdfQueue.MdfDatas = this;
                trMdfQueue.Dimension = (int)Math.Pow(2, terrain.GridMipLevels.Length - i - 1);

                WaterMesh[i] = new Graphics.Mesh.UMesh();
                WaterMesh[i].Initialize(terrain.GridMipLevels[i], twMaterials, mdfType);
                trMdfQueue = WaterMesh[i].MdfQueue as UTerrainMdfQueue;
                trMdfQueue.MdfDatas = this;
                trMdfQueue.Dimension = (int)Math.Pow(2, terrain.GridMipLevels.Length - i - 1);
                trMdfQueue.IsWater = true;

                //trMdfQueue.StartPosition.X = IndexX * terrain.PatchSize;
                //trMdfQueue.StartPosition.Z = IndexZ * terrain.PatchSize;

                //trMdfQueue.StartPosition += node.StartPosition;
            }

            var PatchSize = level.GetTerrainNode().PatchSize;

            AABB.Minimum.X = IndexX * PatchSize;
            AABB.Minimum.Z = IndexZ * PatchSize;
            AABB.Minimum.Y = double.MaxValue;

            AABB.Maximum.X = (IndexX + 1) * PatchSize;
            AABB.Maximum.Z = (IndexZ + 1) * PatchSize;
            AABB.Maximum.Y = double.MinValue;

            int TexSizePerPatch  = level.GetTerrainNode().TexSizePerPatch;
            var heightData = HeightMap;
            for (int i = 0; i < TexSizePerPatch; i++)
            {
                for (int j = 0; j < TexSizePerPatch; j++)
                {
                    float alt = heightData.GetPixel<float>(x * TexSizePerPatch + j, z * TexSizePerPatch + i);
                    if (alt > AABB.Maximum.Y)
                    {
                        AABB.Maximum.Y = alt;
                    }
                    if (alt < AABB.Minimum.Y)
                    {
                        AABB.Minimum.Y = alt;
                    }
                }
            }

            AABB.Minimum += level.GetTerrainNode().Placement.AbsTransform.mPosition;
            AABB.Maximum += level.GetTerrainNode().Placement.AbsTransform.mPosition;

            var terrainNode = this.Level.Level.Node;
            OnAbsTransformChanged(terrainNode, terrainNode.GetWorld());
            UpdateCameraOffset(terrainNode.GetWorld());
            SetAcceptShadow(level.GetTerrainNode().IsAcceptShadow);

            GrassManager = new UTerrainGrassManager(this);
        }
        public void SetAcceptShadow(bool value)
        {
            for (int i = 0; i < TerrainMesh.Length; i++)
            {
                var mMesh = TerrainMesh[i];
                if (mMesh == null)
                    return;

                //var saved = mMesh.MdfQueue.MdfDatas;
                //Rtti.UTypeDesc mdfQueueType;
                //if (value)
                //{
                //    mdfQueueType = mMesh.MdfQueue.MdfPermutations.ReplacePermutation<Graphics.Pipeline.Shader.UMdf_NoShadow, Graphics.Pipeline.Shader.UMdf_Shadow>();
                //}
                //else
                //{
                //    mdfQueueType = mMesh.MdfQueue.MdfPermutations.ReplacePermutation<Graphics.Pipeline.Shader.UMdf_Shadow, Graphics.Pipeline.Shader.UMdf_NoShadow>();
                //}
                //mMesh.SetMdfQueueType(mdfQueueType);
                //mMesh.MdfQueue.MdfDatas = saved;

                //int ObjectFlags_2Bit = 0;
                //if (value)
                //    ObjectFlags_2Bit |= 1;
                //else
                //    ObjectFlags_2Bit &= (~1);
                //mMesh.PerMeshCBuffer.SetValue(NxRHI.UBuffer.mPerMeshIndexer.ObjectFLags_2Bit, in ObjectFlags_2Bit);
                mMesh.IsAcceptShadow = value;
            }
        }
        public void Tick(GamePlay.UWorld world, Graphics.Pipeline.URenderPolicy policy)
        {
            if (TerrainMesh == null)
                return;

            var node = Level.GetTerrainNode();
            var patchSize = node.PatchSize;
            
            DVector3 CameraOffset = node.Placement.AbsTransform.mPosition - world.CameraOffset;

            StartPosition.X = (float)(((double)(IndexX * patchSize)) + CameraOffset.X);
            StartPosition.Z = (float)(((double)(IndexZ * patchSize)) + CameraOffset.Z);
            StartPosition.Y = (float)(Level.HeightMapMinHeight + CameraOffset.Y);
        }        
        public void OnAbsTransformChanged(UTerrainNode node, GamePlay.UWorld world)
        {
            ref var transform = ref node.Placement.AbsTransform;
            foreach (var i in TerrainMesh)
            {
                i.SetWorldTransform(in transform, world, false);
            }
        }
        public void UpdateCameraOffset(GamePlay.UWorld world)
        {
            foreach (var i in TerrainMesh)
            {
                i.UpdateCameraOffset(world);
            }
        }
        public void OnGatherVisibleMeshes(GamePlay.UWorld.UVisParameter rp)
        {
            if (CurrentLOD >= TerrainMesh.Length)
                return;

            switch (Level.GetTerrainNode().Terrain.ShowMode)
            {
                case UTerrainSystem.EShowMode.Normal:
                    rp.VisibleMeshes.Add(TerrainMesh[CurrentLOD]);
                    break;
                case UTerrainSystem.EShowMode.WireFrame:
                    rp.VisibleMeshes.Add(WireFrameTerrainMesh[CurrentLOD]);
                    break;
                case UTerrainSystem.EShowMode.Both:
                    {
                        rp.VisibleMeshes.Add(TerrainMesh[CurrentLOD]);
                        rp.VisibleMeshes.Add(WireFrameTerrainMesh[CurrentLOD]);
                    }
                    break;
            }

            if (Level.GetTerrainNode().Terrain.IsShowWater)
                rp.VisibleMeshes.Add(WaterMesh[CurrentLOD]);

            GrassManager?.OnGatherVisibleMeshes(rp);
        }
    }
}
