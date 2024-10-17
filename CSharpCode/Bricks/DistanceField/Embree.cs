using EngineNS.Graphics.Mesh;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.DistanceField
{
    public class UEmbreeScene : AuxPtrType<EngineNS.FEmbreeScene>
    {
        public UEmbreeScene()
        {
            mCoreObject = EngineNS.FEmbreeScene.CreateInstance();
        }

        public int NumIndices { get => mCoreObject.NumIndices; set => mCoreObject.NumIndices = value; }
        public bool bMostlyTwoSided { get => mCoreObject.bMostlyTwoSided; set => mCoreObject.bMostlyTwoSided = value; }
    }

    public class UEmbreeManager : AuxPtrType<EngineNS.EmbreeManager>
    {
        public UEmbreeManager()
        {
            mCoreObject = EngineNS.EmbreeManager.CreateInstance();
        }
        public void SetupEmbreeScene(string name, TtMeshDataProvider meshProvider, float DistanceFieldResolutionScale, UEmbreeScene embreeScene)
        {
            mCoreObject.SetupEmbreeScene(VNameString.FromString(name), meshProvider.mCoreObject, DistanceFieldResolutionScale, embreeScene.mCoreObject);
        }
        public void DeleteEmbreeScene(UEmbreeScene embreeScene)
        {
            mCoreObject.DeleteEmbreeScene(embreeScene.mCoreObject);
        }
        public void EmbreePointQuery(UEmbreeScene embreeScene, Vector3 VoxelPosition, float LocalSpaceTraceDistance, ref bool bOutNeedTracyRays, ref float OutClosestDistance)
        {
            mCoreObject.EmbreePointQuery(embreeScene.mCoreObject, VoxelPosition, LocalSpaceTraceDistance, ref bOutNeedTracyRays, ref OutClosestDistance);
        }
        public void EmbreeRayTrace(UEmbreeScene embreeScene, Vector3 StartPosition, Vector3 RayDirection, ref bool bOutHit, ref bool bOutHitTwoSided, ref Vector3 OutHitNormal, ref float OutTFar)
        {
            mCoreObject.EmbreeRayTrace(embreeScene.mCoreObject, StartPosition, RayDirection, ref bOutHit, ref bOutHitTwoSided, ref OutHitNormal, ref OutTFar);
        }

    }
}
