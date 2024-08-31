﻿using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Pipeline;
using EngineNS.NxPhysics;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.NxPhysics
{
    [Bricks.CodeBuilder.ContextMenu("NxSceneDbg", "NxSceneDbg", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(GamePlay.Scene.TtNodeData), DefaultNamePrefix = "NxSceneDbg")]
    public class NxSceneDebugger : GamePlay.Scene.TtSceneActorNode
    {
        public override async Thread.Async.TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            await base.InitializeNode(world, data, bvType, placementType);

            var mDevice = TtEngine.Instance.PxSystem.Device;
            var sceneDesc = new NxSceneDesc();
            sceneDesc.TimeStep = NxReal.ByF32(0.001f);
            mScene = mDevice.CreateScene(in sceneDesc);
            unsafe
            {
                var rigidBodyDesc = new NxRigidBodyDesc();
                rigidBodyDesc.SetDefault();
                var rigidBody = mDevice.CreateRigidBody(in rigidBodyDesc);
                var sphereShapeDesc = new NxSphereShapeDesc();
                sphereShapeDesc.Radius = NxReal.ByF32(1.0f);
                sphereShapeDesc.Density = NxReal.ByF32(2.0f);
                var sphereShape = mDevice.CreateSphereShape(in sphereShapeDesc);
                rigidBody.AddShape(sphereShape);

                var nv = new PxVector3(NxReal.ByF32(13.0f), NxReal.ByF32(0.0f), NxReal.ByF32(0.0f));
                rigidBody.SetVelocity(rigidBody.Velocity + nv);
                //rigidBody.SetAngularVelocity(NxUtility.RandomUnitVector3(mRandom.mCoreObject));
                mScene.AddActor(rigidBody);
            }
            unsafe
            {
                var rigidBodyDesc = new NxRigidBodyDesc();
                rigidBodyDesc.SetDefault();
                //rigidBodyDesc.SetStatic();
                var rigidBody = mDevice.CreateRigidBody(in rigidBodyDesc);
                var sphereShapeDesc = new NxSphereShapeDesc();
                sphereShapeDesc.Radius = NxReal.ByF32(1.0f);
                sphereShapeDesc.Density = NxReal.ByF32(2.0f);
                var sphereShape = mDevice.CreateSphereShape(in sphereShapeDesc);
                rigidBody.AddShape(sphereShape);

                rigidBody.mCoreObject.GetTransform()->Position = new PxVector3(
                    NxReal.ByF32(5.0f), NxReal.ByF32(0.0f), NxReal.ByF32(0.0f));
                mScene.AddActor(rigidBody);
            }
            this.SetStyle(ENodeStyles.VisibleFollowParent);
            return true;
        }
        public TtRandom mRandom = new TtRandom(8);
        public TtScene mScene;
        public NxReal mSumElapsedTime = NxReal.ByF32(0.0f);
        public NxReal mStepTime = NxReal.ByF32(0.01f);
        public unsafe override bool OnTickLogic(GamePlay.TtWorld world, Graphics.Pipeline.TtRenderPolicy policy)
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
        public override void OnGatherVisibleMeshes(GamePlay.TtWorld.UVisParameter rp)
        {
            foreach (var i in mScene.mRigidBodies)
            {
                foreach (var j in i.Shapes)
                {
                    rp.AddVisibleMesh(j.DebugMesh);
                }
            }
        }
    }
}
