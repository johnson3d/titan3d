using EngineNS.NxRHI;
using EngineNS.Graphics.Pipeline;

// =============================================================================
// TtGpuBvhRayCastShading
//
// TtComputeShadingEnv subclass paired with
// enginecontent/Shaders/Bricks/Collision/BVH/GpuBvhTraversal.compute.
// Owned by TtGpuBvh; dispatched from TtGpuBvh.RayCastDispatch. The OnDrawCall
// override is the only place we BindXxx / FindBinder / CreateCBV (per
// CodingGuidelines.md §1.2 — Tick must never bind, see TtGpuBvh's dispatch
// for the corresponding "暂存参数 + SetDrawcallDispatch + PushGpuDraw" half).
// =============================================================================

namespace EngineNS.Bricks.Collision.BVH
{
    public class TtGpuBvhRayCastShading : Graphics.Pipeline.Shader.TtComputeShadingEnv
    {
        public override Vector3ui DispatchArg
        {
            // Compute side declares [numthreads(64, 1, 1)]; matches the
            // round-up TtGpuBvh.RayCastDispatch uses to compute groupX.
            get => new Vector3ui(64, 1, 1);
        }

        public TtGpuBvhRayCastShading()
        {
            CodeName = RName.GetRName("Shaders/Bricks/Collision/BVH/GpuBvhTraversal.compute", RName.ERNameType.Engine);
            MainName = "CS_Main";

            // Permutation update is fire-and-forget here; the actual await
            // happens in TtGpuBvh.InitializeShadingAsync via
            // TtShadingEnv.CreateShadingEnv<T>().
            this.UpdatePermutation().AddWaitTask();
        }

        public override void OnDrawCall(TtComputeDraw drawcall, TtRenderPolicy policy)
        {
            // §3.5 retrieval: TagObject was set to the owning TtGpuBvh in
            // InitializeShadingAsync, immediately after CreateComputeDraw.
            var bvh = drawcall.TagObject as TtGpuBvh;
            if (bvh == null) return;

            // String-overload binds (preferred per ReSTIRGINode.cs comments —
            // avoids FShaderBinder/IXxxView type mismatch). Names mirror the
            // HLSL declarations in GpuBvhTraversal.compute.
            drawcall.BindSrv("BvhNodes", bvh.NodeSrv);
            drawcall.BindSrv("Rays",     bvh.RaySrv);
            drawcall.BindUav("Hits",     bvh.HitUav);

            // CBV: created and seeded on first OnDrawCall via TtGpuBvh's
            // GetOrCreateTraversalCBuffer (which follows §1.1 — full SetValue
            // + MarkDirty + FlushDirty on first creation). Binder lookup goes
            // through FindBinder so we get the field-reflection layout the
            // CBV needs.
            var cbBinder = drawcall.FindBinder(EShaderBindType.SBT_CBV, "cbGpuBvhTraversal");
            if (cbBinder.IsValidPointer)
                drawcall.BindCBV(cbBinder, bvh.GetOrCreateTraversalCBuffer(cbBinder));
        }
    }
}
