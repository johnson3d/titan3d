using EngineNS.Graphics.Mesh;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Collision.Embree
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
        public void DetachGeometryInstance(uint geomID)
        {
            mCoreObject.DetachGeometryInstance(geomID);
        }
        public void RemoveAllGeometries()
        {
            mCoreObject.RemoveAllGeometries();
        }
        public void RemoveAllGeometryInstances()
        {
            mCoreObject.RemoveAllGeometryInstances();
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
        public bool EmbreeRayTrace(Vector3 StartPosition, Vector3 RayDirection, float minDist, float maxDist, ref EngineNS.FHitResult OutHit)
        {
            return mCoreObject.EmbreeRayTrace(StartPosition, RayDirection, minDist, maxDist, ref OutHit);
        }
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
        public TtEmbreeGeometryInstance CreateGeometryInstance(TtEmbreeGeometry geometry)
        {
            FEmbreeGeometryInstance ptr = mCoreObject.CreateGeometryInstance(geometry.mCoreObject);
            return new TtEmbreeGeometryInstance(ptr);
        }

    }
}
