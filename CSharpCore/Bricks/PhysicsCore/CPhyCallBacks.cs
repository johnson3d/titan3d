using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.PhysicsCore
{
    public enum PhyHitFlag
    {
        ePOSITION = (1 << 0),   //!< "position" member of #PxQueryHit is valid
        eNORMAL = (1 << 1), //!< "normal" member of #PxQueryHit is valid
        eDISTANCE = (1 << 2), //!< "distance" member of #PxQueryHit is valid. Deprecated: the system will always compute & return the distance.
        eUV = (1 << 3), //!< "u" and "v" barycentric coordinates of #PxQueryHit are valid. Not applicable to sweep queries.
        eASSUME_NO_INITIAL_OVERLAP = (1 << 4),  //!< Performance hint flag for sweeps when it is known upfront there's no initial overlap.
                                                //!< NOTE: using this flag may cause undefined results if shapes are initially overlapping.
        eMESH_MULTIPLE = (1 << 5),  //!< Report all hits for meshes rather than just the first. Not applicable to sweep queries.
        eMESH_ANY = (1 << 6),   //!< Report any first hit for meshes. If neither eMESH_MULTIPLE nor eMESH_ANY is specified,
                                //!< a single closest hit will be reported for meshes.
        eMESH_BOTH_SIDES = (1 << 7),    //!< Report hits with back faces of mesh triangles. Also report hits for raycast
                                        //!< originating on mesh surface and facing away from the surface normal. Not applicable to sweep queries.
                                        //!< Please refer to the user guide for heightfield-specific differences.
        ePRECISE_SWEEP = (1 << 8),  //!< Use more accurate but slower narrow phase sweep tests.
                                    //!< May provide better compatibility with PhysX 3.2 sweep behavior.
        eMTD = (1 << 9),    //!< Report the minimum translation depth, normal and contact point.
        eFACE_INDEX = (1 << 10),    //!< "face index" member of #PxQueryHit is valid

        eDEFAULT = ePOSITION | eNORMAL | eDISTANCE | eFACE_INDEX | eUV,

        /** \brief Only this subset of flags can be modified by pre-filter. Other modifications will be discarded. */
        eMODIFIABLE_FLAGS = eMESH_MULTIPLE | eMESH_BOTH_SIDES | eASSUME_NO_INITIAL_OVERLAP | ePRECISE_SWEEP
    };
    public struct PhyQueryFilterData
    {
        public PhyFilterData data;      //!< Filter data associated with the scene query
        public PhyQueryFlag flags;     //!< Filter flags (see #PxQueryFlags)
        public PhyQueryFilterData(PhyFilterData filterData, PhyQueryFlag flag = PhyQueryFlag.eSTATIC | PhyQueryFlag.eDYNAMIC)
        {
            data = filterData;
            flags = flag;
        }
    };
    public unsafe struct PhyContactPair
    {
        public CPhyShape.NativePointer shapes0;
        public CPhyShape.NativePointer shapes1;
        public byte* contactPatches;
        public byte* contactPoints;
        public float* contactImpulses;
        public UInt32 requiredBufferSize;
        public byte contactCount;
        public byte patchCount;
        public UInt16 contactStreamSize;
        public UInt16 flags;//PxContactPairFlags
        public UInt16 events;//PxPairFlags
        public fixed UInt32 internalData[2];  // For internal use only
    }
    public unsafe struct PhyContactPairHeader
    {
        public CPhyActor.NativePointer actors0;
        public CPhyActor.NativePointer actors1;
        public byte* extraDataStream;
        public UInt16 extraDataStreamSize;
        public UInt16 flags;//PxContactPairHeaderFlags
        public PhyContactPair* pairs;
        public UInt32 nbPairs;
    }
    public enum PhyFilterFlag
    {
        eKILL = (1 << 0),
        eSUPPRESS = (1 << 1),
        eCALLBACK = (1 << 2),
        eNOTIFY = (1 << 3) | eCALLBACK,
        eDEFAULT = 0
    }
    [Flags]
    public enum PhyPairFlag : UInt16
    {
        eSOLVE_CONTACT = (1 << 0),
        eMODIFY_CONTACTS = (1 << 1),
        eNOTIFY_TOUCH_FOUND = (1 << 2),
        eNOTIFY_TOUCH_PERSISTS = (1 << 3),
        eNOTIFY_TOUCH_LOST = (1 << 4),
        eNOTIFY_TOUCH_CCD = (1 << 5),
        eNOTIFY_THRESHOLD_FORCE_FOUND = (1 << 6),
        eNOTIFY_THRESHOLD_FORCE_PERSISTS = (1 << 7),
        eNOTIFY_THRESHOLD_FORCE_LOST = (1 << 8),
        eNOTIFY_CONTACT_POINTS = (1 << 9),
        eDETECT_DISCRETE_CONTACT = (1 << 10),
        eDETECT_CCD_CONTACT = (1 << 11),
        ePRE_SOLVER_VELOCITY = (1 << 12),
        ePOST_SOLVER_VELOCITY = (1 << 13),
        eCONTACT_EVENT_POSE = (1 << 14),
        eNEXT_FREE = (1 << 15),        //!< For internal use only.
        eCONTACT_DEFAULT = eSOLVE_CONTACT | eDETECT_DISCRETE_CONTACT,
        eTRIGGER_DEFAULT = eNOTIFY_TOUCH_FOUND | eNOTIFY_TOUCH_LOST | eDETECT_DISCRETE_CONTACT
    }
    [Flags]
    public enum PhyTriggerPairFlag : byte
    {
        eREMOVED_SHAPE_TRIGGER = (1 << 0),                  //!< The trigger shape has been removed from the actor/scene.
        eREMOVED_SHAPE_OTHER = (1 << 1),                    //!< The shape causing the trigger event has been removed from the actor/scene.
        eNEXT_FREE = (1 << 2)                   //!< For internal use only.
    }
    [Flags]
    public enum PhyFilterObjectFlag : UInt32
    {
        eKINEMATIC = (1 << 4),
        eTRIGGER = (1 << 5)
    }
    public unsafe struct PhyTriggerPair
    {
        public CPhyShape.NativePointer triggerShape;  //!< The shape that has been marked as a trigger.
        public CPhyActor.NativePointer triggerActor; //PxRigidActor*
        public CPhyShape.NativePointer otherShape;        //!< The shape causing the trigger event. \deprecated (see #PxSimulationEventCallback::onTrigger()) If collision between trigger shapes is enabled, then this member might point to a trigger shape as well.
        public CPhyActor.NativePointer otherActor;       //!< The actor to which otherShape is attached
        public PhyPairFlag status;            //!< Type of trigger event (eNOTIFY_TOUCH_FOUND or eNOTIFY_TOUCH_LOST). eNOTIFY_TOUCH_PERSISTS events are not supported.
        public PhyTriggerPairFlag flags;           //!< Additional information on the pair (see #PxTriggerPairFlag)
    }
    public struct PhyConstraintInfo
    {
        public IntPtr constraint;               //PxConstraint*
        public IntPtr externalReference;        //!< The external object which owns the constraint (see #PxConstraintConnector::getExternalReference())
        public UInt32 type;                 //!< Unique type ID of the external object. Allows to cast the provided external reference to the appropriate type
    };
    public struct PhyFilterData
    {
        public UInt32 word0;
        public UInt32 word1;
        public UInt32 word2;
        public UInt32 word3;
    }
    public unsafe class PhySimulationEventCallback
    {
        public unsafe delegate void FonConstraintBreak(IntPtr self, PhyConstraintInfo* constraints, UInt32 count);
        public unsafe delegate void FonWake(IntPtr self, CPhyActor.NativePointer* actors, UInt32 count);
        public unsafe delegate void FonSleep(IntPtr self, CPhyActor.NativePointer* actors, UInt32 count);
        public unsafe delegate void FonContact(IntPtr self, PhyContactPairHeader* pairHeader, PhyContactPair* pairs, UInt32 nbPairs);
        public unsafe delegate void FonTrigger(IntPtr self, PhyTriggerPair* pairs, UInt32 count);
        public unsafe delegate void FonAdvance(IntPtr self, /*const PxRigidBody*const**/CPhyEntity.NativePointer* bodyBuffer, PhyTransform* poseBuffer, UInt32 count);

        [System.Runtime.InteropServices.UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.StdCall)]
        public unsafe delegate PhyFilterFlag FPxSimulationFilterShader(//IntPtr self,
            UInt32 attributes0, PhyFilterData* filterData0,
            UInt32 attributes1, PhyFilterData* filterData1,
            PhyPairFlag* pairFlags, IntPtr constantBlock, UInt32 constantBlockSize);

        #region NativeCallBack
        public FonConstraintBreak onConstraintBreak = _OnConstraintBreak;
        public FonWake onWake = _OnWake;
        public FonSleep onSleep = _OnSleep;
        public FonContact onContact = _OnContact;
        public FonTrigger onTrigger = _OnTrigger;
        public FonAdvance onAdvance = _OnAdvance;
        public static FPxSimulationFilterShader pxSimulationFilterShader = _PxSimulationFilterShader;

#if PlatformIOS
        [ObjCRuntime.MonoPInvokeCallback(typeof(FonConstraintBreak))]
#endif
        private static unsafe void _OnConstraintBreak(IntPtr self, PhyConstraintInfo* constraints, UInt32 count)
        {
            var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(self);
            var cb = handle.Target as PhySimulationEventCallback;
            if (cb == null)
            {
                return;
            }

            cb.OnConstraintBreak(constraints, count);
        }

#if PlatformIOS
        [ObjCRuntime.MonoPInvokeCallback(typeof(FonWake))]
#endif
        private static unsafe void _OnWake(IntPtr self, CPhyActor.NativePointer* actors, UInt32 count)
        {
            var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(self);
            var cb = handle.Target as PhySimulationEventCallback;
            if (cb == null)
            {
                return;
            }
            cb.OnWake(actors, count);
        }

#if PlatformIOS
        [ObjCRuntime.MonoPInvokeCallback(typeof(FonSleep))]
#endif
        private static unsafe void _OnSleep(IntPtr self, CPhyActor.NativePointer* actors, UInt32 count)
        {
            var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(self);
            var cb = handle.Target as PhySimulationEventCallback;
            if (cb == null)
            {
                return;
            }

            cb.OnSleep(actors, count);
        }

#if PlatformIOS
        [ObjCRuntime.MonoPInvokeCallback(typeof(FonContact))]
#endif
        private static unsafe void _OnContact(IntPtr self, PhyContactPairHeader* pairHeader, PhyContactPair* pairs, UInt32 nbPairs)
        {
            var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(self);
            var cb = handle.Target as PhySimulationEventCallback;
            if (cb == null)
            {
                return;
            }
            cb.OnContact(pairHeader, pairs, nbPairs);
        }

#if PlatformIOS
        [ObjCRuntime.MonoPInvokeCallback(typeof(FonTrigger))]
#endif
        private static unsafe void _OnTrigger(IntPtr self, PhyTriggerPair* pairs, UInt32 count)
        {
            var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(self);
            var cb = handle.Target as PhySimulationEventCallback;
            if (cb == null)
            {
                return;
            }
            cb.OnTrigger(pairs, count);
        }

#if PlatformIOS
        [ObjCRuntime.MonoPInvokeCallback(typeof(FonAdvance))]
#endif
        private static unsafe void _OnAdvance(IntPtr self, /*const PxRigidBody*const**/CPhyEntity.NativePointer* bodyBuffer, PhyTransform* poseBuffer, UInt32 count)
        {
            var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(self);
            var cb = handle.Target as PhySimulationEventCallback;
            if (cb == null)
            {
                return;
            }
            cb.OnAdvance(bodyBuffer, poseBuffer, count);
        }
        public static bool PxFilterObjectIsTrigger(UInt32 attr)
        {
            return ((attr & (UInt32)PhyFilterObjectFlag.eTRIGGER) != 0);
        }

#if PlatformIOS
        [ObjCRuntime.MonoPInvokeCallback(typeof(FPxSimulationFilterShader))]
#endif
        private static unsafe PhyFilterFlag _PxSimulationFilterShader(//IntPtr self,
            UInt32 attributes0, PhyFilterData* filterData0,
            UInt32 attributes1, PhyFilterData* filterData1,
            PhyPairFlag* pairFlags, IntPtr constantBlock, UInt32 constantBlockSize)
        {
            if (PxFilterObjectIsTrigger(attributes0) || PxFilterObjectIsTrigger(attributes1))
            {
                *pairFlags = PhyPairFlag.eTRIGGER_DEFAULT;
                return PhyFilterFlag.eDEFAULT;
            }
            PhyPairFlag retFlags;
            retFlags = PhyPairFlag.eCONTACT_DEFAULT | PhyPairFlag.eNOTIFY_TOUCH_FOUND;
            *pairFlags = retFlags;
            if ((filterData0->word0 & filterData1->word1) == filterData0->word0 && (filterData1->word0 & filterData0->word1) == filterData1->word0)
            {
                return PhyFilterFlag.eDEFAULT;
            }
            else
            {
                return PhyFilterFlag.eKILL;
            }
            //return PhyFilterFlag.eDEFAULT;
        }
        #endregion
        public virtual unsafe void OnConstraintBreak(PhyConstraintInfo* constraints, UInt32 count)
        {

        }
        public virtual unsafe void OnWake(CPhyActor.NativePointer* actors, UInt32 count)
        {

        }
        public virtual unsafe void OnSleep(CPhyActor.NativePointer* actors, UInt32 count)
        {

        }
        public virtual unsafe void OnContact(PhyContactPairHeader* pairHeader, PhyContactPair* pairs, UInt32 nbPairs)
        {

            unsafe
            {
                CPhyEntity entity1 = null, entity2 = null;
                var pair = (*pairHeader);
                var entity1Handle = CPhyEntity.SDK_PhyEntity_GetCSharpHandle(pair.actors0);
                if (entity1Handle != IntPtr.Zero)
                {
                    var weakRef = System.Runtime.InteropServices.GCHandle.FromIntPtr(entity1Handle);
                    entity1 = weakRef.Target as CPhyEntity;
                }
                var entity2Handle = CPhyEntity.SDK_PhyEntity_GetCSharpHandle(pair.actors0);
                if (entity2Handle != IntPtr.Zero)
                {
                    var weakRef = System.Runtime.InteropServices.GCHandle.FromIntPtr(entity2Handle);
                    entity2 = weakRef.Target as CPhyEntity;
                }
                if (entity1.EntityType == PhyEntityType.PET_Actor)
                {

                }
                if (entity1.EntityType == PhyEntityType.PET_Controller)
                {

                }
                if (entity2.EntityType == PhyEntityType.PET_Controller)
                {

                }
            }
        }
        public virtual unsafe void OnTrigger(PhyTriggerPair* pairs, UInt32 count)
        {
            unsafe
            {
                CPhyEntity triggerEntity = null, otherEntity = null;
                for (int i = 0; i < count; ++i)
                {
                    var pair = pairs[i];
                    var otherHandle = CPhyEntity.SDK_PhyEntity_GetCSharpHandle(pair.otherActor);
                    if (otherHandle != IntPtr.Zero)
                    {
                        var weakRef = System.Runtime.InteropServices.GCHandle.FromIntPtr(otherHandle);
                        otherEntity = weakRef.Target as CPhyEntity;
                    }
                    var triggerHandle = CPhyActor.SDK_PhyEntity_GetCSharpHandle(pair.triggerActor);
                    if (triggerHandle != IntPtr.Zero)
                    {
                        var weakRef = System.Runtime.InteropServices.GCHandle.FromIntPtr(triggerHandle);
                        triggerEntity = weakRef.Target as CPhyEntity;
                    }
                    if (otherEntity != null)
                    {
                        //if ((pair.status & PhyPairFlag.eNOTIFY_TOUCH_FOUND) == PhyPairFlag.eNOTIFY_TOUCH_FOUND)
                        //    triggerPhyActor.HostComponent?.OnTriggerIn();
                    }
                    if (triggerEntity != null && otherEntity != null)
                    {
                        var triggerActor = triggerEntity as CPhyActor;
                        if (triggerActor != null)
                        {
                            if (otherEntity.EntityType == PhyEntityType.PET_Actor)
                            {
                                var otherActor = otherEntity as CPhyActor;
                                if ((pair.status & PhyPairFlag.eNOTIFY_TOUCH_FOUND) == PhyPairFlag.eNOTIFY_TOUCH_FOUND)
                                    triggerActor.HostComponent?.OnTriggerIn(otherActor);
                                if ((pair.status & PhyPairFlag.eNOTIFY_TOUCH_LOST) == PhyPairFlag.eNOTIFY_TOUCH_LOST)
                                    triggerActor.HostComponent?.OnTriggerOut(otherActor);
                            }
                            if (otherEntity.EntityType == PhyEntityType.PET_Controller)
                            {
                                var otherController = otherEntity as CPhyController;
                                if ((pair.status & PhyPairFlag.eNOTIFY_TOUCH_FOUND) == PhyPairFlag.eNOTIFY_TOUCH_FOUND)
                                    triggerActor.HostComponent?.OnTriggerIn(otherController);
                                if ((pair.status & PhyPairFlag.eNOTIFY_TOUCH_LOST) == PhyPairFlag.eNOTIFY_TOUCH_LOST)
                                    triggerActor.HostComponent?.OnTriggerOut(otherController);
                            }
                        }
                    }
                }
            }
        }
        public virtual unsafe void OnAdvance(/*const PxRigidBody*const**/CPhyEntity.NativePointer* bodyBuffer, PhyTransform* poseBuffer, UInt32 count)
        {

        }
        //public virtual unsafe UInt16 PxSimulationFilterShader(
        //    UInt32 attributes0, PhyFilterData filterData0,
        //    UInt32 attributes1, PhyFilterData filterData1,
        //    UInt16* pairFlags, IntPtr constantBlock, UInt32 constantBlockSize)
        //{
        //    return 0;
        //}
    };
}
