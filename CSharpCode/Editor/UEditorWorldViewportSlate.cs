﻿using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;

namespace EngineNS.Editor
{
    [Obsolete("已废弃")]
    public class UEditorWorldViewportSlate : EGui.Slate.TtWorldViewportSlate, IRootForm
    {
        public UEditorWorldViewportSlate(bool regRoot)
        {
            if (regRoot)
                TtEngine.RootFormManager.RegRootForm(this);
            CameraController = new Controller.EditorCameraController();
            Title = "WorldEditor";
        }
        public override void OnHitproxySelected(Graphics.Pipeline.IProxiable proxy)
        {
            base.OnHitproxySelected(proxy);

            if (proxy == null)
            {
                this.ShowBoundVolumes(true, false, null);
                return;
            }
            var node = proxy as GamePlay.Scene.TtNode;
            if (node != null)
            {
                this.ShowBoundVolumes(true, true, node);
            }

            var app = TtEngine.Instance.GfxDevice.SlateApplication as Editor.UMainEditorApplication;
            if (app != null)
            {
                app.mMainInspector.PropertyGrid.Target = proxy;
            }
        }
        protected override void OnMouseUp(in Bricks.Input.Event e)
        {
            //test
            if (e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_LEFT)
            {
                var root = World.Root.FindFirstChild("DrawMeshNode");
                if (root == null)
                    return;
                Vector3 ray = new Vector3();
                float sw = Viewport.Width;
                float sh = Viewport.Height;
                var mouse = Window2Viewport(new Vector2((float)e.MouseButton.X, (float)e.MouseButton.Y));
                if (0 != CameraController.Camera.mCoreObject.GetPickRay(ref ray, mouse.X, mouse.Y, sw, sh))
                {
                    var pos = CameraController.Camera.mCoreObject.GetPosition();
                    var end = pos + ray * 1000.0f;
                    VHitResult hit = new VHitResult();
                    if (root.LineCheck(in pos, in end, ref hit))
                    {
                        Console.WriteLine(hit.Distance);
                    }
                }
            }
        }
    }
}
