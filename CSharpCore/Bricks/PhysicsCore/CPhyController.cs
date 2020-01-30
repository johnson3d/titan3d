using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using EngineNS.Bricks.PhysicsCore.CollisionComponent;

namespace EngineNS.Bricks.PhysicsCore
{
    [Flags]
    public enum PhyQueryFlag : UInt16
    {
        eSTATIC = (1 << 0), //!< Traverse static shapes

        eDYNAMIC = (1 << 1),    //!< Traverse dynamic shapes

        ePREFILTER = (1 << 2),  //!< Run the pre-intersection-test filter (see #PxQueryFilterCallback::preFilter())

        ePOSTFILTER = (1 << 3), //!< Run the post-intersection-test filter (see #PxQueryFilterCallback::postFilter())

        eANY_HIT = (1 << 4),    //!< Abort traversal as soon as any hit is found and return it via callback.block.
                                //!< Helps query performance. Both eTOUCH and eBLOCK hitTypes are considered hits with this flag.

        eNO_BLOCK = (1 << 5),   //!< All hits are reported as touching. Overrides eBLOCK returned from user filters with eTOUCH.
                                //!< This is also an optimization hint that may improve query performance.

        eRESERVED = (1 << 15)   //!< Reserved for internal use
    }
    [Flags]
    public enum PhyControllerCollisionFlag : byte
    {
        eCOLLISION_None = 0,        //!< Character has collision none.
        eCOLLISION_SIDES = (1 << 0),    //!< Character is colliding to the sides.
        eCOLLISION_UP = (1 << 1),   //!< Character has collision above.
        eCOLLISION_DOWN = (1 << 2)  //!< Character has collision below.
    }
    public class CPhyControllerDesc : AuxCoreObject<CPhyControllerDesc.NativePointer>
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
        public void SetMaterial(CPhyMaterial mtl)
        {
            SDK_PhyControllerDesc_SetMaterial(CoreObject, mtl.CoreObject);
        }
        PhyFilterData mFilterData;
        public void SetQueryFilterData(PhyFilterData data)
        {
            mFilterData = data;
            unsafe
            {
                fixed (PhyFilterData* pData = &mFilterData)
                {
                    SDK_PhyControllerDesc_SetQueryFilterData(CoreObject, pData);
                }
            }
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static void SDK_PhyControllerDesc_SetMaterial(NativePointer self, CPhyMaterial.NativePointer mtl);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static unsafe void SDK_PhyControllerDesc_SetQueryFilterData(NativePointer self, PhyFilterData* filterData);
        #endregion
    }
    public class CPhyBoxControllerDesc : CPhyControllerDesc
    {
        public CPhyBoxControllerDesc()
        {
            mCoreObject = NewNativeObjectByNativeName<NativePointer>("PhyBoxControllerDesc");
        }
        public Vector3 Extent
        {
            get
            {
                return SDK_PhyBoxControllerDesc_GetExtent(CoreObject);
            }
            set
            {
                unsafe
                {
                    SDK_PhyBoxControllerDesc_SetExtent(CoreObject, &value);
                }
            }
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static Vector3 SDK_PhyBoxControllerDesc_GetExtent(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern unsafe static void SDK_PhyBoxControllerDesc_SetExtent(NativePointer self, Vector3* v);
        #endregion
    }

    public enum PhyCapsuleClimbingMode
    {
        eEASY,          //!< Standard mode, let the capsule climb over surfaces according to impact normal
        eCONSTRAINED,   //!< Constrained mode, try to limit climbing according to the step offset

        eLAST
    }
    public class CPhyCapsuleControllerDesc : CPhyControllerDesc
    {
        public CPhyCapsuleControllerDesc()
        {
            mCoreObject = NewNativeObjectByNativeName<NativePointer>("PhyCapsuleControllerDesc");
        }
        public float CapsuleRadius
        {
            get
            {
                return SDK_PhyCapsuleControllerDesc_GetCapsuleRadius(CoreObject);
            }
            set
            {
                SDK_PhyCapsuleControllerDesc_SetCapsuleRadius(CoreObject, value);
            }
        }
        public float CapsuleHeight
        {
            get
            {
                return SDK_PhyCapsuleControllerDesc_GetCapsuleHeight(CoreObject);
            }
            set
            {
                SDK_PhyCapsuleControllerDesc_SetCapsuleHeight(CoreObject, value);
            }
        }
        //public PhyCapsuleClimbingMode ClimbingModet
        //{
        //    get
        //    {
        //        return SDK_PhyCapsuleControllerDesc_GetClimbingMode(CoreObject);
        //    }
        //    set
        //    {
        //        SDK_PhyCapsuleControllerDesc_SetClimbingModet(CoreObject, value);
        //    }
        //}
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static float SDK_PhyCapsuleControllerDesc_GetCapsuleRadius(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static void SDK_PhyCapsuleControllerDesc_SetCapsuleRadius(NativePointer self, float v);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static float SDK_PhyCapsuleControllerDesc_GetCapsuleHeight(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static void SDK_PhyCapsuleControllerDesc_SetCapsuleHeight(NativePointer self, float v);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //private extern static PhyCapsuleClimbingMode SDK_PhyCapsuleControllerDesc_GetClimbingMode(NativePointer self);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //private extern static void SDK_PhyCapsuleControllerDesc_SetClimbingModet(NativePointer self, PhyCapsuleClimbingMode v);
        #endregion
    }
    public class CPhyController : CPhyEntity
    {
        public GPhyControllerComponent HostComponent { get; set; }
        public CPhyController(NativePointer self) : base(self)
        {

        }
        public void SetQueryFilterData(PhyFilterData filterData)
        {
            unsafe
            {
                SDK_PhyController_SetQueryFilterData(CoreObject, &filterData);
            }
        }
        public PhyControllerCollisionFlag Move(ref Vector3 disp, float minDist, float elapsedTime, ref PhyFilterData filterData, PhyQueryFlag filterFlags)
        {
            unsafe
            {
                fixed (Vector3* pdisp = &disp)
                fixed (PhyFilterData* pfilterData = &filterData)
                {
                    return SDK_PhyController_Move(CoreObject, pdisp, minDist, elapsedTime, pfilterData, filterFlags);
                }
            }
        }
        public Vector3 Position
        {
            get
            {
                return SDK_PhyController_GetPosition(CoreObject);
            }
            set
            {
                unsafe
                {
                    SDK_PhyController_SetPosition(CoreObject, &value);
                }
            }
        }
        public Vector3 FootPosition
        {
            get
            {
                return SDK_PhyController_GetFootPosition(CoreObject);
            }
            set
            {
                unsafe
                {
                    SDK_PhyController_SetFootPosition(CoreObject, &value);
                }
            }
        }
        public float ContactOffset
        {
            get
            {
                return SDK_PhyController_GetContactOffset(CoreObject);
            }
            set
            {
                unsafe
                {
                    SDK_PhyController_GetContactOffset(CoreObject, value);
                }
            }
        }
        public float SlopeLimit
        {
            get
            {
                return SDK_PhyController_GetSlopeLimit(CoreObject);
            }
            set
            {
                unsafe
                {
                    SDK_PhyController_SetSlopeLimit(CoreObject, value);
                }
            }
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static unsafe PhyControllerCollisionFlag SDK_PhyController_Move(NativePointer self, Vector3* disp, float minDist, float elapsedTime, PhyFilterData* filterData, PhyQueryFlag filterFlags);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static unsafe void SDK_PhyController_SetPosition(NativePointer self, Vector3* position);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static unsafe Vector3 SDK_PhyController_GetPosition(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static unsafe void SDK_PhyController_SetFootPosition(NativePointer self, Vector3* position);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static unsafe Vector3 SDK_PhyController_GetFootPosition(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static unsafe void SDK_PhyController_GetContactOffset(NativePointer self, float offset);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static unsafe float SDK_PhyController_GetContactOffset(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static unsafe void SDK_PhyController_SetSlopeLimit(NativePointer self, float slopeLimit);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static unsafe float SDK_PhyController_GetSlopeLimit(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static unsafe void SDK_PhyController_SetQueryFilterData(NativePointer self, PhyFilterData* filterData);

        #endregion
    }
}
