using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.NxRHI
{
    public class TtShaderCode : AuxPtrType<NxRHI.FShaderCode>
    {
        public TtShaderCode()
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
    public class TtShaderDefinitions : AuxPtrType<NxRHI.IShaderDefinitions> , IDisposable
    {
        public TtShaderDefinitions()
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
        public void MergeDefinitions(TtShaderDefinitions def)
        {
            mCoreObject.MergeDefinitions(def.mCoreObject);
        }
    }
    public class TtShaderDesc : AuxPtrType<NxRHI.FShaderDesc>
    {
        public Graphics.Pipeline.Shader.TtShadingEnv.FPermutationId PermutationId { get; set; }
        public TtShaderDesc()
        {
            mCoreObject = FShaderDesc.CreateInstance();
        }
        public TtShaderDesc(EShaderType type)
        {
            mCoreObject = FShaderDesc.CreateInstance();
            mCoreObject.Type = type;
        }
        public bool LoadXnd(XndNode node)
        {
            return false;
        }
        public string DebugName
        {
            get => mCoreObject.DebugName.c_str();
            set => mCoreObject.DebugName = VNameString.FromString(value);
        }
        public string FunctionName
        {
            get => mCoreObject.FunctionName.c_str();
            set => mCoreObject.FunctionName = VNameString.FromString(value);
        }
    }
    public class TtShaderCompiler : AuxPtrType<NxRHI.FShaderCompiler>
    {
        public TtShaderCompiler(FShaderCompiler.FDelegate_FnGetShaderCodeStream fn)
        {
            mCoreObject = FShaderCompiler.CreateInstance();
            mCoreObject.SetCallback(fn);
        }
        
        public bool CompileShader(TtShaderDesc shaderDesc, string shader, string entry, EShaderType type, string sm, TtShaderDefinitions defines, EShaderLanguage sl, bool bDebugShader, string extHlslVersion, string dxcArgs)
        {
            return mCoreObject.CompileShader(shaderDesc.mCoreObject, shader, entry, type, sm, defines.mCoreObject, sl, bDebugShader, extHlslVersion, dxcArgs);
        }
    }
}
