using System;
using System.Collections.Generic;
using System.Security.Permissions;

namespace EngineNS.Editor
{
    public class TtPreviewViewport : EGui.Slate.TtWorldViewportSlate// Graphics.Pipeline.UViewportSlate
    {
        public TtPreviewViewport()
        {

        }
        ~TtPreviewViewport()
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
        new protected async System.Threading.Tasks.Task Initialize_Default(Graphics.Pipeline.TtViewportSlate viewport, USlateApplication application, Graphics.Pipeline.TtRenderPolicy policy, float zMin, float zMax)
        {
            RenderPolicy = policy;

            //await RenderPolicy.Initialize(null);

            CameraController.ControlCamera(RenderPolicy.DefaultCamera);

            var materials = new Graphics.Pipeline.Shader.TtMaterial[1];
            materials[0] = await TtEngine.Instance.GfxDevice.MaterialManager.GetMaterial(RName.GetRName("utest/ttt.material"));
            if (materials[0] == null)
                return;
            var mesh = new Graphics.Mesh.TtMesh();
            var rect = Graphics.Mesh.UMeshDataProvider.MakeBox(-0.5f, -0.5f, -0.5f, 1, 1, 1);
            var rectMesh = rect.ToMesh();
            var ok = mesh.Initialize(rectMesh, materials, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
            if (ok)
            {
                var meshNode = await GamePlay .Scene.TtMeshNode.AddMeshNode(viewport.World, viewport.World.Root, new GamePlay.Scene.TtMeshNode.TtMeshNodeData(), typeof(GamePlay.UPlacement), mesh, DVector3.Zero, Vector3.One, Quaternion.Identity);
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
        public override async System.Threading.Tasks.Task Initialize(USlateApplication application, RName policyName, float zMin, float zMax)
        {
            Graphics.Pipeline.TtRenderPolicy policy = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                var rpAsset = Bricks.RenderPolicyEditor.TtRenderPolicyAsset.LoadAsset(policyName);
                if (rpAsset == null)
                {
                    return null;
                }
                return rpAsset.CreateRenderPolicy(this);
            }, Thread.Async.EAsyncTarget.AsyncIO);
            
            await policy.Initialize(null);
            if (ClientSize.X == 0 || ClientSize.Y == 0)
            {
                policy.OnResize(1, 1);
            }
            else
            {
                policy.OnResize(ClientSize.X, ClientSize.Y);
            }

            await World.InitWorld();

            if (OnInitialize == null)
            {
                OnInitialize = this.Initialize_Default;
            }
            await OnInitialize(this, application, policy, zMin, zMax);

            mDefaultHUD.RenderCamera = this.RenderPolicy.DefaultCamera;
            this.PushHUD(mDefaultHUD);

            mAxis = new GamePlay.UAxis();
            await mAxis.Initialize(this.World, CameraController);

            IsInlitialized = true;
            StartTime = System.DateTime.Now;
            HasAssetSnap = IO.TtFileManager.FileExists(PreviewAsset.Address + ".snap");
        }
        public bool HasAssetSnap = false;
        public System.DateTime StartTime;
        protected override void OnClientChanged(bool bSizeChanged)
        {
            base.OnClientChanged(bSizeChanged);
            var vpSize = this.ClientSize;
            if (bSizeChanged)
            {
                RenderPolicy?.OnResize(vpSize.X, vpSize.Y);
            }
        }
        public RName PreviewAsset { get; set; } = null;
        public delegate void Delegate_OnDrawViewportUIAction(in Vector2 startDrawPos);
        public Delegate_OnDrawViewportUIAction OnDrawViewportUIAction;
        public override void OnDrawViewportUI(in Vector2 startDrawPos) 
        {
            base.OnDrawViewportUI(in startDrawPos);
            if (OnDrawViewportUIAction != null)
            {
                OnDrawViewportUIAction.Invoke(startDrawPos);
            }
            else
            {
                if (PreviewAsset != null)
                {
                    ImGuiAPI.SameLine(0, -1);
                    if (EGui.UIProxy.CustomButton.ToolButton("S", in Vector2.Zero))
                    {
                        Editor.USnapshot.Save(PreviewAsset, TtEngine.Instance.AssetMetaManager.GetAssetMeta(PreviewAsset), RenderPolicy.GetFinalShowRSV());
                    }
                    else if (HasAssetSnap == false)
                    {
                        var tm = System.DateTime.Now;
                        TimeSpan ts = tm.Subtract(StartTime);
                        if (ts.Seconds > 10)
                        {
                            Editor.USnapshot.Save(PreviewAsset, TtEngine.Instance.AssetMetaManager.GetAssetMeta(PreviewAsset), RenderPolicy.GetFinalShowRSV());
                            HasAssetSnap = true;
                        }
                    }
                }
            }
            if (ShowWorldAxis && CameraController.Camera != null)
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
        GamePlay.TtWorld.UVisParameter mVisParameter = new GamePlay.TtWorld.UVisParameter();
        public GamePlay.TtWorld.UVisParameter VisParameter
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
