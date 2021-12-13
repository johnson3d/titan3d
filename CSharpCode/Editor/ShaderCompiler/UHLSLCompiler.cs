using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace EngineNS.Editor.ShaderCompiler
{
    public class UHLSLInclude
    {
        public unsafe virtual MemStreamWriter* GetHLSLCode(string includeName, out bool bIncluded)
        {
            bIncluded = false;
            return (MemStreamWriter*)0;
        }
    }
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

            if (UserInclude != null)
            {
                bool bIncluded;
                var result = UserInclude.GetHLSLCode(file, out bIncluded);
                if (bIncluded)
                    return result;
            }
            bool isVar = false;
            RName rn = null;
            if (file.EndsWith("/Material"))
            {
                if (Material == null)
                    return (MemStreamWriter*)0;
                return Material.SourceCode.mCoreObject;
            }
            else if (file.EndsWith("/MaterialVar"))
            {
                if (Material == null)
                    return (MemStreamWriter*)0;
                return Material.DefineCode.mCoreObject;
            }
            else if (file.EndsWith("/MdfQueue"))
            {
                var mdf = Rtti.UTypeDescManager.CreateInstance(MdfQueueType) as Graphics.Pipeline.Shader.UMdfQueue;
                if (mdf != null)
                {
                    return mdf.SourceCode.mCoreObject;
                }
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
                        return code.DefineCode.mCoreObject;
                    else
                        return code.SourceCode.mCoreObject;
                }
            }
            return (MemStreamWriter*)0;
        }
        private UHLSLInclude UserInclude;
        private Graphics.Pipeline.Shader.UMaterial Material;
        private Rtti.UTypeDesc MdfQueueType;
        private string GetVertexStreamDefine(EVertexSteamType type)
        {
            switch (type)
            {
                case EVertexSteamType.VST_Position:
                    return "USE_VS_Position";
                case EVertexSteamType.VST_Normal:
                    return "USE_VS_Normal";
                case EVertexSteamType.VST_Tangent:
                    return "USE_VS_Tangent";
                case EVertexSteamType.VST_Color:
                    return "USE_VS_Color";
                case EVertexSteamType.VST_UV:
                    return "USE_VS_UV";
                case EVertexSteamType.VST_LightMap:
                    return "USE_VS_LightMap";
                case EVertexSteamType.VST_SkinIndex:
                    return "USE_VS_SkinIndex";
                case EVertexSteamType.VST_SkinWeight:
                    return "USE_VS_SkinWeight";
                case EVertexSteamType.VST_TerrainIndex:
                    return "USE_VS_TerrainIndex";
                case EVertexSteamType.VST_TerrainGradient:
                    return "USE_VS_TerrainGradient";
                case EVertexSteamType.VST_InstPos:
                    return "USE_VS_InstPos";
                case EVertexSteamType.VST_InstQuat:
                    return "USE_VS_InstQuat";
                case EVertexSteamType.VST_InstScale:
                    return "USE_VS_InstScale";
                case EVertexSteamType.VST_F4_1:
                    return "USE_VS_F4_1";
                case EVertexSteamType.VST_F4_2:
                    return "USE_VS_F4_2";
                case EVertexSteamType.VST_F4_3:
                    return "USE_VS_F4_3";
            }
            return null;
        }
        public RHI.CShaderDesc CompileShader(string shader, string entry, EShaderType type, string sm,
            Graphics.Pipeline.Shader.UShadingEnv shadingEnvshadingEnv, Graphics.Pipeline.Shader.UMaterial mtl, Type mdfType,
            RHI.CShaderDefinitions defines, bool bDebugShader, UHLSLInclude incProvider)
        {
            RHI.CShaderDesc desc = new RHI.CShaderDesc(type);
            UserInclude = incProvider;
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
                    if (shadingEnvshadingEnv != null)
                    {
                        var shadingNeeds = shadingEnvshadingEnv.GetNeedStreams();
                        if(shadingNeeds!=null)
                        {
                            foreach (var i in shadingNeeds)
                            {
                                defPtr.AddDefine(GetVertexStreamDefine(i), "1");
                            }
                        }
                        var mdfObj = Rtti.UTypeDescManager.CreateInstance(MdfQueueType) as Graphics.Pipeline.Shader.UMdfQueue;
                        if (mdfObj != null)
                        {
                            var mdfNeeds = mdfObj.GetNeedStreams();
                            if (mdfNeeds != null)
                            {
                                foreach (var i in mdfNeeds)
                                {
                                    defPtr.AddDefine(GetVertexStreamDefine(i), "1");
                                }
                            }
                        }
                    }
                    switch (type)
                    {
                        case EShaderType.EST_VertexShader:
                            defPtr.AddDefine("ShaderStage", "0");//VSStage
                            break;
                        case EShaderType.EST_PixelShader:
                            defPtr.AddDefine("ShaderStage", "1");//PSStage
                            break;
                        case EShaderType.EST_ComputeShader:
                            defPtr.AddDefine("ShaderStage", "2");//CSStage
                            break;
                        default:
                            System.Diagnostics.Debugger.Break();
                            break;
                    }
                    var cfg = UEngine.Instance.Config;
                    if (cfg.CookDXBC)
                    {
                        defPtr.AddDefine("RHI_TYPE", "RHI_DX11");
                        var ok = mCoreObject.CompileShader(desc.mCoreObject, shader, entry, type, sm, defPtr,
                                bDebugShader, EShaderLanguage.SL_DXBC);
                        if (ok == false)
                            return null;
                    }
                    if (cfg.CookGLSL)
                    {
                        defPtr.AddDefine("RHI_TYPE", "RHI_GL");
                        var ok = mCoreObject.CompileShader(desc.mCoreObject, shader, entry, type, sm, defPtr,
                                bDebugShader, EShaderLanguage.SL_GLSL);
                        if (ok == false)
                            return null;
                    }
                    if(cfg.CookMETAL)
                    {
                        defPtr.AddDefine("RHI_TYPE", "RHI_MTL");
                        var ok = mCoreObject.CompileShader(desc.mCoreObject, shader, entry, type, sm, defPtr,
                                bDebugShader, EShaderLanguage.SL_METAL);
                        if (ok == false)
                            return null;
                    }
                    if (cfg.CookSPIRV)
                    {
                        defPtr.AddDefine("RHI_TYPE", "RHI_VK");
                        var ok = mCoreObject.CompileShader(desc.mCoreObject, shader, entry, type, sm, defPtr,
                                bDebugShader, EShaderLanguage.SL_SPIRV);
                        if (ok == false)
                            return null;
                    }
                    
                    return desc;
                }
            }   
        }
    }
}
