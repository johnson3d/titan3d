using System;
using System.Collections.Generic;
using System.Security.Permissions;

namespace EngineNS.Editor
{
    public class USdfPreviewViewport : Graphics.Pipeline.TtViewportSlate
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
            if (mVisParameter != null)
            {
                mVisParameter.Reset();
                mVisParameter = null;
            }
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
        protected override ImTextureRef GetShowTexture()
        {
            var srv = RenderPolicy?.GetFinalShowRSV();
            if (srv == null)
                return new ImTextureRef();
            var result = new ImTextureRef();
            result.m__TexID = (ulong)srv.GetTextureHandle();
            return result;
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
            if (CurrentIntercativeMode != null)
            {
                return CurrentIntercativeMode.OnEvent(in e);
            }

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
            var keyboards = TtEngine.Instance.InputSystem;
            if (e.Type == Bricks.Input.EventType.MOUSEMOTION)
            {
                if (e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_LEFT)
                {
                    if (keyboards.IsKeyDown(Bricks.Input.Keycode.KEY_LALT))
                    {
                        CameraController.Rotate(Graphics.Pipeline.ECameraAxis.Up, (e.MouseMotion.X - mPreMousePt.X) * CameraRotSpeed * TtEngine.Instance.ElapsedSecond);
                        CameraController.Rotate(Graphics.Pipeline.ECameraAxis.Right, (e.MouseMotion.Y - mPreMousePt.Y) * CameraRotSpeed * TtEngine.Instance.ElapsedSecond);
                        /*if (keyboards.IsKeyDown(Bricks.Input.Keycode.KEY_LCTRL))
                        {
                            TtEngine.Instance.GfxDevice.RenderCmdQueue.CaptureRenderDocFrame = true;
                        }*/
                    }
                }
                else if (e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_MIDDLE)
                {
                    CameraController.Move(Graphics.Pipeline.ECameraAxis.Right, (e.MouseMotion.X - mPreMousePt.X) * CameraMoveSpeed * TtEngine.Instance.ElapsedSecond);
                    CameraController.Move(Graphics.Pipeline.ECameraAxis.Up, (e.MouseMotion.Y - mPreMousePt.Y) * CameraMoveSpeed * TtEngine.Instance.ElapsedSecond);
                }
                else if (e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_X1)
                {
                    if (keyboards.IsKeyDown(Bricks.Input.Keycode.KEY_LALT))
                    {
                        CameraController.Move(Graphics.Pipeline.ECameraAxis.Forward, (e.MouseMotion.Y - mPreMousePt.Y) * 0.03f);
                    }
                    else
                    {
                        CameraController.Rotate(Graphics.Pipeline.ECameraAxis.Up, (e.MouseMotion.X - mPreMousePt.X) * CameraRotSpeed * TtEngine.Instance.ElapsedSecond, true);
                        CameraController.Rotate(Graphics.Pipeline.ECameraAxis.Right, (e.MouseMotion.Y - mPreMousePt.Y) * CameraRotSpeed * TtEngine.Instance.ElapsedSecond, true);
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
        protected override void TickOnFocus()
        {
            float step = (TtEngine.Instance.ElapseTickCountMS * 0.001f) * CameraMoveSpeed;
            var keyboards = TtEngine.Instance.InputSystem;
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
        GamePlay.TtWorld.TtVisParameter mVisParameter = new GamePlay.TtWorld.TtVisParameter();
        public GamePlay.TtWorld.TtVisParameter VisParameter
        {
            get => mVisParameter;
        }

        public override unsafe void OnDrawShowTexture()
        {
            var showTexture = GetShowTexture();
            if (showTexture.m__TexID != 0)
            {
                var drawlist = ImGuiAPI.GetWindowDrawList();
                var uv1 = Vector2.Zero;
                var uv2 = Vector2.One;
                var min = ImGuiAPI.GetWindowContentRegionMin();
                var max = ImGuiAPI.GetWindowContentRegionMax();
                min = min + WindowPos;
                max = max + WindowPos;
                drawlist.AddImage(showTexture, in min, in max, in uv1, in uv2, 0x01FFFFFF);// 0xFFFFFFFF);abgr
            }
        }
        public void TickRender(float ellapse)
        {
            
        }
        public Action AfterTickSync;
        public override void TickSync(float ellapse)
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
