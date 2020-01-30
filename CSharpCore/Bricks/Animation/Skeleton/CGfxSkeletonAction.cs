using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using EngineNS.Bricks.Animation.Pose;

namespace EngineNS.Bricks.Animation.Skeleton
{
    public class CGfxSkeletonAction : AuxCoreObject<CGfxSkeletonAction.NativePointer>
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
        public uint Duration
        {
            get
            {
                return SDK_GfxSkeletonAction_GetDuration(mCoreObject);
            }
        }
        public uint FrameCount
        {
            get
            {
                return SDK_GfxSkeletonAction_GetFrameCount(mCoreObject);
            }
        }
        public float Fps
        {
            get
            {
                return SDK_GfxSkeletonAction_GetFps(mCoreObject);
            }
        }

        public CGfxSkeletonAction()
        {
            mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::GfxSkeletonAction");
            if (mCoreObject.Pointer != IntPtr.Zero)
            {
                mResourceState = new CResourceState(SDK_VIUnknown_GetResourceState(mCoreObject.Pointer));
            }
        }
        public CGfxSkeletonAction(NativePointer nativePointer)
        {
            mCoreObject = nativePointer;
            if (mCoreObject.Pointer != IntPtr.Zero)
            {
                mResourceState = new CResourceState(SDK_VIUnknown_GetResourceState(mCoreObject.Pointer));
            }
        }

        private CResourceState mResourceState;
        public CResourceState ResourceState
        {
            get { return mResourceState; }
        }
        public bool Init(CRenderContext rc, RName name)
        {
            return SDK_GfxSkeletonAction_Init(CoreObject, rc.CoreObject, name.Name);
        }
        public void GetAnimaPose(Int64 time,ref CGfxAnimationPoseProxy poseProxy,bool withMotionData = true)
        {
            SDK_GfxSkeletonAction_GetAnimaPose(CoreObject, time, poseProxy.Pose.CoreObject,vBOOL.FromBoolean(withMotionData));
        }
        public void GetAnimaPose(Int64 time, ref CGfxSkeletonPose pose, bool withMotionData = true)
        {
            SDK_GfxSkeletonAction_GetAnimaPose(CoreObject, time, pose.CoreObject, vBOOL.FromBoolean(withMotionData));
        }
        public void GetMotionData(Int64 time,uint nameHash,ref CGfxMotionState motionData)
        {
            unsafe
            {
                fixed (CGfxMotionState* p = &motionData)
                {
                    SDK_GfxSkeletonAction_GetMotionDatas(CoreObject, time, nameHash, p);
                }
            }
        }
        public bool LoadSkeletonAction(CRenderContext rc, RName name, bool firstLoad)
        {
            using (var xnd = IO.XndHolder.SyncLoadXND(name.Address))
            {
                if (xnd == null)
                    return false;

                if (false == SDK_GfxSkeletonAction_LoadXnd(CoreObject, rc.CoreObject, name.Name, xnd.Node.CoreObject, firstLoad))
                    return false;

                var num = SDK_GfxSkeletonAction_GetBoneNumber(CoreObject);
                mAnimNodes = new CGfxBoneAnim[num];
                for (UInt32 i = 0; i < num; i++)
                {
                    var ptr = SDK_GfxSkeletonAction_GetBoneAnum(CoreObject, i);
                    if (ptr.Pointer == IntPtr.Zero)
                        continue;
                    mAnimNodes[i] = new CGfxBoneAnim(ptr);
                }
                return true;
            }
        }
        public bool MakeTPoseActionFromSkeleton(CGfxSkeleton skeleton, RName name, bool firstLoad)
        {
            if (false == SDK_GfxSkeletonAction_MakeTPoseActionFromSkeleton(CoreObject, skeleton.CoreObject, name.Name,firstLoad))
                return false;

            var num = SDK_GfxSkeletonAction_GetBoneNumber(CoreObject);
            mAnimNodes = new CGfxBoneAnim[num];
            for (UInt32 i = 0; i < num; i++)
            {
                var ptr = SDK_GfxSkeletonAction_GetBoneAnum(CoreObject, i);
                if (ptr.Pointer == IntPtr.Zero)
                    continue;
                mAnimNodes[i] = new CGfxBoneAnim(ptr);
            }
            return true;
        }

