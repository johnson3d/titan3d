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
        public UInt32_4 UserData;

        public UInt32_4 PointLightIndices;
    };

    public class UInstanceModifier
    {
        uint mCurNumber = 0;
        uint mMaxNumber = 0;
        public class UInstantVBs
        {
            public Vector3[] mPosData = null;
            public Vector4[] mScaleData = null;
            public Quaternion[] mRotateData = null;
            public UInt32_4[] mF41Data = null;

            public RHI.CVertexBuffer mPosVB;
            public RHI.CVertexBuffer mScaleVB;
            public RHI.CVertexBuffer mRotateVB;
            public RHI.CVertexBuffer mF41VB;

            public RHI.CVertexArray mAttachVBs = new RHI.CVertexArray();
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
                mF41Data = new UInt32_4[mdf.mMaxNumber];

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

                    fixed (UInt32_4* pSrc = &oldF41[0])
                    fixed (UInt32_4* pTar = &mF41Data[0])
                    {
                        CoreSDK.MemoryCopy(pTar, pSrc, mdf.mCurNumber * (uint)sizeof(UInt32_4));
                    }
                }


                var rc = UEngine.Instance.GfxDevice.RenderContext;
                var desc = new IVertexBufferDesc();
                desc.SetDefault();
                desc.CPUAccess = (UInt32)ECpuAccess.CAS_WRITE;
                desc.ByteWidth = (UInt32)(sizeof(Vector3) * mdf.mMaxNumber);
                desc.Stride = (UInt32)sizeof(Vector3);
                mPosVB = rc.CreateVertexBuffer(in desc);

                desc.ByteWidth = (UInt32)(sizeof(Vector4) * mdf.mMaxNumber);
                desc.Stride = (UInt32)sizeof(Vector4);
                mScaleVB = rc.CreateVertexBuffer(in desc);

                desc.ByteWidth = (UInt32)(sizeof(Quaternion) * mdf.mMaxNumber);
                desc.Stride = (UInt32)sizeof(Quaternion);
                mRotateVB = rc.CreateVertexBuffer(in desc);

                desc.ByteWidth = (UInt32)(sizeof(Vector4) * mdf.mMaxNumber);
                desc.Stride = (UInt32)sizeof(Vector4);
                mF41VB = rc.CreateVertexBuffer(in desc);
            }

            public unsafe void Flush2VB(ICommandList cmd, UInstanceModifier mdf)
            {
                mAttachVBs.mCoreObject.mNumInstances = mdf.mCurNumber;
                if (mdf.mCurNumber == 0)
                    return;

                var rc = UEngine.Instance.GfxDevice.RenderContext;
                fixed (Vector3* p = &mPosData[0])
                {
                    mPosVB.mCoreObject.UpdateGPUBuffData(cmd, p, (UInt32)(sizeof(Vector3) * mdf.mCurNumber));
                }
                fixed (Vector4* p = &mScaleData[0])
                {
                    mScaleVB.mCoreObject.UpdateGPUBuffData(cmd, p, (UInt32)(sizeof(Vector4) * mdf.mCurNumber));
                }
                fixed (Quaternion* p = &mRotateData[0])
                {
                    mRotateVB.mCoreObject.UpdateGPUBuffData(cmd, p, (UInt32)(sizeof(Quaternion) * mdf.mCurNumber));
                }
                fixed (UInt32_4* p = &mF41Data[0])
                {
                    mF41VB.mCoreObject.UpdateGPUBuffData(cmd, p, (UInt32)(sizeof(UInt32_4) * mdf.mCurNumber));
                }

                mAttachVBs.mCoreObject.BindVertexBuffer(EVertexStreamType.VST_InstPos, mPosVB.mCoreObject);
                mAttachVBs.mCoreObject.BindVertexBuffer(EVertexStreamType.VST_InstQuat, mRotateVB.mCoreObject);
                mAttachVBs.mCoreObject.BindVertexBuffer(EVertexStreamType.VST_InstScale, mScaleVB.mCoreObject);
                mAttachVBs.mCoreObject.BindVertexBuffer(EVertexStreamType.VST_F4_1, mF41VB.mCoreObject);
            }

            public void PushInstance(UInstanceModifier mdf, in Vector3 pos, in Vector3 scale, in Quaternion quat, in UInt32_4 f41, uint hitProxyId)
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

                mdf.mCurNumber++;
            }
        }        
        public UInstantVBs InstantVBs;

        public class UInstantSSBO
        {
            public FVSInstantData[] InstData;
            public RHI.CGpuBuffer InstantBuffer;
            public RHI.CShaderResourceView InstantSRV;
            public RHI.CVertexArray mAttachVBs = new RHI.CVertexArray();
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

                var bfDesc = new IGpuBufferDesc();
                bfDesc.SetDefault();
                bfDesc.SetMode(true, false);
                bfDesc.StructureByteStride = (uint)sizeof(FVSInstantData);
                bfDesc.ByteWidth = (uint)sizeof(FVSInstantData) * mdf.mMaxNumber;

                if (mdf.mCurNumber > 0)
                {
                    fixed (FVSInstantData* pSrc = &oldData[0])
                    fixed (FVSInstantData* pTar = &InstData[0])
                    {
                        CoreSDK.MemoryCopy(pTar, pSrc, mdf.mCurNumber * (uint)sizeof(FVSInstantData));
                        InstantBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateGpuBuffer(in bfDesc, (IntPtr)pTar);
                    }
                }
                else
                {
                    InstantBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateGpuBuffer(in bfDesc, IntPtr.Zero);
                }

                var srvDesc = new IShaderResourceViewDesc();
                srvDesc.SetBuffer();
                srvDesc.mGpuBuffer = InstantBuffer.mCoreObject;
                srvDesc.Buffer.NumElements = mdf.mMaxNumber;
                InstantSRV = UEngine.Instance.GfxDevice.RenderContext.CreateShaderResourceView(in srvDesc);
            }
            public void PushInstance(UInstanceModifier mdf, in Vector3 pos, in Vector3 scale, in Quaternion quat, in UInt32_4 f41, uint hitProxId)
            {
                var rc = UEngine.Instance.GfxDevice.RenderContext;

                uint growSize = 1;
                if (mdf.mMaxNumber > 10)
                {
                    growSize += mdf.mMaxNumber;
                }
                SureBuffers(mdf, mdf.mCurNumber + growSize);

                InstData[mdf.mCurNumber].Position = pos;
                InstData[mdf.mCurNumber].Quat = quat;
                InstData[mdf.mCurNumber].Scale = scale;
                InstData[mdf.mCurNumber].UserData = f41;
                InstData[mdf.mCurNumber].HitProxyId = hitProxId;

                mdf.mCurNumber++;

                IsDirty = true;
            }
            public unsafe void Flush2VB(ICommandList cmd, UInstanceModifier mdf)
            {
                mAttachVBs.mCoreObject.mNumInstances = mdf.mCurNumber;
                if (mdf.mCurNumber == 0)
                    return;

                if (IsDirty)
                {
                    var rc = UEngine.Instance.GfxDevice.RenderContext;
                    fixed (FVSInstantData* pTar = &InstData[0])
                    {
                        InstantBuffer.mCoreObject.UpdateBufferData(cmd, 0, pTar, mdf.mCurNumber * (uint)sizeof(FVSInstantData));
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

        public void PushInstance(in Vector3 pos, in Vector3 scale, in Quaternion quat, in UInt32_4 f41, uint hitProxyId)
        {
            if (InstantSSBO != null)
            {
                InstantSSBO.PushInstance(this, in pos, in scale, in quat, in f41, hitProxyId);
            }
            else if (InstantVBs != null)
            {
                InstantVBs.PushInstance(this, in pos, in scale, in quat, in f41, hitProxyId);
            }
        }

        public unsafe void Flush2VB(ICommandList cmd)
        {
            if (InstantSSBO != null)
            {
                InstantSSBO.Flush2VB(cmd, this);
            }
            else if (InstantVBs != null)
            {
                InstantVBs.Flush2VB(cmd, this);
            }
        }
        public unsafe void OnDrawCall(Pipeline.URenderPolicy.EShadingType shadingType, RHI.CDrawCall drawcall, Pipeline.URenderPolicy policy, UMesh mesh)
        {
            if (InstantSSBO != null)
            {
                var binder = drawcall.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "VSInstantDataArray");
                if (CoreSDK.IsNullPointer(binder))
                    return;
                this.Flush2VB(UEngine.Instance.GfxDevice.RenderContext.mCoreObject.GetImmCommandList());
                drawcall.mCoreObject.BindShaderSrv(binder, InstantSSBO.InstantSRV.mCoreObject);
                drawcall.mCoreObject.BindAttachVBs(InstantSSBO.mAttachVBs.mCoreObject);
            }
            else if (InstantVBs != null)
            {
                drawcall.mCoreObject.BindAttachVBs(InstantVBs.mAttachVBs.mCoreObject);
            }
        }
    }
}
