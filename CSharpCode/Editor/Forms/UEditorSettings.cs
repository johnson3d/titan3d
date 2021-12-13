using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Editor.Forms
{
    public class UEditorSettings : Graphics.Pipeline.IRootForm
    {
        public UEditorSettings()
        {
            Editor.UMainEditorApplication.RegRootForm(this);
        }
        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            await RPolicyPropGrid.Initialize();
            return true;
        }
        public void Cleanup() { }
        public bool Visible { get; set; } = true;
        public uint DockId { get; set; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        public EGui.Slate.UWorldViewportSlate ViewportSlate { get; set; }
        public EGui.Controls.PropertyGrid.PropertyGrid RPolicyPropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        public unsafe void OnDraw()
        {
            if (Visible == false)
                return;
            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);

            Vector2 size = new Vector2(0, 0);
            if (ImGuiAPI.Begin("EditorSettings", null, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                DockId = ImGuiAPI.GetWindowDockID();
                if (ImGuiAPI.CollapsingHeader("Camera", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_DefaultOpen))
                {
                    float v = ViewportSlate.CameraMoveSpeed;
                    ImGuiAPI.SliderFloat("KeyMove", ref v, 1.0f, 150.0f, "%.3f", ImGuiSliderFlags_.ImGuiSliderFlags_None);
                    ViewportSlate.CameraMoveSpeed = v;

                    v = ViewportSlate.CameraMouseWheelSpeed;
                    ImGuiAPI.InputFloat("WheelMove", ref v, 0.1f, 1.0f, "%.3f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                    ViewportSlate.CameraMouseWheelSpeed = v;

                    var zN = ViewportSlate.CameraController.Camera.mCoreObject.mZNear;
                    var zF = ViewportSlate.CameraController.Camera.mCoreObject.mZFar;                    
                    ImGuiAPI.SliderFloat("ZNear", ref zN, 0.1f, 100.0f, "%.1f", ImGuiSliderFlags_.ImGuiSliderFlags_None);
                    ImGuiAPI.SliderFloat("ZFar", ref zF, 10.0f, 10000.0f, "%.1f", ImGuiSliderFlags_.ImGuiSliderFlags_None);
                    if (zN != ViewportSlate.CameraController.Camera.mCoreObject.mZNear ||
                        zF != ViewportSlate.CameraController.Camera.mCoreObject.mZFar)
                    {
                        ViewportSlate.CameraController.Camera.SetZRange(zN, zF);
                    }

                    var camPos = ViewportSlate.CameraController.Camera.mCoreObject.GetPosition();
                    var saved = camPos;
                    ImGuiAPI.InputDouble($"X", ref camPos.X, 0.1, 10.0, "%.2f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                    ImGuiAPI.InputDouble($"Y", ref camPos.Y, 0.1, 10.0, "%.2f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                    ImGuiAPI.InputDouble($"Z", ref camPos.Z, 0.1, 10.0, "%.2f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                    if (saved != camPos)
                    {
                        var lookAt = ViewportSlate.CameraController.Camera.mCoreObject.GetLookAt();
                        var up = ViewportSlate.CameraController.Camera.mCoreObject.GetUp();
                        ViewportSlate.CameraController.Camera.mCoreObject.LookAtLH(in camPos, lookAt - saved + camPos, up);
                    }
                }
                if (ImGuiAPI.CollapsingHeader("RenderPolicy", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                {
                    if (RPolicyPropGrid.Target != ViewportSlate.RenderPolicy)
                    {
                        RPolicyPropGrid.Target = ViewportSlate.RenderPolicy;
                    }
                    RPolicyPropGrid.OnDraw(true, false, false);
                }
            }
            ImGuiAPI.End();
        }
    }
}
