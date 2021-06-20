using System;
using System.Collections.Generic;
using SDL2;

namespace EngineNS.Editor
{
    public class UEditorWorldViewportSlate : EGui.Slate.UWorldViewportSlate
    {
        public UEditorWorldViewportSlate(bool regRoot)
            : base(regRoot)
        {
            CameraController = new Controller.EditorCameraController();
        }
        protected override void OnHitproxySelected(Graphics.Pipeline.IProxiable proxy)
        {
            var edtorPolicy = this.RenderPolicy as Graphics.Pipeline.Mobile.UMobileEditorFSPolicy;
            if (edtorPolicy != null)
            {
                if (proxy == null)
                {
                    edtorPolicy.PickedProxiableManager.ClearSelected();
                }
                else
                {
                    edtorPolicy.PickedProxiableManager.Selected(proxy);
                }
            }
            if (proxy == null)
            {
                this.ShowBoundVolumes(false, null);
                return;
            }
            var node = proxy as GamePlay.Scene.UNode;
            if (node != null)
            {
                this.ShowBoundVolumes(true, node);
            }
        }
    }
}
