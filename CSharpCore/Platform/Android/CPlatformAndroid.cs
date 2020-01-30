using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace EngineNS
{
    public partial class CIPlatform
    {
        Android.Content.Res.AssetManager AssetManager;
        public string ExternalFilesDir;
        public CIPlatform()
        {
            mPlatformType = EPlatformType.PLATFORM_DROID;
            mProfileType = Profiler.TimeScope.EProfileFlag.Android;
            mCoreObject = SDK_PlatformAndroid_GetInstance();
            Core_AddRef();
        }
        public void PrintCPUInfo()
        {
            //Android.OS.Build.CpuAbi
        }
        public void InitAndroid(Android.App.Activity context, Android.Content.Res.AssetManager assetMgr)
        {
            SDK_PlatformAndroid_InitAndroid(CoreObject, Android.Runtime.JNIEnv.Handle, assetMgr.Handle);
            SDK_PlatformAndroid_SetAssetsFileNameCB(mGetAssetsRelativeFileName);

            ExternalFilesDir = context.GetExternalFilesDir(null).AbsolutePath;//这个目录是app再sdcard下的沙箱目录，卸载后会被删除
            //保留AssetManager
            AssetManager = assetMgr;
        }
        public System.IntPtr WindowFromSurface(System.IntPtr surface)
        {
            return Android_ANWinFromSurface(Android.Runtime.JNIEnv.Handle, surface);
        }

        public bool IsKeyDown(Input.Device.Keyboard.Keys key)
        {
            return false;
        }

        partial void ReadFileFromAssets(string file, ref List<byte> result)
        {
            file = file.Replace(EngineNS.CEngine.Instance.FileManager.ProjectRoot, "").ToLower();
            Android.Runtime.InputStreamInvoker stream = (Android.Runtime.InputStreamInvoker)AssetManager.Open(file);
            
            Type type = stream.GetType();
            int len = stream.BaseInputStream.Available();
            if (stream != null || len > 0)
            {
                byte[] bytes = new byte[len];
                stream.Read(bytes, 0, len);
                stream.Close();

                result.AddRange(bytes);
            }
        }

        private delegate void FAndroid_GetAssetsRelativeFileName(IntPtr str, UInt32 bufferSize, IntPtr fullFileName);
        private static FAndroid_GetAssetsRelativeFileName mGetAssetsRelativeFileName = GetAssetsRelativeFileName;
        private static unsafe void GetAssetsRelativeFileName(IntPtr str, UInt32 bufferSize, IntPtr fullFileName)
        {
            unsafe
            {
                char* pAssets = (char*)str.ToPointer();
                var pFileName = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(fullFileName);

                string contentAsset = "";
                if (pFileName.StartsWith(CEngine.Instance.FileManager.ProjectContent))
                {
                    contentAsset = "content/" + pFileName.Substring(CEngine.Instance.FileManager.ProjectContent.Length);
                }
                else if (pFileName.StartsWith(CEngine.Instance.FileManager.EngineContent))
                {
                    contentAsset = "enginecontent/" + pFileName.Substring(CEngine.Instance.FileManager.EngineContent.Length);
                }
                else if (pFileName.StartsWith(CEngine.Instance.FileManager.EditorContent))
                {
                    contentAsset = "editorcontent/" + pFileName.Substring(CEngine.Instance.FileManager.EditorContent.Length);
                }
                else if (pFileName.StartsWith(CEngine.Instance.FileManager.DDCDirectory))
                {
                    contentAsset = "deriveddatacache/" + pFileName.Substring(CEngine.Instance.FileManager.DDCDirectory.Length);
                }

                var src = System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(contentAsset);
                CoreSDK.SDK_Memory_Copy(pAssets, src.ToPointer(), (UInt32)contentAsset.Length);
                System.Runtime.InteropServices.Marshal.FreeHGlobal(src);

                //var ssss = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(str);
            }
        }
        //System.Windows.Forms.Form form;
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static NativePointer SDK_PlatformAndroid_GetInstance();
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static System.IntPtr Android_ANWinFromSurface(System.IntPtr jniEnv, System.IntPtr surface);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_PlatformAndroid_InitAndroid(NativePointer self, IntPtr env, IntPtr assetMgr);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static unsafe void SDK_PlatformAndroid_SetAssetsFileNameCB(FAndroid_GetAssetsRelativeFileName fun);
        #endregion
    }
}

namespace EngineNS.IO
{
    public partial class FileManager
    {   
        partial void InitPaths()
        {
            mProjectRoot = CIPlatform.Instance.ExternalFilesDir + "/";
            mEngineRoot = mProjectRoot;

            mProjectContent = mProjectRoot + "content/";
            mEngineContent = mEngineRoot + "enginecontent/";
            mEditorContent = mEngineRoot + "editcontent/";
            mDDCDirectory = mProjectRoot + "deriveddatacache/";

            Profiler.Log.WriteLine(Profiler.ELogTag.Info, "IO", $"AssetsVrDirectory Root = {AssetsVrDirectory.Name}");
        }
    }
}

