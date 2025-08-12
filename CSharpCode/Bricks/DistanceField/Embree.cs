using EngineNS.Graphics.Mesh;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.DistanceField
{
    public interface IEmbreeGeometry
    {
        public uint GeomID { get; }
        public void SetTransform(in Matrix matrix);
    }
    public class TtEmbreeGeometry : AuxPtrType<EngineNS.FEmbreeGeometry>, IEmbreeGeometry
    {
        public TtEmbreeGeometry(FEmbreeGeometry self)
        {
            mCoreObject = self;
        }
        public uint GeomID
        {
            get => mCoreObject.GeomID;
        }
        public void SetTransform(in Matrix matrix)
        {
            mCoreObject.SetGeometryTransform(in matrix);
        }
    }
    public class TtEmbreeGeometryInstance : AuxPtrType<EngineNS.FEmbreeGeometryInstance>, IEmbreeGeometry
    {
        public TtEmbreeGeometryInstance(FEmbreeGeometryInstance self)
        {
            mCoreObject = self;
        }
        public uint GeomID
        {
            get => mCoreObject.NativeSuper.GeomID;
        }
        public void SetTransform(in Matrix matrix)
        {
            mCoreObject.NativeSuper.SetGeometryTransform(in matrix);
        }
    }
    public class TtEmbreeScene : AuxPtrType<EngineNS.FEmbreeScene>
    {
        public TtEmbreeScene()
        {
            mCoreObject = EngineNS.FEmbreeScene.CreateInstance();
        }
        public TtEmbreeScene(FEmbreeScene self)
        {
            mCoreObject = self;
        }

        public void AttachGeometry(TtEmbreeGeometry Geometry)
        {
            mCoreObject.AttachGeometry(Geometry.mCoreObject);
        }
        public void AttachGeometryInstance(TtEmbreeGeometryInstance Geometry)
        {
            mCoreObject.AttachGeometryInstance(Geometry.mCoreObject);
        }
        public void DetachGeometry(uint geomID)
        {
            mCoreObject.DetachGeometry(geomID);
        }
        public void CommitScene()
        {
            mCoreObject.CommitScene();
        }
        public FEmbreeGeometry FindGeometry(uint geomID)
        {
            return mCoreObject.FindGeometry(geomID);
        }
        public void EmbreePointQuery(Vector3 VoxelPosition, float LocalSpaceTraceDistance, ref bool bOutNeedTracyRays, ref float OutClosestDistance)
        {
            mCoreObject.EmbreePointQuery(VoxelPosition, LocalSpaceTraceDistance, ref bOutNeedTracyRays, ref OutClosestDistance);
        }
        public void EmbreeRayTrace(Vector3 StartPosition, Vector3 RayDirection, ref bool bOutHit, ref bool bOutHitTwoSided, ref Vector3 OutHitNormal, ref float OutTFar)
        {
            mCoreObject.EmbreeRayTrace(StartPosition, RayDirection, ref bOutHit, ref bOutHitTwoSided, ref OutHitNormal, ref OutTFar);
        }

        #region Old API
        public int NumIndices { get => mCoreObject.NumIndices; set => mCoreObject.NumIndices = value; }
        public bool bMostlyTwoSided { get => mCoreObject.bMostlyTwoSided; set => mCoreObject.bMostlyTwoSided = value; }
        #endregion
    }

    public class TtEmbreeManager : AuxPtrType<EngineNS.EmbreeManager>
    {
        public TtEmbreeManager()
        {
            mCoreObject = EngineNS.EmbreeManager.CreateInstance();
        }
        public bool Initialize()
        {
            return mCoreObject.Initialize();
        }
        public TtEmbreeScene CreateScene()
        {
            return new TtEmbreeScene(mCoreObject.CreateScene());
        }
        public TtEmbreeGeometry CreateGeometry(string name, TtMeshDataProvider meshProvider)
        {
            return new TtEmbreeGeometry(mCoreObject.CreateGeometry(VNameString.FromString(name), meshProvider.mCoreObject));
        }

        #region Old API
        public void SetupEmbreeScene(string name, TtMeshDataProvider meshProvider, float DistanceFieldResolutionScale, TtEmbreeScene embreeScene)
        {
            mCoreObject.SetupEmbreeScene(VNameString.FromString(name), meshProvider.mCoreObject, DistanceFieldResolutionScale, embreeScene.mCoreObject);
        }
        public void DeleteEmbreeScene(TtEmbreeScene embreeScene)
        {
            mCoreObject.DeleteEmbreeScene(embreeScene.mCoreObject);
        }
        public void EmbreePointQuery(TtEmbreeScene embreeScene, Vector3 VoxelPosition, float LocalSpaceTraceDistance, ref bool bOutNeedTracyRays, ref float OutClosestDistance)
        {
            mCoreObject.EmbreePointQuery(embreeScene.mCoreObject, VoxelPosition, LocalSpaceTraceDistance, ref bOutNeedTracyRays, ref OutClosestDistance);
        }
        public void EmbreeRayTrace(TtEmbreeScene embreeScene, Vector3 StartPosition, Vector3 RayDirection, ref bool bOutHit, ref bool bOutHitTwoSided, ref Vector3 OutHitNormal, ref float OutTFar)
        {
            mCoreObject.EmbreeRayTrace(embreeScene.mCoreObject, StartPosition, RayDirection, ref bOutHit, ref bOutHitTwoSided, ref OutHitNormal, ref OutTFar);
        }
        #endregion
    }
}
