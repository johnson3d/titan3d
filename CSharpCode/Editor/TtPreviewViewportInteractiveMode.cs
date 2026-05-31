using EngineNS.EGui.Slate;
using EngineNS.Graphics.Pipeline;

namespace EngineNS.Editor
{
    public class TtPreviewViewportInteractiveMode : TtWorldViewportInteractiveMode
    {
        protected TtPreviewViewport PreviewViewport => Viewport as TtPreviewViewport;

        Vector2 mPreMousePt;
        Vector2 mStartMousePt;

        public override bool OnEvent(in Bricks.Input.Event e)
        {
            var previewViewport = PreviewViewport;
            if (previewViewport == null)
                return true;

            if (!previewViewport.IsDrawing)
                return true;

            if (e.Type == Bricks.Input.EventType.MOUSEBUTTONDOWN)
            {
                mStartMousePt.X = e.MouseButton.X;
                mStartMousePt.Y = e.MouseButton.Y;
                mPreMousePt.X = e.MouseButton.X;
                mPreMousePt.Y = e.MouseButton.Y;
            }

            var viewportPoint = new Vector2(e.MouseMotion.X, e.MouseMotion.Y) + previewViewport.ViewportPos;
            if (previewViewport.PointInOverlappedArea(in viewportPoint))
                return true;

            previewViewport.OnEventAction?.Invoke(in e);

            if (previewViewport.FreezCameraControl)
                return true;

            var keyboards = TtEngine.Instance.InputSystem;
            if (e.Type == Bricks.Input.EventType.MOUSEMOTION)
            {
                if (previewViewport.IsFocused == false)
                {
                    return true;
                }
                if (e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_LEFT)
                {
                    if (keyboards.IsKeyDown(Bricks.Input.Keycode.KEY_LALT))
                    {
                        previewViewport.CameraController.Rotate(ECameraAxis.Up, (e.MouseMotion.X - mPreMousePt.X) * previewViewport.CameraMouseRotSpeed * TtEngine.Instance.ElapsedSecond);
                        previewViewport.CameraController.Rotate(ECameraAxis.Right, (e.MouseMotion.Y - mPreMousePt.Y) * previewViewport.CameraMouseRotSpeed * TtEngine.Instance.ElapsedSecond);
                        previewViewport.ViewportMotion = TtPreviewViewport.EViewportMotion.Rotate;
                    }
                }
                else if (e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_MIDDLE)
                {
                    previewViewport.CameraController.Move(ECameraAxis.Right, (e.MouseMotion.X - mPreMousePt.X) * previewViewport.CameraMoveSpeed * TtEngine.Instance.ElapsedSecond);
                    previewViewport.CameraController.Move(ECameraAxis.Up, (e.MouseMotion.Y - mPreMousePt.Y) * previewViewport.CameraMoveSpeed * TtEngine.Instance.ElapsedSecond);
                    previewViewport.ViewportMotion = TtPreviewViewport.EViewportMotion.Move;
                }
                else if (e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_X1)
                {
                    if (keyboards.IsKeyDown(Bricks.Input.Keycode.KEY_LALT))
                    {
                        previewViewport.CameraController.Move(ECameraAxis.Forward, (e.MouseMotion.Y - mPreMousePt.Y) * 0.03f);
                        previewViewport.ViewportMotion = TtPreviewViewport.EViewportMotion.Zoom;
                    }
                    else
                    {
                        previewViewport.CameraController.Rotate(ECameraAxis.Up, (e.MouseMotion.X - mPreMousePt.X) * previewViewport.CameraMouseRotSpeed * TtEngine.Instance.ElapsedSecond, true);
                        previewViewport.CameraController.Rotate(ECameraAxis.Right, (e.MouseMotion.Y - mPreMousePt.Y) * previewViewport.CameraMouseRotSpeed * TtEngine.Instance.ElapsedSecond, true);
                        previewViewport.ViewportMotion = TtPreviewViewport.EViewportMotion.Move;
                    }
                }

                mPreMousePt.X = e.MouseMotion.X;
                mPreMousePt.Y = e.MouseMotion.Y;
            }
            else if (e.Type == Bricks.Input.EventType.MOUSEWHEEL)
            {
                if (previewViewport.IsViewportSlateFocused == false)
                {
                    return true;
                }
                if (keyboards.IsKeyDown(Bricks.Input.Keycode.KEY_LALT))
                {
                    previewViewport.CameraMoveSpeed += (float)(e.MouseWheel.Y * 0.01f);
                    previewViewport.ViewportMotion = TtPreviewViewport.EViewportMotion.ChangeMoveSpeed;
                }
                else
                {
                    previewViewport.CameraController.Move(ECameraAxis.Forward, e.MouseWheel.Y * previewViewport.CameraMouseWheelSpeed);
                    previewViewport.ViewportMotion = TtPreviewViewport.EViewportMotion.Zoom;
                }
            }
            else if (e.Type == Bricks.Input.EventType.MOUSEBUTTONUP)
            {
                previewViewport.ViewportMotion = TtPreviewViewport.EViewportMotion.None;

                // 左键点击且未拖动轴时，执行 hitproxy 选中
                if (e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_LEFT &&
                    previewViewport.Axis != null &&
                    previewViewport.Axis.CurrentAxisType == GamePlay.TtAxis.enAxisType.Null &&
                    ((new Vector2(e.MouseMotion.X, e.MouseMotion.Y) - mStartMousePt).Length() < 1.0f) &&
                    !previewViewport.UIOperated &&
                    previewViewport.IsMouseIn)
                {
                    previewViewport.ProcessHitproxySelected(e.MouseMotion.X, e.MouseMotion.Y);
                }

                OnMouseUp(in e);
            }
            else if (e.Type == Bricks.Input.EventType.MOUSEBUTTONDOWN)
            {
                OnMouseDown(in e);
            }

            // 驱动坐标轴交互（高亮、拖拽平移/旋转/缩放）
            previewViewport.Axis?.OnEvent(previewViewport, in e);

            return true;
        }

