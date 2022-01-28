using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.RHI
{
    public class CShaderProgram : AuxPtrType<IShaderProgram>
    {
        public class ShaderVarAttribute : Attribute
        {
            public string VarType;
            public string CBuffer;
        }
        public class IShaderVarIndexer
        {
            public void UpdateIndex(CShaderProgram program)
            {
                var members = this.GetType().GetFields();
                foreach (var i in members)
                {
                    var attrs = i.GetCustomAttributes(typeof(CShaderProgram.ShaderVarAttribute), true);
                    if (attrs.Length == 0)
                    {
                        continue;
                    }

                    var varAttr = attrs[0] as ShaderVarAttribute;
                    if (varAttr.VarType == "Texture")
                    {
                        var index = program.mCoreObject.GetReflector().FindShaderBinder(EShaderBindType.SBT_Srv, i.Name);
                        i.SetValue(this, index);
                    }
                    else if (varAttr.VarType == "CBuffer")
                    {
                        var index = program.mCoreObject.GetReflector().FindShaderBinder(EShaderBindType.SBT_CBuffer, i.Name);
                        i.SetValue(this, index);
                    }
                    else if (varAttr.VarType == "Var")
                    {
                        unsafe
                        {
                            var pDesc = (IConstantBufferDesc*)program.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_CBuffer, i.Name);
                            if (pDesc != (IConstantBufferDesc*)0)
                            {
                                var index = pDesc->FindVar(varAttr.CBuffer);
                                i.SetValue(this, index);
                            }
                        }
                    }
                }
            }
            public IShaderVarIndexer NextIndexer = null;
        }

        public ref IConstantBufferDesc GetCBuffer(string name)
        {
            unsafe
            {
                var pDesc = (IConstantBufferDesc*)mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_CBuffer, name);
                return ref *pDesc;
            }
        }
        public ref IConstantBufferDesc GetCBuffer(uint idx)
        {
            unsafe
            {
                var pDesc = (IConstantBufferDesc*)mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_CBuffer, idx);
                return ref *pDesc;
            }
        }
        public ref ShaderRViewBindInfo GetTexture(string name)
        {
            unsafe
            {
                var pDesc = (ShaderRViewBindInfo*)mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, name);
                return ref *pDesc;
            }
        }
        public ref ShaderRViewBindInfo GetTexture(uint idx)
        {
            unsafe
            {
                var pDesc = (ShaderRViewBindInfo*)mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, idx);
                return ref *pDesc;
            }
        }
        public ref SamplerBindInfo GetSampler(string name)
        {
            unsafe
            {
                var pDesc = (SamplerBindInfo*)mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Sampler, name);
                return ref *pDesc;
            }
        }
        public ref SamplerBindInfo GetSampler(uint idx)
        {
            unsafe
            {
                var pDesc = (SamplerBindInfo*)mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Sampler, idx);
                return ref *pDesc;
            }
        }
    }
}
