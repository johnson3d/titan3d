﻿using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.PhysicsCore
{
    public class TtPhyMaterialEditor : Editor.IAssetEditor, IRootForm
    {
        public RName AssetName { get; set; }
        public bool Visible { get; set; }
        public uint DockId { get; set; }
        ImGuiWindowClass mDockKeyClass;
        public ImGuiWindowClass DockKeyClass => mDockKeyClass;
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        public TtPhyMaterial Material;
        public EGui.Controls.PropertyGrid.PropertyGrid MaterialPropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        ~TtPhyMaterialEditor()
        {
            Dispose();
        }
        public void Dispose()
        {
            Material = null;
            MaterialPropGrid.Target = null;
        }
        public async Thread.Async.TtTask<bool> Initialize()
        {
            await MaterialPropGrid.Initialize();
            return true;
        }
        public IRootForm GetRootForm()
        {
            return this;
        }
        public float LoadingPercent { get; set; } = 1.0f;
        public string ProgressText { get; set; } = "Loading";
        public async Thread.Async.TtTask<bool> OpenEditor(Editor.UMainEditorApplication mainEditor, RName name, object arg)
        {
            AssetName = name;
            Material = await TtEngine.Instance.PhyModule.PhyContext.PhyMaterialManager.CreateMaterial(name);
            if (Material == null)
                return false;

            MaterialPropGrid.Target = Material;
            Visible = true;
            return true;
        }
        public void OnCloseEditor()
        {
            Dispose();
        }
        bool mDockInitialized = false;
        protected void ResetDockspace(bool force = false)
        {
            var pos = ImGuiAPI.GetCursorPos();
            var id = ImGuiAPI.GetID(AssetName.Name + "_Dockspace");
            mDockKeyClass.ClassId = id;
            ImGuiAPI.DockSpace(id, Vector2.Zero, ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None, mDockKeyClass);
            if (mDockInitialized && !force)
                return;
            ImGuiAPI.DockBuilderRemoveNode(id);
            ImGuiAPI.DockBuilderAddNode(id, ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None);
            ImGuiAPI.DockBuilderSetNodePos(id, pos);
            ImGuiAPI.DockBuilderSetNodeSize(id, Vector2.One);
            mDockInitialized = true;

            var rightId = id;
            uint leftId = 0;
            ImGuiAPI.DockBuilderSplitNode(rightId, ImGuiDir.ImGuiDir_Left, 0.2f, ref leftId, ref rightId);

            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Left", mDockKeyClass), leftId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Right", mDockKeyClass), rightId);
            ImGuiAPI.DockBuilderFinish(id);
        }
        public Vector2 WindowPos;
        public Vector2 WindowSize = new Vector2(800, 600);
        public unsafe void OnDraw()
        {
            if (Visible == false || Material == null)
                return;

            var pivot = new Vector2(0);
            ImGuiAPI.SetNextWindowSize(in WindowSize, ImGuiCond_.ImGuiCond_FirstUseEver);
            var result = EGui.UIProxy.DockProxy.BeginMainForm(GetWindowsName(), this, ImGuiWindowFlags_.ImGuiWindowFlags_None |
                ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings);
            if (result)
            {
                if (ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_RootAndChildWindows))
                {
                    var mainEditor = TtEngine.Instance.GfxDevice.SlateApplication as Editor.UMainEditorApplication;
                    if (mainEditor != null)
                        mainEditor.AssetEditorManager.CurrentActiveEditor = this;
                }
                WindowPos = ImGuiAPI.GetWindowPos();
                WindowSize = ImGuiAPI.GetWindowSize();
                DrawToolBar();
            }
            ResetDockspace();
            EGui.UIProxy.DockProxy.EndMainForm(result);

            DrawLeft();
            DrawRight();
        }
        protected unsafe void DrawToolBar()
        {
            var btSize = Vector2.Zero;
            if (EGui.UIProxy.CustomButton.ToolButton("Save", in btSize))
            {
                Material.SaveAssetTo(Material.AssetName);
                var unused = TtEngine.Instance.PhyModule.PhyContext.PhyMaterialManager.ReloadMaterial(Material.AssetName);
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Reload", in btSize))
            {
                var unused = TtEngine.Instance.PhyModule.PhyContext.PhyMaterialManager.ReloadMaterial(Material.AssetName);
            }
        }
        bool mLeftShow = true;
        protected unsafe void DrawLeft()
        {
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Left", ref mLeftShow, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                if (ImGuiAPI.CollapsingHeader("MaterialProperty", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                {
                    MaterialPropGrid.OnDraw(true, false, false);
                }
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        protected unsafe void DrawRight()
        {
            
        }

        public void OnEvent(in Bricks.Input.Event e)
        {
            //throw new NotImplementedException();
        }

        public string GetWindowsName()
        {
            return Material.AssetName.Name;
        }
    }
}
