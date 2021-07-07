using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Shader
{
    public class UMdfQueue : AuxPtrType<IMdfQueue>, IShaderCodeProvider
    {
        public override string ToString()
        {
            string result = $"Var: {DefineCode?.AsText}\n";
            result += $"Code: {SourceCode.AsText}\n";
            return result;
        }
        protected Hash160 mMdfQueueHash;
        public virtual Hash160 MdfQueueHash
        {
            get
            {
                return mMdfQueueHash;
            }
            set
            {
                mMdfQueueHash = value;
            }
        }
        public virtual Hash160 GetHash()
        {
            string result = DefineCode?.AsText;
            result += SourceCode?.AsText;
            mMdfQueueHash = Hash160.CreateHash160(result);
            return mMdfQueueHash;
        }
        public UMdfQueue()
        {
            mCoreObject = IMdfQueue.CreateInstance();
        }
        public virtual void CopyFrom(UMdfQueue mdf)
        {

        }
        public RName CodeName { get; set; }
        public IO.CMemStreamWriter DefineCode { get; protected set; }
        public IO.CMemStreamWriter SourceCode { get; protected set; }
        protected virtual void UpdateShaderCode()
        {

        }
        public virtual RHI.CInputLayout GetInputLayout(RHI.CShaderDesc vsDesc)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            uint inputStreams = 0;
            mCoreObject.GetInputStreams(ref inputStreams);
            unsafe
            {
                var layoutDesc = IMesh.CreateInputLayoutDesc(inputStreams);
                layoutDesc.SetShaderDesc(vsDesc.mCoreObject);
                var ret = UEngine.Instance.GfxDevice.InputLayoutManager.GetPipelineState(rc, *layoutDesc.CppPointer);
                CoreSDK.IUnknown_Release(layoutDesc);
                return ret;
            }
        }
        public virtual void OnDrawCall(Pipeline.IRenderPolicy.EShadingType shadingType, RHI.CDrawCall drawcall, IRenderPolicy policy, Mesh.UMesh mesh)
        {

        }
    }
}
