using System;
using System.Collections.Generic;

namespace EngineNS.Editor
{
    public class UPreviewViewport : Graphics.Pipeline.UViewportSlate
    {
        public Editor.Controller.EditorCameraController CameraController = new Editor.Controller.EditorCameraController();
        public UPreviewViewport()
        {

        }
        ~UPreviewViewport()
        {
            Cleanup();
        }
        public void Cleanup()
        {
            PresentWindow?.UnregEventProcessor(this);
            RenderPolicy?.Cleanup();
            RenderPolicy = null;
        }
        protected async System.Threading.Tasks.Task Initialize_Default(Graphics.Pipeline.UViewportSlate viewport, USlateApplication application, Graphics.Pipeline.URenderPolicy policy, float zMin, float zMax)
        {
            RenderPolicy = policy;

            //await RenderPolicy.Initialize(null);

            CameraController.ControlCamera(RenderPolicy.DefaultCamera);

            var materials = new Graphics.Pipeline.Shader.UMaterial[1];
            materials[0] = await UEngine.Instance.GfxDevice.MaterialManager.GetMaterial(RName.GetRName("utest/ttt.material"));
            if (materials[0] == null)
                return;
            var mesh = new Graphics.Mesh.UMesh();
            var rect = Graphics.Mesh.UMeshDataProvider.MakeBox(-0.5f, -0.5f, -0.5f, 1, 1, 1);
            var rectMesh = rect.ToMesh();
            var ok = mesh.Initialize(rectMesh, materials, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
            if (ok)
            {
                var meshNode = await GamePlay .Scene.UMeshNode.AddMeshNode(viewport.World, viewport.World.Root, new GamePlay.Scene.UMeshNode.UMeshNodeData(), typeof(GamePlay.UPlacement), mesh, DVector3.Zero, Vector3.One, Quaternion.Identity);
                meshNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
                meshNode.NodeData.Name = "PreviewObject";
                meshNode.IsCastShadow = true;
            }

            //this.RenderPolicy.GBuffers.SunLightColor = new Vector3(1, 1, 1);
            //this.RenderPolicy.GBuffers.SunLightDirection = new Vector3(1, 1, 1);
            //this.RenderPolicy.GBuffers.SkyLightColor = new Vector3(0.1f, 0.1f, 0.1f);
            //this.RenderPolicy.GBuffers.GroundLightColor = new Vector3(0.1f, 0.1f, 0.1f);
            //this.RenderPolicy.GBuffers.UpdateViewportCBuffer();
        }
        bool Initialized = false;
        public override async System.Threading.Tasks.Task Initialize(USlateApplication application, RName policyName, float zMin, float zMax)
        {
            Graphics.Pipeline.URenderPolicy policy = null;
            var rpAsset = Bricks.RenderPolicyEditor.URenderPolicyAsset.LoadAsset(policyName);
            if (rpAsset != null)
            {
                policy = rpAsset.CreateRenderPolicy();
            }
            await policy.Initialize(null);

            await World.InitWorld();

            if (OnInitialize == null)
            {
                OnInitialize = this.Initialize_Default;
            }
            await OnInitialize(this, application, policy, zMin, zMax);

            if (ClientSize.X == 0 || ClientSize.Y == 0)
            {
                RenderPolicy.OnResize(1, 1);
            }
            else
            {
                RenderPolicy.OnResize(ClientSize.X, ClientSize.Y);
            }
            Initialized = true;
        }
        protected override void OnClientChanged(bool bSizeChanged)
        {
            var vpSize = this.ClientSize;
            if (bSizeChanged)
            {
                RenderPolicy?.OnResize(vpSize.X, vpSize.Y);                
            }
        }
        public RName PreviewAsset { get; set; } = null;
        public override void OnDrawViewportUI(in Vector2 startDrawPos) 
        {
            if (PreviewAsset != null && ImGuiAPI.Button("S"))
            {
                Editor.USnapshot.Save(PreviewAsset, UEngine.Instance.AssetMetaManager.GetAssetMeta(PreviewAsset), RenderPolicy.GetFinalShowRSV());
            }
            if (ShowWorldAxis)
                DrawWorldAxis(this.CameraController.Camera);
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
        public unsafe override bool OnEvent(in Bricks.Input.Event e)
        {
            if (this.IsFocused == false)
            {
                return true;
            }

            var keyboards = UEngine.Instance.InputSystem;
            if (e.Type == Bricks.Input.EventType.MOUSEMOTION)
            {
                if (e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_LEFT)
                {
                    if (keyboards.IsKeyDown(Bricks.Input.Keycode.KEY_LALT))
                    {
                        CameraController.Rotate(Graphics.Pipeline.ECameraAxis.Up, (e.MouseMotion.X - mPreMousePt.X) * 0.01f);
                        CameraController.Rotate(Graphics.Pipeline.ECameraAxis.Right, (e.MouseMotion.Y - mPreMousePt.Y) * 0.01f);
                    }
                }
                else if (e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_MIDDLE)
                {
                    CameraController.Move(Graphics.Pipeline.ECameraAxis.Right, (e.MouseMotion.X - mPreMousePt.X) * 0.01f);
                    CameraController.Move(Graphics.Pipeline.ECameraAxis.Up, (e.MouseMotion.Y - mPreMousePt.Y) * 0.01f);
                }
                else if (e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_X1)
                {
                    if (keyboards.IsKeyDown(Bricks.Input.Keycode.KEY_LALT))
                    {
                        CameraController.Move(Graphics.Pipeline.ECameraAxis.Forward, (e.MouseMotion.Y - mPreMousePt.Y) * 0.03f);
                    }
                    else
                    {
                        CameraController.Rotate(Graphics.Pipeline.ECameraAxis.Up, (e.MouseMotion.X - mPreMousePt.X) * 0.01f, true);
                        CameraController.Rotate(Graphics.Pipeline.ECameraAxis.Right, (e.MouseMotion.Y - mPreMousePt.Y) * 0.01f, true);
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
        GamePlay.UWorld.UVisParameter mVisParameter = new GamePlay.UWorld.UVisParameter();
        public GamePlay.UWorld.UVisParameter VisParameter
        {
            get => mVisParameter;
        }

        public void TickLogic(int ellapse)
        {
            if (Initialized == false)
                return;
            RenderPolicy?.BeginTickLogic(World);

            World.TickLogic(this.RenderPolicy, ellapse);

            if (IsDrawing)
            {
                if (this.IsFocused)
                {
                    TickOnFocus();
                }

                mVisParameter.World = World;
                mVisParameter.VisibleMeshes = RenderPolicy.VisibleMeshes;
                mVisParameter.VisibleNodes = RenderPolicy.VisibleNodes;
                mVisParameter.CullCamera = RenderPolicy.DefaultCamera;
                World.GatherVisibleMeshes(mVisParameter);

                RenderPolicy?.TickLogic(World);
            }

            RenderPolicy?.EndTickLogic(World);
        }
        public void TickRender(int ellapse)
        {
            
        }
        public Action AfterTickSync;
        public void TickSync(int ellapse)
        {
            if (Initialized == false)
                return;
            //if (IsDrawing == false)
            //    return;
            RenderPolicy?.TickSync();
            if (AfterTickSync != null)
                AfterTickSync();
        }
    }
}
