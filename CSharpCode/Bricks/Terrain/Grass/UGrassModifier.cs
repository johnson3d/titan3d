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
        public int GrassDataPad0;
        public int GrassDataPad1;
    }

    public class UGrassModifier : IDisposable
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
        public class UInstantVBs : IDisposable
        {
            public UInt32[] mData = null;
            public float[] mHeight = null;

            public NxRHI.UVbView mDataVB;
            public NxRHI.UVbView mHeightVB;

            public NxRHI.UVertexArray mAttachVBs = new NxRHI.UVertexArray();
            public void Dispose()
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
                Dispose();
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
            public unsafe void Flush2VB(UGrassModifier mdf)
            {
                if (mdf.mCurNumber == 0)
                    return;

                var rc = UEngine.Instance.GfxDevice.RenderContext;
                fixed(UInt32* p = &mData[0])
                {
                    var dataSize = (UInt32)sizeof(UInt32) * mdf.mCurNumber;
                    mDataVB.UpdateGpuData(0, p, dataSize);
                }
                fixed(float* p = &mHeight[0])
                {
                    var dataSize = (UInt32)sizeof(float) * mdf.mCurNumber;
                    mHeightVB.UpdateGpuData(0, p, dataSize);
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
        public class UInstantSSBO : IDisposable
        {
            public FVSGrassData[] InstData;
            public NxRHI.UBuffer InstantBuffer;
            public NxRHI.USrView InstantSRV;
            public bool IsDirty { get; private set; } = true;
            public void Dispose()
            {
                InstantBuffer?.Dispose();
                InstantBuffer = null;

                InstantSRV?.Dispose();
                InstantSRV = null;

                InstData = null;
            }
            public unsafe void SureBuffers(UGrassModifier mdf, uint nSize)
            {
                if (mdf.mMaxNumber >= nSize)
                    return;

                var oldData = InstData;
                Dispose();
                mdf.mMaxNumber = nSize;
                InstData = new FVSGrassData[mdf.mMaxNumber];

                var bfDesc = new NxRHI.FBufferDesc();
                bfDesc.SetDefault(false);
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
                srvDesc.SetBuffer(false);
                srvDesc.Buffer.NumElements = mdf.mMaxNumber;
                srvDesc.Buffer.StructureByteStride = bfDesc.StructureStride;
                InstantSRV = UEngine.Instance.GfxDevice.RenderContext.CreateSRV(InstantBuffer, in srvDesc);
                InstantSRV.SetDebugName("InstantSRV");
                InstantSRV.SetDebugName("InstantSRV");
            }
            public uint PushInstance(UGrassModifier mdf, uint data, float height)
            {
                //uint growSize = 1;
                //if(mdf.mMaxNumber > 10)
                //{
                //    growSize += mdf.mMaxNumber;
                //}
                SureBuffers(mdf, mdf.mCurNumber + 1);
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
            public unsafe uint Flush2VB(UGrassModifier mdf)
            {
                if (mdf.mCurNumber == 0)
                    return 0;

                if(IsDirty)
                {
                    var rc = UEngine.Instance.GfxDevice.RenderContext;
                    fixed(FVSGrassData* pTar = &InstData[0])
                    {
                        InstantBuffer.UpdateGpuData(0, pTar, mdf.mCurNumber * (uint)sizeof(FVSGrassData));
                    }
                    IsDirty = false;
                }
                return mdf.mCurNumber;
            }
        }
        public UInstantSSBO InstantSSBO;

        ~UGrassModifier()
        {
            Dispose();
        }

        public void Dispose()
        {
            InstantVBs?.Dispose();
            InstantVBs = null;

            InstantSSBO?.Dispose();
            InstantSSBO = null;
        }

        public void SetMode(bool bSSBO = true)
        {
            Dispose();
            if (bSSBO)
                InstantSSBO = new UInstantSSBO();
            else
                InstantVBs = new UInstantVBs();

            if (GrassTemp == null)
            {
                GrassTemp = new Graphics.Pipeline.Common.TtCpu2GpuBuffer<FVSGrassData>();
                GrassTemp.Initialize(false);
                GrassTemp.PushData(new FVSGrassData());
                GrassTemp.Flush2GPU();
            }
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
        public unsafe uint Flush2VB()
        {
            if (InstantSSBO != null)
                return InstantSSBO.Flush2VB(this);
            else if (InstantVBs != null)
                InstantVBs.Flush2VB(this);
            return 0;
        }
        private void SureCBuffer(NxRHI.IGraphicsEffect shaderProg)
        {
            var coreBinder = UEngine.Instance.GfxDevice.CoreShaderBinder;
            if(GrassType.GrassCBuffer == null)
            {
                coreBinder.CBPerGrassType.UpdateFieldVar(shaderProg, "cbPerGrassType");
                if(coreBinder.CBPerGrassType.Binder != null)
                    GrassType.GrassCBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateCBV(coreBinder.CBPerGrassType.Binder.mCoreObject);
            }
        }
        public static Graphics.Pipeline.Common.TtCpu2GpuBuffer<FVSGrassData> GrassTemp = null;
        public class UMdfShaderBinder : Graphics.Pipeline.UCoreShaderBinder.UShaderResourceIndexer
        {
            public void Init(NxRHI.UShaderEffect effect)
            {
                UpdateBindResouce(effect);
                HeightMapTexture = effect.FindBinder("HeightMapTexture");
                Samp_HeightMapTexture = effect.FindBinder("Samp_HeightMapTexture");
                cbPerPatch = effect.FindBinder("cbPerPatch");
                cbPerTerrain = effect.FindBinder("cbPerTerrain");
                cbPerGrassType = effect.FindBinder("cbPerGrassType");
                VSGrassDataArray = effect.FindBinder("VSGrassDataArray");
            }
            public NxRHI.UEffectBinder HeightMapTexture;
            public NxRHI.UEffectBinder Samp_HeightMapTexture;
            public NxRHI.UEffectBinder cbPerPatch;
            public NxRHI.UEffectBinder cbPerTerrain;
            public NxRHI.UEffectBinder cbPerGrassType;
            public NxRHI.UEffectBinder VSGrassDataArray;
        }
        public unsafe void OnDrawCall(Graphics.Pipeline.URenderPolicy.EShadingType shadingType, NxRHI.UGraphicDraw drawcall, Graphics.Pipeline.URenderPolicy policy, Graphics.Mesh.UMesh mesh)
        {
            var pat = GrassType.Patch;
            if ((pat.Level.Level.Node.TerrainCBuffer == null) || (pat.PatchCBuffer == null))
                return;

            SureCBuffer(drawcall.mCoreObject.GetGraphicsEffect());

            var effectBinder = drawcall.Effect.mBindIndexer as UMdfShaderBinder;
            if (effectBinder == null)
            {
                effectBinder = new UMdfShaderBinder();
                effectBinder.Init(drawcall.Effect.ShaderEffect);
                drawcall.Effect.mBindIndexer = effectBinder;
            }

            //var index = drawcall.FindBinder("HeightMapTexture");
            if (effectBinder.HeightMapTexture != null)
            {
                drawcall.BindSRV(effectBinder.HeightMapTexture, pat.Level.HeightMapSRV);
                //drawcall.BindSRV(effectBinder.HeightMapTexture, UEngine.Instance.GfxDevice.TextureManager.DefaultTexture);
            }
            //index = drawcall.FindBinder("Samp_HeightMapTexture");
            if (effectBinder.Samp_HeightMapTexture != null)
                drawcall.BindSampler(effectBinder.Samp_HeightMapTexture.mCoreObject, policy.ClampState);

            //index = drawcall.FindBinder("cbPerTerrain");
            if (effectBinder.cbPerTerrain != null)
                drawcall.BindCBuffer(effectBinder.cbPerTerrain.mCoreObject, pat.Level.Level.Node.TerrainCBuffer);
            //var cbIndex = drawcall.FindBinder("cbPerPatch");
            if (effectBinder.cbPerPatch != null)
            {
                var coreBinder = UEngine.Instance.GfxDevice.CoreShaderBinder;
                pat.PatchCBuffer.SetValue(coreBinder.CBPerTerrainPatch.StartPosition, in pat.StartPosition);

                pat.PatchCBuffer.SetValue(coreBinder.CBPerTerrainPatch.CurrentLOD, pat.CurrentLOD);

                var terrain = pat.Level.GetTerrainNode();
                pat.PatchCBuffer.SetValue(coreBinder.CBPerTerrainPatch.EyeCenter, terrain.EyeLocalCenter - pat.StartPosition);

                pat.TexUVOffset.X = ((float)pat.XInLevel / (float)pat.Level.GetTerrainNode().PatchSide);
                pat.TexUVOffset.Y = ((float)pat.ZInLevel / (float)pat.Level.GetTerrainNode().PatchSide);

                pat.PatchCBuffer.SetValue(coreBinder.CBPerTerrainPatch.TexUVOffset, in pat.TexUVOffset);

                pat.PatchCBuffer.FlushDirty();
                drawcall.BindCBuffer(effectBinder.cbPerPatch.mCoreObject, pat.PatchCBuffer);
            }

            uint instCount = 0;
            if (InstantSSBO != null)
            {
                //var binder = drawcall.FindBinder("VSGrassDataArray");
                if (effectBinder.VSGrassDataArray != null)
                {
                    instCount = this.Flush2VB();
                    drawcall.BindSRV(effectBinder.VSGrassDataArray, InstantSSBO.InstantSRV);
                    //drawcall.BindSRV(effectBinder.VSGrassDataArray, GrassTemp.DataSRV); 
                }
            }
            else if (InstantVBs != null)
            {
                drawcall.BindAttachVertexArray(InstantVBs.mAttachVBs);
            }

            //System.Diagnostics.Debug.Assert(this.CurNumber == instCount);
            drawcall.mCoreObject.DrawInstance = (ushort)instCount;
            //index = drawcall.FindBinder("cbPerGrassType");
            if (effectBinder.cbPerGrassType != null)
            {
                var coreBinder = UEngine.Instance.GfxDevice.CoreShaderBinder;
                GrassType.GrassCBuffer.SetValue(coreBinder.CBPerGrassType.MinScale, GrassType.GrassDesc.MinScale);
                GrassType.GrassCBuffer.SetValue(coreBinder.CBPerGrassType.MaxScale, GrassType.GrassDesc.MaxScale);
                GrassType.GrassCBuffer.SetValue(coreBinder.CBPerGrassType.HeightMapMinHeight, in pat.Level.HeightMapMinHeight);
                //GrassType.GrassCBuffer.SetValue(coreBinder.CBPerGrassType.HeightMapMinHeight, (int)1);
                GrassType.GrassCBuffer.SetValue(coreBinder.CBPerGrassType.PatchIdxX, pat.IndexX);
                GrassType.GrassCBuffer.SetValue(coreBinder.CBPerGrassType.PatchIdxZ, pat.IndexZ);
                GrassType.GrassCBuffer.SetValue(coreBinder.CBPerGrassType.MaxGrassInstanceNum, instCount);
                GrassType.GrassCBuffer.FlushDirty();
                drawcall.BindCBuffer(effectBinder.cbPerGrassType, GrassType.GrassCBuffer);
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
            [NxRHI.UShader.UShaderVar(VarType = typeof(int))]
            public NxRHI.FShaderVarDesc MaxGrassInstanceNum;
        }
        public readonly UCBufferPerGrassTypeIndexer CBPerGrassType = new UCBufferPerGrassTypeIndexer();
    }
}