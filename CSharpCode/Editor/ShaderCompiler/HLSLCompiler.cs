using Microsoft.CodeAnalysis.CSharp.Syntax;
using NPOI.HPSF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using static EngineNS.Graphics.Pipeline.Shader.TtShadingEnv;

namespace EngineNS.Editor.ShaderCompiler
{
    public class TtHLSLInclude
    {
        public unsafe virtual NxRHI.FShaderCode* GetHLSLCode(string includeName, out bool bIncluded)
        {
            bIncluded = false;
            return (NxRHI.FShaderCode*)0;
        }
    }
    public unsafe class TtHLSLCompiler
    {
        public bool IsWriteDebugFile
        {
            get
            {
                return TtEngine.Instance.Config.IsWriteShaderDebugFile;
            }
        }
        NxRHI.TtShaderCompiler mShaderCompiler = null;
        public Graphics.Pipeline.Shader.TtMdfQueueBase MdfQueue = null;
        static CoreSDK.FDelegate_FOnShaderTranslated OnShaderTranslated = OnShaderTranslatedImpl;
        static void OnShaderTranslatedImpl(NxRHI.FShaderDesc arg0)
        {
            var text = arg0.GetRhiDataAsText();
            if (text.Length > 0)
            {
                bool changed = false;
                if (text.Contains("#error No extension available for FP16."))
                {
                    text = text.Replace("#error No extension available for FP16.", "#define float16_t float");
                    changed = true;
                }
                if (text.Contains("#error No extension available for Int16."))
                {
                    text = text.Replace("#error No extension available for Int16.", "#define uint16_t uint");
                    changed = true;
                }
                if (changed)
                {
                    arg0.SetRhiDataFromText(text);
                }
            }
        }
        static TtHLSLCompiler()
        {
            CoreSDK.SetOnShaderTranslated(OnShaderTranslated);
        }
        public TtHLSLCompiler()
        {
            GetShaderCodeStream = this.GetHLSLCode;
            mShaderCompiler = new NxRHI.TtShaderCompiler(GetShaderCodeStream);
        }
        private static string FixRootPath(string file)
        {
            file = file.Replace("\\", "/");
            string result = file;
            var repPos = file.IndexOf("@Engine/");
            if (repPos >= 0)
            {
                result = file.Substring(repPos);
                result = file.Replace("@Engine/", TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Engine));
            }
            else
            {
                repPos = file.IndexOf("@Game/");
                if (repPos >= 0)
                {
                    result = file.Substring(repPos);
                    result = result.Replace("@Game/", TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Game));
                }
                else
                {
                    
                }
            }
            return result;
            //return result = result.Replace('\\','/');
        }
        public static NxRHI.FShaderCode GetIncludeCode(string file)
        {
            file = FixRootPath(file);
            RName rn = null;
            if (file.StartsWith("@"))
            {
                rn = RName.GetRName(file, RName.ERNameType.Engine);
            }
            else if (file.StartsWith(TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Engine)))
            {
                var path = IO.TtFileManager.GetRelativePath(TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Engine), file);
                rn = RName.GetRName(path, RName.ERNameType.Engine);
            }
            else if (file.StartsWith(TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Game)))
            {
                var path = IO.TtFileManager.GetRelativePath(TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Game), file);
                rn = RName.GetRName(path, RName.ERNameType.Game);
            }
            else
            {
                rn = RName.GetRName(file, RName.ERNameType.Engine);
            }
            if (rn != null)
            {
                var code = TtShaderCodeManager.Instance.GetShaderCodeProvider(rn);
                if (code != null)
                {
                    return code.SourceCode.mCoreObject;
                }
            }
            return new NxRHI.FShaderCode();
        }
        private NxRHI.FShaderCompiler.FDelegate_FnGetShaderCodeStream GetShaderCodeStream;
        public string MaterialCodeForDebug;
        //[UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private NxRHI.FShaderCode* GetHLSLCode(sbyte* includeName, sbyte* includeOriName)
        {
            var oriInc = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)includeOriName);
            var file = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)includeName);
            file = FixRootPath(file);

            if (UserInclude != null)
            {
                bool bIncluded;
                var result = UserInclude.GetHLSLCode(file, out bIncluded);
                if (bIncluded)
                    return result;
            }
            bool isVar = false;
            RName rn = null;
            if (file.EndsWith("/EnginePresessors"))
            {

            }
            else if (file.EndsWith("/Material"))
            {
                if (Material == null)
                    return (NxRHI.FShaderCode*)0;
                MaterialCodeForDebug = Material.SourceCode.TextCode;

                if (IsWriteDebugFile && Material.AssetName != null)
                {
                    var path = TtEngine.Instance.FileManager.GetPath(IO.TtFileManager.ERootDir.Cache, IO.TtFileManager.ESystemDir.DebugUtility) + $"/material/{Material.AssetName.RNameType}/{IO.TtFileManager.GetBaseDirectory(Material.AssetName.Name)}";
                    IO.TtFileManager.SureDirectory(path);
                    IO.TtFileManager.WriteAllText(path + $"{Material.AssetName.PureName}.shader", MaterialCodeForDebug);
                }
                return Material.SourceCode.mCoreObject;
            }
            else if (file.EndsWith("/MaterialVar"))
            {
                if (Material == null)
                    return (NxRHI.FShaderCode*)0;
                if (IsWriteDebugFile && Material.AssetName != null)
                {
                    var path = TtEngine.Instance.FileManager.GetPath(IO.TtFileManager.ERootDir.Cache, IO.TtFileManager.ESystemDir.DebugUtility) + $"/material/{Material.AssetName.RNameType}/{IO.TtFileManager.GetBaseDirectory(Material.AssetName.Name)}";
                    IO.TtFileManager.SureDirectory(path);
                    
                    IO.TtFileManager.WriteAllText(path + $"{Material.AssetName.PureName}_var.shader", Material.DefineCode.TextCode);
                }
                return Material.DefineCode.mCoreObject;
            }
            else if (file.EndsWith("/MdfQueue"))
            {
                var mdf = Rtti.TtTypeDescManager.CreateInstance(MdfQueueType) as Graphics.Pipeline.Shader.TtMdfQueueBase;
                if (mdf != null)
                {
                    if (IsWriteDebugFile)
                    {
                        var path = TtEngine.Instance.FileManager.GetPath(IO.TtFileManager.ERootDir.Cache, IO.TtFileManager.ESystemDir.DebugUtility) + $"/mdfqueue/";
                        IO.TtFileManager.SureDirectory(path);
                        IO.TtFileManager.WriteAllText(path + $"{mdf.GetType().FullName}.shader", mdf.SourceCode.TextCode);
                    }
                    return mdf.SourceCode.mCoreObject;
                }
                else
                    return (NxRHI.FShaderCode*)0;
            }
            else if (file.StartsWith("@"))
            {
                rn = RName.GetRName(file, RName.ERNameType.Engine);
            }
            else if (file.StartsWith(TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Engine)))
            {
                var path = IO.TtFileManager.GetRelativePath(TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Engine), file);
                rn = RName.GetRName(path, RName.ERNameType.Engine);
            }
            else if (file.StartsWith(TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Game)))
            {
                var path = IO.TtFileManager.GetRelativePath(TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Game), file);
                rn = RName.GetRName(path, RName.ERNameType.Game);
            }
            else
            {
                rn = RName.GetRName(file, RName.ERNameType.Engine);
            }
            if (rn != null)
            {
                var code = TtShaderCodeManager.Instance.GetShaderCodeProvider(rn);
                if (code != null)
                {
                    if (isVar)
                        return code.DefineCode.mCoreObject;
                    else
                        return code.SourceCode.mCoreObject;
                }
            }
            if (MdfQueue != null)
            {
                return MdfQueue.GetHLSLCode(file, oriInc);
            }
            return (NxRHI.FShaderCode*)0;
        }
        private TtHLSLInclude UserInclude;
        private Graphics.Pipeline.Shader.TtMaterial Material;
        public Graphics.Pipeline.Shader.TtMaterial GetMaterial()
        {
            return Material;
        }
        private Rtti.TtTypeDesc MdfQueueType;
        private string GetVertexStreamDefine(NxRHI.EVertexStreamType type)
        {
            switch (type)
            {
                case NxRHI.EVertexStreamType.VST_Position:
                    return "USE_VS_Position";
                case NxRHI.EVertexStreamType.VST_Normal:
                    return "USE_VS_Normal";
                case NxRHI.EVertexStreamType.VST_Tangent:
                    return "USE_VS_Tangent";
                case NxRHI.EVertexStreamType.VST_Color:
                    return "USE_VS_Color";
                case NxRHI.EVertexStreamType.VST_UV:
                    return "USE_VS_UV";
                case NxRHI.EVertexStreamType.VST_ExtraUV:
                    return "USE_VS_ExtraUV";
                case NxRHI.EVertexStreamType.VST_SkinIndex:
                    return "USE_VS_SkinIndex";
                case NxRHI.EVertexStreamType.VST_SkinWeight:
                    return "USE_VS_SkinWeight";
                case NxRHI.EVertexStreamType.VST_TerrainIndex:
                    return "USE_VS_TerrainIndex";
                case NxRHI.EVertexStreamType.VST_TerrainGradient:
                    return "USE_VS_TerrainGradient";
                case NxRHI.EVertexStreamType.VST_InstPos:
                    return "USE_VS_InstPos";
                case NxRHI.EVertexStreamType.VST_InstQuat:
                    return "USE_VS_InstQuat";
                case NxRHI.EVertexStreamType.VST_InstScale:
                    return "USE_VS_InstScale";
                case NxRHI.EVertexStreamType.VST_F4_1:
                    return "USE_VS_F4_1";
                case NxRHI.EVertexStreamType.VST_F4_2:
                    return "USE_VS_F4_2";
                case NxRHI.EVertexStreamType.VST_F4_3:
                    return "USE_VS_F4_3";
            }
            return null;
        }
        private string GetPixelInputDefine(Graphics.Pipeline.Shader.EPixelShaderInput type)
        {
            switch(type)
            {
                case Graphics.Pipeline.Shader.EPixelShaderInput.PST_Position:
                    return "USE_PS_Position";
                case Graphics.Pipeline.Shader.EPixelShaderInput.PST_Normal:
                    return "USE_PS_Normal";
                case Graphics.Pipeline.Shader.EPixelShaderInput.PST_Color:
                    return "USE_PS_Color";
                case Graphics.Pipeline.Shader.EPixelShaderInput.PST_UV:
                    return "USE_PS_UV";
                case Graphics.Pipeline.Shader.EPixelShaderInput.PST_WorldPos:
                    return "USE_PS_WorldPos";
                case Graphics.Pipeline.Shader.EPixelShaderInput.PST_Tangent:
                    return "USE_PS_Tangent";
                case Graphics.Pipeline.Shader.EPixelShaderInput.PST_ExtraUV:
                    return "USE_PS_ExtraUV";
                case Graphics.Pipeline.Shader.EPixelShaderInput.PST_Custom0:
                    return "USE_PS_Custom0";
                case Graphics.Pipeline.Shader.EPixelShaderInput.PST_Custom1:
                    return "USE_PS_Custom1";
                case Graphics.Pipeline.Shader.EPixelShaderInput.PST_Custom2:
                    return "USE_PS_Custom2";
                case Graphics.Pipeline.Shader.EPixelShaderInput.PST_Custom3:
                    return "USE_PS_Custom3";
                case Graphics.Pipeline.Shader.EPixelShaderInput.PST_Custom4:
                    return "USE_PS_Custom4";
                case Graphics.Pipeline.Shader.EPixelShaderInput.PST_F4_1:
                    return "USE_PS_F4_1";
                case Graphics.Pipeline.Shader.EPixelShaderInput.PST_F4_2:
                    return "USE_PS_F4_2";
                case Graphics.Pipeline.Shader.EPixelShaderInput.PST_F4_3:
                    return "USE_PS_F4_3";
                case Graphics.Pipeline.Shader.EPixelShaderInput.PST_SpecialData:
                    return "USE_PS_SpecialData";
                case Graphics.Pipeline.Shader.EPixelShaderInput.PST_InstanceID:
                    return "USE_PS_InstanceID";
            }
            return null;
        }
        public static string GetShaderCachePath()
        {
            var path = TtEngine.Instance.FileManager.GetPath(IO.TtFileManager.ERootDir.Cache, IO.TtFileManager.ESystemDir.Shader);
            IO.TtFileManager.SureDirectory(path);
            return path;
        }
        public static string GetShaderFileExtension(NxRHI.EShaderLanguage lang)
        {
            switch (lang)
            {
                case NxRHI.EShaderLanguage.SL_DXBC:
                    return ".dxbc";
                case NxRHI.EShaderLanguage.SL_DXIL:
                    return ".dxil";
                case NxRHI.EShaderLanguage.SL_SPIRV:
                    return ".spirv";
                case NxRHI.EShaderLanguage.SL_GLSL:
                    return ".glsl";
                case NxRHI.EShaderLanguage.SL_METAL:
                    return ".metal";
                default:
                    return ".bin";
            }
        }
        public static void SaveShaderToCache(NxRHI.TtShaderDesc desc, Hash160 hash)
        {
            var path = GetShaderCachePath();
            IO.TtFileManager.SureDirectory(path);
            var lang = TtEngine.Instance.Config.ShaderLanguage;
            var file = path + hash.ToString() + GetShaderFileExtension(lang);
            var xnd = new IO.TtXndHolder("TtShader", 0, 0);
            var node = xnd.RootNode.mCoreObject;
            unsafe
            {
                desc.mCoreObject.SaveXnd(node);
            }
            xnd.SaveXnd(file);
        }
        public static NxRHI.TtShaderDesc LoadShaderFromCache(Hash160 hash, NxRHI.EShaderType type)
        {
            var path = GetShaderCachePath();
            var lang = TtEngine.Instance.Config.ShaderLanguage;
            var file = path + hash.ToString() + GetShaderFileExtension(lang);
            if (!IO.TtFileManager.FileExists(file))
                return null;
            using (var xnd = IO.TtXndHolder.LoadXnd(file))
            {
                if (xnd == null)
                    return null;
                var desc = new NxRHI.TtShaderDesc(type);
                unsafe
                {
                    if (desc.mCoreObject.LoadXnd(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject, xnd.RootNode.mCoreObject) == false)
                        return null;
                }
                return desc;
            }
        }
        public static void DeleteShaderCache(Hash160 hash)
        {
            var path = GetShaderCachePath();
            var hashStr = hash.ToString();
            string[] extensions = { ".dxbc", ".dxil", ".spirv", ".glsl", ".metal" };
            foreach (var ext in extensions)
            {
                var file = path + hashStr + ext;
                if (IO.TtFileManager.FileExists(file))
                    IO.TtFileManager.DeleteFile(file);
            }
        }
        public unsafe NxRHI.TtShaderDesc CompileShader(string shader, string entry, NxRHI.EShaderType type,
            Graphics.Pipeline.Shader.TtShadingEnv shadingEnv, FPermutationId permutationId, Graphics.Pipeline.Shader.TtMaterial mtl, Type mdfType,
            NxRHI.TtShaderDefinitions defines, TtHLSLInclude incProvider, string sm = null, bool bDebugShader = true, string extHlslVersion = null, bool asModule = false)
        {
            var code_text = IO.TtFileManager.ReadAllText(shader);
            var metaIndex = code_text.IndexOf($"/**Meta Begin:({entry})");
            if (metaIndex >= 0)
            {
                code_text = code_text.Substring(metaIndex + $"/**Meta Begin:({entry})".Length);
                metaIndex = code_text.IndexOf($"Meta End:({entry})**/");
                if (metaIndex >= 0)
                {
                    code_text = code_text.Substring(0, metaIndex);
                    var lines = code_text.Split("\r\n");
                    foreach (var i in lines)
                    {
                        if (string.IsNullOrEmpty(i))
                            continue;
                        var pairs = i.Split('=');
                        if (pairs.Length == 2)
                        {
                            switch (pairs[0])
                            {
                                case "SM":
                                    sm = pairs[1];
                                    break;
                                case "HLSL":
                                    extHlslVersion = pairs[1];
                                    if (extHlslVersion == "none")
                                        extHlslVersion = null;
                                    break;
                            }
                        }
                    }
                }
            }
            var desc = new NxRHI.TtShaderDesc(type);
            var mtlName = "";
            if (mtl != null)
                mtlName = mtl.ToString();
            var mdfName = "";
            if (mdfType != null)
                mdfName = mdfType.FullName;
            desc.DebugName = $"{shader}:{entry}[id,{permutationId}][mtl,{mtlName}][mdf,{mdfName}]";
            desc.PermutationId = permutationId;
            UserInclude = incProvider;
            Material = mtl;
            MdfQueueType = Rtti.TtTypeDesc.TypeOf(mdfType);
            using (var defPtr = new NxRHI.TtShaderDefinitions())
            {
                if (defines != null)
                {
                    defPtr.MergeDefinitions(defines);
                }
                if (mtl != null && mtl.Defines != null)
                {
                    defPtr.MergeDefinitions(mtl.Defines);
                    var vsStreams = mtl.GetVSNeedStreams();
                    if (vsStreams != null)
                    {
                        foreach (var i in vsStreams)
                        {
                            defPtr.AddDefine(GetVertexStreamDefine(i), "1");
                        }
                    }
                    var psInputs = mtl.GetPSNeedInputs();
                    if (psInputs != null)
                    {
                        foreach (var i in psInputs)
                        {
                            defPtr.AddDefine(GetPixelInputDefine(i), "1");
                        }
                    }
                }
                var graphicsEnv = shadingEnv as Graphics.Pipeline.Shader.TtGraphicsShadingEnv;
                if (graphicsEnv != null)
                {
                    {
                        var shadingNeeds = graphicsEnv.GetNeedStreams();
                        if (shadingNeeds != null)
                        {
                            foreach (var i in shadingNeeds)
                            {
                                defPtr.AddDefine(GetVertexStreamDefine(i), "1");
                            }
                        }
                        var shadingPSNeeds = graphicsEnv.GetPSNeedInputs();
                        if (shadingPSNeeds != null)
                        {
                            foreach (var i in shadingPSNeeds)
                            {
                                defPtr.AddDefine(GetPixelInputDefine(i), "1");
                            }
                        }
                    }
                    {
                        var mdfObj = Rtti.TtTypeDescManager.CreateInstance(MdfQueueType) as Graphics.Pipeline.Shader.TtMdfQueueBase;
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
                            var mdfPSNeeds = mdfObj.GetPSNeedInputs();
                            if (mdfPSNeeds != null)
                            {
                                foreach (var i in mdfPSNeeds)
                                {
                                    defPtr.AddDefine(GetPixelInputDefine(i), "1");
                                }
                            }
                        }
                    }
                }
                else
                {
                    var computeEnv = shadingEnv as Graphics.Pipeline.Shader.TtComputeShadingEnv;
                    if (computeEnv != null)
                    {
                        defPtr.AddDefine("DispatchX", $"{computeEnv.DispatchArg.X}");
                        defPtr.AddDefine("DispatchY", $"{computeEnv.DispatchArg.Y}");
                        defPtr.AddDefine("DispatchZ", $"{computeEnv.DispatchArg.Z}");
                    }
                }
                switch (type)
                {
                    case NxRHI.EShaderType.SDT_AmplificationShader:
                        defPtr.AddDefine("ShaderStage", "0");
                        break;
                    case NxRHI.EShaderType.SDT_MeshShader:
                        defPtr.AddDefine("ShaderStage", "0");
                        break;
                    case NxRHI.EShaderType.SDT_VertexShader:
                        defPtr.AddDefine("ShaderStage", "0");
                        break;
                    case NxRHI.EShaderType.SDT_PixelShader:
                        defPtr.AddDefine("ShaderStage", "1");
                        break;
                    case NxRHI.EShaderType.SDT_ComputeShader:
                        defPtr.AddDefine("ShaderStage", "0");
                        break;
                    case NxRHI.EShaderType.SDT_RayTracing:
                        defPtr.AddDefine("ShaderStage", "0");
                        extHlslVersion = "2021";
                        asModule = true;
                        break;
                    default:
                        System.Diagnostics.Debugger.Break();
                        break;
                }

                defPtr.MergeDefinitions(TtEngine.Instance.GfxDevice.RenderContext.GlobalEnvDefines);

                var cfg = TtEngine.Instance.Config;
                int CP_SM_major = cfg.ShaderModelMajor;
                int CP_SM_minor = cfg.ShaderModelMinor;
                if (sm != null)
                {
                    var segs = sm.Split('_');
                    if (segs.Length == 2)
                    {
                        CP_SM_major = int.Parse(segs[0]);
                        CP_SM_minor = int.Parse(segs[1]);
                    }
                }
                var compile_sm = $"{CP_SM_major}_{CP_SM_minor}";

                var shaderLang = cfg.ShaderLanguage;
                bool compiled = false;
                if (shaderLang == NxRHI.EShaderLanguage.SL_DXBC)
                {
                    if (type == NxRHI.EShaderType.SDT_MeshShader || type == NxRHI.EShaderType.SDT_AmplificationShader)
                        return null;
                    defPtr.AddDefine("RHI_TYPE", "RHI_DX11");
                    defPtr.AddDefine("CP_SM_major", "5");
                    defPtr.AddDefine("CP_SM_minor", "0");
                    compiled = mShaderCompiler.CompileShader(this, desc, shader, entry, type, "5_0", defPtr, NxRHI.EShaderLanguage.SL_DXBC, bDebugShader, extHlslVersion, null, asModule);
                }
                else if (shaderLang == NxRHI.EShaderLanguage.SL_DXIL)
                {
                    if (extHlslVersion == null)
                        extHlslVersion = "2021";
                    defPtr.AddDefine("RHI_TYPE", "RHI_DX12");
                    defPtr.AddDefine("CP_SM_major", CP_SM_major.ToString());
                    defPtr.AddDefine("CP_SM_minor", CP_SM_minor.ToString());
                    defPtr.AddDefine("HLSL_VERSION", extHlslVersion);
                    compiled = mShaderCompiler.CompileShader(this, desc, shader, entry, type, compile_sm, defPtr, NxRHI.EShaderLanguage.SL_DXIL, bDebugShader, extHlslVersion, null, asModule);
                }
                else if (shaderLang == NxRHI.EShaderLanguage.SL_SPIRV)
                {
                    if (extHlslVersion == null)
                        extHlslVersion = "2021";
                    defPtr.AddDefine("RHI_TYPE", "RHI_VK");
                    defPtr.AddDefine("CP_SM_major", CP_SM_major.ToString());
                    defPtr.AddDefine("CP_SM_minor", CP_SM_minor.ToString());
                    defPtr.AddDefine("HLSL_VERSION", extHlslVersion);
                    compiled = mShaderCompiler.CompileShader(this, desc, shader, entry, type, compile_sm, defPtr, NxRHI.EShaderLanguage.SL_SPIRV, bDebugShader, extHlslVersion, null, asModule);
                }
                else if (shaderLang == NxRHI.EShaderLanguage.SL_GLSL)
                {
                    if (extHlslVersion == null)
                        extHlslVersion = "2021";
                    defPtr.AddDefine("RHI_TYPE", "RHI_GL");
                    defPtr.AddDefine("CP_SM_major", CP_SM_major.ToString());
                    defPtr.AddDefine("CP_SM_minor", CP_SM_minor.ToString());
                    defPtr.AddDefine("HLSL_VERSION", extHlslVersion);
                    compiled = mShaderCompiler.CompileShader(this, desc, shader, entry, type, compile_sm, defPtr, NxRHI.EShaderLanguage.SL_GLSL, bDebugShader, extHlslVersion, null, asModule);
                }
                else if (shaderLang == NxRHI.EShaderLanguage.SL_METAL)
                {
                    if (extHlslVersion == null)
                        extHlslVersion = "2021";
                    defPtr.AddDefine("RHI_TYPE", "RHI_MTL");
                    defPtr.AddDefine("CP_SM_major", CP_SM_major.ToString());
                    defPtr.AddDefine("CP_SM_minor", CP_SM_minor.ToString());
                    defPtr.AddDefine("HLSL_VERSION", extHlslVersion);
                    compiled = mShaderCompiler.CompileShader(this, desc, shader, entry, type, compile_sm, defPtr, NxRHI.EShaderLanguage.SL_METAL, bDebugShader, extHlslVersion, null, asModule);
                }

                if (!compiled)
                    return null;

                // Compute hash from compiled binary data
                desc.ComputeRhiDataHash();
                if (shadingEnv.IsOnlyBuildMode == false)
                {
                    SaveShaderToCache(desc, desc.RhiDataHash);
                }
                return desc;
            }
        }
    }
}
