using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using EngineNS.Editor;
using System.Text.RegularExpressions;

namespace EngineNS
{
    public struct CRenderContextCaps
    {
        public void SetDefault()
        {
            ShaderModel = 5;
            MaxShaderStorageBlockSize = 1024 * 1024 * 4;
            MaxShaderStorageBufferBindings = 16;
            ShaderStorageBufferOffsetAlignment = 16;
            MaxVertexShaderStorageBlocks = 16;
            MaxPixelShaderStorageBlocks = 16;
            MaxComputeShaderStorageBlocks = 16;
            MaxVertexUniformBlocks = 16;
            MaxPixelUniformBlocks = 16;
            MaxUniformBufferBindings = 16;
            MaxUniformBlockSize = 65536;
            MaxTextureBufferSize = 65536;
            MaxVertexTextureImageUnits = 16;
            MaxCombinedTextureImageUnits = 16;
            SupportFloatRT = 1;
            SupportHalfRT = 1;
            SupportFloatTexture = 1;
            SupportHalfTexture = 1;
        }
        public int ShaderModel;
        public int MaxShaderStorageBlockSize;
        public int MaxShaderStorageBufferBindings;
        public int ShaderStorageBufferOffsetAlignment;
        public int MaxVertexShaderStorageBlocks;
        public int MaxPixelShaderStorageBlocks;
        public int MaxComputeShaderStorageBlocks;
        public int MaxVertexUniformBlocks;
        public int MaxPixelUniformBlocks;
        public int MaxUniformBufferBindings;
        public int MaxUniformBlockSize;
        public int MaxTextureBufferSize;
        public int MaxVertexTextureImageUnits;
        public int MaxCombinedTextureImageUnits;
        public int SupportFloatRT;
        public int SupportHalfRT;
        public int SupportFloatTexture;
        public int SupportHalfTexture;
    }
    public partial class CRenderContext : AuxCoreObject<CRenderContext.NativePointer>
    {
        public struct NativePointer : INativePointer
        {
            public IntPtr Pointer;
            public IntPtr GetPointer()
            {
                return Pointer;
            }
            public void SetPointer(IntPtr ptr)
            {
                Pointer = ptr;
            }
            public override string ToString()
            {
                return "0x" + Pointer.ToString("x");
            }
        }
        public CRenderContext(NativePointer self)
        {
            mCoreObject = self;

            var ptr = SDK_IRenderContext_GetImmCommandList(CoreObject);
            mImmCommandList = new CCommandList(ptr);
            mImmCommandList.Core_AddRef();
#if PWindow
            SDK_CscShaderConductor_SetTranslateCB(_OnShaderTranslated);
#endif

            //if(ShaderModel > SDK_IRenderContext_GetShaderModel(CoreObject))
            //{
            //    Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Graphics", "选择的ShaderModel比GPU能提供的大，可能会有未知错误");
            //}
        }
        public static Profiler.TimeScope ScopeTickSwapChain = Profiler.TimeScopeManager.GetTimeScope(typeof(CEngine), "TickSwapChain");
        public void FlushImmContext()
        {
            lock (ImmCommandList)
            {
                SDK_IRenderContext_FlushImmContext(CoreObject);

                ScopeTickSwapChain.Begin();
                CEngine.SDK_RResourceSwapChain_TickSwap(CoreObject);
                ScopeTickSwapChain.End();
            }
        }
        private CCommandList mImmCommandList;
        public CCommandList ImmCommandList
        {
            get { return mImmCommandList; }
        }
        public int DeviceShaderModel
        {
            get
            {
                return SDK_IRenderContext_GetShaderModel(CoreObject);
            }
        }
        static int mShaderModel = 5;
        public static int ShaderModel
        {
            get { return mShaderModel; }
            set
            {
                mShaderModel = value;
                SDK_IRenderContext_ChooseShaderModel(value);
                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Graphics", $"App choose ShaderMode = {value}");
            }
        }
        public static string ShaderModelString
        {
            get
            {
                if (CIPlatform.Instance.PlayMode == CIPlatform.enPlayMode.Editor || CIPlatform.Instance.PlayMode == CIPlatform.enPlayMode.PlayerInEditor)
                    return "shader";
                return "sm" + ShaderModel;
            }
        }
        static bool bTestSlot = false;
        static FOnShaderTranslated _OnShaderTranslated = OnShaderTranslated;
        private static void OnShaderTranslated(CShaderDesc.NativePointer coreObj)
        {
            var desc = new CShaderDesc(coreObj);
            desc.Core_AddRef();

            if(bTestSlot)
            {
                string pattern = "layout\\(binding = ([0-9]+), std140\\)";
                // @"(?<=19)\d{2}\b";

                foreach (Match match in Regex.Matches(desc.GLCode, pattern))
                {
                    int slot = System.Convert.ToInt32(match.Groups[1].Value);
                    if (slot > 16)
                        Console.WriteLine(match.Value);
                }
            }

            ////var reg = new System.Text.RegularExpressions.Regex("^(-?\\d+)(\\.\\d+)?$hf");
            //var reg = new System.Text.RegularExpressions.Regex("[0-9]*\\.[0-9]+([Ee][+\\-]?[0-9]+)?hf");
            //var srcCode = desc.GLCode;
            //var result = reg.Matches(srcCode);
            //var hfNums = new List<string>();
            //foreach (System.Text.RegularExpressions.Match i in result)
            //{
            //    var src = i.ToString();
            //    hfNums.Add(src);
            //}
            //if (hfNums.Count > 0)
            //{
            //    foreach (var i in hfNums)
            //    {
            //        var tar = i.Substring(0, i.Length - 2) + 'f';
            //        srcCode = srcCode.Replace(i, tar);
            //    }
            //    desc.SetGLCode(srcCode);
            //}
            if (desc.GLCode.Contains("precision mediump float;"))
            {
                var code = desc.GLCode.Replace("precision mediump float;", "precision highp float;");
                desc.SetGLCode(code);
            }
            if (desc.GLCode.Contains("#extension GL_NV_gpu_shader5 : require"))
            {
                const string GLES_Define = "#extension GL_NV_gpu_shader5 : require\r\n" +
                                           "#define f16mat2 mat2\r\n" +
                                           "#define f16mat3 mat3\r\n" +
                                           "#define f16mat4 mat4\r\n";
                var code = desc.GLCode.Replace("#extension GL_NV_gpu_shader5 : require", GLES_Define);
                desc.SetGLCode(code);
            }
            if (desc.GLCode.Contains("#else\n#error No extension available for FP16."))
            {
                const string GLES_Define = "#elif defined(EXT_shader_16bit_storage)\r\n" +
                                           "#extension EXT_shader_16bit_storage : require\r\n" +
                                           "#define f16mat2 mat2\r\n" +
                                           "#define f16mat3 mat3\r\n" +
                                           "#define f16mat4 mat4\r\n" +
                                           "#else\r\n" +
                                           "#define float16_t float\r\n" +
                                           "#define f16vec2 vec2\r\n" +
                                           "#define f16vec3 vec3\r\n" +
                                           "#define f16vec4 vec4\r\n" +
                                           "#define float16_t float\r\n" +
                                           "#define f16mat2 mat2\r\n" +
                                           "#define f16mat3 mat3\r\n" +
                                           "#define f16mat4 mat4\r\n";

                var code = desc.GLCode.Replace("#else\n#error No extension available for FP16.", GLES_Define);
                desc.SetGLCode(code);
            }
            if (desc.GLCode.Contains("#else\n#error No extension available for Int16."))
            {
                const string GLES_Define = "#else\r\n" +
                                           "#define int16_t int\r\n" +
                                           "#define uint16_t uint\r\n";

                var code = desc.GLCode.Replace("#else\n#error No extension available for Int16.", GLES_Define);
                desc.SetGLCode(code);
            }
            
            if(desc.ShaderType == EShaderType.EST_VertexShader)
            {
                if (desc.GLCode.IndexOf("#extension") < 0)
                {
                    if (desc.GLCode.StartsWith("#version 310 es"))
                    {
                        const string GLES_DefaultPrecision = "#version 310 es\n" + "precision highp float;\n";
                        var code = desc.GLCode.Replace("#version 310 es", GLES_DefaultPrecision);
                        desc.SetGLCode(code);
                    }
                    else if (desc.GLCode.StartsWith("#version 300 es"))
                    {
                        const string GLES_DefaultPrecision = "#version 300 es\n" + "precision highp float;\n";
                        var code = desc.GLCode.Replace("#version 300 es", GLES_DefaultPrecision);
                        desc.SetGLCode(code);
                    }
                }
                else
                {
                    const string GLES_DefaultPrecision = "precision highp float;\n" + "void main()";
                    var code = desc.GLCode.Replace("void main()", GLES_DefaultPrecision);
                    desc.SetGLCode(code);
                }
            }
        }
        public CPass CreatePass()
        {
            var obj = SDK_IRenderContext_CreatePass(CoreObject);
            if (obj.Pointer == IntPtr.Zero)
                return null;
            return new CPass(obj);
        }
        public CSwapChain CreateSwapChain(CSwapChainDesc desc)
        {
            unsafe
            {
                var sc = SDK_IRenderContext_CreateSwapChain(CoreObject, &desc);
                if (sc.Pointer == IntPtr.Zero)
                    return null;
                return new CSwapChain(sc);
            }
        }
        //public void BindCurrentSwapChain(CSwapChain swapChain)
        //{
        //    SDK_IRenderContext_BindCurrentSwapChain(CoreObject, swapChain.CoreObject);
        //}

