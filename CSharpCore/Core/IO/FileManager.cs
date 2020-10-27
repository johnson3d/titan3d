using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;

namespace EngineNS.IO
{
    public enum EFileType : int
    {
        Unknown,
        Texture,
        Mesh,
        DataTabel,
        Material,
        Xml,
        Xnd,
    }

    enum OpenFlags
    {
        modeRead = 0x0000,
        modeWrite = 0x0001,
        modeReadWrite = 0x0002,
        shareCompat = 0x0000,
        shareExclusive = 0x0010,
        shareDenyWrite = 0x0020,
        shareDenyRead = 0x0030,
        shareDenyNone = 0x0040,
        modeNoInherit = 0x0080,
        modeCreate = 0x1000,
        modeNoTruncate = 0x2000,
        typeText = 0x4000, // typeText and typeBinary are used in
        typeBinary = (Int32)0x8000 // derived classes only
    };

    public partial class FileManager
    {
        public string Root
        {
            get
            {
                System.Diagnostics.Debug.Assert(false);
                return null;
            }
        }
        string mProjectSourceRoot;
        //游戏项目源代码根目录
        public string ProjectSourceRoot
        {
            get { return mProjectSourceRoot; }
        }
        string mBin;
        public string Bin
        {
            get { return mBin; }
        }

        string mEngineRoot;
        public string EngineRoot
        {
            //编辑器模式下，EngineRoot和ProjectRoot不是同一个路径，但是游戏模式下是一致的
            //游戏模式下，引擎binaries下的执行模块都会被copy到项目binaries
            get { return mEngineRoot; }
        }
        string mProjectRoot;
        public string ProjectRoot
        {
            get { return mProjectRoot; }
        }
        string mEngineContent;
        public string EngineContent
        {
            get => mEngineContent;
        }
        string mEditorContent;
        public string EditorContent
        {
            get => mEditorContent;
        }
        string mProjectContent;
        public string ProjectContent
        {
            get => mProjectContent;
        }
        string mDDCDirectory;
        public string DDCDirectory
        {
            get => mDDCDirectory;
        }
        string mCooked;
        public string Cooked
        {
            get { return mCooked; }
        }
        public string CookedTemp
        {
            get { return mCooked + "temp/"; }
        }
        public string CookingPlatform;
        public string CookingRoot
        {
            get
            {
                if(CookingPlatform=="android")
                    return Cooked + CookingPlatform + "/Assets/";
                else
                    return Cooked + CookingPlatform + "/";
            }
        }
        //public string ProjectEditorContent
        //{
        //    get => mProjectRoot + "ProjectEditorContent/";
        //}
        public string[] AllContents
        {
            get
            {
                return new string[] { ProjectContent, EngineContent, EditorContent };//ProjectEditorContent, 
            }
        }
        public string[] AllContentsWithoutEditor
        {
            get
            {
                return new string[] { ProjectContent, EngineContent };
            }
        }

        #region Directory&File
        IO.VrDirectory mAssetsVrDirectory = null;
        public IO.VrDirectory AssetsVrDirectory
        {
            get
            {
                if(mAssetsVrDirectory==null)
                {
                    if(CIPlatform.Instance.PlayMode == CIPlatform.enPlayMode.Game)
                    {
                        mAssetsVrDirectory = EngineNS.IO.VrDirectory.LoadVrDirectory(RName.GetRName("assetinfos.xml", RName.enRNameType.Game).Address, true);
                        mAssetsVrDirectory.UpdateFullName(mProjectRoot);
                    }
                }
                return mAssetsVrDirectory;
            }
        }
        public List<string> GetDirectories(string path, string searchPattern = "*", SearchOption searchOp = SearchOption.TopDirectoryOnly)
        {
            List<string> result = new List<string>();

            if (AssetsVrDirectory != null)
            {
                if (path.StartsWith(AssetsVrDirectory.FullName))
                    path = path.Substring(AssetsVrDirectory.FullName.Length);

                var findRoot = AssetsVrDirectory.GetDirectory(path);
                if (findRoot == null)
                    return result;

                List<VrDirectory> dirs = new List<VrDirectory>();
                findRoot.GetDirectorys(dirs, searchPattern, searchOp);

                foreach (var i in dirs)
                {
                    result.Add(i.FullName.TrimEnd('/'));
                }
            }

            if (System.IO.Directory.Exists(path))
            {
                var realFiles = System.IO.Directory.GetDirectories(path, searchPattern, searchOp);
                foreach(var i in realFiles)
                {
                    if (result.Contains(i) == false)
                        result.Add(i);
                }
            }

            return result;
        }

