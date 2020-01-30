using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS
{
    public partial class CIPlatform
    {
        public CIPlatform()
        {
            
        }
        public void InitAppleOS()
        {
            mPlatformType = EPlatformType.PLATFORM_IOS;
            mProfileType = Profiler.TimeScope.EProfileFlag.IOS;
            SDK_PlatformIOS_SetAssetsFileNameCB(mGetAssetsRelativeFileName);
        }
        public bool IsKeyDown(Input.Device.Keyboard.Keys key)
        {
            return false;
        }
        private delegate void FIOS_GetAssetsFileName(IntPtr str, UInt32 bufferSize, IntPtr fullFileName, vBOOL fromDocument);
        private static FIOS_GetAssetsFileName mGetAssetsRelativeFileName = GetAssetsFileName;
#if PlatformIOS
        [ObjCRuntime.MonoPInvokeCallback(typeof(FIOS_GetAssetsFileName))]
#endif
        private static void GetAssetsFileName(IntPtr CppString, UInt32 bufferSize, IntPtr FullFileName, vBOOL LoadFromDocDir)
        {
            if(LoadFromDocDir)
            {
                unsafe
                {
                    char* refCppString = (char*)CppString.ToPointer();
                    var AnsiFullFileName = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(FullFileName);
                    var SrcCppString = System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(AnsiFullFileName);
                    CoreSDK.SDK_Memory_Copy(refCppString, SrcCppString.ToPointer(), (UInt32)AnsiFullFileName.Length);
                    System.Runtime.InteropServices.Marshal.FreeHGlobal(SrcCppString);

                    //var RelatedFileLocation = "Content/" + AnsiFullFileName.Substring(CEngine.Instance.FileManager.Content.Length);
                    //var SrcCppString = System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(RelatedFileLocation);
                    //CoreSDK.SDK_Memory_Copy(refCppString, SrcCppString.ToPointer(), (UInt32)RelatedFileLocation.Length);

                }
            }
            else
            {

            }
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static unsafe void SDK_PlatformIOS_SetAssetsFileNameCB(FIOS_GetAssetsFileName fun);
        #endregion
    }
}

namespace EngineNS.IO
{
    public partial class FileManager
    {
        partial void InitPaths()
        {
            mBin = AppDomain.CurrentDomain.BaseDirectory.Replace("\\", "/");
            mBin = mBin.TrimEnd('/');
            //Abs_Assets = mBin.Substring(0, mBin.LastIndexOf('/') + 1) + "assets/";
            // 不知道为什么这里app目录下的文件需要用相对路径读了
            //mAssets = "assets/";
            mBin += "/";
            
            mRoot = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/";
            mProjectRoot = mRoot;

            mProjectContent = mProjectRoot + "content/";
            mEngineContent = mProjectRoot + "enginecontent/";
            mEditorContent = mProjectRoot + "editcontent/";
            mDDCDirectory = mProjectRoot + "deriveddatacache/";
        }
        public string[] GetDirectories(string path, string searchPattern = "*", SearchOption searchOp = SearchOption.TopDirectoryOnly)
        {
            return System.IO.Directory.GetDirectories(path, searchPattern, searchOp);
        }
        public string[] GetFiles(string absDir, string fileKeyName, SearchOption searOp)
        {
            try
            {
                //List<string> result = new List<string>();
                //var iter = System.IO.Directory.EnumerateDirectories("./");
                //foreach(var i in iter)
                //{
                //    result.Add(i);
                //}
                //return result.ToArray();

                if (System.IO.Directory.Exists(absDir))
                {
                    var files = System.IO.Directory.GetFiles(absDir, fileKeyName, searOp);
                    return files;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
