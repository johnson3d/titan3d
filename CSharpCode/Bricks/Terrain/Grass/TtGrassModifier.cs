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

    public class TtGrassModifier : Graphics.Pipeline.Shader.IMeshModifier
    {
        public TtGrassModifier()
        {
            SetMode(true);
        }
        public void Dispose()
        {
            InstantVBs?.Dispose();
            InstantVBs = null;

            InstantSSBO?.Dispose();
            InstantSSBO = null;
        }

        public string ModifierNameVS { get => "DoGrassModifierVS"; }
        public string ModifierNamePS { get => null; }
        public RName SourceName
        {
            get
            {
                return RName.GetRName("shaders/Bricks/Terrain/GrassModifier.cginc", RName.ERNameType.Engine);
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
            return new NxRHI.EVertexStreamType[]
            {
                NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_Normal,
                NxRHI.EVertexStreamType.VST_Color,
            };
        }
        public Graphics.Pipeline.Shader.EPixelShaderInput[] GetPSNeedInputs()
        {
            return null;
        }
        public void Initialize(Graphics.Mesh.TtMaterialMesh materialMesh)
        {

        }
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

            public NxRHI.UVertexArray mAttachVBs = null;
            public void Dispose()
            {
                mDataVB?.Dispose();
                mDataVB = null;
                mData = null;
            }
            public unsafe void SureBuffers(TtGrassModifier mdf, uint nSize)
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

                var rc = TtEngine.Instance.GfxDevice.RenderContext;

                if (mAttachVBs == null)
                    mAttachVBs = rc.CreateVertexArray();
                var desc = new NxRHI.FVbvDesc();
                desc.m_Size = (UInt32)(sizeof(UInt32) * mdf.mMaxNumber);
                desc.m_Stride = (UInt32)sizeof(UInt32);
                mDataVB = rc.CreateVBV(null, in desc);

                desc.m_Size = (UInt32)(sizeof(float) * mdf.mMaxNumber);
                desc.m_Stride = (UInt32)sizeof(float);
                mHeightVB = rc.CreateVBV(null, in desc);
            }
            public unsafe void Flush2VB(NxRHI.ICommandList cmd, TtGrassModifier mdf)
            {
                if (mdf.mCurNumber == 0)
                    return;

                var rc = TtEngine.Instance.GfxDevice.RenderContext;
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
            public uint PushInstance(TtGrassModifier mdf, UInt32 data, float height)
            {
                var rc = TtEngine.Instance.GfxDevice.RenderContext;
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
            public NxRHI.TtSrView InstantSRV;
            public bool IsDirty { get; private set; } = true;
            public void Dispose()
            {
                CoreSDK.DisposeObject(ref InstantBuffer);
                CoreSDK.DisposeObject(ref InstantSRV);

                InstData = null;
            }
            public unsafe void SureBuffers(TtGrassModifier mdf, uint nSize)
            {
                if (mdf.mMaxNumber >= nSize)
                    return;

                var oldData = InstData;
                Dispose();
                mdf.mMaxNumber = nSize;
                InstData = new FVSGrassData[mdf.mMaxNumber];

                var bfDesc = new NxRHI.FBufferDesc();
                bfDesc.SetDefault(false, NxRHI.EBufferType.BFT_SRV);
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
                        InstantBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateBuffer(in bfDesc);
                    }
                }
                else
                {
                    InstantBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateBuffer(in bfDesc);
                }
                InstantBuffer.SetDebugName("GrassInst");
                
                var srvDesc = new NxRHI.FSrvDesc();
                srvDesc.SetBuffer(false);
                srvDesc.Buffer.NumElements = mdf.mMaxNumber;
                srvDesc.Buffer.StructureByteStride = bfDesc.StructureStride;
                InstantSRV = TtEngine.Instance.GfxDevice.RenderContext.CreateSRV(InstantBuffer, in srvDesc);
                InstantSRV.SetDebugName("InstantSRV");

            }
            public uint PushInstance(TtGrassModifier mdf, uint data, float height)
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
            public unsafe uint Flush2VB(NxRHI.ICommandList cmd, TtGrassModifier mdf)
            {
                if (mdf.mCurNumber == 0)
                    return 0;

                if(IsDirty)
                {
                    var rc = TtEngine.Instance.GfxDevice.RenderContext;
                    fixed(FVSGrassData* pTar = &InstData[0])
                    {
                        InstantBuffer.UpdateGpuData(cmd, 0, pTar, mdf.mCurNumber * (uint)sizeof(FVSGrassData));
                    }
                    IsDirty = false;
                }
                return mdf.mCurNumber;
            }
        }
        public UInstantSSBO InstantSSBO;

        ~TtGrassModifier()
        {
            Dispose();
        }

        public void SetMode(bool bSSBO = true)
        {
            Dispose();
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
        public unsafe uint Flush2VB(NxRHI.ICommandList cmd)
        {
            if (InstantSSBO != null)
                return InstantSSBO.Flush2VB(cmd, this);
            else if (InstantVBs != null)
                InstantVBs.Flush2VB(cmd, this);
            return 0;
        }
        private void SureCBuffer(NxRHI.IGraphicsEffect shaderProg)
        {
            var coreBinder = TtEngine.Instance.GfxDevice.CoreShaderBinder;
            if(GrassType.GrassCBuffer == null)
            {
                coreBinder.CBPerGrassType.UpdateFieldVar(shaderProg, "cbPerGrassType");
                if(coreBinder.CBPerGrassType.Binder != null)
                    GrassType.GrassCBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(coreBinder.CBPerGrassType.Binder.mCoreObject);
            }
        }
        public class UMdfShaderBinder : Graphics.Pipeline.UCoreShaderBinder.UShaderResourceIndexer
        {
            public void Init(NxRHI.TtShaderEffect effect)
            {
                UpdateBindResouce(effect);
                HeightMapTexture = effect.FindBinder("HeightMapTexture");
                Samp_HeightMapTexture = effect.FindBinder("Samp_HeightMapTexture");
                cbPerPatch = effect.FindBinder("cbPerPatch");
                cbPerTerrain = effect.FindBinder("cbPerTerrain");
                cbPerGrassType = effect.FindBinder("cbPerGrassType");
                VSGrassDataArray = effect.FindBinder("VSGrassDataArray");
            }
            public NxRHI.TtEffectBinder HeightMapTexture;
            public NxRHI.TtEffectBinder Samp_HeightMapTexture;
            public NxRHI.TtEffectBinder cbPerPatch;
            public NxRHI.TtEffectBinder cbPerTerrain;
            public NxRHI.TtEffectBinder cbPerGrassType;
            public NxRHI.TtEffectBinder VSGrassDataArray;
        }
        public unsafe void OnDrawCall(Graphics.Pipeline.Shader.TtMdfQueueBase mdfQueue1, NxRHI.ICommandList cmd, NxRHI.UGraphicDraw drawcall, Graphics.Pipeline.TtRenderPolicy policy, Graphics.Mesh.TtMesh.TtAtom atom)
        {
            var pat = GrassType.Patch;
            pat.SureCBuffer(drawcall.mCoreObject.GetGraphicsEffect(), ref pat.PatchCBuffer);

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
                //drawcall.BindSRV(effectBinder.HeightMapTexture, TtEngine.Instance.GfxDevice.TextureManager.DefaultTexture);
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
                var coreBinder = TtEngine.Instance.GfxDevice.CoreShaderBinder;
                pat.PatchCBuffer.SetValue(coreBinder.CBPerTerrainPatch.StartPosition, in pat.StartPosition);

                pat.PatchCBuffer.SetValue(coreBinder.CBPerTerrainPatch.CurrentLOD, pat.CurrentLOD);

                pat.TexUVOffset.X = ((float)pat.XInLevel / (float)pat.Level.GetTerrainNode().PatchSide);
                pat.TexUVOffset.Y = ((float)pat.ZInLevel / (float)pat.Level.GetTerrainNode().PatchSide);

                pat.PatchCBuffer.SetValue(coreBinder.CBPerTerrainPatch.TexUVOffset, in pat.TexUVOffset);

                drawcall.BindCBuffer(effectBinder.cbPerPatch.mCoreObject, pat.PatchCBuffer);
            }

            uint instCount = 0;
            if (InstantSSBO != null)
            {
                //var binder = drawcall.FindBinder("VSGrassDataArray");
                if (effectBinder.VSGrassDataArray != null)
                {
                    instCount = this.Flush2VB(cmd);
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
                var coreBinder = TtEngine.Instance.GfxDevice.CoreShaderBinder;
                GrassType.GrassCBuffer.SetValue(coreBinder.CBPerGrassType.MinScale, GrassType.GrassDesc.MinScale);
                GrassType.GrassCBuffer.SetValue(coreBinder.CBPerGrassType.MaxScale, GrassType.GrassDesc.MaxScale);
                GrassType.GrassCBuffer.SetValue(coreBinder.CBPerGrassType.HeightMapMinHeight, in pat.Level.HeightMapMinHeight);
                //GrassType.GrassCBuffer.SetValue(coreBinder.CBPerGrassType.HeightMapMinHeight, (int)1);
                GrassType.GrassCBuffer.SetValue(coreBinder.CBPerGrassType.PatchIdxX, pat.IndexX);
                GrassType.GrassCBuffer.SetValue(coreBinder.CBPerGrassType.PatchIdxZ, pat.IndexZ);
                GrassType.GrassCBuffer.SetValue(coreBinder.CBPerGrassType.MaxGrassInstanceNum, instCount);
                drawcall.BindCBuffer(effectBinder.cbPerGrassType, GrassType.GrassCBuffer);
            }
        }
    }
}

namespace EngineNS.Graphics.Pipeline
{
    public partial class UCoreShaderBinder
    {
        public class UCBufferPerGrassTypeIndexer : NxRHI.TtShader.UShaderVarIndexer
        {
            [NxRHI.TtShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc MinScale;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc MaxScale;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc HeightMapMinHeight;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc PatchIdxX;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc PatchIdxZ;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(int))]
            public NxRHI.FShaderVarDesc MaxGrassInstanceNum;
        }
        public readonly UCBufferPerGrassTypeIndexer CBPerGrassType = new UCBufferPerGrassTypeIndexer();
    }
}