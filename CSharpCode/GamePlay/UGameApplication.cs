using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay
{
    class UGameApplication : Graphics.Pipeline.USlateApplication, ITickable
    {
        public UGameViewportState WorldViewportSlate = null;
        public override EGui.Slate.UWorldViewportSlate GetWorldViewportSlate()
        {
            return WorldViewportSlate;
        }
        public override void Cleanup()
        {
            UEngine.Instance.EndPlayInEditor();
            Graphics.Pipeline.USlateApplication.RootForms.Clear();
            WorldViewportSlate?.Cleanup();
            WorldViewportSlate = null;
            UEngine.Instance?.TickableManager.RemoveTickable(this);
            base.Cleanup();
        }
        public override async System.Threading.Tasks.Task<bool> InitializeApplication(RHI.CRenderContext rc, Type rpType)
        {
            await base.InitializeApplication(rc, rpType);

            var RenderPolicy = Rtti.UTypeDescManager.CreateInstance(rpType) as Graphics.Pipeline.IRenderPolicy;

            WorldViewportSlate = new UGameViewportState(true);
            await WorldViewportSlate.Initialize(this, RenderPolicy, 0, 1);
            WorldViewportSlate.ShowCloseButton = true;

            UEngine.Instance.TickableManager.AddTickable(this);

            var root = UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Current);
            UEngine.Instance.MacrossModule.ReloadAssembly(root + "/net5.0/GameProject.dll");

            await UEngine.Instance.StartPlayInEditor(RName.GetRName("Demo0.mcrs"));

            this.NativeWindow.RegEventProcessor(WorldViewportSlate);
            return true;
        }
        public override void OnResize(float x, float y)
        {
            base.OnResize(x, y);
        }
        protected unsafe override void OnDrawUI()
        {
            if (WorldViewportSlate.Visible)
            {
                if (UEngine.Instance.Config.SupportMultWindows == false)
                {
                    var size = NativeWindow.GetWindowSize();
                    var pos = new Vector2(0);
                    var pivot = new Vector2(0);
                    ImGuiAPI.SetNextWindowPos(ref pos, ImGuiCond_.ImGuiCond_Always, ref pivot);
                    ImGuiAPI.SetNextWindowSize(ref size, ImGuiCond_.ImGuiCond_Always);
                }
                WorldViewportSlate.OnDraw();
            }
        }
        #region Tick
        public void TickLogic(int ellapse)
        {
            WorldViewportSlate.TickLogic(ellapse);
        }
        public void TickRender(int ellapse)
        {
            WorldViewportSlate.TickRender(ellapse);
        }
        public void TickSync(int ellapse)
        {
            WorldViewportSlate.TickSync(ellapse);

            if (WorldViewportSlate.Visible == false)
            {
                var num = ImGuiAPI.PlatformIO_Viewports_Size(ImGuiAPI.GetPlatformIO());
                if (num == 1)
                {//只剩下被特意隐藏的主Viewport了
                    UEngine.Instance.PostQuitMessage();
                }
            }
            OnDrawSlate();
        }
        #endregion
    }
}
