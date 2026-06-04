using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Mobile
{
    public class TtMobileFSPolicy : TtRenderPolicy
    {
        #region Feature On/Off
        protected bool mDisableAO;
        public virtual bool DisableAO
        {
            get => mDisableAO;
            set
            {
                mDisableAO = value;
                BasePassNode.mOpaqueShading.DisableAO.SetValue(value);
                BasePassNode.mOpaqueShading.UpdatePermutation().AddWaitTask();
            }
        }
        bool mDisablePointLight;
        public bool DisablePointLight
        {
            get
            {
                return mDisablePointLight;
            }
            set
            {
                mDisablePointLight = value;
                var shading = BasePassNode.mOpaqueShading;
                shading?.DisablePointLights.SetValue(value);
                if (shading != null)
                    shading.UpdatePermutation().AddWaitTask();
            }
        }
        #endregion

        public TtMobileOpaqueNode BasePassNode = new TtMobileOpaqueNode();
        public Shadow.TtShadowMapNode mShadowMapNode = new Shadow.TtShadowMapNode();        
        public override NxRHI.TtSrView GetFinalShowRSV()
        {
            return this.AttachmentCache.FindAttachement(BasePassNode.GBuffers.RenderTargets[0].Attachement.AttachmentName).Srv;
        }
        public override void OnResize(float x, float y)
        {
            base.OnResize(x, y);

            BasePassNode.OnResize(this, x, y);
        }
        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref BasePassNode);
            base.Dispose();
        }
        public override void OnDrawCall(NxRHI.ICommandList cmd, NxRHI.TtGraphicDraw drawcall, Mesh.TtRenderMesh.TtAtom atom)
        {
            base.OnDrawCall(cmd, drawcall, atom);
            BasePassNode.mOpaqueShading.OnDrawCall(cmd, drawcall, this, atom);
        }
        public unsafe override void Tick(GamePlay.TtWorld world, Action<TtRenderGraphNode, TtRenderGraphPin, TtAttachBuffer> onRemove)
        {
            var cmdlist = TtEngine.Instance.GfxDevice.RenderContext.CmdListManager.GetCmdList();
            BasePassNode.Tick(world, this, cmdlist, true);
            this.CommitCommandList(cmdlist, "Frame");
        }
        public unsafe override void TickSync()
        {
            BasePassNode.TickSync(this);
        }
    }
}
