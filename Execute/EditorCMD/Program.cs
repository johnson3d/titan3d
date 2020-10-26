using EngineNS;
using EngineNS.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorCMD
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach(var i in args)
            {
                Console.WriteLine(i);
            }
            if (args == null || args.Length == 0)
                return;

            EngineNS.Rtti.VAssembly assm = null;
            var files = System.IO.Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "Game.Windows*.dll", System.IO.SearchOption.TopDirectoryOnly);
            if (files.Length > 0)
            {
                assm = EngineNS.Rtti.VAssemblyManager.Instance.LoadAssembly(files[0], EngineNS.ECSType.Client, false, false);
                EngineNS.Rtti.RttiHelper.RegisterAnalyseAssembly("Game", assm);
            }

            var cmdEngine = new CMDEngine();
            EngineNS.CIPlatform.Instance.PlayMode = EngineNS.CIPlatform.enPlayMode.Editor;
            CMDEngine.Instance = cmdEngine;
            CMDEngine.Instance.Interval = 25;
            CMDEngine.Instance.PreInitEngine();
            CMDEngine.Instance.InitEngine("Game", null);

            cmdEngine.FileManager.CreateDirectory(cmdEngine.FileManager.CookedTemp);

            EngineNS.Thread.Async.ContextThreadManager.ImmidiateMode = true;
            var unused = RealMain(cmdEngine, args);

            while(cmdEngine.IsRun)
            {
                cmdEngine.MainTick();
            }

            cmdEngine.FileManager.DeleteDirectory(cmdEngine.FileManager.CookedTemp, true);
            cmdEngine.Cleanup();
        }

        static string FindArgument(string[] args, string startWith)
        {
            foreach(var i in args)
            {
                if (i.StartsWith(startWith))
                    return i;
            }
            return null;
        }

        static async System.Threading.Tasks.Task RealMain(CMDEngine cmdEngine, string[] args)
        {
            await EngineNS.CEngine.Instance.InitSystem(IntPtr.Zero, 0, 0, EngineNS.ERHIType.RHT_VirtualDevice, true);
            await EngineNS.CEngine.Instance.OnEngineInited();

            CIPlatform.Instance.PlayMode = CIPlatform.enPlayMode.Cook;

            switch (args[0].ToLower())
            {
                case "pack":
                    {
                        var src = FindArgument(args, "src=").Substring("src=".Length);
                        if (src == null)
                            return;
                        var tar = FindArgument(args, "tar=").Substring("tar=".Length);
                        if (tar == null)
                            return;

                        AssetsPacker.PackAList(src, tar);

                        var pak = new EngineNS.IO.CPakFile();
                        pak.LoadPak(tar);
                        for (UInt32 i = 0; i < pak.AssetNum; i++)
                        {
                            var name = pak.GetAssetName(i);
                            var sz = pak.GetAssetSize(i);
                            var szip = pak.GetAssetSizeInPak(i);
                        }
                        cmdEngine.IsRun = false;
                    }
                    break;
                case "unpack":
                    {

                    }
                    break;
                case "cook":
                    {
                        var entry = FindArgument(args, "entry=").Substring("entry=".Length);
                        var rn = EngineNS.RName.GetRName(entry);

                        var platformStr = FindArgument(args, "platform=").Substring("platform=".Length);
                        var platforms = platformStr.Split('+');

                        var copyRInfo = FindArgument(args, "copyrinfo");

                        EngineNS.IO.XmlHolder AssetInfos = EngineNS.IO.XmlHolder.NewXMLHolder("AssetsPackage", ""); //For andorid

                        string[] sm = null;
                        var smStr = FindArgument(args, "shadermodel=");
                        if (smStr != null)
                        {
                            sm = smStr.Substring("shadermodel=".Length).Split('+');
                        }

                        if (FindArgument(args, "genvsproj") != null)
                        {
                            CMDEngine.CMDEngineInstance.IsNeedProject = true;
                        }

                        var texEncoder = FindArgument(args, "texencoder=");
                        if (texEncoder != null)
                        {
                            ResourceCooker.TexCompressFlags = 0;
                            var texFormats = texEncoder.Substring("texencoder=".Length).Split('+');
                            if(FindArgument(texFormats, "PNG")!=null)
                            {
                                ResourceCooker.TexCompressFlags |= ResourceCooker.ETexCompressMode.PNG;
                            }
                            if (FindArgument(texFormats, "ETC2") != null)
                            {
                                ResourceCooker.TexCompressFlags |= ResourceCooker.ETexCompressMode.ETC2;
                            }
                            if (FindArgument(texFormats, "ASTC") != null)
                            {
                                ResourceCooker.TexCompressFlags |= ResourceCooker.ETexCompressMode.ASTC;
                            }
                        }

                        var pakAssets = FindArgument(args, "pak=");
                        if(pakAssets!=null)
                        {
                            pakAssets = pakAssets.Substring("pak=".Length);
                        }
                        try
                        {
                            foreach (var i in platforms)
                            {
                                CEngine.Instance.FileManager.CookingPlatform = i;
                                var cooker = new AssetCooker();
                                await cooker.CollectAssets(rn, i, copyRInfo!=null?true:false, sm);
                                await CookPlatformShader(args, i, sm, cooker.MaterialAssets);
                                cooker.DirectCopyFiles(i);
                                CMDEngine.CMDEngineInstance.SaveAssetinfos(i);

                                if(pakAssets!=null)
                                {
                                    var listFileName = CEngine.Instance.FileManager.Cooked + CEngine.Instance.FileManager.CookingPlatform + "/tmp_pakassets.alist";
                                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(listFileName))
                                    {
                                        var files = CEngine.Instance.FileManager.GetFiles(CEngine.Instance.FileManager.CookingRoot, "*.*", System.IO.SearchOption.AllDirectories);
                                        foreach (var j in files)
                                        {
                                            var absName = j.ToLower();
                                            bool error = false;
                                            absName = CEngine.Instance.FileManager.NormalizePath(absName, out error);
                                            if (absName.EndsWith(".rinfo"))
                                                continue;
                                            if (absName.EndsWith(".cs"))
                                                continue;
                                            if (absName.EndsWith(".noused"))
                                                continue;

                                            var NameInPak = absName.Substring(CEngine.Instance.FileManager.CookingRoot.Length);
                                            sw.WriteLine($"{absName} {NameInPak} normal");
                                        }
                                    }

                                    AssetsPacker.PackAList(listFileName, pakAssets);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            EngineNS.Profiler.Log.WriteException(ex);
                        }
                        finally
                        {
                            cmdEngine.IsRun = false;
                        }
                    }
                    break;
                case "bake":
                    {

                    }
                    break;
                case "localhost":
                    {

                    }
                    break;
                case "gen_proj":
                    {
                        if (args.Length != 2)
                            return;

                        GenProject.Instance.Command(args);

                        cmdEngine.IsRun = false;
                    }
                    break;
                case "fresh_rinfo":
                    {
                        var subdir = FindArgument(args, "dir=").Substring("dir=".Length);

                        var types = FindArgument(args, "type=").Substring("type=".Length);
                        var resTypes = types.Split('+');

                        var nouse = cmdEngine.FreshRInfo(subdir, resTypes);

                        cmdEngine.IsRun = false;
                    }
                    break;
                case "rname_change":
                    {
                        Dictionary<string, string> changeList = new Dictionary<string, string>();
                        var name = FindArgument(args, "name=");
                        if (name != null)
                        {
                            name = name.Substring("name=".Length);

                            var seg = name.Split('+');
                            foreach (var i in seg)
                            {
                                var rnPair = i.Split('#');

                                if (rnPair.Length != 2)
                                {
                                    EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Warning, "RName", $"rename resource arguments error:{i}");
                                    continue;
                                }

                                var rn = RName.GetRName(rnPair[0]);
                                if(CEngine.Instance.FileManager.FileExists(rn.Address))
                                {
                                    var rnModifier = new RNameModifier();
                                    rnModifier.CollectRefObjects(rn);

                                    await rnModifier.SaveRefObjects(rn, rnPair[1]);

                                    changeList[rnPair[0]] = rnPair[1];
                                }
                                else
                                {
                                    EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Warning, "RName", $"rename resource doesn't exist:{i}");
                                }
                            }
                        }

                        //移动目录
                        var dir = FindArgument(args, "dir=");
                        if (dir != null)
                        {
                            dir = dir.Replace('&', ' ');
                            dir = dir.Substring("dir=".Length);
                            var seg = dir.Split('#');
                            if (seg.Length != 2)
                            {
                                EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Warning, "RName", $"rename directory:{dir}");
                            }
                            else
                            {
                                var src = seg[0].ToLower();
                                var tar = seg[1].ToLower();
                                var files = CEngine.Instance.FileManager.GetFiles(EngineNS.CEngine.Instance.FileManager.ProjectContent + src, "*.rinfo", System.IO.SearchOption.AllDirectories);
                                foreach (var i in files)
                                {
                                    bool error;
                                    var sf = EngineNS.CEngine.Instance.FileManager.NormalizePath(i, out error);
                                    sf = sf.Substring(EngineNS.CEngine.Instance.FileManager.ProjectContent.Length);
                                    sf = sf.Substring(0, sf.Length - ".rinfo".Length);
                                    var rn = EngineNS.RName.GetRName(sf);
                                    var sf2 = sf.Substring(src.Length);
                                    sf2 = tar + sf2;

                                    if (CEngine.Instance.FileManager.FileExists(rn.Address))
                                    {
                                        var rnModifier = new RNameModifier();
                                        rnModifier.CollectRefObjects(rn);
                                        await rnModifier.SaveRefObjects(rn, sf2);

                                        changeList[sf] = sf2;
                                    }   
                                }
                            }
                        }

                        foreach (var i in changeList)
                        {
                            CEngine.Instance.FileManager.RNameRemap[i.Key] = i.Value;
                        }

                        var map = FindArgument(args, "savemap=");
                        if (map != null)
                        {
                            map = map.Substring("savemap=".Length);
                            var seg = map.Split('+');
                            foreach(var i in seg)
                            {
                                var rn = RName.GetRName(i);
                                EngineNS.GamePlay.GWorld World = new EngineNS.GamePlay.GWorld();
                                World.Init();
                                var scene = await EngineNS.GamePlay.GGameInstance.LoadScene(CEngine.Instance.RenderContext, World, rn);
                                if (scene != null)
                                {
                                    EngineNS.Vector3 pos = new EngineNS.Vector3(0,-10,0);
                                    EngineNS.Vector3 lookAt = new EngineNS.Vector3(0,0,0);
                                    EngineNS.Vector3 up = new EngineNS.Vector3(0,1,0);
                                    {
                                        var xnd = await EngineNS.IO.XndHolder.LoadXND(rn.Address + "/scene.map");
                                        if (xnd != null)
                                        {
                                            var att = xnd.Node.FindAttrib("ED_info");
                                            if (att != null)
                                            {
                                                att.BeginRead();

                                                att.Read(out pos);
                                                att.Read(out lookAt);
                                                att.Read(out up);
                                                att.EndRead();
                                            }
                                            xnd.Dispose();
                                        }
                                    }

                                    await scene.SaveScene(rn, null, (InScene, InXnd)=>
                                    {
                                        var att = InXnd.Node.AddAttrib("ED_info");
                                        att.BeginWrite();
                                        att.Write(pos);
                                        att.Write(lookAt);
                                        att.Write(up);
                                        att.EndWrite();
                                    });
                                }
                            }
                        }
                        cmdEngine.IsRun = false;
                    }
                    break;
                default:
                    break;
            }
        }
        static async System.Threading.Tasks.Task CookPlatformShader(string[] args, string platform, string[] sm, HashSet<RName> MaterialAssets)
        {
            //把准备打包的数据做一次优化，初步想法是利用杨智做的Package流程，重新走一次资源存盘，然后用参数控制特殊Save流程
            
            var cookShader = FindArgument(args, "cookshader");
            if (cookShader != null)
            {
                CEngine.Instance.FileManager.CookingPlatform = platform;
                EPlatformType platformFlags = EPlatformType.PLATFORM_WIN;

                platformFlags = 0;
                if (platform == "windows")
                {
                    platformFlags = EPlatformType.PLATFORM_WIN;
                }
                else if (platform == "android")
                {
                    platformFlags = EPlatformType.PLATFORM_DROID;
                }
                else if (platform == "ios")
                {
                    platformFlags = EPlatformType.PLATFORM_IOS;
                }
                foreach (var i in sm)//每个平台的多个shadermodel
                {
                    await CookShadersWithFilter(args, i, platformFlags, MaterialAssets);
                }
            }
        }
        static async System.Threading.Tasks.Task CookShadersWithFilter(string[] args, string smStr, EPlatformType platforms, HashSet<RName> MaterialAssets)
        {
            var rc = CEngine.Instance.RenderContext;

            int ShaderModel = System.Convert.ToInt32(smStr);

            string targetDir = CEngine.Instance.FileManager.CookingRoot + "deriveddatacache/" + "/sm" + smStr + "/";
            if (FindArgument(args, "recompile") != null)
            {
                if(CEngine.Instance.FileManager.DirectoryExists(targetDir))
                    CEngine.Instance.FileManager.DeleteDirectory(targetDir, true);
            }
            CEngine.Instance.FileManager.CreateDirectory(targetDir);
            CEngine.Instance.FileManager.CreateDirectory(targetDir + "cs/");
            CEngine.Instance.FileManager.CreateDirectory(targetDir + "vs/");
            CEngine.Instance.FileManager.CreateDirectory(targetDir + "ps/");
            EngineNS.CRenderContext.ShaderModel = ShaderModel;
            var shaderPath = CEngine.Instance.FileManager.DDCDirectory + "shaderinfo/";
            var shaders = CEngine.Instance.FileManager.GetFiles(shaderPath, "*.xml");
            int NumOfFailed = 0;
            int NumOfNotUsed = 0;
            foreach (var i in shaders)
            {
                using (var xml = EngineNS.IO.XmlHolder.LoadXML(i))
                {
                    EngineNS.Graphics.CGfxEffectDesc desc = await EngineNS.Graphics.CGfxEffectDesc.LoadEffectDescFromXml(rc, xml.RootNode, false);
                    if (desc == null)
                    {
                        EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Warning, "CookShader", $"CookShader Failed: {xml.GetTextString()}");
                        NumOfFailed++;
                        continue;
                    }
                    if(MaterialAssets.Contains(desc.MtlShaderPatch.Name)==false)
                    {
                        NumOfNotUsed++;
                        continue;
                    }
                    var effect = new EngineNS.Graphics.CGfxEffect(desc);
                    await effect.CookEffect(rc, desc, ShaderModel, platforms);
                }
            }
            shaders = CEngine.Instance.FileManager.GetFiles(shaderPath, "*.shaderxml");
            foreach (var i in shaders)
            {
                using (var xml = EngineNS.IO.XmlHolder.LoadXML(i))
                {
                    RName shaderName = RName.GetRName(xml.RootNode.FindAttrib("Shader").Value, RName.enRNameType.Engine);
                    EShaderType type = EShaderType.EST_UnknownShader;
                    switch (xml.RootNode.Name)
                    {
                        case "EST_ComputeShader":
                            type = EShaderType.EST_ComputeShader;
                            break;
                        case "EST_VertexShader":
                            type = EShaderType.EST_VertexShader;
                            break;
                        case "EST_PixelShader":
                            type = EShaderType.EST_PixelShader;
                            break;
                    }

                    string entry = xml.RootNode.FindAttrib("Entry").Value;
                    var node = xml.RootNode.FindNode("Macro");
                    var attrs = node.GetAttribs();
                    CShaderDefinitions defines = new CShaderDefinitions();
                    foreach (var j in attrs)
                    {
                        defines.SetDefine(j.Name, j.Value);
                    }

                    CEngine.Instance.RenderContext.CookShaderDesc(shaderName, entry, type, defines, ShaderModel, platforms);
                }
            }
        }
    }
}
