﻿using System;
using System.Collections.Generic;
using System.Text;
using SDL2;

namespace EngineNS.Editor.Forms
{
    public class UMeshPrimitiveEditor : Editor.IAssetEditor, ITickable, Graphics.Pipeline.IRootForm
    {
        public RName AssetName { get; set; }
        protected bool mVisible = true;
        public bool Visible { get => mVisible; set => mVisible = value; }
        public uint DockId { get; set; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        public Graphics.Mesh.CMeshPrimitives Mesh;
        public Bricks.CodeBuilder.ShaderNode.UPreviewViewport PreviewViewport = new Bricks.CodeBuilder.ShaderNode.UPreviewViewport();
        public EGui.Controls.PropertyGrid.PropertyGrid MeshPropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        ~UMeshPrimitiveEditor()
        {
            Cleanup();
        }
        public void Cleanup()
        {
            Mesh = null;
            PreviewViewport?.Cleanup();
            PreviewViewport = null;
            MeshPropGrid.Target = null;
        }
        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            await MeshPropGrid.Initialize();
            return true;
        }
        public Graphics.Pipeline.IRootForm GetRootForm()
        {
            return this;
        }
        protected async System.Threading.Tasks.Task Initialize_PreviewMaterialInstance(Bricks.CodeBuilder.ShaderNode.UPreviewViewport viewport, Graphics.Pipeline.USlateApplication application, Graphics.Pipeline.IRenderPolicy policy, float zMin, float zMax)
        {
            viewport.RenderPolicy = policy;

            await viewport.RenderPolicy.Initialize(1, 1);

            viewport.CameraController.Camera = viewport.RenderPolicy.GBuffers.Camera;

            var materials = new Graphics.Pipeline.Shader.UMaterial[Mesh.mCoreObject.GetAtomNumber()];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = await UEngine.Instance.GfxDevice.MaterialManager.GetMaterial(UEngine.Instance.Config.DefaultMaterial);
            }
            var mesh = new Graphics.Mesh.UMesh();
            
            var ok = mesh.Initialize(Mesh, materials, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
            if (ok)
            {
                mesh.SetWorldMatrix(ref Matrix.mIdentity);
                viewport.RenderPolicy.VisibleMeshes.Add(mesh);
            }

            var aabb = mesh.MaterialMesh.Mesh.mCoreObject.mAABB;
            float radius = aabb.GetMaxSide();
            BoundingSphere sphere;
            sphere.Center = aabb.GetCenter();
            sphere.Radius = radius;
            policy.GBuffers.Camera.AutoZoom(ref sphere);
        }
        public async System.Threading.Tasks.Task<bool> OpenEditor(UMainEditorApplication mainEditor, RName name, object arg)
        {
            AssetName = name;
            Mesh = await UEngine.Instance.GfxDevice.MeshPrimitiveManager.GetMeshPrimitive(name);
            if (Mesh == null)
                return false;

            PreviewViewport.Title = $"Mesh:{name}";
            PreviewViewport.OnInitialize = Initialize_PreviewMaterialInstance;
            await PreviewViewport.Initialize(UEngine.Instance.GfxDevice.MainWindow, new Graphics.Pipeline.Mobile.UMobileEditorFSPolicy(), 0, 1);

            MeshPropGrid.Target = Mesh;
            UEngine.Instance.TickableManager.AddTickable(this);
            return true;
        }
        public void OnCloseEditor()
        {
            UEngine.Instance.TickableManager.RemoveTickable(this);
            Cleanup();
        }
        public float LeftWidth = 0;
        public Vector2 WindowPos;
        public Vector2 WindowSize = new Vector2(800, 600);
        public unsafe void OnDraw()
        {
            if (Visible == false || Mesh == null)
                return;

            var pivot = new Vector2(0);
            ImGuiAPI.SetNextWindowSize(ref WindowSize, ImGuiCond_.ImGuiCond_FirstUseEver);
            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);
            if (ImGuiAPI.Begin(Mesh.AssetName.Name, ref mVisible, ImGuiWindowFlags_.ImGuiWindowFlags_None |
                ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings))
            {
                if (ImGuiAPI.IsWindowDocked())
                {
                    DockId = ImGuiAPI.GetWindowDockID();
                }
                if (ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_RootAndChildWindows))
                {
                    var mainEditor = UEngine.Instance.GfxDevice.MainWindow as Editor.UMainEditorApplication;
                    if (mainEditor != null)
                        mainEditor.AssetEditorManager.CurrentActiveEditor = this;
                }
                WindowPos = ImGuiAPI.GetWindowPos();
                WindowSize = ImGuiAPI.GetWindowSize();
                DrawToolBar();
                //var sz = new Vector2(-1);
                //ImGuiAPI.BeginChild("Client", ref sz, false, ImGuiWindowFlags_.)
                ImGuiAPI.Separator();
                ImGuiAPI.Columns(2, null, true);
                if (LeftWidth == 0)
                {
                    ImGuiAPI.SetColumnWidth(0, 300);
                }
                LeftWidth = ImGuiAPI.GetColumnWidth(0);
                var min = ImGuiAPI.GetWindowContentRegionMin();
                var max = ImGuiAPI.GetWindowContentRegionMin();

