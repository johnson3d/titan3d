using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorCommon
{
    [EngineNS.Rtti.MetaClass]
    public class GameProjectConfig : EngineNS.IO.Serializer.Serializer
    {
        static GameProjectConfig mInstance;
        public static GameProjectConfig Instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = new GameProjectConfig();
                    var file = EngineNS.CEngine.Instance.FileManager.Bin + "projectconfig.xml";
                    if(EngineNS.CEngine.Instance.FileManager.FileExists(file))
                    {
                        var xml = EngineNS.IO.XmlHolder.LoadXML(file);
                        mInstance.ReadObjectXML(xml.RootNode);
                    }
                    else
                    {
                        var xml = EngineNS.IO.XmlHolder.NewXMLHolder("ProjConfig", "");
                        mInstance.WriteObjectXML(xml.RootNode);
                        EngineNS.IO.XmlHolder.SaveXML(file, xml);
                    }
                }

                return mInstance;
            }
        }
        [EngineNS.Rtti.MetaData]
        public string GameDllFileName_Android
        {
            get;
            set;
        } = "Game.Android";

        [EngineNS.Rtti.MetaData]
        public string GameDllFileName
        {
            get;
            set;
        } = "Game.Windows";

        // 相对于Root
        [EngineNS.Rtti.MetaData]
        public string GameDllDir
        {
            get
            {
                return EngineNS.CEngine.Instance.FileManager.Bin + "Batman/";
            }
        }// = "binaries/Batman/";

        //[EngineNS.Rtti.MetaData]
        public string GameProjFileName
        {
            get
            {
                return EngineNS.CEngine.Instance.FileManager.ProjectSourceRoot + "Game.Windows/Game.Windows.csproj";
            }
        }// = "Execute/Games/Batman/Game.Windows/Game.Windows.csproj";
        public string MacrossGenerateProjItemsFileName
        {
            get
            {
                return EngineNS.CEngine.Instance.FileManager.ProjectSourceRoot + "Macross.Generated/Macross.Generated.projitems";
            }
        }// = "Execute/Games/Batman/Macross.Generated/Macross.Generated.projitems";
        
        string mMSBuildAbsFileName = null;
        public string MSBuildAbsFileName
        {
            get
            {
                if(mMSBuildAbsFileName == null)
                {
                    mMSBuildAbsFileName = "";
                    var outPut = "";

                    var p = new Process();
                    p.StartInfo.FileName = $"{EngineNS.CEngine.Instance.FileManager.Bin}vswhere.exe";
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardInput = true;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.RedirectStandardError = true;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.Arguments = "-latest -property installationPath";
                    p.Start();
                    outPut = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();
                    p.Close();

                    var dir = new System.IO.DirectoryInfo(outPut);
                    var files = System.IO.Directory.GetFiles(dir.FullName, "MSBuild.exe", System.IO.SearchOption.AllDirectories);
                    foreach(var file in files)
                    {
                        if (file.ToLower().Contains("amd64"))
                            mMSBuildAbsFileName = file;
                    }
                    if (string.IsNullOrEmpty(mMSBuildAbsFileName) && files.Length > 0)
                        mMSBuildAbsFileName = files[0];
                }

                return mMSBuildAbsFileName;
                //return @"C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\amd64\msbuild.exe";
                /*/ 注册表取出来的不一定是最新的
                if(mMSBuildAbsFileName == null)
                {
                    mMSBuildAbsFileName = "";
                    // 从注册表获取MSBuild位置
                    var softwareKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE");
                    if (softwareKey == null)
                        return mMSBuildAbsFileName;
                    var microsoftKey = softwareKey.OpenSubKey("Microsoft");
                    if (microsoftKey == null)
                        return mMSBuildAbsFileName;
                    var msBuildKey = microsoftKey.OpenSubKey("MSBuild");
                    if (msBuildKey == null)
                        return mMSBuildAbsFileName;
                    var toolVers = msBuildKey.OpenSubKey("ToolsVersions");
                    if (toolVers == null)
                        return mMSBuildAbsFileName;
                    var keys = toolVers.GetSubKeyNames();
                    if (keys.Length > 0)
                    {
                        var maxLen = 0;
                        List<int[]> vers = new List<int[]>();
                        for (int i = 0; i < keys.Length; i++)
                        {
                            var key = keys[i];
                            var nums = key.Split('.');
                            var ary = new int[nums.Length];
                            for (int numIdx = 0; numIdx < nums.Length; numIdx++)
                            {
                                ary[numIdx] = System.Convert.ToInt32(nums[numIdx]);
                            }
                            vers.Add(ary);
                            if (ary.Length > maxLen)
                                maxLen = ary.Length;
                        }

                        for (int i = 0; i < maxLen; i++)
                        {
                            int maxVer = 0;
                            for (int keyIdx = vers.Count - 1; keyIdx >= 0; keyIdx--)
                            {
                                var ver = vers[keyIdx];
                                if (ver.Length <= i)
                                    vers.Remove(ver);
                                else if (ver[i] > maxVer)
                                {
                                    maxVer = ver[i];
                                }
                                else
                                    vers.Remove(ver);
                            }

                            if (vers.Count == 1)
                                break;
                        }

                        string curVer = "";
                        foreach(var ver in vers[0])
                        {
                            curVer += ver + ".";
                        }
                        curVer = curVer.TrimEnd('.');

                        var verKey = toolVers.OpenSubKey(curVer);
                        var path = verKey.GetValue("MSBuildToolsPath").ToString().Replace("/", "\\").TrimEnd('\\') + "\\";
                        mMSBuildAbsFileName = path + "MSBuild.exe";
                    }
                }

                return mMSBuildAbsFileName;*/
            }
        }
    }
}
