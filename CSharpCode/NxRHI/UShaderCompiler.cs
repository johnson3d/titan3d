using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.NxRHI
{
    public class UShaderCode : AuxPtrType<NxRHI.FShaderCode>
    {
        public UShaderCode()
        {
            mCoreObject = FShaderCode.CreateInstance();
        }
        public string TextCode
        {
            get
            {
                return mCoreObject.GetSourceCode();
            }
            set
            {
                mCoreObject.SetSourceCode(value);
            }
        }
    }
    public class UShaderDefinitions : AuxPtrType<NxRHI.IShaderDefinitions> , IDisposable
    {
        public UShaderDefinitions()
        {
            mCoreObject = IShaderDefinitions.CreateInstance();
        }
        public string DefineCode
        {
            get
            {
                string result = "";
                for (uint i = 0; i < mCoreObject.GetDefineCount(); i++)
                {
                    result = result + mCoreObject.GetName(i) + "=" + mCoreObject.GetValue(i);
                }
                return result;
            }
        }
        

        public override string ToString()
        {
            return DefineCode;
        }
        public uint GetDefineCount()
        {
            return mCoreObject.GetDefineCount();
        }
        public void AddDefine(string name, string value)
        {
            mCoreObject.AddDefine(name, value);
        }
        public void AddDefine(string name, int value)
        {
            mCoreObject.AddDefine(name, value.ToString());
        }
        public void ClearDefines()
        {
            mCoreObject.ClearDefines();
        }
        public void RemoveDefine(string name)
        {
            mCoreObject.RemoveDefine(name);
        }
        public void MergeDefinitions(UShaderDefinitions def)
        {
            mCoreObject.MergeDefinitions(def.mCoreObject);
        }
    }
    public class UShaderDesc : AuxPtrType<NxRHI.FShaderDesc>
    {
        public Graphics.Pipeline.Shader.UShadingEnv.FPermutationId PermutationId { get; set; }
        public UShaderDesc()
        {
            mCoreObject = FShaderDesc.CreateInstance();
        }
        public UShaderDesc(EShaderType type)
        {
            mCoreObject = FShaderDesc.CreateInstance();
            mCoreObject.Type = type;
        }
        public bool LoadXnd(XndNode node)
        {
            return false;
        }
    }
    public class UShaderCompiler : AuxPtrType<NxRHI.FShaderCompiler>
    {
        public UShaderCompiler(FShaderCompiler.FDelegate_FnGetShaderCodeStream fn)
        {
            mCoreObject = FShaderCompiler.CreateInstance();
            mCoreObject.SetCallback(fn);
        }
        
        public bool CompileShader(UShaderDesc shaderDesc, string shader, string entry, EShaderType type, string sm, UShaderDefinitions defines, EShaderLanguage sl, bool bDebugShader, string extHlslVersion)
        {
            return mCoreObject.CompileShader(shaderDesc.mCoreObject, shader, entry, type, sm, defines.mCoreObject, sl, bDebugShader, extHlslVersion);
        }
    }
}
