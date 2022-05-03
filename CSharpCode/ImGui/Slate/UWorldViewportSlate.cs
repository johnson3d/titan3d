using System;
using System.Collections.Generic;
using EngineNS.Graphics.Pipeline;
using SDL2;

namespace EngineNS.EGui.Slate
{
    public class UWorldViewportSlate : Graphics.Pipeline.UViewportSlate
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(ReadOnly = true, UserDraw = false)]
        public Graphics.Pipeline.Shader.CommanShading.UCopy2DPolicy Draw2ViewportPolicy { get; } = new Graphics.Pipeline.Shader.CommanShading.UCopy2DPolicy();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(ReadOnly = true, UserDraw = false)]
        public RHI.CViewPort Viewport { get; } = new RHI.CViewPort();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(ReadOnly = true, UserDraw = false)]
        public RHI.CScissorRect ScissorRect { get; } = new RHI.CScissorRect();

        public Graphics.Pipeline.UDrawBuffers Copy2SwapChainPass = new Graphics.Pipeline.UDrawBuffers();
        public RHI.CRenderPass SwapChainPassDesc;

        GamePlay.UAxis mAxis;

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
            RenderPolicy?.Cleanup();
            RenderPolicy = null;
        }
        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            return true;
        }
        public async System.Threading.Tasks.Task Initialize_Default(Graphics.Pipeline.UViewportSlate viewport, Graphics.Pipeline.USlateApplication application, Graphics.Pipeline.URenderPolicy policy, float zMin, float zMax)
        {
            RenderPolicy = policy;

            CameraController.ControlCamera(RenderPolicy.DefaultCamera);
        }
        public override async System.Threading.Tasks.Task Initialize(Graphics.Pipeline.USlateApplication application, RName policyName, Rtti.UTypeDesc policyType, float zMin, float zMax)
        {
            URenderPolicy policy = null;
            var rpAsset = Bricks.RenderPolicyEditor.URenderPolicyAsset.LoadAsset(policyName);
            if (rpAsset != null)
            {
                policy = rpAsset.CreateRenderPolicy();
            }
            if (policy == null)
            {
                policy = Rtti.UTypeDescManager.CreateInstance(policyType) as Graphics.Pipeline.URenderPolicy;
            }
            await InitializeImpl(application, policy, zMin, zMax);
        }
        private async System.Threading.Tasks.Task InitializeImpl(Graphics.Pipeline.USlateApplication application, Graphics.Pipeline.URenderPolicy policy, float zMin, float zMax)
        {
            await Initialize();
            
            await policy.Initialize(null);

            if (OnInitialize == null)
            {
                OnInitialize = this.Initialize_Default;
            }
            await OnInitialize(this, application, policy, zMin, zMax);

            await this.World.InitWorld();
            SetCameraOffset(in DVector3.Zero);
            SetCameraOffset(new DVector3(-300, 0, 0));

            mAxis = new GamePlay.UAxis();
            await mAxis.Initialize(this.World, CameraController);
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
        }
        protected override IntPtr GetShowTexture()
        {
            var srv = RenderPolicy.GetFinalShowRSV();
            if (srv == null)
                return IntPtr.Zero;
            return srv.GetTextureHandle();
        }
        #region CameraControl
        Vector2 mPreMousePt;
        Vector2 mStartMousePt;
        public float CameraMoveSpeed { get; set; } = 1.0f;
        public float CameraMouseWheelSpeed { get; set; } = 1.0f;
        public unsafe override bool OnEvent(ref SDL.SDL_Event e)
        {
            mAxis?.OnEvent(this, in e);
            var keyboards = UEngine.Instance.InputSystem;
            if (e.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN)
            {
                mStartMousePt = new Vector2(e.motion.x, e.motion.y);
            }
            else if (e.type == SDL.SDL_EventType.SDL_MOUSEMOTION)
            {
                if (e.button.button == SDL.SDL_BUTTON_LEFT)
                {
                    if (keyboards.IsKeyDown(Bricks.Input.Keycode.KEY_LALT))
                    {
                        CameraController.Rotate(Graphics.Pipeline.ECameraAxis.Up, (e.motion.x - mPreMousePt.X) * 0.01f);
                        CameraController.Rotate(Graphics.Pipeline.ECameraAxis.Right, (e.motion.y - mPreMousePt.Y) * 0.01f);
                    }                    
                }
                else if (e.button.button == SDL.SDL_BUTTON_MIDDLE)
                {
                    CameraController.Move(Graphics.Pipeline.ECameraAxis.Right, (e.motion.x - mPreMousePt.X) * 0.01f, true);
                    CameraController.Move(Graphics.Pipeline.ECameraAxis.Up, (e.motion.y - mPreMousePt.Y) * -0.01f, true);
                }
                else if (e.button.button == SDL.SDL_BUTTON_X1)
                {
                    if (keyboards.IsKeyDown(Bricks.Input.Keycode.KEY_LALT))
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
                if (IsViewportSlateFocused)
                {
                    if (keyboards.IsKeyDown(Bricks.Input.Keycode.KEY_LALT))
                    {
                        CameraMoveSpeed += (float)(e.wheel.y * 0.01f);
                    }
                    else
                    {
                        CameraController.Move(Graphics.Pipeline.ECameraAxis.Forward, e.wheel.y * CameraMouseWheelSpeed, true);
                    }
                }
            }
            else if(e.type == SDL.SDL_EventType.SDL_MOUSEBUTTONUP)
            {
                if (e.button.button == SDL.SDL_BUTTON_LEFT && mAxis != null &&
                    mAxis.CurrentAxisType == GamePlay.UAxis.enAxisType.Null &&
                    (new Vector2(e.motion.x, e.motion.y) - mStartMousePt).Length() < 1.0f &&
                    !mAxisOperated &&
                    IsMouseIn)
                {
                    ProcessHitproxySelected(e.motion.x, e.motion.y);
                }
            }

            return base.OnEvent(ref e);
        }
        #endregion
        GamePlay.UWorld.UVisParameter mVisParameter = new GamePlay.UWorld.UVisParameter();
        protected virtual void TickOnFocus()
        {
            float step = (UEngine.Instance.ElapseTickCount * 0.001f) * CameraMoveSpeed;
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
        public unsafe void TickLogic(int ellapse)
        {
            World.TickLogic(this.RenderPolicy, ellapse);

            if (IsDrawing == false)
                return;
            if (this.IsFocused)
            {
                TickOnFocus();
            }

            mVisParameter.World = World;
            mVisParameter.VisibleMeshes = RenderPolicy.VisibleMeshes;
            mVisParameter.VisibleNodes = RenderPolicy.VisibleNodes;
            mVisParameter.CullCamera = RenderPolicy.DefaultCamera;
            World.GatherVisibleMeshes(mVisParameter);

            if (mWorldBoundShapes != null)
            {
                RenderPolicy.VisibleMeshes.AddRange(mWorldBoundShapes);
            }

            RenderPolicy?.TickLogic(World);
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

        public override void OnHitproxySelected(IProxiable proxy)
        {
            base.OnHitproxySelected(proxy);
            var node = proxy as GamePlay.Scene.UNode;
            mAxis.SetSelectedNodes(node);
        }

        bool mAxisOperated = false;
        public override void OnDrawViewportUI(in Vector2 startDrawPos)
        {
            if (mAxis != null)
                mAxisOperated = mAxis.OnDrawUI(this, startDrawPos);
        }
    }
}
