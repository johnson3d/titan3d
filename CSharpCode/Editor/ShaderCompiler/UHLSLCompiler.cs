using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace EngineNS.Editor.ShaderCompiler
{
    public unsafe class UHLSLCompiler : AuxPtrType<IShaderConductor>
    {
        static CoreSDK.FDelegate_FOnShaderTranslated OnShaderTranslated = OnShaderTranslatedImpl;
        static void OnShaderTranslatedImpl(EngineNS.IShaderDesc arg0)
        {
            var glsl = arg0.GetGLCode();
            if (glsl.Length > 0)
            {
                bool changed = false;
                if (glsl.Contains("#error No extension available for FP16."))
                {
                    glsl = glsl.Replace("#error No extension available for Int16.", "#define float16_t float");
                    changed = true;
                }
                if (glsl.Contains("#error No extension available for Int16."))
                {
                    glsl = glsl.Replace("#error No extension available for Int16.", "#define uint16_t uint");
                    changed = true;
                }
                if (changed)
                {
                    arg0.SetGLCode(glsl);
                }
            }
        }
        static UHLSLCompiler()
        {
            CoreSDK.SetOnShaderTranslated(OnShaderTranslated);
        }
        public UHLSLCompiler()
        {
            mCoreObject = IShaderConductor.CreateInstance();
            GetShaderCodeStream = this.GetHLSLCode;
            mCoreObject.SetCodeStreamGetter(GetShaderCodeStream);
        }
        private IShaderConductor.FDelegate_FGetShaderCodeStream GetShaderCodeStream;
        //[UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private MemStreamWriter* GetHLSLCode(void* includeName)
        {
            var file = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)includeName);

            bool isVar = false;
            RName rn = null;
            if (file.EndsWith("/Material"))
            {
                rn = Material;
            }
            else if (file.EndsWith("/MaterialVar"))
            {
                rn = Material;
                isVar = true;
            }
            else if (file.EndsWith("/MdfQueue"))
            {
                var mdf = Rtti.UTypeDescManager.CreateInstance(MdfQueueType) as Graphics.Pipeline.Shader.UMdfQueue;
                if (mdf != null)
                    return mdf.SourceCode.mCoreObject.CppPointer;
                else
                    return (MemStreamWriter*)0;
            }
            else if (file.StartsWith(UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Engine)))
            {
                var path = IO.FileManager.GetRelativePath(UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Engine), file);
                rn = RName.GetRName(path, RName.ERNameType.Engine);
            }
            else if (file.StartsWith(UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Game)))
            {
                var path = IO.FileManager.GetRelativePath(UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Game), file);
                rn = RName.GetRName(path, RName.ERNameType.Game);
            }
            else
            {
                rn = RName.GetRName(file, RName.ERNameType.Engine);
            }
            if (rn != null)
            {
                var code = UShaderCodeManager.Instance.GetShaderCodeProvider(rn);
                if (code != null)
                {
                    if (isVar)
                        return code.DefineCode.mCoreObject.CppPointer;
                    else
                        return code.SourceCode.mCoreObject.CppPointer;
                }
            }
            return (MemStreamWriter*)0;
        }
        private RName Material;
        private Rtti.UTypeDesc MdfQueueType;
        public RHI.CShaderDesc CompileShader(string shader, string entry, EShaderType type, string sm,
            RName mtl, Type mdfType,
            RHI.CShaderDefinitions defines, bool bDebugShader)
        {
            RHI.CShaderDesc desc = new RHI.CShaderDesc(type);
            Material = mtl;
            MdfQueueType = Rtti.UTypeDesc.TypeOf(mdfType);
            //IShaderDefinitions defPtr = new IShaderDefinitions((void*)0);
            unsafe
            {
                using (IShaderDefinitions defPtr = IShaderDefinitions.CreateInstance())
                {
                    if (defines != null)
                    {
                        defPtr.MergeDefinitions(defines.mCoreObject);
                    }

                    var cfg = UEngine.Instance.Config;
                    if (cfg.CookGLSL)
                    {
                        defPtr.AddDefine("RHI_OGL", "1");
                    }

                    var ok = mCoreObject.CompileShader(desc.mCoreObject, shader, entry, type, sm, defPtr,
                                bDebugShader, cfg.CookDXBC, cfg.CookGLSL, cfg.CookMETAL);

                    if (ok == false)
                        return null;

                    return desc;
                }
            }   
        }
    }
}
