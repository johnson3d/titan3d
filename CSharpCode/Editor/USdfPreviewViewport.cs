using System;
using System.Collections.Generic;
using System.Security.Permissions;

namespace EngineNS.Editor
{
    public class USdfPreviewViewport : Graphics.Pipeline.UViewportSlate
    {
        public Editor.Controller.EditorCameraController CameraController = new Editor.Controller.EditorCameraController();
        public USdfPreviewViewport()
        {

        }
        ~USdfPreviewViewport()
        {
            Dispose();
        }
        public override void Dispose()
        {
            PresentWindow?.UnregEventProcessor(this);
            RenderPolicy?.Dispose();
            RenderPolicy = null;
            base.Dispose();
        }
        protected override void OnClientChanged(bool bSizeChanged)
        {
            base.OnClientChanged(bSizeChanged);
            var vpSize = this.ClientSize;
            if (bSizeChanged)
            {
                //RenderPolicy?.OnResize(vpSize.X, vpSize.Y);
            }
        }
        protected override IntPtr GetShowTexture()
        {
            var srv = RenderPolicy?.GetFinalShowRSV();
            if (srv == null)
                return IntPtr.Zero;
            return srv.GetTextureHandle();
        }
        #region CameraControl
        Vector2 mPreMousePt;
        public float CameraMoveSpeed { get; set; } = 1.0f;
        public float CameraMouseWheelSpeed { get; set; } = 1.0f;
        public float CameraRotSpeed = 1.0f;
        public bool FreezCameraControl = false;
        public delegate void Delegate_OnEvent(in Bricks.Input.Event e);
        public Delegate_OnEvent OnEventAction;
        public unsafe override bool OnEvent(in Bricks.Input.Event e)
        {
            if(e.Type == Bricks.Input.EventType.MOUSEBUTTONDOWN)
            {
                mPreMousePt.X = e.MouseButton.X;
                mPreMousePt.Y = e.MouseButton.Y;
            }

            if (this.IsFocused == false)
            {
                return true;
            }

            OnEventAction?.Invoke(in e);

            if (FreezCameraControl)
                return true;
            var keyboards = UEngine.Instance.InputSystem;
            if (e.Type == Bricks.Input.EventType.MOUSEMOTION)
            {
                if (e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_LEFT)
                {
                    if (keyboards.IsKeyDown(Bricks.Input.Keycode.KEY_LALT))
                    {
                        CameraController.Rotate(Graphics.Pipeline.ECameraAxis.Up, (e.MouseMotion.X - mPreMousePt.X) * CameraRotSpeed * UEngine.Instance.ElapsedSecond);
                        CameraController.Rotate(Graphics.Pipeline.ECameraAxis.Right, (e.MouseMotion.Y - mPreMousePt.Y) * CameraRotSpeed * UEngine.Instance.ElapsedSecond);
                        /*if (keyboards.IsKeyDown(Bricks.Input.Keycode.KEY_LCTRL))
                        {
                            UEngine.Instance.GfxDevice.RenderCmdQueue.CaptureRenderDocFrame = true;
                        }*/
                    }
                }
                else if (e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_MIDDLE)
                {
                    CameraController.Move(Graphics.Pipeline.ECameraAxis.Right, (e.MouseMotion.X - mPreMousePt.X) * CameraMoveSpeed * UEngine.Instance.ElapsedSecond);
                    CameraController.Move(Graphics.Pipeline.ECameraAxis.Up, (e.MouseMotion.Y - mPreMousePt.Y) * CameraMoveSpeed * UEngine.Instance.ElapsedSecond);
                }
                else if (e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_X1)
                {
                    if (keyboards.IsKeyDown(Bricks.Input.Keycode.KEY_LALT))
                    {
                        CameraController.Move(Graphics.Pipeline.ECameraAxis.Forward, (e.MouseMotion.Y - mPreMousePt.Y) * 0.03f);
                    }
                    else
                    {
                        CameraController.Rotate(Graphics.Pipeline.ECameraAxis.Up, (e.MouseMotion.X - mPreMousePt.X) * CameraRotSpeed * UEngine.Instance.ElapsedSecond, true);
                        CameraController.Rotate(Graphics.Pipeline.ECameraAxis.Right, (e.MouseMotion.Y - mPreMousePt.Y) * CameraRotSpeed * UEngine.Instance.ElapsedSecond, true);
                    }
                }

                mPreMousePt.X = e.MouseMotion.X;
                mPreMousePt.Y = e.MouseMotion.Y;
            }
            else if (e.Type == Bricks.Input.EventType.MOUSEWHEEL)
            {
                if (keyboards.IsKeyDown(Bricks.Input.Keycode.KEY_LALT))
                {
                    CameraMoveSpeed += (float)(e.MouseWheel.Y * 0.01f);
                }
                else
                {
                    CameraController.Move(Graphics.Pipeline.ECameraAxis.Forward, e.MouseWheel.Y * CameraMouseWheelSpeed);
                }
            }
            return true;
        }
        #endregion
        protected virtual void TickOnFocus()
        {
            float step = (UEngine.Instance.ElapseTickCountMS * 0.001f) * CameraMoveSpeed;
            var keyboards = UEngine.Instance.InputSystem;
            if (keyboards.IsKeyDown(Bricks.Input.Keycode.KEY_w))
            {
                CameraController.Move(Graphics.Pipeline.ECameraAxis.Forward, step, true);
            }
            else if (keyboards.IsKeyDown(Bricks.Input.Keycode.KEY_s))
            {
                CameraController.Move(Graphics.Pipeline.ECameraAxis.Forward, -step, true);
            }

            if (keyboards.IsKeyDown(Bricks.Input.Keycode.KEY_a))
            {
                CameraController.Move(Graphics.Pipeline.ECameraAxis.Right, step, true);
            }
            else if (keyboards.IsKeyDown(Bricks.Input.Keycode.KEY_d))
                {
                CameraController.Move(Graphics.Pipeline.ECameraAxis.Right, -step, true);
            }
        }
        GamePlay.UWorld.UVisParameter mVisParameter = new GamePlay.UWorld.UVisParameter();
        public GamePlay.UWorld.UVisParameter VisParameter
        {
            get => mVisParameter;
        }

        public override void TickLogic(float ellapse)
        {
            if (IsInlitialized == false)
                return;
            
            if (IsDrawing)
            {
                RenderPolicy?.BeginTickLogic(World);

                World.TickLogic(this.RenderPolicy, ellapse);

                if (this.IsFocused)
                {
                    TickOnFocus();
                }

                RenderPolicy?.TickLogic(World, null);

                RenderPolicy?.EndTickLogic(World);

                IsDrawing = false;
            }

            base.TickLogic(ellapse);
        }
        public void TickRender(float ellapse)
        {
            
        }
        public Action AfterTickSync;
        public void TickSync(float ellapse)
        {
            if (IsInlitialized == false)
                return;
            //if (IsDrawing == false)
            //    return;
            RenderPolicy?.TickSync();
            if (AfterTickSync != null)
                AfterTickSync();
        }
    }
}
