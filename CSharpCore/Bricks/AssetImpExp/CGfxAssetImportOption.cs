using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace EngineNS.Bricks.AssetImpExp
{
    public enum ImportAssetType
    {
        IAT_Unknown,
        IAT_Null,
        IAT_Marker,
        IAT_Skeleton,
        IAT_Mesh,
        IAT_Nurbs,
        IAT_Patch,
        IAT_Camera,
        IAT_CameraStereo,
        IAT_CameraSwitcher,
        IAT_Light,
        IAT_OpticalReference,
        IAT_OpticalMarker,
        IAT_NurbsCurve,
        IAT_TrimNurbsSurface,
        IAT_Boundary,
        IAT_NurbsSurface,
        IAT_Shape,
        IAT_LODGroup,
        IAT_SubDiv,
        IAT_CachedEffect,
        IAT_Line,
        IAT_Animation,
        IAT_Default,
    };
    [EngineNS.Editor.Editor_DisplayNameInEnumerable("Asset")]
    public class CGfxAssetImportOption : AuxCoreObject<CGfxAssetImportOption.NativePointer>,INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion
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
        public CGfxAssetImportOption()
        {
            mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::GfxAssetImportOption");
        }
        public CGfxAssetImportOption(NativePointer native)
        {
            mCoreObject = native;
        }
        protected CGfxAssetImportOption(string deriveClass)
        {
            mCoreObject = NewNativeObjectByNativeName<NativePointer>(deriveClass);
        }
        [Category("Option"), DisplayName("名称"),Editor.Editor_PropertyGridSortIndex(1)]
        public virtual string Name
        {
            get { return SDK_GfxAssetImportOption_GetName(mCoreObject); }
            set { SDK_GfxAssetImportOption_SetName(mCoreObject, value); OnPropertyChanged("Name"); }
        }
        [Browsable(false)]
        public uint Hash
        {
            get { return SDK_GfxAssetImportOption_GetHash(mCoreObject); }
            set { SDK_GfxAssetImportOption_SetHash(mCoreObject, value); }
        }
        [Category("Option"), DisplayName("导入")]
        public virtual bool IsImport
        {
            get { return SDK_GfxAssetImportOption_GetIsImport(mCoreObject); }
            set { SDK_GfxAssetImportOption_SetIsImport(mCoreObject, vBOOL.FromBoolean(value)); OnPropertyChanged("IsImport"); }
        }
        [Category("Option"), DisplayName("缩放")]
        public virtual float Scale
        {
            get { return SDK_GfxAssetImportOption_GetScale(mCoreObject); }
            set { SDK_GfxAssetImportOption_SetScale(mCoreObject, value); OnPropertyChanged("Scale"); }
        }
        [Browsable(false)]
        public ImportAssetType AssetType
        {
            get { return SDK_GfxAssetImportOption_GetAssetType(mCoreObject); }
            set { SDK_GfxAssetImportOption_SetAssetType(mCoreObject, value); }
        }
        [Category("Option"), DisplayName("保存路径"), Editor.Editor_PropertyGridSortIndex(2)]
        public virtual string AbsSavePath
        {
            get { return SDK_GfxAssetImportOption_GetAbsSavePath(mCoreObject); }
            set { SDK_GfxAssetImportOption_SetAbsSavePath(mCoreObject, value); }
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxAssetImportOption_SetName(NativePointer self, string name);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
        public extern static string SDK_GfxAssetImportOption_GetName(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxAssetImportOption_SetAbsSavePath(NativePointer self, string name);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
        public extern static string SDK_GfxAssetImportOption_GetAbsSavePath(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxAssetImportOption_SetHash(NativePointer self, uint hash);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static uint SDK_GfxAssetImportOption_GetHash(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxAssetImportOption_SetScale(NativePointer self, float scale);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static float SDK_GfxAssetImportOption_GetScale(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxAssetImportOption_SetIsImport(NativePointer self, vBOOL isImport);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxAssetImportOption_GetIsImport(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxAssetImportOption_SetAssetType(NativePointer self, ImportAssetType type);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static ImportAssetType SDK_GfxAssetImportOption_GetAssetType(NativePointer self);
        #endregion
    }
    [EngineNS.Editor.Editor_DisplayNameInEnumerable("Mesh")]
    public class CGfxMeshImportOption : CGfxAssetImportOption
    {
        public CGfxMeshImportOption() : base("GfxMeshImportOption ")
        {

        }
        public CGfxMeshImportOption(NativePointer nativePointer):base(nativePointer)
        {
        }
        
        public bool ReCalculateTangent
        {
            get { return SDK_GfxMeshImportOption_GetReCalculateTangent(mCoreObject); }
            set { SDK_GfxMeshImportOption_SetReCalculateTangent(mCoreObject, vBOOL.FromBoolean(value)); OnPropertyChanged("ReCalculateTangent"); }
        }
        [Browsable(false)]
        public bool AsCollision
        {
            get { return SDK_GfxMeshImportOption_GetAsCollision(mCoreObject); }
            set { SDK_GfxMeshImportOption_SetAsCollision(mCoreObject, vBOOL.FromBoolean(value)); OnPropertyChanged("AsCollision"); }
        }
        bool mAsPhyGemoConvex = false;
        [DisplayName("动态碰撞(Convex:凸体)")]
        public bool AsPhyGemoConvex
        {
            get => mAsPhyGemoConvex;
            set => mAsPhyGemoConvex = value;
        }
        bool mAsPhyGemoTri = false;
        [DisplayName("静态碰撞(Triangle:三角面)")]
        public bool AsPhyGemoTri
        {
            get => mAsPhyGemoTri;
            set => mAsPhyGemoTri = value;
        }
        bool mAsPhyGemoHeightField = false;
        [DisplayName("静态碰撞(HeightField:高度场)")]
        public bool AsPhyGemoHeightField
        {
            get => mAsPhyGemoHeightField;
            set => mAsPhyGemoHeightField = value;
        }
        //public bool AsLocalSpace
        //{
        //    get { return SDK_GfxMeshImportOption_GetAsLocalSpace(mCoreObject); }
        //    set { SDK_GfxMeshImportOption_SetAsLocalSpace(mCoreObject, vBOOL.FromBooleam(value)); }
        //}
        public bool TransformVertexToAbsolute
        {
            get { return SDK_GfxMeshImportOption_GetTransformVertexToAbsolute(mCoreObject); }
            set { SDK_GfxMeshImportOption_SetTransformVertexToAbsolute(mCoreObject, vBOOL.FromBoolean(value)); OnPropertyChanged("TransformVertexToAbsolute"); }
        }

        public bool HaveSkin
        {
            get { return SDK_GfxMeshImportOption_GetHaveSkin(mCoreObject); }
            //set { SDK_GfxMeshImportOption_SetHaveSkin(mCoreObject, vBOOL.FromBooleam(value)); OnPropertyChanged("HaveSkin"); }
        }
        public bool AsStaticMesh
        {
            get { return SDK_GfxMeshImportOption_GetAsStaticMesh(mCoreObject); }
            set { SDK_GfxMeshImportOption_SetAsStaticMesh(mCoreObject,vBOOL.FromBoolean(value)); OnPropertyChanged("AsStaticMesh"); }
        }
        [DisplayName("骨骼资产"), EngineNS.Editor.Editor_RNameType(EngineNS.Editor.Editor_RNameTypeAttribute.Skeleton)]
        public RName Skeleton { get; set; } = RName.EmptyName;
        public bool CreateGms { get; set; } = true;

        [Browsable(false)]
        public uint RenderAtom
        {
            get { return SDK_GfxMeshImportOption_GetRenderAtom(CoreObject); }
            set { SDK_GfxMeshImportOption_SetRenderAtom(CoreObject, value); }
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxMeshImportOption_SetReCalculateTangent(NativePointer self, vBOOL reCalculateTangent);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxMeshImportOption_GetReCalculateTangent(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxMeshImportOption_SetAsCollision(NativePointer self, vBOOL isImport);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxMeshImportOption_GetAsCollision(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxMeshImportOption_SetAsLocalSpace(NativePointer self, vBOOL asLocalSpace);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxMeshImportOption_GetAsLocalSpace(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxMeshImportOption_SetHaveSkin(NativePointer self, vBOOL haveSkin);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxMeshImportOption_GetHaveSkin(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxMeshImportOption_SetAsStaticMesh(NativePointer self, vBOOL asStaticMesh);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxMeshImportOption_GetAsStaticMesh(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxMeshImportOption_SetRenderAtom(NativePointer self, uint atom);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static uint SDK_GfxMeshImportOption_GetRenderAtom(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxMeshImportOption_SetTransformVertexToAbsolute(NativePointer self, vBOOL asLocalSpace);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxMeshImportOption_GetTransformVertexToAbsolute(NativePointer self);
        #endregion
    }
    [EngineNS.Editor.Editor_DisplayNameInEnumerable("Animation")]
    public class CGfxAnimationImportOption : CGfxAssetImportOption
    {
        [Browsable(false)]
        public ImportAssetType AnimationType
        {
            get { return SDK_GfxAnimationImportOption_GetAnimationType(mCoreObject); }
            set { SDK_GfxAnimationImportOption_SetAnimationType(mCoreObject, value); }
        }
        public float Duration
        {
            get { return SDK_GfxAnimationImportOption_GetDuration(mCoreObject); }
            set { SDK_GfxAnimationImportOption_SetDuration(mCoreObject, value); }
        }
        public float SampleRate
        {
            get { return SDK_GfxAnimationImportOption_GetSampleRate(mCoreObject); }
            set { SDK_GfxAnimationImportOption_SetSampleRate(mCoreObject, value); }
        }
        [DisplayName("骨骼资产"), EngineNS.Editor.Editor_RNameType(EngineNS.Editor.Editor_RNameTypeAttribute.Skeleton)]
        public RName Skeleton { get; set; } = RName.EmptyName;
        public CGfxAnimationImportOption() : base("GfxAnimationImportOption ")
        {


        }
        public CGfxAnimationImportOption(NativePointer nativePointer) :base(nativePointer)
        {
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxAnimationImportOption_SetAnimationType(NativePointer self, ImportAssetType type);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static ImportAssetType SDK_GfxAnimationImportOption_GetAnimationType(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxAnimationImportOption_SetDuration(NativePointer self, float duration);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static float SDK_GfxAnimationImportOption_GetDuration(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxAnimationImportOption_SetSampleRate(NativePointer self, float sampleRate);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static float SDK_GfxAnimationImportOption_GetSampleRate(NativePointer self);
        #endregion
    }
}
