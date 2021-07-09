using System;
using System.Collections.Generic;
using SDL2;

namespace EngineNS.EGui.Slate
{
    public class UWorldViewportSlate : Graphics.Pipeline.UViewportSlate
    {
        public GamePlay.UWorld World { get; protected set; } = new GamePlay.UWorld();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(ReadOnly = true, UserDraw = false)]
        public Graphics.Pipeline.Shader.CommanShading.UCopy2DPolicy Draw2ViewportPolicy { get; } = new Graphics.Pipeline.Shader.CommanShading.UCopy2DPolicy();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(ReadOnly = true, UserDraw = false)]
        public RHI.CViewPort Viewport { get; } = new RHI.CViewPort();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(ReadOnly = true, UserDraw = false)]
        public RHI.CScissorRect ScissorRect { get; } = new RHI.CScissorRect();

        public Graphics.Pipeline.UDrawBuffers Copy2SwapChainPass = new Graphics.Pipeline.UDrawBuffers();
        public RenderPassDesc SwapChainPassDesc = new RenderPassDesc();

        public Graphics.Pipeline.ICameraController CameraController { get; set; }
        public UWorldViewportSlate()
        {
            CameraController = new Editor.Controller.EditorCameraController();
        }
        ~UWorldViewportSlate()
        {
            Cleanup();
        }
        public void Cleanup()
        {
            World?.Cleanup();
            World = null;
            if (SnapshotPtr != IntPtr.Zero)
            {
                var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(SnapshotPtr);
                handle.Free();
                SnapshotPtr = IntPtr.Zero;
            }
            RenderPolicy?.Cleanup();
            RenderPolicy = null;
        }
        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            return true;
        }
        public async System.Threading.Tasks.Task Initialize_Default(Graphics.Pipeline.UViewportSlate viewport, Graphics.Pipeline.USlateApplication application, Graphics.Pipeline.IRenderPolicy policy, float zMin, float zMax)
        {
            RenderPolicy = policy;

            await RenderPolicy.Initialize(1, 1);

            CameraController.Camera = RenderPolicy.GBuffers.Camera;
        }
        public override async System.Threading.Tasks.Task Initialize(Graphics.Pipeline.USlateApplication application, Graphics.Pipeline.IRenderPolicy policy, float zMin, float zMax)
        {
            if (OnInitialize == null)
            {
                OnInitialize = this.Initialize_Default;
            }
            await OnInitialize(this, application, policy, zMin, zMax);
        }
        protected override void OnClientChanged(bool bSizeChanged)
        {
            if (RenderPolicy == null)
                return;

            var vpSize = this.ClientSize;
            Viewport.mCoreObject.TopLeftX = WindowPos.X + ClientMin.X;
            Viewport.mCoreObject.TopLeftY = WindowPos.Y + ClientMin.Y;
            Viewport.mCoreObject.Width = vpSize.X;
            Viewport.mCoreObject.Height = vpSize.Y;

            ScissorRect.mCoreObject.SetRectNumber(1);
            ScissorRect.mCoreObject.SetSCRect(0, 
                (int)Viewport.mCoreObject.TopLeftX, 
                (int)Viewport.mCoreObject.TopLeftY,
                (int)(Viewport.mCoreObject.TopLeftX + vpSize.X),
                (int)(Viewport.mCoreObject.TopLeftY + vpSize.Y));

            if (bSizeChanged)
            {
                RenderPolicy?.OnResize(vpSize.X, vpSize.Y);
            }

            if (SnapshotPtr != IntPtr.Zero)
            {
                var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(SnapshotPtr);
                handle.Free();
            }
            SnapshotPtr = System.Runtime.InteropServices.GCHandle.ToIntPtr(System.Runtime.InteropServices.GCHandle.Alloc(RenderPolicy.GetFinalShowRSV()));
        }
        IntPtr SnapshotPtr;
        protected override IntPtr GetShowTexture()
        {
            return SnapshotPtr;
        }
        #region CameraControl
        Vector2 mPreMousePt;
        public float CameraMoveSpeed { get; set; } = 1.0f;
        public float CameraMouseWheelSpeed { get; set; } = 1.0f;
        public unsafe override bool OnEvent(ref SDL.SDL_Event e)
        {
            var keyboards = UEngine.Instance.EventProcessorManager.Keyboards;
            if (e.type == SDL.SDL_EventType.SDL_MOUSEMOTION)
            {
                if (e.button.button == SDL.SDL_BUTTON_LEFT)
                {
                    if (keyboards[(int)SDL.SDL_Scancode.SDL_SCANCODE_LALT])
                    {
                        CameraController.Rotate(Graphics.Pipeline.ECameraAxis.Up, (e.motion.x - mPreMousePt.X) * 0.01f);
                        CameraController.Rotate(Graphics.Pipeline.ECameraAxis.Right, (e.motion.y - mPreMousePt.Y) * 0.01f);
                    }                    
                }
                else if (e.button.button == SDL.SDL_BUTTON_MIDDLE)
                {
                    CameraController.Move(Graphics.Pipeline.ECameraAxis.Right, (e.motion.x - mPreMousePt.X) * 0.01f, true);
                    CameraController.Move(Graphics.Pipeline.ECameraAxis.Up, (e.motion.y - mPreMousePt.Y) * 0.01f, true);
                }
                else if (e.button.button == SDL.SDL_BUTTON_X1)
                {
                    if (keyboards[(int)SDL.SDL_Scancode.SDL_SCANCODE_LALT])
                    {
                        CameraController.Move(Graphics.Pipeline.ECameraAxis.Forward, (e.motion.y - mPreMousePt.Y) * 0.03f, true);
                    }
                    else
                    {
                        CameraController.Rotate(Graphics.Pipeline.ECameraAxis.Up, (e.motion.x - mPreMousePt.X) * 0.01f, true);
                        CameraController.Rotate(Graphics.Pipeline.ECameraAxis.Right, (e.motion.y - mPreMousePt.Y) * 0.01f, true);
                    }
                }

                mPreMousePt.X = e.motion.x;
                mPreMousePt.Y = e.motion.y;
            }
            else if (e.type == SDL.SDL_EventType.SDL_MOUSEWHEEL)
            {
                if (keyboards[(int)SDL.SDL_Scancode.SDL_SCANCODE_LALT])
                {
                    CameraMoveSpeed += (float)(e.wheel.y * 0.01f);
                }
                else
                {
                    CameraController.Move(Graphics.Pipeline.ECameraAxis.Forward, e.wheel.y * CameraMouseWheelSpeed, true);
                }
            }
            else
            {
                return base.OnEvent(ref e);
            }
            return true;
        }
        #endregion
        GamePlay.UWorld.UVisParameter mVisParameter = new GamePlay.UWorld.UVisParameter();
        protected virtual void TickOnFocus()
        {
            float step = (UEngine.Instance.ElapseTickCount * 0.001f) * CameraMoveSpeed;
            var keyboards = UEngine.Instance.EventProcessorManager.Keyboards;
            if (keyboards[(int)SDL.SDL_Scancode.SDL_SCANCODE_W])
            {
                CameraController.Move(Graphics.Pipeline.ECameraAxis.Forward, step, true);
            }
            else if (keyboards[(int)SDL.SDL_Scancode.SDL_SCANCODE_S])
            {
                CameraController.Move(Graphics.Pipeline.ECameraAxis.Forward, -step, true);
            }

            if (keyboards[(int)SDL.SDL_Scancode.SDL_SCANCODE_A])
            {
                CameraController.Move(Graphics.Pipeline.ECameraAxis.Right, step, true);
            }
            else if (keyboards[(int)SDL.SDL_Scancode.SDL_SCANCODE_D])
            {
                CameraController.Move(Graphics.Pipeline.ECameraAxis.Right, -step, true);
            }
        }
        public unsafe void TickLogic(int ellapse)
        {
            World.TickLogic();

            if (IsDrawing == false)
                return;
            if (this.IsFocused)
            {
                TickOnFocus();
            }

            mVisParameter.VisibleMeshes = RenderPolicy.VisibleMeshes;
            mVisParameter.CullCamera = RenderPolicy.GBuffers.Camera;
            World.GatherVisibleMeshes(mVisParameter);

            if (mWorldBoundShapes != null)
            {
                RenderPolicy.VisibleMeshes.AddRange(mWorldBoundShapes);
            }

            RenderPolicy?.TickLogic();
        }
        public unsafe void TickRender(int ellapse)
        {
            if (IsDrawing == false)
                return;
            RenderPolicy?.TickRender();
        }
        public void TickSync(int ellapse)
        {
            if (IsDrawing == false)
                return;
            RenderPolicy?.TickSync();
        }
        #region Debug Assist
        List<Graphics.Mesh.UMesh> mWorldBoundShapes;
        public void ShowBoundVolumes(bool bShow, GamePlay.Scene.UNode node = null)
        {
            if (bShow)
            {
                mWorldBoundShapes = new List<Graphics.Mesh.UMesh>();
                World.GatherBoundShapes(mWorldBoundShapes, node);
            }
            else
            {
                mWorldBoundShapes?.Clear();
                mWorldBoundShapes = null;
            }
        }
        #endregion
    }
}
