﻿using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay
{
    public class TtGameApplication : TtSlateApplication, ITickable
    {
        public int GetTickOrder()
        {
            return -1;
        }
        public UGameInstance GameInstance;
        //public override EGui.Slate.UWorldViewportSlate GetWorldViewportSlate()
        //{
        //    return GameInstance.WorldViewportSlate;
        //}
        public override void Cleanup()
        {
            TtEngine.Instance.TickableManager.RemoveTickable(this);
            base.Cleanup();
        }
        public override async System.Threading.Tasks.Task<bool> InitializeApplication(NxRHI.UGpuDevice rc, RName rpName)
        {
            await base.InitializeApplication(rc, rpName);
            await TtEngine.Instance.StartPlayInEditor(this, rpName);

            TtEngine.Instance.TickableManager.AddTickable(this);
            return true;
        }
        protected unsafe override void OnDrawUI()
        {
            var worldSlate = GameInstance.WorldViewportSlate;
            worldSlate.IsSetViewportPos = true;            
            worldSlate.GameViewportPos = new Vector2(0);
            worldSlate.GameViewportSize = this.NativeWindow.GetWindowSize();

            TtEngine.RootFormManager.DrawRootForms();
        }
        #region Tick
        public virtual void TickLogic(float ellapse)
        {
            
        }
        public virtual void TickRender(float ellapse)
        {
            
        }
        public void TickBeginFrame(float ellapse)
        {

        }
        public virtual void TickSync(float ellapse)
        {
            //OnDrawSlate();
        }
        #endregion
    }
}
