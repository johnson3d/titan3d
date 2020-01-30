using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EngineNS.Graphics
{
    [Rtti.MetaClass]
    public class SRVParam : EngineNS.IO.Serializer.Serializer
    {
        [Rtti.MetaData]
        public string ShaderName;
        [Rtti.MetaData]
        public EngineNS.RName RName;
        public CShaderResourceView RSView;

        ~SRVParam()
        {
            Cleanup();
        }
        public void Cleanup()
        {
            RSView?.Cleanup();
        }
    }

    [Rtti.MetaClass]
    public class CGfxVarValue : IO.Serializer.Serializer
    {
        EngineNS.Graphics.CGfxVar mDefinition = new CGfxVar();
        [Rtti.MetaData]
        public EngineNS.Graphics.CGfxVar Definition
        {
            get => mDefinition;
            set => mDefinition = value;
        }
        byte[] mValueArray;
        [Rtti.MetaData]
        public byte[] ValueArray
        {
            get => mValueArray;
            set => mValueArray = value;
        }

        public int GetValueSize(EShaderVarType type)
        {
            switch (type)
            {
                case EShaderVarType.SVT_Float1:
                    return 4;
                case EShaderVarType.SVT_Float2:
                    return 8;
                case EShaderVarType.SVT_Float3:
                    return 12;
                case EShaderVarType.SVT_Float4:
                    return 16;
                case EShaderVarType.SVT_Int1:
                    return 4;
                case EShaderVarType.SVT_Int2:
                    return 8;
                case EShaderVarType.SVT_Int3:
                    return 12;
                case EShaderVarType.SVT_Int4:
                    return 16;
                case EShaderVarType.SVT_Matrix4x4:
                    return 64;
                case EShaderVarType.SVT_Matrix3x3:
                    return 36;
                default:
                    return -1;
            }
        }

    }

    [Rtti.MetaClass]
    public class CGfxVar : EngineNS.IO.Serializer.Serializer
    {
        [Rtti.MetaData]
        public EShaderVarType Type { get; set; }
        [Rtti.MetaData]
        public UInt32 Elements { get; set; }
        [Rtti.MetaData]
        public string Name { get; set; }
    };
    public partial class CGfxEffect
    {
        EngineNS.Thread.Async.TaskLoader.WaitContext WaitContext = new Thread.Async.TaskLoader.WaitContext();
        public async System.Threading.Tasks.Task<Thread.Async.TaskLoader.WaitContext> AwaitLoad()
        {
            this.PreUse(null);
            return await EngineNS.Thread.Async.TaskLoader.Awaitload(WaitContext);
        }
        CShaderProgram mShaderProgram;
        public CShaderProgram ShaderProgram
        {
            get
            {
                return mShaderProgram;
            }
        }
        CInputLayout mInputLayout;
        public CInputLayout InputLayout
        {
            get { return mInputLayout; }
        }
        CShaderDesc mVSDesc;
        public CShaderDesc VSDesc
        {
            get { return mVSDesc; }
        }
        CShaderDesc mPSDesc;
        public CShaderDesc PSDesc
        {
            get { return mPSDesc; }
        }
        CGfxEffectDesc mDesc;

        public CGfxEffectDesc Desc
        {
            get { return mDesc; }
        }
        public struct ProgramCache
        {
            public ProgramCache(bool dummy)
            {
                EnvMapBindPoint = UInt32.MaxValue;
                SampEnvMapBindPoint = UInt32.MaxValue;
                EyeEnvMapBindPoint = UInt32.MaxValue;
                SampEyeEnvMapBindPoint = UInt32.MaxValue;
                BindingSlot_ShadowMap = UInt32.MaxValue;
                BindingSlot_ShadowMapSampler = UInt32.MaxValue;

                PerFrameId = UInt32.MaxValue;
                PerInstanceId = UInt32.MaxValue;
                CBID_Mesh = UInt32.MaxValue;
                CBID_Camera = UInt32.MaxValue;
                CBID_View = UInt32.MaxValue;
                CBID_ShadingEnv = UInt32.MaxValue;
            }
            void Reset()
            {
                EnvMapBindPoint = UInt32.MaxValue;
                SampEnvMapBindPoint = UInt32.MaxValue;
                EyeEnvMapBindPoint = UInt32.MaxValue;
                SampEyeEnvMapBindPoint = UInt32.MaxValue;
                BindingSlot_ShadowMap = UInt32.MaxValue;
                BindingSlot_ShadowMapSampler = UInt32.MaxValue;

                PerFrameId = UInt32.MaxValue;
                PerInstanceId = UInt32.MaxValue;
                CBID_Mesh = UInt32.MaxValue;
                CBID_Camera = UInt32.MaxValue;
                CBID_View = UInt32.MaxValue;
                CBID_ShadingEnv = UInt32.MaxValue;
            }
            public UInt32 EnvMapBindPoint;
            public UInt32 SampEnvMapBindPoint;
            public UInt32 EyeEnvMapBindPoint;
            public UInt32 SampEyeEnvMapBindPoint;
            public UInt32 BindingSlot_ShadowMap;
            public UInt32 BindingSlot_ShadowMapSampler;

            public UInt32 PerFrameId;
            public UInt32 PerInstanceId;
            public UInt32 CBID_Mesh;
            public UInt32 CBID_Camera;
            public UInt32 CBID_View;
            public UInt32 CBID_ShadingEnv;
            public unsafe static void ResetProgramIDs(ProgramCache* cache, CGfxEffect effect)
            {
                unsafe
                {
                    cache->Reset();

                    var shaderProg = effect.ShaderProgram;
                    cache->PerFrameId = shaderProg.FindCBuffer("cbPerFrame");
                    cache->PerInstanceId = shaderProg.FindCBuffer("cbPerInstance");
                    cache->CBID_Camera = shaderProg.FindCBuffer("cbPerCamera");
                    cache->CBID_View = shaderProg.FindCBuffer("cbPerViewport");
                    cache->CBID_ShadingEnv = shaderProg.FindCBuffer("cbPerShadingEnv");
                    cache->CBID_Mesh = shaderProg.FindCBuffer("cbPerMesh");

                    {
                        CTextureBindInfo info = new CTextureBindInfo(); //this is a struct,so you can use new as convenient;
                        if (shaderProg.FindTextureBindInfo(null, "gEnvMap", ref info))
                        {
                            cache->EnvMapBindPoint = info.PSBindPoint;
                        }
                    }
                    {
                        var SamplerBindInfo = new CSamplerBindInfo();
                        if (shaderProg.FindSamplerBindInfo(null, "Samp_gEnvMap", ref SamplerBindInfo) == true)
                        {
                            cache->SampEnvMapBindPoint = SamplerBindInfo.PSBindPoint;
                        }
                    }
                    {
                        CTextureBindInfo info = new CTextureBindInfo();
                        if (shaderProg.FindTextureBindInfo(null, "gEyeEnvMap", ref info))
                        {
                            cache->EyeEnvMapBindPoint = info.PSBindPoint;
                        }
                    }
                    {
                        var SamplerBindInfo = new CSamplerBindInfo();
                        if (shaderProg.FindSamplerBindInfo(null, "Samp_gEyeEnvMap", ref SamplerBindInfo) == true)
                        {
                            cache->SampEyeEnvMapBindPoint = SamplerBindInfo.PSBindPoint;
                        }
                    }
                    {
                        CTextureBindInfo info = new CTextureBindInfo();
                        if (shaderProg.FindTextureBindInfo(null, "gShadowMap", ref info))
                        {
                            cache->BindingSlot_ShadowMap = info.PSBindPoint;
                        }
                    }
                    {
                        var SamplerBindInfo = new CSamplerBindInfo();
                        if (shaderProg.FindSamplerBindInfo(null, "Samp_gShadowMap", ref SamplerBindInfo) == true)
                        {
                            cache->BindingSlot_ShadowMapSampler = SamplerBindInfo.PSBindPoint;
                        }
                    }
                }
            }
        }
        public readonly ProgramCache CacheData = new ProgramCache(true);
        const string VSMain = "VS_Main";
        const string PSMain = "PS_Main";
        const string VSVersion = "vs_5_0";
        const string PSVersion = "ps_5_0";
        private async System.Threading.Tasks.Task<bool> CreateEffectAsync(CRenderContext rc, CGfxEffectDesc desc, EPlatformType platforms, bool tryLoad = true)
        {
            int loadOK = 0;
            if (tryLoad)
            {
                var savedHash = desc.GetHash64();
                loadOK = await LoadFromXndAsync(rc, desc.GetHash64());
                if (loadOK == 0)
                {
                    if (mDesc.GetHash64() != savedHash)
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }
                    return true;
                }
            }

            mDesc = desc;

            if(CIPlatform.Instance.PlayMode == CIPlatform.enPlayMode.Game && EngineNS.CEngine.Instance.RenderSystem.RHIType != ERHIType.RHT_D3D11)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Shader", $"LoadEffectDescFromXml failed: {desc.ToString()}");
                return false;
            }

            var targetThread = Thread.Async.EAsyncTarget.AsyncIO;
            if (EngineNS.CEngine.Instance.RenderSystem.RHIType == ERHIType.RHT_OGL)
            {
                targetThread = Thread.Async.EAsyncTarget.Render;
            }
            return await CEngine.Instance.EventPoster.Post(() =>
            {
                return CreateEffectByD11Editor(rc, desc, platforms);
            }, targetThread);
        }
        internal bool CreateEffectByD11Editor(CRenderContext rc, CGfxEffectDesc desc, EPlatformType platforms, bool saveShader = true)
        {
            if (CreateEffectByD11EditorImpl(rc, desc, platforms) == false)
            {
                EngineNS.Thread.Async.TaskLoader.Release(ref WaitContext, null);
                return false;
            }
            else
            {
                EngineNS.Thread.Async.TaskLoader.Release(ref WaitContext, this);

                var savedHash = desc.GetHash64();
                if (mDesc != desc || mDesc.GetHash64() != savedHash)
                {
                    System.Diagnostics.Debug.Assert(false);
                }
                if (saveShader)
                {
                    CEngine.Instance.EventPoster.RunOn(() =>
                    {
                        Save2Xnd(savedHash, platforms);
                        //mDesc.SaveXML();
                        return null;
                    }, Thread.Async.EAsyncTarget.AsyncIO);
                }
                return true;
            }
        }
        private void UpdateCBIDs()
        {
            unsafe
            {
                fixed (ProgramCache* p = &CacheData)
                    ProgramCache.ResetProgramIDs(p, this);
            }
        }
        public async System.Threading.Tasks.Task<bool> CookEffect(CRenderContext rc, CGfxEffectDesc desc, int ShaderModel, EPlatformType platforms)
        {
            mDesc = desc;
            var absPath = desc.EnvShaderPatch.ShaderName.Address;

            mDesc.UpdateHash64(true);
            var hash = mDesc.GetHash64();
            if (await this.LoadFromXndAsync(rc, hash)==0)
                return true;

            mVSDesc = rc.CompileHLSLFromFile(absPath, VSMain, VSVersion, desc.ShaderMacros, platforms);
            if (mVSDesc == null)
            {
                return false;
            }
            mPSDesc = rc.CompileHLSLFromFile(absPath, PSMain, PSVersion, desc.ShaderMacros, platforms);
            if (mPSDesc == null)
            {
                return false;
            }

            var savedHash = desc.GetHash64();

            Save2Xnd(savedHash, platforms);
            return true;
        }
        private bool CreateEffectByD11EditorImpl(CRenderContext rc, CGfxEffectDesc desc, EPlatformType platforms)
        {
            mDesc = desc;

            var absPath = desc.EnvShaderPatch.ShaderName.Address;

            mVSDesc = rc.CompileHLSLFromFile(absPath, VSMain, VSVersion, desc.ShaderMacros, platforms);
            if (mVSDesc == null)
            {
                return false;
            }
            mPSDesc = rc.CompileHLSLFromFile(absPath, PSMain, PSVersion, desc.ShaderMacros, platforms);
            if (mPSDesc == null)
            {
                return false;
            }

            using (StreamReader sr = new StreamReader(desc.MtlShaderPatch.Name.Address + ".var"))
            {
                MtlVar = sr.ReadToEnd();
            }
            using (StreamReader sr = new StreamReader(desc.MtlShaderPatch.Name.Address + ".code"))
            {
                MtlCode = sr.ReadToEnd();
            }

            CShaderProgramDesc spDesc = new CShaderProgramDesc();
            mShaderProgram = rc.CreateShaderProgram(spDesc);
            if (mShaderProgram == null)
                return false;
            var vs = rc.CreateVertexShader(mVSDesc);
            if (vs == null)
                return false;
            var ps = rc.CreatePixelShader(mPSDesc);
            if (ps == null)
                return false;
            CEngine.Instance.DefaultLayoutDesc.SetShaderDesc(mVSDesc);
            mInputLayout = CEngine.Instance.PrebuildPassData.DefaultInputLayout; //rc.CreateInputLayout(CEngine.Instance.DefaultLayoutDesc);

            var psCodeDebugger = mPSDesc.GLCode;
            mShaderProgram.InputLayout = mInputLayout;
            mShaderProgram.VertexShader = vs;
            mShaderProgram.PixelShader = ps;
            if(false == mShaderProgram.LinkShaders(rc))
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Shader", $"LinkShaders failed:{desc.ToString()}");
            }

            UpdateCBIDs();

            if (CIPlatform.Instance.PlayMode != CIPlatform.enPlayMode.Game)
            {
                CEngine.Instance.EventPoster.RunOn(() =>
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Effect", $"Shader {mDesc.ToString()} Build Successed!");
                    return null;
                }, Thread.Async.EAsyncTarget.AsyncEditor);
            }
            return true;
        }
        public string MtlVar = "";
        public string MtlCode = "";
        public string MdfCaller = "";
        public void Save2Xnd(Hash64 hash, EPlatformType platforms)
        {
            mDesc.UpdateHash64(true);
            if (hash != mDesc.GetHash64())
            {
                //System.Diagnostics.Debug.Assert(false);
                Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Shader", $"Effect在即将存盘后，又发生了ShadingEnv的宏切换，导致内容变化了，请尽量在上一个切换结束后再做切换操作");
                hash = mDesc.GetHash64();
            }

            string path = GetShaderFile(ref hash);
            //string path;
            //if(shadermodel!=null)
            //    path = CEngine.Instance.FileManager.DDCDirectory + shadermodel + "/" + hash + ".shader";
            //else
            //    path = CEngine.Instance.FileManager.DDCDirectory + "shader/" + hash + ".shader";
            bool saveToFile = true;
            if(saveToFile)
            {
                if (mVSDesc == null || mPSDesc == null)
                    return;

                var xml = IO.XmlHolder.NewXMLHolder("Desc", "");
                var descXml = xml.RootNode;
                mDesc.Save2XML(descXml);
                mDesc.SaveXML(xml, hash);
                
                string xmlString = "";
                IO.XmlHolder.GetXMLStringFromNode(ref xmlString, descXml);
                var xnd = IO.XndHolder.NewXNDHolder();
                IO.XndNode node = xnd.Node;
                var attr = node.AddAttrib("Info");
                attr.BeginWrite();
                attr.Write(xmlString);
                attr.EndWrite();
                var vs = node.AddNode("VertexShader", 0, 0);
                var ps = node.AddNode("PixelShader", 0, 0);

                //if (shadermodel == null)
                {
                    attr = node.AddAttrib("DebugInfo");
                    attr.BeginWrite();
                    attr.Write(MtlVar);
                    attr.Write(MtlCode);
                    attr.Write(mDesc.MdfQueueShaderPatch.GetMdfQueueCaller());
                    attr.EndWrite();
                }

                mVSDesc.Save2Xnd(vs, platforms);
                mPSDesc.Save2Xnd(ps, platforms);
                IO.XndHolder.SaveXND(path, xnd);
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
        internal async System.Threading.Tasks.Task<int> LoadFromXndAsync(CRenderContext rc, Hash64 hash)
        {
            string descXmlString = "";

            var path = GetShaderFile(ref hash);

            int ret0 = await CEngine.Instance.EventPoster.Post(() =>
            {   
                using (var xnd = IO.XndHolder.SyncLoadXND(path))
                {
                    if (xnd == null)
                    {
                        return -1;
                    }
                    var attr = xnd.Node.FindAttrib("Info");
                    if (attr == null)
                    {
                        return -2;
                    }
                    attr.BeginRead();
                    attr.Read(out descXmlString);
                    attr.EndRead();

                    attr = xnd.Node.FindAttrib("DebugInfo");
                    if (attr != null)
                    {
                        attr.BeginRead();
                        attr.Read(out MtlVar);
                        attr.Read(out MtlCode);
                        attr.Read(out MdfCaller);
                        attr.EndRead();
                    }

                    var vsNode = xnd.Node.FindNode("VertexShader");
                    var psNode = xnd.Node.FindNode("PixelShader");
                    if (vsNode == null || psNode == null)
                    {
                        return -3;
                    }
                    mVSDesc = new CShaderDesc(EShaderType.EST_VertexShader);
                    if (mVSDesc.LoadXnd(vsNode) == false)
                    {
                        return -4;
                    }
                    mPSDesc = new CShaderDesc(EShaderType.EST_PixelShader);
                    if (mPSDesc.LoadXnd(psNode) == false)
                    {
                        return -5;
                    }
                }
                return 0;
            }, Thread.Async.EAsyncTarget.AsyncIO);

            if (ret0 != 0)
            {
                return ret0;
            }

            var xmlHolder = IO.XmlHolder.ParseXML(descXmlString);
            mDesc = await CGfxEffectDesc.LoadEffectDescFromXml(rc, xmlHolder.RootNode);
            if(mDesc==null)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Shader", $"LoadEffectDescFromXml failed: {hash.ToString()}");
                return -6;
            }

            if (CIPlatform.Instance.PlayMode != CIPlatform.enPlayMode.Game)
            {
                EngineNS.Graphics.CShadingPermutation.CollectShaderInfo(mDesc);
            }
#if PWindow
            if (mDesc.GetHash64() != hash)
            {
                CEngine.Instance.FileManager.DeleteFile(path);
                path = CGfxEffectDesc.GetShaderInfoFileName(hash);
                CEngine.Instance.FileManager.DeleteFile(path);
                Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Effect", $"shader {descXmlString} 修改过材质宏，删除原来的cache文件");
                return -100;
            }
#endif

            Thread.Async.EAsyncTarget target = Thread.Async.EAsyncTarget.AsyncIO;
            if (CEngine.Instance.Desc.RHIType == ERHIType.RHT_OGL)
            {
                target = Thread.Async.EAsyncTarget.Render;
            }
            int ret = await CEngine.Instance.EventPoster.Post(() =>
            {
                CShaderProgramDesc spDesc = new CShaderProgramDesc();
                mShaderProgram = rc.CreateShaderProgram(spDesc);
                if (mShaderProgram == null)
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Shader", $"CreateShaderProgram failed: {hash.ToString()}");
                    return -7;
                }
                var vs = rc.CreateVertexShader(mVSDesc);
                if (vs == null)
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Shader", $"CreateVertexShader failed: {hash.ToString()}");
                    return -8;
                }
                var ps = rc.CreatePixelShader(mPSDesc);
                if (ps == null)
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Shader", $"CreatePixelShader failed: {hash.ToString()}");
                    return -9;
                }
                                
                CEngine.Instance.DefaultLayoutDesc.SetShaderDesc(mVSDesc);
                //mInputLayout = rc.CreateInputLayout(CEngine.Instance.DefaultLayoutDesc);
                mInputLayout = CEngine.Instance.PrebuildPassData.DefaultInputLayout;

                mShaderProgram.InputLayout = mInputLayout;
                mShaderProgram.VertexShader = vs;
                mShaderProgram.PixelShader = ps;

                string psCode = mPSDesc.GLCode;
                if (mShaderProgram.LinkShaders(rc) == false)
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Shader", $"LinkShaders failed:{descXmlString}");
                }

                UpdateCBIDs();

                var t4 = Support.Time.HighPrecision_GetTickCount();
                return 0;
            }, target);
            if (ret < 0)
            {
                return ret;
            }

            EngineNS.Thread.Async.TaskLoader.Release(ref WaitContext, this);
            return 0;
        }
    }
    
}
