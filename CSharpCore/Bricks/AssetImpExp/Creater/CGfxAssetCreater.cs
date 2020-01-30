using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Bricks.AssetImpExp.Creater
{
    public enum AssetImportMessageType
    {
        AMT_Import = 0,
        AMT_Warning,
        AMT_Error,
        AMT_Save,
        //file conflict
        AMT_FileExist,
        //as return value/////
        AMT_DeleteOriginFile,
        AMT_IgnoreFile,
        AMT_RenameFile,
        //////////////////
        AMT_UnKnown,
    };
    public delegate AssetImportMessageType CreaterAssetImportMessageHandle(object sender, AssetImportMessageType type, int level, string info, float percent);
    public class CGfxAssetCreater : AuxCoreObject<CGfxAssetCreater.NativePointer>
    {
        public delegate void ImportMessageHandle(IntPtr self, int type, int level, string info, float percent);
        public event CreaterAssetImportMessageHandle OnCreaterAssetImportMessageDumping;
        public ImportMessageHandle mImportingHandle = _OnNativeImportMessageDumping;
        
        public float ImportPercent { get; set; } = 1.0f;
        public static CGfxAssetCreater CreateAssetCreater(CGfxAssetImportOption option)
        {
            CGfxAssetCreater assetCreater = null;
            switch (option.AssetType)
            {
                case ImportAssetType.IAT_Mesh:
                    {
                        assetCreater = new CGfxAsset_MeshCreater();
                        assetCreater.AssetImportOption = option;
                    }
                    break;
                case ImportAssetType.IAT_Animation:
                    {
                        assetCreater = new CGfxAsset_AnimationCreater();
                        assetCreater.AssetImportOption = option;
                    }
                    break;
                default:
                    break;

            }
            assetCreater.AssetType = option.AssetType;
            return assetCreater;
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
        IntPtr handle;
        public CGfxAssetCreater()
        {
            mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::GfxAssetCreater");
            SetNativeCallback();
        }
        protected CGfxAssetCreater(string deriveClass)
        {
            mCoreObject = NewNativeObjectByNativeName<NativePointer>(deriveClass);
            SetNativeCallback();
        }
        ~CGfxAssetCreater()
        {
            if (handle != IntPtr.Zero)
            {
                System.Runtime.InteropServices.GCHandle.FromIntPtr(handle).Free();
                handle = IntPtr.Zero;
            }
        }
        public virtual bool CheckIfNeedImport()
        {
            return false;
        }
        public void SetNativeCallback()
        {
            handle = System.Runtime.InteropServices.GCHandle.ToIntPtr(System.Runtime.InteropServices.GCHandle.Alloc(this, System.Runtime.InteropServices.GCHandleType.WeakTrackResurrection));
            SDK_GfxAssetCreater_SetCShaprHandle(CoreObject, handle);
            SDK_GfxAssetCreater_SetFOnImportMessageHandle(CoreObject, mImportingHandle);
        }
        ImportAssetType mAssetType = ImportAssetType.IAT_Unknown;
        public ImportAssetType AssetType
        {
            get
            {
                return mAssetType;
            }
            set
            {
                mAssetType = value;
                //native set
                SDK_GfxAssetCreater_SetAssetType(CoreObject, value);

            }
        }
        CGfxAssetImportOption mAssetImportOption = null;
        public CGfxAssetImportOption AssetImportOption
        {
            get
            {
                return mAssetImportOption;
            }
            set
            {
                mAssetImportOption = value;
                //native set
                SDK_GfxAssetCreater_SetImportOption(CoreObject, value.CoreObject);
                Init();
            }
        }
        public virtual void Init()
        {

        }
        public AssetImportMessageType FileOperation(AssetImportMessageType messageType,RName file)
        {
            var operationResult = AssetImportMessageType.AMT_UnKnown;
            switch (messageType)
            {
                case AssetImportMessageType.AMT_DeleteOriginFile:
                    {

                        EngineNS.CEngine.Instance.FileManager.DeleteFile(file.Address);
                        EngineNS.CEngine.Instance.FileManager.DeleteFile(file.Address + EditorCommon.Program.ResourceInfoExt);
                        operationResult = AssetImportMessageType.AMT_DeleteOriginFile;
                    }
                    break;
                case AssetImportMessageType.AMT_IgnoreFile:
                    {
                        operationResult = AssetImportMessageType.AMT_IgnoreFile;
                    }
                    break;
                case AssetImportMessageType.AMT_RenameFile:
                    {
                        operationResult = AssetImportMessageType.AMT_RenameFile;
                    }
                    break;
                default:
                    operationResult = AssetImportMessageType.AMT_UnKnown;
                    break;
            }
            return operationResult;
        }
        public int GetValidFileNameSuffix(string absSavePath,string filePureName,string extension)
        {
            int i = 1;
            while(EngineNS.CEngine.Instance.FileManager.FileExists(absSavePath + "/" + filePureName+ i.ToString() + extension))
            {
                ++i;
            }
            return i;
        }
        public string GetValidFileName(string absSavePath, string filePureName, string extension)
        {
            int i = 1;
            while (EngineNS.CEngine.Instance.FileManager.FileExists(absSavePath + "/" + filePureName + i.ToString() + extension))
            {
                ++i;
            }
            return absSavePath + "/" + filePureName + i.ToString() + extension;
        }
        public virtual async System.Threading.Tasks.Task SaveAsset()
        {
            await Thread.AsyncDummyClass.DummyFunc();
        }
        public static unsafe void _OnNativeImportMessageDumping(IntPtr self, int type, int level, string info, float percent)
        {
            var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(self);
            var cb = handle.Target as CGfxAssetCreater;
            if (cb == null)
            {
                return;
            }
            cb._NativeImportMessageDumping(cb, (AssetImportMessageType)type,level, info, percent);
        }
        protected AssetImportMessageType _OnCreaterAssetImportMessageDumping(object sender, AssetImportMessageType type, int level, string info, float percent)
        {
          return (AssetImportMessageType)OnCreaterAssetImportMessageDumping?.Invoke(sender, type, level, AssetImportOption.Name + ":"+ info, ImportPercent);
        }
        protected void _NativeImportMessageDumping(object sender, AssetImportMessageType type, int level, string info, float percent)
        {
            ImportPercent = percent * 0.5f;
            _OnCreaterAssetImportMessageDumping(sender, type, level, info, ImportPercent);
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxAssetCreater_SetAssetType(NativePointer self, ImportAssetType type);

        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxAssetCreater_SetImportOption(NativePointer self, CGfxAssetImportOption.NativePointer importOption);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static IntPtr SDK_GfxAssetCreater_SetFOnImportMessageHandle(NativePointer self, ImportMessageHandle handle);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static IntPtr SDK_GfxAssetCreater_SetCShaprHandle(NativePointer self, IntPtr handle);
        #endregion
    }
}
