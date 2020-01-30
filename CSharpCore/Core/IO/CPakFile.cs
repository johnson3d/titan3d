using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace EngineNS.IO
{
    public class CPakFile : AuxCoreObject<CPakFile.NativePointer>
    {
        public struct VPakPair : IDisposable
        {
            internal IntPtr SrcAddress;
            internal IntPtr FullName;
            internal IntPtr MD5;
            internal UInt32 Flags;
            internal UInt32 Pad0;
            public string StrSrcAddress
            {
                get { return System.Runtime.InteropServices.Marshal.PtrToStringAnsi(SrcAddress); }
            }
            public string StrFullName
            {
                get { return System.Runtime.InteropServices.Marshal.PtrToStringAnsi(FullName); }
            }
            public string StrMD5
            {
                get { return System.Runtime.InteropServices.Marshal.PtrToStringAnsi(MD5); }
            }
            public void Dispose()
            {
                if (SrcAddress != IntPtr.Zero)
                {
                    System.Runtime.InteropServices.Marshal.FreeHGlobal(SrcAddress);
                    SrcAddress = IntPtr.Zero;
                }
                if (FullName != IntPtr.Zero)
                {
                    System.Runtime.InteropServices.Marshal.FreeHGlobal(FullName);
                    FullName = IntPtr.Zero;
                }
                if (MD5 != IntPtr.Zero)
                {
                    System.Runtime.InteropServices.Marshal.FreeHGlobal(MD5);
                    MD5 = IntPtr.Zero;
                }
                Flags = 0;
            }
            public static VPakPair CreatePair(string address, string name, string md5, UInt32 flags)
            {
                VPakPair tmp;
                tmp.Flags = flags;
                tmp.Pad0 = 0;
                unsafe
                {
                    tmp.SrcAddress = System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(address);
                    tmp.FullName = System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(name);
                    tmp.MD5 = System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(md5);
                }
                return tmp;
            }
        }
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
        public CPakFile()
        {
            //mCoreObject = NewNativeObjectByNativeName<NativePointer>("VPakFile");
            mCoreObject = SDK_VPakFile_New();
        }
        public static bool BuildPakFile(VPakPair[] pairs, string pakFile)
        {
            var xml = IO.XmlHolder.NewXMLHolder("AssetsPackage", "");
            var root = xml.RootNode;
            foreach(var i in pairs)
            {
                var cur = root;
                var segs = i.StrFullName.Split('/');
                for(int j = 0; j < segs.Length - 1; j++)
                {
                    var child = cur.FindNode((curNode) =>
                    {
                        var attr = curNode.FindAttrib("Name");
                        if(attr!=null)
                        {
                            return attr.Value == segs[j];
                        }
                        return false;
                    });
                    if(child == null)
                    {
                        child = cur.AddNode("Folder", null, xml);
                        child.AddAttrib("Name", segs[j]);
                    }
                    cur = child;
                }
                var fileNode = cur.FindNode("Files");
                if (fileNode == null)
                    fileNode = cur.AddNode("Files", null, xml);
                var fn = fileNode.FindNode((curNode) =>
                {
                    var attr = curNode.FindAttrib("Name");
                    return (attr.Value == segs[segs.Length - 1]);
                });
                if (fn == null)
                {
                    fn = fileNode.AddNode("Name", null, xml);
                    fn.AddAttrib("Name", segs[segs.Length - 1]);
                    fn.AddAttrib("MD5", i.StrMD5);
                }
            }
            IO.XmlHolder.SaveXML(pakFile + ".vtree", xml);

            unsafe
            {
                fixed (VPakPair* p = &pairs[0])
                {
                    return SDK_VPakFile_BuildPakFile(p, (UInt32)pairs.Length, pakFile);
                }
            }
        }
        public bool LoadPak(string name)
        {
            return SDK_VPakFile_LoadPak(CoreObject, name);
        }
        public bool MountPak(string mountPoint, int order)
        {
            return SDK_VPakFile_MountPak(CoreObject, mountPoint, order);
        }
        public bool UnMountPak()
        {
            return SDK_VPakFile_UnMountPak(CoreObject);
        }

        public UInt32 AssetNum
        {
            get
            {
                return SDK_VPakFile_GetAssetsNum(CoreObject);
            }
        }
        public UInt32 GetAssetSize(UInt32 index)
        {
            return SDK_VPakFile_GetAssetSize(CoreObject, index);
        }
        public UInt32 GetAssetSizeInPak(UInt32 index)
        {
            return SDK_VPakFile_GetAssetSizeInPak(CoreObject, index);
        }
        public string GetAssetName(UInt32 index)
        {
            var ptr = SDK_VPakFile_GetAssetName(CoreObject, index);
            return System.Runtime.InteropServices.Marshal.PtrToStringAnsi(ptr);
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern unsafe static NativePointer SDK_VPakFile_New();
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern unsafe static vBOOL SDK_VPakFile_BuildPakFile(VPakPair* pairs, UInt32 count, string pakFile);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_VPakFile_LoadPak(NativePointer self, string name);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_VPakFile_MountPak(NativePointer self, string mountPoint, int order);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_VPakFile_UnMountPak(NativePointer self);

        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_VPakFile_GetAssetsNum(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_VPakFile_GetAssetSize(NativePointer self, UInt32 index);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_VPakFile_GetAssetSizeInPak(NativePointer self, UInt32 index);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static IntPtr SDK_VPakFile_GetAssetName(NativePointer self, UInt32 index);
        #endregion
    }
    //public interface ICookable
    //{
    //    bool IsCooked();
    //    void Cook2Xnd(IO.XndNode node);
    //}

    public class VrFile : IComparable
    {
        public string Name;
        public string FullName;
        public VrDirectory Parent;
        public object ExtObject;
        public int CompareTo(object obj)
        {
            var rh = obj as VrFile;
            return Name.CompareTo(rh.Name);
        }
    }
    public class VrDirectory : IComparable
    {
        public string Name;
        public string FullName;
        public VrDirectory Parent;
        public Dictionary<string, VrFile> Files = new Dictionary<string, VrFile>();
        public Dictionary<string, VrDirectory> Folders = new Dictionary<string, VrDirectory>();
        public object ExtObject;
        public int CompareTo(object obj)
        {
            var rh = obj as VrDirectory;
            return Name.CompareTo(rh.Name);
        }
        public VrFile FindFile(string name)
        {
            VrFile result;
            if (Files.TryGetValue(name, out result))
                return result;

            return null;
        }
        public VrDirectory FindDirectory(string name)
        {
            VrDirectory result;
            if (Folders.TryGetValue(name, out result))
                return result;

            return null;
        }

        public VrFile GetFile(string path)
        {
            bool error = false;
            path = CEngine.Instance.FileManager.NormalizePath(path, out error);
            if (error)
                return null;

            var segs = path.Split('/');

            VrDirectory dir = this;
            for (int i=0; i<segs.Length; i++)
            {
                if(i==segs.Length-1)
                {
                    return dir.FindFile(segs[i]);
                }
                dir = dir.FindDirectory(segs[i]);
                if (dir == null)
                    break;
            }
            return null;
        }

        public VrDirectory GetDirectory(string path)
        {
            bool error = false;
            path = CEngine.Instance.FileManager.NormalizePath(path, out error);
            if (error)
                return null;

            var segs = path.TrimStart('/').TrimEnd('/').Split('/');

            VrDirectory dir = this;
            for (int i = 0; i < segs.Length; i++)
            {
                dir = dir.FindDirectory(segs[i]);
                if (dir == null)
                    break;
            }
            return dir;
        }

        public void UpdateFullName(string fullName)
        {
            FullName = fullName;
            foreach(var i in Files.Values)
            {
                i.FullName = FullName + i.Name;
            }

            foreach (var i in Folders.Values)
            {
                i.UpdateFullName(FullName + i.Name + "/");
            }
        }

        public void GetDirectorys(List<VrDirectory> dirs, string searchPattern = "*", SearchOption searchOp = SearchOption.TopDirectoryOnly)
        {
            dirs.AddRange(this.Folders.Values);

            if (searchOp == SearchOption.AllDirectories)
            {
                foreach (var i in Folders.Values)
                {
                    i.GetDirectorys(dirs);
                }
            }
        }
        public void GetFiles(List<VrFile> files, string searchPattern = "*", SearchOption searchOp = SearchOption.TopDirectoryOnly)
        {
            files.AddRange(this.Files.Values);

            if (searchOp == SearchOption.AllDirectories)
            {
                foreach (var i in Folders.Values)
                {
                    i.GetFiles(files);
                }
            }
        }
        public static VrDirectory LoadVrDirectory(string file, bool ignoreAssetsNode)
        {
            var xml = EngineNS.IO.XmlHolder.LoadXML(file);
            var node = xml.RootNode;
            if (ignoreAssetsNode)
            {
                node = node.FindNode("Folder");
                var attr = node.FindAttrib("Name");
                if(attr!=null)
                {
                    if (attr.Value != "Assets")
                        return null;
                }
            }
            VrDirectory result = new VrDirectory();
            LoadVrDirFromNode(result, node);
            return result;
        }
        private static void LoadVrDirFromNode(VrDirectory dir, EngineNS.IO.XmlNode node)
        {
            var fileNode = node.FindNode("Files");
            if(fileNode!=null)
            {
                var files = fileNode.GetNodes();
                foreach(var i in files)
                {
                    var attr = i.FindAttrib("Name");
                    if(attr!=null)
                    {
                        VrFile curFile = new VrFile();
                        curFile.Parent = dir;
                        curFile.Name = attr.Value;
                        dir.Files.Add(curFile.Name, curFile);
                    }
                }
            }

            var folders = node.GetNodes();
            foreach (var i in folders)
            {
                if (i.Name == "Folder")
                {
                    var attr = i.FindAttrib("Name");
                    if (attr != null)
                    {
                        VrDirectory subDir = new VrDirectory();
                        subDir.Parent = dir;
                        subDir.Name = attr.Value;
                        dir.Folders.Add(subDir.Name, subDir);
                        LoadVrDirFromNode(subDir, i);
                    }
                }
            }
        }
    }
}

