using Org.BouncyCastle.Bcpg.OpenPgp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace EngineNS.IO
{
    public struct FScopedResMemory : IDisposable
    {
        public IntPtr Pointer;
        public ulong Size;
        public TtRes2Memory Res2Mem;
        public unsafe FScopedResMemory(TtRes2Memory r2m)
        {
            Res2Mem = r2m;
            Size = r2m.mCoreObject.Length();
            Pointer = (IntPtr)r2m.mCoreObject.Ptr(0, Size);
        }
        public void Dispose()
        {
            if (Res2Mem==null)
                return;
            Res2Mem.mCoreObject.Free();
            Res2Mem = null;
            Pointer = IntPtr.Zero; 
            Size = 0;
        }
    }
    public class TtRes2Memory : AuxPtrType<VRes2Memory>
    {
        public static TtRes2Memory CreateFromFile(string file)
        {
            var ptr = VRes2Memory.CreateFromFile(file);
            if (ptr.IsValidPointer == false)
                return null;
            return new TtRes2Memory(ptr);
        }
        public TtRes2Memory(VRes2Memory self)
        {
            mCoreObject = self;
        }
        public FScopedResMemory GetMemory()
        {
            return new FScopedResMemory(this);
        }
    }
    [Rtti.Meta("")]
    public class TtFileInfo
    {
        [Rtti.Meta("")]
        public string Path { get; set; }
        [Rtti.Meta("")]
        public string Hash { get; set; }
        public string CalcFileHash(string root)
        {
            return StaticCalcFileHash(Path, root);
        }
        public static string StaticCalcFileHash(string path, string root)
        {
            var file = TtFileManager.CombinePath(root, path);
            var bytes = TtFileManager.ReadAllBytes(file);
            if (bytes==null)
                return null;
            return ComputeSHA256Hash(bytes);
        }
        public static string StaticCalcFileHash(string file)
        {
            var bytes = TtFileManager.ReadAllBytes(file);
            if (bytes==null)
                return null;
            return ComputeSHA256Hash(bytes);
        }
        public bool UpdateHash(string root)
        {
            Hash = CalcFileHash(root);
            return Hash!=null;
        }
        public static string ComputeSHA256Hash(byte[] input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(input);
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
        public static byte[] HexStringToByteArray(string hexString)
        {
            // 检查字符串长度是否为偶数（每个字节由2个十六进制字符表示）
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException("十六进制字符串长度必须为偶数");
            }

            // 创建字节数组
            byte[] bytes = new byte[hexString.Length / 2];

            // 每两个字符转换为一个字节
            for (int i = 0; i < hexString.Length; i += 2)
            {
                string hexByte = hexString.Substring(i, 2);
                bytes[i / 2] = Convert.ToByte(hexByte, 16);
            }

            return bytes;
        }
    }
    public partial class TtFileManager
    {
        internal TtFileManager(string[] args)
        {
            InitDirectory(args);
            SetSysDir(ESystemDir.MetaData, "metadata");
            SetSysDir(ESystemDir.Config, "config");
            SetSysDir(ESystemDir.GraphicEffect, "effect/graphic");
            SetSysDir(ESystemDir.ComputeEffect, "effect/compute");
            SetSysDir(ESystemDir.RayTracingEffect, "effect/raytracing");
            SetSysDir(ESystemDir.PSO, "pso");
            SetSysDir(ESystemDir.RenderDoc, "renderdoc");
            SetSysDir(ESystemDir.DebugUtility, "debugutility");
            SureDirectory(GetPath(ERootDir.Engine, ESystemDir.MetaData));
            SureDirectory(GetPath(ERootDir.Game, ESystemDir.Config));
            SureDirectory(GetPath(ERootDir.Cache, ESystemDir.GraphicEffect));
            SureDirectory(GetPath(ERootDir.Cache, ESystemDir.ComputeEffect));
            SureDirectory(GetPath(ERootDir.Cache, ESystemDir.PSO));
            SureDirectory(GetPath(ERootDir.Cache, ESystemDir.RenderDoc));
            SureDirectory(GetPath(ERootDir.Cache, ESystemDir.DebugUtility));
        }
        partial void InitDirectory(string[] args);
        public enum ERootDir
        {
            //Root,
            Game,
            Engine,
            PluginContent,
            Editor,
            Cache,
            Cloud,
            Plugin,
            Execute,
            EngineSource,
            PluginSource,
            GameSource,
            Count,
        }
        public enum ESystemDir
        {
            MetaData,
            Config,
            GraphicEffect,
            ComputeEffect,
            RayTracingEffect,
            PSO,
            RenderDoc,
            DebugUtility,
            Count,
        }
        public string BinariesDir { get; private set; }
        public string[] Roots = new string[(int)ERootDir.Count];
        public string[] SysDirs = new string[(int)ESystemDir.Count];
        public string CloudUrlBase = "https://localhost/CloudAssets/";
        public void SetRoot(ERootDir type, string path)
        {
            Roots[(int)type] = GetValidDirectory(path);
            IO.TtFileManager.SureDirectory(Roots[(int)type]);
        }
        public void SetSysDir(ESystemDir type, string path)
        {
            SysDirs[(int)type] = GetValidFileName(path);
        }
        public string GetRoot(ERootDir type)
        {
            return Roots[(int)type];
        }
        public string GetRoot2(RName.ERNameType type)
        {
            switch (type)
            {
                case RName.ERNameType.Engine:
                    return Roots[(int)ERootDir.Engine];
                case RName.ERNameType.Game:
                    return Roots[(int)ERootDir.Game];
                case RName.ERNameType.Cloud:
                    return Roots[(int)ERootDir.Cloud];
            }
            return null;
        }
        public string GetPath(ERootDir root, ESystemDir type)
        {
            return SureAsDirectory(CombinePath(Roots[(int)root], SysDirs[(int)type]));
        }
        public ERootDir GetRootDirType(string path)
        {
            path = GetValidDirectory(path);
            for (ERootDir i= ERootDir.Game; i< ERootDir.Count; i++)
            {
                if (path.StartsWith(GetRoot(i)))
                    return i;
            }
            return ERootDir.Count;
        }
        
        #region Path Op
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

            while (intPos >= 0)
            {
                intPos++;

                if (string.Compare(strPathSrc, 0, strPathTag, 0, intPos, true) != 0)
                    break;

                intIndex = intPos;
                intPos = strPathSrc.IndexOf('/', intPos);
            }

            if (intIndex >= 0)
            {
                strPathTag = strPathTag.Substring(intIndex);
                intPos = strPathSrc.IndexOf('/', intIndex);

                while (intPos >= 0)
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
        public static string GetPureName(string str, int maxWorld = int.MaxValue)
        {
            var result = GetPureNameImpl(str);
            if (result.Length > maxWorld)
            {
                result = result.Substring(0, maxWorld) + "..";
            }
            return result;
        }
        public static string GetPureNameImpl(string str)
        {
            var filename = EngineNS.IO.TtFileManager.GetLastestPathName(str);
            var pos = filename.LastIndexOf('.');
            if (pos < 0)
                return str;
            return filename.Substring(0, pos);
        }
        public static string GetExtName(string str)
        {
            var filename = EngineNS.IO.TtFileManager.GetLastestPathName(str);
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
        public static string SureAsDirectory(string str)
        {
            if (str.EndsWith("/") == false && str.EndsWith("\\") == false)
                str += "/";
            return str;
        }
        public static string CombinePath(string path1, string path2)
        {
            if (path1.EndsWith("\\") || path1.EndsWith("/"))
            {
                path1 = path1.Substring(0, path1.Length - 1);
            }
            if (path2.StartsWith("\\") || path2.StartsWith("/"))
            {
                path2 = path2.Substring(1, path2.Length - 1);
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
                var start = path.LastIndexOf('/', cur);
                if (start < 0)
                    return null;
                path = path.Remove(start, cur + 1 - start + 3);
                cur = path.IndexOf("/..");
            }
            return path;
        }
        #endregion

        #region Directory & File Op
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
            try
            {
                if (System.IO.Directory.Exists(path))
                    System.IO.Directory.Delete(path, recursive);
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteException(ex);
            }
        }
        public static void DeleteFile(string path)
        {
            try
            {
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteException(ex);
            }
        }
        public static void CopyFile(string src, string tar, bool bOverride = true)
        {
            if (System.IO.File.Exists(src))
            {
                System.IO.File.Copy(src, tar, bOverride);
            }
        }
        public static void MoveFile(string src, string tar)
        {
            if (System.IO.File.Exists(src))
            {
                System.IO.File.Move(src, tar);
            }
        }
        #endregion
        public static System.Security.Cryptography.MD5 GetMD5HashFromFile(string fileName)
        {
            try
            {
                var file = new System.IO.FileStream(fileName, System.IO.FileMode.Open);
                System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();
                return md5;
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteException(ex);
                return null; 
                //throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
            }
        }

        #region Stream
        public static string ReadAllText(string file, System.Text.Encoding encoding = null)
        {
            if (System.IO.File.Exists(file) == false)
                return null;
            if (encoding == null)
                return System.IO.File.ReadAllText(file);
            return System.IO.File.ReadAllText(file, encoding);
        }
        public static void WriteAllText(string file, string text)
        {
            System.IO.File.WriteAllText(file, text);
        }

        public static unsafe byte[] ReadAllBytes(string file)
        {
            if (FileExists(file) == false)
                return null;
            using (var r2m = TtRes2Memory.CreateFromFile(file))
            {
                if (r2m==null)
                    return null;
                using (var res = r2m.GetMemory())
                {
                    var result = new byte[res.Size];
                    fixed (byte* p = &result[0])
                    {
                        CoreSDK.MemoryCopy(p, res.Pointer.ToPointer(), (uint)res.Size);
                    }
                    return result;
                }
            }
            //return System.IO.File.ReadAllBytes(file);
        }
        public static void WriteBytes(string file, byte[] data)
        {
            System.IO.File.WriteAllBytes(file, data);
        }
        #endregion

        #region xml
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
        public static unsafe System.Xml.XmlDocument LoadXml(string file)
        {
            if (FileExists(file) == false)
                return null;
            using (var res = TtRes2Memory.CreateFromFile(file))
            {
                if (res == null)
                    return null;
                using (var r2m = res.GetMemory())
                using (var stream = new UnmanagedMemoryStream((byte*)r2m.Pointer.ToPointer(), (long)r2m.Size, (long)r2m.Size, FileAccess.Read))
                {
                    var xml = new System.Xml.XmlDocument();
                    xml.Load(stream);
                    return xml;
                }
            }
        }
        public static System.Xml.XmlDocument LoadXmlFromString(string xmlStr)
        {
            if (string.IsNullOrEmpty(xmlStr))
                return null;
            var xml = new System.Xml.XmlDocument();
            try
            {
                xml.LoadXml(xmlStr);
            }
            catch(System.Exception exp)
            { 
                Profiler.Log.WriteException(exp);
                return null;
            }
            return xml;
        }
        public static void SaveObjectToXml(string file, object obj)
        {
            var xml = new System.Xml.XmlDocument();
            var xmlRoot = xml.CreateElement($"Root", xml.NamespaceURI);
            xml.AppendChild(xmlRoot);
            IO.SerializerHelper.WriteObjectMetaFields(xml, xmlRoot, obj);
            var xmlText = IO.TtFileManager.GetXmlText(xml);
            IO.TtFileManager.WriteAllText(file, xmlText);
        }
        public static object LoadXmlToObject(string file, System.Type type)
        {
            var xml = IO.TtFileManager.LoadXml(file);
            if (xml == null)
                return null;
            object pThis = Rtti.TtTypeDescManager.CreateInstance(type);
            IO.SerializerHelper.ReadObjectMetaFields(null, xml.LastChild as System.Xml.XmlElement, ref pThis, null);
            return pThis;
        }
        public static object LoadXmlToObject(string file)
        {
            var xml = IO.TtFileManager.LoadXml(file);
            if (xml == null)
                return null;
            Rtti.TtTypeDesc type = null;
            foreach(System.Xml.XmlAttribute i in xml.LastChild.Attributes)
            {
                if (i.Name == "Type")
                {
                    type = Rtti.TtTypeDesc.TypeOf(i.Value);
                    break;
                }
            }
            if (type == null)
                return null;
            object pThis = Rtti.TtTypeDescManager.CreateInstance(type);
            IO.SerializerHelper.ReadObjectMetaFields(null, xml.LastChild as System.Xml.XmlElement, ref pThis, null);
            return pThis;
        }
        public static T LoadXmlToObject<T>(string file) where T : class, new()
        {
            var xml = IO.TtFileManager.LoadXml(file);
            if (xml == null)
                return null;
            object pThis = null;
            IO.SerializerHelper.ReadObjectMetaFields(null, xml.LastChild as System.Xml.XmlElement, ref pThis, null);
            return (T)pThis;
        }
        public static bool LoadXmlToObject<T>(string file, T host) where T : class, new()
        {
            var xml = IO.TtFileManager.LoadXml(file);
            if (xml == null)
                return false;
            object pThis = host;
            IO.SerializerHelper.ReadObjectMetaFields(null, xml.LastChild as System.Xml.XmlElement, ref pThis, null);
            return true;
        }
        #endregion

        #region json
        public static System.Text.Json.Nodes.JsonNode LoadJsonFromString(string jsonString)
        {
            return System.Text.Json.Nodes.JsonNode.Parse(jsonString);
        }
        public static string SaveJson(System.Text.Json.Nodes.JsonNode jsNode)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            return jsNode!.ToJsonString(options);
        }
        public static string SaveObjectToJson(object obj)
        {
            string jsonString = JsonSerializer.Serialize(obj, TtJsonOptions.Options);
            return jsonString;
        }
        public static T LoadObjectFromJson<T>(string jsonStr)
        {
            return JsonSerializer.Deserialize<T>(jsonStr, TtJsonOptions.Options);
        }
        public static object LoadObjectFromJson(System.Type type, string jsonStr)
        {
            return JsonSerializer.Deserialize(jsonStr, type, TtJsonOptions.Options);
        }
        #endregion
    }

    public partial class TtOpenFileDialog
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

namespace EngineNS.UnitTest
{
    [TtTest]
    public class UTest_FileManager
    {
        public void UnitTestEntrance()
        {
            var path = IO.TtFileManager.CombinePath("abc/cda\\bad/", "fdsa/dac");

            var path2 = IO.TtFileManager.CombinePath("abc/cda\\bad/", "../fdsa/dac");

            var path3 = IO.TtFileManager.CombinePath("abc/cda\\bad/", "/../fdsa/dac");
        }
    }
}