        public List<string> GetFiles(string path, string fileKeyName, SearchOption searOp)
        {
            List<string> result = new List<string>();

            if (AssetsVrDirectory != null)
            {
                if (path.StartsWith(AssetsVrDirectory.FullName))
                    path = path.Substring(AssetsVrDirectory.FullName.Length);

                var findRoot = AssetsVrDirectory.GetDirectory(path);
                if (findRoot == null)
                    return result;

                List<VrFile> files = new List<VrFile>();
                findRoot.GetFiles(files, fileKeyName, searOp);

                foreach (var i in files)
                {
                    var fileKeyName1 = fileKeyName.Replace("*", "").ToLower();
                    //System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"*");
                    if (i.Name.IndexOf(fileKeyName1) != -1)
                    {
                        result.Add(i.FullName);
                    }
                }
            }

            if (System.IO.Directory.Exists(path))
            {
                var realFiles = System.IO.Directory.GetFiles(path, fileKeyName, searOp);
                foreach (var i in realFiles)
                {
                    if (result.Contains(i) == false)
                        result.Add(i);
                }
            }

            return result;
        }

        public bool FileExistsImpl(string name)
        {
            //bool error = false;
            //name = this.NormalizePath(name, out error);
            //if (error == true)
            //    return false;

            if (AssetsVrDirectory != null)
            {
                if (name.StartsWith(AssetsVrDirectory.FullName))
                {
                    name = name.Substring(AssetsVrDirectory.FullName.Length);
                    return AssetsVrDirectory.GetFile(name) != null;
                }
            }

            if (System.IO.File.Exists(name))
                return true;

            return false;
        }
        #endregion

        Dictionary<string, Resource2Memory> mReadOnlyFiles = new Dictionary<string, Resource2Memory>();
        ~FileManager()
        {
            Cleanup();
        }
        public void RemoveFromManager(string file)
        {
            Resource2Memory f2m;
            if (mReadOnlyFiles.TryGetValue(file, out f2m))
            {
                mReadOnlyFiles.Remove(file);
            }
        }

        partial void InitPaths();

        public void Initialize()
        {
            InitPaths();

            HLSLFileDescManager.InitFiles();

            //FreshRName
            foreach (var i in RNameManager.Values)
            {
                if(System.IO.Path.IsPathRooted(i.Address)==false)
                {
                    i.FixAddressDirect();
                }
            }
        }

        public CHLSLFileDescManager HLSLFileDescManager
        {
            get;
        } = new CHLSLFileDescManager();

        // 编辑器特殊接口，请勿随意调用
        public void Editor_Initialize(string binPath)
        {
            mBin = binPath.Replace("\\", "/").TrimEnd('/');
            mBin += "/";
        }

