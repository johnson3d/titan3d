using EngineNS.GamePlay;
using EngineNS.Graphics.Pipeline;

namespace EngineNS.EGui.Slate
{
    public class TtWorldViewportInteractiveMode : TtInteractiveMode
    {
        protected TtWorldViewportSlate WorldViewport => Viewport as TtWorldViewportSlate;

        Vector2 mPreMousePt;
        Vector2 mStartMousePt;

        public override bool OnEvent(in Bricks.Input.Event e)
        {
            var worldViewport = WorldViewport;
            if (worldViewport == null)
                return true;

            var keyboards = TtEngine.Instance.InputSystem;
            if (e.Type == Bricks.Input.EventType.MOUSEBUTTONDOWN)
            {
                mStartMousePt = new Vector2(e.MouseMotion.X, e.MouseMotion.Y);
            }
            else if (e.Type == Bricks.Input.EventType.MOUSEMOTION)
            {
                // 多个世界视口(如同时打开两个场景编辑器)会同时收到同一份全局鼠标事件。
                // 若不按窗口焦点门控, 隐藏在后面的非激活视口的相机也会跟着一起动。
                // 与 TtPreviewViewportInteractiveMode 的 MOUSEMOTION 处理保持一致: 非聚焦视口直接跳过相机操作。
                if (worldViewport.IsFocused == false)
                    return true;

                if (e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_LEFT)
                {
                    if (keyboards.IsKeyDown(Bricks.Input.Keycode.KEY_LALT))
                    {
                        worldViewport.CameraController.Rotate(ECameraAxis.Up, (e.MouseMotion.X - mPreMousePt.X) * worldViewport.CameraMouseRotSpeed * TtEngine.Instance.ElapsedSecond);
                        worldViewport.CameraController.Rotate(ECameraAxis.Right, (e.MouseMotion.Y - mPreMousePt.Y) * worldViewport.CameraMouseRotSpeed * TtEngine.Instance.ElapsedSecond);
                    }
                }
                else if (e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_MIDDLE)
                {
                    worldViewport.CameraController.Move(ECameraAxis.Right, (e.MouseMotion.X - mPreMousePt.X) * worldViewport.CameraMoveSpeed * TtEngine.Instance.ElapsedSecond, true);
                    worldViewport.CameraController.Move(ECameraAxis.Up, (e.MouseMotion.Y - mPreMousePt.Y) * -worldViewport.CameraMoveSpeed * TtEngine.Instance.ElapsedSecond, true);
                }
                else if (e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_X1)
                {
                    if (keyboards.IsKeyDown(Bricks.Input.Keycode.KEY_LALT))
                    {
                        worldViewport.CameraController.Move(ECameraAxis.Forward, (e.MouseMotion.Y - mPreMousePt.Y) * 0.03f, false);
                    }
                    else
                    {
                        worldViewport.CameraController.Rotate(ECameraAxis.Up, (e.MouseMotion.X - mPreMousePt.X) * worldViewport.CameraMouseRotSpeed * TtEngine.Instance.ElapsedSecond, true);
                        worldViewport.CameraController.Rotate(ECameraAxis.Right, (e.MouseMotion.Y - mPreMousePt.Y) * worldViewport.CameraMouseRotSpeed * TtEngine.Instance.ElapsedSecond, true);
                    }
                }

                mPreMousePt.X = e.MouseMotion.X;
                mPreMousePt.Y = e.MouseMotion.Y;
            }
            else if (e.Type == Bricks.Input.EventType.MOUSEWHEEL)
            {
                if (worldViewport.IsViewportSlateFocused)
                {
                    if (keyboards.IsKeyDown(Bricks.Input.Keycode.KEY_LALT))
                    {
                        worldViewport.CameraMoveSpeed += (float)(e.MouseWheel.Y * 0.01f);
                    }
                    else
                    {
                        worldViewport.CameraController.Move(ECameraAxis.Forward, e.MouseWheel.Y * worldViewport.CameraMouseWheelSpeed, worldViewport.CameralWheelMoveWithLookAt);
                    }
                }
            }
            else if (e.Type == Bricks.Input.EventType.MOUSEBUTTONUP)
            {
                var viewportPoint = new Vector2(e.MouseMotion.X, e.MouseMotion.Y) + worldViewport.ViewportPos;
                if (e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_LEFT && worldViewport.Axis != null &&
                    worldViewport.Axis.CurrentAxisType == TtAxis.enAxisType.Null &&
                    ((new Vector2(e.MouseMotion.X, e.MouseMotion.Y) - mStartMousePt).Length() < 1.0f) &&
                    !worldViewport.UIOperated &&
                    worldViewport.IsMouseIn &&
                    !worldViewport.PointInOverlappedArea(in viewportPoint))
                {
                    if (e.MouseButton.Clicks >= 2)
                        worldViewport.ProcessHitproxyDoubleClick(e.MouseMotion.X, e.MouseMotion.Y);
                    else
                        worldViewport.ProcessHitproxySelected(e.MouseMotion.X, e.MouseMotion.Y);
                }
            }
            worldViewport.Axis?.OnEvent(worldViewport, in e);

            base.OnEvent(in e);
            return true;
        }

        public override void TickOnFocus()
        {
            var worldViewport = WorldViewport;
            if (worldViewport == null)
                return;

            float step = (TtEngine.Instance.ElapseTickCountMS * 0.001f) * worldViewport.CameraMoveSpeed;
            var keyboards = TtEngine.Instance.InputSystem;
            if (keyboards.IsKeyDown(Bricks.Input.Keycode.KEY_w))
            {
                worldViewport.CameraController.Move(ECameraAxis.Forward, step, true);
            }
            else if (keyboards.IsKeyDown(Bricks.Input.Keycode.KEY_s))
            {
                worldViewport.CameraController.Move(ECameraAxis.Forward, -step, true);
            }

            if (keyboards.IsKeyDown(Bricks.Input.Keycode.KEY_a))
            {
                worldViewport.CameraController.Move(ECameraAxis.Right, step, true);
            }
            else if (keyboards.IsKeyDown(Bricks.Input.Keycode.KEY_d))
            {
                worldViewport.CameraController.Move(ECameraAxis.Right, -step, true);
            }
        }
    }
}
