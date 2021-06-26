using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Editor.Forms
{
    public class UWorldOutliner : UTreeNodeDrawer, Graphics.Pipeline.IRootForm
    {
        GamePlay.UWorld mWorld;
        public UWorldOutliner()
        {
            Editor.UMainEditorApplication.RegRootForm(this);
        }
        public bool Visible { get; set; } = true;
        public uint DockId { get; set; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        public void TestUWorldOutliner(UMainEditorApplication application)
        {
            mWorld = application.WorldViewportSlate.World;
            
            var scene = new GamePlay.Scene.UScene(new GamePlay.Scene.USceneData() { Name = "TestScene" });
            scene.Parent = mWorld.Root;

            var node = scene.NewNode(typeof(GamePlay.Scene.UNode), new GamePlay.Scene.UNodeData() { Name = "n1" }, GamePlay.Scene.EBoundVolumeType.Box, typeof(GamePlay.UPlacement));
            node.Parent = scene;
            node.Placement.Position = Vector3.UnitXYZ;

            node = scene.NewNode(typeof(GamePlay.Scene.UNode), new GamePlay.Scene.UNodeData() { Name = "n2" }, GamePlay.Scene.EBoundVolumeType.Box, typeof(GamePlay.UPlacement));
            node.Parent = scene;
            node.Placement.Position = Vector3.UnitXYZ * 2;

            var curNode = node;
            node = scene.NewNode(typeof(GamePlay.Scene.UNode), new GamePlay.Scene.UNodeData() { Name = "n3" }, GamePlay.Scene.EBoundVolumeType.Box, typeof(GamePlay.UPlacement));
            node.Parent = curNode;
            node.Placement.Position = Vector3.UnitXYZ;
            node = scene.NewNode(typeof(GamePlay.Scene.UNode), new GamePlay.Scene.UNodeData() { Name = "n4" }, GamePlay.Scene.EBoundVolumeType.Box, typeof(GamePlay.UPlacement));
            node.Parent = curNode;
            node.Placement.Position = Vector3.UnitXYZ * 2;
        }
        public unsafe void OnDraw()
        {
            if (Visible == false)
                return;
            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);
            if (ImGuiAPI.Begin("WorldOutliner", null, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                if (ImGuiAPI.IsWindowDocked())
                {
                    DockId = ImGuiAPI.GetWindowDockID();
                }
                if (mWorld == null)
                    return;
                DrawTree(mWorld.Root, 0);
            }
            if (OnDrawMenu != null)
                OnDrawMenu();
            ImGuiAPI.End();
        }
        public List<INodeUIProvider> SelectedNodes = new List<INodeUIProvider>();
        protected override void AfterNodeShow(INodeUIProvider provider, int index)
        {
            if (ImGuiAPI.IsItemActivated())
            {
                OnNodeUI_Activated(provider);
                //if (ImGuiAPI.IsKeyDown((int)SDL2.SDL.SDL_Scancode.SDL_SCANCODE_DELETE))
                //{
                //    int xxx = 0;
                //}
            }
            if (ImGuiAPI.IsItemDeactivated())
            {
            }
            if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
            {
                OnNodeUI_LClick(provider);
            }
            if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Right))
            {
                OnNodeUI_RClick(provider);
            }
            if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None) && ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Left))
            {
                //这里考虑一下单选多选的问题
                if (ImGuiAPI.IsKeyDown((int)SDL2.SDL.SDL_Scancode.SDL_SCANCODE_LCTRL))
                {
                    provider.Selected = !provider.Selected;
                    if (provider.Selected == false)
                    {
                        SelectedNodes.Remove(provider);
                    }
                    else
                    {
                        if (SelectedNodes.Contains(provider) == false)
                        {
                            SelectedNodes.Add(provider);
                        }
                    }
                }
                else
                {
                    foreach (var i in SelectedNodes)
                    {
                        i.Selected = false;
                    }
                    SelectedNodes.Clear();
                    provider.Selected = true;
                    SelectedNodes.Add(provider);
                }
            }   
        }
        protected override bool OnDrawNode(INodeUIProvider provider, int index)
        {
            ImGuiTreeNodeFlags_ flags = 0;
            if (provider.Selected)
                flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Selected;
            var ret = ImGuiAPI.TreeNodeEx(index.ToString(), flags, "");
            ImGuiAPI.SameLine(0, -3);
            ImGuiAPI.Text(provider.NodeName);
            return ret;
        }
        System.Action OnDrawMenu = null;
        bool mNodeMenuShow = false;
        protected override void OnNodeUI_RClick(INodeUIProvider provider)
        {
            var node = provider as GamePlay.Scene.UNode;
            if (node == null)
            {
                mNodeMenuShow = false;
                OnDrawMenu = null;
                return;
            }
            var scene = provider as GamePlay.Scene.UScene;
            OnDrawMenu = () =>
            {
                if (ImGuiAPI.BeginPopupContextWindow(null, ImGuiPopupFlags_.ImGuiPopupFlags_MouseButtonRight))
                {
                    mNodeMenuShow = true;
                    if (ImGuiAPI.BeginMenu("AddChild", true))
                    {
                        var typeDesc = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(typeof(GamePlay.Scene.UNode).FullName);
                        var meta = Rtti.UClassMetaManager.Instance.GetMeta(typeDesc);
                        var subClasses = meta.SubClasses;
                        if (ImGuiAPI.MenuItem(meta.ClassType.FullName, null, false, true))
                        {
                            var newNode = node.ParentScene.NewNode(meta.ClassType.TypeString, new GamePlay.Scene.UNodeData(), GamePlay.Scene.EBoundVolumeType.Box, typeof(GamePlay.UPlacement));
                            newNode.NodeData.Name = $"Node_{newNode.Id}";
                            newNode.Parent = node;
                        }
                        foreach (var i in subClasses)
                        {
                            if (ImGuiAPI.MenuItem(i.ClassType.FullName, null, false, true))
                            {
                                var newNode = node.ParentScene.NewNode(meta.ClassType.TypeString, new GamePlay.Scene.UNodeData(), GamePlay.Scene.EBoundVolumeType.Box, typeof(GamePlay.UPlacement));
                                newNode.NodeData.Name = $"Node_{newNode.Id}";
                                newNode.Parent = node;
                            }
                        }
                        ImGuiAPI.EndMenu();
                    }

                    if (ImGuiAPI.MenuItem($"Delete", null, false, true))
                    {
                        if (mWorld.Root != node)
                        {
                            node.Parent = null;
                        }
                    }
                    ImGuiAPI.EndPopup();
                }
                else
                {
                    if (mNodeMenuShow)
                    {
                        OnDrawMenu = null;
                    }
                    mNodeMenuShow = false;
                }
            };
        }
        protected override void OnNodeUI_LClick(INodeUIProvider provider)
        {
            var appliction = UEngine.Instance.GfxDevice.MainWindow as EngineNS.Editor.UMainEditorApplication;
            if (appliction == null)
                return;
            appliction.mMainInspector.PropertyGrid.SingleTarget = provider;
        }
    }
}
