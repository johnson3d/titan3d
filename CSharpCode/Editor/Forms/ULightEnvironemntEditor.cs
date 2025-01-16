using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Editor.Forms
{
    public class TtLightEnvironemnt : ITickable
    {
        EngineNS.GamePlay.Scene.TtMeshNode mArrowMeshNode;
        public float mArrowRadius = 1.0f;
        [System.ComponentModel.Category("Light")]
        [EGui.Controls.PropertyGrid.PGValueRange(-3.1416f, 3.1416f)]
        [EGui.Controls.PropertyGrid.PGValueChangeStep(3.1416f / 100.0f)]
        public float Yaw { get; set; } = 0;
        [System.ComponentModel.Category("Light")]
        [EGui.Controls.PropertyGrid.PGValueRange(-3.1416f, 3.1416f)]
        [EGui.Controls.PropertyGrid.PGValueChangeStep(3.1416f / 100.0f)]
        public float Roll { get; set; } = /*-1.178f*/-0.698f;

        private GamePlay.TtDirectionLight EnvDirLight = null; 

        public async System.Threading.Tasks.Task<bool> InitializeLightEnv(Graphics.Pipeline.TtViewportSlate viewport, float arrowRasius)
        {
            mArrowRadius = arrowRasius;
            EnvDirLight = viewport.World.DirectionLight;
            var arrowMaterialMesh = await TtEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(RName.GetRName("mesh/base/arrow.ums", RName.ERNameType.Engine));
            var arrowMesh = new Graphics.Mesh.TtMesh();
            var ok = arrowMesh.Initialize(arrowMaterialMesh, Rtti.TtTypeDescGetter<Graphics.Mesh.TtMdfStaticMesh>.TypeDesc);
            if (ok)
            {
                mArrowMeshNode = await GamePlay.Scene.TtMeshNode.AddMeshNode(viewport.World, viewport.World.Root, new GamePlay.Scene.TtMeshNode.TtMeshNodeData(), typeof(GamePlay.TtPlacement), arrowMesh, DVector3.UnitX * 3, Vector3.One, Quaternion.Identity);
                mArrowMeshNode.HitproxyType = Graphics.Pipeline.TtHitProxy.EHitproxyType.Root;
                mArrowMeshNode.NodeData.Name = "PreviewArrow";
                mArrowMeshNode.IsAcceptShadow = false;
                mArrowMeshNode.IsCastShadow = false;
            }

            return true;
        }

        public virtual void TickLogic(float ellapse) { } 
        public virtual void TickRender(float ellapse) 
        {
            if (ImGuiAPI.IsMouseDragging(ImGuiMouseButton_.ImGuiMouseButton_Left, -1) || ImGuiAPI.IsMouseDragging(ImGuiMouseButton_.ImGuiMouseButton_Right, -1))
            {
                if (TtEngine.Instance.InputSystem.IsKeyDown(EngineNS.Bricks.Input.Keycode.KEY_l))
                {
                    var delta = ImGuiAPI.GetMouseDragDelta(ImGuiMouseButton_.ImGuiMouseButton_Left, -1);
                    var delta2 = ImGuiAPI.GetMouseDragDelta(ImGuiMouseButton_.ImGuiMouseButton_Right, -1);
                    delta.X = Math.Max(delta.X, delta.X);
                    delta.Y = Math.Max(delta.Y, delta.Y);

                    var step = 3.1416f / 500.0f;
                    Yaw -= delta.X * step;
                    Roll += delta.Y * step;
                    ImGuiAPI.ResetMouseDragDelta(ImGuiMouseButton_.ImGuiMouseButton_Left);
                    ImGuiAPI.ResetMouseDragDelta(ImGuiMouseButton_.ImGuiMouseButton_Right);
                    mArrowMeshNode.Placement.Scale = new Vector3(Math.Min(mArrowRadius * 0.5f, 2.0f));
                }
            }
            else
            {
                mArrowMeshNode.Placement.Scale = Vector3.Zero;
            }

            var quat = EngineNS.Quaternion.RotationYawPitchRoll(Yaw, 0, Roll);
            EnvDirLight.Direction = quat * Vector3.UnitX;

            var arrowPos = -mArrowRadius * EnvDirLight.Direction;
            mArrowMeshNode.Placement.Position = new DVector3(arrowPos.X, arrowPos.Y, arrowPos.Z);
            mArrowMeshNode.Placement.Quat = quat;
        }
        public virtual void TickBeginFrame(float ellapse)  { }
        public virtual void TickSync(float ellapse) { } 


    }
}
