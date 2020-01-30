using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using EngineNS;
using EngineNS.IO;

namespace EditorCMD
{
    public class CMDEngine : EngineNS.CEngine
    {
        EngineNS.IO.XmlHolder AssetInfos = EngineNS.IO.XmlHolder.NewXMLHolder("AssetsPackage", ""); //For andorid
        EngineNS.IO.XmlHolder ProjectXML;
        public bool IsRun = true;

        public static CMDEngine CMDEngineInstance
        {
            get
            {
                return Instance as CMDEngine;
            }
        }

        public bool IsNeedProject
        {
            get;
            set;
        }


        //public bool InitSystem()
        //{
        //    var ok = EngineNS.CEngine.Instance.GfxInitEngine(EngineNS.ERHIType.RHT_VirtualDevice, 0, IntPtr.Zero);
        //    if (ok == false)
        //    {
        //        return false;
        //    }
        //    return true;
        //}

        public EditorCommon.Resources.ResourceInfoManager mInfoManager;
        public override async System.Threading.Tasks.Task<bool> OnEngineInited()
        {
            if (false == await base.OnEngineInited())
                return false;

            string[] dllNames = new string[]
            {
                "EditorCommon.dll",
                "MaterialEditor.dll",
                "Macross.dll",
                "ExcelViewEditor.dll",
                "UVAnimEditor.dll",
                "Plugins/TextureSourceEditor/bin/TextureSourceEditor.dll",
                "Plugins/UVAnimEditor/bin/UVAnimEditor.dll",
            };
            mInfoManager = new EditorCommon.Resources.ResourceInfoManager();

            foreach(var i in dllNames)
            {
                var edCAssembly = EngineNS.Rtti.RttiHelper.GetAssemblyFromDllFileName(EngineNS.ECSType.Client, EngineNS.CEngine.Instance.FileManager.Bin + i);
                mInfoManager.RegResourceInfo(edCAssembly.Assembly);
            }

            return true;
        }

        public async System.Threading.Tasks.Task FreshRInfo(string subDir, string[] resTypes)
        {
            var gmsFiles = EngineNS.CEngine.Instance.FileManager.GetFiles(EngineNS.CEngine.Instance.FileManager.ProjectContent + subDir, "*.gms", System.IO.SearchOption.AllDirectories);
            var rc = EngineNS.CEngine.Instance.RenderContext;
            if(resTypes.Contains("gms"))
            {
                foreach (var i in gmsFiles)
                {
                    bool error;
                    var sf = EngineNS.CEngine.Instance.FileManager.NormalizePath(i, out error);
                    sf = sf.Substring(EngineNS.CEngine.Instance.FileManager.ProjectContent.Length);
                    var rn = EngineNS.RName.GetRName(sf);

                    var mesh = await EngineNS.CEngine.Instance.MeshManager.CreateMeshAsync(rc, rn, false, true);
                    //mesh.SaveMesh(i);

                    var resInfo = mInfoManager.CreateResourceInfo("Mesh") as EditorCommon.ResourceInfos.MeshResourceInfo;
                    resInfo.Load(i + ".rinfo");
                    resInfo.RefreshReferenceRNames(mesh);
                    var t = resInfo.Save(i + ".rinfo", false);
                }
            }

            if (resTypes.Contains("instmtl"))
            {
                gmsFiles = EngineNS.CEngine.Instance.FileManager.GetFiles(EngineNS.CEngine.Instance.FileManager.ProjectContent + subDir, "*.instmtl", System.IO.SearchOption.AllDirectories);
                foreach (var i in gmsFiles)
                {
                    bool error;
                    var sf = EngineNS.CEngine.Instance.FileManager.NormalizePath(i, out error);
                    sf = sf.Substring(EngineNS.CEngine.Instance.FileManager.ProjectContent.Length);
                    var rn = EngineNS.RName.GetRName(sf);

                    var mtlInst = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, rn);
                    if (mtlInst == null)
                        continue;
                    //mesh.SaveMesh(i);

                    var resInfo = mInfoManager.CreateResourceInfo("MaterialInstance") as MaterialEditor.ResourceInfos.MaterialInstanceResourceInfo;
                    resInfo.Load(i + ".rinfo");
                    resInfo.RefreshReferenceRNames(mtlInst);
                    var t = resInfo.Save(i + ".rinfo", false);
                }
            }

            if (resTypes.Contains("material"))
            {
                gmsFiles = EngineNS.CEngine.Instance.FileManager.GetFiles(EngineNS.CEngine.Instance.FileManager.ProjectContent + subDir, "*.material", System.IO.SearchOption.AllDirectories);
                foreach (var i in gmsFiles)
                {
                    bool error;
                    var sf = EngineNS.CEngine.Instance.FileManager.NormalizePath(i, out error);
                    sf = sf.Substring(EngineNS.CEngine.Instance.FileManager.ProjectContent.Length);
                    var rn = EngineNS.RName.GetRName(sf);

                    var mtlInst = await EngineNS.CEngine.Instance.MaterialManager.GetMaterialAsync(rc, rn);
                    if (mtlInst == null)
                        continue;
                    //mesh.SaveMesh(i);

                    var resInfo = mInfoManager.CreateResourceInfo("Material") as MaterialEditor.ResourceInfos.MaterialResourceInfo;
                    resInfo.Load(i + ".rinfo");
                    resInfo.RefreshReferenceRNames(mtlInst);
                    var t = resInfo.Save(i + ".rinfo", false);
                }
            }

