using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Graphics.Mesh
{
    public struct TSurface
    {
        public Vector3 Point1;
        public Vector3 Point2;
        public Vector3 Point3;

        public double a;
        public double b;
        public double c;

        public double S;


    }

    public partial class CGfxMeshPrimitives : AuxCoreObject<CGfxMeshPrimitives.NativePointer>, IO.IResourceFile
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

        public CGfxMeshPrimitives()
        {
            mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::GfxMeshPrimitives");
            if (mCoreObject.Pointer != IntPtr.Zero)
            {
                mResourceState = new CResourceState(SDK_VIUnknown_GetResourceState(mCoreObject.Pointer));
            }
        }
        public CGfxMeshPrimitives(NativePointer self)
        {
            mCoreObject = self;
            if (self.Pointer != IntPtr.Zero)
            {
                mResourceState = new CResourceState(SDK_VIUnknown_GetResourceState(self.Pointer));
                mGeometryMesh = new CGeometryMesh(SDK_GfxMeshPrimitives_GetGeomtryMesh(CoreObject));
                if (mGeometryMesh.CoreObject.Pointer == IntPtr.Zero)
                {
                    return;
                }
                mMdfQueue = new CGfxMdfQueue(CEngine.Instance.RenderContext, SDK_GfxMeshPrimitives_GetMdfQueue(CoreObject));
            }
        }
        internal void UnsafeReInit(NativePointer self)
        {
            Core_Release();
            mCoreObject = self;
            mResourceState.UnsafeReInit(SDK_VIUnknown_GetResourceState(self.Pointer));
            if (mGeometryMesh == null)
            {
                mGeometryMesh = new CGeometryMesh(SDK_GfxMeshPrimitives_GetGeomtryMesh(CoreObject));
            }
            else
            {
                mGeometryMesh.UnsafeReInit(SDK_GfxMeshPrimitives_GetGeomtryMesh(CoreObject));
            }
            if (mMdfQueue == null)
            {
                mMdfQueue = new CGfxMdfQueue(CEngine.Instance.RenderContext, SDK_GfxMeshPrimitives_GetMdfQueue(CoreObject));
            }
            else
            {
                mMdfQueue.UnsafeReInit(CEngine.Instance.RenderContext, SDK_GfxMeshPrimitives_GetMdfQueue(CoreObject));
            }
        }
        private CGfxMeshPrimitives(NativePointer self, CResourceState state)
        {
            mCoreObject = self;
            mResourceState = state;
            Core_AddRef();
        }
        ~CGfxMeshPrimitives()
        {
            Cleanup();
            mGeometryMesh = null;
        }
        public CGfxMeshPrimitives CloneMeshPrimitives()
        {
            var result = new CGfxMeshPrimitives(CoreObject, mResourceState);
            result.mName = mName;
            result.mGeometryMesh = mGeometryMesh;
            result.mMdfQueue = mMdfQueue;
            return result;
        }
        public override string ToString()
        {
            if (Name == null)
                return "NoNameVMS";
            return Name.ToString();
        }
        private CResourceState mResourceState;
        public CResourceState ResourceState
        {
            get { return mResourceState; }
        }

        protected CGeometryMesh mGeometryMesh;
        public CGeometryMesh GeometryMesh
        {
            get { return mGeometryMesh; }
        }
        protected CGfxMdfQueue mMdfQueue;
        public CGfxMdfQueue MdfQueue
        {
            get
            {
                return mMdfQueue;
            }
        }
        public bool Init(CRenderContext rc, RName name, UInt32 atom)
        {
            if (false == SDK_GfxMeshPrimitives_Init(CoreObject, rc.CoreObject, name != null ? name.Name : "", atom))
                return false;
            mName = name;
            mGeometryMesh = new CGeometryMesh(SDK_GfxMeshPrimitives_GetGeomtryMesh(CoreObject));
            mMdfQueue = new CGfxMdfQueue(rc, SDK_GfxMeshPrimitives_GetMdfQueue(CoreObject));

            ResourceState.StreamState = EStreamingState.SS_Valid;
            return true;
        }
        public bool InitFromGeomtryMesh(CRenderContext rc, CGeometryMesh mesh, UInt32 atom, ref BoundingBox aabb)
        {
            unsafe
            {
                fixed(BoundingBox* pAABB = &aabb)
                {
                    if (false == SDK_GfxMeshPrimitives_InitFromGeomtryMesh(CoreObject, rc.CoreObject, mesh.CoreObject, atom, pAABB))
                        return false;
                }
            }
            mGeometryMesh = new CGeometryMesh(SDK_GfxMeshPrimitives_GetGeomtryMesh(CoreObject));
            mMdfQueue = new CGfxMdfQueue(rc, SDK_GfxMeshPrimitives_GetMdfQueue(CoreObject));

            ResourceState.StreamState = EStreamingState.SS_Valid;
            return true;
        }
        public bool LoadMesh(CRenderContext rc, RName name, bool firstLoad)
        {
            using (var xnd = IO.XndHolder.SyncLoadXND(name.Address))
            {
                if (xnd == null)
                    return false;

                if (false == SDK_GfxMeshPrimitives_LoadXnd(CoreObject, rc.CoreObject, name.Name, xnd.Node.CoreObject, firstLoad))
                    return false;

                mName = name;
                mGeometryMesh = new CGeometryMesh(SDK_GfxMeshPrimitives_GetGeomtryMesh(CoreObject));
                mMdfQueue = new CGfxMdfQueue(rc, SDK_GfxMeshPrimitives_GetMdfQueue(CoreObject));

                var modQueueNode = xnd.Node.FindNode("ModStacks");
                if (modQueueNode != null)
                {
                    var mdfs = modQueueNode.GetNodes();
                    foreach (var i in mdfs)
                    {
                        if (i.GetClassId() == CGfxSkinModifier.CoreClassId)//GfxSkinModifier
                        {
                            //var rtti = CEngine.Instance.NativeRttiManager.FindRttiById(i.GetClassId());
                            //CGfxModifier.NativePointer corePtr = new CGfxModifier.NativePointer();
                            //corePtr.Pointer = rtti.CreateInstance("CGfxMeshPrimitives.cs", 117);
                            var skinModifier = new CGfxSkinModifier();
                            skinModifier.LoadXnd(i);
                            mMdfQueue.AddModifier(skinModifier);
                        }
                    }
                }

                if (firstLoad)
                {
                    ResourceState.StreamState = EStreamingState.SS_Valid;
                }
                else
                {
                    ResourceState.StreamState = EStreamingState.SS_Invalid;
                }
                return true;
            }
        }
        public void SaveMesh()
        {
            SaveMesh(Name.Address);
        }
        public void SaveMesh(string absPath)
        {
            var xnd = IO.XndHolder.NewXNDHolder();
            SDK_GfxMeshPrimitives_Save2Xnd(CoreObject, CEngine.Instance.RenderContext.CoreObject, xnd.Node.CoreObject);
            if (mMdfQueue != null)
            {
                var mdfQueueNode = xnd.Node.AddNode("ModStacks", 0, 0);
                foreach (var mdf in mMdfQueue.Modifiers)
                {
                    var mdfNode = mdfQueueNode.AddNode(mdf.Name, (long)mdf.RTTI.ClassId, 0);
                    mdf.Save2Xnd(mdfNode);
                }
            }
            IO.XndHolder.SaveXND(absPath, xnd);
        }
        public bool SetGeomtryMeshStream(CRenderContext rc, EVertexSteamType stream, IntPtr data, UInt32 size, UInt32 stride, UInt32 cpuAccess)
        {
            return (bool)SDK_GfxMeshPrimitives_SetGeomtryMeshStream(CoreObject, rc.CoreObject, stream, data, size, stride, cpuAccess);
        }
        public bool SetGeomtryMeshIndex(CRenderContext rc, IntPtr data, UInt32 size, EIndexBufferType type, UInt32 cpuAccess)
        {
            return (bool)SDK_GfxMeshPrimitives_SetGeomtryMeshIndex(CoreObject, rc.CoreObject, data, size, type, cpuAccess);
        }
        RName mName = null;
        public RName Name
        {
            get
            {
                //return RName.GetRName(SDK_GfxMeshPrimitives_GetName(CoreObject));
                return mName;
            }
        }

        //只有保存的時候调用设值
        public void SetRName(RName name)
        {
            mName = name;
        }

        public UInt32 AtomNumber
        {
            get
            {
                return SDK_GfxMeshPrimitives_GetAtomNumber(CoreObject);
            }
        }
        public CDrawPrimitiveDesc this[UInt32 idx]
        {
            get
            {
                CDrawPrimitiveDesc desc = new CDrawPrimitiveDesc();
                GetAtom(idx, 0, ref desc);
                return desc;
            }
        }
        public bool GetAtom(UInt32 index, UInt32 lod, ref CDrawPrimitiveDesc desc)
        {
            unsafe
            {
                fixed(CDrawPrimitiveDesc* p = &desc)
                {
                    return (bool)SDK_GfxMeshPrimitives_GetAtom(CoreObject, index, lod, p);
                }
            }
        }
        public bool SetAtom(UInt32 index, UInt32 lod, ref CDrawPrimitiveDesc desc)
        {
            unsafe
            {
                fixed (CDrawPrimitiveDesc* p = &desc)
                {
                    return (bool)SDK_GfxMeshPrimitives_SetAtom(CoreObject, index, lod, p);
                }
            }
        }
        public void PushAtomLOD(UInt32 index, ref CDrawPrimitiveDesc desc)
        {
            unsafe
            {
                fixed(CDrawPrimitiveDesc* p = &desc)
                {
                    SDK_GfxMeshPrimitives_PushAtomLOD(CoreObject, index, p);
                }
            }
        }
        public UInt32 GetAtomLOD(UInt32 index)
        {
            return SDK_GfxMeshPrimitives_GetAtomLOD(CoreObject, index);
        }
        public UInt32 GetLodLevel(UInt32 index, float lod)
        {
            return SDK_GfxMeshPrimitives_GetLodLevel(CoreObject, index, lod);
        }
        public BoundingBox AABB
        {
            get
            {
                return SDK_GfxMeshPrimitives_GetAABB(CoreObject);
            }
            set
            {
                unsafe
                {
                    SDK_GfxMeshPrimitives_SetAABB(CoreObject, &value);
                }
            }
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
                        if(isSync)
                        {
                            ResourceState.StreamState = EStreamingState.SS_Streaming;
                            if(false==this.RestoreResource())
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
                            var pendings = CEngine.Instance.MeshPrimitivesManager.PendingMeshPrimitives;
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
                            while(ResourceState.StreamState != EStreamingState.SS_Valid)
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
            if(isSync)
            {
                if(ResourceState.StreamState == EStreamingState.SS_Invalid)
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

        public bool RefreshResource(CRenderContext rc, string name, IO.XndNode node)
        {
            return SDK_GfxMeshPrimitives_RefreshResource(CoreObject, rc.CoreObject, name, node.CoreObject);
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxMeshPrimitives_Init(NativePointer self, CRenderContext.NativePointer rc, string name, UInt32 atom);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe vBOOL SDK_GfxMeshPrimitives_InitFromGeomtryMesh(NativePointer self, CRenderContext.NativePointer rc, CGeometryMesh.NativePointer mesh, UInt32 atom, BoundingBox* box);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxMeshPrimitives_Save2Xnd(NativePointer self, CRenderContext.NativePointer rc, IO.XndNode.NativePointer node);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxMeshPrimitives_LoadXnd(NativePointer self, CRenderContext.NativePointer rc, string name, IO.XndNode.NativePointer node, bool isLoad);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CGeometryMesh.NativePointer SDK_GfxMeshPrimitives_GetGeomtryMesh(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CGfxMdfQueue.NativePointer SDK_GfxMeshPrimitives_GetMdfQueue(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
        public extern static string SDK_GfxMeshPrimitives_GetName(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_GfxMeshPrimitives_GetAtomNumber(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe vBOOL SDK_GfxMeshPrimitives_GetAtom(NativePointer self, UInt32 index, UInt32 lod, CDrawPrimitiveDesc* desc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe vBOOL SDK_GfxMeshPrimitives_SetAtom(NativePointer self, UInt32 index, UInt32 lod, CDrawPrimitiveDesc* desc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_GfxMeshPrimitives_PushAtomLOD(NativePointer self, UInt32 index, CDrawPrimitiveDesc* desc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_GfxMeshPrimitives_GetAtomLOD(NativePointer self, UInt32 index);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_GfxMeshPrimitives_GetLodLevel(NativePointer self, UInt32 index, float lod);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxMeshPrimitives_SetGeomtryMeshStream(NativePointer self, CRenderContext.NativePointer rc, EVertexSteamType stream, IntPtr data, UInt32 size, UInt32 stride, UInt32 cpuAccess);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxMeshPrimitives_SetGeomtryMeshIndex(NativePointer self, CRenderContext.NativePointer rc, IntPtr data, UInt32 size, EIndexBufferType type, UInt32 cpuAccess);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static BoundingBox SDK_GfxMeshPrimitives_GetAABB(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_GfxMeshPrimitives_SetAABB(NativePointer self, BoundingBox* box);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxMeshPrimitives_RefreshResource(NativePointer self, CRenderContext.NativePointer rc, string name, IO.XndNode.NativePointer node);
        #endregion
    }
    public class CGfxMeshPrimitivesManager
    {
        public Dictionary<RName, CGfxMeshPrimitives> MeshPrimitives
        {
            get;
        } = new Dictionary<RName, CGfxMeshPrimitives>(new RName.EqualityComparer());
        public void Cleanup()
        {
            lock (MeshPrimitives)
            {
                foreach (var i in MeshPrimitives)
                {
                    i.Value.Core_Release(true);
                }
                MeshPrimitives.Clear();
            }
        }
        public void RemoveMeshPimitives(RName name)
        {
            lock (MeshPrimitives)
            {
                if (MeshPrimitives.ContainsKey(name))
                {
                    MeshPrimitives.Remove(name);
                }
            }   
        }
        public CGfxMeshPrimitives GetMeshPrimitives(CRenderContext rc, RName name, bool firstLoad = false)
        {
            if (name.IsExtension(CEngineDesc.MeshSourceExtension) == false)
                return null;

            lock (MeshPrimitives)
            {
                CGfxMeshPrimitives mesh;
                if (false == MeshPrimitives.TryGetValue(name, out mesh))
                {
                    mesh = new CGfxMeshPrimitives();
                    if (mesh.LoadMesh(rc, name, firstLoad) == false)
                        return null;

                    MeshPrimitives.Add(name, mesh);
                }
                return mesh.CloneMeshPrimitives();
            }
        }
        public CGfxMeshPrimitives RefreshMeshPrimitives(CRenderContext rc, RName name)
        {
            if (name.IsExtension(CEngineDesc.MeshSourceExtension) == false)
                return null;

            lock (MeshPrimitives)
            {
                CGfxMeshPrimitives mesh;
                if (false == MeshPrimitives.TryGetValue(name, out mesh))
                {
                    mesh = new CGfxMeshPrimitives();
                    if (mesh.LoadMesh(rc, name, true) == false)
                        return null;

                    MeshPrimitives.Add(name, mesh);
                }
                else
                {
                    using (var xnd = IO.XndHolder.SyncLoadXND(name.Address))
                    {
                        mesh.RefreshResource(rc, name.Name, xnd.Node);
                    }
                }
                return mesh.CloneMeshPrimitives();
            }
        }
        public CGfxMeshPrimitives CreateMeshPrimitives(CRenderContext rc, UInt32 atom)
        {
            var mesh = new CGfxMeshPrimitives();

            if (false == mesh.Init(rc, RName.GetRName(null), atom))
                return null;

            return mesh;
        }
        public CGfxMeshPrimitives NewMeshPrimitives(CRenderContext rc, RName name, UInt32 atom)
        {
            if (name.IsExtension(CEngineDesc.MeshSourceExtension) == false)
                return null;
            lock (MeshPrimitives)
            {
                CGfxMeshPrimitives mesh;
                if (true == MeshPrimitives.TryGetValue(name, out mesh))
                {
                    return mesh;
                }
                mesh = new CGfxMeshPrimitives();

                if (false == mesh.Init(rc, name, atom))
                    return null;

                MeshPrimitives.Add(name, mesh);
                return mesh.CloneMeshPrimitives();
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
            lock (MeshPrimitives)
            {
                foreach (var i in MeshPrimitives)
                {
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
                                    lock (PendingMeshPrimitives)
                                    {
                                        PendingMeshPrimitives.Add(i.Value);
                                    }
                                }
                            }
                            break;
                    }
                }
            }
            TotalSize = resSize;
        }

        public List<CGfxMeshPrimitives> PendingMeshPrimitives
        {
            get;
        } = new List<CGfxMeshPrimitives>();

        public CGfxMeshPrimitives RestoreOneMesh()
        {
            if (PendingMeshPrimitives.Count == 0)
                return null;
            CGfxMeshPrimitives mesh;
            lock (PendingMeshPrimitives)
            {
                mesh = PendingMeshPrimitives[0];
                PendingMeshPrimitives.RemoveAt(0);
            }
            if (mesh.Name.Name.Contains("@cook"))
            {
                // cook出来的模型不做Restore操作
                mesh.ResourceState.StreamState = EStreamingState.SS_Valid;
                return mesh;
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

        public void GetReferences(CGfxMeshPrimitives template, List<CGfxMesh> outMeshes, Dictionary<RName, RName> outSRVs = null)
        {

        }
    }

    public class CGfxVertexCloud
    {
        [EngineNS.Editor.Editor_RNameTypeAttribute(EngineNS.Editor.Editor_RNameTypeAttribute.MeshSource)]
        public RName MeshSource
        {
            get;
            set;
        }
        public Vector3[] Positions;
        public Vector4[] Datas;
        public int VertexNum
        {
            get
            {
                if (Positions == null)
                    return 0;
                return Positions.Length;
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 GetPosition(int index)
        {
            if (index < 0 || index >= Positions.Length)
                return Vector3.Zero;
            return Positions[index];
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector4 GetData(int index)
        {
            if (index < 0 || index >= Datas.Length)
                return Vector4.Zero;
            return Datas[index];
        }

        public static TSurface CreateSurface(ref Vector3 p1, ref Vector3 p2, ref Vector3 p3)
        {
            TSurface surf = new TSurface();
            surf.Point1 = p1;
            surf.Point2 = p2;
            surf.Point3 = p3;

            surf.a = Math.Round((surf.Point1 - surf.Point2).Length(), 6);
            surf.b = Math.Round((surf.Point2 - surf.Point3).Length(), 6);
            surf.c = Math.Round((surf.Point3 - surf.Point1).Length(), 6);

            var p = (surf.a + surf.b + surf.c) / 2;
            surf.S = Math.Round(Math.Round(10000 * p * (p - surf.c), 4) * Math.Round(10000 * (p - surf.a) * (p - surf.b), 4), 6);
            return surf;
        }

        public static async System.Threading.Tasks.Task<CGfxVertexCloud> CookFromMesh(CRenderContext rc, CGfxMeshPrimitives mesh, float density = 1.0f)
        {          
            mesh.PreUse(true);
            var result = new CGfxVertexCloud();
            var posVB = mesh.GeometryMesh.GetVertexBuffer(EVertexSteamType.VST_Position);
            if (posVB == null)
                return null;
            var uvVB = mesh.GeometryMesh.GetVertexBuffer(EVertexSteamType.VST_UV);
            if (uvVB == null)
                return null;
            var indexBuff = mesh.GeometryMesh.GetIndexBuffer();
            if (indexBuff == null)
                return null;
            var posBlob = new Support.CBlobObject();
            var uvBlob = new Support.CBlobObject();
            var indexBlob = new Support.CBlobObject();
            await CEngine.Instance.EventPoster.Post(() =>
            {
                posVB.GetBufferData(rc, posBlob);
                uvVB.GetBufferData(rc, uvBlob);
                if (density != 1.0f)
                {
                    indexBuff.GetBufferData(rc, indexBlob);
                }
                
                return true;
            }, Thread.Async.EAsyncTarget.Render);
            //await posVB.GetBufferData(rc, posBlob);
            //await uvVB.GetBufferData(rc, uvBlob);
            if (density == 1.0f)
            {
                unsafe
                {
                    UInt32 num = posBlob.Size / (UInt32)sizeof(Vector3);
                    result.Positions = new Vector3[num];
                    Vector3* src = (Vector3*)posBlob.Data.ToPointer();
                    fixed (Vector3* dst = &result.Positions[0])
                    {
                        for (UInt32 i = 0; i < num; i++)
                        {
                            dst[i] = src[i];
                        }
                    }

                    num = uvBlob.Size / (UInt32)sizeof(Vector2);
                    result.Datas = new Vector4[num];
                    Vector2* src2 = (Vector2*)uvBlob.Data.ToPointer();
                    fixed (Vector4* dst = &result.Datas[0])
                    {
                        for (UInt32 i = 0; i < num; i++)
                        {
                            dst[i].X = src2[i].X;
                            dst[i].Y = src2[i].Y;
                        }
                    }
                }
            }
            else
            {
                List<TSurface> surfaces = new List<TSurface>();
                unsafe
                {
                    UInt32 num = posBlob.Size / (UInt32)sizeof(Vector3);
                    UInt32 numindex = 0;
                    Vector3* src = (Vector3*)posBlob.Data.ToPointer();
                    if (indexBuff.Desc.Type == EIndexBufferType.IBT_Int16)
                    {
                        numindex = indexBlob.Size / sizeof(UInt16);
                        UInt16* indices = (UInt16*)indexBlob.Data.ToPointer();
                        for (int i = 0; i < numindex; i += 3)
                        {
                            surfaces.Add(CreateSurface(ref src[indices[i]], ref src[indices[i + 1]], ref src[indices[i + 2]]));
                        }
                    }
                    else
                    {
                        numindex = indexBlob.Size / sizeof(UInt32);
                        UInt32* indices = (UInt32*)indexBlob.Data.ToPointer();
                        for (int i = 0; i < numindex; i += 3)
                        {
                            surfaces.Add(CreateSurface(ref src[indices[i]], ref src[indices[i + 1]], ref src[indices[i + 2]]));
                        }
                    }
                    
                    //根据面积排序
                    surfaces.Sort((a, b) =>
                    {
                        if (a.S > b.S)
                            return 1;
                        else if (a.S < b.S)
                            return -1;
                        return 0;
                    });

                    //填充点
                    List<Vector3> Positions = new List<Vector3>();
                    for (UInt32 i = 0; i < num; i++)
                    {
                        Positions.Add(src[i]);
                    }
                    
                    int count = (int)((density - 1f) / 0.5f);
                    for (int n = 0; n < count; n++)
                    {
                        int surfacecount = surfaces.Count / 2;
                        for (int i = surfaces.Count - 1; i >= surfacecount; i--)
                        {
                            Positions.Add((surfaces[i].Point1 + surfaces[i].Point2 + surfaces[i].Point3) / 3);

                            var p1 = (surfaces[i].Point1 + surfaces[i].Point2) / 2;
                            var p2 = (surfaces[i].Point2 + surfaces[i].Point3) / 2;
                            var p3 = (surfaces[i].Point3 + surfaces[i].Point1) / 2;
                            var ss = surfaces[i].S;
                            var sss = surfaces[surfaces.Count - 1 - i].S;
                            Positions.Add(p1);
                            Positions.Add(p2);
                            Positions.Add(p3);
                            surfaces.RemoveAt(surfaces.Count - 1);
                            surfaces.Add(CreateSurface(ref p1, ref p2, ref p3));
                           
                        }

                        if (n != count - 1)
                        {
                            surfaces.Sort((a, b) =>
                            {
                                if (a.S > b.S)
                                    return 1;
                                else if (a.S < b.S)
                                    return -1;
                                return 0;
                            });
                        }
                    }
                    
                    result.Positions = new Vector3[Positions.Count];
                    fixed (Vector3* dst = &result.Positions[0])
                    {
                        for (int i = 0; i < Positions.Count; i++)
                        {
                            dst[i] = Positions[i];
                        }
                    }

                    result.Datas = new Vector4[1];

                }
            }
           
            return result;
        }
        public void SaveVertexCloud(RName name)
        {
            var xnd = IO.XndHolder.NewXNDHolder();
            this.Save2Xnd(xnd.Node);
            IO.XndHolder.SaveXND(name.Address, xnd);
        }
        public void Save2Xnd(IO.XndNode node)
        {
            unsafe
            {
                var attr = node.AddAttrib("Desc");

                attr.BeginWrite();
                if(MeshSource!=null)
                    attr.Write(MeshSource.Name);
                else
                    attr.Write("");
                attr.EndWrite();

                attr = node.AddAttrib("Position");
                attr.BeginWrite();
                attr.Write(Positions.Length);
                fixed (Vector3* p = &Positions[0])
                {
                    attr.Write((IntPtr)p, sizeof(Vector3) * Positions.Length);
                }
                attr.EndWrite();

                attr = node.AddAttrib("Data");
                attr.BeginWrite();
                attr.Write(Datas.Length);
                fixed (Vector4* p = &Datas[0])
                {
                    attr.Write((IntPtr)p, sizeof(Vector4) * Datas.Length);
                }
                attr.EndWrite();
            }
        }
        public async System.Threading.Tasks.Task<bool> LoadXnd(IO.XndNode node)
        {
            return await CEngine.Instance.EventPoster.Post(() =>
            {
                unsafe
                {
                    var attr = node.FindAttrib("Desc");

                    if (attr != null)
                    {
                        attr.BeginRead();
                        string name;
                        attr.Read(out name);
                        this.MeshSource = RName.GetRName(name);
                        attr.EndRead();
                    }

                    attr = node.FindAttrib("Position");
                    if (attr == null)
                        return false;
                    attr.BeginRead();
                    int length;
                    attr.Read(out length);
                    if (length == 0)
                        return false;
                    Positions = new Vector3[length];
                    fixed (Vector3* p = &Positions[0])
                    {
                        attr.Read((IntPtr)p, sizeof(Vector3) * Positions.Length);
                    }
                    attr.EndRead();

                    attr = node.FindAttrib("Data");
                    if (attr == null)
                        return false;
                    attr.BeginRead();
                    attr.Read(out length);
                    if (length == 0)
                        return false;
                    Datas = new Vector4[length];
                    fixed (Vector4* p = &Datas[0])
                    {
                        attr.Read((IntPtr)p, sizeof(Vector4) * Datas.Length);
                    }
                    attr.EndRead();
                }
                return true;
            }, Thread.Async.EAsyncTarget.AsyncIO);
        }
    }
    public class CGfxVertexCloudManager
    {
        public Dictionary<RName, CGfxVertexCloud> VertexClouds
        {
            get;
        } = new Dictionary<RName, CGfxVertexCloud>(new RName.EqualityComparer());
        public async System.Threading.Tasks.Task<CGfxVertexCloud> GetVertexCloud(RName name)
        {
            CGfxVertexCloud vc;
            if (VertexClouds.TryGetValue(name, out vc))
                return vc;
            vc = new CGfxVertexCloud();

            var xnd = await IO.XndHolder.LoadXND(name.Address);
            if (xnd == null)
                return null;
            if (false == await vc.LoadXnd(xnd.Node))
                return null;

            CGfxVertexCloud prev;
            if (VertexClouds.TryGetValue(name, out prev))
                return prev;
            VertexClouds.Add(name, vc);
            return vc;
        }
        public void UnmanageVertexCloud(RName name)
        {
            VertexClouds.Remove(name);
        }
    }
}

namespace EngineNS
{
    partial class CEngine
    {
        public Graphics.Mesh.CGfxVertexCloudManager VertexCoudManager
        {
            get;
        } = new Graphics.Mesh.CGfxVertexCloudManager();
    }
}