        //public static Profiler.TimeScope ScopePresent = Profiler.TimeScopeManager.GetTimeScope(typeof(CRenderContext), nameof(Present));
        //public void Present(UInt32 SyncInterval, UInt32 Flags)
        //{
        //    ScopePresent.Begin();
        //    SDK_IRenderContext_Present(CoreObject, SyncInterval, Flags);
        //    ScopePresent.End();
        //}

        public CShaderResourceView LoadShaderResourceView(RName name)
        {
            unsafe
            {
                var obj = SDK_IRenderContext_LoadShaderResourceView(CoreObject, name.Address);
                if (obj.Pointer == IntPtr.Zero)
                    return null;
                var rsv = new CShaderResourceView(obj);
                rsv.Name = name;
                return rsv;
            }
        }
        public CTexture2D CreateTexture2D(CTexture2DDesc desc)
        {
            unsafe
            {
                var obj = SDK_IRenderContext_CreateTexture2D(CoreObject, &desc);
                if (obj.Pointer == IntPtr.Zero)
                    return null;
                return new CTexture2D(obj);
            }
        }
        public CRenderTargetView CreateRenderTargetView(CRenderTargetViewDesc desc)
        {
            unsafe
            {
                var obj = SDK_IRenderContext_CreateRenderTargetView(CoreObject, &desc);
                if (obj.Pointer == IntPtr.Zero)
                    return null;
                return new CRenderTargetView(obj);
            }
        }
        public CDepthStencilView CreateDepthStencilView(CDepthStencilViewDesc desc)
        {
            unsafe
            {
                var obj = SDK_IRenderContext_CreateDepthRenderTargetView(CoreObject, &desc);
                if (obj.Pointer == IntPtr.Zero)
                    return null;
                return new CDepthStencilView(obj);
            }
        }
        public CShaderResourceView CreateShaderResourceView(CShaderResourceViewDesc desc)
        {
            unsafe
            {
                var obj = SDK_IRenderContext_CreateShaderResourceView(CoreObject, &desc);
                if (obj.Pointer == IntPtr.Zero)
                    return null;
                return new CShaderResourceView(obj);
            }
        }
        public CGpuBuffer CreateGpuBuffer(CGpuBufferDesc desc, IntPtr pInitData)
        {
            unsafe
            {
                var obj = SDK_IRenderContext_CreateGpuBuffer(CoreObject, &desc, pInitData.ToPointer());
                if (obj.Pointer == IntPtr.Zero)
                    return null;
                return new CGpuBuffer(obj);
            }
        }
        public CShaderResourceView CreateShaderResourceViewFromBuffer(CGpuBuffer pBuffer, ISRVDesc desc)
        {
            unsafe
            {
                var obj = SDK_IRenderContext_CreateShaderResourceViewFromBuffer(CoreObject, pBuffer.CoreObject, &desc);
                if (obj.Pointer == IntPtr.Zero)
                    return null;
                return new CShaderResourceView(obj);
            }
        }
        public CUnorderedAccessView CreateUnorderedAccessView(CGpuBuffer pBuffer, CUnorderedAccessViewDesc desc)
        {
            unsafe
            {
                var obj = SDK_IRenderContext_CreateUnorderedAccessView(CoreObject, pBuffer.CoreObject, &desc);
                if (obj.Pointer == IntPtr.Zero)
                    return null;
                return new CUnorderedAccessView(obj);
            }
        }
        public CFrameBuffer CreateFrameBuffers(CFrameBuffersDesc desc)
        {
            unsafe
            {
                var obj = SDK_IRenderContext_CreateFrameBuffers(CoreObject, &desc);
                if (obj.Pointer == IntPtr.Zero)
                    return null;
                return new CFrameBuffer(obj);
            }
        }
        public CCommandList CreateCommandList(CCommandListDesc desc)
        {
            unsafe
            {
                var obj = SDK_IRenderContext_CreateCommandList(CoreObject, &desc);
                if (obj.Pointer == IntPtr.Zero)
                    return null;
                return new CCommandList(obj);
            }
        }
        internal static string GetShaderFile(ref Hash64 hash)
        {
            var sm = CRenderContext.ShaderModelString;
            string path = "";
            switch (CIPlatform.Instance.PlayMode)
            {
                case CIPlatform.enPlayMode.Cook:
                    path = CEngine.Instance.FileManager.CookingRoot + "deriveddatacache/" + sm + "/" + hash.ToString().ToLower() + ".shader";
                    break;
                case CIPlatform.enPlayMode.Game:
                case CIPlatform.enPlayMode.Editor:
                case CIPlatform.enPlayMode.PlayerInEditor:
                    path = CEngine.Instance.FileManager.DDCDirectory + sm + "/" + hash.ToString().ToLower() + ".shader";
                    break;
            }
            return path;
        }
        public void CookShaderDesc(RName shader, string entry, EShaderType type, CShaderDefinitions defines, int ShaderModel, EPlatformType platforms)
        {
            string cachedName = "";
            string sm = "vs_5_0";
            var shaderHash = UniHash.APHash(shader.Name + entry + defines.ToString());
            var macro = defines.GetHash64().ToString();
            switch (type)
            {
                case EShaderType.EST_ComputeShader:
                    cachedName = $"cs/{shaderHash}_{macro}.cshader";
                    sm = "cs_5_0";
                    if (ShaderModel == 3)
                        return;
                    break;
                case EShaderType.EST_VertexShader:
                    cachedName = $"vs/{shaderHash}_{macro}.vshader";
                    sm = "vs_5_0";
                    break;
                case EShaderType.EST_PixelShader:
                    cachedName = $"ps/{shaderHash}_{macro}.pshader";
                    sm = "ps_5_0";
                    break;
            }
            var shaderFile = "";
            switch (CIPlatform.Instance.PlayMode)
            {
                case CIPlatform.enPlayMode.Cook:
                    shaderFile = CEngine.Instance.FileManager.CookingRoot + "deriveddatacache/" + $"sm{ShaderModel}/" + cachedName;
                    break;
                case CIPlatform.enPlayMode.Game:
                case CIPlatform.enPlayMode.Editor:
                case CIPlatform.enPlayMode.PlayerInEditor:
                    shaderFile = CEngine.Instance.FileManager.DDCDirectory + $"sm{ShaderModel}/" + cachedName;
                    break;
            }
            using (var xnd = IO.XndHolder.SyncLoadXND(shaderFile))
            {
                if (xnd != null)
                {
                    bool needCompile = false;
                    var fileDesc = CEngine.Instance.FileManager.HLSLFileDescManager.FindFileDesc(shader.Address);
                    if (fileDesc != null)
                    {
                        var hashAttr = xnd.Node.FindAttrib("HashCode");
                        if (hashAttr != null)
                        {
                            hashAttr.BeginRead();
                            string savedHash;
                            hashAttr.Read(out savedHash);
                            hashAttr.EndRead();
                            if (savedHash != fileDesc.HashCode.ToString())
                            {
                                needCompile = true;
                            }
                        }
                    }
                    if (needCompile == false)
                    {
                        CShaderDesc desc = new CShaderDesc(type);
                        desc.LoadXnd(xnd.Node);
                        return;
                    }
                }

                {   
                    var desc = CompileHLSLFromFile(shader.Address, entry, sm, defines, platforms);
                    var saved = IO.XndHolder.NewXNDHolder();

                    var fileDesc = CEngine.Instance.FileManager.HLSLFileDescManager.FindFileDesc(shader.Address);
                    if (fileDesc != null)
                    {
                        var hashAttr = saved.Node.AddAttrib("HashCode");
                        hashAttr.BeginWrite();
                        hashAttr.Write(fileDesc.HashCode.ToString());
                        hashAttr.EndWrite();
                        desc.Save2Xnd(saved.Node, platforms);
                    }

                    IO.XndHolder.SaveXND(shaderFile, saved);
                }
            }
        }
        public CShaderDesc CreateShaderDesc(RName shader, string entry, EShaderType type, CShaderDefinitions defines, EPlatformType platforms)
        {
            string cachedName = "";
            string sm = "vs_5_0";
            var shaderHash = UniHash.APHash(shader.Name + entry + defines.ToString());
            var macro = defines.GetHash64().ToString();
            switch (type)
            {
                case EShaderType.EST_ComputeShader:
                    cachedName = $"cs/{shaderHash}_{macro}.cshader";
                    sm = "cs_5_0";
                    if(ShaderModel==3)
                    {
                        return null;
                    }
                    break;
                case EShaderType.EST_VertexShader:
                    cachedName = $"vs/{shaderHash}_{macro}.vshader";
                    sm = "vs_5_0";
                    break;
                case EShaderType.EST_PixelShader:
                    cachedName = $"ps/{shaderHash}_{macro}.pshader";
                    sm = "ps_5_0";
                    break;
            }
            var smStr = CRenderContext.ShaderModelString;
            var shaderFile = CEngine.Instance.FileManager.DDCDirectory + smStr + "/" + cachedName;
            using (var xnd = IO.XndHolder.SyncLoadXND(shaderFile))
            {
                if (xnd != null)
                {
                    bool needCompile = false;
                    var fileDesc = CEngine.Instance.FileManager.HLSLFileDescManager.FindFileDesc(shader.Address);
                    if(fileDesc!=null)
                    {
                        var hashAttr = xnd.Node.FindAttrib("HashCode");
                        if (hashAttr != null)
                        {
                            hashAttr.BeginRead();
                            string savedHash;
                            hashAttr.Read(out savedHash);
                            hashAttr.EndRead();
                            if (savedHash != fileDesc.HashCode.ToString())
                            {
                                needCompile = true;
                            }
                        }
                    }
                    if (needCompile == false)
                    {
                        CShaderDesc desc = new CShaderDesc(type);
                        desc.LoadXnd(xnd.Node);
                        return desc;
                    }
                }

                {
                    var xml = IO.XmlHolder.NewXMLHolder($"{type.ToString()}", "");
                    xml.RootNode.AddAttrib("Shader", shader.ToString());
                    xml.RootNode.AddAttrib("Entry", entry);
                    var node = xml.RootNode.AddNode("Macro", "", xml);
                    foreach (var i in defines.mShaderMacroArray)
                    {
                        node.AddAttrib(i.Name, i.Definition);
                    }
                    var fileDesc = CEngine.Instance.FileManager.HLSLFileDescManager.FindFileDesc(shader.Address);
                    if(fileDesc!=null)
                    {
                        xml.RootNode.AddAttrib("HashCode", fileDesc.HashCode.ToString());
                    }

                    var shaderDescFile = CEngine.Instance.FileManager.DDCDirectory + "shaderinfo/" + cachedName + ".shaderxml";
                    IO.XmlHolder.SaveXML(shaderDescFile, xml);

                    var desc = CompileHLSLFromFile(shader.Address, entry, sm, defines, platforms);
                    var saved = IO.XndHolder.NewXNDHolder();

                    if (fileDesc != null)
                    {
                        var hashAttr = saved.Node.AddAttrib("HashCode");
                        hashAttr.BeginWrite();
                        hashAttr.Write(fileDesc.HashCode.ToString());
                        hashAttr.EndWrite();
                    }

                    desc.Save2Xnd(saved.Node, platforms);

                    IO.XndHolder.SaveXND(shaderFile, saved);

                    return desc;
                }
            }
        }
        public static bool IsDebugHLSL = false;
        public CShaderDesc CompileHLSLFromFile(string file, string entry, string sm, CShaderDefinitions defines, EPlatformType platforms)
        {
            //defines.SetDefine("ShaderModel", CEngine.Instance.RenderContext.ShaderModel.ToString());
            defines.SetDefine("ShaderModel", CRenderContext.ShaderModel.ToString());

            unsafe
            {
                int TempIsCrossPlatform = CEngine.mGenerateShaderForMobilePlatform == true ? 1 : 0;

                CShaderDesc.NativePointer obj;
                if (defines == null)
                {
                    obj = SDK_IRenderContext_CompileHLSLFromFile(CoreObject, file, entry, sm, CShaderDefinitions.GetEmptyNativePointer(), (UInt32)platforms, vBOOL.FromBoolean(IsDebugHLSL));
                }
                else
                {
                    obj = SDK_IRenderContext_CompileHLSLFromFile(CoreObject, file, entry, sm, defines.CoreObject, (UInt32)platforms, vBOOL.FromBoolean(IsDebugHLSL));
                }

                defines.RemoveDefine("ShaderModel");
                if (obj.Pointer == IntPtr.Zero)
                    return null;
                return new CShaderDesc(obj);
            }
        }

