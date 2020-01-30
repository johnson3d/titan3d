using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Graphics.View;

namespace EngineNS.Graphics
{
    public class CGfxRenderPolicy
    {
        public CGfxSceneView BaseSceneView;
        
        public EngineNS.CSwapChain SwapChain;
        public CGfxCamera Camera
        {
            get;
            set;
        }
        protected int CurrDPNumber = 0;
        public int DPLimitter = int.MaxValue;
        public static bool GraphicsDebug = false;
        private CPass mLatestPass;
        public CPass LatestPass
        {
            get { return mLatestPass; }
            set
            {
                if (value == null)
                    return;
                mLatestPass = value;
            }
        }
        public void ClearLatestPass()
        {
            mLatestPass = null;
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [return: Editor.Editor_TypeChangeWithParam(0)]
        public static async System.Threading.Tasks.Task<CGfxRenderPolicy> CreateRenderPolicy(
            [Editor.Editor_TypeFilterAttribute(typeof(CGfxRenderPolicy))]
            Type type, 
            UInt32 width, UInt32 height, CGfxCamera camera, IntPtr WinHandle)
        {
            var result = Rtti.RttiHelper.CreateInstance(type) as CGfxRenderPolicy;
            if (result == null)
                return null;

            if (await result.Init(CEngine.Instance.RenderContext, width, height, camera, WinHandle) == false)
                return null;
            return result;
        }
        public virtual async System.Threading.Tasks.Task<bool> Init(CRenderContext RHICtx, UInt32 width, UInt32 height, CGfxCamera camera, IntPtr WinHandle)
        {
            await Thread.AsyncDummyClass.DummyFunc();
            return false;
        }
        public virtual void Cleanup()
        {

        }
        public virtual void BeforeFrame()
        {

        }
        public virtual void TickLogic(CGfxSceneView view, CRenderContext RHICtx)
        {

        }
        public virtual void TickRender(CSwapChain swapChain)
        {

        }
        public virtual void TickSync()
        {

        }
        public virtual void OnResize(CRenderContext rc, CSwapChain SwapChain, UInt32 width, UInt32 height)
        {

        }
        public int DrawCall
        {
            get;
            set;
        }
        public int DrawTriangle
        {
            get;
            set;
        }
        public UInt32 CmdCount
        {
            get;
            set;
        }
        public delegate void FOnDrawUI(CCommandList cmd, CGfxScreenView view);
        public event FOnDrawUI OnDrawUI;
        public void RiseOnDrawUI(CCommandList cmd, CGfxScreenView view)
        {
            OnDrawUI?.Invoke(cmd, view);
        }

        public virtual void SetEnvMap(RName name)
        {

        }

        public static Profiler.TimeScope ScopeTickLogic = Profiler.TimeScopeManager.GetTimeScope(typeof(CGfxRenderPolicy), nameof(TickLogic));
    }
}
