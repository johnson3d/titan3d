using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.PhysicsCore
{
    public struct FPxTransform
    {
        public Vector3 P;
        public Quaternion Q;
    }
    public class TtPhyContext : AuxPtrType<PhyContext>
    {
        public TtPhyContext()
        {
            mCoreObject = PhyContext.CreateInstance();
        }
        public TtPhySceneDesc CreateSceneDesc()
        {
            var self = mCoreObject.CreateSceneDesc();
            if (self.IsValidPointer == false)
                return null;
            return new TtPhySceneDesc(self);
        }
        public TtPhyScene CreateScene(TtPhySceneDesc desc)
        {
            var self = mCoreObject.CreateScene(desc.mCoreObject);
            if (self.IsValidPointer == false)
                return null;
            return new TtPhyScene(self);
        }
        public TtPhyActor CreateActor(EPhyActorType type, in DVector3 p, in Quaternion q)
        {
            var self = mCoreObject.CreateActor(type, p.ToSingleVector3(), in q);
            if (self.IsValidPointer == false)
                return null;
            return new TtPhyActor(self);
        }
        public TtPhyMaterial CreateMaterial(float staticFriction, float dynamicFriction, float restitution)
        {
            var self = mCoreObject.CreateMaterial(staticFriction, dynamicFriction, restitution);
            if (self.IsValidPointer == false)
                return null;
            return new TtPhyMaterial(self);
        }
        public TtPhyShape CreateShapePlane(TtPhyMaterial material)
        {
            var self = mCoreObject.CreateShapePlane(material.mCoreObject);
            if (self.IsValidPointer == false)
                return null;
            var result = new TtPhyPlaneShape(self);
            result.Materials = new List<TtPhyMaterial> { material };
            return result;
        }
        public TtPhyShape CreateShapeBox(TtPhyMaterial material, in Vector3 halfExtent)
        {
            var self = mCoreObject.CreateShapeBox(material.mCoreObject, in halfExtent);
            if (self.IsValidPointer == false)
                return null;
            var result = new TtPhyBoxShape(self);
            result.Materials = new List<TtPhyMaterial> { material };
            return result;
        }
        public TtPhyShape CreateShapeSphere(TtPhyMaterial material, float radius)
        {
            var self = mCoreObject.CreateShapeSphere(material.mCoreObject, radius);
            if (self.IsValidPointer == false)
                return null;
            var result = new TtPhySphereShape(self);
            result.Materials = new List<TtPhyMaterial> { material };
            return result;
        }
        public TtPhyShape CreateShapeCapsule(TtPhyMaterial material, float radius, float halfHeight)
        {
            var self = mCoreObject.CreateShapeCapsule(material.mCoreObject, radius, halfHeight);
            if (self.IsValidPointer == false)
                return null;
            var result = new TtPhyCapsuleShape(self);
            result.Materials = new List<TtPhyMaterial> { material };
            return result;
        }
        public TtPhyShape CreateShapeConvex(TtPhyMaterial material, TtPhyConvexMesh mesh, in Vector3 scale, in Quaternion scaleRot)
        {
            var self = mCoreObject.CreateShapeConvex(material.mCoreObject, mesh.mCoreObject, in scale, in scaleRot);
            if (self.IsValidPointer == false)
                return null;
            var result = new TtPhyConvoxShape(self);
            result.Materials = new List<TtPhyMaterial> { material };
            return result;
        }
        public TtPhyShape CreateShapeTriMesh(List<TtPhyMaterial> material, TtPhyTriMesh mesh, in Vector3 scale, in Quaternion scaleRot)
        {
            unsafe
            {
                PhyMaterial* pMtls = stackalloc PhyMaterial[material.Count];
                for (int i = 0; i < material.Count; i++)
                {
                    pMtls[i] = material[i].mCoreObject;
                }
                var self = mCoreObject.CreateShapeTriMesh((PhyMaterial**)pMtls, material.Count, mesh.mCoreObject, in scale, in scaleRot);
                if (self.IsValidPointer == false)
                    return null;
                var result = new TtPhyTriMeshShape(self);
                result.Materials = material;
                return result;
            }
        }
        public TtPhyShape CreateShapeHeightfield(List<TtPhyMaterial> material, TtPhyHeightfield heightfield, float heightScale, in Vector3 scale)
        {
            unsafe
            {
                PhyMaterial* pMtls = stackalloc PhyMaterial[material.Count];
                for (int i = 0; i < material.Count; i++)
                {
                    pMtls[i] = material[i].mCoreObject;
                }
                var self = mCoreObject.CreateShapeHeightfield((PhyMaterial**)pMtls, material.Count, heightfield.mCoreObject, heightScale, in scale);
                if (self.IsValidPointer == false)
                    return null;
                var result = new TtPhyHeightfieldShape(self);
                result.Materials = material;
                return result;
            }
        }

        public unsafe TtPhyHeightfield CookHeightfield(int nbColumns, int nbRows, PhyHeightFieldSample* pData, float convexEdgeThreshold = 0.0f, bool bNoBoundaryEdge = true)
        {
            unsafe
            {
                var self = mCoreObject.CookHeightfield(nbColumns, nbRows, pData, convexEdgeThreshold, bNoBoundaryEdge);
                if (self.IsValidPointer == false)
                    return null;
                return new TtPhyHeightfield(self);
            }
        }
        public TtPhyConvexMesh CookConvexMesh(Graphics.Mesh.UMeshDataProvider mesh)
        {
            var self = mCoreObject.CookConvexMesh(mesh.mCoreObject);
            if (self.IsValidPointer == false)
                return null;
            return new TtPhyConvexMesh(self);
        }
        public TtPhyTriMesh CookTriMesh(Graphics.Mesh.UMeshDataProvider mesh, Support.UBlobObject uvblob, Support.UBlobObject faceblob, Support.UBlobObject posblob)
        {
            var self = mCoreObject.CookTriMesh(mesh.mCoreObject, (uvblob != null) ? uvblob.mCoreObject : new IBlobObject(),
                (faceblob != null) ? faceblob.mCoreObject : new IBlobObject(),
                (posblob != null) ? posblob.mCoreObject : new IBlobObject());
            if (self.IsValidPointer == false)
                return null;
            return new TtPhyTriMesh(self);
        }
        public TtPhyMaterialManager PhyMaterialManager { get; } = new TtPhyMaterialManager();
        public UPhyMeshManager PhyMeshManager { get; } = new UPhyMeshManager();
    }
    public class UPhyModule : UModule<UEngine>
    {
        TtPhyContext mPhyContext;
        public TtPhyContext PhyContext { get => mPhyContext; }
        public override void Cleanup(UEngine host)
        {
            mPhyContext.PhyMeshManager.Cleanup();
            mPhyContext.PhyMaterialManager.Cleanup();
            mPhyContext = null;
        }
        public override async System.Threading.Tasks.Task<bool> Initialize(UEngine host)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();
            mPhyContext = new TtPhyContext();
            if (mPhyContext.mCoreObject.Init(0xFFFFFFFF) == 0)
                return false;
            return true;
        }
        public override void TickModule(UEngine host)
        {

        }
    }
}

namespace EngineNS
{
    partial class UEngine
    {
        public Bricks.PhysicsCore.UPhyModule PhyModule { get; set; } = new Bricks.PhysicsCore.UPhyModule();
    }
}
