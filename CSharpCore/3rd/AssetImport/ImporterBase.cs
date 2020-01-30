using EngineNS;
using EngineNS.Bricks.Animation.AnimNode;
using EngineNS.Bricks.Animation.Skeleton;
using EngineNS.Graphics.Mesh;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace AssetImport
{
    public enum AsseetImportType
    {
        AIT_Import = 0,
        AIT_Warning,
    }
    public delegate void ResourceImportHandle(object sender, AsseetImportType type, string resourceName);

    public class ImporterBase : AuxCoreObject<ImporterBase.NativePointer>
    {
        public delegate void ImportingHandle(IntPtr self, int type, string info);
        public event ResourceImportHandle OnResourceImport;

        public ImportingHandle mImportingHandle = _OnNativeImporting;

        protected string mSkeletonAsset = "";
        public string SkeletonAsset
        {
            get => mSkeletonAsset;
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
        protected bool Async = true;
        IntPtr handle;
        public ImporterBase()
        {
            mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::AssetImporter");
        }
        ~ImporterBase()
        {
            if (handle != IntPtr.Zero)
            {
                System.Runtime.InteropServices.GCHandle.FromIntPtr(handle).Free();
                handle = IntPtr.Zero;
            }
        }
        public void SetNativeCallback()
        {
            handle = System.Runtime.InteropServices.GCHandle.ToIntPtr(System.Runtime.InteropServices.GCHandle.Alloc(this, System.Runtime.InteropServices.GCHandleType.WeakTrackResurrection));
            SDK_AssetImporter_SetCShaprHandle(CoreObject, handle);
            SDK_AssetImporter_SetFOnImportHandle(CoreObject, mImportingHandle);
        }
        protected string mAbsFileName;
        public string AbsFileName
        {
            get => mAbsFileName;
        }
        protected string mAbsSavePath;
        public string AbsSavePath
        {
            get => mAbsSavePath;
        }
        protected ImportOption mImportOption;
        protected uint mImportFlag;
        public static string Ext = "";
        protected Dictionary<string, EngineNS.Graphics.Mesh.CGfxMeshPrimitives> mMeshPrimitives = new Dictionary<string, EngineNS.Graphics.Mesh.CGfxMeshPrimitives>();
        protected Dictionary<string, RName> mSkeletonNeedInfo = new Dictionary<string, RName>();
        protected Dictionary<string, EngineNS.Graphics.Mesh.CGfxMesh> mMeshes = new Dictionary<string, EngineNS.Graphics.Mesh.CGfxMesh>();
        protected Dictionary<string, CGfxAnimationSequence> mActions = new Dictionary<string, CGfxAnimationSequence>();
        protected Dictionary<string, EngineNS.Graphics.CGfxMaterial> mMaterials = new Dictionary<string, EngineNS.Graphics.CGfxMaterial>();
        protected uint mMeshNum = 0;
        protected uint mActionsNum = 0;
        protected uint mMaterialsNum = 0;
        protected bool mHaveSkeleton = false;
        public Dictionary<string, RName> SkeletonNeedInfo { get => mSkeletonNeedInfo; }
        public Dictionary<string, CGfxMeshPrimitives> MeshPrimitives { get => mMeshPrimitives; }
        public Dictionary<string, CGfxMesh> Meshes { get => mMeshes; }
        public Dictionary<string, CGfxAnimationSequence> Actions { get => mActions; }
        public ImportOption ImportOption { get => mImportOption; }
        public virtual async System.Threading.Tasks.Task<bool> PreImport(string absFileName, string absSavePath, ImportOption option, uint flags)
        {
            OnResourceImportCheck(this, AsseetImportType.AIT_Import, "Preprocessing");
            mAbsFileName = absFileName;
            mAbsSavePath = absSavePath;
            mImportOption = option;
            mImportFlag = flags;
            if (Async)
            {
                await CEngine.Instance.EventPoster.Post(() =>
                {
                    //core readfile
                    if (!SDK_AssetImporter_ReadFile(mCoreObject, mAbsFileName, mImportOption.GetNativeImportOption(), flags))
                        return false;
                    //then we should know meshNum,actionNum,MaterialNum,
                    //and have skeleton or not
                    mMeshNum = SDK_AssetImporter_GetMeshesNum(mCoreObject);
                    mActionsNum = SDK_AssetImporter_GetActionsNum(mCoreObject);
                    mMaterialsNum = SDK_AssetImporter_GetMaterialsNum(mCoreObject);
                    mHaveSkeleton = SDK_AssetImporter_GetAnyHasSkeleton(mCoreObject);
                    return true;
                }, EngineNS.Thread.Async.EAsyncTarget.AsyncEditor);
            }
            else
            {
                if (!SDK_AssetImporter_ReadFile(mCoreObject, mAbsFileName, mImportOption.GetNativeImportOption(), flags))
                    return false;
                //then we should know meshNum,actionNum,MaterialNum,
                //and have skeleton or not
                mMeshNum = SDK_AssetImporter_GetMeshesNum(mCoreObject);
                mActionsNum = SDK_AssetImporter_GetActionsNum(mCoreObject);
                mMaterialsNum = SDK_AssetImporter_GetMaterialsNum(mCoreObject);
                mHaveSkeleton = SDK_AssetImporter_GetAnyHasSkeleton(mCoreObject);
            }
            return true;
        }


        public virtual async System.Threading.Tasks.Task<bool> Import(/*RName fileName, RName savePath, ImportOption option, uint flagsh*/)
        {
            var rc = EngineNS.CEngine.Instance.RenderContext;
            for (uint i = 0; i < mMeshNum; i++)
            {
                await CreateVmsAndGms(rc, i);
            }
            for (uint i = 0; i < mActionsNum; i++)
            {
                await CreateAnimationSequence(rc, i);

            }
            for (uint i = 0; i < mMaterialsNum; i++)
            {

            }
            return true;
        }

        protected virtual async System.Threading.Tasks.Task CreateVmsAndGms(CRenderContext rc, uint meshIndex)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            return;
        }
        protected virtual async System.Threading.Tasks.Task CreateAnimationSequence(CRenderContext rc, uint actionIndex)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            return;
        }
        public static unsafe void _OnNativeImporting(IntPtr self, int type, string info)
        {
            var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(self);
            var cb = handle.Target as ImporterBase;
            if (cb == null)
            {
                return;
            }
            cb.OnResourceImportCheck(cb, (AsseetImportType)type, info);
        }
        protected void OnResourceImportCheck(object sender, AsseetImportType type, string info)
        {
            OnResourceImport?.Invoke(sender, type, info);
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static EngineNS.vBOOL SDK_AssetImporter_ReadFile(NativePointer self, string fileName, NativeImportOption importOption, uint flags);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static EngineNS.vBOOL SDK_AssetImporter_GetAnyHasSkeleton(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static uint SDK_AssetImporter_GetMeshesNum(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static uint SDK_AssetImporter_GetActionsNum(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static uint SDK_AssetImporter_GetMaterialsNum(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static IntPtr SDK_AssetImporter_GetMeshNameByIndex(NativePointer self, uint index);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static IntPtr SDK_AssetImporter_SetFOnImportHandle(NativePointer self, ImportingHandle handle);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static IntPtr SDK_AssetImporter_SetCShaprHandle(NativePointer self, IntPtr handle);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
        public extern static IntPtr SDK_AssetImporter_GetActionNameByIndex(NativePointer self, uint index);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static uint SDK_AssetImporter_GetMesAtomByIndex(NativePointer self, uint index);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
        public extern static IntPtr SDK_AssetImporter_GetMaterialNameByIndex(NativePointer self, uint index);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static uint SDK_AssetImporter_GetMeshPrimitiveByIndex(NativePointer self, CRenderContext.NativePointer rc, uint index, EngineNS.Graphics.Mesh.CGfxMeshPrimitives.NativePointer primitiveMesh);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CGfxSkinModifier.NativePointer SDK_AssetImporter_BuildMeshPrimitiveSkinModifierByIndex(NativePointer self, CRenderContext.NativePointer rc, uint index, EngineNS.Graphics.Mesh.CGfxMeshPrimitives.NativePointer primitiveMesh);

        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static uint SDK_AssetImporter_GetSkeletonActionByIndex(NativePointer self, CRenderContext.NativePointer rc, uint index, CGfxSkeletonAction.NativePointer skeletonAction);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static uint SDK_AssetImporter_CaculateSkeletonActionMotionData(NativePointer self, CRenderContext.NativePointer rc, CGfxSkeletonAction.NativePointer skeletonAction, EngineNS.Bricks.Animation.Pose.CGfxSkeletonPose.NativePointer pose);
        #endregion
    }
}
