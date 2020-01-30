using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Bricks.Animation.Skeleton
{
    public enum BoneType
    {
        Bone,
        Socket,
    };
    public enum ConstraintType
    {
        Translation = 1,
        Rotation = 2,
        Scale = 4,
    };

    public struct MotionConstraint
    {
        public ConstraintType ConstraintType;
        public Vector3 MaxRotation;
        public Vector3 MinRotation;
    };
    [Rtti.MetaClass]
    public class CGfxBoneDesc : AuxIOCoreObject<CGfxBoneDesc.NativePointer>
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

        [Rtti.MetaData]
        public string Name
        {
            get
            {
                return SDK_GfxBoneDesc_GetName(CoreObject);
            }
            set
            {
                SDK_GfxBoneDesc_SetName(CoreObject, value);
            }
        }
        [Rtti.MetaData]
        public string Parent
        {
            get
            {
                return SDK_GfxBoneDesc_GetParent(CoreObject);
            }
            set
            {
                SDK_GfxBoneDesc_SetParent(CoreObject, value);
            }
        }
        [Rtti.MetaData]
        public MotionConstraint Constraint
        {
            get;
            set;
        }
        public UInt32 NameHash
        {
            get
            {
                return SDK_GfxBoneDesc_GetNameHash(CoreObject);
            }
        }
        public UInt32 ParentHash
        {
            get
            {
                return SDK_GfxBoneDesc_GetParentHash(CoreObject);
            }
        }
        [Rtti.MetaData]
        public string GrantParent
        {
            get
            {
                return SDK_GfxBoneDesc_GetGrantParent(CoreObject);
            }
            set
            {
                SDK_GfxBoneDesc_SetGrantParent(CoreObject, value);
            }
        }
        //public UInt32 GrantParentHash
        //{
        //    get
        //    {
        //        return SDK_GfxBoneDesc_GetGrantParentHash(CoreObject);
        //    }
        //}
        [Rtti.MetaData]
        public float GrantWeight
        {
            get
            {
                return SDK_GfxBoneDesc_GetGrantWeight(CoreObject);
            }
            set
            {
                SDK_GfxBoneDesc_SetGrantWeight(CoreObject, value);
            }
        }
        [Rtti.MetaData]
        public Matrix BindMatrix
        {
            get
            {
                unsafe
                {
                    Matrix mat = new Matrix();
                    SDK_GfxBoneDesc_GetBindMatrix(CoreObject, &mat);
                    return mat;
                }
            }
            set
            {
                unsafe
                {
                    SDK_GfxBoneDesc_SetBindMatrix(CoreObject, &value);
                }
            }
        }
        CGfxBoneTransform mBindTransform = CGfxBoneTransform.Zero;
        public CGfxBoneTransform BindTransform
        {
            get
            {
                if (CGfxBoneTransform.IsZero(mBindTransform))
                    BindMatrix.Decompose(out mBindTransform.Scale, out mBindTransform.Rotation, out mBindTransform.Position);
                return mBindTransform;
            }
        }
        [Rtti.MetaData]
        public Matrix InvBindMatrix
        {
            get
            {
                unsafe
                {
                    Matrix mat = new Matrix();
                    SDK_GfxBoneDesc_GetBindInvInitMatrix(CoreObject, &mat);
                    return mat;
                }
            }
            set
            {
                unsafe
                {
                    SDK_GfxBoneDesc_SetBindInvInitMatrix(CoreObject, &value);
                    Vector3 invPos = Vector3.Zero;
                    Vector3 invScale = Vector3.UnitXYZ;
                    Quaternion invQuat = Quaternion.Identity;
                    value.Decompose(out invScale, out invQuat, out invPos);
                    InvPos = invPos;
                    InvScale = invScale;
                    InvQuat = invQuat;
                }
            }
        }
        public Vector3 InvPos
        {
            get
            {
                unsafe
                {
                    Vector3 invPos = Vector3.Zero;
                    SDK_GfxBoneDesc_GetInvPos(CoreObject, &invPos);
                    return invPos;
                }
            }
            set
            {
                unsafe
                {
                    SDK_GfxBoneDesc_SetInvPos(CoreObject, &value);
                }
            }
        }
        public Vector3 InvScale
        {
            get
            {
                unsafe
                {
                    Vector3 invScale = Vector3.UnitXYZ;
                    SDK_GfxBoneDesc_GetInvScale(CoreObject, &invScale);
                    return invScale;
                }
            }
            set
            {
                unsafe
                {
                    SDK_GfxBoneDesc_SetInvScale(CoreObject, &value);
                }
            }
        }
        public Quaternion InvQuat
        {
            get
            {
                unsafe
                {
                    Quaternion invQuat = Quaternion.Identity;
                    SDK_GfxBoneDesc_GetInvQuat(CoreObject, &invQuat);
                    return invQuat;
                }
            }
            set
            {
                unsafe
                {
                    SDK_GfxBoneDesc_SetInvQuat(CoreObject, &value);
                }
            }
        }
        [Rtti.MetaData]
        public BoneType Type
        {
            get
            {
                return SDK_GfxBoneDesc_GetBoneType(CoreObject);
            }
            set
            {
                SDK_GfxBoneDesc_SetBoneType(CoreObject, value);
            }
        }

        public CGfxBoneDesc()
        {
            mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::GfxBoneDesc");
        }
        public CGfxBoneDesc(NativePointer self)
        {
            mCoreObject = self;
            Core_AddRef();
        }
        public CGfxBoneDesc Clone()
        {
            CGfxBoneDesc clone = new CGfxBoneDesc();
            clone.Name = Name;
            clone.Parent = Parent;
            clone.GrantParent = GrantParent;
            clone.GrantWeight = GrantWeight;
            clone.BindMatrix = BindMatrix;
            clone.InvBindMatrix = InvBindMatrix;

            return clone;
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
        public extern static string SDK_GfxBoneDesc_GetName(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxBoneDesc_SetName(NativePointer self, string name);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
        public extern static string SDK_GfxBoneDesc_GetParent(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxBoneDesc_SetParent(NativePointer self, string name);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
        public extern static string SDK_GfxBoneDesc_GetGrantParent(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxBoneDesc_SetGrantParent(NativePointer self, string name);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_GfxBoneDesc_GetNameHash(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_GfxBoneDesc_GetParentHash(NativePointer self);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static UInt32 SDK_GfxBoneDesc_GetGrantParentHash(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static float SDK_GfxBoneDesc_GetGrantWeight(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxBoneDesc_SetGrantWeight(NativePointer self, float weight);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static BoneType SDK_GfxBoneDesc_GetBoneType(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxBoneDesc_SetBoneType(NativePointer self, BoneType type);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public unsafe extern static void SDK_GfxBoneDesc_GetBindMatrix(NativePointer self, Matrix* mat);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public unsafe extern static void SDK_GfxBoneDesc_SetBindMatrix(NativePointer self, Matrix* mat);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public unsafe extern static void SDK_GfxBoneDesc_GetBindInvInitMatrix(NativePointer self, Matrix* mat);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public unsafe extern static void SDK_GfxBoneDesc_SetBindInvInitMatrix(NativePointer self, Matrix* mat);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public unsafe extern static void SDK_GfxBoneDesc_GetInvPos(NativePointer self, Vector3* value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public unsafe extern static void SDK_GfxBoneDesc_SetInvPos(NativePointer self, Vector3* value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public unsafe extern static void SDK_GfxBoneDesc_GetInvScale(NativePointer self, Vector3* value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public unsafe extern static void SDK_GfxBoneDesc_SetInvScale(NativePointer self, Vector3* value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public unsafe extern static void SDK_GfxBoneDesc_GetInvQuat(NativePointer self, Quaternion* value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public unsafe extern static void SDK_GfxBoneDesc_SetInvQuat(NativePointer self, Quaternion* value);

        #endregion
    }
}
