using EngineNS.Bricks.Animation.Curve;
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Bricks.Animation
{
    public enum AnimationElementType
    {
        Default,
        Bone,
        Skeleton,
    };
    public enum BindHierarchyObjectType
    {
        BHOT_Root,  //BHOTRoot
        BHOT_Child, //BHOTChild
        BHOT_Property,//BHOTProperty
        BHOT_Target,
    }
    public class BindHierarchyObject
    {
        public BindHierarchyObjectType BindHierarchyType;
        public string BindHierarchyName;
        public System.Reflection.PropertyInfo InstanceProperty;
    }

    [Rtti.MetaClass]
    public class CGfxAnimationElementDesc : AuxIOCoreObject<CGfxAnimationElementDesc.NativePointer>
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
        public CGfxAnimationElementDesc()
        {
            mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::GfxAnimationElementDesc");
        }
        public CGfxAnimationElementDesc(NativePointer nativePointer)
        {
            mCoreObject = nativePointer;
        }
        [Rtti.MetaData]
        public string Name
        {
            get
            {
                return SDK_GfxAnimationElementDesc_GetName(CoreObject);
            }
            set
            {
                SDK_GfxAnimationElementDesc_SetName(CoreObject, value);
            }
        }
        [Rtti.MetaData]
        public string Parent
        {
            get
            {
                return SDK_GfxAnimationElementDesc_GetParent(CoreObject);
            }
            set
            {
                SDK_GfxAnimationElementDesc_SetParent(CoreObject, value);
            }
        }
        [Rtti.MetaData]
        public string GrantParent
        {
            get
            {
                return SDK_GfxAnimationElementDesc_GetGrantParent(CoreObject);
            }
            set
            {
                SDK_GfxAnimationElementDesc_SetGrantParent(CoreObject, value);
            }
        }
        public uint NameHash
        {
            get { return SDK_GfxAnimationElementDesc_GetNameHash(CoreObject); }
        }
        [Rtti.MetaData]
        public string Path //GActor:actor1/bool:IsVisable
        {
            get
            {
                return SDK_GfxAnimationElementDesc_GetPath(CoreObject);
            }
            set
            {
                SDK_GfxAnimationElementDesc_SetPath(CoreObject, value);
            
            }
        }

        public uint ParentHash
        {
            get { return SDK_GfxAnimationElementDesc_GetParentHash(CoreObject); }
        }

        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
        public extern static string SDK_GfxAnimationElementDesc_GetName(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxAnimationElementDesc_SetName(NativePointer self, string name);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
        public extern static string SDK_GfxAnimationElementDesc_GetParent(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxAnimationElementDesc_SetParent(NativePointer self, string name);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
        public extern static string SDK_GfxAnimationElementDesc_GetGrantParent(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxAnimationElementDesc_SetGrantParent(NativePointer self, string name);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
        public extern static string SDK_GfxAnimationElementDesc_GetPath(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxAnimationElementDesc_SetPath(NativePointer self, string name);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_GfxAnimationElementDesc_GetNameHash(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_GfxAnimationElementDesc_GetParentHash(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_GfxAnimationElementDesc_GetGrantParentHash(NativePointer self);
        #endregion
    };
}
namespace EngineNS.Bricks.Animation.AnimElement
{
    [Rtti.MetaClass]
    public class CGfxAnimationElement : AuxIOCoreObject<CGfxAnimationElement.NativePointer>
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
        public CGfxAnimationElement()
        {
            mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::GfxAnimationElement");
        }
        public CGfxAnimationElement(NativePointer nativePointer)
        {
            mCoreObject = nativePointer;
            Core_AddRef();
        }
        protected CGfxAnimationElement(string deriveClass)
        {
            mCoreObject = NewNativeObjectByNativeName<NativePointer>(deriveClass);
        }
        [Rtti.MetaData]
        public AnimationElementType ElementType
        {
            get
            {
                return SDK_GfxAnimationElement_GetAnimationElementType(CoreObject);
            }
            set
            {
                SDK_GfxAnimationElement_SetAnimationElementType(CoreObject, value);
            }
        }
        CGfxAnimationElementDesc mDesc = null;
        [Rtti.MetaData]
        public CGfxAnimationElementDesc Desc
        {
            get
            {
                if (mDesc == null)
                {
                    mDesc = new CGfxAnimationElementDesc(SDK_GfxAnimationElement_GetAnimationElementDesc(CoreObject));
                }
                return mDesc;
            }
            set
            {
                mDesc = value;
                SDK_GfxAnimationElement_SetAnimationElementDesc(CoreObject, value.CoreObject);
            }
        }
        CurveType mCurveType = CurveType.CT_Invalid;
        [Rtti.MetaData]
        public CurveType CurveType
        { get => mCurveType; set => mCurveType = value; }
        protected CGfxICurve mCurve = null;
        public virtual CGfxICurve Curve
        {
            get
            {
                if (mCurve == null)
                {
                    var nativeCurve = SDK_GfxAnimationElement_GetCurve(CoreObject);
                    //var type = CGfxICurve.SDK_GfxICurve_GetCurveType(nativeCurve);
                    //创建交给派生类
                    //switch()
                }
                return mCurve;
            }
            set
            {
                mCurve = value;
                SDK_GfxAnimationElement_SetCurve(CoreObject, value.CoreObject);
                mCurveType = value.CurveType;
            }
        }
        public virtual uint GetKeyCount()
        {
            return Curve.GetKeyCount();
        }
        public virtual void Evaluate(float curveT, Binding.AnimationElementBinding bindingElement)
        {
            CurveResult curveResult = new CurveResult();
            curveResult = Curve.Evaluate(curveT, ref curveResult);
            bindingElement.Value = curveResult;
        }
        public virtual void SyncNative()
        {
            var nativeCurve = SDK_GfxAnimationElement_GetCurve(CoreObject);
            var type = CGfxICurve.SDK_GfxICurve_GetCurveType(nativeCurve);
            CreateCurve(type, nativeCurve);
            mCurveType = type;
        }
        public virtual async System.Threading.Tasks.Task<bool> Load(CRenderContext rc, IO.XndNode node)
        {
            await CEngine.Instance.EventPoster.Post(() =>
            {
                SyncLoad(rc, node);
                return true;
            }, Thread.Async.EAsyncTarget.AsyncIO);
            return true;
        }
        public virtual bool SyncLoad(CRenderContext rc, IO.XndNode node)
        {
            var att = node.FindAttrib("AnimationElement");
            if (att != null)
            {
                att.BeginRead();
                att.ReadMetaObject(this);
                att.EndRead();
            }

            CreateCurve(CurveType);
            Curve.SyncLoad(rc, node);
            return true;
        }
        public virtual void Save(IO.XndNode node)
        {
            var att = node.AddAttrib("AnimationElement");
            att.BeginWrite();
            att.WriteMetaObject(this);
            att.EndWrite();
            if (Curve != null)
                Curve.Save(node);
        }
        void CreateCurve(CurveType curveType)
        {
            switch (curveType)
            {
                case CurveType.CT_Bool:
                    {

                    }
                    break;
                case CurveType.CT_Int:
                    {

                    }
                    break;
                case CurveType.CT_Float:
                    {
                        Curve = new CGfxFloatCurve();
                    }
                    break;
                case CurveType.CT_Vector2:
                    {

                    }
                    break;
                case CurveType.CT_Vector3:
                    {
                        Curve = new CGfxVector3Curve();
                    }
                    break;
                case CurveType.CT_Vector4:
                    {

                    }
                    break;
                case CurveType.CT_Quaternion:
                    {
                        Curve = new CGfxQuaternionCurve();
                    }
                    break;
                case CurveType.CT_Bone:
                    {
                        Curve = new CGfxBoneCurve();
                    }
                    break;
                case CurveType.CT_Skeleton:
                    {

                    }
                    break;
                default:
                    break;
            }


        }
        void CreateCurve(CurveType curveType, CGfxICurve.NativePointer nativePointer)
        {
            switch (curveType)
            {
                case CurveType.CT_Bool:
                    {

                    }
                    break;
                case CurveType.CT_Int:
                    {

                    }
                    break;
                case CurveType.CT_Float:
                    {
                        Curve = new CGfxFloatCurve(nativePointer);
                    }
                    break;
                case CurveType.CT_Vector2:
                    {

                    }
                    break;
                case CurveType.CT_Vector3:
                    {
                        Curve = new CGfxVector3Curve(nativePointer);
                    }
                    break;
                case CurveType.CT_Vector4:
                    {

                    }
                    break;
                case CurveType.CT_Quaternion:
                    {
                        Curve = new CGfxQuaternionCurve(nativePointer);
                    }
                    break;
                case CurveType.CT_Bone:
                    {
                        Curve = new CGfxBoneCurve(nativePointer);
                    }
                    break;
                case CurveType.CT_Skeleton:
                    {

                    }
                    break;
                default:
                    break;
            }


        }

        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern unsafe static void SDK_GfxAnimationElement_SetCurve(NativePointer self, CGfxICurve.NativePointer val);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern unsafe static void SDK_GfxAnimationElement_SetAnimationElementType(NativePointer self, AnimationElementType val);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern unsafe static void SDK_GfxAnimationElement_SetAnimationElementDesc(NativePointer self, CGfxAnimationElementDesc.NativePointer val);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern unsafe static CGfxICurve.NativePointer SDK_GfxAnimationElement_GetCurve(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern unsafe static AnimationElementType SDK_GfxAnimationElement_GetAnimationElementType(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern unsafe static CGfxAnimationElementDesc.NativePointer SDK_GfxAnimationElement_GetAnimationElementDesc(NativePointer self);
        #endregion
    }
}