        public override void TickOnFocus()
        {
            var previewViewport = PreviewViewport;
            if (previewViewport == null)
                return;

            float step = (TtEngine.Instance.ElapseTickCountMS * 0.001f) * previewViewport.CameraMoveSpeed;
            var keyboards = TtEngine.Instance.InputSystem;

            // F11: trigger RenderDoc capture
            if (keyboards.IsKeyDown(Bricks.Input.Keycode.KEY_F11))
            {
                if (!mF11WasDown)
                {
                    mF11WasDown = true;
                    TriggerRenderDocCapture();
                }
            }
            else
            {
                mF11WasDown = false;
            }

            if (keyboards.IsKeyDown(Bricks.Input.Keycode.KEY_w))
            {
                previewViewport.CameraController.Move(ECameraAxis.Forward, step, true);
            }
            else if (keyboards.IsKeyDown(Bricks.Input.Keycode.KEY_s))
            {
                previewViewport.CameraController.Move(ECameraAxis.Forward, -step, true);
            }

            if (keyboards.IsKeyDown(Bricks.Input.Keycode.KEY_a))
            {
                previewViewport.CameraController.Move(ECameraAxis.Right, step, true);
            }
            else if (keyboards.IsKeyDown(Bricks.Input.Keycode.KEY_d))
            {
                previewViewport.CameraController.Move(ECameraAxis.Right, -step, true);
            }
        }

        private bool mF11WasDown = false;

        private unsafe void TriggerRenderDocCapture()
        {
            IRenderDocTool.GetInstance().SetGpuDevice(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject);
            TtEngine.Instance.GfxDevice.RenderQueue.CaptureRenderDocFrame = true;
        }
    }
}
