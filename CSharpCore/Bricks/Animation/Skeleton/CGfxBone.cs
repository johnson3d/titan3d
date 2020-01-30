using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Bricks.Animation.Skeleton
{
    //public struct CGfxBoneSRT
    //{
    //    public Vector3 Position;
    //    public Vector3 Scale;
    //    public Quaternion Rotation;
    //    public static CGfxBoneSRT Identify
    //    {
    //        get
    //        {
    //            var temp = new CGfxBoneSRT();
    //            temp.Position = Vector3.Zero;
    //            temp.Scale = Vector3.UnitXYZ;
    //            temp.Rotation = Quaternion.Identity;
    //            return temp;
    //        }
    //    }
    //    static void Relative(ref CGfxBoneSRT result, CGfxBoneSRT child, CGfxBoneSRT parent)
    //    {
    //        result.Rotation = child.Rotation * parent.Rotation;
    //        result.Position = parent.Position + parent.Rotation * child.Position;
    //        //result.Scale = parent.Scale * cld.Scale;
    //    }
    //}
    public struct CGfxBoneTransform
    {
        public Vector3 Position;
        public Vector3 Scale;
        public Quaternion Rotation;
        public static CGfxBoneTransform Identify
        {
            get
            {
                var temp = new CGfxBoneTransform();
                temp.Position = Vector3.Zero;
                temp.Scale = Vector3.UnitXYZ;
                temp.Rotation = Quaternion.Identity;
                return temp;
            }
        }
        public static CGfxBoneTransform Zero
        {
            get
            {
                var temp = new CGfxBoneTransform();
                temp.Position = Vector3.Zero;
                temp.Scale = Vector3.Zero;
                temp.Rotation = new Quaternion(0, 0, 0, 0);
                return temp;
            }
        }
        public static CGfxBoneTransform Transform(CGfxBoneTransform cld, CGfxBoneTransform parent)
        {
            CGfxBoneTransform temp = CGfxBoneTransform.Identify;
            temp.Rotation = cld.Rotation * parent.Rotation;
            temp.Position = parent.Position + parent.Rotation * cld.Position;
            temp.Scale = Vector3.Modulate(parent.Scale, cld.Scale);
            return temp;
        }
        public static bool IsZero(CGfxBoneTransform transform)
        {
            if (transform.Position == Vector3.Zero && transform.Scale == Vector3.Zero &&
                                !transform.Rotation.IsValid)
                return true;
            return false;
        }

    }
    public struct CGfxMotionState
    {
        public Vector3 Position; //Character Space
        public Vector3 Velocity; //Character Space
                                 //public Quaternion Rotation;
    }
    //public class CGfxSkinBone : AuxCoreObject<CGfxSkinBone.NativePointer>
    //{
    //    public struct NativePointer : INativePointer
    //    {
    //        public IntPtr Pointer;
    //        public IntPtr GetPointer()
    //        {
    //            return Pointer;
    //        }
    //        public void SetPointer(IntPtr ptr)
    //        {
    //            Pointer = ptr;
    //        }
    //        public override string ToString()
    //        {
    //            return "0x" + Pointer.ToString("x");
    //        }
    //    }
    //    public CGfxSkinBone(NativePointer self)
    //    {
    //        mCoreObject = self;
    //        Core_AddRef();

    //        //mBoneDesc = new CGfxBoneDesc(SDK_GfxSkinBone_GetBoneDesc(CoreObject));
    //    }
    //    protected CGfxBoneDesc mBoneDesc;
    //    public CGfxBoneDesc BoneDesc
    //    {
    //        get { return mBoneDesc; }
    //    }
    //    #region SDK
    //    //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    //    //public extern static CGfxBoneDesc.NativePointer SDK_GfxSkinBone_GetBoneDesc(NativePointer self);
    //    #endregion
    //}
    [EngineNS.Rtti.MetaClass]
    [System.Diagnostics.DebuggerDisplay("{Name}")]
    public class CGfxBone : AuxIOCoreObject<CGfxBone.NativePointer>
    {
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
        public string Name
        {
            get => BoneDesc.Name;
        }
        protected CGfxBoneDesc mBoneDesc = null;
        [EngineNS.Rtti.MetaData]
        public CGfxBoneDesc BoneDesc
        {
            get
            {
                if (mBoneDesc == null)
                {
                    mBoneDesc = BoneDescNative;
                }
                return mBoneDesc;
            }
            set
            {
                mBoneDesc = value;
                SDK_GfxBone_SetBoneDesc(CoreObject, mBoneDesc.CoreObject);
            }
        }
        public CGfxBoneDesc BoneDescNative
        {
            get
            {
                return new CGfxBoneDesc(SDK_GfxBone_GetBoneDesc(CoreObject));
            }
        }
        public CGfxBone()
        {
            mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::GfxBone");
        }
        public CGfxBone(CGfxBoneDesc desc)
        {
            mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::GfxBone");
            BoneDesc = desc;
        }
        public CGfxBone(NativePointer self)
        {
            mCoreObject = self;
            Core_AddRef();
        }

        public UInt16 IndexInTable
        {
            get
            {
                return SDK_GfxBone_GetIndexInTable(CoreObject);
            }
            set
            {
                SDK_GfxBone_SetIndexInTable(CoreObject, value);
            }
        }
        public UInt32 ChildNumber
        {
            get
            {
                return SDK_GfxBone_GetChildNumber(CoreObject);
            }
        }
        //public CGfxTransform ParentSpaceTransform
        //{
        //    get
        //    {
        //        CGfxTransform bindTransform = CGfxTransform.Identify;
        //        BoneDesc.BindMatrix.Decompose(out bindTransform.Scale, out bindTransform.Rotation, out bindTransform.Position);
        //        return CGfxTransform.Transform(LocalSpaceTransform, bindTransform);
        //    }
        //}

        public UInt16 GetChild(UInt32 i)
        {
            return SDK_GfxBone_GetChild(CoreObject, i);
        }
        public void AddChild(UInt16 index)
        {
            SDK_GfxBone_AddChild(CoreObject, index);
        }
        public void ClearChildren()
        {
            SDK_GfxBone_ClearChildren(CoreObject);
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CGfxBoneDesc.NativePointer SDK_GfxBone_GetBoneDesc(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxBone_SetBoneDesc(NativePointer self, CGfxBoneDesc.NativePointer boneDesc);

        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt16 SDK_GfxBone_GetIndexInTable(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxBone_SetIndexInTable(NativePointer self, UInt16 index);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_GfxBone_GetChildNumber(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt16 SDK_GfxBone_GetChild(NativePointer self, UInt32 i);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxBone_AddChild(NativePointer self, UInt16 index);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxBone_ClearChildren(NativePointer self);
        #endregion
    }
}
