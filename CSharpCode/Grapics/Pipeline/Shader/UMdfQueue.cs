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
            string result = DefineCode?.TextCode;
            result += SourceCode.TextCode;
            mMdfQueueHash = Hash160.CreateHash160(result);
            return mMdfQueueHash;
        }
        public UMdfQueue()
        {
            mCoreObject = IMdfQueue.CreateInstance();
        }
        public abstract NxRHI.EVertexStreamType[] GetNeedStreams();
        public virtual EPixelShaderInput[] GetPSNeedInputs()
        {
            return null;
        }
        //{
        //    return new EVertexSteamType[] { EVertexSteamType.VST_Position, };
        //}
        public virtual void CopyFrom(UMdfQueue mdf)
        {
            OnDrawCallCallback = mdf.OnDrawCallCallback;
        }
        public RName CodeName { get; set; }
        public NxRHI.UShaderCode DefineCode { get; protected set; }
        public NxRHI.UShaderCode SourceCode { get; protected set; }
        protected virtual string GetBaseBuilder(Bricks.CodeBuilder.Backends.UHLSLCodeGenerator codeBuilder)
        {
            return "";
        }
        protected virtual void UpdateShaderCode()
        {
            var codeBuilder = new Bricks.CodeBuilder.Backends.UHLSLCodeGenerator();
            GetBaseBuilder(codeBuilder);
        }

        public delegate void FOnDrawCall(Pipeline.URenderPolicy.EShadingType shadingType, NxRHI.UGraphicDraw drawcall, URenderPolicy policy, Mesh.UMesh mesh, int atom);
        public FOnDrawCall OnDrawCallCallback = null;
        public virtual void OnDrawCall(Pipeline.URenderPolicy.EShadingType shadingType, NxRHI.UGraphicDraw drawcall, URenderPolicy policy, Mesh.UMesh mesh, int atom)
        {
            if (OnDrawCallCallback != null)
                OnDrawCallCallback(shadingType, drawcall, policy, mesh, atom);
        }

        public virtual Rtti.UTypeDesc GetPermutation(List<string> features)
        {
            return Rtti.UTypeDesc.TypeOf(this.GetType());
        }
    }

    public class UMdf_Shadow
    {
    }
    public class UMdf_NoShadow : UMdf_Shadow
    {

    }
}
