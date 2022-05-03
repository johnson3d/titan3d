using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Shader
{
    public abstract class UMdfQueue : AuxPtrType<IMdfQueue>, IShaderCodeProvider
    {
        public object MdfDatas;
        public override string ToString()
        {
            return Rtti.UTypeDescManager.Instance.GetTypeStringFromType(this.GetType());
            //string result = $"Var: {DefineCode?.AsText}\n";
            //result += $"Code: {SourceCode.AsText}\n";
            //return result;
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
        public abstract EVertexSteamType[] GetNeedStreams();
        //{
        //    return new EVertexSteamType[] { EVertexSteamType.VST_Position, };
        //}
        public virtual void CopyFrom(UMdfQueue mdf)
        {

        }
        public RName CodeName { get; set; }
        public IO.CMemStreamWriter DefineCode { get; protected set; }
        public IO.CMemStreamWriter SourceCode { get; protected set; }
        protected virtual string GetBaseBuilder(Bricks.CodeBuilder.Backends.UHLSLCodeGenerator codeBuilder)
        {
            return "";
        }
        protected virtual void UpdateShaderCode()
        {
            var codeBuilder = new Bricks.CodeBuilder.Backends.UHLSLCodeGenerator();
            GetBaseBuilder(codeBuilder);
        }
        
        public virtual void OnDrawCall(Pipeline.URenderPolicy.EShadingType shadingType, RHI.CDrawCall drawcall, URenderPolicy policy, Mesh.UMesh mesh)
        {

        }

        public virtual Rtti.UTypeDesc GetPermutation(List<string> features)
        {
            return Rtti.UTypeDesc.TypeOf(this.GetType());
        }
    }

    public class UMdf_Shadow
    {
    }
    public class UMdf_NoShadow
    {

    }
}
