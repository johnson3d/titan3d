using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace EngineNS.Graphics.Mesh.Modifier
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
    public struct FCullBounding
    {
        public FCullBounding()
        {

        }
        public FCullBounding(in BoundingBox box)
        {
            Center = box.GetCenter();
            Extent = box.GetSize() * 0.5f;
        }
        public FCullBounding(in DBoundingBox box)
        {
            Center = box.GetCenter().ToSingleVector3();
            Extent = box.GetSize().ToSingleVector3() * 0.5f;
        }
        public Vector3 Center;
        public float Center_Pad;
        public Vector3 Extent;
        public float Radius;
    }

    public class TtGpuCullSetupShading : Graphics.Pipeline.Shader.TtComputeShadingEnv
    {
        public override Vector3ui DispatchArg
        {
            get => new Vector3ui(128, 1, 1);
        }
        public TtGpuCullSetupShading()
        {
            CodeName = RName.GetRName("Shaders/Bricks/GpuDriven/GpuCulling.cginc", RName.ERNameType.Engine);
            MainName = "CS_GPUCullingSetup";

            this.UpdatePermutation();
        }
        protected override void EnvShadingDefines(in FPermutationId id, NxRHI.TtShaderDefinitions defines)
        {
            base.EnvShadingDefines(in id, defines);
        }
        public override void OnDrawCall(NxRHI.TtComputeDraw drawcall, Graphics.Pipeline.TtRenderPolicy policy)
        {
            var instanceSSBO = drawcall.TagObject as TtInstanceBufferSSBO;
            if (instanceSSBO == null)
                return;
        }
    }
    public class TtGpuCullFlushShading : Graphics.Pipeline.Shader.TtComputeShadingEnv
    {
        public override Vector3ui DispatchArg
        {
            get => new Vector3ui(64, 1, 1);
        }
        public TtGpuCullFlushShading()
        {
            CodeName = RName.GetRName("Shaders/Bricks/GpuDriven/GpuCulling.cginc", RName.ERNameType.Engine);
            MainName = "CS_GPUCullingFlush";

            this.UpdatePermutation();
        }
        protected override void EnvShadingDefines(in FPermutationId id, NxRHI.TtShaderDefinitions defines)
        {
            base.EnvShadingDefines(in id, defines);
        }
        public override void OnDrawCall(NxRHI.TtComputeDraw drawcall, Graphics.Pipeline.TtRenderPolicy policy)
        {
            var instanceSSBO = drawcall.TagObject as TtInstanceBufferSSBO;
            if (instanceSSBO == null)
                return;
        }
    }
    public class TtGpuCullShading : Graphics.Pipeline.Shader.TtComputeShadingEnv
    {
        public override Vector3ui DispatchArg
        {
            get => new Vector3ui(128, 1, 1);
        }
        public TtGpuCullShading()
        {
            CodeName = RName.GetRName("Shaders/Bricks/GpuDriven/GpuCulling.cginc", RName.ERNameType.Engine);
            MainName = "CS_GPUCullingMain";

            this.UpdatePermutation();
        }
        protected override void EnvShadingDefines(in FPermutationId id, NxRHI.TtShaderDefinitions defines)
        {
            base.EnvShadingDefines(in id, defines);
        }
        public override void OnDrawCall(NxRHI.TtComputeDraw drawcall, Graphics.Pipeline.TtRenderPolicy policy)
        {
            var instanceSSBO = drawcall.TagObject as TtInstanceBufferSSBO;
            if (instanceSSBO == null)
                return;
        }
    }
    public class TtGpuDrivenData
    {
        public unsafe struct FDrawArgs
        {
            public uint Offset;
            public fixed uint Arguments[5];
        }
        public Pipeline.TtGpuBuffer<Pipeline.Shader.FVSInstanceData> CullingBuffer = new Pipeline.TtGpuBuffer<Pipeline.Shader.FVSInstanceData>();
        public Pipeline.TtCpu2GpuBuffer<uint> DrawArgsBuffer = null;
        public Dictionary<uint, FDrawArgs> DrawArgsOffsetDict = null;

        public NxRHI.TtComputeDraw GpuCullSetupDrawcall;        
        public NxRHI.TtComputeDraw GpuCullFlushDrawcall;        
        public NxRHI.TtComputeDraw GpuCullDrawcall;
        public NxRHI.TtCbView GPUCullingCBV = null;

        public Mesh.Modifier.TtGpuCullSetupShading GpuCullSetupShading;
        public Mesh.Modifier.TtGpuCullFlushShading GpuCullFlushShading;
        public Mesh.Modifier.TtGpuCullShading GpuCullShading;

        public void SetupGpuData(Graphics.Pipeline.TtGpuCullingNode node, TtInstanceModifier instanceModifier)
        {
            GpuCullSetupShading = node.GpuCullSetupShading;
            GpuCullFlushShading = node.GpuCullFlushShading;
            GpuCullShading = node.GpuCullShading;

            DrawArgsOffsetDict = instanceModifier.DrawArgsOffsetDict;
            if (DrawArgsBuffer == null)
            {
                DrawArgsBuffer = new Pipeline.TtCpu2GpuBuffer<uint>();
                DrawArgsBuffer.Initialize(NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_IndirectArgs);
            }
            DrawArgsBuffer.SetSize(DrawArgsOffsetDict.Count * 5);

            unsafe
            {
                using (var tsCmd = new NxRHI.FTransientCmd(NxRHI.EQueueType.QU_Default, "TtInstanceModifier.FlushArgsBuffer"))
                {
                    foreach (var i in DrawArgsOffsetDict.Values)
                    {
                        DrawArgsBuffer.UpdateData((int)i.Offset * sizeof(uint), i.Arguments, 5 * sizeof(uint));
                    }

                    DrawArgsBuffer.Flush2GPU(tsCmd.CmdList);
                    DrawArgsBuffer.GpuBuffer.TransitionTo(tsCmd.CmdList, NxRHI.EGpuResourceState.GRS_UavIndirect);

                    if (instanceModifier.InstanceBuffers.NumOfInstance > 0)
                        CullingBuffer.SetSize((uint)instanceModifier.InstanceBuffers.NumOfInstance, IntPtr.Zero.ToPointer(), NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_SRV);
                }
            }
            
            GpuCullSetupDrawcall = TtEngine.Instance.GfxDevice.RenderContext.CreateComputeDraw();
            GpuCullSetupDrawcall.TagObject = this;

            GpuCullFlushDrawcall = TtEngine.Instance.GfxDevice.RenderContext.CreateComputeDraw();
            GpuCullFlushDrawcall.TagObject = this;
            
            GpuCullDrawcall = TtEngine.Instance.GfxDevice.RenderContext.CreateComputeDraw();
            GpuCullDrawcall.TagObject = this;
        }

        public unsafe void InstanceCulling(TtInstanceModifier mdf, NxRHI.ICommandList cmd, Pipeline.TtRenderPolicy policy)
        {
            if (!GpuCullSetupShading.IsReady || !GpuCullShading.IsReady || !GpuCullFlushShading.IsReady)
            {
                return;
            }
            if (mdf.InstanceBuffers.InstanceBuffer.DataArray.Count > CullingBuffer.NumElement)
            {
                CullingBuffer.SetSize((uint)mdf.InstanceBuffers.InstanceBuffer.DataArray.Count, IntPtr.Zero.ToPointer(), NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);
            }
            mdf.InstanceBuffers.Flush2GPU(cmd, mdf);

            int NumOfIndirectDraw = DrawArgsOffsetDict.Count;
            GpuCullSetupShading.SetDrawcallDispatch(this, policy, GpuCullSetupDrawcall, 1, 1, 1, true);
            GpuCullSetupDrawcall.BindCBuffer("cbGPUCulling", ref GPUCullingCBV);
            {
                GPUCullingCBV.SetValue("BoundCenter", mdf.MeshAABB.GetCenter());
                GPUCullingCBV.SetValue("BoundExtent", mdf.MeshAABB.GetSize() * 0.5f);
                GPUCullingCBV.SetValue("MaxInstance", mdf.InstanceBuffers.InstanceBuffer.DataArray.Count);
                GPUCullingCBV.SetValue("IndirectArgsOffset", 0);
                GPUCullingCBV.SetValue("NumOfIndirectDraw", NumOfIndirectDraw);
                GPUCullingCBV.SetValue("UseInstanceBounding", mdf.InstanceBuffers.InstanceBoundingBuffer != null ? (int)1 : (int)0);
            }
            GpuCullSetupDrawcall.BindUav("IndirectArgsBuffer", DrawArgsBuffer.Uav);
            //GpuCullSetupDrawcall.SetDebugName("InstanceCulling.Setup");
            cmd.PushGpuDraw(GpuCullSetupDrawcall.mCoreObject.NativeSuper);

            GpuCullShading.SetDrawcallDispatch(this, policy, GpuCullDrawcall, (uint)mdf.InstanceBuffers.InstanceBuffer.DataArray.Count, 1, 1, true);
            GpuCullDrawcall.BindCBuffer("cbGPUCulling", ref GPUCullingCBV);
            GpuCullDrawcall.BindCBuffer("cbPerCamera", policy.DefaultCamera.PerCameraCBuffer);
            GpuCullDrawcall.BindSrv("InstanceDataArray", mdf.InstanceBuffers.InstanceBuffer.Srv);
            if (mdf.InstanceBuffers.InstanceBoundingBuffer != null)
            {
                GpuCullDrawcall.BindSrv("InstanceBoundingArray", mdf.InstanceBuffers.InstanceBoundingBuffer.Srv);
            }
            GpuCullDrawcall.BindUav("CullInstanceDataArray", CullingBuffer.Uav);
            GpuCullDrawcall.BindUav("IndirectArgsBuffer", DrawArgsBuffer.Uav);
            //GpuCullDrawcall.SetDebugName("InstanceCulling");
            cmd.PushGpuDraw(GpuCullDrawcall.mCoreObject.NativeSuper);

            GpuCullFlushShading.SetDrawcallDispatch(this, policy, GpuCullFlushDrawcall, (uint)(NumOfIndirectDraw - 1), 1, 1, true);
            GpuCullFlushDrawcall.BindCBuffer("cbGPUCulling", ref GPUCullingCBV);
            GpuCullFlushDrawcall.BindUav("IndirectArgsBuffer", DrawArgsBuffer.Uav);
            //GpuCullFlushDrawcall.SetDebugName("InstanceCulling.Flush");
            cmd.PushGpuDraw(GpuCullFlushDrawcall.mCoreObject.NativeSuper);
        }

        //static bool bIndirect = true;
        public void OnDrawCall(TtInstanceModifier mdf, NxRHI.ICommandList cmd, NxRHI.TtGraphicDraw drawcall, Pipeline.TtRenderPolicy policy, TtMesh.TtAtom atom)
        {
            uint key = ((uint)(drawcall.SubMesh << 16) | drawcall.MeshAtom);
            TtGpuDrivenData.FDrawArgs drawArgs;
            if (DrawArgsOffsetDict.TryGetValue(key, out drawArgs))
            {
                drawcall.BindSRV(TtNameTable.VSInstanceDataArray, CullingBuffer.Srv);//mdf.InstanceBuffers.InstanceBuffer.Srv);// 
                drawcall.BindIndirectDrawArgsBuffer(DrawArgsBuffer.GpuBuffer, drawArgs.Offset * sizeof(int));
                //if (bIndirect)
                //    drawcall.BindIndirectDrawArgsBuffer(DrawArgsBuffer.GpuBuffer, drawArgs.Offset * sizeof(int));
                //else
                //    drawcall.BindIndirectDrawArgsBuffer(new NxRHI.UBuffer(), drawArgs.Offset * sizeof(int));
            }
        }
    }
    public class TtInstanceBufferSSBO : IDisposable
    {
        public Pipeline.TtCpu2GpuBuffer<Pipeline.Shader.FVSInstanceData> InstanceBuffer = new Pipeline.TtCpu2GpuBuffer<Pipeline.Shader.FVSInstanceData>();
        public Pipeline.TtCpu2GpuBuffer<FCullBounding> InstanceBoundingBuffer = null;

        public TtInstanceBufferSSBO()
        {
            InstanceBuffer.Initialize(NxRHI.EBufferType.BFT_SRV);
        }
        public void Dispose()
        {
            CoreSDK.DisposeObject(ref InstanceBuffer);
        }
        public int NumOfInstance
        {
            get
            {
                return InstanceBuffer.DataArray.Count;
            }
        }
        public unsafe void SetCapacity(TtInstanceModifier mdf, uint nSize, bool useInstanceBounding)
        {
            InstanceBuffer.SetCapacity((int)nSize);
            if (useInstanceBounding)
            {
                InstanceBoundingBuffer = new Pipeline.TtCpu2GpuBuffer<FCullBounding>();
                InstanceBoundingBuffer.Initialize(NxRHI.EBufferType.BFT_SRV);
                InstanceBoundingBuffer.SetCapacity((int)nSize);
            }
            else
            {
                CoreSDK.DisposeObject(ref InstanceBoundingBuffer);
            }
        }
        public void ResetInstance()
        {
            InstanceBuffer.SetSize(0);
            if (InstanceBoundingBuffer != null)
                InstanceBoundingBuffer.SetSize(0);
        }
        public uint PushInstance(TtInstanceModifier mdf, in Pipeline.Shader.FVSInstanceData instance, in FCullBounding bounding)
        {
            var index = (uint)InstanceBuffer.PushData(in instance);
            if (InstanceBoundingBuffer != null)
            {
                InstanceBoundingBuffer.PushData(bounding);
            }
            return index;
        }
        public unsafe void SetInstance(uint index, in Pipeline.Shader.FVSInstanceData instance)
        {
            InstanceBuffer.UpdateData((int)index, in instance);
        }
        public unsafe void Flush2GPU(NxRHI.ICommandList cmd, TtInstanceModifier mdf)
        {
            InstanceBuffer.Flush2GPU(cmd);
            if (InstanceBoundingBuffer != null)
            {
                InstanceBoundingBuffer.Flush2GPU(cmd);
            }
        }

        public void OnDrawCall(TtInstanceModifier mdf, NxRHI.ICommandList cmd, NxRHI.TtGraphicDraw drawcall, Pipeline.TtRenderPolicy policy, TtMesh.TtAtom atom)
        {
            drawcall.mCoreObject.DrawInstance = (ushort)NumOfInstance;

            if (mdf.GpuDrivenData != null)
            {
                mdf.GpuDrivenData.OnDrawCall(mdf, cmd, drawcall, policy, atom);
            }
            else
            {
                this.Flush2GPU(cmd, mdf);
                drawcall.BindSRV(TtNameTable.VSInstanceDataArray, InstanceBuffer.Srv);
            }
        }
    }

    public class TtInstanceModifier : Pipeline.Shader.IMeshModifier, IDisposable
    {
        public string ModifierNameVS { get => "DoInstancingModifierVS"; }
        public string ModifierNamePS { get => null; }
        public RName SourceName
        {
            get
            {
                return RName.GetRName("shaders/modifier/InstancingModifier.cginc", RName.ERNameType.Engine);
            }
        }
        public unsafe NxRHI.FShaderCode* GetHLSLCode(string includeName, string includeOriName)
        {
            return (NxRHI.FShaderCode*)0;
        }
        public string GetUniqueText()
        {
            return "";
        }
        public NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] {
                    NxRHI.EVertexStreamType.VST_Position,
                    NxRHI.EVertexStreamType.VST_Normal,
                    NxRHI.EVertexStreamType.VST_InstPos,
                    NxRHI.EVertexStreamType.VST_InstQuat,
                    NxRHI.EVertexStreamType.VST_InstScale,
                    NxRHI.EVertexStreamType.VST_F4_1};
        }
        public Graphics.Pipeline.Shader.EPixelShaderInput[] GetPSNeedInputs()
        {
            return new Graphics.Pipeline.Shader.EPixelShaderInput[] {
                    Graphics.Pipeline.Shader.EPixelShaderInput.PST_SpecialData,
                };
        }

        public TtInstanceBufferSSBO InstanceBuffers;

        public BoundingBox MeshAABB;

        public Dictionary<uint, TtGpuDrivenData.FDrawArgs> DrawArgsOffsetDict = new Dictionary<uint, TtGpuDrivenData.FDrawArgs>();
        ~TtInstanceModifier()
        {
            Dispose();
        }
        public unsafe void Initialize(Graphics.Mesh.TtMaterialMesh materialMesh)
        {
            InstanceBuffers = new TtInstanceBufferSSBO();

            MeshAABB = materialMesh.AABB;
            DrawArgsOffsetDict.Clear();
            uint DrawArgBufferOffset = 0;
            for (int i = 0; i < materialMesh.SubMeshes.Count; i++)
            {
                var mesh = materialMesh.SubMeshes[i].Mesh;
                for (uint j = 0; j < mesh.NumAtom; j++)
                {
                    var key = (uint)(i << 16) | j;
                    ref var atom = ref *mesh.mCoreObject.GetAtom(j, 0);

                    var arg = new TtGpuDrivenData.FDrawArgs();
                    arg.Offset = DrawArgBufferOffset;
                    arg.Arguments[0] = atom.m_NumPrimitives * 3;
                    arg.Arguments[1] = 0;
                    arg.Arguments[2] = atom.m_StartIndex;
                    arg.Arguments[3] = atom.m_BaseVertexIndex;
                    arg.Arguments[4] = 0;

                    DrawArgsOffsetDict.Add(key, arg);
                    DrawArgBufferOffset += 5;
                }
            }
        }
        public void Dispose()
        {
            CoreSDK.DisposeObject(ref InstanceBuffers);
        }
        public void SetCapacity(uint nSize, bool useInstanceBounding)
        {
            InstanceBuffers.SetCapacity(this, nSize, useInstanceBounding);
        }

        public TtGpuDrivenData GpuDrivenData = null;

        public uint PushInstance(in Pipeline.Shader.FVSInstanceData instance, in FCullBounding bounding)
        {
            return InstanceBuffers.PushInstance(this, in instance, in bounding);
        }
        public uint PushInstance(in Pipeline.Shader.FVSInstanceData instance)
        {
            return InstanceBuffers.PushInstance(this, in instance, new FCullBounding());
        }
        public unsafe void SetInstance(uint index, in Pipeline.Shader.FVSInstanceData instance)
        {
            InstanceBuffers.SetInstance(index, instance);
        }

        public unsafe void OnDrawCall(Graphics.Pipeline.Shader.TtMdfQueueBase mdfQueue1, NxRHI.ICommandList cmd, NxRHI.TtGraphicDraw drawcall, Pipeline.TtRenderPolicy policy, TtMesh.TtAtom atom)
        {
            InstanceBuffers.OnDrawCall(this, cmd, drawcall, policy, atom);
        }
    }

}
