using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Mesh.Modifier
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
    public struct FVSInstantData
    {
        public Vector3 Position;
        public uint HitProxyId;

        public Vector3 Scale;
        public uint CustomData2;

        public Quaternion Quat;
        public Vector4ui UserData;

        public Vector4ui PointLightIndices;
    };

    public class UInstanceModifier
    {
        uint mCurNumber = 0;
        public uint CurNumber
        {
            get => mCurNumber;
        }
        uint mMaxNumber = 0;
        public uint MaxNumber
        {
            get => mMaxNumber;
        }
        public class UInstantVBs
        {
            public Vector3[] mPosData = null;
            public Vector4[] mScaleData = null;
            public Quaternion[] mRotateData = null;
            public Vector4ui[] mF41Data = null;

            public NxRHI.UVbView mPosVB;
            public NxRHI.UVbView mScaleVB;
            public NxRHI.UVbView mRotateVB;
            public NxRHI.UVbView mF41VB;

            public NxRHI.UVertexArray mAttachVBs = new NxRHI.UVertexArray();
            public void Cleanup()
            {
                mPosVB?.Dispose();
                mPosVB = null;
                mScaleVB?.Dispose();
                mScaleVB = null;
                mRotateVB?.Dispose();
                mRotateVB = null;
                mF41VB?.Dispose();
                mF41VB = null;

                mPosData = null;
                mScaleData = null;
                mRotateData = null;
                mF41Data = null;
            }
            public unsafe void SureBuffers(UInstanceModifier mdf, uint nSize)
            {
                if (mdf.mMaxNumber > nSize)
                {
                    return;
                }

                var oldPos = mPosData;
                var oldScale = mScaleData;
                var oldQuat = mRotateData;
                var oldF41 = mF41Data;

                Cleanup();

                mdf.mMaxNumber = nSize * 2;
                //mInstDataArray = new VSInstantData[mMaxNumber];
                mPosData = new Vector3[mdf.mMaxNumber];
                mScaleData = new Vector4[mdf.mMaxNumber];
                mRotateData = new Quaternion[mdf.mMaxNumber];
                mF41Data = new Vector4ui[mdf.mMaxNumber];

                if (mdf.mCurNumber > 0)
                {
                    fixed (Vector3* pSrc = &oldPos[0])
                    fixed (Vector3* pTar = &mPosData[0])
                    {
                        CoreSDK.MemoryCopy(pTar, pSrc, mdf.mCurNumber * (uint)sizeof(Vector3));
                    }

                    fixed (Vector4* pSrc = &oldScale[0])
                    fixed (Vector4* pTar = &mScaleData[0])
                    {
                        CoreSDK.MemoryCopy(pTar, pSrc, mdf.mCurNumber * (uint)sizeof(Vector4));
                    }

                    fixed (Quaternion* pSrc = &oldQuat[0])
                    fixed (Quaternion* pTar = &mRotateData[0])
                    {
                        CoreSDK.MemoryCopy(pTar, pSrc, mdf.mCurNumber * (uint)sizeof(Quaternion));
                    }

                    fixed (Vector4ui* pSrc = &oldF41[0])
                    fixed (Vector4ui* pTar = &mF41Data[0])
                    {
                        CoreSDK.MemoryCopy(pTar, pSrc, mdf.mCurNumber * (uint)sizeof(Vector4ui));
                    }
                }


                var rc = UEngine.Instance.GfxDevice.RenderContext;
                var desc = new NxRHI.FVbvDesc();
                //desc.CpuAccess = NxRHI.ECpuAccess.CAS_WRITE;
                desc.m_Size = (UInt32)(sizeof(Vector3) * mdf.mMaxNumber);
                desc.m_Stride = (UInt32)sizeof(Vector3);
                mPosVB = rc.CreateVBV(null, in desc);

                desc.m_Size = (UInt32)(sizeof(Vector4) * mdf.mMaxNumber);
                desc.m_Stride = (UInt32)sizeof(Vector4);
                mScaleVB = rc.CreateVBV(null, in desc);

                desc.m_Size = (UInt32)(sizeof(Quaternion) * mdf.mMaxNumber);
                desc.m_Stride = (UInt32)sizeof(Quaternion);
                mRotateVB = rc.CreateVBV(null, in desc);

                desc.m_Size = (UInt32)(sizeof(Vector4) * mdf.mMaxNumber);
                desc.m_Stride = (UInt32)sizeof(Vector4);
                mF41VB = rc.CreateVBV(null, in desc);
            }

            public unsafe void Flush2VB(UInstanceModifier mdf)
            {
                if (mdf.mCurNumber == 0)
                    return;

                var rc = UEngine.Instance.GfxDevice.RenderContext;
                fixed (Vector3* p = &mPosData[0])
                {
                    var dataSize = (UInt32)sizeof(Vector3) * mdf.mCurNumber;
                    mPosVB.UpdateGpuData(0, p, dataSize);
                }
                fixed (Vector4* p = &mScaleData[0])
                {
                    var dataSize = (UInt32)sizeof(Vector4) * mdf.mCurNumber;
                    mScaleVB.UpdateGpuData(0, p, dataSize);
                }
                fixed (Quaternion* p = &mRotateData[0])
                {
                    var dataSize = (UInt32)sizeof(Quaternion) * mdf.mCurNumber;
                    mRotateVB.UpdateGpuData(0, p, dataSize);
                }
                fixed (Vector4ui* p = &mF41Data[0])
                {
                    var dataSize = (UInt32)sizeof(Vector4ui) * mdf.mCurNumber;
                    mF41VB.UpdateGpuData(0, p, dataSize);
                }

                mAttachVBs.BindVB(NxRHI.EVertexStreamType.VST_InstPos, mPosVB);
                mAttachVBs.BindVB(NxRHI.EVertexStreamType.VST_InstQuat, mRotateVB);
                mAttachVBs.BindVB(NxRHI.EVertexStreamType.VST_InstScale, mScaleVB);
                mAttachVBs.BindVB(NxRHI.EVertexStreamType.VST_F4_1, mF41VB);
            }

            public uint PushInstance(UInstanceModifier mdf, in Vector3 pos, in Vector3 scale, in Quaternion quat, in Vector4ui f41, uint hitProxyId)
            {
                var rc = UEngine.Instance.GfxDevice.RenderContext;
                SureBuffers(mdf, mdf.mCurNumber + 1);

                mPosData[mdf.mCurNumber] = pos;
                mScaleData[mdf.mCurNumber].X = scale.X;
                mScaleData[mdf.mCurNumber].Y = scale.Y;
                mScaleData[mdf.mCurNumber].Z = scale.Z;
                mScaleData[mdf.mCurNumber].W = hitProxyId;
                mRotateData[mdf.mCurNumber] = quat;
                mF41Data[mdf.mCurNumber] = f41;

                var result = mdf.mCurNumber;
                mdf.mCurNumber++;
                return result;
            }
            public unsafe void SetInstance(uint index, Vector3* pos, Vector3* scale, Quaternion* quat, Vector4ui* f41, uint* hitProxyId)
            {
                if (pos != IntPtr.Zero.ToPointer())
                    mPosData[index] = *pos;
                if (scale != IntPtr.Zero.ToPointer())
                {
                    mScaleData[index].X = scale->X;
                    mScaleData[index].Y = scale->Y;
                    mScaleData[index].Z = scale->Z;
                }
                if (hitProxyId != IntPtr.Zero.ToPointer())
                    mScaleData[index].W = *hitProxyId;
                if (quat != IntPtr.Zero.ToPointer())
                    mRotateData[index] = *quat;
                if (f41 != IntPtr.Zero.ToPointer())
                    mF41Data[index] = *f41;
            }
        }        
        public UInstantVBs InstantVBs;

        public class UInstantSSBO
        {
            public FVSInstantData[] InstData;
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
            public unsafe void SureBuffers(UInstanceModifier mdf, uint nSize)
            {
                if (mdf.mMaxNumber >= nSize)
                {
                    return;
                }

                var oldData = InstData;

                Cleanup();

                mdf.mMaxNumber = nSize;
                
                InstData = new FVSInstantData[mdf.mMaxNumber];

                var bfDesc = new NxRHI.FBufferDesc();
                bfDesc.SetDefault(false, NxRHI.EBufferType.BFT_SRV);
                bfDesc.Type = NxRHI.EBufferType.BFT_SRV;// | NxRHI.EBufferType.BFT_UAV;
                bfDesc.CpuAccess = NxRHI.ECpuAccess.CAS_WRITE;
                bfDesc.Usage = NxRHI.EGpuUsage.USAGE_DYNAMIC;
                bfDesc.StructureStride = (uint)sizeof(FVSInstantData);
                bfDesc.Size = (uint)sizeof(FVSInstantData) * mdf.mMaxNumber;

                if (mdf.mCurNumber > 0)
                {
                    fixed (FVSInstantData* pSrc = &oldData[0])
                    fixed (FVSInstantData* pTar = &InstData[0])
                    {
                        CoreSDK.MemoryCopy(pTar, pSrc, mdf.mCurNumber * (uint)sizeof(FVSInstantData));
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
            }
            public uint PushInstance(UInstanceModifier mdf, in Vector3 pos, in Vector3 scale, in Quaternion quat, in Vector4ui f41, uint hitProxyId)
            {
                var rc = UEngine.Instance.GfxDevice.RenderContext;

                //uint growSize = 1;
                //if (mdf.mMaxNumber > 10)
                //{
                //    growSize += mdf.mMaxNumber;
                //}
                //SureBuffers(mdf, mdf.mCurNumber + growSize);
                System.Diagnostics.Debug.Assert(mdf.CurNumber < mdf.mMaxNumber);

                InstData[mdf.mCurNumber].Position = pos;
                InstData[mdf.mCurNumber].Quat = quat;
                InstData[mdf.mCurNumber].Scale = scale;
                InstData[mdf.mCurNumber].UserData = f41;
                InstData[mdf.mCurNumber].HitProxyId = hitProxyId;

                var result = mdf.mCurNumber;
                mdf.mCurNumber++;

                IsDirty = true;
                return result;
            }
            public unsafe void SetInstance(uint index, Vector3* pos, Vector3* scale, Quaternion* quat, Vector4ui* f41, uint* hitProxyId)
            {
                if (pos != IntPtr.Zero.ToPointer())
                    InstData[index].Position = *pos;
                if (quat != IntPtr.Zero.ToPointer())
                    InstData[index].Quat = *quat;
                if (scale != IntPtr.Zero.ToPointer())
                    InstData[index].Scale = *scale;
                if (f41 != IntPtr.Zero.ToPointer())
                    InstData[index].UserData = *f41;
                if (hitProxyId != IntPtr.Zero.ToPointer())
                    InstData[index].HitProxyId = *hitProxyId;

                IsDirty = true;
            }
            public unsafe void Flush2VB(UInstanceModifier mdf)
            {
                if (mdf.mCurNumber == 0)
                    return;

                if (IsDirty)
                {
                    var rc = UEngine.Instance.GfxDevice.RenderContext;
                    fixed (FVSInstantData* pTar = &InstData[0])
                    {
                        InstantBuffer.UpdateGpuData(0,  pTar, mdf.mCurNumber * (uint)sizeof(FVSInstantData));
                    }
                    IsDirty = false;
                }
            }
        }
        public UInstantSSBO InstantSSBO;

        ~UInstanceModifier()
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
            {
                InstantSSBO = new UInstantSSBO();
            }
            else
            {
                InstantVBs = new UInstantVBs();
            }
        }
        public void SureBuffers(uint nSize)
        {
            if (InstantSSBO!=null)
            {
                InstantSSBO.SureBuffers(this, nSize);
            }
            else if (InstantVBs != null)
            {
                InstantVBs.SureBuffers(this, nSize);
            }
        }

        public uint PushInstance(in Vector3 pos, in Vector3 scale, in Quaternion quat, in Vector4ui f41, uint hitProxyId)
        {
            if (InstantSSBO != null)
            {
                return InstantSSBO.PushInstance(this, in pos, in scale, in quat, in f41, hitProxyId);
            }
            else if (InstantVBs != null)
            {
                return InstantVBs.PushInstance(this, in pos, in scale, in quat, in f41, hitProxyId);
            }
            return uint.MaxValue;
        }
        public unsafe void SetInstance(uint index, Vector3* pos, Vector3* scale, Quaternion* quat, Vector4ui* f41, uint* hitProxyId)
        {
            if (InstantSSBO != null)
            {
                InstantSSBO.SetInstance(index, pos, scale, quat, f41, hitProxyId);
            }
            else if (InstantVBs != null)
            {
                InstantVBs.SetInstance(index, pos, scale, quat, f41, hitProxyId);
            }
        }

        public unsafe void Flush2VB()
        {
            if (InstantSSBO != null)
            {
                InstantSSBO.Flush2VB(this);
            }
            else if (InstantVBs != null)
            {
                InstantVBs.Flush2VB(this);
            }
        }
        public unsafe void OnDrawCall(Pipeline.URenderPolicy.EShadingType shadingType, NxRHI.UGraphicDraw drawcall, Pipeline.URenderPolicy policy, UMesh mesh)
        {
            drawcall.mCoreObject.DrawInstance = (ushort)this.CurNumber;
            if (InstantSSBO != null)
            {
                var binder = drawcall.FindBinder("VSInstantDataArray");
                if (binder.IsValidPointer == false)
                    return;
                this.Flush2VB();
                drawcall.BindSRV(binder, InstantSSBO.InstantSRV);
            }
            else if (InstantVBs != null)
            {
                drawcall.BindAttachVertexArray(InstantVBs.mAttachVBs);
            }
        }
    }
}
