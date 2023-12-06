using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.NxPhysics
{
    public class TtDevice : AuxPtrType<EngineNS.NxPhysics.NxDevice>
    {
        public TtDevice()
        {
            mCoreObject = EngineNS.NxPhysics.NxDevice.CreateInstance();
        }
        public TtScene CreateScene(in EngineNS.NxPhysics.NxSceneDesc desc)
        {
            return new TtScene(mCoreObject.CreateScene(in desc));
        }
        public TtRigidBody CreateRigidBody(in EngineNS.NxPhysics.NxRigidBodyDesc desc)
        {
            return new TtRigidBody(mCoreObject.CreateRigidBody(in desc));
        }
        public TtSphereShape CreateSphereShape(in EngineNS.NxPhysics.NxSphereShapeDesc desc)
        {
            return new TtSphereShape(mCoreObject.CreateSphereShape(in desc));
        }
    }

    public class TtPxSystem : UModule<UEngine>
    {
        public TtDevice Device { get; } = new TtDevice();
        public Graphics.Pipeline.Shader.UMaterialInstance DebugShapeMaterial;
        public System.Random Random { get; } = new Random(0);
        public float DebugTriangleSize = 0.1f;
        public override int GetOrder()
        {
            return 2;
        }
        public override async Task<bool> Initialize(UEngine host)
        {
            DebugShapeMaterial = await host.GfxDevice.MaterialInstanceManager.GetMaterialInstance(
                RName.GetRName("material/whitecolor.uminst", RName.ERNameType.Engine));

            //var meshBuilder = new Graphics.Mesh.UMeshDataProvider();
            //var mesh = await UEngine.Instance.GfxDevice.MeshPrimitiveManager.GetMeshPrimitive(RName.GetRName("mesh/sphere001.vms", RName.ERNameType.Engine));
            //meshBuilder.InitFrom(mesh);
            //var clrBuffer = meshBuilder.mCoreObject.CreateStream(NxRHI.EVertexStreamType.VST_Color);
            //unsafe
            //{
            //    var ptr = (uint*)clrBuffer.GetData();
            //    for(uint i=0; i < meshBuilder.mCoreObject.VertexNumber; i++)
            //    {
            //        ptr[i] = 0xffffffff;
            //    }
            //}
            //meshBuilder.ToMesh().SaveAssetTo(RName.GetRName("mesh/sphere001_1.vms", RName.ERNameType.Engine));
            return true;
        }
        public override void Cleanup(UEngine host)
        {
            DebugShapeMaterial = null;
            base.Cleanup(host);
        }
    }
}

namespace EngineNS
{
    public partial class UEngine
    {
        public NxPhysics.TtPxSystem PxSystem { get; } = new NxPhysics.TtPxSystem();
    }
}
