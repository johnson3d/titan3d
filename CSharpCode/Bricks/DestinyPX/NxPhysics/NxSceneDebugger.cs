using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Pipeline;
using EngineNS.NxPhysics;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.NxPhysics
{
    [Bricks.CodeBuilder.ContextMenu("NxSceneDbg", "NxSceneDbg", UNode.EditorKeyword)]
    [UNode(NodeDataType = typeof(GamePlay.Scene.UNodeData), DefaultNamePrefix = "NxSceneDbg")]
    public class NxSceneDebugger : GamePlay.Scene.USceneActorNode
    {
        public override async Task<bool> InitializeNode(UWorld world, UNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            await base.InitializeNode(world, data, bvType, placementType);

            var mDevice = UEngine.Instance.PxSystem.Device;
            var sceneDesc = new NxSceneDesc();
            sceneDesc.TimeStep = NxReal.ByF32(0.001f);
            mScene = mDevice.CreateScene(in sceneDesc);
            unsafe
            {
                var rigidBodyDesc = new NxRigidBodyDesc();
                var rigidBody = mDevice.CreateRigidBody(in rigidBodyDesc);
                var sphereShapeDesc = new NxSphereShapeDesc();
                sphereShapeDesc.Radius = NxReal.ByF32(1.0f);
                sphereShapeDesc.Density = NxReal.ByF32(2.0f);
                var sphereShape = mDevice.CreateSphereShape(in sphereShapeDesc);
                rigidBody.AddShape(sphereShape);

                var nv = new PxVector3(NxReal.ByF32(10.0f), NxReal.ByF32(0.0f), NxReal.ByF32(0.0f));
                rigidBody.SetVelocity(rigidBody.Velocity + nv);
                mScene.AddActor(rigidBody);
            }
            unsafe
            {
                var rigidBodyDesc = new NxRigidBodyDesc();
                var rigidBody = mDevice.CreateRigidBody(in rigidBodyDesc);
                var sphereShapeDesc = new NxSphereShapeDesc();
                sphereShapeDesc.Radius = NxReal.ByF32(1.0f);
                sphereShapeDesc.Density = NxReal.ByF32(20.0f);
                var sphereShape = mDevice.CreateSphereShape(in sphereShapeDesc);
                rigidBody.AddShape(sphereShape);

                rigidBody.mCoreObject.GetTransform()->Position = new PxVector3(
                    NxReal.ByF32(5.0f), NxReal.ByF32(0.0f), NxReal.ByF32(0.0f));
                mScene.AddActor(rigidBody);
            }
            this.SetStyle(ENodeStyles.VisibleFollowParent);
            return true;
        }
        public TtScene mScene;
        public NxReal mSumElapsedTime = NxReal.ByF32(0.0f);
        public NxReal mStepTime = NxReal.ByF32(0.01f);
        public unsafe override bool OnTickLogic(GamePlay.UWorld world, Graphics.Pipeline.URenderPolicy policy)
        {
            bool bSteped = false;
            mSumElapsedTime = mSumElapsedTime + NxReal.ByF32(world.DeltaTimeSecond);
            while (mSumElapsedTime > mStepTime)
            {
                mScene.Simulate(mStepTime);
                mSumElapsedTime = mSumElapsedTime - mStepTime;
                bSteped = true;
            }

            if (bSteped == false)
                return true;

            BoundVolume.LocalAABB.InitEmptyBox();
            foreach (var i in mScene.mRigidBodies)
            {
                var pPQ = i.mCoreObject.GetTransform();
                var finalTrans = new EngineNS.NxPhysics.PxPQ();
                var transform = new FTransform();
                foreach (var j in i.Shapes)
                {
                    var pShapePQ = j.NativeShape.GetTransform();
                    PxPQ.Multiply(ref finalTrans, in *pPQ, in *pShapePQ);

                    var pos = new Vector3();
                    var quat = new Quaternion();
                    finalTrans.GetPosition()->ToVector3f(ref pos);                    
                    finalTrans.GetQuat()->ToQuatf(ref quat);

                    transform.Position = pos.AsDVector();
                    transform.Quat = quat;
                    j.DebugMesh.SetWorldTransform(in transform, this.GetWorld(), true);
                    j.DebugMesh.HostNode = this;
                }
            }

            return base.OnTickLogic(world, policy);
        }
        public override void OnGatherVisibleMeshes(GamePlay.UWorld.UVisParameter rp)
        {
            foreach (var i in mScene.mRigidBodies)
            {
                foreach (var j in i.Shapes)
                {
                    rp.VisibleMeshes.Add(j.DebugMesh);
                }
            }
        }
    }
}