                DrawLeft(ref min, ref max);
                ImGuiAPI.NextColumn();

                DrawRight(ref min, ref max);
                ImGuiAPI.NextColumn();

                ImGuiAPI.Columns(1, null, true);
            }
            ImGuiAPI.End();
        }
        protected unsafe void DrawToolBar()
        {
            var btSize = new Vector2(64, 64);
            if (ImGuiAPI.Button("Save", ref btSize))
            {
                //Mesh.SaveAssetTo(Mesh.AssetName);
                //var unused = UEngine.Instance.GfxDevice.MaterialInstanceManager.ReloadMaterialInstance(Mesh.AssetName);

                USnapshot.Save(Mesh.AssetName, Mesh.GetAMeta(), PreviewViewport.RenderPolicy.GetFinalShowRSV(), UEngine.Instance.GfxDevice.RenderContext.mCoreObject.GetImmCommandList());
            }
            ImGuiAPI.SameLine(0, -1);
            if (ImGuiAPI.Button("Reload", ref btSize))
            {

            }
            ImGuiAPI.SameLine(0, -1);
            if (ImGuiAPI.Button("Undo", ref btSize))
            {
                
            }
            ImGuiAPI.SameLine(0, -1);
            if (ImGuiAPI.Button("Redo", ref btSize))
            {
                
            }
        }
        protected unsafe void DrawLeft(ref Vector2 min, ref Vector2 max)
        {
            var sz = new Vector2(-1);
            if (ImGuiAPI.BeginChild("LeftView", ref sz, true, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                if (ImGuiAPI.CollapsingHeader("MeshProperty", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                {
                    MeshPropGrid.OnDraw(true, false, false);
                }
            }
            ImGuiAPI.EndChild();
        }
        protected unsafe void DrawRight(ref Vector2 min, ref Vector2 max)
        {
            PreviewViewport.VieportType = Graphics.Pipeline.UViewportSlate.EVieportType.ChildWindow;
            PreviewViewport.OnDraw();
        }

        public void OnEvent(ref SDL.SDL_Event e)
        {
            //throw new NotImplementedException();
        }
        #region Tickable
        public void TickLogic(int ellapse)
        {
            PreviewViewport.TickLogic(ellapse);
        }
        public void TickRender(int ellapse)
        {
            PreviewViewport.TickRender(ellapse);
        }
        public void TickSync(int ellapse)
        {
            PreviewViewport.TickSync(ellapse);
        }
        #endregion
    }
}

namespace EngineNS.Graphics.Mesh
{
    [Editor.UAssetEditor(EditorType = typeof(Editor.Forms.UMeshPrimitiveEditor))]
    public partial class CMeshPrimitives
    {
    }
}