        public void Cleanup()
        {
            mReadOnlyFiles.Clear();
        }
        public string GetPathFromFullName(string file, bool change2FullPath = true)
        {
            if (string.IsNullOrEmpty(file))
                return "";

            var _file = file.Replace("\\", "/");
            _file = _file.TrimEnd('/');
            int pos = _file.LastIndexOf('/');

#if PWindow
            if (System.IO.Path.IsPathRooted(file))	// 全路径，包含盘符
                return _file.Substring(0, pos + 1);
            else							// 相对路径
            {
                if (change2FullPath)
                {
                    if (_file.StartsWith("content/"))
                    {
                        var retStr = ProjectContent.Substring(0, ProjectContent.Length - "content/".Length) + _file.Substring(0, pos + 1);
                        return retStr.Replace("//", "/");
                    }
                    else if (_file.StartsWith("enginecontent/"))
                    {
                        var retStr = ProjectContent.Substring(0, ProjectContent.Length - "enginecontent/".Length) + _file.Substring(0, pos + 1);
                        return retStr.Replace("//", "/");
                    }
                    else if (_file.StartsWith("editorcontent/"))
                    {
                        var retStr = ProjectContent.Substring(0, ProjectContent.Length - "editorcontent/".Length) + _file.Substring(0, pos + 1);
                        return retStr.Replace("//", "/");
                    }
                    else
                    {
                        return _file.Substring(0, pos + 1);
                    }
                }
                else
                    return _file.Substring(0, pos + 1);
            }
#elif PlatformIOS
            string path = string.Empty;
            if (System.IO.Path.IsPathRooted(file))
            {
                //path = _file.Substring(0, pos + 1).ToLower();
                path = _file.Substring(0, pos + 1);
            }
            else
            {
                if(change2FullPath)
                    path = mRoot + _file.Substring(0, pos + 1).ToLower();
                else
                    path = _file.Substring(0, pos + 1).ToLower();
            }
            return path; //TransCrossPlateFormPath(path);
#else
            if (_file.Contains(mProjectRoot))
            {
                return _file.Substring(0, pos + 1);
            }
            else
            {
                if(change2FullPath)
                    return mProjectRoot + _file.Substring(0, pos + 1).ToLower();
                else
                    return _file.Substring(0, pos + 1).ToLower();
            }
#endif
        }

        public string NormalizePath(string path, out bool error)
        {
            error = false;

            int UpDirLength = "../".Length;

            path = path.Replace("\\", "/");

            path = path.Replace("./", "");

            path = path.ToLower();

            int startPos = path.LastIndexOf("../");
            while(startPos>=0)
            {
                int rmvNum = 1;
                var head = path.Substring(0, startPos);
                var tail = path.Substring(startPos + UpDirLength);
                while (head.Length> UpDirLength && head.EndsWith("../"))
                {
                    rmvNum++;
                }
                if (head.Length - UpDirLength * rmvNum < 0)
                {
                    error = true;
                    return null;
                }
                head = head.Substring(0, head.Length - UpDirLength * rmvNum);
                int discardPos = -1;
                for(int i=0; i<rmvNum; i++)
                {
                    discardPos = head.LastIndexOf("/");
                    if(discardPos<0)
                    {
                        error = true;
                        return null;
                    }
                }
                head = head.Substring(0, discardPos);
                path = head + tail;

                startPos = path.LastIndexOf("../");
            }
            
            return path;
        }

        public string GetPureFileFromFullName(string file, bool containExt = true)
        {
            if (string.IsNullOrEmpty(file))
                return "";

            var _file = file.Replace("\\", "/");
            int pos = _file.LastIndexOf('/');
            if (containExt)
                return _file.Substring(pos + 1);//,file->Length-pos-1);
            else
            {
                var idx = _file.LastIndexOf('.');
                if (idx > -1 && idx > pos)
                {
                    return _file.Substring(pos + 1, idx - pos - 1);
                }
                return _file.Substring(pos + 1);
            }
        }
        public string RemoveExtension(string file)
        {
            if (string.IsNullOrEmpty(file))
                return "";

            var _file = file.Replace("\\", "/");
            int pos = _file.LastIndexOf('.');
            return _file.Substring(0,pos);
        }
        public string GetFileExtension(string file, bool withDot = false)
        {
            if (string.IsNullOrEmpty(file))
                return "";

            var _file = file.Replace("\\", "/");
            int pos = _file.LastIndexOf('.');
            if (pos < 0)
                return "";
            if (withDot)
                return _file.Substring(pos);
            else
                return _file.Substring(pos + 1);
        }

        public bool IsFileEqual(string file1, string file2)
        {
            if (string.Equals(file1, file2))
                return true;

            if (string.IsNullOrEmpty(file1) || string.IsNullOrEmpty(file2))
                return false;

            var f1 = file1.Replace("\\", "/");
            var f2 = file2.Replace("\\", "/");
            return f1 == f2;
        }
        public bool IsDirectoryEqual(string dir1, string dir2)
        {
            string d1 = dir1.Replace("\\", "/");
            string d2 = dir2.Replace("\\", "/");
            if (d1[d1.Length - 1] == '/')
                d1 = d1.Remove(d1.Length - 1);
            if (d2[d2.Length - 1] == '/')
                d2 = d2.Remove(d2.Length - 1);
            return d1 == d2;
        }

