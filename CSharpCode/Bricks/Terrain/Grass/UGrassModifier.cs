using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Terrain.Grass
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
    public struct FVSGrassData
    {
        // test only ///////////////////
        //public Vector3 GrassPosition;
        //public float GrassScale;
        //public Quaternion GrassQuat;
        ////////////////////////////////
        // position(12b*2), rot(4b), scale(4b)
        public UInt32 Data;
        public float TerrainHeight;
    }

    public class UGrassModifier
    {
        public CDLOD.UTerrainGrassManager.UGrassType GrassType;
        uint mCurNumber = 0;
        public uint CurNumber => mCurNumber;
        public void ResetCurNumber()
        {
            mCurNumber = 0;
        }
        uint mMaxNumber = 0;
        public uint MaxNumber => mMaxNumber;
        public class UInstantVBs
        {
            public UInt32[] mData = null;
            public float[] mHeight = null;

            public NxRHI.UVbView mDataVB;
            public NxRHI.UVbView mHeightVB;

            public NxRHI.UVertexArray mAttachVBs = new NxRHI.UVertexArray();
            public void Cleanup()
            {
                mDataVB?.Dispose();
                mDataVB = null;
                mData = null;
            }
            public unsafe void SureBuffers(UGrassModifier mdf, uint nSize)
            {
                if (mdf.mMaxNumber > nSize)
                    return;

                var oldData = mData;
                var oldHeight = mHeight;
                Cleanup();
                mdf.mMaxNumber = nSize * 2;
                mData = new UInt32[mdf.mMaxNumber];
                mHeight = new float[mdf.mMaxNumber];

                if(mdf.mCurNumber > 0)
                {
                    fixed(UInt32* pSrc = &oldData[0])
                    fixed(UInt32* pTar = &mData[0])
                    {
                        CoreSDK.MemoryCmp(pTar, pSrc, mdf.mCurNumber * (uint)sizeof(Int32));
                    }

                    fixed(float* pSrc = &oldHeight[0])
                    fixed(float* pTar = &mHeight[0])
                    {
                        CoreSDK.MemoryCmp(pTar, pSrc, mdf.mCurNumber * (uint)sizeof(float));
                    }
                }

                var rc = UEngine.Instance.GfxDevice.RenderContext;
                var desc = new NxRHI.FVbvDesc();
                desc.m_Size = (UInt32)(sizeof(UInt32) * mdf.mMaxNumber);
                desc.m_Stride = (UInt32)sizeof(UInt32);
                mDataVB = rc.CreateVBV(null, in desc);

                desc.m_Size = (UInt32)(sizeof(float) * mdf.mMaxNumber);
                desc.m_Stride = (UInt32)sizeof(float);
                mHeightVB = rc.CreateVBV(null, in desc);
            }
            public unsafe void Flush2VB(NxRHI.ICommandList cmd, UGrassModifier mdf)
            {
                if (mdf.mCurNumber == 0)
                    return;

                var rc = UEngine.Instance.GfxDevice.RenderContext;
                fixed(UInt32* p = &mData[0])
                {
                    var dataSize = (UInt32)sizeof(UInt32) * mdf.mCurNumber;
                    mDataVB.UpdateGpuData(cmd, 0, p, dataSize);
                }
                fixed(float* p = &mHeight[0])
                {
                    var dataSize = (UInt32)sizeof(float) * mdf.mCurNumber;
                    mHeightVB.UpdateGpuData(cmd, 0, p, dataSize);
                }

                mAttachVBs.BindVB(NxRHI.EVertexStreamType.VST_Color, mDataVB);
                //mAttachVBs.BindVB(NxRHI.EVertexStreamType.VST_TerrainGradient)
            }
            public uint PushInstance(UGrassModifier mdf, UInt32 data, float height)
            {
                var rc = UEngine.Instance.GfxDevice.RenderContext;
                SureBuffers(mdf, mdf.mCurNumber + 1);

                mData[mdf.mCurNumber] = data;
                mHeight[mdf.mCurNumber] = height;

                var result = mdf.mCurNumber;
                mdf.mCurNumber++;
                return result;
            }
            public unsafe void SetInstance(uint index, UInt32* data, float* height)
            {
                if (data != IntPtr.Zero.ToPointer())
                    mData[index] = *data;
                if (height != IntPtr.Zero.ToPointer())
                    mHeight[index] = *height;
            }
        }
        public UInstantVBs InstantVBs;
        public class UInstantSSBO
        {
            public FVSGrassData[] InstData;
            public NxRHI.UBuffer InstantBuffer;
            public NxRHI.USrView InstantSRV;
            public bool IsDirty { get; private set; } = true;
            public void Cleanup()
            {
                InstantSRV?.Dispose();
                InstantSRV = null;

                InstantBuffer?.Dispose();
                InstantBuffer = null;

                InstData = null;
            }
            public unsafe void SureBuffers(UGrassModifier mdf, uint nSize)
            {
                if (mdf.mMaxNumber >= nSize)
                    return;

                var oldData = InstData;
                Cleanup();
                mdf.mMaxNumber = nSize;
                InstData = new FVSGrassData[mdf.mMaxNumber];

                var bfDesc = new NxRHI.FBufferDesc();
                bfDesc.SetDefault();
                bfDesc.Type = NxRHI.EBufferType.BFT_SRV;
                bfDesc.CpuAccess = NxRHI.ECpuAccess.CAS_WRITE;
                bfDesc.Usage = NxRHI.EGpuUsage.USAGE_DYNAMIC;
                bfDesc.StructureStride = (uint)sizeof(FVSGrassData);
                bfDesc.Size = (uint)sizeof(FVSGrassData) * mdf.mMaxNumber;

                if(mdf.mCurNumber > 0)
                {
                    fixed(FVSGrassData* pSrc = &oldData[0])
                    fixed(FVSGrassData* pTar = &InstData[0])
                    {
                        CoreSDK.MemoryCopy(pTar, pSrc, mdf.mCurNumber * (uint)sizeof(FVSGrassData));
                        bfDesc.InitData = pTar;
                        InstantBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateBuffer(in bfDesc);
                    }
                }
                else
                {
                    InstantBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateBuffer(in bfDesc);
                }

                var srvDesc = new NxRHI.FSrvDesc();
                srvDesc.SetBuffer(0);
                srvDesc.Buffer.NumElements = mdf.mMaxNumber;
                srvDesc.Buffer.StructureByteStride = bfDesc.StructureStride;
                InstantSRV = UEngine.Instance.GfxDevice.RenderContext.CreateSRV(InstantBuffer, in srvDesc);
            }
            public uint PushInstance(UGrassModifier mdf, uint data, float height)
            {
                var rc = UEngine.Instance.GfxDevice.RenderContext;

                //uint growSize = 1;
                //if(mdf.mMaxNumber > 10)
                //{
                //    growSize += mdf.mMaxNumber;
                //}
                //SureBuffers(mdf, mdf.mCurNumber + growSize);
                System.Diagnostics.Debug.Assert(mdf.CurNumber < mdf.mMaxNumber);

                InstData[mdf.mCurNumber].Data = data;
                InstData[mdf.mCurNumber].TerrainHeight = height;
                /////////////////////////////////////////////////
                //InstData[mdf.mCurNumber].GrassPosition = pos;
                //InstData[mdf.mCurNumber].GrassScale = scale;
                //InstData[mdf.mCurNumber].GrassQuat = quat;
                /////////////////////////////////////////////////

                var result = mdf.mCurNumber;
                mdf.mCurNumber++;

                IsDirty = true;
                return result;
            }
            public unsafe void SetInstance(uint index, uint* data, float* height)
            {
                if (data != IntPtr.Zero.ToPointer())
                    InstData[index].Data = *data;
                if(height != IntPtr.Zero.ToPointer())
                    InstData[index].TerrainHeight = *height;

                IsDirty = true;
            }
            public unsafe void Flush2VB(NxRHI.ICommandList cmd, UGrassModifier mdf)
            {
                if (mdf.mCurNumber == 0)
                    return;

                if(IsDirty)
                {
                    var rc = UEngine.Instance.GfxDevice.RenderContext;
                    fixed(FVSGrassData* pTar = &InstData[0])
                    {
                        InstantBuffer.UpdateGpuData(cmd, 0, pTar, (NxRHI.FSubresourceBox*)IntPtr.Zero.ToPointer(), mdf.mCurNumber * (uint)sizeof(FVSGrassData), 1);
                    }
                    IsDirty = false;
                }
            }
        }
        public UInstantSSBO InstantSSBO;

        ~UGrassModifier()
        {
            Cleanup();
        }

        public void Cleanup()
        {
            InstantVBs?.Cleanup();
            InstantVBs = null;

            InstantSSBO?.Cleanup();
            InstantSSBO = null;
        }

        public void SetMode(bool bSSBO = true)
        {
            Cleanup();
            if (bSSBO)
                InstantSSBO = new UInstantSSBO();
            else
                InstantVBs = new UInstantVBs();
        }
        public void SureBuffers(uint nSize)
        {
            if (InstantSSBO != null)
                InstantSSBO.SureBuffers(this, nSize);
            else if (InstantVBs != null)
                InstantVBs.SureBuffers(this, nSize);
        }
        public uint PushInstance(UInt32 data, float height)
        {
            if(InstantSSBO != null)
                return InstantSSBO.PushInstance(this, data, height);
            else if(InstantVBs != null)
                return InstantVBs.PushInstance(this, data, height);
            return uint.MaxValue;
        }
        public unsafe void SetInstance(uint index, UInt32* data, float* height)
        {
            if (InstantSSBO != null)
                InstantSSBO.SetInstance(index, data, height);
            else if(InstantVBs != null)
                InstantVBs.SetInstance(index, data, height);
        }
        public unsafe void Flush2VB(NxRHI.ICommandList cmd)
        {
            if (InstantSSBO != null)
                InstantSSBO.Flush2VB(cmd, this);
            else if (InstantVBs != null)
                InstantVBs.Flush2VB(cmd, this);
        }
        private void SureCBuffer(NxRHI.IShaderEffect shaderProg)
        {
            var coreBinder = UEngine.Instance.GfxDevice.CoreShaderBinder;
            if(GrassType.GrassCBuffer == null)
            {
                coreBinder.CBPerGrassType.UpdateFieldVar(shaderProg, "cbPerGrassType");
                if(coreBinder.CBPerGrassType.Binder != null)
                    GrassType.GrassCBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateCBV(coreBinder.CBPerGrassType.Binder.mCoreObject);
            }
        }
        public unsafe void OnDrawCall(Graphics.Pipeline.URenderPolicy.EShadingType shadingType, NxRHI.UGraphicDraw drawcall, Graphics.Pipeline.URenderPolicy policy, Graphics.Mesh.UMesh mesh)
        {
            var pat = GrassType.Patch;
            if ((pat.Level.Level.Node.TerrainCBuffer == null) || (pat.PatchCBuffer == null))
                return;

            SureCBuffer(drawcall.mCoreObject.GetShaderEffect());

            drawcall.mCoreObject.DrawInstance = (ushort)this.CurNumber;

            var index = drawcall.FindBinder("HeightMapTexture");
            if (index.IsValidPointer)
                drawcall.BindSRV(index, pat.Level.HeightMapSRV);
            index = drawcall.FindBinder("Samp_HeightMapTexture");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, policy.ClampState);

            index = drawcall.FindBinder("cbPerTerrain");
            if(index.IsValidPointer)
                drawcall.BindCBuffer(index, pat.Level.Level.Node.TerrainCBuffer);
            var cbIndex = drawcall.FindBinder("cbPerPatch");
            if(cbIndex.IsValidPointer)
            {
                var coreBinder = UEngine.Instance.GfxDevice.CoreShaderBinder;
                pat.PatchCBuffer.SetValue(coreBinder.CBPerTerrainPatch.StartPosition, in pat.StartPosition);

                pat.PatchCBuffer.SetValue(coreBinder.CBPerTerrainPatch.CurrentLOD, pat.CurrentLOD);

                var terrain = pat.Level.GetTerrainNode();
                pat.PatchCBuffer.SetValue(coreBinder.CBPerTerrainPatch.EyeCenter, terrain.EyeLocalCenter - pat.StartPosition);

                pat.TexUVOffset.X = ((float)pat.XInLevel / (float)pat.Level.GetTerrainNode().PatchSide);
                pat.TexUVOffset.Y = ((float)pat.ZInLevel / (float)pat.Level.GetTerrainNode().PatchSide);

                pat.PatchCBuffer.SetValue(coreBinder.CBPerTerrainPatch.TexUVOffset, in pat.TexUVOffset);

                drawcall.BindCBuffer(cbIndex, pat.PatchCBuffer);
            }

            index = drawcall.FindBinder("cbPerGrassType");
            if(index.IsValidPointer)
            {
                var coreBinder = UEngine.Instance.GfxDevice.CoreShaderBinder;
                GrassType.GrassCBuffer.SetValue(coreBinder.CBPerGrassType.MinScale, GrassType.GrassDesc.MinScale);
                GrassType.GrassCBuffer.SetValue(coreBinder.CBPerGrassType.MaxScale, GrassType.GrassDesc.MaxScale);
                GrassType.GrassCBuffer.SetValue(coreBinder.CBPerGrassType.HeightMapMinHeight, in pat.Level.HeightMapMinHeight);
                GrassType.GrassCBuffer.SetValue(coreBinder.CBPerGrassType.PatchIdxX, pat.IndexX);
                GrassType.GrassCBuffer.SetValue(coreBinder.CBPerGrassType.PatchIdxZ, pat.IndexZ);
                drawcall.BindCBuffer(index, GrassType.GrassCBuffer);
            }

            if(InstantSSBO != null)
            {
                var binder = drawcall.FindBinder("VSGrassDataArray");
                if (binder.IsValidPointer == false)
                    return;
                var cmd = UEngine.Instance.GfxDevice.RenderContext.CmdQueue.GetIdleCmdlist(NxRHI.EQueueCmdlist.QCL_FramePost);
                this.Flush2VB(cmd);
                UEngine.Instance.GfxDevice.RenderContext.CmdQueue.ReleaseIdleCmdlist(cmd, NxRHI.EQueueCmdlist.QCL_FramePost);
                drawcall.BindSRV(binder, InstantSSBO.InstantSRV);
            }
            else if(InstantVBs != null)
            {
                drawcall.BindAttachVertexArray(InstantVBs.mAttachVBs);
            }
        }
    }
}

namespace EngineNS.Graphics.Pipeline
{
    public partial class UCoreShaderBinder
    {
        public class UCBufferPerGrassTypeIndexer : NxRHI.UShader.UShaderVarIndexer
        {
            [NxRHI.UShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc MinScale;
            [NxRHI.UShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc MaxScale;
            [NxRHI.UShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc HeightMapMinHeight;
            [NxRHI.UShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc PatchIdxX;
            [NxRHI.UShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc PatchIdxZ;
        }
        public readonly UCBufferPerGrassTypeIndexer CBPerGrassType = new UCBufferPerGrassTypeIndexer();
    }
}