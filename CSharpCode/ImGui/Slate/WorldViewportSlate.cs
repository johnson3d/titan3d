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
        static TtWorldViewportSlate()
        {
            TtEngine.Instance.InteractiveModeManager.RegisterMode<TtWorldViewportSlate, TtWorldViewportInteractiveMode>();
        }
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
        public async Thread.Async.TtTask<bool> Initialize_Default(Graphics.Pipeline.TtViewportSlate viewport, TtSlateApplication application, Graphics.Pipeline.TtRenderPolicy policy, float zMin, float zMax)
        {
            RenderPolicy = policy;

            CameraController.ControlCamera(RenderPolicy.DefaultCamera);
            return true;
        }
        public override async Thread.Async.TtTask<bool> Initialize(TtSlateApplication application, RName policyName, float zMin, float zMax)
        {
            var policy = Bricks.RenderPolicyEditor.TtRenderPolicyAsset.CreateRenderPolicy(policyName, this);
            if (false == await InitializeImpl(application, policy, zMin, zMax))
                return false;

            await ReCreateInteractiveModes();
            await InitParticlePolicy();

            IsInlitialized = true;
            return true;
        }
        private async Thread.Async.TtTask<bool> InitializeImpl(TtSlateApplication application, Graphics.Pipeline.TtRenderPolicy policy, float zMin, float zMax)
        {
            await InitWorld();

            await Initialize();

            await policy.Initialize(null);
            if (Viewport.Width > 1 && Viewport.Height > 1)
                policy.OnResize(Viewport.Width, Viewport.Height);

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
        #endregion
        

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
        public bool UIOperated
        {
            get => mUIOperated;
            internal set => mUIOperated = value;
        }
        public bool DrawAxisUI = true;
        public bool DrawCameraOPUI = true;

        public override Vector2 OnDrawViewportUI(in Vector2 startDrawPos)
        {
            var baseUsedSize = base.OnDrawViewportUI(in startDrawPos);
            var totalUsedSize = baseUsedSize;
            var adjustedStartPos = new Vector2(startDrawPos.X, startDrawPos.Y + baseUsedSize.Y);

            if (mAxis != null && DrawAxisUI)
                mUIOperated = mAxis.OnDrawUI(this, adjustedStartPos);

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

            var currentPos = ImGuiAPI.GetCursorScreenPos();
            totalUsedSize.X = Math.Max(totalUsedSize.X, currentPos.X - startDrawPos.X);
            totalUsedSize.Y = currentPos.Y - startDrawPos.Y;

            if (ShowWorldAxis)
                DrawWorldAxis(this.CameraController.Camera);

            return totalUsedSize;
        }
    }
}