        public string GetCommonDirectory(string dir1, string dir2)
        {
            dir1 = dir1.Replace("\\", "/");
            dir2 = dir2.Replace("\\", "/");

            var dirSplits1 = dir1.Split('/');
            var dirSplits2 = dir2.Split('/');

            string outDirectory = "";
            var min = System.Math.Min(dirSplits1.Length, dirSplits2.Length);
            for (int i = 0; i < min; i++)
            {
                if (string.Equals(dirSplits1[i], dirSplits2[i]))
                    outDirectory += dirSplits1[i] + "/";
            }

            return outDirectory;
        }

        public string _GetRelativePathFromAbsPath(string file)
        {
            return _GetRelativePathFromAbsPath(file, "");
            //    pin_ptr<const System::Char> rootPath = PtrToStringChars(mRoot);
            //    pin_ptr<const System::Char> absPath = PtrToStringChars(file);
            //    VString str = _vfxFileGetRelativePath( rootPath , absPath );
            //    return gcnew System::String(str);
        }

        public string _GetRelativePathFromAbsPath(string file, string cPath = "")
        {
            if (string.IsNullOrEmpty(cPath))
                cPath = mProjectContent;

            if (file == cPath)
                return "";

            string strPath2 = _GetAbsPathFromRelativePath(file);
            string strPath1 = _GetAbsPathFromRelativePath(cPath);

            strPath1 = strPath1.Replace("\\", "/");
            strPath2 = strPath2.Replace("\\", "/");

            if (strPath1 == strPath2)
                return "";

            if (!strPath1.EndsWith("/"))
                strPath1 += "/";
            int intIndex = -1;
            int intPos = strPath1.IndexOf("/");

            while (intPos >= 0)
            {
                intPos++;

                if (string.Compare(strPath1, 0, strPath2, 0, intPos, true) != 0)
                    break;

                intIndex = intPos;
                intPos = strPath1.IndexOf("/", intPos);
            }

            if (intIndex >= 0)
            {
                strPath2 = strPath2.Substring(intIndex);
                intPos = strPath1.IndexOf("/", intIndex);

                while (intPos >= 0)
                {
                    strPath2 = "../" + strPath2;
                    intPos = strPath1.IndexOf("/", intPos + 1);
                }
            }

            return strPath2.ToLower();
        }

        public string _GetAbsPathFromRelativePath(string file)
        {
            if (string.IsNullOrEmpty(file))
                return "";

            if (System.IO.Path.IsPathRooted(file))
                return file.Replace('\\', '/');

            RName.enRNameType rType = RName.enRNameType.Game;
            file = file.ToLower();
            file.Replace('\\', '/');
            if (file.StartsWith("content/"))
            {
                rType = RName.enRNameType.Game;
                file = file.Substring("content/".Length);
            }
            else if (file.StartsWith("enginecontent/"))
            {
                rType = RName.enRNameType.Engine;
                file = file.Substring("enginecontent/".Length);
            }
            else if (file.StartsWith("editorcontent/"))
            {
                rType = RName.enRNameType.Editor;
                file = file.Substring("editorcontent/".Length);
            }

            switch (rType)
            {
                case RName.enRNameType.Game:
                    return System.IO.Path.Combine(ProjectContent, file);
                case RName.enRNameType.Engine:
                    return System.IO.Path.Combine(EngineContent, file);
                case RName.enRNameType.Editor:
                    return System.IO.Path.Combine(EditorContent, file);
                default:
                    return System.IO.Path.Combine(EngineContent, file);
            }
        }
        

