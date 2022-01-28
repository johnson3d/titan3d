using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.IO
{
    public partial class FileManager
    {
        internal FileManager()
        {
            InitDirectory();
            SetSysDir(ESystemDir.MetaData, "metadata");
            SetSysDir(ESystemDir.Effect, "effect");
            SureDirectory(GetPath(ERootDir.Engine, ESystemDir.MetaData));
            SureDirectory(GetPath(ERootDir.Cache, ESystemDir.Effect));
        }
        partial void InitDirectory();
        public enum ERootDir
        {
            Root,
            Current,
            Game,
            Engine,
            Editor,
            Cache,
            EngineSource,
            GameSource,
            Count,
        }
        public enum ESystemDir
        {
            MetaData,
            Effect,
            Count,
        }
        public string[] Roots = new string[(int)ERootDir.Count];
        public string[] SysDirs = new string[(int)ESystemDir.Count];
        public void SetRoot(ERootDir type, string path)
        {
            Roots[(int)type] = GetValidDirectory(path);
            IO.FileManager.SureDirectory(Roots[(int)type]);
        }
        public void SetSysDir(ESystemDir type, string path)
        {
            SysDirs[(int)type] = GetValidFileName(path);
        }
        public string GetRoot(ERootDir type)
        {
            return Roots[(int)type];
        }
        public string GetPath(ERootDir root, ESystemDir type)
        {
            return Roots[(int)root] + SysDirs[(int)type] + "/";
        }
        public static string GetValidDirectory(string path)
        {
            path = path.Replace('\\', '/');
            if (path.StartsWith("/"))
            {
                path = path.Substring(1, path.Length - 1);
            }
            if (path.EndsWith("/") == false)
            {
                path += "/";
            }
            return path.ToLower();
        }
        public static string GetValidFileName(string path)
        {
            path = path.Replace('\\', '/');
            if (path.StartsWith("/"))
            {
                path = path.Substring(1, path.Length - 1);
            }
            return path.ToLower();
        }
        public static string GetBaseDirectory(string path, int numOfParents = 1)
        {
            path = path.Replace('\\', '/');
            if (path.EndsWith("/"))
            {
                path = path.Substring(0, path.Length - 1);
            }
            int count = 0;
            for (int i = path.Length - 1; i >= 0; i--)
            {
                if (path[i] == '/')
                {
                    count++;
                    if (count == numOfParents)
                    {
                        return path.Substring(0, i + 1);
                    }
                }
            }
            return null;
        }
        public static string[] GetFiles(string path, string searchPattern, bool bAllDirectory = true)
        {
            System.IO.SearchOption option = bAllDirectory ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly;
            return System.IO.Directory.GetFiles(path, searchPattern, option);
        }
        public static string[] GetDirectories(string path, string searchPattern, bool bAllDirectory = true)
        {
            System.IO.SearchOption option = bAllDirectory ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly;
            return System.IO.Directory.GetDirectories(path, searchPattern, option);
        }
        public static bool DirectoryExists(string path)
        {
            return System.IO.Directory.Exists(path);
        }
        public static bool FileExists(string path)
        {
            return System.IO.File.Exists(path);
        }
        public static void SureDirectory(string path)
        {
            if (DirectoryExists(path) == false)
            {
                CreateDirectory(path);
            }
        }
        public static System.IO.DirectoryInfo CreateDirectory(string path)
        {
            return System.IO.Directory.CreateDirectory(path);
        }
        public static void DeleteDirectory(string path, bool recursive = true)
        {
            System.IO.Directory.Delete(path, recursive);
        }
        public static void DeleteFile(string path)
        {
            System.IO.File.Delete(path);
        }
        public static void CopyFile(string src, string tar)
        {
            System.IO.File.Copy(src, tar);
        }
        public static string GetRelativePath(string absoluteSourcePath, string absoluteTargetPath)
        {
            if (!System.IO.Path.IsPathRooted(absoluteSourcePath))
                throw new ArgumentException("Source path is not rooted!");
            if (!System.IO.Path.IsPathRooted(absoluteTargetPath))
                throw new ArgumentException("Target path is not rooted!");

            var strPathTag = absoluteTargetPath.Replace("\\", "/");
            var strPathSrc = absoluteSourcePath.Replace("\\", "/");

            if (strPathTag == strPathSrc)
                return "";

            if (!strPathSrc.EndsWith('/'))
                strPathSrc += "/";
            int intIndex = -1;
            int intPos = strPathSrc.IndexOf('/');

            while(intPos >= 0)
            {
                intPos++;

                if (string.Compare(strPathSrc, 0, strPathTag, 0, intPos, true) != 0)
                    break;

                intIndex = intPos;
                intPos = strPathSrc.IndexOf('/', intPos);
            }

            if(intIndex >= 0)
            {
                strPathTag = strPathTag.Substring(intIndex);
                intPos = strPathSrc.IndexOf('/', intIndex);

                while(intPos >= 0)
                {
                    strPathTag = "../" + strPathTag;
                    intPos = strPathSrc.IndexOf('/', intPos + 1);
                }
            }

            return strPathTag.ToLower();
        }
        private static bool IsSamePathChar(char l, char r)
        {
            if (l == r)
                return true;
            else if (l == '/' && r == '\\')
                return true;
            else if (l == '\\' && r == '/')
                return true;
            else
                return false;
        }
        //dir1/dir2/dir3/file1.txt
        //dir1/dir2/dir3/dir4.dir
        public static string GetLastestPathName(string str)
        {
            str = str.Replace('\\', '/');
            if (str.EndsWith("/"))
            {
                str = str.Substring(0, str.Length - 1);
            }
            var segs = str.Split('/');
            return segs[segs.Length - 1];
        }
        public static string GetParentPathName(string str)
        {
            str = str.Replace('\\', '/');
            if (str.EndsWith("/"))
            {
                str = str.Substring(0, str.Length - 1);
            }
            var pos = str.LastIndexOf('/');
            return str.Substring(0, pos);
        }
        public static string GetPureName(string str)
        {
            var filename = EngineNS.IO.FileManager.GetLastestPathName(str);
            var pos = filename.LastIndexOf('.');
            if (pos < 0)
                return str;
            return filename.Substring(0, pos);
        }
        public static string GetExtName(string str)
        {
            var filename = EngineNS.IO.FileManager.GetLastestPathName(str);
            var pos = filename.LastIndexOf('.');
            if (pos < 0)
                return "";
            return filename.Substring(pos, filename.Length - pos);
        }
        public static string RemoveExtName(string str)
        {
            var pos = str.LastIndexOf('.');
            if (pos < 0)
                return str;
            return str.Substring(0, pos);
        }
        public static string CombinePath(string path1, string path2)
        {
            if (path1.EndsWith("\\") || path1.EndsWith("/"))
            {
                path1 = path1.Substring(0, path1.Length - 1);
            }
            if (path2.StartsWith("\\") || path2.StartsWith("/"))
            {
                path2 = path2.Substring(1, path1.Length - 1);
            }

            var result = path1 + '/' + path2;

            return GetRegularPath(result);
        }
        public static string GetRegularPath(string path)
        {
            path = path.Replace('\\', '/');

            var cur = path.IndexOf("/..");
            while (cur >= 0)
            {
                cur--;
                var start = path.LastIndexOf('/',  cur);
                if (start < 0)
                    return null;
                path = path.Remove(start, cur + 1 - start + 3);
                cur = path.IndexOf("/..");
            }
            return path;
        }
        public static string ReadAllText(string file)
        {
            if (System.IO.File.Exists(file) == false)
                return null;
            return System.IO.File.ReadAllText(file);
        }
        public static void WriteAllText(string file, string text)
        {
            System.IO.File.WriteAllText(file, text);
        }
        public static string GetXmlText(System.Xml.XmlDocument xml)
        {
            var streamXml = new System.IO.MemoryStream();
            var writer = new System.Xml.XmlTextWriter(streamXml, Encoding.UTF8);
            writer.Formatting = System.Xml.Formatting.Indented;
            xml.Save(writer);
            var reader = new System.IO.StreamReader(streamXml, Encoding.UTF8);
            streamXml.Position = 0;
            var content = reader.ReadToEnd();
            reader.Close();
            streamXml.Close();
            return content;
        }
        public static System.Xml.XmlDocument LoadXml(string file)
        {
            if (FileExists(file) == false)
                return null;
            var xml = new System.Xml.XmlDocument();
            xml.Load(file);
            return xml;
        }
        public static System.Xml.XmlDocument LoadXmlFromString(string xmlStr)
        {
            if (string.IsNullOrEmpty(xmlStr))
                return null;
            var xml = new System.Xml.XmlDocument();
            xml.LoadXml(xmlStr);
            return xml;
        }
        public static void SaveObjectToXml(string file, object obj)
        {
            var xml = new System.Xml.XmlDocument();
            var xmlRoot = xml.CreateElement($"Root", xml.NamespaceURI);
            xml.AppendChild(xmlRoot);
            IO.SerializerHelper.WriteObjectMetaFields(xml, xmlRoot, obj);
            var xmlText = IO.FileManager.GetXmlText(xml);
            IO.FileManager.WriteAllText(file, xmlText);
        }
        public static object LoadXmlToObject(string file, System.Type type)
        {
            var xml = IO.FileManager.LoadXml(file);
            if (xml == null)
                return null;
            object pThis = Rtti.UTypeDescManager.CreateInstance(type);
            IO.SerializerHelper.ReadObjectMetaFields(null, xml.LastChild as System.Xml.XmlElement, ref pThis, null);
            return pThis;
        }
        public static object LoadXmlToObject(string file)
        {
            var xml = IO.FileManager.LoadXml(file);
            if (xml == null)
                return null;
            System.Type type = null;
            foreach(System.Xml.XmlAttribute i in xml.LastChild.Attributes)
            {
                if (i.Name == "Type")
                {
                    type = Rtti.UTypeDesc.TypeOf(i.Value).SystemType;
                    break;
                }
            }
            if (type == null)
                return null;
            object pThis = Rtti.UTypeDescManager.CreateInstance(type);
            IO.SerializerHelper.ReadObjectMetaFields(null, xml.LastChild as System.Xml.XmlElement, ref pThis, null);
            return pThis;
        }
        public static T LoadXmlToObject<T>(string file) where T : class, new()
        {
            var xml = IO.FileManager.LoadXml(file);
            if (xml == null)
                return null;
            object pThis = null;
            IO.SerializerHelper.ReadObjectMetaFields(null, xml.LastChild as System.Xml.XmlElement, ref pThis, null);
            return (T)pThis;
        }
        public static bool LoadXmlToObject<T>(string file, T host) where T : class, new()
        {
            var xml = IO.FileManager.LoadXml(file);
            if (xml == null)
                return false;
            object pThis = host;
            IO.SerializerHelper.ReadObjectMetaFields(null, xml.LastChild as System.Xml.XmlElement, ref pThis, null);
            return true;
        }
    }

    public partial class UOpenFileDialog
    {
        public string Title { get; set; }
        public string InitialDirectory { get; set; }
        public string Filter { get; set; }
        public bool Multiselect { get; set; }
        public bool ShowReadOnly { get; set; }
        protected string mFileName;
        public string FileName { get => mFileName; }
        protected string[] mFileNames = null;
        public string[] FileNames { get => mFileNames; }
        public int ShowDialog()
        {
            int result = 0;
            ShowDialogImpl(ref result);
            return result;
        }
        partial void ShowDialogImpl(ref int result);
    }
}

namespace EngineNS.UTest
{
    [UTest]
    public class UTest_FileManager
    {
        public void UnitTestEntrance()
        {
            var path = IO.FileManager.CombinePath("abc/cda\\bad/", "fdsa/dac");

            var path2 = IO.FileManager.CombinePath("abc/cda\\bad/", "../fdsa/dac");

            var path3 = IO.FileManager.CombinePath("abc/cda\\bad/", "/../fdsa/dac");
        }
    }
}
