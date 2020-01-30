using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Bricks.RecastRuntime
{
    public class CNavMesh : AuxCoreObject<CNavMesh.NativePointer>
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
        public CNavMesh()
        {
            mCoreObject = NewNativeObjectByNativeName<NativePointer>("RcNavMesh");
        }

        public CNavMesh(NativePointer self)
        {
            mCoreObject = self;
        }

        //maxNodes [Limits: 0 < value <= 65535]
        public CNavQuery CreateQuery(int maxNodes)
        {
            var ptr = SDK_RcNavMesh_CreateQuery(CoreObject, maxNodes);
            if (ptr.GetPointer() == IntPtr.Zero)
                return null;
            return new CNavQuery(ptr);
        }
        public Graphics.Mesh.CGfxMeshPrimitives CreateRenderMeshPrimitives(CRenderContext rc)
        {
            var ptr = SDK_RcNavMesh_CreateRenderMesh(CoreObject, rc.CoreObject);
            if (ptr.GetPointer() == IntPtr.Zero)
                return null;
            var result = new Graphics.Mesh.CGfxMeshPrimitives(ptr);
            return result;
        }
        public async System.Threading.Tasks.Task<EngineNS.GamePlay.Actor.GActor> CreateRenderActor(CRenderContext rc, Graphics.Mesh.CGfxMeshPrimitives pri)
        {
            EngineNS.GamePlay.Actor.GActor actor = EngineNS.GamePlay.Actor.GActor.NewMeshActorDirect(EngineNS.CEngine.Instance.MeshManager.CreateMesh(rc, pri));
            await actor.GetComponent<EngineNS.GamePlay.Component.GMeshComponent>().SetMaterialInstance(EngineNS.CEngine.Instance.RenderContext, 0, RName.GetRName("editor/icon/icon_3D/material/pathfinding.instmtl"), null);
            actor.Placement.Location = new EngineNS.Vector3(0.0f, 0.0f, 0.0f);
            actor.Placement.Rotation = EngineNS.Quaternion.RotationAxis(EngineNS.Vector3.UnitY, 0.0f);
            actor.Placement.Scale = new EngineNS.Vector3(1.0f, 1.0f, 1.0f);
            actor.SpecialName = "NavMeshDebugger";
            return actor;
        }
        public int GetTilesWidth( )
        {
            return SDK_RcNavMesh_GetTilesWidth( CoreObject );
        }
        public int GetTilesHeight( )
        {
            return SDK_RcNavMesh_GetTilesHeight( CoreObject );
        }
        public int GetTilesCount()
        {
            return SDK_RcNavMesh_GetTilesCount(CoreObject);
        }
        public bool CheckVaildAt(int tile, int layer)
        {
            return SDK_RcNavMesh_CheckVaildAt(CoreObject, tile, layer);
        }
        public Vector3 GetPositionAt(int tile, int layer)
        {
            return SDK_RcNavMesh_GetPositionAt(CoreObject, tile, layer);
        }
        public Vector3 GetBoundBoxMinAt(int tile)
        {
            return SDK_RcNavMesh_GetBoundBoxMinAt(CoreObject, tile);
        }
        public Vector3 GetBoundBoxMaxAt(int tile)
        {
            return SDK_RcNavMesh_GetBoundBoxMaxAt(CoreObject, tile);
        }
        public bool LoadXnd(IO.XndNode node)
        {
            return SDK_RcNavMesh_LoadXnd(CoreObject, node.CoreObject);
        }
        public void Save2Xnd(IO.XndNode node)
        {
            SDK_RcNavMesh_Save2Xnd(CoreObject, node.CoreObject);
        }
        public void SaveNavMesh(string file)
        {
            var xnd = IO.XndHolder.NewXNDHolder();
            Save2Xnd(xnd.Node);
            IO.XndHolder.SaveXND(file, xnd);
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CNavQuery.NativePointer SDK_RcNavMesh_CreateQuery(NativePointer self, int maxNodes);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_RcNavMesh_LoadXnd(NativePointer self, IO.XndNode.NativePointer node);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_RcNavMesh_Save2Xnd(NativePointer self, IO.XndNode.NativePointer node);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static Graphics.Mesh.CGfxMeshPrimitives.NativePointer SDK_RcNavMesh_CreateRenderMesh(NativePointer self, CRenderContext.NativePointer rc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int SDK_RcNavMesh_GetTilesWidth(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int SDK_RcNavMesh_GetTilesHeight(NativePointer self);      
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int SDK_RcNavMesh_GetTilesCount(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_RcNavMesh_CheckVaildAt(NativePointer self, int tileindex, int layer);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static Vector3 SDK_RcNavMesh_GetPositionAt(NativePointer self, int tileindex, int layer);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static Vector3 SDK_RcNavMesh_GetBoundBoxMinAt(NativePointer self, int tileindex);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static Vector3 SDK_RcNavMesh_GetBoundBoxMaxAt(NativePointer self, int tileindex);
        #endregion
    }
}

namespace EngineNS.GamePlay.Actor
{
    partial class GActor
    {
        public static async System.Threading.Tasks.Task<GActor> NewNavMeshActorAsync(Bricks.RecastRuntime.CNavMesh navMesh)
        {
            var rc = CEngine.Instance.RenderContext;
            var actor = new GamePlay.Actor.GActor();
            actor.ActorId = Guid.NewGuid();
            var placement = new GamePlay.Component.GPlacementComponent();
            actor.Placement = placement;
            var navComp = new Bricks.RecastRuntime.CNavMeshComponent();
            var init = new Bricks.RecastRuntime.CNavMeshComponent.CNavMeshComponentInitializer();
            navComp.NavMesh = navMesh;
            await navComp.SetInitializer(rc, actor, actor, init);
            actor.AddComponent(navComp);

            return actor;
        }
    }
}

namespace EngineNS.GamePlay.SceneGraph
{
    public partial class GSceneGraph
    {
        public Bricks.RecastRuntime.CNavMesh NavMesh = null;

        partial void SaveNavMesh()
        {
            if (NavMesh != null)
            {
                NavMesh.SaveNavMesh(SceneFilename.Address + "/navmesh.dat");
            }
        }
    }
}

namespace EngineNS
{
    partial class CEngine
    {
        public static bool IsRenderNavMesh = false;

        public static bool NavtionDebug = false;

        public static bool IsRenderBoundBox = true;
    }
}

namespace EngineNS.GamePlay
{
    public partial class GWorld
    {
        public Guid NavMeshActorID = new Guid();
    }
}
