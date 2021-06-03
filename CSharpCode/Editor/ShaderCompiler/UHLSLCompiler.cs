using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace EngineNS.Editor.ShaderCompiler
{
    public unsafe class UHLSLCompiler : AuxPtrType<IShaderConductor>
    {
        public UHLSLCompiler()
        {
            mCoreObject = IShaderConductor.CreateInstance();
            GetShaderCodeStream = this.GetHLSLCode;
            mCoreObject.SetCodeStreamGetter(GetShaderCodeStream);
        }
        private FGetShaderCodeStream GetShaderCodeStream;
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
            RHI.CShaderDefinitions defines, bool bDebugShader, bool bDxbc, bool bGlsl, bool bMetal)
        {
            RHI.CShaderDesc desc = new RHI.CShaderDesc(type);
            Material = mtl;
            MdfQueueType = Rtti.UTypeDesc.TypeOf(mdfType);
            IShaderDefinitions* defPtr = (IShaderDefinitions*)0;
            if (defines != null)
                defPtr = defines.mCoreObject.Ptr;
            var ok = mCoreObject.CompileShader(desc.mCoreObject.Ptr, shader, entry, type, sm, defPtr,
                        bDebugShader, bDxbc, bGlsl, bMetal);

            if (ok == false)
                return null;

            return desc;
        }
    }
}
