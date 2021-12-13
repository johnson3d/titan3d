using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Mesh.Modifier
{
    public class UInstanceModifier
    {
        uint mCurNumber = 0;
        uint mMaxNumber = 0;
        Vector3[] mPosData = null;
        Vector4[] mScaleData = null;
        Quaternion[] mRotateData = null;
        UInt32_4[] mF41Data = null;

        RHI.CVertexBuffer mPosVB;
        RHI.CVertexBuffer mScaleVB;
        RHI.CVertexBuffer mRotateVB;
        RHI.CVertexBuffer mF41VB;

        ~UInstanceModifier()
        {
            Cleanup();
        }

        RHI.CVertexArray mAttachVBs = new RHI.CVertexArray();
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
        }
        private unsafe void SureBuffers(uint nSize)
        {
            if (mMaxNumber > nSize)
            {
                return;
            }

            Cleanup();

            var oldPos = mPosData;
            var oldScale = mScaleData;
            var oldQuat = mRotateData;
            var oldF41 = mF41Data;

            mMaxNumber = nSize * 2;
            //mInstDataArray = new VSInstantData[mMaxNumber];
            mPosData = new Vector3[mMaxNumber];
            mScaleData = new Vector4[mMaxNumber];
            mRotateData = new Quaternion[mMaxNumber];
            mF41Data = new UInt32_4[mMaxNumber];

            if (mCurNumber > 0)
            {
                fixed (Vector3* pSrc = &oldPos[0])
                fixed (Vector3* pTar = &mPosData[0])
                {
                    CoreSDK.MemoryCopy(pTar, pSrc, mCurNumber * (uint)sizeof(Vector3));
                }

                fixed (Vector4* pSrc = &oldScale[0])
                fixed (Vector4* pTar = &mScaleData[0])
                {
                    CoreSDK.MemoryCopy(pTar, pSrc, mCurNumber * (uint)sizeof(Vector4));
                }

                fixed (Quaternion* pSrc = &oldQuat[0])
                fixed (Quaternion* pTar = &mRotateData[0])
                {
                    CoreSDK.MemoryCopy(pTar, pSrc, mCurNumber * (uint)sizeof(Quaternion));
                }

                fixed (UInt32_4* pSrc = &oldF41[0])
                fixed (UInt32_4* pTar = &mF41Data[0])
                {
                    CoreSDK.MemoryCopy(pTar, pSrc, mCurNumber * (uint)sizeof(UInt32_4));
                }
            }
            

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            var desc = new IVertexBufferDesc();
            desc.SetDefault();
            desc.CPUAccess = (UInt32)ECpuAccess.CAS_WRITE;
            desc.ByteWidth = (UInt32)(sizeof(Vector3) * mMaxNumber);
            desc.Stride = (UInt32)sizeof(Vector3);
            mPosVB = rc.CreateVertexBuffer(ref desc);

            desc.ByteWidth = (UInt32)(sizeof(Vector4) * mMaxNumber);
            desc.Stride = (UInt32)sizeof(Vector4);
            mScaleVB = rc.CreateVertexBuffer(ref desc);

            desc.ByteWidth = (UInt32)(sizeof(Quaternion) * mMaxNumber);
            desc.Stride = (UInt32)sizeof(Quaternion);
            mRotateVB = rc.CreateVertexBuffer(ref desc);

            desc.ByteWidth = (UInt32)(sizeof(Vector4) * mMaxNumber);
            desc.Stride = (UInt32)sizeof(Vector4);
            mF41VB = rc.CreateVertexBuffer(ref desc);
        }
        public void PushInstance(ref Vector3 pos, ref Vector3 scale, ref Quaternion quat, ref UInt32_4 f41, int lightNum)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            SureBuffers(mCurNumber + 1);

            if (UEngine.Instance.RuntimeConfig.VS_StructureBuffer)
            {
                //mInstDataArray[mCurSize].WorldMatrix = Matrix.Transformation(scale, quat, pos);
                //mInstDataArray[mCurSize].WorldMatrix.Transpose();
                //mInstDataArray[mCurSize].CustomData.x = (uint)lightNum;
                //mInstDataArray[mCurSize].PointLightIndices = f41;
            }
            else
            {
                mPosData[mCurNumber] = pos;
                mScaleData[mCurNumber].X = scale.X;
                mScaleData[mCurNumber].Y = scale.Y;
                mScaleData[mCurNumber].Z = scale.Z;
                mScaleData[mCurNumber].W = lightNum;
                mRotateData[mCurNumber] = quat;
                mF41Data[mCurNumber] = f41;
            }
            mCurNumber++;
        }
        public void Flush2VB(ICommandList cmd)
        {
            mAttachVBs.mCoreObject.mNumInstances = mCurNumber;
            if (mCurNumber == 0)
                return;

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            unsafe
            {
                if (UEngine.Instance.RuntimeConfig.VS_StructureBuffer)
                {
                    //fixed (VSInstantData* p = &mInstDataArray[0])
                    //{
                    //    mInstDataBuffer.UpdateBufferData(cmd, (IntPtr)p, (UInt32)(sizeof(VSInstantData) * mCurSize));
                    //}
                }
                else
                {
                    fixed (Vector3* p = &mPosData[0])
                    {
                        mPosVB.mCoreObject.UpdateGPUBuffData(cmd, p, (UInt32)(sizeof(Vector3) * mCurNumber));
                    }
                    fixed (Vector4* p = &mScaleData[0])
                    {
                        mScaleVB.mCoreObject.UpdateGPUBuffData(cmd, p, (UInt32)(sizeof(Vector4) * mCurNumber));
                    }
                    fixed (Quaternion* p = &mRotateData[0])
                    {
                        mRotateVB.mCoreObject.UpdateGPUBuffData(cmd, p, (UInt32)(sizeof(Quaternion) * mCurNumber));
                    }
                    fixed (UInt32_4* p = &mF41Data[0])
                    {
                        mF41VB.mCoreObject.UpdateGPUBuffData(cmd, p, (UInt32)(sizeof(UInt32_4) * mCurNumber));
                    }
                }
            }
        }
        public void OnDrawCall(Pipeline.IRenderPolicy.EShadingType shadingType, RHI.CDrawCall drawcall, Pipeline.IRenderPolicy policy, UMesh mesh)
        {
            drawcall.mCoreObject.BindAttachVBs(mAttachVBs.mCoreObject);
        }
    }
}
