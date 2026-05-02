using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Permissions;

namespace EngineNS.Editor
{
    public class TtPreviewViewport : EGui.Slate.TtWorldViewportSlate
    {
        static TtPreviewViewport()
        {
            TtEngine.Instance.InteractiveModeManager.RegisterMode<TtPreviewViewport, TtPreviewViewportInteractiveMode>();
        }
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
            if (mVisParameter != null)
            {
                mVisParameter.Reset();
                mVisParameter = null;
            }
            base.Dispose();
        }
        new protected async Thread.Async.TtTask<bool> Initialize_Default(Graphics.Pipeline.TtViewportSlate viewport, TtSlateApplication application, Graphics.Pipeline.TtRenderPolicy policy, float zMin, float zMax)
        {
            RenderPolicy = policy;

            //await RenderPolicy.Initialize(null);

            CameraController.ControlCamera(RenderPolicy.DefaultCamera);

            var materials = new Graphics.Pipeline.Shader.TtMaterial[1];
            materials[0] = await RName.GetRName("utest/ttt.material").GetAsset<Graphics.Pipeline.Shader.TtMaterial>();// await TtEngine.Instance.GfxDevice.MaterialManager.GetMaterial(RName.GetRName("utest/ttt.material"));
            if (materials[0] == null)
                return false;
            var mesh = new Graphics.Mesh.TtRenderMesh();
            var rect = Graphics.Mesh.TtMeshDataProvider.MakeBox(-0.5f, -0.5f, -0.5f, 1, 1, 1);
            var rectMesh = rect.ToMesh();
            var ok = mesh.Initialize(rectMesh, materials, Rtti.TtTypeDescGetter<Graphics.Mesh.TtMdfStaticMesh>.TypeDesc);
            if (ok)
            {
                var meshNode = await GamePlay .Scene.TtMeshNode.AddMeshNode(viewport.World, viewport.World.Root, new GamePlay.Scene.TtMeshNode.TtMeshNodeData(), typeof(GamePlay.TtPlacement), mesh, DVector3.Zero, Vector3.One, Quaternion.Identity);
                meshNode.HitproxyType = Graphics.Pipeline.TtHitProxy.EHitproxyType.Root;
                meshNode.NodeData.Name = "PreviewObject";
                meshNode.IsCastShadow = true;
            }

            //this.RenderPolicy.GBuffers.SunLightColor = new Vector3(1, 1, 1);
            //this.RenderPolicy.GBuffers.SunLightDirection = new Vector3(1, 1, 1);
            //this.RenderPolicy.GBuffers.SkyLightColor = new Vector3(0.1f, 0.1f, 0.1f);
            //this.RenderPolicy.GBuffers.GroundLightColor = new Vector3(0.1f, 0.1f, 0.1f);
            //this.RenderPolicy.GBuffers.UpdateViewportCBuffer();

            return true;
        }
        public override async Thread.Async.TtTask<bool> Initialize(TtSlateApplication application, RName policyName, float zMin, float zMax)
        {
            Graphics.Pipeline.TtRenderPolicy policy = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                var rpAsset = policyName.GetAsset<Bricks.RenderPolicyEditor.TtRenderPolicyAsset>().GetResultUntilCompleted();
                if (rpAsset == null)
                {
                    return null;
                }
                return rpAsset.CreateRenderPolicy(policyName, this);
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

            //mDefaultHUD.RenderCamera = this.RenderPolicy.DefaultCamera;
            //this.PushHUD(mDefaultHUD);

            mAxis = new GamePlay.TtAxis();
            await mAxis.Initialize(this.World, CameraController);

            await ReCreateInteractiveModes();

            IsInlitialized = true;
            StartTime = System.DateTime.Now;
            HasAssetSnap = IO.TtFileManager.FileExists(PreviewAsset.Address + ".snap");
            return true;
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
        [Category("Option")]
        [ReadOnly(true)]
        public RName PreviewAsset { get; set; } = null;
        public delegate Vector2 Delegate_OnDrawViewportUIAction(in Vector2 startDrawPos);
        public Delegate_OnDrawViewportUIAction OnDrawViewportUIAction;
        public override Vector2 OnDrawViewportUI(in Vector2 startDrawPos) 
        {
            var baseUsedSize = base.OnDrawViewportUI(in startDrawPos);
            var totalUsedSize = baseUsedSize;
            var adjustedStartPos = new Vector2(startDrawPos.X, startDrawPos.Y + baseUsedSize.Y);

            if (OnDrawViewportUIAction != null)
            {
                var actionUsedSize = OnDrawViewportUIAction.Invoke(adjustedStartPos);
                totalUsedSize.X = Math.Max(totalUsedSize.X, actionUsedSize.X);
                totalUsedSize.Y = baseUsedSize.Y + actionUsedSize.Y;
            }
            else
            {
                if (PreviewAsset != null)
                {
                    ImGuiAPI.SameLine(0, -1);
                    if (EGui.UIProxy.CustomButton.ToolButton("S", in Vector2.Zero))
                    {
                        Editor.TtSnapshot.Save(PreviewAsset, TtEngine.Instance.AssetMetaManager.GetAssetMeta(PreviewAsset), RenderPolicy.GetFinalShowRSV());
                    }
                    else if (HasAssetSnap == false)
                    {
                        var tm = System.DateTime.Now;
                        TimeSpan ts = tm.Subtract(StartTime);
                        if (ts.Seconds > 10)
                        {
                            Editor.TtSnapshot.Save(PreviewAsset, TtEngine.Instance.AssetMetaManager.GetAssetMeta(PreviewAsset), RenderPolicy.GetFinalShowRSV());
                            HasAssetSnap = true;
                        }
                    }
                }
            }

            var currentPos = ImGuiAPI.GetCursorScreenPos();
            totalUsedSize.X = Math.Max(totalUsedSize.X, currentPos.X - startDrawPos.X);
            totalUsedSize.Y = currentPos.Y - startDrawPos.Y;

            if (ShowWorldAxis && CameraController.Camera != null)
                DrawWorldAxis(this.CameraController.Camera);
            return totalUsedSize;
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
        public bool FreezCameraControl = false;
        public delegate void Delegate_OnEvent(in Bricks.Input.Event e);
        public Delegate_OnEvent OnEventAction;
        public enum EViewportMotion
        {
            None = 0,
            Rotate,
            Zoom,
            Move,
            ChangeMoveSpeed,
        }
        EViewportMotion mViewportMotion = EViewportMotion.None;
        public EViewportMotion ViewportMotion
        {
            get => mViewportMotion;
            set => mViewportMotion = value;
        }
        #endregion

        GamePlay.TtWorld.TtVisParameter mVisParameter = new GamePlay.TtWorld.TtVisParameter();
        public GamePlay.TtWorld.TtVisParameter VisParameter
        {
            get => mVisParameter;
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
