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
                drawcall.BindSrv(binder, node.VBV_Normal);
            }
            binder = drawcall.FindBinder(EShaderBindType.SBT_UAV, "RenderTarget");
            if (binder.IsValidPointer)
            {
                drawcall.BindUav(binder, node.GetAttachBuffer(node.LightingPinOut).Uav);
            }
            if (node.DiffuseTextures == null)
            {
                node.DiffuseTextures = drawcall.CreateBindless("DiffuseTextures");
                if (node.DiffuseTextures != null && node.DiffuseTextures.BindType == EShaderBindType.SBT_SRV)
                {
                    node.DiffuseTextures.SetSrv(0, TtEngine.Instance.GfxDevice.TextureManager.DefaultTexture);
                    node.DiffuseTextures.SetSrv(1, TtEngine.Instance.GfxDevice.TextureManager.DefaultTexture);
                    node.DiffuseTextures.SetSrv(2, TtEngine.Instance.GfxDevice.TextureManager.DefaultTexture);
                }
            }
            base.OnDrawCall(drawcall, policy);
        }
    }
    [Bricks.CodeBuilder.ContextMenu("RayTracing", "GI\\RayTracing", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    public class TtRayTracingNode : TAuxRenderGraphNode<TtRayTracingNode>
    {
        public TtRenderGraphPin ColorPinInOut = TtRenderGraphPin.CreateInputOutput("Color", true, EPixelFormat.PXF_R16_FLOAT, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);
        public TtRenderGraphPin LightingPinOut = TtRenderGraphPin.CreateOutput("Lighting", true, EPixelFormat.PXF_R11G11B10_FLOAT, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);
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
        public NxRHI.TtSrView VBV_Normal;
        public NxRHI.TtSrView IBV;
        public NxRHI.TtBindless DiffuseTextures;
        public override async Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);

            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            mBasePassShading = await Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<TtRayTracingEnv>();
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
            var mesh = await RName.GetRName("mesh/base/box.vms", RName.ERNameType.Engine).GetAsset<Graphics.Mesh.TtMeshPrimitives>();
            mBlas0 = Graphics.Mesh.TtMeshPrimitives.CreateAStructure(mesh);
            var ib = mesh.GetIndexBuffer();
            var vb = mesh.GetVertexBuffer(EVertexStreamType.VST_Normal);
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
                vbvDesc.Buffer.NumElements = mesh.VertexNumber;
                VBV_Normal = TtEngine.Instance.GfxDevice.RenderContext.CreateSRV(vb, in vbvDesc);
            }
            
            FAStructureInstanceDesc asiDesc = new FAStructureInstanceDesc();
            asiDesc.SetDefault();
            asiDesc.InstanceID = 0;
            asiDesc.InstanceMask = 1;
            asiDesc.Flags = ERayTracingInstanceFlags.RTI_FLAG_NONE;
            asiDesc.InstanceContributionToHitGroupIndex = 0;
            asiDesc.Matrix = Matrix.Identity;
            mASInst0 = TtEngine.Instance.GfxDevice.RenderContext.CreateAccelerationStructureInstance(in asiDesc, mBlas0);
            //mASInst0.mCoreObject.SetMatrix();
            mTopAccelerationStructure.AddBLASInstance(mASInst0);
            mTopAccelerationStructure.BuildAcclerationStruture();
            mRayTracingDraw = TtEngine.Instance.GfxDevice.RenderContext.CreateRayTracingDraw();
            mRayTracingDraw.TagObject = this;
        }
        public override void OnResize(TtRenderPolicy policy, float x, float y)
        {
            LightingPinOut.Attachement.Width = (uint)x;
            LightingPinOut.Attachement.Height = (uint)y;
        }
        public override void Tick(TtWorld world, TtRenderPolicy policy, NxRHI.TtCommandList frameCmdList, bool bClear)
        {
            if (mRayTracingDraw == null)
            {
                return;
            }
            var vpInvMatrix = policy.DefaultCamera.mCoreObject.GetViewProjectionInverse();
            var offset = policy.DefaultCamera.mCoreObject.GetMatrixStartPosition();
            SceneCBV.SetMatrix("projectionToWorld", 0, in vpInvMatrix);
            var cameraPosition = policy.DefaultCamera.mCoreObject.GetLocalPosition();
            SceneCBV.SetValue("cameraPosition", new Vector4(cameraPosition.X, cameraPosition.Y, cameraPosition.Z, 0));
            var lightPosition = new Vector3(10, 10, 10);//world.GetSun(0).Location.ToLocalPosition(offset);
            SceneCBV.SetValue("lightPosition", new Vector4(lightPosition.X, lightPosition.Y, lightPosition.Z, 0));
            SceneCBV.SetValue("lightAmbientColor", Color4f.FromColor4b(new Color4b(128, 128, 0, 255)));
            SceneCBV.SetValue("lightDiffuseColor", Color4f.FromColor4b(Color4b.White));
            if (world.GetSun(0) != null)
                SceneCBV.SetValue("lightDirection", world.GetSun(0).DirectionLight.Direction);

            CubeCBV.SetValue("albedo", Vector4.One);
            mBasePassShading.SetDispatchRay(this, policy, mRayTracingDraw, LightingPinOut.Attachement.Width, LightingPinOut.Attachement.Height, 1);
            //TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.Flush();

            var cmdlist = TtEngine.Instance.GfxDevice.RenderContext.CmdListManager.GetCmdList();
            using (new NxRHI.TtCmdListScope(cmdlist, "RayTracing"))
            {
                cmdlist.PushGpuDraw(mRayTracingDraw);
                cmdlist.FlushDraws();
            }
            policy.CommitCommandList(cmdlist, "RayTracing");
            //TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.Flush();
        }
    }
}

namespace EngineNS.Graphics.Mesh
{
    public partial class TtMeshPrimitives
    { 
        public static unsafe TtAccelerationStructure CreateAStructure(TtMeshPrimitives mesh)
        {
            FAccelerationStructureDesc asDesc = new FAccelerationStructureDesc();
            asDesc.SetDefault();
            asDesc.m_GeometryCount = 1;
            NxRHI.FMeshPrimitives** pGeometries = stackalloc NxRHI.FMeshPrimitives*[1];
            pGeometries[0] = mesh.mCoreObject;
            asDesc.m_Geometries = pGeometries;
            return TtEngine.Instance.GfxDevice.RenderContext.CreateAccelerationStructure(in asDesc);
        }

        /// <summary>
        /// 尝试构建 BLAS.
        /// 当 AMeta.HasBLAS == true 时, 在 mesh 加载完成后自动调用.
        /// 当前 DXR 不支持 Blob 序列化, 始终从 VB/IB 重建.
        /// </summary>
        public async Thread.Async.TtTask TryLoadOrBuildBLAS()
        {
            if (BLAS != null)
                return;

            var meshMeta = GetAMeta() as TtMeshPrimitivesAMeta;
            if (meshMeta == null || meshMeta.HasBLAS == false)
                return;

            BLAS = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                return CreateAStructure(this);
            }, Thread.Async.EAsyncTarget.TPools);

            if (BLAS != null)
            {
                Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(
                    Profiler.ELogTag.Info, "BLAS",
                    $"BLAS built for {AssetName}");
            }
        }
    }
}