        public Resource2Memory OpenResource2Memory(string file, EFileType type, bool bHold)
        {
            Resource2Memory f2m;
            if (mReadOnlyFiles.TryGetValue(file, out f2m))
            {
                return f2m;
            }
            string finalStr = _GetAbsPathFromRelativePath(file);

            var res = VFile2Memory_F2M(finalStr, false);
            if (res.Pointer == IntPtr.Zero)
                return null;
            f2m = new Resource2Memory(res, bHold);
            f2m.mType = type;
            if (bHold)
            {   
                mReadOnlyFiles.Add(file, f2m);
            }
            return f2m;
        }
        public FileReader OpenFileForRead(string file, EFileType type)
        {
            unsafe
            {
                var result = new FileReader();

                var fileName = _GetAbsPathFromRelativePath(file);
                if (result.OpenRead(fileName) == false)
                    return null;

                return result;
            }
        }
        public FileWriter OpenFileForWrite(string file, EFileType type)
        {
            unsafe
            {
                var result = new FileWriter();

                var fileName = _GetAbsPathFromRelativePath(file);
                if (result.OpenWrite(fileName) == false)
                    return null;

                return result;
            }
        }

        public System.Text.Encoding GetEncoding(string absFileName)
        {
            if (FileExists(absFileName))
                return System.Text.Encoding.Default;
            var stream = new System.IO.FileStream(absFileName, System.IO.FileMode.Open);

            byte[] Unicode = new byte[] { 0xFF, 0xFE, 0x41 };
            byte[] UnicodeBIG = new byte[] { 0xFE, 0xFF, 0x00 };
            byte[] UTF8 = new byte[] { 0xEF, 0xBB, 0xBF }; //带BOM   
            Encoding reVal = Encoding.Default;

            BinaryReader r = new BinaryReader(stream, System.Text.Encoding.Default);
            byte[] ss = r.ReadBytes(4);
            if (ss[0] == 0xFE && ss[1] == 0xFF && ss[2] == 0x00)
            {
                reVal = Encoding.BigEndianUnicode;
            }
            else if (ss[0] == 0xFF && ss[1] == 0xFE && ss[2] == 0x41)
            {
                reVal = Encoding.Unicode;
            }
            else
            {
                if (ss[0] == 0xEF && ss[1] == 0xBB && ss[2] == 0xBF)
                {
                    reVal = Encoding.UTF8;
                }
                else
                {
                    int i;
                    int.TryParse(stream.Length.ToString(), out i);
                    ss = r.ReadBytes(i);

                    if (IsUTF8Bytes(ss))
                        reVal = Encoding.UTF8;
                }
            }
            r.Close();
            return reVal;
        }

