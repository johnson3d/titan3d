using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace EngineNS.Bricks.PhysicsCore
{
    public class CPhySceneDesc : AuxCoreObject<CPhySceneDesc.NativePointer>
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
        public CPhySceneDesc(NativePointer self)
        {
            mCoreObject = self;
        }
        public void Clear()
        {
            SDK_PhySceneDesc_SetContactCallBack(CoreObject, IntPtr.Zero, null, null, null, null, null, null, PhySimulationEventCallback.pxSimulationFilterShader);
            if (SimulationEventCallbackHandle.IsAllocated)
                SimulationEventCallbackHandle.Free();
        }
        ~CPhySceneDesc()
        {

        }
        public Vector3 Gravity
        {
            get
            {
                Vector3 ret = new Vector3();
                unsafe
                {
                    SDK_PhySceneDesc_GetGravity(CoreObject, &ret);
                }
                return ret;
            }
            set
            {
                unsafe
                {
                    SDK_PhySceneDesc_SetGravity(CoreObject, &value);
                }
            }
        }
        public enum PxSceneFlag : UInt32
        {
            eENABLE_ACTIVE_ACTORS = (1 << 0),
            eENABLE_ACTIVETRANSFORMS = (1 << 1),//PX_DEPRECATED
            eENABLE_CCD = (1 << 2),
            eDISABLE_CCD_RESWEEP = (1 << 3),
            eADAPTIVE_FORCE = (1 << 4),
            eENABLE_KINEMATIC_STATIC_PAIRS = (1 << 5),
            eENABLE_KINEMATIC_PAIRS = (1 << 6),
            eENABLE_PCM = (1 << 9),
            eDISABLE_CONTACT_REPORT_BUFFER_RESIZE = (1 << 10),
            eDISABLE_CONTACT_CACHE = (1 << 11),
            eREQUIRE_RW_LOCK = (1 << 12),
            eENABLE_STABILIZATION = (1 << 14),
            eENABLE_AVERAGE_POINT = (1 << 15),
            eDEPRECATED_TRIGGER_TRIGGER_REPORTS = (1 << 16),
            eEXCLUDE_KINEMATICS_FROM_ACTIVE_ACTORS = (1 << 17),
            eSUPPRESS_EAGER_SCENE_QUERY_REFIT = (1 << 18),//PX_DEPRECATED
            eENABLE_GPU_DYNAMICS = (1 << 19),
            eENABLE_ENHANCED_DETERMINISM = (1 << 20),

            eMUTABLE_FLAGS = eENABLE_ACTIVE_ACTORS | eENABLE_ACTIVETRANSFORMS | eEXCLUDE_KINEMATICS_FROM_ACTIVE_ACTORS
        }

        public UInt32 SceneFlags
        {
            get
            {
                return SDK_PhySceneDesc_GetFlags(CoreObject);
            }
            set
            {
                SDK_PhySceneDesc_SetFlags(CoreObject, value);
            }
        }
        public UInt32 ContactDataBlocks
        {
            get
            {
                return SDK_PhySceneDesc_GetContactDataBlocks(CoreObject);
            }
            set
            {
                SDK_PhySceneDesc_SetContactDataBlocks(CoreObject, value);
            }
        }

        PhySimulationEventCallback SimulationEventCallback;
        System.Runtime.InteropServices.GCHandle SimulationEventCallbackHandle;

        public void SetPhySimulationEventCallback(PhySimulationEventCallback cb)
        {
            if (cb == null)
            {
                SDK_PhySceneDesc_SetContactCallBack(CoreObject, IntPtr.Zero, null, null, null, null, null, null, PhySimulationEventCallback.pxSimulationFilterShader);
                if (SimulationEventCallbackHandle != null)
                    SimulationEventCallbackHandle.Free();
            }
            else
            {
                SimulationEventCallback = cb;
                SimulationEventCallbackHandle = System.Runtime.InteropServices.GCHandle.Alloc(cb, GCHandleType.WeakTrackResurrection);
                SDK_PhySceneDesc_SetContactCallBack(CoreObject, System.Runtime.InteropServices.GCHandle.ToIntPtr(SimulationEventCallbackHandle),
                    cb.onContact,
                    cb.onTrigger,
                    cb.onConstraintBreak,
                    cb.onWake,
                    cb.onSleep,
                    cb.onAdvance,
                    PhySimulationEventCallback.pxSimulationFilterShader);
            }
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static unsafe void SDK_PhySceneDesc_SetGravity(NativePointer self, Vector3* gravity);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static unsafe void SDK_PhySceneDesc_GetGravity(NativePointer self, Vector3* gravity);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static void SDK_PhySceneDesc_SetFlags(NativePointer self, UInt32 flags);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static UInt32 SDK_PhySceneDesc_GetFlags(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static void SDK_PhySceneDesc_SetContactDataBlocks(NativePointer self, UInt32 nb);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static UInt32 SDK_PhySceneDesc_GetContactDataBlocks(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static void SDK_PhySceneDesc_SetContactCallBack(NativePointer self, IntPtr handle,
                            PhySimulationEventCallback.FonContact onContact,
                            PhySimulationEventCallback.FonTrigger onTrigger,
                            PhySimulationEventCallback.FonConstraintBreak onConstraintBreak,
                            PhySimulationEventCallback.FonWake onWake,
                            PhySimulationEventCallback.FonSleep onSleep,
                            PhySimulationEventCallback.FonAdvance onAdvance,
                            PhySimulationEventCallback.FPxSimulationFilterShader pxSimulationFilterShader);
        #endregion
    }
    public class CPhyScene : CPhyEntity
    {
        public CPhyScene(NativePointer self, Guid sceneId) : base(self)
        {
            SceneId = sceneId;
        }
        public void Clear()
        {
            Desc.Clear();
        }
        public Guid SceneId
        {
            get;
            private set;
        }
        public CPhySceneDesc Desc
        {
            get;
            set;
        }
        public GamePlay.SceneGraph.GSceneGraph GameScene
        {
            get;
            set;
        }
        public bool NeedTick
        {
            get
            {
                if (GameScene == null)
                    return false;
                return GameScene.NeedTick;
            }
        }
        public HashSet<CPhyActor> Actors
        {
            get;
        } = new HashSet<CPhyActor>();
        public void RemoveAllActors()
        {
            foreach (var i in Actors)
            {
                i.Scene = null;
            }
            Actors.Clear();
        }
        public void Simulate(float elapsedTime)
        {
            IntPtr completionTask = IntPtr.Zero;
            IntPtr scratchMemBlock = IntPtr.Zero;
            UInt32 scratchMemBlockSize = 0;
            vBOOL controlSimulation = vBOOL.FromBoolean(true);
            SDK_PhyScene_Simulate(CoreObject, elapsedTime, completionTask, scratchMemBlock, scratchMemBlockSize, controlSimulation);
        }
        public bool FetchResults()
        {
            vBOOL block = vBOOL.FromBoolean(true);
            unsafe
            {
                UInt32 errorState = 0;
                return SDK_PhyScene_FetchResults(CoreObject, block, &errorState);
            }
        }
        public void UpdateActorTransforms()
        {
            SDK_PhyScene_UpdateActorTransforms(CoreObject);
        }
        public bool Raycast(ref Vector3 origin, ref Vector3 unitDir, float maxDistance, ref GamePlay.SceneGraph.VHitResult hitResult)
        {
            unsafe
            {
                fixed (Vector3* porigin = &origin)
                fixed (Vector3* punitDir = &unitDir)
                fixed (GamePlay.SceneGraph.VHitResult* phitResult = &hitResult)
                {
                    return SDK_PhyScene_Raycast(CoreObject, porigin, punitDir, maxDistance, phitResult);
                }
            }
        }
        public bool RaycastWithFilter(ref Vector3 origin, ref Vector3 unitDir, float maxDistance, ref PhyQueryFilterData queryFilterData, ref GamePlay.SceneGraph.VHitResult hitResult)
        {
            unsafe
            {
                fixed (Vector3* porigin = &origin)
                fixed (Vector3* punitDir = &unitDir)
                fixed (PhyQueryFilterData* pqueryFilterData = &queryFilterData)
                fixed (GamePlay.SceneGraph.VHitResult* phitResult = &hitResult)
                {
                    return SDK_PhyScene_RaycastWithFilter(CoreObject, porigin, punitDir, maxDistance, pqueryFilterData, phitResult);
                }
            }
        }
        public bool Sweep(CPhyShape shape, Vector3 position, Vector3 unitDir, float maxDistance, ref GamePlay.SceneGraph.VHitResult hitResult)
        {
            unsafe
            {
                fixed (GamePlay.SceneGraph.VHitResult* phitResult = &hitResult)
                {
                    return SDK_PhyScene_Sweep(CoreObject, shape.CoreObject, &position, &unitDir, maxDistance, phitResult);
                }
            }
        }
        public bool SweepWithFilter(CPhyShape shape, Vector3 position, Vector3 unitDir, float maxDistance, ref PhyQueryFilterData queryFilterData, ref GamePlay.SceneGraph.VHitResult hitResult)
        {
            unsafe
            {
                fixed (PhyQueryFilterData* pqueryFilterData = &queryFilterData)
                fixed (GamePlay.SceneGraph.VHitResult* phitResult = &hitResult)
                {
                    return SDK_PhyScene_SweepWithFilter(CoreObject, shape.CoreObject, &position, &unitDir, maxDistance, pqueryFilterData, phitResult);
                }
            }
        }

        //public bool Overlap(ref BoundingBox box, ref GamePlay.SceneGraph.VHitResult hitResult)
        //{
        //    unsafe
        //    {
        //        fixed (BoundingBox* pbox = &box)
        //        fixed (GamePlay.SceneGraph.VHitResult* phitResult = &hitResult)
        //        {
        //            return SDK_PhyScene_Overlap(CoreObject, pbox, phitResult);
        //        }
        //    }
        //}
        public bool Overlap(CPhyShape shape, Vector3 postion, Quaternion rotation, ref GamePlay.SceneGraph.VHitResult hitResult)
        {
            unsafe
            {
                fixed (GamePlay.SceneGraph.VHitResult* phitResult = &hitResult)
                {
                    return SDK_PhyScene_Overlap(CoreObject, shape.CoreObject, &postion, &rotation, phitResult);
                }
            }
        }
        public bool OverlapWithFilter(CPhyShape shape, Vector3 postion, Quaternion rotation, ref PhyQueryFilterData queryFilterData, ref GamePlay.SceneGraph.VHitResult hitResult)
        {
            unsafe
            {
                fixed (PhyQueryFilterData* pqueryFilterData = &queryFilterData)
                fixed (GamePlay.SceneGraph.VHitResult* phitResult = &hitResult)
                {
                    return SDK_PhyScene_OverlapWithFilter(CoreObject, shape.CoreObject, &postion, &rotation, pqueryFilterData, phitResult);
                }
            }
        }
        public CPhyController CreateBoxController(CPhyBoxControllerDesc desc)
        {
            var ptr = SDK_PhyScene_CreateBoxController(CoreObject, desc.CoreObject);
            return new CPhyController(ptr);
        }
        public CPhyController CreateCapsuleController(CPhyCapsuleControllerDesc desc)
        {
            var ptr = SDK_PhyScene_CreateCapsuleController(CoreObject, desc.CoreObject);
            return new CPhyController(ptr);
        }
        private long PrevTickTime;
        public void Tick()
        {
            if (PrevTickTime == 0)
            {
                PrevTickTime = Support.Time.HighPrecision_GetTickCount();
                return;
            }
            var now = Support.Time.HighPrecision_GetTickCount();
            float elapse = (float)(now - PrevTickTime) / 1000000.0F;
            PrevTickTime = now;
            if (elapse > 1.0F || elapse <= 0.0F)
                return;
            //这里要考虑elapse过久，分多次tick处理
            const float StepTime = 1 / 20.0f;
            int count = (int)(elapse / StepTime);
            float fm = elapse % StepTime;
            for (int i = 0; i < count; i++)
            {
                Simulate(elapse);
                FetchResults();
            }
            Simulate(fm);
            FetchResults();
            UpdateActorTransforms();
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static void SDK_PhyScene_Simulate(NativePointer self, float elapsedTime, IntPtr completionTask, IntPtr scratchMemBlock, UInt32 scratchMemBlockSize, vBOOL controlSimulation);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static unsafe vBOOL SDK_PhyScene_FetchResults(NativePointer self, vBOOL block, UInt32* errorState);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static unsafe vBOOL SDK_PhyScene_Raycast(NativePointer self, Vector3* origin, Vector3* unitDir, float maxDistance, GamePlay.SceneGraph.VHitResult* hitResult);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static unsafe vBOOL SDK_PhyScene_Sweep(NativePointer self, CPhyShape.NativePointer shape, Vector3* position, Vector3* unitDir, float maxDistance, GamePlay.SceneGraph.VHitResult* hitResult);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static unsafe vBOOL SDK_PhyScene_Overlap(NativePointer self, CPhyShape.NativePointer shape, Vector3* position, Quaternion* rotation, GamePlay.SceneGraph.VHitResult* hitResult);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static unsafe vBOOL SDK_PhyScene_RaycastWithFilter(NativePointer self, Vector3* origin, Vector3* unitDir, float maxDistance, PhyQueryFilterData* queryFilterData, GamePlay.SceneGraph.VHitResult* hitResult);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static unsafe vBOOL SDK_PhyScene_SweepWithFilter(NativePointer self, CPhyShape.NativePointer shape, Vector3* position, Vector3* unitDir, float maxDistance, PhyQueryFilterData* queryFilterData, GamePlay.SceneGraph.VHitResult* hitResult);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static unsafe vBOOL SDK_PhyScene_OverlapWithFilter(NativePointer self, CPhyShape.NativePointer shape, Vector3* position, Quaternion* rotation, PhyQueryFilterData* queryFilterData, GamePlay.SceneGraph.VHitResult* hitResult);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static void SDK_PhyScene_UpdateActorTransforms(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static CPhyController.NativePointer SDK_PhyScene_CreateBoxController(NativePointer self, CPhyBoxControllerDesc.NativePointer desc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static CPhyController.NativePointer SDK_PhyScene_CreateCapsuleController(NativePointer self, CPhyCapsuleControllerDesc.NativePointer desc);
        #endregion
    }
}

namespace EngineNS.GamePlay.SceneGraph
{
    public partial class GSceneGraph
    {
        protected Bricks.PhysicsCore.CPhyScene mPhyScene;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Browsable(false)]
        public Bricks.PhysicsCore.CPhyScene PhyScene
        {
            get { return mPhyScene; }
        }
        public void CreateDefaultPhysicsScene()
        {
            if (CEngine.Instance.PhyContext == null)
            {
                return;
            }
            var phyDesc = CEngine.Instance.PhyContext.CreateSceneDesc();
            UInt32 sceneFlags = 0;
            sceneFlags |= (UInt32)EngineNS.Bricks.PhysicsCore.CPhySceneDesc.PxSceneFlag.eENABLE_ACTIVETRANSFORMS;
            //sceneFlags |= (UInt32)EngineNS.Bricks.PhysicsCore.CPhySceneDesc.PxSceneFlag.PX_DEPRECATED; 
            //sceneFlags |= (UInt32)EngineNS.Bricks.PhysicsCore.CPhySceneDesc.PxSceneFlag.eENABLE_ACTIVE_ACTORS;
            sceneFlags |= (UInt32)EngineNS.Bricks.PhysicsCore.CPhySceneDesc.PxSceneFlag.eENABLE_CCD;
            sceneFlags |= (UInt32)EngineNS.Bricks.PhysicsCore.CPhySceneDesc.PxSceneFlag.eENABLE_KINEMATIC_PAIRS;
            sceneFlags |= (UInt32)EngineNS.Bricks.PhysicsCore.CPhySceneDesc.PxSceneFlag.eENABLE_KINEMATIC_STATIC_PAIRS;
            sceneFlags |= (UInt32)EngineNS.Bricks.PhysicsCore.CPhySceneDesc.PxSceneFlag.eREQUIRE_RW_LOCK;
            phyDesc.SceneFlags = sceneFlags;
            phyDesc.ContactDataBlocks = 4;
            phyDesc.Gravity = new Vector3(0, -9.81f, 0);
            phyDesc.SetPhySimulationEventCallback(new EngineNS.Bricks.PhysicsCore.PhySimulationEventCallback());
            if (false == this.CreatePhysicsScene(phyDesc))
            {
                EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Warning, "Physics", $"PhysicsScene Create Failed");
            }
            else
            {
                this.AddPhysicsTick();
            }
        }
        public bool CreatePhysicsScene(Bricks.PhysicsCore.CPhySceneDesc desc)
        {
            mPhyScene = CEngine.Instance.PhyContext.CreateScene(this.SceneId, desc);
            mPhyScene.GameScene = this;
            return mPhyScene != null;
        }
        public void AddPhysicsTick()
        {
            if (mPhyScene != null)
                CEngine.Instance.PhyContext.AddTickScene(mPhyScene);
        }
        public void RemovePhysicsTick()
        {
            if (mPhyScene != null)
                CEngine.Instance.PhyContext.RemoveTickScene(mPhyScene.SceneId);
        }
        partial void _CreatePhysicsScene(GSceneGraphDesc parameter)
        {
            CreateDefaultPhysicsScene();
        }
        partial void _CleanupPhysicsScene()
        {
            if (mPhyScene != null)
            {
                mPhyScene.Clear();
                mPhyScene.RemoveAllActors();
                CEngine.Instance.PhyContext.RemoveTickScene(mPhyScene.SceneId);
            }
        }
        partial void LineCheckByPhysics(ref Vector3 start, ref Vector3 end, ref VHitResult rst, ref bool isChecked)
        {
            isChecked = false;
            if (mPhyScene == null)
                return;
            var dir = end - start;
            var dist = dir.Length();
            dir = dir / dist;
            isChecked = mPhyScene.Raycast(ref start, ref dir, dist, ref rst);
        }
        partial void LineCheckWithFilterByPhysics(ref Vector3 start, ref Vector3 end, ref Bricks.PhysicsCore.PhyQueryFilterData queryFilterData, ref VHitResult rst, ref bool isChecked)
        {
            isChecked = false;
            if (mPhyScene == null)
                return;
            var dir = end - start;
            var dist = dir.Length();
            dir = dir / dist;
            isChecked = mPhyScene.RaycastWithFilter(ref start, ref dir, dist,ref queryFilterData, ref rst);
        }
        partial void GetHitActorByPhysics(ref SceneGraph.VHitResult hitResult, ref Actor.GActor outActor)
        {
            if (hitResult.ExtData == IntPtr.Zero)
                return;
            var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(hitResult.ExtData);
            var phyActor = handle.Target as Bricks.PhysicsCore.CPhyActor;
            if (phyActor == null)
                return;

            outActor = phyActor.HostActor;
        }

        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Actor.GActor PickActor(out VHitResult hitResult, Graphics.CGfxCamera camera, int x, int y, float length = 1000.0f)
        {
            if (camera == null)
                camera = CEngine.Instance.GameInstance?.GameCamera;
            if (camera == null)
            {
                hitResult = VHitResult.Default;
                return null;
            }
            var pickDir = new Vector3();
            if (camera.GetPickRay(ref pickDir, x, y, 0, 0) == false)
            {
                hitResult = VHitResult.Default;
                return null;
            }
            var start = camera.CameraData.Position;
            var end = camera.CameraData.Position + pickDir * length;
            var tmp = VHitResult.Default;
            Bricks.PhysicsCore.PhyFilterData filterData = new Bricks.PhysicsCore.PhyFilterData();
            filterData.word1 = (uint)Bricks.PhysicsCore.CollisionComponent.CollisionLayers.Camera;
            Bricks.PhysicsCore.PhyQueryFilterData queryFilterData = new Bricks.PhysicsCore.PhyQueryFilterData(filterData);
            if (this.LineCheckWithFilter(ref start, ref end, ref queryFilterData,ref tmp) == false)
            {
                hitResult = VHitResult.Default;
                return null;
            }
            hitResult = tmp;
            Actor.GActor outActor = null;
            GetHitActorByPhysics(ref hitResult, ref outActor);

            return outActor;
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Actor.GActor PickActorWithFilter(out VHitResult hitResult, ref Bricks.PhysicsCore.PhyQueryFilterData queryFilterData, Graphics.CGfxCamera camera, int x, int y, float length = 1000.0f)
        {
            if (camera == null)
                camera = CEngine.Instance.GameInstance?.GameCamera;
            if (camera == null)
            {
                hitResult = VHitResult.Default;
                return null;
            }
            var pickDir = new Vector3();
            if (camera.GetPickRay(ref pickDir, x, y, 0, 0) == false)
            {
                hitResult = VHitResult.Default;
                return null;
            }
            var start = camera.CameraData.Position;
            var end = camera.CameraData.Position + pickDir * length;
            var tmp = VHitResult.Default;
            if (this.LineCheckWithFilter(ref start, ref end, ref queryFilterData,ref tmp) == false)
            {
                hitResult = VHitResult.Default;
                return null;
            }
            hitResult = tmp;
            Actor.GActor outActor = null;
            GetHitActorByPhysics(ref hitResult, ref outActor);

            return outActor;
        }
    }
}
