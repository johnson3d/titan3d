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

        public bool AutoZoomToNode(TtNode node, float zoomTimeInSecond = 0.0f)
        {
            if (node == null)
                return false;

            return AutoZoomToNodes(new TtNode[] { node }, zoomTimeInSecond);
        }
        static bool IsFinite(in DVector3 value)
        {
            return double.IsFinite(value.X) && double.IsFinite(value.Y) && double.IsFinite(value.Z);
        }
        static bool IsValidFocusBox(in DBoundingBox box)
        {
            return IsFinite(in box.Minimum) && IsFinite(in box.Maximum) &&
                   box.Minimum.X < box.Maximum.X &&
                   box.Minimum.Y < box.Maximum.Y &&
                   box.Minimum.Z < box.Maximum.Z;
        }
        public bool AutoZoomToNodes(IList<TtNode> nodes, float zoomTimeInSecond = 0.0f)
        {
            var camera = CameraController?.Camera;
            if (nodes == null || nodes.Count == 0 || camera == null)
                return false;

            var box = DBoundingBox.EmptyBox();
            bool hasTarget = false;
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (node == null || node.Placement == null)
                    continue;

                node.GetWorldSpaceBoundingBox(out var nodeBox);
                if (IsValidFocusBox(in nodeBox))
                {
                    box.Merge(in nodeBox);
                    hasTarget = true;
                    continue;
                }

                // 没有有效包围盒的逻辑节点按其位置聚焦；位置本身非法则跳过该节点。
                var position = node.Placement.AbsTransform.Position;
                if (IsFinite(in position))
                {
                    box.Merge(in position);
                    hasTarget = true;
                }
            }

            if (hasTarget == false || !IsFinite(in box.Minimum) || !IsFinite(in box.Maximum))
                return false;

            var size = box.Maximum - box.Minimum;
            if (!IsFinite(in size))
                return false;
            var maxSide = Math.Max(size.X, Math.Max(size.Y, size.Z));
            if (!double.IsFinite(maxSide) || maxSide > float.MaxValue)
                return false;

            // 分量分别乘 0.5，避免两个同号大坐标相加时溢出。
            var center = box.Minimum * 0.5 + box.Maximum * 0.5;
            if (!IsFinite(in center))
                return false;

            var radius = (float)Math.Max(maxSide, 1.0);
            var sphere = new DBoundingSphere(center, radius);
            camera.AutoZoom(in sphere, zoomTimeInSecond);
            return true;
        }
        public override bool FocusViewport()
        {
            return AutoZoomToNode(World?.Root);
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
