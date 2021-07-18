using System;
using System.Collections.Generic;
using SDL2;

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
            RenderPolicy?.Cleanup();
            RenderPolicy = null;
            if (SnapshotPtr != IntPtr.Zero)
            {
                var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(SnapshotPtr);
                handle.Free();
                SnapshotPtr = IntPtr.Zero;
            }
        }
        IntPtr SnapshotPtr;
        
        protected async System.Threading.Tasks.Task Initialize_Default(Graphics.Pipeline.UViewportSlate viewport, Graphics.Pipeline.USlateApplication application, Graphics.Pipeline.IRenderPolicy policy, float zMin, float zMax)
        {
            RenderPolicy = policy;

            await RenderPolicy.Initialize(1, 1);

            CameraController.Camera = RenderPolicy.GetBasePassNode().GBuffers.Camera;

            var materials = new Graphics.Pipeline.Shader.UMaterial[1];
            materials[0] = await UEngine.Instance.GfxDevice.MaterialManager.GetMaterial(RName.GetRName("utest/ttt.material"));
            if (materials[0] == null)
                return;
            var mesh = new Graphics.Mesh.UMesh();
            var rect = Graphics.Mesh.CMeshDataProvider.MakeBox(-0.5f, -0.5f, -0.5f, 1, 1, 1);
            var rectMesh = rect.ToMesh();
            var ok = mesh.Initialize(rectMesh, materials, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
            if (ok)
            {
                var meshNode = GamePlay.Scene.UMeshNode.AddMeshNode(viewport.World.Root, new GamePlay.Scene.UMeshNode.UMeshNodeData(), typeof(GamePlay.UPlacement), mesh, Vector3.Zero, Vector3.One, Quaternion.Identity);
                meshNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
                meshNode.NodeData.Name = "PreviewObject";
                meshNode.IsScaleChildren = false;
                meshNode.IsCastShadow = true;
            }

            //this.RenderPolicy.GBuffers.SunLightColor = new Vector3(1, 1, 1);
            //this.RenderPolicy.GBuffers.SunLightDirection = new Vector3(1, 1, 1);
            //this.RenderPolicy.GBuffers.SkyLightColor = new Vector3(0.1f, 0.1f, 0.1f);
            //this.RenderPolicy.GBuffers.GroundLightColor = new Vector3(0.1f, 0.1f, 0.1f);
            //this.RenderPolicy.GBuffers.UpdateViewportCBuffer();
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
            var vpSize = this.ClientSize;
            if (bSizeChanged)
            {
                RenderPolicy?.OnResize(vpSize.X, vpSize.Y);
                if (SnapshotPtr != IntPtr.Zero)
                {
                    var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(SnapshotPtr);
                    handle.Free();
                }
                SnapshotPtr = System.Runtime.InteropServices.GCHandle.ToIntPtr(System.Runtime.InteropServices.GCHandle.Alloc(RenderPolicy.GetFinalShowRSV()));
            }
        }
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
            if (this.IsFocused == false)
            {
                return true;
            }
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
                    CameraController.Move(Graphics.Pipeline.ECameraAxis.Right, (e.motion.x - mPreMousePt.X) * 0.01f);
                    CameraController.Move(Graphics.Pipeline.ECameraAxis.Up, (e.motion.y - mPreMousePt.Y) * 0.01f);
                }
                else if (e.button.button == SDL.SDL_BUTTON_X1)
                {
                    if (keyboards[(int)SDL.SDL_Scancode.SDL_SCANCODE_LALT])
                    {
                        CameraController.Move(Graphics.Pipeline.ECameraAxis.Forward, (e.motion.y - mPreMousePt.Y) * 0.03f);
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
                    CameraController.Move(Graphics.Pipeline.ECameraAxis.Forward, e.wheel.y * CameraMouseWheelSpeed);
                }
            }
            return true;
        }
        #endregion
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
        GamePlay.UWorld.UVisParameter mVisParameter = new GamePlay.UWorld.UVisParameter();
        public void TickLogic(int ellapse)
        {
            World.TickLogic();

            if (IsDrawing == false)
                return;

            if (this.IsFocused)
            {
                TickOnFocus();
            }

            mVisParameter.VisibleMeshes = RenderPolicy.VisibleMeshes;
            mVisParameter.CullCamera = RenderPolicy.GetBasePassNode().GBuffers.Camera;
            World.GatherVisibleMeshes(mVisParameter);

            RenderPolicy?.TickLogic(World);
        }
        public void TickRender(int ellapse)
        {
            if (IsDrawing == false)
                return;
            RenderPolicy?.TickRender();
        }
        public Action AfterTickSync;
        public void TickSync(int ellapse)
        {
            if (IsDrawing == false)
                return;
            RenderPolicy?.TickSync();
            if (AfterTickSync != null)
                AfterTickSync();
        }
    }
}
