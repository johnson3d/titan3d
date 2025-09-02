using System;
using System.Collections.Generic;
using EngineNS.Editor.Forms;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Pipeline;

namespace EngineNS.EGui.Slate
{
    public class TtWorldViewportSlate : Graphics.Pipeline.TtViewportSlate
    {
        public NxRHI.TtRenderPass SwapChainPassDesc;

        protected GamePlay.TtAxis mAxis;
        public GamePlay.TtAxis Axis
        {
            get => mAxis;
        }

        public Graphics.Pipeline.ICameraController CameraController { get; set; }
        public TtWorldViewportSlate()
        {
            CameraController = new Editor.Controller.EditorCameraController();
        }
        public override void Dispose()
        {
            PresentWindow?.UnregEventProcessor(this);
            World?.Dispose();
            World = null;
            RenderPolicy?.Dispose();
            RenderPolicy = null;

            base.Dispose();
        }
        public async Thread.Async.TtTask<bool> Initialize()
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
            return true;
        }
        public async System.Threading.Tasks.Task<bool> Initialize_Default(Graphics.Pipeline.TtViewportSlate viewport, TtSlateApplication application, Graphics.Pipeline.TtRenderPolicy policy, float zMin, float zMax)
        {
            RenderPolicy = policy;

            CameraController.ControlCamera(RenderPolicy.DefaultCamera);
            return true;
        }
        public override async System.Threading.Tasks.Task<bool> Initialize(TtSlateApplication application, RName policyName, float zMin, float zMax)
        {
            TtRenderPolicy policy = null;
            var rpAsset = Bricks.RenderPolicyEditor.TtRenderPolicyAsset.LoadAsset(policyName);
            if (rpAsset != null)
            {
                policy = rpAsset.CreateRenderPolicy(this);
            }
            if (false == await InitializeImpl(application, policy, zMin, zMax))
                return false;

            IsInlitialized = true;
            return true;
        }
        private async Thread.Async.TtTask<bool> InitializeImpl(TtSlateApplication application, Graphics.Pipeline.TtRenderPolicy policy, float zMin, float zMax)
        {
            await Initialize();
            
            await policy.Initialize(null);
            if (Viewport.Width > 1 && Viewport.Height > 1)
                policy.OnResize(Viewport.Width, Viewport.Height);

            await this.World.InitWorld();
            if (OnInitialize == null)
            {
                OnInitialize = this.Initialize_Default;
            }
            if (false == await OnInitialize(this, application, policy, zMin, zMax))
                return false;

            SetCameraOffset(in DVector3.Zero);

            //mDefaultHUD.RenderCamera = this.RenderPolicy.DefaultCamera;
            //this.PushHUD(mDefaultHUD);
            //SetCameraOffset(new DVector3(-300, 0, 0));

            mAxis = new GamePlay.TtAxis();
            await mAxis.Initialize(this.World, CameraController);

            return true;
        }
        #region CameraControl
        Vector2 mPreMousePt;
        Vector2 mStartMousePt;
        float mCameraMoveSpeed = 1.0f;
        public float CameraMoveSpeed 
        {
            get => mCameraMoveSpeed;
            set
            {
                mCameraMoveSpeed = value;
            }
        }
        float mCameraMouseWheelSpeed = 1.0f;
        public float CameraMouseWheelSpeed 
        {
            get => mCameraMouseWheelSpeed;
            set
            {
                mCameraMouseWheelSpeed = value;
            }
        }
        float mCameraMouseRotSpeed = 1.0f;
        public float CameraMouseRotSpeed
        {
            get => mCameraMouseRotSpeed;
            set
            {
                mCameraMouseRotSpeed = value;
            }
        }
        public bool CameralWheelMoveWithLookAt { get; set; } = false;
        public unsafe override bool OnEvent(in Bricks.Input.Event e)
        {
            var keyboards = TtEngine.Instance.InputSystem;
            if (e.Type == Bricks.Input.EventType.MOUSEBUTTONDOWN)
            {
                mStartMousePt = new Vector2(e.MouseMotion.X, e.MouseMotion.Y);
            }
            else if (e.Type == Bricks.Input.EventType.MOUSEMOTION)
            {
                if (e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_LEFT)
                {
                    if (keyboards.IsKeyDown(Bricks.Input.Keycode.KEY_LALT))
                    {
                        CameraController.Rotate(Graphics.Pipeline.ECameraAxis.Up, (e.MouseMotion.X - mPreMousePt.X) * mCameraMouseRotSpeed * TtEngine.Instance.ElapsedSecond);
                        CameraController.Rotate(Graphics.Pipeline.ECameraAxis.Right, (e.MouseMotion.Y - mPreMousePt.Y) * mCameraMouseRotSpeed * TtEngine.Instance.ElapsedSecond);
                    }                    
                }
                else if (e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_MIDDLE)
                {
                    CameraController.Move(Graphics.Pipeline.ECameraAxis.Right, (e.MouseMotion.X - mPreMousePt.X) * CameraMoveSpeed * TtEngine.Instance.ElapsedSecond, true);
                    CameraController.Move(Graphics.Pipeline.ECameraAxis.Up, (e.MouseMotion.Y - mPreMousePt.Y) * -CameraMoveSpeed * TtEngine.Instance.ElapsedSecond, true);
                }
                else if (e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_X1)
                {
                    if (keyboards.IsKeyDown(Bricks.Input.Keycode.KEY_LALT))
                    {
                        CameraController.Move(Graphics.Pipeline.ECameraAxis.Forward, (e.MouseMotion.Y - mPreMousePt.Y) * 0.03f, false);
                    }
                    else
                    {
                        CameraController.Rotate(Graphics.Pipeline.ECameraAxis.Up, (e.MouseMotion.X - mPreMousePt.X) * mCameraMouseRotSpeed * TtEngine.Instance.ElapsedSecond, true);
                        CameraController.Rotate(Graphics.Pipeline.ECameraAxis.Right, (e.MouseMotion.Y - mPreMousePt.Y) * mCameraMouseRotSpeed * TtEngine.Instance.ElapsedSecond, true);
                    }
                }

                mPreMousePt.X = e.MouseMotion.X;
                mPreMousePt.Y = e.MouseMotion.Y;
            }
            else if (e.Type == Bricks.Input.EventType.MOUSEWHEEL)
            {
                if (IsViewportSlateFocused)
                {
                    if (keyboards.IsKeyDown(Bricks.Input.Keycode.KEY_LALT))
                    {
                        CameraMoveSpeed += (float)(e.MouseWheel.Y * 0.01f);
                    }
                    else
                    {
                        CameraController.Move(Graphics.Pipeline.ECameraAxis.Forward, e.MouseWheel.Y * CameraMouseWheelSpeed, CameralWheelMoveWithLookAt);
                    }
                }
            }
            else if(e.Type == Bricks.Input.EventType.MOUSEBUTTONUP)
            {
                var viewportPoint = new Vector2(e.MouseMotion.X, e.MouseMotion.Y) + ViewportPos;
                if (e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_LEFT && mAxis != null &&
                    mAxis.CurrentAxisType == GamePlay.TtAxis.enAxisType.Null &&
                    ((new Vector2(e.MouseMotion.X, e.MouseMotion.Y) - mStartMousePt).Length() < 1.0f) &&
                    !mUIOperated &&
                    IsMouseIn &&
                    !PointInOverlappedArea(in viewportPoint))
                {
                    ProcessHitproxySelected(e.MouseMotion.X, e.MouseMotion.Y);
                }
            }
            mAxis?.OnEvent(this, in e);

            return base.OnEvent(in e);
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
        
        #region Debug Assist
        public List<FVisibleMesh> WorldBoundShapes = new List<FVisibleMesh>();
        public void ShowBoundVolumes(bool bClear, bool bShow, params GamePlay.Scene.TtNode[] nodes)
        {
            if (bShow)
            {
                if (bClear)
                    WorldBoundShapes.Clear();
                for(int i=0; i<nodes.Length; i++)
                {
                    World.GatherBoundShapes(WorldBoundShapes, nodes[i]);
                }
            }
            else
            {
                WorldBoundShapes?.Clear();
            }
        }
        #endregion

        public override void OnHitproxySelected(IProxiable proxy)
        {
            base.OnHitproxySelected(proxy);
            var node = proxy as GamePlay.Scene.TtNode;
            mAxis.SetSelectedNodes(node);
        }

        public override void OnHitproxySelectedMulti(bool clearPre, params IProxiable[] proxies)
        {
            base.OnHitproxySelectedMulti(clearPre, proxies);
            if (proxies == null || proxies.Length == 0)
                mAxis.SetSelectedNodes((List<TtNode>)null);
            else
            {
                var nodes = new List<TtNode>(proxies.Length);
                for (int i = 0; i < proxies.Length; i++)
                {
                    var uNode = proxies[i] as TtNode;
                    nodes.Add(uNode);
                }
                mAxis.SetSelectedNodes(nodes);
            }
        }
        public override void OnHitproxyUnSelectedMulti(params IProxiable[] proxies)
        {
            base.OnHitproxyUnSelectedMulti(proxies);
            for(int i=0; i<proxies.Length; i++)
            {
                mAxis.UnSelectedNode(proxies[i] as TtNode);
            }
        }

        bool mUIOperated = false;
        public bool DrawAxisUI = true;
        public bool DrawCameraOPUI = true;

        public override void OnDrawViewportUI(in Vector2 startDrawPos)
        {
            if (mAxis != null && DrawAxisUI)
                mUIOperated = mAxis.OnDrawUI(this, startDrawPos);

            if(DrawCameraOPUI)
            {
                bool mCameraOPToggled = false;
                if(ImGuiAPI.BeginPopup("ViewPortCameraOP", ImGuiWindowFlags_.ImGuiWindowFlags_NoTitleBar | ImGuiWindowFlags_.ImGuiWindowFlags_NoResize))
                {
                    mCameraOPToggled = true;
                    ImGuiAPI.InputFloat("MoveSpeed", ref mCameraMoveSpeed, 0, 0, "%.6f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                    ImGuiAPI.InputFloat("WheelSpeed", ref mCameraMouseWheelSpeed, 0, 0, "%.6f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                    ImGuiAPI.InputFloat("RotSpeed", ref mCameraMouseRotSpeed, 0, 0, "%.6f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);

                    ImGuiAPI.EndPopup();
                }
                if(EGui.UIProxy.CustomButton.ToggleButton("Camera", in Vector2.Zero, ref mCameraOPToggled))
                {
                    ImGuiAPI.OpenPopup("ViewPortCameraOP", ImGuiPopupFlags_.ImGuiPopupFlags_None);
                }
                bool mRPolicyOPToggled = false;
                if (EGui.UIProxy.CustomButton.ToggleButton("RPolicy", in Vector2.Zero, ref mRPolicyOPToggled))
                {
                    var mainEditor = TtEngine.Instance.GfxDevice.SlateApplication as EngineNS.Editor.TtMainEditorApplication;
                    if (mainEditor != null)
                    {
                        mainEditor.mMainInspector.PropertyGrid.Target = this.RenderPolicy;
                    }
                }
            }

            if (ShowWorldAxis)
                DrawWorldAxis(this.CameraController.Camera);
        }
    }
}
