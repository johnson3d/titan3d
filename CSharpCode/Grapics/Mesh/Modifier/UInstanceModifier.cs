using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace EngineNS.Graphics.Mesh.Modifier
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
    public struct FVSInstanceData
    {
        public Vector3 Position;
        public uint HitProxyId;

        public Vector3 Scale;
        public uint CustomData2;

        public Quaternion Quat;
        public Vector4ui UserData;

        public Vector4ui PointLightIndices;
    };

    public class TtGpuCullSetupShading : Graphics.Pipeline.Shader.UComputeShadingEnv
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
        protected override void EnvShadingDefines(in FPermutationId id, NxRHI.UShaderDefinitions defines)
        {
            base.EnvShadingDefines(in id, defines);
        }
        public override void OnDrawCall(NxRHI.UComputeDraw drawcall, Graphics.Pipeline.URenderPolicy policy)
        {
            var instanceSSBO = drawcall.TagObject as TtInstanceBufferSSBO;
            if (instanceSSBO == null)
                return;
        }
    }
    public class TtGpuCullFlushShading : Graphics.Pipeline.Shader.UComputeShadingEnv
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
        protected override void EnvShadingDefines(in FPermutationId id, NxRHI.UShaderDefinitions defines)
        {
            base.EnvShadingDefines(in id, defines);
        }
        public override void OnDrawCall(NxRHI.UComputeDraw drawcall, Graphics.Pipeline.URenderPolicy policy)
        {
            var instanceSSBO = drawcall.TagObject as TtInstanceBufferSSBO;
            if (instanceSSBO == null)
                return;
        }
    }
    public class TtGpuCullShading : Graphics.Pipeline.Shader.UComputeShadingEnv
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
        protected override void EnvShadingDefines(in FPermutationId id, NxRHI.UShaderDefinitions defines)
        {
            base.EnvShadingDefines(in id, defines);
        }
        public override void OnDrawCall(NxRHI.UComputeDraw drawcall, Graphics.Pipeline.URenderPolicy policy)
        {
            var instanceSSBO = drawcall.TagObject as TtInstanceBufferSSBO;
            if (instanceSSBO == null)
                return;
        }
    }
    public interface IInstanceBuffers : IDisposable
    {
        int NumOfInstance { get; }
        void SetCapacity(TtInstanceModifier mdf, uint nSize);
        uint PushInstance(TtInstanceModifier mdf, in FVSInstanceData instance);
        void SetInstance(uint index, in FVSInstanceData instance);
        void Flush2GPU(NxRHI.ICommandList cmd, TtInstanceModifier mdf);
        void OnDrawCall(TtInstanceModifier mdf, NxRHI.ICommandList cmd, Pipeline.URenderPolicy.EShadingType shadingType, NxRHI.UGraphicDraw drawcall, Pipeline.URenderPolicy policy, TtMesh.TtAtom atom);
        void InstanceCulling(TtInstanceModifier mdf, NxRHI.ICommandList cmd, Pipeline.URenderPolicy policy, NxRHI.UUaView argBufferUAV, uint argBufferOffset);
    }
    public class TtInstanceBufferSSBO : IInstanceBuffers
    {
        public Pipeline.TtCpu2GpuBuffer<FVSInstanceData> InstanceBuffer = new Pipeline.TtCpu2GpuBuffer<FVSInstanceData>();
        public Pipeline.TtGpuBuffer<FVSInstanceData> CullingBuffer = new Pipeline.TtGpuBuffer<FVSInstanceData>();
        public TtGpuCullSetupShading GpuCullSetupShading;
        public NxRHI.UComputeDraw GpuCullSetupDrawcall;
        public TtGpuCullFlushShading GpuCullFlushShading;
        public NxRHI.UComputeDraw GpuCullFlushDrawcall;
        public TtGpuCullShading GpuCullShading;
        public NxRHI.UComputeDraw GpuCullDrawcall;
        public NxRHI.UCbView GPUCullingCBV = null;
        public TtInstanceBufferSSBO()
        {
            InstanceBuffer.Initialize(NxRHI.EBufferType.BFT_SRV);
            GpuCullSetupShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<TtGpuCullSetupShading>();
            GpuCullSetupDrawcall = UEngine.Instance.GfxDevice.RenderContext.CreateComputeDraw();
            GpuCullSetupDrawcall.TagObject = this;

            GpuCullFlushShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<TtGpuCullFlushShading>();
            GpuCullFlushDrawcall = UEngine.Instance.GfxDevice.RenderContext.CreateComputeDraw();
            GpuCullFlushDrawcall.TagObject = this;

            GpuCullShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<TtGpuCullShading>();
            GpuCullDrawcall = UEngine.Instance.GfxDevice.RenderContext.CreateComputeDraw();
            GpuCullDrawcall.TagObject = this;
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
        public unsafe void SetCapacity(TtInstanceModifier mdf, uint nSize)
        {
            InstanceBuffer.SetCapacity((int)nSize);
            CullingBuffer.SetSize(nSize, IntPtr.Zero.ToPointer(), NxRHI.EBufferType.BFT_SRV| NxRHI.EBufferType.BFT_UAV);
        }
        public uint PushInstance(TtInstanceModifier mdf, in FVSInstanceData instance)
        {
            return (uint)InstanceBuffer.PushData(in instance);
        }
        public unsafe void SetInstance(uint index, in FVSInstanceData instance)
        {
            InstanceBuffer.UpdateData((int)index, in instance);
        }
        public unsafe void Flush2GPU(NxRHI.ICommandList cmd, TtInstanceModifier mdf)
        {
            InstanceBuffer.Flush2GPU(cmd);
        }

        public void OnDrawCall(TtInstanceModifier mdf, NxRHI.ICommandList cmd, Pipeline.URenderPolicy.EShadingType shadingType, NxRHI.UGraphicDraw drawcall, Pipeline.URenderPolicy policy, TtMesh.TtAtom atom)
        {
            drawcall.mCoreObject.DrawInstance = (ushort)NumOfInstance;
            
            this.Flush2GPU(cmd, mdf);
            if (mdf.IsGpuCulling)
            {
                uint key = ((uint)(drawcall.SubMesh << 16) | drawcall.MeshAtom);
                uint offset;
                if (mdf.DrawArgsOffsetDict.TryGetValue(key, out offset))
                {
                    drawcall.BindSRV(VNameString.FromString("VSInstanceDataArray"), CullingBuffer.Srv);
                    drawcall.BindIndirectDrawArgsBuffer(mdf.DrawArgsBuffer.GpuBuffer, offset * sizeof(int));
                }
            }
            else
            {
                drawcall.BindSRV(VNameString.FromString("VSInstanceDataArray"), InstanceBuffer.Srv);
            }
        }
        public unsafe void InstanceCulling(TtInstanceModifier mdf, NxRHI.ICommandList cmd, Pipeline.URenderPolicy policy, NxRHI.UUaView argBufferUAV, uint argBufferOffset)
        {
            if (!GpuCullSetupShading.IsReady || !GpuCullShading.IsReady || !GpuCullFlushShading.IsReady)
            {
                return;
            }
            int NumOfIndirectDraw = mdf.DrawArgsOffsetDict.Count;
            GpuCullSetupShading.SetDrawcallDispatch(this, policy, GpuCullSetupDrawcall, 1, 1, 1, true);
            GpuCullSetupDrawcall.BindCBuffer("cbGPUCulling", ref GPUCullingCBV);
            GPUCullingCBV.SetValue("BoundCenter", mdf.MeshAABB.GetCenter());
            GPUCullingCBV.SetValue("BoundExtent", mdf.MeshAABB.GetSize() * 0.5f);
            GPUCullingCBV.SetValue("MaxInstance", CullingBuffer.NumElement);
            GPUCullingCBV.SetValue("IndirectArgsOffset", argBufferOffset);
            GPUCullingCBV.SetValue("NumOfIndirectDraw", NumOfIndirectDraw);
            GpuCullSetupDrawcall.BindUav("IndirectArgsBuffer", mdf.DrawArgsBuffer.Uav);
            GpuCullSetupDrawcall.SetDebugName("InstanceCulling.Setup");
            cmd.PushGpuDraw(GpuCullSetupDrawcall.mCoreObject.NativeSuper);

            GpuCullShading.SetDrawcallDispatch(this, policy, GpuCullDrawcall, 1, 1, 1, true);
            GpuCullDrawcall.BindCBuffer("cbGPUCulling", ref GPUCullingCBV);
            GpuCullDrawcall.BindCBuffer("cbPerCamera", policy.DefaultCamera.PerCameraCBuffer);
            GpuCullDrawcall.BindSrv("InstanceDataArray", InstanceBuffer.Srv);
            GpuCullDrawcall.BindUav("CullInstanceDataArray", CullingBuffer.Uav);
            GpuCullDrawcall.BindUav("IndirectArgsBuffer", mdf.DrawArgsBuffer.Uav);
            GpuCullDrawcall.SetDebugName("InstanceCulling");
            cmd.PushGpuDraw(GpuCullDrawcall.mCoreObject.NativeSuper);

            GpuCullFlushShading.SetDrawcallDispatch(this, policy, GpuCullFlushDrawcall, (uint)(NumOfIndirectDraw - 1), 1, 1, true);
            GpuCullFlushDrawcall.BindCBuffer("cbGPUCulling", ref GPUCullingCBV);
            GpuCullFlushDrawcall.BindUav("IndirectArgsBuffer", mdf.DrawArgsBuffer.Uav);
            GpuCullFlushDrawcall.SetDebugName("InstanceCulling.Flush");
            cmd.PushGpuDraw(GpuCullFlushDrawcall.mCoreObject.NativeSuper);
        }
    }
    //public class TtInstanceBufferVB : IInstanceBuffers
    //{
    //    public Vector3[] mPosData = null;
    //    public Vector4[] mScaleData = null;
    //    public Quaternion[] mRotateData = null;
    //    public Vector4ui[] mF41Data = null;

    //    public NxRHI.UVbView mPosVB;
    //    public NxRHI.UVbView mScaleVB;
    //    public NxRHI.UVbView mRotateVB;
    //    public NxRHI.UVbView mF41VB;

    //    public NxRHI.UVertexArray mAttachVBs = null;
    //    public void Dispose()
    //    {
    //        mPosVB?.Dispose();
    //        mPosVB = null;
    //        mScaleVB?.Dispose();
    //        mScaleVB = null;
    //        mRotateVB?.Dispose();
    //        mRotateVB = null;
    //        mF41VB?.Dispose();
    //        mF41VB = null;

    //        mPosData = null;
    //        mScaleData = null;
    //        mRotateData = null;
    //        mF41Data = null;
    //    }
    //    public unsafe void SureBuffers(TtInstanceModifier mdf, uint nSize)
    //    {
    //        if (mdf.mMaxNumber > nSize)
    //        {
    //            return;
    //        }

    //        var oldPos = mPosData;
    //        var oldScale = mScaleData;
    //        var oldQuat = mRotateData;
    //        var oldF41 = mF41Data;

    //        Dispose();

    //        mdf.mMaxNumber = nSize * 2;
    //        //mInstDataArray = new VSInstantData[mMaxNumber];
    //        mPosData = new Vector3[mdf.mMaxNumber];
    //        mScaleData = new Vector4[mdf.mMaxNumber];
    //        mRotateData = new Quaternion[mdf.mMaxNumber];
    //        mF41Data = new Vector4ui[mdf.mMaxNumber];

    //        if (mdf.mCurNumber > 0)
    //        {
    //            fixed (Vector3* pSrc = &oldPos[0])
    //            fixed (Vector3* pTar = &mPosData[0])
    //            {
    //                CoreSDK.MemoryCopy(pTar, pSrc, mdf.mCurNumber * (uint)sizeof(Vector3));
    //            }

    //            fixed (Vector4* pSrc = &oldScale[0])
    //            fixed (Vector4* pTar = &mScaleData[0])
    //            {
    //                CoreSDK.MemoryCopy(pTar, pSrc, mdf.mCurNumber * (uint)sizeof(Vector4));
    //            }

    //            fixed (Quaternion* pSrc = &oldQuat[0])
    //            fixed (Quaternion* pTar = &mRotateData[0])
    //            {
    //                CoreSDK.MemoryCopy(pTar, pSrc, mdf.mCurNumber * (uint)sizeof(Quaternion));
    //            }

    //            fixed (Vector4ui* pSrc = &oldF41[0])
    //            fixed (Vector4ui* pTar = &mF41Data[0])
    //            {
    //                CoreSDK.MemoryCopy(pTar, pSrc, mdf.mCurNumber * (uint)sizeof(Vector4ui));
    //            }
    //        }


    //        var rc = UEngine.Instance.GfxDevice.RenderContext;

    //        if (mAttachVBs == null)
    //            mAttachVBs = rc.CreateVertexArray();

    //        var desc = new NxRHI.FVbvDesc();
    //        //desc.CpuAccess = NxRHI.ECpuAccess.CAS_WRITE;
    //        desc.m_Size = (UInt32)(sizeof(Vector3) * mdf.mMaxNumber);
    //        desc.m_Stride = (UInt32)sizeof(Vector3);
    //        mPosVB = rc.CreateVBV(null, in desc);

    //        desc.m_Size = (UInt32)(sizeof(Vector4) * mdf.mMaxNumber);
    //        desc.m_Stride = (UInt32)sizeof(Vector4);
    //        mScaleVB = rc.CreateVBV(null, in desc);

    //        desc.m_Size = (UInt32)(sizeof(Quaternion) * mdf.mMaxNumber);
    //        desc.m_Stride = (UInt32)sizeof(Quaternion);
    //        mRotateVB = rc.CreateVBV(null, in desc);

    //        desc.m_Size = (UInt32)(sizeof(Vector4) * mdf.mMaxNumber);
    //        desc.m_Stride = (UInt32)sizeof(Vector4);
    //        mF41VB = rc.CreateVBV(null, in desc);
    //    }

    //    public unsafe void Flush2GPU(NxRHI.ICommandList cmd, TtInstanceModifier mdf)
    //    {
    //        if (mdf.mCurNumber == 0)
    //            return;

    //        var rc = UEngine.Instance.GfxDevice.RenderContext;
    //        fixed (Vector3* p = &mPosData[0])
    //        {
    //            var dataSize = (UInt32)sizeof(Vector3) * mdf.mCurNumber;
    //            mPosVB.UpdateGpuData(cmd, 0, p, dataSize);
    //        }
    //        fixed (Vector4* p = &mScaleData[0])
    //        {
    //            var dataSize = (UInt32)sizeof(Vector4) * mdf.mCurNumber;
    //            mScaleVB.UpdateGpuData(cmd, 0, p, dataSize);
    //        }
    //        fixed (Quaternion* p = &mRotateData[0])
    //        {
    //            var dataSize = (UInt32)sizeof(Quaternion) * mdf.mCurNumber;
    //            mRotateVB.UpdateGpuData(cmd, 0, p, dataSize);
    //        }
    //        fixed (Vector4ui* p = &mF41Data[0])
    //        {
    //            var dataSize = (UInt32)sizeof(Vector4ui) * mdf.mCurNumber;
    //            mF41VB.UpdateGpuData(cmd, 0, p, dataSize);
    //        }

    //        mAttachVBs.BindVB(NxRHI.EVertexStreamType.VST_InstPos, mPosVB);
    //        mAttachVBs.BindVB(NxRHI.EVertexStreamType.VST_InstQuat, mRotateVB);
    //        mAttachVBs.BindVB(NxRHI.EVertexStreamType.VST_InstScale, mScaleVB);
    //        mAttachVBs.BindVB(NxRHI.EVertexStreamType.VST_F4_1, mF41VB);
    //    }

    //    public uint PushInstance(TtInstanceModifier mdf, in FVSInstanceData instance)
    //    {
    //        var rc = UEngine.Instance.GfxDevice.RenderContext;
    //        SureBuffers(mdf, mdf.mCurNumber + 1);

    //        mPosData[mdf.mCurNumber] = pos;
    //        mScaleData[mdf.mCurNumber].X = scale.X;
    //        mScaleData[mdf.mCurNumber].Y = scale.Y;
    //        mScaleData[mdf.mCurNumber].Z = scale.Z;
    //        mScaleData[mdf.mCurNumber].W = hitProxyId;
    //        mRotateData[mdf.mCurNumber] = quat;
    //        mF41Data[mdf.mCurNumber] = f41;

    //        var result = mdf.mCurNumber;
    //        mdf.mCurNumber++;
    //        return result;
    //    }
    //    public unsafe void SetInstance(uint index, in FVSInstanceData instance)
    //    {
    //        if (pos != IntPtr.Zero.ToPointer())
    //            mPosData[index] = *pos;
    //        if (scale != IntPtr.Zero.ToPointer())
    //        {
    //            mScaleData[index].X = scale->X;
    //            mScaleData[index].Y = scale->Y;
    //            mScaleData[index].Z = scale->Z;
    //        }
    //        if (hitProxyId != IntPtr.Zero.ToPointer())
    //            mScaleData[index].W = *hitProxyId;
    //        if (quat != IntPtr.Zero.ToPointer())
    //            mRotateData[index] = *quat;
    //        if (f41 != IntPtr.Zero.ToPointer())
    //            mF41Data[index] = *f41;
    //    }

    //    public void OnDrawCall(TtInstanceModifier mdf, NxRHI.ICommandList cmd, Pipeline.URenderPolicy.EShadingType shadingType, NxRHI.UGraphicDraw drawcall, Pipeline.URenderPolicy policy, TtMesh.TtAtom atom)
    //    {
    //        drawcall.BindAttachVertexArray(mAttachVBs);
    //    }
    //    public void InstanceCulling(NxRHI.ICommandList cmd, NxRHI.UGraphicDraw drawcall, Pipeline.URenderPolicy policy, NxRHI.UUaView argBufferUAV, uint argBufferOffset)
    //    {

    //    }
    //}

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

        public IInstanceBuffers InstanceBuffers;

        public BoundingBox MeshAABB;
        public Pipeline.TtCpu2GpuBuffer<uint> DrawArgsBuffer = null;
        public Dictionary<uint, uint> DrawArgsOffsetDict = new Dictionary<uint, uint>();
        ~TtInstanceModifier()
        {
            Dispose();
        }
        public unsafe void Initialize(Graphics.Mesh.UMaterialMesh materialMesh)
        {
            MeshAABB = materialMesh.AABB;

            DrawArgsBuffer = new Pipeline.TtCpu2GpuBuffer<uint>();
            DrawArgsBuffer.Initialize(NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_IndirectArgs);

            DrawArgsOffsetDict.Clear();
            for (int i = 0; i < materialMesh.SubMeshes.Count; i++)
            {
                var mesh = materialMesh.SubMeshes[i].Mesh;
                for (uint j = 0; j < mesh.NumAtom; j++)
                {
                    var key = (uint)(i << 16) | j;
                    DrawArgsOffsetDict.Add(key, (uint)DrawArgsBuffer.DataArray.Count);

                    ref var atom = ref *mesh.mCoreObject.GetAtom(j, 0);
                    DrawArgsBuffer.PushData(atom.m_NumPrimitives * 3);
                    DrawArgsBuffer.PushData(0);
                    DrawArgsBuffer.PushData(atom.m_StartIndex);
                    DrawArgsBuffer.PushData(atom.m_BaseVertexIndex);
                    DrawArgsBuffer.PushData(0);
                }
            }

            using (var tsCmd = new NxRHI.FTransientCmd(NxRHI.EQueueType.QU_Default, "TtInstanceModifier.FlushArgsBuffer"))
            {
                DrawArgsBuffer.Flush2GPU(tsCmd.CmdList);
                DrawArgsBuffer.GpuBuffer.TransitionTo(tsCmd.CmdList, NxRHI.EGpuResourceState.GRS_UavIndirect);
            }
        }
        public void Dispose()
        {
            CoreSDK.DisposeObject(ref InstanceBuffers);
        }

        public void SetMode(bool bSSBO = true)
        {
            Dispose();

            InstanceBuffers = new TtInstanceBufferSSBO();
        }
        public void SetCapacity(uint nSize)
        {
            InstanceBuffers.SetCapacity(this, nSize);
        }

        public bool IsGpuCulling { get; set; } = true;

        public uint PushInstance(in FVSInstanceData instance)
        {
            return InstanceBuffers.PushInstance(this, in instance);
        }
        public unsafe void SetInstance(uint index, in FVSInstanceData instance)
        {
            InstanceBuffers.SetInstance(index, instance);
        }

        public unsafe void Flush2GPU(NxRHI.ICommandList cmd)
        {
            InstanceBuffers.Flush2GPU(cmd, this);
        }
        public unsafe void OnDrawCall(NxRHI.ICommandList cmd, Pipeline.URenderPolicy.EShadingType shadingType, NxRHI.UGraphicDraw drawcall, Pipeline.URenderPolicy policy, TtMesh.TtAtom atom)
        {
            InstanceBuffers.OnDrawCall(this, cmd, shadingType, drawcall, policy, atom);
        }
    }
}
