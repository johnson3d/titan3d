using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace EngineNS.Profiler
{
    public struct TimeScopeHelper : IDisposable//Waiting for C#8 ,ref struct -> Dispose
    {
        public TimeScope mTime;
        public TimeScopeHelper(TimeScope t)
        {
            mTime = t;
            mTime.Begin();
        }
        public void Dispose()
        {
            mTime.End();
        }
        private static void testc()
        {
            var tmp = TimeScopeManager.GetTimeScope("aaaaaaa");
            using (new TimeScopeHelper(tmp))
            {
                //int xxxx = 0;
            }
        }
    }
    public struct TimeScopeWindows : IDisposable
    {
#if PWindow
        public TimeScope mTime;
#endif
        public TimeScopeWindows(TimeScope t)
        {
#if PWindow
            mTime = t;
            mTime.Begin();
#endif
        }
        public void Dispose()
        {
#if PWindow
            mTime.End();
#endif
        }
    }
    public struct TimeClipHelper : IDisposable
    {
#if PWindow
        public TimeClip mClip;
        long mStartTime;
#endif
        public TimeClipHelper(TimeClip clip)
        {
#if PWindow
            mClip = clip;
            mStartTime = Support.Time.HighPrecision_GetTickCount();
#endif
        }
        public void Dispose()
        {
#if PWindow
            var now = Support.Time.HighPrecision_GetTickCount();
            if (now - mStartTime> mClip.MaxElapse)
            {
                mClip.OnTimeOut(now - mStartTime);
            }
#endif
        }
    }
    public class TimeClip
    {
        public TimeClip(string n, long time, Action<TimeClip, long> onTimeout)
        {
            Name = n;
            MaxElapse = time;
            TimeOutAction = onTimeout;
        }
        public TimeClip(string n, long time)
        {
            Name = n;
            MaxElapse = time;
        }
        public TimeClip(long time, Action<TimeClip, long> onTimeout)
        {
            MaxElapse = time;
            TimeOutAction = onTimeout;
        }
        public string Name;
        public long MaxElapse;
        public Action<TimeClip, long> TimeOutAction = (clip, time)=>{System.Diagnostics.Debug.WriteLine($"TimeClip {clip.Name} time out: {time}/{clip.MaxElapse}");};
        public virtual void OnTimeOut(long time)
        {
            TimeOutAction(this, time);
        }
    }
    public class TimeScope
    {
        [Flags]
        public enum EProfileFlag : byte
        {
            Windows = 1,
            Android = (1<<1),
            IOS = (1 << 2),

            FlagsAll = 0xFF,
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
        NativePointer mCoreObject;
        public NativePointer CoreObject
        {
            get { return mCoreObject; }
        }
        public TimeScope(NativePointer self, EProfileFlag flag)
        {
            mCoreObject = self;
            mEnable = (bool)SDK_SampResult_GetEnable(CoreObject);
            Flags = flag;
        }
        EProfileFlag Flags;
        bool mEnable;
        public bool Enable
        {
            get { return mEnable; }
            set
            {
                mEnable = value;
                SDK_SampResult_SetEnable(CoreObject, value);
            }
        }
        Int64 mBeginTime;
        public void Begin()
        {
            //if ((CIPlatform.mProfileType & Flags) == 0)
            //    return;
            if (mEnable == false)
                return;
            mBeginTime = SDK_SampResult_Begin(CoreObject, TimeScopeManager.Instance.CoreObject);
        }
        public void End()
        {
            //if ((CIPlatform.mProfileType & Flags) == 0)
            //    return;
            if (mEnable == false)
                return;
            SDK_SampResult_End(CoreObject, TimeScopeManager.Instance.CoreObject, mBeginTime);            
        }
        public string Name
        {
            get
            {
                return SDK_SampResult_GetName(CoreObject);
            }
        }
        public Int64 AvgTime
        {
            get
            {
                return SDK_SampResult_GetAvgTime(CoreObject);
            }
        }
        public int AvgHit
        {
            get
            {
                return SDK_SampResult_GetAvgHit(CoreObject);
            }
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(CoreObjectBase.ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
        public extern static string SDK_SampResult_GetName(NativePointer self);
        [System.Runtime.InteropServices.DllImport(CoreObjectBase.ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static Int64 SDK_SampResult_Begin(NativePointer self, TimeScopeManager.NativePointer mgr);
        [System.Runtime.InteropServices.DllImport(CoreObjectBase.ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_SampResult_End(NativePointer self, TimeScopeManager.NativePointer mgr, Int64 begin);
        [System.Runtime.InteropServices.DllImport(CoreObjectBase.ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_SampResult_GetEnable(NativePointer self);
        [System.Runtime.InteropServices.DllImport(CoreObjectBase.ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_SampResult_SetEnable(NativePointer self, bool enable);
        [System.Runtime.InteropServices.DllImport(CoreObjectBase.ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static Int64 SDK_SampResult_GetAvgTime(NativePointer self);
        [System.Runtime.InteropServices.DllImport(CoreObjectBase.ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int SDK_SampResult_GetAvgHit(NativePointer self);
        #endregion
    }
    public class TimeScopeManager
    {
        public static TimeScopeManager Instance = new TimeScopeManager();
        Dictionary<string, TimeScope> mCounters = new Dictionary<string, TimeScope>();
        public static TimeScope GetTimeScope(string name, TimeScope.EProfileFlag flags = TimeScope.EProfileFlag.FlagsAll, bool createWhenNotFound = true)
        {
            TimeScope counter;
            if (Instance.mCounters.TryGetValue(name, out counter))
            {
                return counter;
            }
            else if (createWhenNotFound || SDK_v3dSampMgr_PureFindSamp(Instance.CoreObject, name).GetPointer() != IntPtr.Zero)
            {
                counter = new TimeScope(SDK_v3dSampMgr_FindSamp(Instance.CoreObject, name), flags);
                Instance.mCounters[name] = counter;
                return counter;
            }
            return null;
        }
        public static TimeScope GetTimeScope(Type type, string method, TimeScope.EProfileFlag flags = TimeScope.EProfileFlag.FlagsAll)
        {
            return GetTimeScope(type.FullName + "." + method);
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

        NativePointer mCoreObject;
        public NativePointer CoreObject
        {
            get { return mCoreObject; }
        }
        TimeScopeManager()
        {
            mCoreObject = SDK_v3dSampMgr_GetInstance();
        }
        public TimeScope FindTimeScope(string name)
        {
            var co = SDK_v3dSampMgr_PureFindSamp(CoreObject, name);
            return new TimeScope(co, TimeScope.EProfileFlag.FlagsAll);
        }
        public void Tick()
        {
            SDK_v3dSampMgr_Update(CoreObject);
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(CoreObjectBase.ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static NativePointer SDK_v3dSampMgr_GetInstance();
        [System.Runtime.InteropServices.DllImport(CoreObjectBase.ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static TimeScope.NativePointer SDK_v3dSampMgr_FindSamp(NativePointer self, string name);
        [System.Runtime.InteropServices.DllImport(CoreObjectBase.ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static TimeScope.NativePointer SDK_v3dSampMgr_PureFindSamp(NativePointer self, string name);
        [System.Runtime.InteropServices.DllImport(CoreObjectBase.ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_v3dSampMgr_Update(NativePointer self);
        #endregion
    }

    public class PerfViewer
    {
        public class Viewer
        {
            public string Name;
            public Func<string[]> GetValueAction;
            public Func<string[]> GetValueNameAction;
        }
        public Dictionary<string, Viewer> Viewers
        {
            get;
        } = new Dictionary<string, Viewer>();
        public Viewer GetViewer(string name)
        {
            Viewer v;
            if (Viewers.TryGetValue(name, out v))
                return v;
            return null;
        }
        
        public class PfValue_PerfCounter : IO.Serializer.Serializer
        {
            string mName;
            public string Name
            {
                get
                {
                    return mName;
                }
                set
                {
                    mName = value;
                    NameHash = UniHash.APHash(value);
                }
            }
            [EngineNS.Rtti.MetaData]
            public UInt32 NameHash
            {
                get;
                set;
            }
            [EngineNS.Rtti.MetaData]
            public int AvgTime
            {
                get;
                set;
            }
            [EngineNS.Rtti.MetaData]
            public int AvgHit
            {
                get;
                set;
            }
        }

        public class PfValue_Data : IO.Serializer.Serializer
        {
            [EngineNS.Rtti.MetaData]
            public string Name
            {
                get;
                set;
            }
            [EngineNS.Rtti.MetaData]
            public List<string> ValueNames
            {
                get;
                set;
            }
            [EngineNS.Rtti.MetaData]
            public List<string> ValueDatas
            {
                get;
                set;
            }
        }

        public List<string> Scopes = new List<string>();
        public List<Viewer> Datas = new List<Viewer>();
        bool mIsReporting = false;
        public bool IsReporting
        {
            get { return mIsReporting; }
            set
            {
                mIsReporting = value;
                for (int i = 0; i < Scopes.Count; i++)
                {
                    var pf = TimeScopeManager.GetTimeScope(Scopes[i], TimeScope.EProfileFlag.FlagsAll ,true);
                    if (pf == null)
                        continue;
                    pf.Enable = value;
                }
            }
        }
        public void LoadReportLists(RName cfg)
        {
            using (var xmlHolder = EngineNS.IO.XmlHolder.LoadXML(cfg.Address))
            {
                if (xmlHolder == null)
                    return;
                LoadPerformanceTree(xmlHolder.RootNode);
            }
        }
        private void LoadPerformanceTree(EngineNS.IO.XmlNode parentNode)
        {
            foreach (var node in parentNode.GetNodes())
            {
                if (node.GetAttribs().Count > 0)
                {
                    var att = node.FindAttrib("Value");
                    if (att != null)
                    {
                        Scopes.Add(att.Value);
                    }
                    att = node.FindAttrib("Variable");
                    if (att != null)
                    {
                        var viewer = EngineNS.CEngine.Instance.Stat.PViewer.CreateViewer(att.Value);
                        if (viewer == null)
                            continue;
                        viewer.Name = att.Value;
                        Datas.Add(viewer);
                    }
                }
                LoadPerformanceTree(node);
            }
        }
        class ViewerWatch : Viewer
        {
            public ViewerWatch(int num)
            {
                WatchName = new string[num];
                Value = new string[num];
            }
            public string[] WatchName;
            public string[] Value;
        }
        public virtual Viewer CreateViewer(string name)
        {
            switch(name)
            {
                case "GameDraw":
                    {
                        var viewer = new ViewerWatch(5);
                        viewer.GetValueNameAction = () =>
                        {
                            viewer.WatchName[0] = "DrawCall";
                            viewer.WatchName[1] = "DrawTriangle";
                            viewer.WatchName[2] = "ShadowDrawCall";
                            viewer.WatchName[3] = "ShadowDrawTriangle";
                            viewer.WatchName[4] = "CmdCount";
                            return viewer.WatchName;
                        };
                        viewer.GetValueAction = () =>
                        {
                            if (CEngine.Instance.GameInstance != null)
                            {
                                viewer.Value[0] = CEngine.Instance.GameInstance.RenderPolicy?.DrawCall.ToString();
                                viewer.Value[1] = CEngine.Instance.GameInstance.RenderPolicy?.DrawTriangle.ToString();
                                var gamePolicy = CEngine.Instance.GameInstance.RenderPolicy as Graphics.RenderPolicy.CGfxRP_GameMobile;
                                if (gamePolicy != null)
                                {
                                    viewer.Value[2] = gamePolicy.mSSM.DrawCall.ToString();
                                    viewer.Value[3] = gamePolicy.mSSM.DrawTriangle.ToString();
                                }
                                viewer.Value[4] = CEngine.Instance.GameInstance.RenderPolicy?.CmdCount.ToString();
                            }
                            return viewer.Value;
                        };
                        Viewers[name] = viewer;
                        return viewer;
                    }
                case "EditorDraw":
                    {
                        var viewer = new ViewerWatch(5);
                        viewer.GetValueNameAction = () =>
                        {
                            viewer.WatchName[0] = "DrawCall";
                            viewer.WatchName[1] = "DrawTriangle";
                            viewer.WatchName[2] = "ShadowDrawCall";
                            viewer.WatchName[3] = "ShadowDrawTriangle";
                            viewer.WatchName[4] = "CmdCount";
                            return viewer.WatchName;
                        };
                        viewer.GetValueAction = () =>
                        {
#if PWindow
                            if (CEngine.Instance.GameEditorInstance != null)
                            {
                                viewer.Value[0] = CEngine.Instance.GameEditorInstance.RenderPolicy?.DrawCall.ToString();
                                viewer.Value[1] = CEngine.Instance.GameEditorInstance.RenderPolicy?.DrawTriangle.ToString();
                                var editPolicy = CEngine.Instance.GameEditorInstance.RenderPolicy as Graphics.RenderPolicy.CGfxRP_EditorMobile;
                                if (editPolicy != null)
                                {
                                    viewer.Value[2] = editPolicy.mCSM.DrawCall.ToString();
                                    viewer.Value[3] = editPolicy.mCSM.DrawTriangle.ToString();
                                }
                                viewer.Value[4] = CEngine.Instance.GameEditorInstance.RenderPolicy?.CmdCount.ToString();
                            }
#endif
                            return viewer.Value;
                        };
                        Viewers[name] = viewer;
                        return viewer;
                    }
                case "GameObjects":
                    {
                        var viewer = new ViewerWatch(6);
                        viewer.GetValueNameAction = () =>
                        {
                            viewer.WatchName[0] = "GActor";
                            viewer.WatchName[1] = "GComponent";
                            viewer.WatchName[2] = "Texture";
                            viewer.WatchName[3] = "Mesh";
                            viewer.WatchName[4] = "Action";
                            viewer.WatchName[5] = "SceneGraph";
                            return viewer.WatchName;
                        };
                        viewer.GetValueAction = () =>
                        {
                            viewer.Value[0] = GamePlay.Actor.GActor.InstanceNumber.ToString();
                            viewer.Value[1] = GamePlay.Component.GComponent.InstanceNumber.ToString();
                            viewer.Value[2] = CEngine.Instance.TextureManager.Textures.Count.ToString();
                            viewer.Value[3] = CEngine.Instance.MeshPrimitivesManager.MeshPrimitives.Count.ToString();
                            viewer.Value[4] = CEngine.Instance.SkeletonActionManager.SkeletonActions.Count.ToString();
                            viewer.Value[5] = GamePlay.SceneGraph.GSceneGraph.InstanceNumber.ToString();
                            return viewer.Value;
                        };
                        Viewers[name] = viewer;
                        return viewer;
                    }
                case "AsyncNumbers":
                    {
                        var viewer = new ViewerWatch(3);
                        viewer.GetValueNameAction = () =>
                        {
                            viewer.WatchName[0] = "Texture";
                            viewer.WatchName[1] = "Mesh";
                            viewer.WatchName[2] = "SkeletonAction";
                            return viewer.WatchName;
                        };
                        viewer.GetValueAction = () =>
                        {
                            viewer.Value[0] = CEngine.Instance.TextureManager.WaitStreamingCount.ToString();
                            viewer.Value[1] = CEngine.Instance.MeshPrimitivesManager.PendingMeshPrimitives.Count.ToString();
                            viewer.Value[1] = CEngine.Instance.SkeletonActionManager.PendingActions.Count.ToString();
                            return viewer.Value;
                        };
                        Viewers[name] = viewer;
                        return viewer;
                    }
                case "AwaitLogic":
                    {
                        var viewer = new ViewerWatch(3);
                        viewer.GetValueNameAction = () =>
                        {
                            viewer.WatchName[0] = "Priority";
                            viewer.WatchName[1] = "Async";
                            viewer.WatchName[2] = "Continue";
                            return viewer.WatchName;
                        };
                        viewer.GetValueAction = () =>
                        {
                            var trd = CEngine.Instance.ThreadLogic;
                            viewer.Value[0] = trd.PriorityEvents.Count.ToString();
                            viewer.Value[1] = trd.AsyncEvents.Count.ToString();
                            viewer.Value[1] = trd.ContinueEvents.Count.ToString();
                            return viewer.Value;
                        };
                        Viewers[name] = viewer;
                        return viewer;
                    }
                case "AwaitRender":
                    {
                        var viewer = new ViewerWatch(3);
                        viewer.GetValueNameAction = () =>
                        {
                            viewer.WatchName[0] = "Priority";
                            viewer.WatchName[1] = "Async";
                            viewer.WatchName[2] = "Continue";
                            return viewer.WatchName;
                        };
                        viewer.GetValueAction = () =>
                        {
                            var trd = CEngine.Instance.ThreadRHI;
                            viewer.Value[0] = trd.PriorityEvents.Count.ToString();
                            viewer.Value[1] = trd.AsyncEvents.Count.ToString();
                            viewer.Value[1] = trd.ContinueEvents.Count.ToString();
                            return viewer.Value;
                        };
                        Viewers[name] = viewer;
                        return viewer;
                    }
                case "AwaitAsync":
                    {
                        var viewer = new ViewerWatch(3);
                        viewer.GetValueNameAction = () =>
                        {
                            viewer.WatchName[0] = "Priority";
                            viewer.WatchName[1] = "Async";
                            viewer.WatchName[2] = "Continue";
                            return viewer.WatchName;
                        };
                        viewer.GetValueAction = () =>
                        {
                            var trd = CEngine.Instance.ThreadAsync;
                            viewer.Value[0] = trd.PriorityEvents.Count.ToString();
                            viewer.Value[1] = trd.AsyncEvents.Count.ToString();
                            viewer.Value[1] = trd.ContinueEvents.Count.ToString();
                            return viewer.Value;
                        };
                        Viewers[name] = viewer;
                        return viewer;
                    }
                case "AwaitPhysics":
                    {
                        var viewer = new ViewerWatch(3);
                        viewer.GetValueNameAction = () =>
                        {
                            viewer.WatchName[0] = "Priority";
                            viewer.WatchName[1] = "Async";
                            viewer.WatchName[2] = "Continue";
                            return viewer.WatchName;
                        };
                        viewer.GetValueAction = () =>
                        {
                            var trd = CEngine.Instance.ThreadPhysics;
                            viewer.Value[0] = trd.PriorityEvents.Count.ToString();
                            viewer.Value[1] = trd.AsyncEvents.Count.ToString();
                            viewer.Value[1] = trd.ContinueEvents.Count.ToString();
                            return viewer.Value;
                        };
                        Viewers[name] = viewer;
                        return viewer;
                    }
                case "NativeMem":
                    {
                        var viewer = new ViewerWatch(3);
                        viewer.GetValueNameAction = () =>
                        {
                            viewer.WatchName[0] = "Used";
                            viewer.WatchName[1] = "Max";
                            viewer.WatchName[2] = "AllocTimes";
                            return viewer.WatchName;
                        };
                        viewer.GetValueAction = () =>
                        {
                            viewer.Value[0] = SDK_vfxMemory_MemoryUsed().ToString();
                            viewer.Value[1] = SDK_vfxMemory_MemoryMax().ToString();
                            viewer.Value[2] = SDK_vfxMemory_MemoryAllocTimes().ToString();
                            return viewer.Value;
                        };
                        Viewers[name] = viewer;
                        return viewer;
                    }
                case "ManagedMem":
                    {
                        var viewer = new ViewerWatch(3);
                        viewer.GetValueNameAction = () =>
                        {
                            viewer.WatchName[0] = "Total";
                            viewer.WatchName[1] = "CollectTimes1";
                            viewer.WatchName[2] = "CollectTimes2";
                            return viewer.WatchName;
                        };
                        viewer.GetValueAction = () =>
                        {
                            viewer.Value[0] = GC.GetTotalMemory(false).ToString();
                            viewer.Value[1] = GC.CollectionCount(0).ToString();
                            viewer.Value[2] = GC.CollectionCount(1).ToString();
                            return viewer.Value;
                        };
                        Viewers[name] = viewer;
                        return viewer;
                    }
            }
            return null;
        }

        #region SDK
        public const string ModuleNC = CoreObjectBase.ModuleNC;
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static IntPtr SDK_vfxMemory_MemoryUsed();
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static IntPtr SDK_vfxMemory_MemoryMax();
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static IntPtr SDK_vfxMemory_MemoryAllocTimes();
        #endregion
    }
}
