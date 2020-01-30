using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Bricks.RecastBuilder
{
    public class RCInputGeom : AuxCoreObject<RCInputGeom.NativePointer>
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
        public RCInputGeom()
        {
            mCoreObject = NewNativeObjectByNativeName<NativePointer>("InputGeom");
        }
 
        public bool LoadMesh(CRenderContext rc, CGeometryMesh mesh, float scale)
        {
            return SDK_InputGeom_LoadMesh(CoreObject, rc.CoreObject, mesh.CoreObject, scale);
        }

        public void AddOffMeshConnections(Vector3 startpos, Vector3 endpos, float radius, EngineNS.Bricks.RecastRuntime.NavLinkProxyComponent.LinkDirection dir)
        {
            if (dir == EngineNS.Bricks.RecastRuntime.NavLinkProxyComponent.LinkDirection.Reverse)
            {
                SDK_InputGeom_CSAddOffMeshConnection(CoreObject, endpos, startpos, radius,
                    (int)EngineNS.Bricks.RecastRuntime.NavLinkProxyComponent.LinkDirection.Forward);
            }
            else
            {
                SDK_InputGeom_CSAddOffMeshConnection(CoreObject, startpos, endpos, radius, (int)dir);
            }
            
        }
        public void CreateConvexVolumes(RecastRuntime.NavMeshBoundVolumeComponent.AreaType areatype, Support.CBlobObject blob, ref Vector3 min, ref Vector3 max)
        {
            SDK_InputGeom_CreateConvexVolumes(CoreObject, areatype, blob.CoreObject, min, max);
        }

        public void DeleteConvexVolumesByArea(RecastRuntime.NavMeshBoundVolumeComponent.AreaType areatype)
        {
            SDK_InputGeom_DeleteConvexVolumesByArea(CoreObject, areatype);
        }

        public void ClearConvexVolumes()
        {
            SDK_InputGeom_ClearConvexVolumes(CoreObject);
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_InputGeom_LoadMesh(NativePointer self, CRenderContext.NativePointer rc, CGeometryMesh.NativePointer mesh, float scale);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_InputGeom_CSAddOffMeshConnection(NativePointer self, Vector3 startpos, Vector3 endpos, float radius, int dir);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_InputGeom_CreateConvexVolumes(NativePointer self, RecastRuntime.NavMeshBoundVolumeComponent.AreaType areatype, Support.CBlobObject.NativePointer blob, Vector3 min, Vector3 max);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_InputGeom_DeleteConvexVolumesByArea(NativePointer self, RecastRuntime.NavMeshBoundVolumeComponent.AreaType areatype);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_InputGeom_ClearConvexVolumes(NativePointer self);
        #endregion
    }
}

namespace EngineNS.GamePlay.Actor
{
    partial class GActor
    {
        public delegate void PlacementChangeDelegate();
        public event PlacementChangeDelegate PlacementChange;
        public bool NeedRefreshNavgation
        {
            get
            {
                return Initializer.ActorBits.IsBit(GActorBits.EBitDefine.NeedRefreshNavgation);
            }
            set
            {
                Initializer.ActorBits.SetBit(GActorBits.EBitDefine.NeedRefreshNavgation, value);
            }
        }
        public bool IsNavgation
        {
            get
            {
                return Initializer.IsNavgation;
            }
            set
            {
                Initializer.IsNavgation = value;
            }
        }
        partial void PlacementChangedCallback()
        {
            if (PlacementChange != null && NeedRefreshNavgation)
                PlacementChange();
        }

        partial void PlacementChangeDefault()
        {
            PlacementChange -= OnDrawMatrixChanged;
            PlacementChange += OnDrawMatrixChanged;
        }
        
    }
}

namespace EngineNS.GamePlay.SceneGraph
{
    public partial class GSceneGraph
    {
        public EngineNS.GamePlay.Actor.GActor CreateNavActor()
        {
            NavAreaActor = new EngineNS.GamePlay.Actor.GActor();
            NavAreaActor.ActorId = Guid.NewGuid();
            EngineNS.GamePlay.Component.GPlacementComponent component = new EngineNS.GamePlay.Component.GPlacementComponent();
            var placement = new GamePlay.Component.GPlacementComponent();
            NavAreaActor.Placement = placement;
            return NavAreaActor;
        }
    }
}