        private static bool IsUTF8Bytes(byte[] data)
        {
            int charByteCounter = 1;　 //计算当前正分析的字符应还有的字节数   
            byte curByte; //当前分析的字节.   
            for (int i = 0; i < data.Length; i++)
            {
                curByte = data[i];
                if (charByteCounter == 1)
                {
                    if (curByte >= 0x80)
                    {
                        //判断当前   
                        while (((curByte <<= 1) & 0x80) != 0)
                        {
                            charByteCounter++;
                        }
                        //标记位首位若为非0 则至少以2个1开始 如:110XXXXX...........1111110X　   
                        if (charByteCounter == 1 || charByteCounter > 6)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    //若是UTF-8 此时第一位必须为1   
                    if ((curByte & 0xC0) != 0x80)
                    {
                        return false;
                    }
                    charByteCounter--;
                }
            }
            if (charByteCounter > 1)
            {
                throw new Exception("非预期的byte格式!");
            }
            return true;
        }
        /// <summary>
        /// 根据文件特征名从指定文件夹获取文件全路径
        /// </summary>
        /// <param name="absDir">要查找的文件绝对路径</param>
        /// <returns>找到的文件</returns>
        public List<string> GetFiles(string absDir)
        {
            return GetFiles(absDir, "*.*", SearchOption.TopDirectoryOnly);
        }

        public List<string> GetFiles(string path, string searchPattern)
        {
            return GetFiles(path, searchPattern, SearchOption.AllDirectories);
        }

        public static string NormalizePath(string path)
        {
            path = path.Replace("\\", "/");
            string keyWord = "../";
            int idx = 0;
            do
            {
                idx = path.IndexOf(keyWord);
                if (idx >= 0)
                {
                    var cIdx = path.LastIndexOf('/', idx - 2);
                    path = path.Remove(cIdx, idx - cIdx + keyWord.Length - 1);
                }
            } while (idx >= 0);
            return path;
        }

        public DDCDataManager DDCManager
        {
            get;
        } = new DDCDataManager();

#region Platform API
        public bool DirectoryExists(string absFolder)
        {
            return System.IO.Directory.Exists(absFolder);
        }
        public bool FileExists(string absFileName)
        {
            return FileExistsImpl(absFileName);
        }
        public static byte[] ReadFile(string file)
        {
            //if (System.IO.File.Exists(file))
            //{
            //    return System.IO.File.ReadAllBytes(file);
            //}
            //else
            //{
            //    List<byte> result = new List<byte>();
            //    EngineNS.CIPlatform.Instance.ReadAllBytes(file, ref result);
            //    return result.ToArray();
            //}

            var res = VFile2Memory_F2M(file, false);
            if (res.Pointer == IntPtr.Zero)
                return null;
            var fr = new Resource2Memory(res, false);

            fr.BeginRead();
            byte[] result = new byte[(int)fr.GetLength()];
            if (result.Length > 0)
            {
                unsafe
                {
                    fixed (byte* pData = &result[0])
                    {
                        CoreSDK.SDK_Memory_Copy(pData, fr.NativePtr.ToPointer(), (uint)result.Length);
                    }
                }
            }
            fr.EndRead();

            return result;
        }
        public bool CopyFile(string srcAbsFile, string tagAbsFile, bool overrideIfExist, bool autoCreateFolder = true)
        {
            ////////if(srcAbsFile.Length >= 248)
            ////////    srcAbsFile = @"\\?\" + srcAbsFile;
            ////////if(tagAbsFile.Length >= 248)
            ////////    tagAbsFile = @"\\?\" + tagAbsFile;
            //////try
            //////{
            //////    var result = DllImportAPI.VFile_CopyFile(srcAbsFile, tagAbsFile, overrideIfExist ? 1 : 0);
            //////    if (result != 0)
            //////    {
            //////        throw new InvalidOperationException($"{new System.ComponentModel.Win32Exception((int)result).Message}: CopyFile({srcAbsFile}, {tagAbsFile})");
            //////    }
            //////}
            //////catch (System.Exception ex)
            //////{
            //////    System.Diagnostics.Trace.WriteLine(ex.ToString());
            //////}
            if (!System.IO.File.Exists(srcAbsFile))
                return false;
            if(autoCreateFolder)
            {
                var path = GetPathFromFullName(tagAbsFile);
                if (!DirectoryExists(path))
                    CreateDirectory(path);
            }
            try
            {
                System.IO.File.Copy(srcAbsFile, tagAbsFile, overrideIfExist);
            }
            catch(System.Exception ex)
            {
                Profiler.Log.WriteException(ex);
            }
            return true;
        }
        public void MoveFile(string srcAbsFile, string tagAbsFile)
        {
            if (!System.IO.File.Exists(srcAbsFile))
                return;
            System.IO.File.Copy(srcAbsFile, tagAbsFile, true);
            System.IO.File.Delete(srcAbsFile);
        }
        public bool DeleteFile(string absFileName)
        {
            //var result = DllImportAPI.VFile_DeleteFile(absFileName);
            //if (result != 0)
            //{
            //    throw new InvalidOperationException(new System.ComponentModel.Win32Exception((int)result).Message + ": DeleteFile(" + absFileName + ")");
            //}
            try
            {
                if (System.IO.File.Exists(absFileName))
                    System.IO.File.Delete(absFileName);
            }
            catch
            {
                return false;
            }
            return true;
        }
        public void DeleteFilesInDirectory(string absPath, string searchPattern, SearchOption opt)
        {
            var files = System.IO.Directory.GetFiles(absPath, searchPattern, opt);
            foreach(var i in files)
            {
                System.IO.File.Delete(i);
            }
        }
        public void CreateFile(string absFileName)
        {
            System.IO.File.Create(absFileName);
        }
        public void CreateFileAndFlush(string absFileName)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(absFileName, true))
            {
                file.Flush();
                file.Close();
            }
        }
        public void WriteAllTextASCII(string absFileName, string text)
        {
            using (FileStream file = new FileStream(absFileName, FileMode.Create))
            {
                var strData = System.Text.ASCIIEncoding.ASCII.GetBytes(text);
                file.Write(strData, 0, strData.Length);
                file.Flush();
                file.Close();
            }
        }
        public void CreateDirectory(string absDir)
        {
            System.IO.Directory.CreateDirectory(absDir);
        }
        public void DeleteDirectory(string absDir, bool recursive)
        {
            System.IO.Directory.Delete(absDir, recursive);
        }
        public void MoveDirectory(string absDir, string absTagDir)
        {
            System.IO.Directory.Move(absDir, absTagDir);
        }
        public void CopyDirectory(string absDir, string absTagDir)
        {
            if (!DirectoryExists(absTagDir))
                CreateDirectory(absTagDir);

            absTagDir = absTagDir.Replace("\\", "/");
            absTagDir = absTagDir.TrimEnd('/') + "/";
            var files = GetFiles(absDir);
            foreach(var file in files)
            {
                var fileName = GetPureFileFromFullName(file);
                CopyFile(file, absTagDir + fileName, true);
            }
            var dirs = GetDirectories(absDir);
            foreach(var dir in dirs)
            {
                var dirName = GetPureFileFromFullName(dir);
                CopyDirectory(dir, absTagDir + dirName);
            }
        }
#endregion

#region SDK
        const string ModuleNC = CoreObjectBase.ModuleNC;
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static Resource2Memory.NativePointer VFile2Memory_F2M(string psz, bool bShareWrite);
        #endregion
    }

    public class CHLSLFileDescManager
    {
        public class HLSLFileDesc
        {
            public string Directory;
            public string FileName;
            public string FullName
            {
                get { return Directory + FileName; }
            }
            public string HLSLCode;
            public Hash64 HashCode;
            public Hash64 HashWithDepends;
            public Dictionary<string, HLSLFileDesc> Depends = new Dictionary<string, HLSLFileDesc>();
        }
        public Dictionary<string, HLSLFileDesc> FileDescDict
        {
            get;
        } = new Dictionary<string, HLSLFileDesc>();
        public void InitFiles()
        {
#if PWindow
            if (EngineNS.IO.FileManager.UseCooked != null)
                return;

            var files = CEngine.Instance.FileManager.GetFiles(RName.GetRName("Shaders", RName.enRNameType.Engine).Address, "*.*", SearchOption.AllDirectories);
            if (files == null)
                return;
            foreach (var f in files)
            {
                bool error = false;
                var i = CEngine.Instance.FileManager.NormalizePath(f, out error);
                if (error)
                    continue;
                if (i.EndsWith(".shaderinc") ||
                    i.EndsWith(".cginc") ||
                    i.EndsWith(".shadingenv") ||
                    i.EndsWith(".compute"))
                {
                    var desc = new HLSLFileDesc();
                    desc.Directory = CEngine.Instance.FileManager.GetPathFromFullName(i);
                    desc.FileName = CEngine.Instance.FileManager.GetPureFileFromFullName(i);
                    desc.HLSLCode = System.IO.File.ReadAllText(i);
                    Hash64.CalcHash64(ref desc.HashCode, desc.HLSLCode);
                    FileDescDict.Add(desc.Directory + desc.FileName, desc);
                }
            }
            foreach(var i in FileDescDict)
            {
                GetDepends(i.Value);
            }

            foreach (var i in FileDescDict)
            {
                if(i.Key.EndsWith(".shadingenv") || i.Key.EndsWith(".compute"))
                {
                    var allDepends = new List<HLSLFileDesc>();
                    CollectDependTree(i.Value, allDepends);
                    allDepends.Sort((left, right) =>
                    {
                        return left.FullName.CompareTo(right.FullName);
                    });
                    string AllCode = i.Value.HLSLCode;
                    foreach(var j in allDepends)
                    {
                        AllCode += j.HLSLCode;
                    }
                    Hash64.CalcHash64(ref i.Value.HashWithDepends, AllCode);
                }
            }
#endif
        }
        private void GetDepends(HLSLFileDesc desc)
        {
            var code = desc.HLSLCode;
            int keywordLength = "#include".Length;
            int startPos = 0;
            startPos = code.IndexOf("#include", startPos);
            while (startPos >= 0)
            {
                while (code[startPos + keywordLength] == ' ' || code[startPos + keywordLength] == '\t')
                {
                    startPos++;
                }
                int nameStrStart = startPos + keywordLength;
                if (code[nameStrStart] == '\"')
                {
                    if (startPos >= code.Length)
                    {
                        Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Shaders", $"HLSL({desc.FileName}) includer can't match character:\"");
                        return;
                    }
                    startPos++;
                    nameStrStart++;
                    int nameStrEnd = nameStrStart;
                    while (code[nameStrEnd] != '\"')
                    {
                        startPos++;
                        nameStrEnd++;
                    }
                    var dependAddress = code.Substring(nameStrStart, nameStrEnd - nameStrStart);

                    bool error = false;
                    var f = CEngine.Instance.FileManager.NormalizePath(desc.Directory + dependAddress, out error);
                    if(error)
                    {
                        Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Shaders", $"HLSL({desc.FileName}) include {dependAddress} is invalid");
                    }

                    var dependDesc = FindFileDesc(f);
                    if(dependDesc != null && desc.Depends.Keys.Contains(f)==false)
                    {
                        desc.Depends.Add(f, dependDesc);
                    }
                }

                startPos = code.IndexOf("#include", startPos);
            }
        }
        public HLSLFileDesc FindFileDesc(string address)
        {
            if (string.IsNullOrEmpty(address))
                return null;
            address = address.ToLower();
            HLSLFileDesc result;
            if(FileDescDict.TryGetValue(address, out result))
            {
                return result;
            }
            return null;
        }
        private void CollectDependTree(HLSLFileDesc cur, List<HLSLFileDesc> allDepends)
        {
            foreach(var i in cur.Depends)
            {
                if(allDepends.Contains(i.Value)==false)
                {
                    allDepends.Add(i.Value);
                    CollectDependTree(i.Value, allDepends);
                }
            }
        }
    }

    public class DDCDataManager
    {
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct DDCDataDesc
        {
            public Hash160 SourceHash;
            public long UpdateTimeData;
            public int Size;
            public System.DateTime UpdateTime
            {
                get
                {
                    return System.DateTime.FromBinary(UpdateTimeData);
                }
                set
                {
                    UpdateTimeData = value.ToBinary();
                }
            }
        }
        public byte[] GetCacheData(string key, string type, ref Hash160 sourceHash)
        {
            var hash = Hash160.CreateHash160(key);
            var ddcName = CEngine.Instance.FileManager.DDCDirectory + type +  "/" + hash.ToString();
            var fr = CEngine.Instance.FileManager.OpenFileForRead(ddcName, EFileType.Unknown);
            if (fr == null)
                return null;
            var frProxy = new IO.Serializer.FileReaderProxy(fr);

            DDCDataDesc desc = new DDCDataDesc();
            unsafe
            {
                frProxy.Read(out desc);
                if (sourceHash != Hash160.Emtpy && desc.SourceHash != sourceHash)
                {
                    fr.Cleanup();
                    return null;
                }
                byte[] result = new byte[desc.Size];
                unsafe
                {
                    fixed (byte* pTar = &result[0])
                    {
                        frProxy.ReadPtr(pTar, desc.Size);
                    }
                }
                fr.Cleanup();
                return result;
            }
        }
        public bool SetCacheData(string key, string type, ref Hash160 sourceHash, byte[] data)
        {
            var hash = Hash160.CreateHash160(key);
            var ddcName = CEngine.Instance.FileManager.DDCDirectory + type + "/" + hash.ToString();
            var fw = CEngine.Instance.FileManager.OpenFileForWrite(ddcName, EFileType.Unknown);
            if (fw == null)
                return false;

            DDCDataDesc desc = new DDCDataDesc();
            desc.Size = data.Length;
            desc.UpdateTime = System.DateTime.UtcNow;
            System.DateTime.UtcNow.ToBinary();
            desc.SourceHash = sourceHash;
            unsafe
            {
                fw.Write(&desc, (UIntPtr)sizeof(DDCDataDesc));
                fixed (byte* p = &data[0])
                {
                    var writeNum = fw.Write(p, (UIntPtr)data.Length);
                    return ((UIntPtr)data.Length == writeNum);
                }
            }
        }
    }
}
