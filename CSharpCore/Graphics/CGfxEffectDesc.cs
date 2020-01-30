using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics
{
    public class CGfxEffectDesc
    {
        private CGfxEffectDesc()
        {

        }
        public CGfxEffectDesc CloneEffectDesc()
        {
            CGfxEffectDesc result = new CGfxEffectDesc();
            result.MtlShaderPatch = MtlShaderPatch;
            result.MdfQueueShaderPatch = MdfQueueShaderPatch.CloneMdfQueue(CEngine.Instance.RenderContext, null);
            result.EnvShaderPatch = EnvShaderPatch.CloneEnvShaderCode();
            result.UpdateHash64(true);
            return result;
        }
        public static CGfxEffectDesc CreateDesc(CGfxMaterial mtl, Mesh.CGfxMdfQueue mdf, GfxEnvShaderCode shadingenv)
        {
            CGfxEffectDesc result = new CGfxEffectDesc();
            result.MtlShaderPatch = mtl;
            result.MdfQueueShaderPatch = mdf.CloneMdfQueue(CEngine.Instance.RenderContext, null);
            result.EnvShaderPatch = shadingenv;
            //result.EnvShaderPatch = shadingenv.CloneShadingEnv();
            result.UpdateHash64(true);
            return result;
        }

        public string HLSLHashCode
        {
            get;
            set;
        }

        public CGfxMaterial MtlShaderPatch
        {
            get;
            set;
        }
        public Mesh.CGfxMdfQueue MdfQueueShaderPatch
        {
            get;
            private set;
        }

        GfxEnvShaderCode mEnvShaderPatch;
        public GfxEnvShaderCode EnvShaderPatch
        {
            get
            {
                return mEnvShaderPatch;
            }
            private set
            {
                mEnvShaderPatch = value;
            }
        }
        private CShaderDefinitions mShaderMacros;
        
        public CShaderDefinitions ShaderMacros
        {
            get
            {
                if (mShaderMacros == null)
                {
                    mShaderMacros = new CShaderDefinitions();
                }
                return mShaderMacros;
            }
        }
        public Hash64 NameHash;
        public string String
        {
            get;
            private set;
        }
        internal void UpdateHash64(bool UpdateMacro)
        {
            if (UpdateMacro)
                UpdateMacroArray();

            String = "";
            if (MtlShaderPatch != null)
                String += MtlShaderPatch.ToString();
            if (MdfQueueShaderPatch != null)
                String += MdfQueueShaderPatch.ToString();
            if (ShaderMacros != null)
                String += ShaderMacros.ToString();
            String += EnvShaderPatch.ToString();

            Hash64.CalcHash64(ref NameHash, String);
        }
        public Hash64 GetHash64()
        {
            return NameHash;
        }
        public override string ToString()
        {
            return String;
        }
        private void UpdateMacroArray()
        {
            ShaderMacros.ClearDefines();
            ShaderMacros.SetExtraInclude(CShaderDefinitions.EExtraIncludeType.Material, MtlShaderPatch.GetShaderIncludes());
            if (MdfQueueShaderPatch.GetShaderIncludes() != null)
                ShaderMacros.SetExtraInclude(CShaderDefinitions.EExtraIncludeType.MdfQueue, MdfQueueShaderPatch.GetShaderIncludes());
            
            ShaderMacros.SetExtraDefines(CShaderDefinitions.EExtraIncludeType.Material, MtlShaderPatch.GetShaderDefines());
            if (MdfQueueShaderPatch.GetShaderDefines() != null)
                ShaderMacros.SetExtraDefines(CShaderDefinitions.EExtraIncludeType.MdfQueue, MdfQueueShaderPatch.GetShaderDefines());
            if (EnvShaderPatch.MacroDefines != null)
            {
                var defs = EnvShaderPatch.MacroDefines;
                foreach (var i in defs)
                {
                    ShaderMacros.SetDefine(i.Name, i.Definition);
                }
            }

            List<CGfxMaterial.MtlMacro> MacroArray = MtlShaderPatch.GetMtlMacroArray();
            for (int idx = 0; idx < MacroArray.Count; idx++)
            {
                ShaderMacros.SetDefine(MacroArray[idx].mMacroName, MacroArray[idx].mMacroValue);
            }

            var mdfCaller = MdfQueueShaderPatch.GetMdfQueueCaller();
            if (string.IsNullOrEmpty(mdfCaller) == false)
            {
                ShaderMacros.SetDefine("MDFQUEUE_FUNCTION", mdfCaller);
            }
        }
        public void SaveXML(IO.XmlHolder xml, Hash64 hash)
        {
            if(this.MtlShaderPatch.Name.RNameType == RName.enRNameType.Editor && this.MtlShaderPatch.Name.Name.Contains("MaterialEditor/Mats/"))
            {
                return;
            }
            if (xml == null)
            {
                xml = IO.XmlHolder.NewXMLHolder("EffectDesc", "");

                Save2XML(xml.RootNode);
            }

            var file = GetShaderInfoFileName(hash);
            IO.XmlHolder.SaveXML(file, xml);
        }
        public static string GetShaderInfoFileName(Hash64 hash)
        {
            return CEngine.Instance.FileManager.DDCDirectory + "shaderinfo/" + hash.ToString().ToLower() + ".xml";
        }
        public void Save2XML(IO.XmlNode node)
        {
            //string fileName = "";

            var hlslDesc = CEngine.Instance.FileManager.HLSLFileDescManager.FindFileDesc(this.EnvShaderPatch.ShaderName.Address);

            node.AddAttrib("HLSLHashCode", hlslDesc.HashWithDepends.ToString());
            node.AddAttrib("Material_RNameType", ((int)MtlShaderPatch.Name.RNameType).ToString());
            node.AddAttrib("Material", MtlShaderPatch.Name.ToString());
            node.AddAttrib("ShadingEnv", EnvShaderPatch.ShaderName.ToString());
            node.AddAttrib("ShadingEnvType", Rtti.RttiHelper.GetTypeSaveString(EnvShaderPatch.EnvType));
            node.AddAttrib("CacheTime", System.DateTime.Now.ToString());

            {
                var mdfCaller = MdfQueueShaderPatch.GetMdfQueueCaller();
                var mdf = node.AddNode("MdfQueue", "", node.mHolder);
                mdf.AddAttrib("MDFQUEUE_FUNCTION", mdfCaller);
                foreach (var i in MdfQueueShaderPatch.Modifiers)
                {
                    mdf.AddNode(i.Name, "", node.mHolder);
                }
            }

            {
                var mtl = MtlShaderPatch.GetMtlMacroArray();
                var mtlNode = node.AddNode("MtlMacro", "", node.mHolder);
                foreach (var i in mtl)
                {
                    mtlNode.AddAttrib(i.mMacroName, i.mMacroValue);
                }
            }

            {
                var env = node.AddNode("EnvMacro", "", node.mHolder);
                var defs = EnvShaderPatch.MacroDefines;
                if (defs != null)
                {
                    foreach (var i in defs)
                    {
                        env.AddAttrib(i.Name, i.Definition);
                    }
                }
            }
        }
        public static async System.Threading.Tasks.Task<CGfxEffectDesc> LoadEffectDescFromXml(CRenderContext rc, IO.XmlNode node, bool testHash = true)
        {
            var result = new CGfxEffectDesc();
            if (node == null)
                return result;
            RName.enRNameType rNameType = RName.enRNameType.Game;
            var attr = node.FindAttrib("Material_RNameType");
            if (attr != null)
            {
                rNameType = (RName.enRNameType)System.Convert.ToInt32(attr.Value);
            }
            attr = node.FindAttrib("Material");
            if (attr != null)
            {
                result.MtlShaderPatch = await CEngine.Instance.MaterialManager.GetMaterialAsync(rc, RName.GetRName(attr.Value, rNameType));
                if (result.MtlShaderPatch == null)
                    return null;
            }
            
            attr = node.FindAttrib("ShadingEnv");
            string shadingEnvName = null;
            if (attr != null)
            {
                result.mEnvShaderPatch = new Graphics.GfxEnvShaderCode();
                shadingEnvName = attr.Value;

                var attr1 = node.FindAttrib("ShadingEnvType");
                if(attr1!=null)
                {
                    var envType = Rtti.RttiHelper.GetTypeFromSaveString(attr1.Value);
                    var envTemp = Rtti.RttiHelper.CreateInstance(envType) as CGfxShadingEnv;
                    if(envTemp!=null)
                    {
                        envTemp.InitCodeOnce(result.mEnvShaderPatch);
                    }
                }
                var LoadDefs = new List<CShaderDefinitions.MacroDefine>();
                var defs = node.FindNode("EnvMacro");
                if (defs != null)
                {
                    var macroAttrs = defs.GetAttribs();
                    for (int i = 0; i < macroAttrs.Count; i++)
                    {
                        result.mEnvShaderPatch.SetMacroDefineValue(macroAttrs[i].Name, macroAttrs[i].Value);
                    }
                }
            }

            result.MdfQueueShaderPatch = new Mesh.CGfxMdfQueue();
            var mdf = node.FindNode("MdfQueue").GetNodes();
            foreach (var i in mdf)
            {
                var type = Rtti.RttiHelper.GetTypeFromSaveString(i.Name);
                var om = System.Activator.CreateInstance(type) as EngineNS.Graphics.Mesh.CGfxModifier;
                if (om == null)
                    continue;
                result.MdfQueueShaderPatch.AddModifier(om);
            }
            result.UpdateMacroArray();
            result.UpdateHash64(false);

            if (testHash)
            {
                attr = node.FindAttrib("HLSLHashCode");
                if (attr != null)
                {
                    result.HLSLHashCode = attr.Value;
                    var hlslDesc = CEngine.Instance.FileManager.HLSLFileDescManager.FindFileDesc(RName.GetRName(shadingEnvName, RName.enRNameType.Engine).Address);
                    if (hlslDesc != null && hlslDesc.HashWithDepends.ToString() != result.HLSLHashCode)
                    {
                        Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Shaders", $"effect {shadingEnvName} need recompile");
                        return null;
                    }
                }
            }
            return result;
        }

        class GlobalShaderCacheSetting : IO.Serializer.Serializer
        {
            [Rtti.MetaData]
            public int Version
            {
                get;
                set;
            } = 1;

            [Rtti.MetaData]
            public bool ClearShaderInfo
            {
                get;
                set;
            } = false;
        }

        class LocalShaderCacheSetting : IO.Serializer.Serializer
        {
            [Rtti.MetaData]
            public int Version
            {
                get;
                set;
            } = 1;

            [Rtti.MetaData]
            public bool IsCrossPlatform
            {
                get;
                set;
            } = false;
        }

        public static async System.Threading.Tasks.Task CheckRebuildShaders()
        {
            await Thread.AsyncDummyClass.DummyFunc();
            var GlobalSCSetting = IO.XmlHolder.CreateObjectFromXML(RName.GetRName("Cache/GlobalShaderCacheSetting.xml")) as GlobalShaderCacheSetting;
            if (GlobalSCSetting == null)
            {
                GlobalSCSetting = new GlobalShaderCacheSetting();
                IO.XmlHolder.SaveObjectToXML(GlobalSCSetting, RName.GetRName("Cache/GlobalShaderCacheSetting.xml"));
                return;
            }

            if (GlobalSCSetting.ClearShaderInfo == true)
            {
                CEngine.Instance.FileManager.DeleteFilesInDirectory(RName.GetRName("Cache/ShaderInfo/").Address, "*.xml", System.IO.SearchOption.AllDirectories);
                GlobalSCSetting.ClearShaderInfo = false;
                IO.XmlHolder.SaveObjectToXML(GlobalSCSetting, RName.GetRName("Cache/GlobalShaderCacheSetting.xml"));
            }

            var LocalSCSetting = IO.XmlHolder.CreateObjectFromXML(RName.GetRName("Cache/LocalShaderCacheSetting.xml")) as LocalShaderCacheSetting;
            if (LocalSCSetting == null)
            {
                LocalSCSetting = new LocalShaderCacheSetting();
                IO.XmlHolder.SaveObjectToXML(LocalSCSetting, RName.GetRName("Cache/LocalShaderCacheSetting.xml"));
            }

            CEngine.mGenerateShaderForMobilePlatform = LocalSCSetting.IsCrossPlatform;

            if (LocalSCSetting.Version == GlobalSCSetting.Version)
            {
                return;
            }

            LocalSCSetting.Version = GlobalSCSetting.Version;
            var SrcDir = CEngine.Instance.FileManager.EngineRoot + "Shaders/CoreShader/";
            CEngine.Instance.FileManager.CopyDirectory(SrcDir, RName.GetRName("Shaders").Address);
            CEngine.Instance.FileManager.DeleteFilesInDirectory(RName.GetRName("Cache/Shader/").Address, "*.shader", System.IO.SearchOption.AllDirectories);
            IO.XmlHolder.SaveObjectToXML(LocalSCSetting, RName.GetRName("Cache/LocalShaderCacheSetting.xml"));
        }
        static bool MultiThreadCompile = true;
        public static async System.Threading.Tasks.Task LoadAllShaders(CRenderContext rc)
        {
            var sm = CRenderContext.ShaderModelString;
            var shaderPath = CEngine.Instance.FileManager.DDCDirectory + sm + "/";
            var shaders = CEngine.Instance.FileManager.GetFiles(shaderPath, "*.shader");
            Profiler.Log.WriteLine(Profiler.ELogTag.Info, "LoadShaders", $"Begin LoadShaders Number = {shaders.Count}");
            var t1 = Support.Time.HighPrecision_GetTickCount();
            var smp = Thread.ASyncSemaphore.CreateSemaphore(shaders.Count);
            foreach (var i in shaders)
            {
                if (MultiThreadCompile)
                {
                    CEngine.Instance.EventPoster.RunOn(async () =>
                    {
                        var fileName = CEngine.Instance.FileManager.GetPureFileFromFullName(i, false);
                        Hash64 hash = Hash64.TryParse(fileName);
                        var effect = new CGfxEffect();
                        if (await effect.LoadFromXndAsync(rc, hash) == 0)
                        {
                            CEngine.Instance.EffectManager.RegEffect(hash, effect);
                        }
                        else
                        {
                            CEngine.Instance.FileManager.DeleteFile(shaderPath + hash.ToString() + ".shader");
                        }
                        smp.Release();
                        return true;
                    }, Thread.Async.EAsyncTarget.TPools);
                }
                else
                {
                    var fileName = CEngine.Instance.FileManager.GetPureFileFromFullName(i, false);
                    Hash64 hash = Hash64.TryParse(fileName);
                    var effect = new CGfxEffect();
                    if (await effect.LoadFromXndAsync(rc, hash) == 0)
                    {
                        CEngine.Instance.EffectManager.RegEffect(hash, effect);
                    }
                    else
                    {
                        CEngine.Instance.FileManager.DeleteFile(shaderPath + hash.ToString() + ".shader");
                    }
                    smp.Release();
                }
            }
            await smp.Await();
            var t2 = Support.Time.HighPrecision_GetTickCount();
            Profiler.Log.WriteLine(Profiler.ELogTag.Info, "LoadShaders", $"End LoadShaders, Times = {(t2 - t1) / 1000} ms");
        }
        public static async System.Threading.Tasks.Task BuildCachesWhenCleaned(CRenderContext rc)
        {
            var sm = CRenderContext.ShaderModelString;
            var shaderPath = CEngine.Instance.FileManager.DDCDirectory + sm + "/";
            var shaders = CEngine.Instance.FileManager.GetFiles(shaderPath, "*.shader");
            if (shaders != null && shaders.Count > 0)
                return;
            var t1 = Support.Time.HighPrecision_GetTickCount();
            Profiler.Log.WriteLine(Profiler.ELogTag.Info, "BuildShaderCache", "Begin Shader Cache");
            var path = CEngine.Instance.FileManager.DDCDirectory + "shaderinfo";
            var files = CEngine.Instance.FileManager.GetFiles(path, "*.xml");
            var descs = new List<CGfxEffectDesc>(files.Count);
            foreach (var i in files)
            {
                using (var xml = IO.XmlHolder.LoadXML(i))
                {
                    CGfxEffectDesc desc = await CGfxEffectDesc.LoadEffectDescFromXml(rc, xml.RootNode);
                    if (desc !=null && desc.MdfQueueShaderPatch != null)
                        descs.Add(desc);
                }
            }

            if (MultiThreadCompile == false)
            {
                foreach (var i in descs)
                {
                    CGfxEffect effect = new CGfxEffect(i);
                    if (effect.CreateEffectByD11Editor(rc, i, EPlatformType.PLATFORM_WIN))
                    {
                        CEngine.Instance.EffectManager.RegEffect(i.GetHash64(), effect);
                    }
                }
            }
            else
            {
                var DescNum = new Thread.ThreadSafeNumber(descs.Count);
                await CEngine.Instance.EventPoster.AwaitMTS_Foreach(descs.Count, (idx, smp) =>
                {
                    var remain = DescNum.Release();
                    CGfxEffect effect = new CGfxEffect(descs[idx]);
                    if (effect.CreateEffectByD11Editor(rc, descs[idx], EPlatformType.PLATFORM_WIN))
                    {
                        Profiler.Log.WriteLine(Profiler.ELogTag.Info, "BuildShaderCache", $"CacheShader = {descs[idx].GetHash64()}; Remain = {remain}");
                        CEngine.Instance.EffectManager.RegEffect(descs[idx].GetHash64(), effect);
                    }
                    else
                    {
                        Profiler.Log.WriteLine(Profiler.ELogTag.Info, "BuildShaderCache", $"CacheShader Failed = {descs[idx].GetHash64()}; Remain = {remain}");
                    }
                });
            }
            var t2 = Support.Time.HighPrecision_GetTickCount();
            Profiler.Log.WriteLine(Profiler.ELogTag.Info, "BuildShaderCache", $"End Shader Cache, Times = {(t2 - t1) / 1000} ms");
            System.GC.Collect();
        }
    }

}
