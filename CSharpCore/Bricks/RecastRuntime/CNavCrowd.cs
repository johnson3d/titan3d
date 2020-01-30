using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Bricks.RecastRuntime
{
    public class CNavCrowd : AuxCoreObject<CNavCrowd.NativePointer>
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

        public CNavCrowd()
        {
            mCoreObject = NewNativeObjectByNativeName<NativePointer>("RcNavCrowd");
        }
   
        public CNavCrowd(NativePointer self)
        {
            mCoreObject = self;
        }
 
        public bool Init(CNavQuery navquery, CNavMesh nav, float radius = 1.0f)
        {
            return SDK_RcNavCrowd_Init(mCoreObject, navquery.CoreObject, nav.CoreObject, radius);
        }

        /// flags -->Crowd agent update flags.
        public int AddAgent(Vector3 pos, float radius, float height, int flags = 0xff, float m_obstacleAvoidanceType = 3.0f, float m_separationWeight = 2.0f)
        {
            unsafe
            {
                return SDK_RcNavCrowd_AddAgent(mCoreObject, &pos, radius, height, flags, m_obstacleAvoidanceType, m_separationWeight);
            }
            
        }

        public void RemoveAgent(int id)
        {
            SDK_RcNavCrowd_RemoveAgent(mCoreObject, id);
        }

        public void SetMoveTarget(int id, Vector3 pos)
        {
            unsafe
            {
                SDK_RcNavCrowd_SetMoveTarget(mCoreObject, id, &pos);
            }
           
        }

        public void UpdateTick(float t)
        {
            SDK_RcNavCrowd_UpdateTick(mCoreObject, t);
        }

        public Vector3 GetPosition(int id)
        {
            return SDK_RcNavCrowd_GetPosition(mCoreObject, id);
        }

        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static bool SDK_RcNavCrowd_Init(NativePointer self, CNavQuery.NativePointer navquery, CNavMesh.NativePointer nav, float radius);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe int SDK_RcNavCrowd_AddAgent(NativePointer self, Vector3* pos, float radius, float height, int flags, float m_obstacleAvoidanceType, float m_separationWeight);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_RcNavCrowd_RemoveAgent(NativePointer self, int id);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_RcNavCrowd_SetMoveTarget(NativePointer self, int id, Vector3* pos);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_RcNavCrowd_UpdateTick(NativePointer self, float t);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static Vector3 SDK_RcNavCrowd_GetPosition(NativePointer self, int id);
        #endregion
    }
}

namespace EngineNS.GamePlay.SceneGraph
{
    public partial class GSceneGraph
    {
        public Bricks.RecastRuntime.CNavCrowd NavCrowd = null;

        public void CreateNavCrowd(Bricks.RecastRuntime.CNavQuery navquery, Bricks.RecastRuntime.CNavMesh nav, float radius = 1.0f)
        {
            NavCrowd = new Bricks.RecastRuntime.CNavCrowd();
            if (NavCrowd.Init(navquery, nav, radius) == false)
                NavCrowd = null;
        }

        partial void UpdateCrowd(float t)
        {
            if (NavCrowd != null)
            {
                NavCrowd.UpdateTick(t);
                // Test..
                //foreach (var i in Actors.Values)
                //{
                //    Actor.GActor actor;
                //    if (i.TryGetTarget(out actor) == false)
                //    {
                //        continue;
                //    }
                //    if (actor.CrowdAgent != -1)
                //    {
                //        actor.Placement.Location = NavCrowd.GetPosition(actor.CrowdAgent);
                //    }
                //}
            }
        }

    }
}

namespace EngineNS.GamePlay.Actor
{
    partial class GActor
    {
        bool mActiveCrowd = false;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool ActiveCrowdAgent
        {
            get { return mActiveCrowd; }
            set
            {
                if (mActiveCrowd == value)
                    return;
                if (value == false)
                {
                    if (CrowdAgent != -1 && Scene != null && Scene.NavCrowd != null)
                    {
                        Scene.NavCrowd.RemoveAgent(CrowdAgent);
                        CrowdAgent = -1;
                    }
                }
                else
                {
                    if (Scene != null && Scene.NavCrowd != null)
                    {
                        Vector3 pos = Placement.Location;
                        float xx = Math.Abs(LocalBoundingBox.Maximum.X - LocalBoundingBox.Minimum.X) * Placement.Scale.X;
                        float yy = Math.Abs(LocalBoundingBox.Maximum.Y - LocalBoundingBox.Minimum.Y) * Placement.Scale.Y;
                        float zz = Math.Abs(LocalBoundingBox.Maximum.Z - LocalBoundingBox.Minimum.Z) * Placement.Scale.Z;
                        CrowdAgent = Scene.NavCrowd.AddAgent(pos, Math.Max(xx, zz) * 0.5f, yy);
                    }
                }

                mActiveCrowd = value;
            }
        }
    
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public int CrowdAgent
        {
            set;
            get;
        } = -1;
    }
}



