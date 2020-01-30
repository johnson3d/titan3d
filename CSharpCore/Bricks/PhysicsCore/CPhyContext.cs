using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Bricks.PhysicsCore
{
    public enum EPhyActorType
    {
        PAT_Dynamic,
        PAT_Static,
    }
    public struct PhyTransform
    {
        static PhyTransform mIdentity = CreateTransform(Vector3.Zero, Quaternion.Identity);
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public static PhyTransform Identity
        {
            get { return mIdentity; }
            set { }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public static PhyTransform CreateTransform(Vector3 p, Quaternion q)
        {
            PhyTransform ret;
            ret.P = p;
            ret.Q = q;
            return ret;
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public static PhyTransform CreateTransformFromMatrix(ref Matrix mat)
        {
            PhyTransform ret;
            Vector3 scale;
            mat.Decompose(out scale, out ret.Q, out ret.P);
            return ret;
        }
        public Quaternion Q;
        public Vector3 P;
    }
    public class CPhyContext : CPhyEntity
    {
        public CPhyContext(NativePointer self) : base(self)
        {
            
        }
        public async System.Threading.Tasks.Task<bool> InitContext()
        {
            if (false == SDK_PhyContext_Init(CoreObject, 0xFFFFFFFF))
                return false;
            await Thread.AsyncDummyClass.DummyFunc();
            CEngine.Instance.ThreadPhysics.TickAction = this.Tick;
            return true;
        }
        protected void Tick(Thread.ContextThread ctx)
        {
            for (int i = 0; i < TickScenes.Count; i++)
            {
                var scene = TickScenes[i];
                if (scene == null)
                    continue;
                if (scene.NeedTick == false)
                    continue;
                scene.Tick();
            }
        }
        public List<CPhyScene> TickScenes
        {
            get;
        } = new List<CPhyScene>();
        public bool AddTickScene(CPhyScene scene)
        {
            foreach(var i in TickScenes)
            {
                if (i.SceneId == scene.SceneId)
                    return false;
            }
            TickScenes.Add(scene);
            return true;
        }
        public bool RemoveTickScene(Guid sceneId)
        {
            foreach (var i in TickScenes)
            {
                if (i.SceneId == sceneId)
                {
                    TickScenes.Remove(i);
                    return true;
                }
            }
            return false;
        }
        public CPhySceneDesc CreateSceneDesc()
        {
            var ptr = SDK_PhyContext_CreateSceneDesc(CoreObject);
            if (ptr.GetPointer() == IntPtr.Zero)
                return null;
            return new CPhySceneDesc(ptr);
        }
        public CPhyScene CreateScene(Guid sceneId, CPhySceneDesc desc)
        {
            var ptr = SDK_PhyContext_CreateScene(CoreObject, desc.CoreObject);
            if (ptr.GetPointer() == IntPtr.Zero)
                return null;
            var ret = new CPhyScene(ptr, sceneId);
            ret.Desc = desc;
            return ret;
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public CPhyActor CreateActor(EPhyActorType type, ref PhyTransform pose)
        {
            CPhyActor.NativePointer ptr;
            unsafe
            {
                fixed(PhyTransform* p = &pose)
                {
                    ptr = SDK_PhyContext_CreateActor(CoreObject, type, p);
                }
            }

            var ret = new CPhyActor(ptr, type);
            return ret;
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public CPhyMaterial CreateMaterial(float staticFriction, float dynamicFriction, float restitution)
        {
            var ptr = SDK_PhyContext_CreateMaterial(CoreObject, staticFriction, dynamicFriction, restitution);
            if (ptr.GetPointer() == IntPtr.Zero)
                return null;
            return new CPhyMaterial(ptr);
        }

        public Dictionary<RName, CPhyMaterial> Materials
        {
            get;
        } = new Dictionary<RName, CPhyMaterial>(new RName.EqualityComparer());
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public CPhyMaterial LoadMaterial(RName name)
        {
            CPhyMaterial result;
            if(Materials.TryGetValue(name, out result))
            {
                return result;
            }

            using (IO.XndHolder xnd = IO.XndHolder.SyncLoadXND(name.Address))
            {
                if (xnd == null)
                    return null;
                var attr = xnd.Node.FindAttrib("Params");
                if (attr == null)
                    return null;

                result = CPhyMaterial.LoadXnd(this, attr);
                Materials.Add(name, result);
                return result;
            }
        }
        public Dictionary<RName, CPhyShape> Shapes
        {
            get;
        } = new Dictionary<RName, CPhyShape>(new RName.EqualityComparer());
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public CPhyShape CreateShapePlane(CPhyMaterial material)
        {
            var ptr = SDK_PhyContext_CreateShapePlane(CoreObject, material.CoreObject);
            return new CPhyShape(ptr, material);
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public CPhyShape CreateShapeBox(CPhyMaterial material, float width, float height, float length)
        {
            var ptr = SDK_PhyContext_CreateShapeBox(CoreObject, material.CoreObject, width, height, length);
            return new CPhyShape(ptr, material);
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public CPhyShape CreateShapeSphere(CPhyMaterial material, float radius)
        {
            var ptr = SDK_PhyContext_CreateShapeSphere(CoreObject, material.CoreObject, radius);
            return new CPhyShape(ptr, material);
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public CPhyShape CreateShapeCapsule(CPhyMaterial material, float radius, float halfHeight)
        {
            var ptr = SDK_PhyContext_CreateShapeCapsule(CoreObject, material.CoreObject, radius, halfHeight);
            return new CPhyShape(ptr, material);
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public CPhyShape CreateShapeConvexSync(CPhyMaterial material, RName geomName, Vector3 scale, Quaternion scaleRot)
        {
            var geoms = CEngine.Instance.PhyGeomManager.GetPhyGeomsSync(geomName);
            if (geoms == null)
                return null;
            var blob = geoms.Convex;
            if (blob == null)
                return null;
            unsafe
            {
                var ptr = SDK_PhyContext_CreateShapeConvex(CoreObject, material.CoreObject, blob.CoreObject, &scale, &scaleRot);
                if (ptr.GetPointer() == IntPtr.Zero)
                    return null;
                return new CPhyShape(ptr, material, geomName);
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public async System.Threading.Tasks.Task<CPhyShape> CreateShapeConvex(CPhyMaterial material, RName geomName, Vector3 scale, Quaternion scaleRot)
        {
            var geoms = await CEngine.Instance.PhyGeomManager.GetPhyGeomsAsync(geomName);
            if (geoms == null)
                return null;
            var blob = geoms.Convex;
            if (blob == null)
                return null;
            unsafe
            {
                var ptr = SDK_PhyContext_CreateShapeConvex(CoreObject, material.CoreObject, blob.CoreObject, &scale, &scaleRot);
                if (ptr.GetPointer() == IntPtr.Zero)
                    return null;
                return new CPhyShape(ptr, material, geomName);
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public CPhyShape CreateShapeTriMeshSync(CPhyMaterial material, RName geomName, Vector3 scale, Quaternion scaleRot)
        {
            var geoms = CEngine.Instance.PhyGeomManager.GetPhyGeomsSync(geomName);
            if (geoms == null)
                return null;
            var blob = geoms.TriMesh;
            if (blob == null)
                return null;
            unsafe
            {
                var ptr = SDK_PhyContext_CreateShapeTriMesh(CoreObject, material.CoreObject, blob.CoreObject, &scale, &scaleRot);
                if (ptr.GetPointer() == IntPtr.Zero)
                    return null;
                return new CPhyShape(ptr, material, geomName);
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public async System.Threading.Tasks.Task<CPhyShape> CreateShapeTriMesh(CPhyMaterial material, RName geomName, Vector3 scale, Quaternion scaleRot)
        {
            var geoms = await CEngine.Instance.PhyGeomManager.GetPhyGeomsAsync(geomName);
            if (geoms == null)
                return null;
            var blob = geoms.TriMesh;
            if (blob == null)
                return null;
            unsafe
            {
                var ptr = SDK_PhyContext_CreateShapeTriMesh(CoreObject, material.CoreObject, blob.CoreObject, &scale, &scaleRot);
                if (ptr.GetPointer() == IntPtr.Zero)
                    return null;
                var result = new CPhyShape(ptr, material, geomName);
                result.TriGeom = geoms;
                return result;
            }
        }
        public bool CookConvexMesh(CRenderContext rc, CGeometryMesh mesh, Support.CBlobObject blob)
        {
            return SDK_PhyContext_CookConvexMesh(CoreObject, rc.CoreObject, mesh.CoreObject, blob.CoreObject);
        }
        public bool CookTriMesh(CRenderContext rc, CGeometryMesh mesh, Support.CBlobObject blob, Support.CBlobObject uvblob, Support.CBlobObject faceblob, Support.CBlobObject posblob)
        {
            return SDK_PhyContext_CookTriMesh(CoreObject, rc.CoreObject, mesh.CoreObject, blob.CoreObject, uvblob.CoreObject, faceblob.CoreObject, posblob.CoreObject);
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static vBOOL SDK_PhyContext_Init(NativePointer self, UInt32 featureFlags);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static CPhySceneDesc.NativePointer SDK_PhyContext_CreateSceneDesc(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static unsafe CPhyActor.NativePointer SDK_PhyContext_CreateActor(NativePointer self, EPhyActorType type, PhyTransform* pose);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static unsafe CPhyScene.NativePointer SDK_PhyContext_CreateScene(NativePointer self, CPhySceneDesc.NativePointer desc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static CPhyMaterial.NativePointer SDK_PhyContext_CreateMaterial(NativePointer self, float staticFriction, float dynamicFriction, float restitution);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static CPhyShape.NativePointer SDK_PhyContext_CreateShapePlane(NativePointer self, CPhyMaterial.NativePointer material);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static CPhyShape.NativePointer SDK_PhyContext_CreateShapeBox(NativePointer self, CPhyMaterial.NativePointer material, float width, float height, float length);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static CPhyShape.NativePointer SDK_PhyContext_CreateShapeSphere(NativePointer self, CPhyMaterial.NativePointer material, float radius);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static CPhyShape.NativePointer SDK_PhyContext_CreateShapeCapsule(NativePointer self, CPhyMaterial.NativePointer material, float radius, float halfHeight);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static unsafe CPhyShape.NativePointer SDK_PhyContext_CreateShapeConvex(NativePointer self, CPhyMaterial.NativePointer material, Support.CBlobObject.NativePointer blob, Vector3* scale, Quaternion* scaleRot);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static unsafe CPhyShape.NativePointer SDK_PhyContext_CreateShapeTriMesh(NativePointer self, CPhyMaterial.NativePointer material, Support.CBlobObject.NativePointer blob, Vector3* scale, Quaternion* scaleRot);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static vBOOL SDK_PhyContext_CookConvexMesh(NativePointer self, CRenderContext.NativePointer rc, CGeometryMesh.NativePointer mesh, Support.CBlobObject.NativePointer blob);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static vBOOL SDK_PhyContext_CookTriMesh(NativePointer self, CRenderContext.NativePointer rc, CGeometryMesh.NativePointer mesh, Support.CBlobObject.NativePointer blob, Support.CBlobObject.NativePointer uvblob, Support.CBlobObject.NativePointer faceblob, Support.CBlobObject.NativePointer posblob);
        #endregion
    }

    public class CPhyContextProcessor : CEngineAutoMemberProcessor
    {
        public override async System.Threading.Tasks.Task<object> CreateObject()
        {
            var ptr = CoreObjectBase.NewNativeObjectByNativeName<CPhyContext.NativePointer>("PhyContext");
            var ctx = new CPhyContext(ptr);
            if (await ctx.InitContext() == false)
                return null;
            return ctx;
        }
        public override void Tick(object obj)
        {
            var Services = obj as CPhyContext;
            
        }
        public override void Cleanup(object obj)
        {

        }
    }
}

namespace EngineNS
{
    public partial class CEngine
    {
        [CEngineAutoMemberAttribute(typeof(Bricks.PhysicsCore.CPhyContextProcessor))]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]// | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public Bricks.PhysicsCore.CPhyContext PhyContext
        {
            get;
            set;
        }
    }
}