            if (resTypes.Contains("uvanim"))
            {
                gmsFiles = EngineNS.CEngine.Instance.FileManager.GetFiles(EngineNS.CEngine.Instance.FileManager.ProjectContent + subDir, "*.uvanim", System.IO.SearchOption.AllDirectories);
                foreach (var i in gmsFiles)
                {
                    bool error;
                    var sf = EngineNS.CEngine.Instance.FileManager.NormalizePath(i, out error);
                    sf = sf.Substring(EngineNS.CEngine.Instance.FileManager.ProjectContent.Length);
                    var rn = EngineNS.RName.GetRName(sf);

                    var uvAnim = new EngineNS.UISystem.UVAnim();
                    if (false == await uvAnim.LoadUVAnimAsync(EngineNS.CEngine.Instance.RenderContext, rn))
                        continue;

                    var resInfo = mInfoManager.CreateResourceInfo("UVAnim") as UVAnimEditor.UVAnimResourceInfo;
                    resInfo.Load(i + ".rinfo");
                    resInfo.RefreshReferenceRNames(uvAnim);
                    var t = resInfo.Save(i + ".rinfo", false);
                }
            }
            if (resTypes.Contains("macross"))
            {
                FreshMacrossRInfo(subDir);
            }
        }
        public void FreshMacrossRInfo(string subDir)
        {
            var assembly = System.Reflection.Assembly.LoadFrom("D:/OpenSource/titan3d/binaries/Game.Windows_aa07a08c_a3eb_4039_9628_c0a442275ac3.dll");
            var types = assembly.GetTypes();
            var files = EngineNS.CEngine.Instance.FileManager.GetDirectories(EngineNS.CEngine.Instance.FileManager.ProjectContent + subDir, "*.macross", System.IO.SearchOption.AllDirectories);
            foreach (var i in files)
            {
                bool error;
                var sf = EngineNS.CEngine.Instance.FileManager.NormalizePath(i, out error);
                sf = sf.Substring(EngineNS.CEngine.Instance.FileManager.ProjectContent.Length);

                var rn = EngineNS.RName.GetRName(sf);
                sf = sf.Replace('/', '.');
                sf = sf.Substring(0, sf.Length - ".macross".Length);

                foreach (var j in types)
                {
                    if(j.FullName == sf)
                    {
                        break;
                    }
                }
            }
        }
        private List<RName> FreshByType(System.Type type)
        {
            var macrossObject = System.Activator.CreateInstance(type);
            List<RName> result = new List<RName>();
            var props = type.GetProperties();
            foreach(var i in props)
            {
                if(i.PropertyType == typeof(RName))
                {
                    var rn = i.GetValue(macrossObject) as RName;
                    if(result.Contains(rn)==false)
                    {
                        result.Add(rn);
                    }
                }
            }

            return result;
        }

        public EngineNS.IO.XmlNode FindAndAddAssetNode(EngineNS.IO.XmlNode node, string name)
        {
            List<EngineNS.IO.XmlNode> nodes = node.GetNodes();
            foreach (var i in nodes)
            {
                //这里不能是空
                EngineNS.IO.XmlAttrib att = i.FindAttrib("Name");
                if (att != null && att.Value.Equals(name))
                {
                    return i;
                }
            }

            EngineNS.IO.XmlNode sunnode = node.AddNode("Folder", "", AssetInfos);
            sunnode.AddAttrib("Name", name);

            return sunnode;
        }

        public void AddProjectXMLNode(string value)
        {
            if (ProjectXML == null)
            {
                string projectpath = EngineNS.CEngine.Instance.FileManager.ProjectSourceRoot + "Batman.Droid/";
                ProjectXML = EngineNS.IO.XmlHolder.LoadXML(projectpath + "Batman.Droid.csproj");

                CorrectProjectCatalogue(ProjectXML.RootNode);
            }
            EngineNS.IO.XmlNode ItemGroupNode = ProjectXML.RootNode.AddNode("ItemGroup", "", ProjectXML);
            EngineNS.IO.XmlNode AndroidAssetNode = ItemGroupNode.AddNode("AndroidAsset", "", ProjectXML);
            
            //test
            value = value.Replace("/", "\\");
            AndroidAssetNode.AddAttrib("Include", value);
        }

