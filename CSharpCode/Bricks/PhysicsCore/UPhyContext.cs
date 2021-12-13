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
    public class UPhyContext : AuxPtrType<PhyContext>
    {
        public UPhyContext()
        {
            mCoreObject = PhyContext.CreateInstance();
        }
        public UPhySceneDesc CreateSceneDesc()
        {
            var self = mCoreObject.CreateSceneDesc();
            if (self.IsValidPointer == false)
                return null;
            return new UPhySceneDesc(self);
        }
        public UPhyScene CreateScene(UPhySceneDesc desc)
        {
            var self = mCoreObject.CreateScene(desc.mCoreObject);
            if (self.IsValidPointer == false)
                return null;
            return new UPhyScene(self);
        }
        public UPhyActor CreateActor(EPhyActorType type, in DVector3 p, in Quaternion q)
        {
            var self = mCoreObject.CreateActor(type, p.ToSingleVector3(), in q);
            if (self.IsValidPointer == false)
                return null;
            return new UPhyActor(self);
        }
        public UPhyMaterial CreateMaterial(float staticFriction, float dynamicFriction, float restitution)
        {
            var self = mCoreObject.CreateMaterial(staticFriction, dynamicFriction, restitution);
            if (self.IsValidPointer == false)
                return null;
            return new UPhyMaterial(self);
        }
        public UPhyShape CreateShapePlane(UPhyMaterial material)
        {
            var self = mCoreObject.CreateShapePlane(material.mCoreObject);
            if (self.IsValidPointer == false)
                return null;
            var result = new UPhyPlaneShape(self);
            result.Materials = new UPhyMaterial[1];
            result.Materials[0] = material;
            return result;
        }
        public UPhyShape CreateShapeBox(UPhyMaterial material, in Vector3 halfExtent)
        {
            var self = mCoreObject.CreateShapeBox(material.mCoreObject, in halfExtent);
            if (self.IsValidPointer == false)
                return null;
            var result = new UPhyBoxShape(self);
            result.Materials = new UPhyMaterial[1];
            result.Materials[0] = material;
            return result;
        }
        public UPhyShape CreateShapeSphere(UPhyMaterial material, float radius)
        {
            var self = mCoreObject.CreateShapeSphere(material.mCoreObject, radius);
            if (self.IsValidPointer == false)
                return null;
            var result = new UPhySphereShape(self);
            result.Materials = new UPhyMaterial[1];
            result.Materials[0] = material;
            return result;
        }
        public UPhyShape CreateShapeCapsule(UPhyMaterial material, float radius, float halfHeight)
        {
            var self = mCoreObject.CreateShapeCapsule(material.mCoreObject, radius, halfHeight);
            if (self.IsValidPointer == false)
                return null;
            var result = new UPhyCapsuleShape(self);
            result.Materials = new UPhyMaterial[1];
            result.Materials[0] = material;
            return result;
        }
        public UPhyShape CreateShapeConvex(UPhyMaterial material, UPhyConvexMesh mesh, in Vector3 scale, in Quaternion scaleRot)
        {
            var self = mCoreObject.CreateShapeConvex(material.mCoreObject, mesh.mCoreObject, in scale, in scaleRot);
            if (self.IsValidPointer == false)
                return null;
            var result = new UPhyConvoxShape(self);
            result.Materials = new UPhyMaterial[1];
            result.Materials[0] = material;
            return result;
        }
        public UPhyShape CreateShapeTriMesh(UPhyMaterial[] material, UPhyTriMesh mesh, in Vector3 scale, in Quaternion scaleRot)
        {
            unsafe
            {
                PhyMaterial* pMtls = stackalloc PhyMaterial[material.Length];
                for (int i = 0; i < material.Length; i++)
                {
                    pMtls[i] = material[i].mCoreObject;
                }
                var self = mCoreObject.CreateShapeTriMesh((PhyMaterial**)pMtls, material.Length, mesh.mCoreObject, in scale, in scaleRot);
                if (self.IsValidPointer == false)
                    return null;
                var result = new UPhyTriMeshShape(self);
                result.Materials = material;
                return result;
            }
        }
        public UPhyShape CreateShapeHeightfield(UPhyMaterial[] material, UPhyHeightfield heightfield, float heightScale, in Vector3 scale)
        {
            unsafe
            {
                PhyMaterial* pMtls = stackalloc PhyMaterial[material.Length];
                for (int i = 0; i < material.Length; i++)
                {
                    pMtls[i] = material[i].mCoreObject;
                }
                var self = mCoreObject.CreateShapeHeightfield((PhyMaterial**)pMtls, material.Length, heightfield.mCoreObject, heightScale, in scale);
                if (self.IsValidPointer == false)
                    return null;
                var result = new UPhyHeightfieldShape(self);
                result.Materials = material;
                return result;
            }
        }

        public unsafe UPhyHeightfield CookHeightfield(int nbColumns, int nbRows, PhyHeightFieldSample* pData, float convexEdgeThreshold = 0.0f, bool bNoBoundaryEdge = true)
        {
            unsafe
            {
                var self = mCoreObject.CookHeightfield(nbColumns, nbRows, pData, convexEdgeThreshold, bNoBoundaryEdge);
                if (self.IsValidPointer == false)
                    return null;
                return new UPhyHeightfield(self);
            }
        }
        public UPhyConvexMesh CookConvexMesh(Graphics.Mesh.CMeshDataProvider mesh)
        {
            var self = mCoreObject.CookConvexMesh(mesh.mCoreObject);
            if (self.IsValidPointer == false)
                return null;
            return new UPhyConvexMesh(self);
        }
        public UPhyTriMesh CookTriMesh(Graphics.Mesh.CMeshDataProvider mesh, Support.CBlobObject uvblob, Support.CBlobObject faceblob, Support.CBlobObject posblob)
        {
            var self = mCoreObject.CookTriMesh(mesh.mCoreObject, (uvblob != null) ? uvblob.mCoreObject : new IBlobObject(),
                (faceblob != null) ? faceblob.mCoreObject : new IBlobObject(),
                (posblob != null) ? posblob.mCoreObject : new IBlobObject());
            if (self.IsValidPointer == false)
                return null;
            return new UPhyTriMesh(self);
        }
        public UPhyMaterialManager PhyMaterialManager { get; } = new UPhyMaterialManager();
        public UPhyMeshManager PhyMeshManager { get; } = new UPhyMeshManager();
    }
    public class UPhyModule : UModule<UEngine>
    {
        UPhyContext mPhyContext;
        public UPhyContext PhyContext { get => mPhyContext; }
        public override void Cleanup(UEngine host)
        {
            mPhyContext.PhyMeshManager.Cleanup();
            mPhyContext.PhyMaterialManager.Cleanup();
            mPhyContext = null;
        }
        public override async System.Threading.Tasks.Task<bool> Initialize(UEngine host)
        {
            await Thread.AsyncDummyClass.DummyFunc();
            mPhyContext = new UPhyContext();
            if (mPhyContext.mCoreObject.Init(0xFFFFFFFF) == 0)
                return false;
            return true;
        }
        public override void Tick(UEngine host)
        {

        }
    }
}

namespace EngineNS
{
    partial class UEngine
    {
        public Bricks.PhysicsCore.UPhyModule PhyModue { get; set; } = new Bricks.PhysicsCore.UPhyModule();
    }
}