        public CVertexShader CreateVertexShader(CShaderDesc desc)
        {
            unsafe
            {
                var obj = SDK_IRenderContext_CreateVertexShader(CoreObject, desc.CoreObject);
                if (obj.Pointer == IntPtr.Zero)
                    return null;
                return new CVertexShader(obj);
            }
        }
        public CPixelShader CreatePixelShader(CShaderDesc desc)
        {
            unsafe
            {
                var obj = SDK_IRenderContext_CreatePixelShader(CoreObject, desc.CoreObject);
                if (obj.Pointer == IntPtr.Zero)
                    return null;
                return new CPixelShader(obj);
            }
        }
        public CComputeShader CreateComputeShader(CShaderDesc desc)
        {
            unsafe
            {
                var obj = SDK_IRenderContext_CreateComputeShader(CoreObject, desc.CoreObject);
                if (obj.Pointer == IntPtr.Zero)
                    return null;
                return new CComputeShader(obj);
            }
        }
        public CShaderProgram CreateShaderProgram(CShaderProgramDesc desc)
        {
            unsafe
            {
                var obj = SDK_IRenderContext_CreateShaderProgram(CoreObject, &desc);
                if (obj.Pointer == IntPtr.Zero)
                    return null;
                return new CShaderProgram(obj);
            }
        }
        public CInputLayout CreateInputLayout(CInputLayoutDesc desc)
        {
            var obj = SDK_IRenderContext_CreateInputLayout(CoreObject, desc.CoreObject);
            if (obj.Pointer == IntPtr.Zero)
                return null;
            return new CInputLayout(obj);
        }
        public CRenderPipeline CreateRenderPipeline(CRenderPipelineDesc desc)
        {
            unsafe
            {
                var obj = SDK_IRenderContext_CreateRenderPipeline(CoreObject, &desc);
                if (obj.Pointer == IntPtr.Zero)
                    return null;
                return new CRenderPipeline(obj);
            }
        }
        public CGeometryMesh CreateGeometryMesh()
        {
            var obj = SDK_IRenderContext_CreateGeometryMesh(CoreObject);
            if (obj.Pointer == IntPtr.Zero)
                return null;
            return new CGeometryMesh(obj);
        }
        public CVertexBuffer CreateVertexBuffer(CVertexBufferDesc desc)
        {
            unsafe
            {
                var obj = SDK_IRenderContext_CreateVertexBuffer(CoreObject, &desc);
                if (obj.Pointer == IntPtr.Zero)
                    return null;
                return new CVertexBuffer(obj);
            }
        }
        public CIndexBuffer CreateIndexBuffer(CIndexBufferDesc desc)
        {
            unsafe
            {
                var obj = SDK_IRenderContext_CreateIndexBuffer(CoreObject, &desc);
                if (obj.Pointer == IntPtr.Zero)
                    return null;
                return new CIndexBuffer(obj);
            }
        }
        public CIndexBuffer CreateIndexBufferFromBuffer(CIndexBufferDesc desc, CGpuBuffer pBuffer)
        {
            unsafe
            {
                var obj = SDK_IRenderContext_CreateIndexBufferFromBuffer(CoreObject, &desc, pBuffer.CoreObject);
                if (obj.Pointer == IntPtr.Zero)
                    return null;
                return new CIndexBuffer(obj);
            }
        }
        public CVertexBuffer CreateVertexBufferFromBuffer(CVertexBufferDesc desc, CGpuBuffer pBuffer)
        {
            unsafe
            {
                var obj = SDK_IRenderContext_CreateVertexBufferFromBuffer(CoreObject, &desc, pBuffer.CoreObject);
                if (obj.Pointer == IntPtr.Zero)
                    return null;
                return new CVertexBuffer(obj);
            }
        }
        public CSamplerState CreateSamplerState(CSamplerStateDesc desc)
        {
            unsafe
            {
                var obj = SDK_IRenderContext_CreateSamplerState(CoreObject, &desc);
                if (obj.Pointer == IntPtr.Zero)
                    return null;
                return new CSamplerState(obj);
            }
        }
        public CRasterizerState CreateRasterizerState(CRasterizerStateDesc desc)
        {
            unsafe
            {
                var obj = SDK_IRenderContext_CreateRasterizerState(CoreObject, &desc);
                if (obj.Pointer == IntPtr.Zero)
                    return null;
                return new CRasterizerState(obj);
            }
        }
        public CDepthStencilState CreateDepthStencilState(CDepthStencilStateDesc desc)
        {
            unsafe
            {
                var obj = SDK_IRenderContext_CreateDepthStencilState(CoreObject, &desc);
                if (obj.Pointer == IntPtr.Zero)
                    return null;
                return new CDepthStencilState(obj);
            }
        }
        public CBlendState CreateBlendState(CBlendStateDesc desc)
        {
            unsafe
            {
                var obj = SDK_IRenderContext_CreateBlendState(CoreObject, &desc);
                if (obj.Pointer == IntPtr.Zero)
                    return null;
                return new CBlendState(obj);
            }
        }
        public CConstantBuffer CreateConstantBuffer(CShaderProgram program, UInt32 index)
        {
            var obj = SDK_IRenderContext_CreateConstantBuffer(CoreObject, program.CoreObject, (int)index);
            if (obj.Pointer == IntPtr.Zero)
                return null;
            return new CConstantBuffer(obj);
        }
        public CConstantBuffer CreateConstantBuffer(CShaderDesc desc, UInt32 index)
        {
            var obj = SDK_IRenderContext_CreateConstantBuffer2(CoreObject, desc.CoreObject, (int)index);
            if (obj.Pointer == IntPtr.Zero)
                return null;
            return new CConstantBuffer(obj);
        }
        public CRenderContextCaps ContextCaps
        {
            get
            {
                CRenderContextCaps result = new CRenderContextCaps();
                unsafe
                {
                    SDK_IRenderContext_GetRenderContextCaps(CoreObject, &result);
                }
                return result;
            }
        }
        public unsafe void UnsafeSetRenderContextCaps(CRenderContextCaps* pCaps)
        {//PC上模拟低配置MobileGPU
            SDK_IRenderContext_UnsafeSetRenderContextCaps(CoreObject, pCaps);
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int SDK_IRenderContext_GetShaderModel(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IRenderContext_ChooseShaderModel(int sm);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CCommandList.NativePointer SDK_IRenderContext_GetImmCommandList(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IRenderContext_FlushImmContext(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CPass.NativePointer SDK_IRenderContext_CreatePass(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe CSwapChain.NativePointer SDK_IRenderContext_CreateSwapChain(NativePointer self, CSwapChainDesc* desc);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static void SDK_IRenderContext_BindCurrentSwapChain(NativePointer self, CSwapChain.NativePointer swapChain);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static void SDK_IRenderContext_Present(NativePointer self, UInt32 SyncInterval, UInt32 Flags);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CShaderResourceView.NativePointer SDK_IRenderContext_LoadShaderResourceView(NativePointer self, string file);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe CTexture2D.NativePointer SDK_IRenderContext_CreateTexture2D(NativePointer self, CTexture2DDesc* desc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe CRenderTargetView.NativePointer SDK_IRenderContext_CreateRenderTargetView(NativePointer self, CRenderTargetViewDesc* desc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe CDepthStencilView.NativePointer SDK_IRenderContext_CreateDepthRenderTargetView(NativePointer self, CDepthStencilViewDesc* desc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe CShaderResourceView.NativePointer SDK_IRenderContext_CreateShaderResourceView(NativePointer self, CShaderResourceViewDesc* desc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe CGpuBuffer.NativePointer SDK_IRenderContext_CreateGpuBuffer(NativePointer self, CGpuBufferDesc* desc, void* pInitData);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe CShaderResourceView.NativePointer SDK_IRenderContext_CreateShaderResourceViewFromBuffer(NativePointer self, CGpuBuffer.NativePointer pBuffer, ISRVDesc* desc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe CUnorderedAccessView.NativePointer SDK_IRenderContext_CreateUnorderedAccessView(NativePointer self, CGpuBuffer.NativePointer pBuffer, CUnorderedAccessViewDesc* desc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe CFrameBuffer.NativePointer SDK_IRenderContext_CreateFrameBuffers(NativePointer self, CFrameBuffersDesc* desc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe CCommandList.NativePointer SDK_IRenderContext_CreateCommandList(NativePointer self, CCommandListDesc* desc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe CShaderProgram.NativePointer SDK_IRenderContext_CreateShaderProgram(CRenderContext.NativePointer self, CShaderProgramDesc* desc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe CShaderDesc.NativePointer SDK_IRenderContext_CompileHLSLFromFile(CRenderContext.NativePointer self, string file, string entry, string sm, CShaderDefinitions.NativePointer defines, UInt32 CrossPlatforms, vBOOL bDebugShader);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe CVertexShader.NativePointer SDK_IRenderContext_CreateVertexShader(CRenderContext.NativePointer self, CShaderDesc.NativePointer desc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe CPixelShader.NativePointer SDK_IRenderContext_CreatePixelShader(CRenderContext.NativePointer self, CShaderDesc.NativePointer desc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe CComputeShader.NativePointer SDK_IRenderContext_CreateComputeShader(CRenderContext.NativePointer self, CShaderDesc.NativePointer desc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CInputLayout.NativePointer SDK_IRenderContext_CreateInputLayout(CRenderContext.NativePointer self, CInputLayoutDesc.NativePointer desc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe CRenderPipeline.NativePointer SDK_IRenderContext_CreateRenderPipeline(CRenderContext.NativePointer self, CRenderPipelineDesc* desc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CGeometryMesh.NativePointer SDK_IRenderContext_CreateGeometryMesh(CRenderContext.NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe CVertexBuffer.NativePointer SDK_IRenderContext_CreateVertexBuffer(CRenderContext.NativePointer self, CVertexBufferDesc* desc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe CIndexBuffer.NativePointer SDK_IRenderContext_CreateIndexBuffer(CRenderContext.NativePointer self, CIndexBufferDesc* desc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe CIndexBuffer.NativePointer SDK_IRenderContext_CreateIndexBufferFromBuffer(CRenderContext.NativePointer self, CIndexBufferDesc* desc, CGpuBuffer.NativePointer pBuffer);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe CVertexBuffer.NativePointer SDK_IRenderContext_CreateVertexBufferFromBuffer(CRenderContext.NativePointer self, CVertexBufferDesc* desc, CGpuBuffer.NativePointer pBuffer);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe CSamplerState.NativePointer SDK_IRenderContext_CreateSamplerState(CRenderContext.NativePointer self, CSamplerStateDesc* desc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe CRasterizerState.NativePointer SDK_IRenderContext_CreateRasterizerState(CRenderContext.NativePointer self, CRasterizerStateDesc* desc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe CDepthStencilState.NativePointer SDK_IRenderContext_CreateDepthStencilState(CRenderContext.NativePointer self, CDepthStencilStateDesc* desc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe CBlendState.NativePointer SDK_IRenderContext_CreateBlendState(NativePointer self, CBlendStateDesc* desc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CConstantBuffer.NativePointer SDK_IRenderContext_CreateConstantBuffer(CRenderContext.NativePointer self, CShaderProgram.NativePointer program, int index);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CConstantBuffer.NativePointer SDK_IRenderContext_CreateConstantBuffer2(CRenderContext.NativePointer self, CShaderDesc.NativePointer desc, int index);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_IRenderContext_GetRenderContextCaps(CRenderContext.NativePointer self, CRenderContextCaps* pCaps);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_IRenderContext_UnsafeSetRenderContextCaps(CRenderContext.NativePointer self, CRenderContextCaps* pCaps);
        delegate void FOnShaderTranslated(CShaderDesc.NativePointer shaderDesc);
#if PWindow
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static void SDK_CscShaderConductor_SetTranslateCB(FOnShaderTranslated func);
#endif
    #endregion
    }
}

namespace EngineNS
{
    partial class CEngine
    {
        public static bool mGenerateShaderForMobilePlatform = true;
    }
}
