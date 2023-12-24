using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.NxPhysics
{
    public interface TtShape
    {
        EngineNS.NxPhysics.NxShape NativeShape { get; }
        Graphics.Mesh.TtMesh DebugMesh { get; }
    }
    public class TtSphereShape : AuxPtrType<EngineNS.NxPhysics.NxSphereShape>, TtShape
    {
        Graphics.Mesh.TtMesh mDebugMesh = null;
        public Graphics.Mesh.TtMesh DebugMesh 
        {
            get
            {
                if (mDebugMesh == null)
                {
                    mDebugMesh = new Graphics.Mesh.TtMesh();
                    var clr = ((uint)UEngine.Instance.PxSystem.Random.Next()) | 0xff000000;
                    var radius = mCoreObject.mDesc.Radius.AsSingle();
                    var stack = (uint)(radius / UEngine.Instance.PxSystem.DebugTriangleSize);
                    var geoMesh = Graphics.Mesh.UMeshDataProvider.MakeSphere(radius, stack, stack, clr).ToMesh();
                    Graphics.Pipeline.Shader.UMaterial[] materials = new Graphics.Pipeline.Shader.UMaterial[]
                    {
                        UEngine.Instance.PxSystem.DebugShapeMaterial,
                    };
                    mDebugMesh.Initialize(geoMesh, materials, 
                        Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                }
                return mDebugMesh;
            }
        }
        public TtSphereShape(EngineNS.NxPhysics.NxSphereShape ptr)
        {
            mCoreObject = ptr;
        }
        public EngineNS.NxPhysics.NxShape NativeShape 
        {
            get => mCoreObject.NativeSuper; 
        }
    }
    public class TtBoxShape : AuxPtrType<EngineNS.NxPhysics.NxBoxShape>, TtShape
    {
        Graphics.Mesh.TtMesh mDebugMesh = null;
        public unsafe Graphics.Mesh.TtMesh DebugMesh
        {
            get
            {
                if (mDebugMesh == null)
                {
                    mDebugMesh = new Graphics.Mesh.TtMesh();
                    var clr = ((uint)UEngine.Instance.PxSystem.Random.Next()) | 0xff000000;
                    Vector3 halfExt = new Vector3();
                    Vector3 start = -halfExt;
                    Vector3 size = halfExt * 2.0f;
                    mCoreObject.mDesc.HalfExtent.ToVector3f(&halfExt);
                    var geoMesh = Graphics.Mesh.UMeshDataProvider.MakeBox(start.X, start.Y, start.Z,
                         size.X, size.Y, size.Z, clr).ToMesh();
                    Graphics.Pipeline.Shader.UMaterial[] materials = new Graphics.Pipeline.Shader.UMaterial[]
                    {
                        UEngine.Instance.PxSystem.DebugShapeMaterial,
                    };
                    mDebugMesh.Initialize(geoMesh, materials,
                        Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                }
                return mDebugMesh;
            }
        }
        public TtBoxShape(EngineNS.NxPhysics.NxBoxShape ptr)
        {
            mCoreObject = ptr;
        }
        public EngineNS.NxPhysics.NxShape NativeShape
        {
            get => mCoreObject.NativeSuper;
        }
    }
}
