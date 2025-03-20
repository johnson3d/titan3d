using Assimp;
using EngineNS.GamePlay;
using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.NxRHI;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.RayTracing
{
    public class TtRayTracingEnv : TtRayTracingShadingEnv
    {
        public TtRayTracingEnv() 
        {
            CodeName = RName.GetRName("Shaders/ShadingEnv/RayTracing/Raytracing.hlsl", RName.ERNameType.Engine);
            MainName = "MyRaygenShader";
        }
        public override void OnDrawCall(TtRayTracingDraw drawcall, TtRenderPolicy policy)
        {
            var node = drawcall.TagObject as TtRayTracingNode;
            var binder = drawcall.FindBinder(EShaderBindType.SBT_CBV, "g_sceneCB");
            if (binder.IsValidPointer)
            {    
                drawcall.BindCBV(binder, node.SceneCBV);
            }
            binder = drawcall.FindBinder(EShaderBindType.SBT_CBV, "g_cubeCB");
            if (binder.IsValidPointer)
            {
                drawcall.BindCBV(binder, node.CubeCBV);
            }
            binder = drawcall.FindBinder(EShaderBindType.SBT_SRV, "Scene");
            if (binder.IsValidPointer)
            {
                drawcall.BindSrv(binder, node.mTopAccelerationStructure.mCoreObject.GetGpuBufferSRV());
            }
            binder = drawcall.FindBinder(EShaderBindType.SBT_SRV, "Indices");
            if (binder.IsValidPointer)
            {
                drawcall.BindSrv(binder, node.IBV);
            }
            binder = drawcall.FindBinder(EShaderBindType.SBT_SRV, "Vertices");
            if (binder.IsValidPointer)
            {
                drawcall.BindSrv(binder, node.VBV);
            }

            base.OnDrawCall(drawcall, policy);
        }
    }
    [Bricks.CodeBuilder.ContextMenu("RayTracing", "GI\\RayTracing", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    public class TtRayTracingNode : TtRenderGraphNode
    {
        public TtRenderGraphPin ColorPinInOut = TtRenderGraphPin.CreateInputOutput("Color", true, EPixelFormat.PXF_R16_FLOAT, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);
        public TtRenderGraphPin LightingPinOut = TtRenderGraphPin.CreateOutput("Lighting", true, EPixelFormat.PXF_R16_FLOAT, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);
        public TtRayTracingNode()
        {
            Name = "RayTracingNode";
        }
        public override void InitNodePins()
        {
            AddInputOutput(ColorPinInOut);
            ColorPinInOut.IsAllowInputNull = true;
            AddOutput(LightingPinOut);

            base.InitNodePins();
        }
        public override void Dispose()
        {
            base.Dispose();
        }
        public TtRayTracingEnv mBasePassShading;
        public TtRayTracingDraw mRayTracingDraw;
        public NxRHI.TtTopAccelerationStructure mTopAccelerationStructure;
        public NxRHI.TtAccelerationStructure mBlas0;
        public NxRHI.TtAStructureInstance mASInst0;

        public NxRHI.TtCbView SceneCBV;
        public NxRHI.TtCbView CubeCBV;
        public NxRHI.TtSrView VBV;
        public NxRHI.TtSrView IBV;
        public override async System.Threading.Tasks.Task Initialize(TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);

            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            BasePass.Initialize(rc, debugName + ".BasePass");

            mBasePassShading = await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<TtRayTracingEnv>();
            var binder = mBasePassShading.CurrentEffect.FindBinder(EShaderBindType.SBT_CBV, "g_sceneCB");
            if (binder.IsValidPointer)
            {
                SceneCBV = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);
            }
            binder = mBasePassShading.CurrentEffect.FindBinder(EShaderBindType.SBT_CBV, "g_cubeCB");
            if (binder.IsValidPointer)
            {
                CubeCBV = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);
            }

            FTopAccelerationStructureDesc topAccelerationStructureDesc = new FTopAccelerationStructureDesc();
            topAccelerationStructureDesc.SetDefault();
            mTopAccelerationStructure = TtEngine.Instance.GfxDevice.RenderContext.CreateTopAccelerationStructure(in topAccelerationStructureDesc);
            FAccelerationStructureDesc asDesc = new FAccelerationStructureDesc();
            asDesc.SetDefault();
            asDesc.m_GeometryCount = 1;
            var mesh = await TtEngine.Instance.GfxDevice.MeshPrimitiveManager.GetMeshPrimitive(RName.GetRName("mesh/base/box.vms", RName.ERNameType.Engine));
            var meshPtr = mesh.mCoreObject;
            var ib = meshPtr.GetGeomtryMesh().IndexBuffer.Buffer;
            var vb = meshPtr.GetGeomtryMesh().GetVertexArray().GetVB(EVertexStreamType.VST_Position).Buffer;
            {
                var ibvDesc = new NxRHI.FSrvDesc();
                ibvDesc.SetBuffer(true);
                ibvDesc.Buffer.NumElements = ib.Desc.Size / sizeof(uint);
                IBV = TtEngine.Instance.GfxDevice.RenderContext.CreateSRV(ib, in ibvDesc);
            }
            unsafe
            {
                var vbvDesc = new NxRHI.FSrvDesc();
                vbvDesc.SetBuffer(false);
                vbvDesc.Buffer.ElementWidth = (uint)sizeof(Vector3);
                vbvDesc.Buffer.StructureByteStride = (uint)sizeof(Vector3);
                vbvDesc.Buffer.NumElements = meshPtr.GetVertexNumber();
                VBV = TtEngine.Instance.GfxDevice.RenderContext.CreateSRV(vb, in vbvDesc);
            }
            unsafe
            {
                EngineNS.NxRHI.FMeshPrimitives** pGeometries = stackalloc EngineNS.NxRHI.FMeshPrimitives*[1];
                pGeometries[0] = meshPtr;
                asDesc.m_Geometries = pGeometries;
                mBlas0 = TtEngine.Instance.GfxDevice.RenderContext.CreateAccelerationStructure(in asDesc);
            }
            FAStructureInstanceDesc asiDesc = new FAStructureInstanceDesc();
            
            mASInst0 = TtEngine.Instance.GfxDevice.RenderContext.CreateAccelerationStructureInstance(in asiDesc, mBlas0);

            mTopAccelerationStructure.AddBLASInstance(mASInst0);
            mTopAccelerationStructure.BuildAcclerationStruture();
            mRayTracingDraw = TtEngine.Instance.GfxDevice.RenderContext.CreateRayTracingDraw();
            mRayTracingDraw.TagObject = this;
            mBasePassShading.SetDispatchRay(this, policy, mRayTracingDraw, 1, 1, 1);
        }
        public override void TickLogic(TtWorld world, TtRenderPolicy policy, bool bClear)
        {
            if (mRayTracingDraw == null)
            {
                return;
            }
            TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.Flush();
            //TtEngine.Instance.GfxDevice.RenderSwapQueue.CaptureRenderDocFrame = true;
            //TtEngine.Instance.GfxDevice.RenderSwapQueue.BeginFrameCapture();

            var cmdlist = BasePass.DrawCmdList;
            using (new NxRHI.TtCmdListScope(cmdlist))
            {
                cmdlist.PushGpuDraw(mRayTracingDraw);
                cmdlist.FlushDraws();
            }
            policy.CommitCommandList(cmdlist);
            TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.Flush();

            //TtEngine.Instance.GfxDevice.RenderSwapQueue.EndFrameCapture(this.Name);
        }
    }
}
