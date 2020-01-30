using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace EngineNS.Bricks.PhysicsCore
{
    public enum EPhysShapeType
    {
        PST_Plane,
        PST_Sphere,
        PST_Box,
        PST_Convex,
        PST_TriangleMesh,
        PST_HeightField,
        PST_Capsule,
        PST_Unknown,
    }
    public enum EPhysShapeFlag
    {
        PST_SIMULATION_SHAPE = (1 << 0),
        PST_SCENE_QUERY_SHAPE = (1 << 1),
        PST_TRIGGER_SHAPE = (1 << 2),
        PST_VISUALIZATION = (1 << 3),
        PST_PARTICLE_DRAIN = (1 << 4)
    };
    public class CPhyShape : CPhyEntity
    {
        public delegate void OnChangeShapeType(EngineNS.Bricks.PhysicsCore.EPhysShapeType shapetype, CPhyShape cps);
        public OnChangeShapeType onChangeShapeType;

        public Vector3 CenterOffset = new Vector3();
        public CPhyShape(NativePointer self, CPhyMaterial materal, RName meshname = null) : base(self)
        {
            mMaterial = materal;
        }
        public CPhyGeomManager.GeomBlob TriGeom;

        Byte mAreaType = 0;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Byte AreaType
        {
            get
            {
                return mAreaType;
            }
            set
            {
                if (value > 0 && value < 16)
                    mAreaType = value;
            }
        }

        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public EPhysShapeType ShapeType
        {
            get
            {
                return SDK_PhyShape_GetType(CoreObject);
            }
            set
            {
                if (SDK_PhyShape_GetType(CoreObject) != value && CanChangeShapeType(value))
                {
                    SDK_PhyShape_SetType(CoreObject, value);
                    if (onChangeShapeType != null)
                        onChangeShapeType(value, this);
                }
            }
        }

        public bool CanChangeShapeType(EPhysShapeType shapetype)
        {
            if (shapetype == EPhysShapeType.PST_TriangleMesh || shapetype == EPhysShapeType.PST_Convex)
            {
                return (ShapeDesc.MeshName != null && ShapeDesc.MeshName.Name.Equals("") == false && ShapeDesc.MeshName.Name.Equals("null") == false);
            }
            return true;

        }

        public EngineNS.Bricks.PhysicsCore.GPhysicsComponent.GPhysicsComponentInitializer.ShapeDesc ShapeDesc
        {
            get;
            set;
        }
        protected CPhyMaterial mMaterial;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public CPhyMaterial Material
        {
            get { return mMaterial; }
        }
        protected CPhyActor mActor;
        [Browsable(false)]
        public CPhyActor Actor
        {
            get { return mActor; }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool AddToActor(CPhyActor actor, ref PhyTransform transform)
        {
            if (actor == mActor)
                return true;

            if (mActor != null)
            {
                mActor.Shapes.Remove(this);
            }
            mActor = actor;
            if (mActor != null)
            {
                //TODO..
                Vector3 center = mActor.HostActor.GetAABBCenter();
                Vector3 pos = mActor.HostActor.Placement.Location;
                CenterOffset = center - pos;
                mActor.Shapes.Add(this);
            }

            if (actor == null)
            {
                if (ShapeDesc != null)
                {
                    ShapeDesc.onChangePlacement -= onChangePlacement;
                    ShapeDesc.onChangeShapeSize -= onChangeShapeSize;
                }
            }

            unsafe
            {
                fixed (PhyTransform* p = &transform)
                {
                    if (mActor != null)
                    {
                        return SDK_PhyShape_AddToActor(CoreObject, actor.CoreObject, p);
                    }
                    else
                    {
                        return SDK_PhyShape_AddToActor(CoreObject, CPhyActor.GetEmptyNativePointer(), p);
                    }
                }
            }
        }
        public void SetLocalPose(ref PhyTransform transform)
        {
            unsafe
            {
                fixed (PhyTransform* p = &transform)
                {
                    SDK_PhyShape_SetLocalPose(CoreObject, p);
                }
            }
        }
        public PhyTransform GetLocalPose()
        {
            PhyTransform transform;
            unsafe
            {
                SDK_PhyShape_GetLocalPose(CoreObject, &transform);
            }
            return transform;
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void RemoveFraomActor()
        {

            if (mActor == null)
                return;

            if (ShapeDesc != null)
            {
                ShapeDesc.onChangePlacement -= onChangePlacement;
                ShapeDesc.onChangeShapeSize -= onChangeShapeSize;
            }

            mActor.Shapes.Remove(this);

            //mActor.Shapes.RemoveWhere(new Predicate<CPhyShape>(
            //  delegate (CPhyShape obj)
            //  {
            //      return obj.ShapeDesc.Equals(this.ShapeDesc);//TODO.. 待优化
            //      //return obj.Equals(this);
            //  }));

            unsafe
            {

                SDK_PhyShape_RemoveFromActor(CoreObject);
            }

            mActor.OnPropertyChanged("Shapes");
        }

        public void onChangePlacement(Vector3 location, Quaternion rotation)
        {
            var relativePose = EngineNS.Bricks.PhysicsCore.PhyTransform.CreateTransform(location, rotation);
            AddToActor(Actor, ref relativePose);
        }

        public void onChangeShapeSize()
        {
            onChangeShapeType(ShapeType, this);
        }
        public void SetFlag(EPhysShapeFlag flag, bool value)
        {
            SDK_PhyShape_SetFlag(CoreObject, flag, vBOOL.FromBoolean(value));
        }
        public void SetQueryFilterData(ref PhyFilterData filterData)
        {
            unsafe
            {
                fixed (PhyFilterData* pPhyFilterData = &filterData)
                {
                    SDK_PhyShape_SetQueryFilterData(CoreObject, pPhyFilterData);
                }
            }
        }
        public vBOOL SDK_PhyShape_IfGetBox(ref float w, ref float h, ref float l)
        {
            unsafe
            {
                fixed (float* pW = &w)
                fixed (float* pH = &h)
                fixed (float* pL = &l)
                {
                    return SDK_PhyShape_IfGetBox(CoreObject, pW, pH, pL);
                }
            }
        }
        public bool IfGetSphere(ref float radius)
        {
            unsafe
            {
                fixed (float* pRadius = &radius)
                {
                    return SDK_PhyShape_IfGetSphere(CoreObject, pRadius);
                }
            }
        }
        public bool IfGetCapsule(ref float radius, ref float halfHeight)
        {
            unsafe
            {
                fixed (float* pRadius = &radius)
                fixed (float* pHalfHeight = &halfHeight)
                {
                    return SDK_PhyShape_IfGetCapsule(CoreObject, pRadius, pHalfHeight);
                }
            }
        }
        public bool IfGetScaling(ref Vector3 scale, ref Quaternion scaleRot)
        {
            unsafe
            {
                fixed (Vector3* pScale = &scale)
                fixed (Quaternion* pScaleRot = &scaleRot)
                {
                    return SDK_PhyShape_IfGetScaling(CoreObject, pScale, pScaleRot);
                }
            }
        }
        public Graphics.Mesh.CGfxMeshPrimitives IfGetTriMesh(CRenderContext rc)
        {
            var ptr = SDK_PhyShape_IfGetTriMesh(CoreObject, rc.CoreObject);
            if (ptr.GetPointer() == IntPtr.Zero)
                return null;
            return new Graphics.Mesh.CGfxMeshPrimitives(ptr);
        }
        public Graphics.Mesh.CGfxMeshPrimitives IfGetConvexMesh(CRenderContext rc)
        {
            var ptr = SDK_PhyShape_IfGetConvexMesh(CoreObject, rc.CoreObject);
            if (ptr.GetPointer() == IntPtr.Zero)
                return null;
            return new Graphics.Mesh.CGfxMeshPrimitives(ptr);
        }

        public int GetTrianglesRemap(int index)
        {
            return SDK_PhyShape_GetTrianglesRemap(CoreObject, index);
        }

        GamePlay.Actor.GActor mDebugActor;
        public GamePlay.Actor.GActor DebugActor
        {
            get
            {
                return mDebugActor;
            }
        }

        EngineNS.Thread.Async.TaskLoader.WaitContext WaitContext = new Thread.Async.TaskLoader.WaitContext();
        public async System.Threading.Tasks.Task AwaitLoadDebugActor()
        {
            await EngineNS.Thread.Async.TaskLoader.Awaitload(WaitContext);
        }

        public async System.Threading.Tasks.Task SetMaterial()
        {
            await AwaitLoadDebugActor();
            await mDebugActor.GetComponent<GamePlay.Component.GMeshComponent>().SetMaterialInstance(
                CEngine.Instance.RenderContext, 0,
                RName.GetRName("editor/icon/icon_3D/material/physical_model.instmtl"), null);
        }

        bool mReady = false;
        public void OnEditorCommitVisual(CCommandList cmd, Graphics.CGfxCamera camera, GamePlay.Actor.GActor hostactor, GamePlay.SceneGraph.CheckVisibleParam param)
        {
            if (hostactor == null || !hostactor.Visible)
                return;

            var rc = CEngine.Instance.RenderContext;
            switch (ShapeType)
            {
                case EPhysShapeType.PST_Convex:
                    {
                        if (mDebugActor == null)
                        {
                            Graphics.Mesh.CGfxMeshPrimitives mp = IfGetConvexMesh(rc);
                            mDebugActor = EngineNS.GamePlay.Actor.GActor.NewMeshActorDirect(CEngine.Instance.MeshManager.CreateMesh(rc, mp));
                            EngineNS.Thread.Async.TaskLoader.Release(ref WaitContext, null);
                            var test = SetMaterial();
                        }
                        mDebugActor.Placement.SetMatrix(ref hostactor.Placement.mDrawTransform);
                    }
                    break;
                case EPhysShapeType.PST_TriangleMesh:
                    {
                        if (mDebugActor == null)
                        {
                            Graphics.Mesh.CGfxMeshPrimitives mp = IfGetTriMesh(rc);
                            CEngine.Instance.MeshManager.CreateMesh(rc, mp);
                            EngineNS.Thread.Async.TaskLoader.Release(ref WaitContext, null);
                            mDebugActor = EngineNS.GamePlay.Actor.GActor.NewMeshActorDirect(CEngine.Instance.MeshManager.CreateMesh(rc, mp));

                            var test = SetMaterial();
                        }
                        mDebugActor.Placement.SetMatrix(ref hostactor.Placement.mDrawTransform);
                    }
                    break;
                case EPhysShapeType.PST_Box:
                    {
                        if (mDebugActor == null && mReady == false)
                        {
                            mReady = true;
                            var test = CreateDefaultBox();

                            test = SetMaterial();
                        }

                        if (mDebugActor != null)
                        {
                            float w = 1.0f;
                            float h = 1.0f;
                            float l = 1.0f;
                            SDK_PhyShape_IfGetBox(ref w, ref h, ref l);
                            mDebugActor.Placement.SetMatrix(ref hostactor.Placement.mDrawTransform);
                            Vector3 scale = Vector3.UnitXYZ;//mDebugActor.Placement.Scale;
                            scale.SetValue(scale.X * w, scale.Y * h, scale.Z * l);
                            mDebugActor.Placement.Scale = scale;
                        }
                    }
                    break;
                case EPhysShapeType.PST_Sphere:
                    {
                        if (mDebugActor == null && mReady == false)
                        {
                            mReady = true;
                            var test = CreateDefaultSphere();
                            test = SetMaterial();
                        }

                        if (mDebugActor != null)
                        {
                            float r = 1.0f;
                            IfGetSphere(ref r);
                            mDebugActor.Placement.SetMatrix(ref hostactor.Placement.mDrawTransform);
                            Vector3 scale = Vector3.UnitXYZ;//mDebugActor.Placement.Scale;
                            scale.SetValue(scale.X * r, scale.Y * r, scale.Z * r);
                            mDebugActor.Placement.Scale = scale;
                        }
                    }
                    break;
                default:
                    break;
            }

            if (mDebugActor != null)
            {
                foreach (var comp in mDebugActor.Components)
                {
                    var meshComp = comp as EngineNS.GamePlay.Component.GVisualComponent;
                    if (meshComp != null)
                        meshComp.CommitVisual(cmd, camera, param);
                }
            }
        }

        public async System.Threading.Tasks.Task CreateDefaultBox()
        {
            mDebugActor = await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(RName.GetRName("editor/basemesh/box_center.gms"));
            EngineNS.Thread.Async.TaskLoader.Release(ref WaitContext, null);
        }

        public async System.Threading.Tasks.Task CreateDefaultSphere()
        {
            mDebugActor = await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(RName.GetRName("editor/basemesh/sphere.gms"));
            EngineNS.Thread.Async.TaskLoader.Release(ref WaitContext, null);
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static EPhysShapeType SDK_PhyShape_GetType(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static void SDK_PhyShape_SetFlag(NativePointer self, EPhysShapeFlag flag, vBOOL value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static unsafe void SDK_PhyShape_SetQueryFilterData(NativePointer self, PhyFilterData* filterData);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static unsafe void SDK_PhyShape_SetLocalPose(NativePointer self, PhyTransform* transform);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static unsafe void SDK_PhyShape_GetLocalPose(NativePointer self, PhyTransform* transform);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static void SDK_PhyShape_SetType(NativePointer self, EPhysShapeType value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static unsafe vBOOL SDK_PhyShape_AddToActor(NativePointer self, CPhyActor.NativePointer actor, PhyTransform* transform);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static unsafe void SDK_PhyShape_RemoveFromActor(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static unsafe vBOOL SDK_PhyShape_IfGetBox(NativePointer self, float* w, float* h, float* l);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static unsafe vBOOL SDK_PhyShape_IfGetSphere(NativePointer self, float* radius);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static unsafe vBOOL SDK_PhyShape_IfGetCapsule(NativePointer self, float* radius, float* halfHeight);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static unsafe vBOOL SDK_PhyShape_IfGetScaling(NativePointer self, Vector3* scale, Quaternion* scaleRot);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static Graphics.Mesh.CGfxMeshPrimitives.NativePointer SDK_PhyShape_IfGetTriMesh(NativePointer self, CRenderContext.NativePointer rc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static Graphics.Mesh.CGfxMeshPrimitives.NativePointer SDK_PhyShape_IfGetConvexMesh(NativePointer self, CRenderContext.NativePointer rc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static int SDK_PhyShape_GetTrianglesRemap(NativePointer self, int index);
        #endregion
    }

    public class CPhyGeomManager
    {
        public class GeomBlob
        {
            public Support.CBlobObject TriMesh;
            public Support.CBlobObject Convex;
            public List<Vector2> UV = new List<Vector2>();
            public List<Vector3> Pos = new List<Vector3>();
            public List<int> Indices = new List<int>();
            public void GetUV(int face, float u, float v, out Vector2 result)
            {
                if (Indices.Count == 0 || UV.Count == 0)
                {
                    result = Vector2.InvUnitXY;
                }
                //face = (Indices.Count / 3 - 1) - face;
                int idx = Indices[face * 3 + 0];
                var v0 = UV[idx];
                idx = Indices[face * 3 + 1];
                var v1 = UV[idx];
                idx = Indices[face * 3 + 2];
                var v2 = UV[idx];

                result = (1 - u - v) * v0 + u * v1 + v * v2;
                //result.X = 1 - result.X;
            }
         
            public Vector3 GetPos(int face, float u, float v)
            {
                if (Indices.Count == 0 || Pos.Count == 0)
                {
                    return Vector3.Zero;
                }
                int idx = Indices[face * 3 + 0];
                var v0 = Pos[idx];
                idx = Indices[face * 3 + 1];
                var v1 = Pos[idx];
                idx = Indices[face * 3 + 2];
                var v2 = Pos[idx];

                return (1 - u - v) * v0 + u * v1 + v * v2;
            }
        }
        public Dictionary<RName, GeomBlob> PhyGeoms
        {
            get;
        } = new Dictionary<RName, GeomBlob>(new RName.EqualityComparer());
        public GeomBlob GetPhyGeomsSync(RName name)
        {
            GeomBlob blob;
            if (PhyGeoms.TryGetValue(name, out blob) == false)
            {
                blob = TryLoadPhysicsGeomBlobSync(name);
                PhyGeoms[name] = blob;
            }
            return blob;
        }
        public async System.Threading.Tasks.Task<GeomBlob> GetPhyGeomsAsync(RName name)
        {
            GeomBlob blob;
            if (PhyGeoms.TryGetValue(name, out blob) == false)
            {
                blob = await TryLoadPhysicsGeomBlob(name);
                PhyGeoms[name] = blob;
            }
            return blob;
        }

        private GeomBlob TryLoadPhysicsGeomBlobSync(RName name)
        {
            var result = new GeomBlob();

            var file = name.Address;
            using (var xnd = IO.XndHolder.SyncLoadXND(file))
            {
                if (xnd == null)
                    return null;

                var attr = xnd.Node.FindAttrib("Convex");
                if (attr != null)
                {
                    attr.BeginRead();
                    attr.Read(out result.Convex);
                    attr.EndRead();
                }

                attr = xnd.Node.FindAttrib("TriMesh");
                if (attr != null)
                {
                    attr.BeginRead();
                    attr.Read(out result.TriMesh);
                    attr.EndRead();
                }

                attr = xnd.Node.FindAttrib("PhyiscsGeomPos");
                if (attr != null)
                {
                    Support.CBlobObject posblob;
                    attr.BeginRead();
                    attr.Read(out posblob);
                    attr.EndRead();
                    Support.CBlobProxy Proxy = new Support.CBlobProxy(posblob);
                    uint count = posblob.Size / (sizeof(float) * 3);
                    Vector3 outvalue;
                    for (int i = 0; i < count; i++)
                    {
                        Proxy.Read(out outvalue);
                        result.Pos.Add(outvalue);
                    }
                }

                attr = xnd.Node.FindAttrib("PhyiscsGeomUV");
                if (attr != null)
                {
                    Support.CBlobObject uvblob;
                    attr.BeginRead();
                    attr.Read(out uvblob);
                    attr.EndRead();
                    Support.CBlobProxy Proxy = new Support.CBlobProxy(uvblob);
                    uint count = uvblob.Size / (sizeof(float) * 2);
                    Vector2 outvalue;
                    for (int i = 0; i < count; i++)
                    {
                        Proxy.Read(out outvalue);
                        result.UV.Add(outvalue);
                    }
                    
                }
                
                attr = xnd.Node.FindAttrib("PhyiscsGeomFace");
                if (attr != null)
                {
                    Support.CBlobObject faceblob;
                    attr.BeginRead();
                    string typeint;
                    attr.Read(out typeint);
                    attr.Read(out faceblob);
                    attr.EndRead();
                    Support.CBlobProxy Proxy = new Support.CBlobProxy(faceblob);
                    if (typeint.Equals("IBT_Int16"))
                    {
                        uint count = faceblob.Size / (sizeof(Int16));
                        Int16 outvalue;
                        for (int i = 0; i < count; i++)
                        {
                            Proxy.Read(out outvalue);
                            result.Indices.Add(outvalue);
                        }
                    }
                    else
                    {
                        uint count = faceblob.Size / (sizeof(Int32));
                        Int32 outvalue;
                        for (int i = 0; i < count; i++)
                        {
                            Proxy.Read(out outvalue);
                            result.Indices.Add(outvalue);
                        }
                    }
                   
                }
            }
            return result;
        }
        private async System.Threading.Tasks.Task<GeomBlob> TryLoadPhysicsGeomBlob(RName name)
        {
            var result = new GeomBlob();
            var ret = await CEngine.Instance.EventPoster.Post(() =>
            {
                var file = name.Address;
                using (var xnd = IO.XndHolder.SyncLoadXND(file))
                {
                    if (xnd == null)
                        return false;

                    var attr = xnd.Node.FindAttrib("Convex");
                    if (attr != null)
                    {
                        attr.BeginRead();
                        attr.Read(out result.Convex);
                        attr.EndRead();
                    }

                    attr = xnd.Node.FindAttrib("TriMesh");
                    if (attr != null)
                    {
                        attr.BeginRead();
                        attr.Read(out result.TriMesh);
                        attr.EndRead();
                    }


                    attr = xnd.Node.FindAttrib("PhyiscsGeomPos");
                    if (attr != null)
                    {
                        Support.CBlobObject posblob;
                        attr.BeginRead();
                        attr.Read(out posblob);
                        attr.EndRead();
                        Support.CBlobProxy Proxy = new Support.CBlobProxy(posblob);
                        uint count = posblob.Size / (sizeof(float) * 3);
                        Vector3 outvalue;
                        for (int i = 0; i < count; i++)
                        {
                            Proxy.Read(out outvalue);
                            result.Pos.Add(outvalue);
                        }
                    }

                    attr = xnd.Node.FindAttrib("PhyiscsGeomUV");
                    if (attr != null)
                    {
                        Support.CBlobObject uvblob;
                        attr.BeginRead();
                        attr.Read(out uvblob);
                        attr.EndRead();
                        Support.CBlobProxy Proxy = new Support.CBlobProxy(uvblob);
                        uint count = uvblob.Size / (sizeof(float) * 2);
                        Vector2 outvalue;
                        for (int i = 0; i < count; i++)
                        {
                            Proxy.Read(out outvalue);
                            result.UV.Add(outvalue);
                        }

                    }
                    
                    attr = xnd.Node.FindAttrib("PhyiscsGeomFace");
                    if (attr != null)
                    {
                        Support.CBlobObject faceblob;
                        attr.BeginRead();
                        string typeint;
                        attr.Read(out typeint);
                        attr.Read(out faceblob);
                        attr.EndRead();
                        Support.CBlobProxy Proxy = new Support.CBlobProxy(faceblob);
                        if (typeint.Equals("IBT_Int16"))
                        {
                            uint count = faceblob.Size / (sizeof(Int16));
                            Int16 outvalue;
                            for (int i = 0; i < count; i++)
                            {
                                Proxy.Read(out outvalue);
                                result.Indices.Add(outvalue);
                            }
                        }
                        else
                        {
                            uint count = faceblob.Size / (sizeof(Int32));
                            Int32 outvalue;
                            for (int i = 0; i < count; i++)
                            {
                                Proxy.Read(out outvalue);
                                result.Indices.Add(outvalue);
                            }
                        }
                    }

                    return true;
                }
            }, Thread.Async.EAsyncTarget.AsyncIO);
            if (ret == false)
                return null;

            return result;
        }
    }
}

namespace EngineNS
{
    partial class CEngine
    {
        public Bricks.PhysicsCore.CPhyGeomManager PhyGeomManager
        {
            get;
        } = new Bricks.PhysicsCore.CPhyGeomManager();
    }
}