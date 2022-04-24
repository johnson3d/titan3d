using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay
{
    public class UGameApplication : Graphics.Pipeline.USlateApplication, ITickable
    {
        public override EGui.Slate.UWorldViewportSlate GetWorldViewportSlate()
        {
            return UEngine.Instance.GameInstance?.McObject.Get().WorldViewportSlate;
        }
        public override void Cleanup()
        {
            Graphics.Pipeline.USlateApplication.ClearRootForms();

            UEngine.Instance.TickableManager.RemoveTickable(this);
            base.Cleanup();
        }
        public override async System.Threading.Tasks.Task<bool> InitializeApplication(RHI.CRenderContext rc, RName rpName, Type rpType)
        {
            await base.InitializeApplication(rc, rpName, rpType);
            await UEngine.Instance.StartPlayInEditor(this, typeof(EngineNS.Graphics.Pipeline.Mobile.UMobileEditorFSPolicy), EngineNS.RName.GetRName("Demo0.mcrs"));

            UEngine.Instance.TickableManager.AddTickable(this);
            return true;
        }
        protected unsafe override void OnDrawUI()
        {
            var worldSlate = UEngine.Instance.GameInstance?.McObject.Get().WorldViewportSlate;
            worldSlate.IsSetViewportPos = true;            
            worldSlate.GameViewportPos = new Vector2(0);
            worldSlate.GameViewportSize = this.NativeWindow.GetWindowSize();

            DrawRootForms();
        }
        #region Tick
        public virtual void TickLogic(int ellapse)
        {
            
        }
        public virtual void TickRender(int ellapse)
        {
            
        }
        public virtual void TickSync(int ellapse)
        {
            OnDrawSlate();
        }
        #endregion
    }
}