        public void AddAssetInfos(string filename)
        {
            if (!System.IO.File.Exists(filename) || filename.EndsWith(".noused"))
            {
                return;
            }

            string allpath = EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(filename, EngineNS.CEngine.Instance.FileManager.CookingRoot);
            allpath = EngineNS.CEngine.Instance.FileManager.GetPathFromFullName(allpath, false);
            allpath = allpath.Replace("\\", "/");
            allpath = "Assets/" + allpath;
            string name = EngineNS.CEngine.Instance.FileManager.GetPureFileFromFullName(filename);
            
            string[] folders = allpath.Split('/');
            EngineNS.IO.XmlNode node = AssetInfos.RootNode;
            EngineNS.IO.XmlNode subnode;
            for (int i = 0; i < folders.Length; i++)
            {
                if (folders[i].Equals(""))
                    break;

                subnode = FindAndAddAssetNode(node, folders[i]);
                node = subnode;
            }

            EngineNS.IO.XmlNode filenode = node.FindNode("Files");
            if (filenode == null)
            {
                filenode = node.AddNode("Files", "", AssetInfos);
            }

            foreach(var i in filenode.GetNodes())
            {
                if (i.FindAttrib("Name").Value == name)
                    return;
            }
            var value = filenode.AddNode("File", "", AssetInfos);
            value.AddAttrib("Name", name);

            //生成MD5码
            try
            {
                var MD5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                byte[] code = MD5.ComputeHash(fs);
                string str = System.Text.Encoding.ASCII.GetString(code);
                value.AddAttrib("MD5", System.Text.Encoding.ASCII.GetString(code));
                if (IsNeedProject)
                {
                    AddProjectXMLNode(allpath + name);
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }

        }

        public void GatherFolderInfos(string foldername, string ext)
        {
            System.IO.DirectoryInfo folder = new System.IO.DirectoryInfo(foldername);
            foreach (System.IO.FileInfo file in folder.GetFiles(ext, System.IO.SearchOption.AllDirectories))
            {
                AddAssetInfos(file.FullName);
            }

            foreach (System.IO.DirectoryInfo file in folder.GetDirectories())
            {
                GatherFolderInfos(file.FullName, ext);
            }
        }

        public void CorrectProjectCatalogue(XmlNode pnode)
        {
            List<XmlNode> nodes = pnode.GetNodes();
            foreach (XmlNode node in nodes)
            {
                if (node.Name == "AndroidResource" || node.Name == "None" || node.Name == "Compile" || node.Name == "AndroidNativeLibrary")
                {
                    List<string> attrvalues = new List<string>();
                    List<XmlAttrib> attrs =  node.GetAttribs();
                    foreach (XmlAttrib attr in attrs)
                    {
                        if (attr.Name == "Include" )
                        {
                            attrvalues.Add(attr.Value);
                         //   node.RemoveAttrib(attr);
                        }
                    }

                    foreach (string attrvalue in attrvalues)
                    {
                        string projectpath = EngineNS.CEngine.Instance.FileManager.ProjectSourceRoot + "Batman.Droid/";
                        projectpath = projectpath.Replace("/", "\\");

                        var cookDir = CEngine.Instance.FileManager.Cooked + "android/";
                        //node.AddAttrib("Include", projectpath + attrvalue);
                        EngineNS.CEngine.Instance.FileManager.CopyFile(projectpath + attrvalue, cookDir + attrvalue, true);
                        
                    }
                }

                if (node.Name == "ProjectReference")
                {
                    List<string> attrvalues = new List<string>();
                    List<XmlAttrib> attrs = node.GetAttribs();
                    //List<XmlNode> subnodes = null;
                    foreach (XmlAttrib attr in attrs)
                    {
                        if (attr.Name == "Include")
                        {
                            attrvalues.Add(attr.Value);
                            node.RemoveAttrib(attr);
                        }
                    }

                    foreach (string attrvalue in attrvalues)
                    {
                        string projectpath = EngineNS.CEngine.Instance.FileManager.ProjectSourceRoot;
                        projectpath = projectpath.Replace("/", "\\");

                        string value = attrvalue.Replace("..\\", "");
                        node.AddAttrib("Include", projectpath + value);
                    }
                }

                if (node.Name == "Reference")
                {
                    XmlNode hintpathnode = node.FindNode("HintPath");
                    if (hintpathnode != null)
                    {
                        string projectpath = EngineNS.CEngine.Instance.FileManager.ProjectSourceRoot + "Batman.Droid/";
                        string[] arry = hintpathnode.Value.Split('\\');
                        string newname = EngineNS.CEngine.Instance.FileManager.EngineRoot.Replace("/", "\\") +  "binaries\\" + arry[arry.Length - 1];
                        node.RemoveNode(hintpathnode);
                        node.AddNode("HintPath", newname, ProjectXML);
                    }
                }

                CorrectProjectCatalogue(node);
            }

        }

        public void SaveAssetinfos(string platform)
        {
            var cookDir = CEngine.Instance.FileManager.Cooked + platform + "/";

            bool isandroid = platform == "android";
            if (isandroid)
            {
                cookDir += "Assets/";
            }
            EngineNS.IO.XmlHolder.SaveXML(cookDir + "content/assetinfos.xml", AssetInfos);

            AddAssetInfos(cookDir + "content/assetinfos.xml");

            if (IsNeedProject && ProjectXML != null)
            {
                EngineNS.IO.XmlHolder.SaveXML(CEngine.Instance.FileManager.Cooked + platform + "/" + "temp.csproj", ProjectXML);
            }
        }

    }
}