        private void SaveSkeletonAction(bool needPreUse = true)
        {
            if (needPreUse)
                this.PreUse(true);
            var xnd = IO.XndHolder.NewXNDHolder();
            var node = xnd.Node;
            SDK_GfxSkeletonAction_Save2Xnd(CoreObject, node.CoreObject);
            IO.XndHolder.SaveXND(Name.Address, xnd);
        }
        public RName Name
        {
            get
            {
                return RName.GetRName(SDK_GfxSkeletonAction_GetName(CoreObject));
            }
        }
        CGfxBoneAnim[] mAnimNodes;
        public CGfxBoneAnim[] AnimNodes
        {
            get { return mAnimNodes; }
        }

        public void FixBoneTree(CGfxSkeleton skeleton)
        {
            SDK_GfxSkeletonAction_FixBoneTree(CoreObject, skeleton.CoreObject);
        }

        public void FixBoneAnimPose(CGfxSkeleton skeleton)
        {
            SDK_GfxSkeletonAction_FixBoneAnimPose(CoreObject, skeleton.CoreObject);
        }
        public void PreUse(bool isSync = false)
        {
            ResourceState.AccessTime = CEngine.Instance.EngineTime;
            if (Thread.ContextThread.CurrentContext == CEngine.Instance.ThreadAsync)
            {
                PreUseInAsyncThread(isSync);
                return;
            }
            switch (ResourceState.StreamState)
            {
                case EStreamingState.SS_Valid:
                    {
                        return;
                    }
                case EStreamingState.SS_Invalid:
                    {
                        if (isSync)
                        {
                            ResourceState.StreamState = EStreamingState.SS_Streaming;
                            if (false == this.RestoreResource())
                            {
                                ResourceState.StreamState = EStreamingState.SS_Invalid;
                            }
                            else
                            {
                                ResourceState.StreamState = EStreamingState.SS_Valid;
                            }
                        }
                        else
                        {
                            var pendings = CEngine.Instance.SkeletonActionManager.PendingActions;
                            lock (pendings)
                            {
                                ResourceState.StreamState = EStreamingState.SS_Pending;
                                pendings.Add(this);
                            }
                        }
                    }
                    break;
                case EStreamingState.SS_Pending:
                    {
                        if (isSync)
                        {
                            while (ResourceState.StreamState != EStreamingState.SS_Valid)
                            {
                                System.Threading.Thread.Sleep(0);
                            }
                        }
                    }
                    break;
                case EStreamingState.SS_Streaming:
                    {
                        if (isSync)
                        {
                            while (ResourceState.StreamState != EStreamingState.SS_Valid)
                            {
                                System.Threading.Thread.Sleep(0);
                            }
                        }
                    }
                    break;
                case EStreamingState.SS_Killing:
                    {
                        if (isSync)
                        {
                            while (ResourceState.StreamState != EStreamingState.SS_Invalid)
                            {
                                System.Threading.Thread.Sleep(0);
                            }
                            PreUse(true);
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        private void PreUseInAsyncThread(bool isSync)
        {
            if (isSync)
            {
                if (ResourceState.StreamState == EStreamingState.SS_Invalid)
                {
                    ResourceState.StreamState = EStreamingState.SS_Streaming;
                    if (false == this.RestoreResource())
                    {
                        ResourceState.StreamState = EStreamingState.SS_Invalid;
                    }
                    else
                    {
                        ResourceState.StreamState = EStreamingState.SS_Valid;
                    }
                }
            }
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxSkeletonAction_Init(NativePointer self, CRenderContext.NativePointer rc, string name);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxSkeletonAction_LoadXnd(NativePointer self, CRenderContext.NativePointer rc, string name, IO.XndNode.NativePointer node, bool isLoad);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxSkeletonAction_MakeTPoseActionFromSkeleton(NativePointer self, CGfxSkeleton.NativePointer skeleton, string name, bool isLoad);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
        public extern static string SDK_GfxSkeletonAction_GetName(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static uint SDK_GfxSkeletonAction_GetBoneNumber(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static uint SDK_GfxSkeletonAction_GetDuration(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static uint SDK_GfxSkeletonAction_GetFrameCount(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static float SDK_GfxSkeletonAction_GetFps(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CGfxBoneAnim.NativePointer SDK_GfxSkeletonAction_GetBoneAnum(NativePointer self, UInt32 index);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CGfxBoneAnim.NativePointer SDK_GfxSkeletonAction_FindBoneAnimByHashId(NativePointer self, int hash);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxSkeletonAction_FixBoneTree(NativePointer self, CGfxSkeleton.NativePointer skeleton);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxSkeletonAction_FixBoneAnimPose(NativePointer self, CGfxSkeleton.NativePointer skeleton);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxSkeletonAction_GetAnimaPose(NativePointer self, Int64 time, CGfxSkeletonPose.NativePointer animPose,vBOOL withMotionData);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_GfxSkeletonAction_GetMotionDatas(NativePointer self, Int64 time, uint nameHash, CGfxMotionState* motionData);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxSkeletonAction_Save2Xnd(NativePointer self, IO.XndNode.NativePointer node);
        #endregion
    }
    public class CGfxSkeletonActionManager
    {
        public Dictionary<RName, CGfxSkeletonAction> SkeletonActions
        {
            get;
        } = new Dictionary<RName, CGfxSkeletonAction>();
        public CGfxSkeletonAction GetSkeletonAction(CRenderContext rc, RName name, bool firstLoad = true)
        {
            if (name.IsExtension(CEngineDesc.AnimationSequenceExtension) == false)
                return null;

            CGfxSkeletonAction action;
            if (false == SkeletonActions.TryGetValue(name, out action))
            {
                action = new CGfxSkeletonAction();
                if (action.LoadSkeletonAction(rc, name, firstLoad) == false)
                    return null;

                SkeletonActions.Add(name, action);
            }
            return action;
        }
        public CGfxSkeletonAction GetSkeletonAction(CGfxSkeleton skeleton, RName name, bool firstLoad = true)
        {
            CGfxSkeletonAction action;
            if (false == SkeletonActions.TryGetValue(name, out action))
            {
                action = new CGfxSkeletonAction();
                if (action.MakeTPoseActionFromSkeleton(skeleton, name, firstLoad) == false)
                    return null;

                SkeletonActions.Add(name, action);
            }
            return action;
        }
        public void RemoveSkeletonAction(RName name)
        {
            if(SkeletonActions.ContainsKey(name))
            {
                SkeletonActions.Remove(name);
            }
        }
        public Int64 ForgetTime
        {
            get;
            set;
        } = 1000 * 30;//30 seconds
        public Int64 ActiveTime
        {
            get;
            set;
        } = 100;
        public UInt64 TotalSize
        {
            get;
            set;
        }
        public void Tick()
        {
            UInt64 resSize = 0;
            var now = CEngine.Instance.EngineTime;
            var it = SkeletonActions.GetEnumerator();
            while (it.MoveNext())
            {
                var i = it.Current;
                if (i.Value.ResourceState.KeepValid)
                {
                    resSize += i.Value.ResourceState.ResourceSize;
                    continue;
                }

                switch (i.Value.ResourceState.StreamState)
                {
                    case EStreamingState.SS_Valid:
                        {
                            if (now - i.Value.ResourceState.AccessTime > ForgetTime)
                            {
                                i.Value.ResourceState.StreamState = EStreamingState.SS_Killing;
                                i.Value.InvalidateResource();
                                i.Value.ResourceState.StreamState = EStreamingState.SS_Invalid;
                            }
                            else
                            {
                                resSize += i.Value.ResourceState.ResourceSize;
                            }
                        }
                        break;
                    case EStreamingState.SS_Invalid:
                        {
                            //mesh和texture不一样，texture在c++做的updateAccessTime，所以需要检测
                            //但是mesh应该不可能走这里，因为c#内走的PreUse会自动标志状态，而不是只更新时间戳
                            //留下这样的代码就是一种检测而已
                            if (now - i.Value.ResourceState.AccessTime < ActiveTime)
                            {
                                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Mesh", $"Mesh AccesssTime updated,but StreamingState is invalid");
                                //说明销毁资源后，短期内又有人要用他
                                i.Value.ResourceState.StreamState = EStreamingState.SS_Pending;
                                lock (PendingActions)
                                {
                                    PendingActions.Add(i.Value);
                                }
                            }
                        }
                        break;
                }
            }
            it.Dispose();
            TotalSize = resSize;
        }

        public List<CGfxSkeletonAction> PendingActions
        {
            get;
        } = new List<CGfxSkeletonAction>();

        public CGfxSkeletonAction RestoreOneAction()
        {
            if (PendingActions.Count == 0)
                return null;
            CGfxSkeletonAction mesh;
            lock (PendingActions)
            {
                mesh = PendingActions[0];
                PendingActions.RemoveAt(0);
            }
            mesh.ResourceState.StreamState = EStreamingState.SS_Streaming;
            if (false == mesh.RestoreResource())
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Mesh", $"Mesh {mesh} Restore Resource Failed");
                mesh.ResourceState.StreamState = EStreamingState.SS_Invalid;
            }
            else
            {
                mesh.ResourceState.StreamState = EStreamingState.SS_Valid;
            }
            return mesh;
        }
    }
}
