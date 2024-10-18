using EngineNS.EGui.Controls;
using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Text;

namespace EngineNS.Editor.Forms
{
    public class UWorldOutliner : TtTreeNodeDrawer, IRootForm
    {
        public GamePlay.TtWorld World
        {
            get
            {
                return WorldViewportState.World;
            }
        }
        public EGui.Slate.TtWorldViewportSlate WorldViewportState { get; set; }

        public UWorldOutliner(EGui.Slate.TtWorldViewportSlate viewport, bool regRoot = true)
        {
            WorldViewportState = viewport;
            if (regRoot)
                TtEngine.RootFormManager.RegRootForm(this);

            UpdateAddNodeMenu();
        }

        public void Dispose()
        {

        }
        //List<EGui.UIProxy.MenuItemProxy> mDirContextMenu;
        public virtual async Thread.Async.TtTask<bool> Initialize()
        {
            await Thread.TtAsyncDummyClass.DummyFunc();
            //mDirContextMenu = new List<EGui.UIProxy.MenuItemProxy>()
            //{
            //    new EGui.UIProxy.MenuItemProxy()
            //    {
            //        MenuName = "Goto",
            //        Action = (item, data)=>
            //        {
            //            var node = data.Value.ToObject() as GamePlay.Scene.UNode;
            //            var camera = WorldViewportState.CameraController.Camera;
            //            var radius = (node.AABB.GetMaxSide()) *  5.0f;
            //            camera.LookAtLH(node.Placement.Position - camera.GetDirection().AsDVector() * radius, node.Placement.Position, Vector3.Up);
            //        },
            //    },
            //    new EGui.UIProxy.MenuItemProxy()
            //    {
            //        MenuName = "DoCommand",
            //        Action = (item, data)=>
            //        {
            //            var node = data.Value.ToObject() as GamePlay.Scene.UNode;
            //            node.OnCommand("WorldOutliner");
            //        },
            //    },
            //    new EGui.UIProxy.MenuItemProxy()
            //    {
            //        MenuName = "Delete",
            //        Action = (item, data)=>
            //        {
            //            var node = data.Value.ToObject() as GamePlay.Scene.UNode;
            //            node.Parent = null;
            //        },
            //    },
            //};

            return true;
        }
        public string Title { get; set; } = "WorldOutliner";
        public bool Visible { get; set; } = true;
        public uint DockId { get; set; }
        public ImGuiWindowClass DockKeyClass { get; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        public virtual unsafe void DrawAsChildWindow(in Vector2 size)
        {
            if (ImGuiAPI.BeginChild(Title, in size, ImGuiChildFlags_.ImGuiChildFlags_Borders, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                if (ImGuiAPI.IsWindowDocked())
                {
                    DockId = ImGuiAPI.GetWindowDockID();
                }
                if (World != null)
                {
                    ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Header, EGui.UIProxy.StyleConfig.Instance.TVHeader);
                    ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_HeaderActive, EGui.UIProxy.StyleConfig.Instance.TVHeaderActive);
                    ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_HeaderHovered, EGui.UIProxy.StyleConfig.Instance.TVHeaderHovered);
                    DrawTree(World.Root, 0);
                    ImGuiAPI.PopStyleColor(3);
                }
            }
            if (OnDrawMenu != null)
                OnDrawMenu();
            ImGuiAPI.EndChild();

            
        }
        public unsafe void OnDraw()
        {
            if (Visible == false)
                return;
            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);
            var result = EGui.UIProxy.DockProxy.BeginMainForm(Title, this, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (result)
            {
                if (ImGuiAPI.IsWindowDocked())
                {
                    DockId = ImGuiAPI.GetWindowDockID();
                }
                if (World != null)
                {
                    ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Header, EGui.UIProxy.StyleConfig.Instance.TVHeader);
                    ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_HeaderActive, EGui.UIProxy.StyleConfig.Instance.TVHeaderActive);
                    ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_HeaderHovered, EGui.UIProxy.StyleConfig.Instance.TVHeaderHovered);
                    DrawTree(World.Root, 0);
                    ImGuiAPI.PopStyleColor(3);
                }
            }
            if (OnDrawMenu != null)
                OnDrawMenu();
            EGui.UIProxy.DockProxy.EndMainForm(result);
        }
        protected override bool OnDrawNode(INodeUIProvider provider, int index, int NumOfChild)
        {
            return provider.DrawNode(this, index, NumOfChild);
            //ImGuiTreeNodeFlags_ flags = 0;
            //if (provider.Selected)
            //    flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Selected;
            //var ret = ImGuiAPI.TreeNodeEx(index.ToString(), flags, "");
            //ImGuiAPI.SameLine(0, -3);
            //ImGuiAPI.Text(provider.NodeName);
            //return ret;
        }
        #region PopMenu
        System.Action OnDrawMenu = null;
        GamePlay.Scene.TtNode mAddToNode;
        bool mNodeMenuShow = false;
        bool mAddNodeMenuFilterFocused = false;
        string mAddNodeMenuFilterStr = "";
        public TtMenuItem mAddNodeMenus = new TtMenuItem();
        static void GetNodeNameAndMenuStr(in string menuString, ref string nodeName, ref string menuName)
        {
            menuName = menuString;
            nodeName = menuName;
        }
        private async System.Threading.Tasks.Task<GamePlay.Scene.TtNode> NewNode(Rtti.TtClassMeta i)
        {
            if (mAddToNode == null)
                return null;
            var ntype = Rtti.TtTypeDesc.TypeOf(i.ClassType.TypeString);
            var newNode = Rtti.TtTypeDescManager.CreateInstance(ntype) as GamePlay.Scene.TtSceneActorNode;
            var attrs = newNode.GetType().GetCustomAttributes(typeof(GamePlay.Scene.TtNodeAttribute), false);
            GamePlay.Scene.TtNodeData nodeData = null;
            string prefix = "Node";
            if (attrs.Length > 0)
            {
                nodeData = Rtti.TtTypeDescManager.CreateInstance((attrs[0] as GamePlay.Scene.TtNodeAttribute).NodeDataType) as GamePlay.Scene.TtNodeData;
                prefix = (attrs[0] as GamePlay.Scene.TtNodeAttribute).DefaultNamePrefix;
            }
            await newNode.InitializeNode(World, nodeData, GamePlay.Scene.EBoundVolumeType.Box, typeof(GamePlay.TtPlacement));
            newNode.NodeData.Name = $"{prefix}_{newNode.SceneId}";
            newNode.Parent = mAddToNode;
            return newNode;
        }
        public void UpdateAddNodeMenu()
        {
            mAddNodeMenus = new TtMenuItem();
            var typeDesc = Rtti.TtTypeDescGetter<GamePlay.Scene.TtNode>.TypeDesc;
            var meta = Rtti.TtClassMetaManager.Instance.GetMeta(typeDesc);
            var subClasses = meta.SubClasses;
            foreach (var i in subClasses)
            {
                var atts = i.ClassType.SystemType.GetCustomAttributes(typeof(Bricks.CodeBuilder.ContextMenuAttribute), inherit: false);
                if (atts.Length > 0)
                {
                    var parentMenu = mAddNodeMenus;
                    var att = atts[0] as Bricks.CodeBuilder.ContextMenuAttribute;

                    if (!att.HasKeyString(GamePlay.Scene.TtNode.EditorKeyword))
                        continue;

                    for (var menuIdx = 0; menuIdx < att.MenuPaths.Length; menuIdx++)
                    {
                        var menuStr = att.MenuPaths[menuIdx];
                        string nodeName = null;
                        GetNodeNameAndMenuStr(menuStr, ref nodeName, ref menuStr);
                        if (menuIdx < att.MenuPaths.Length - 1)
                            parentMenu = parentMenu.AddMenuItem(menuStr, null, null);
                        else
                        {
                            parentMenu.AddMenuItem(menuStr, att.FilterStrings, null,
                                (item, sender) =>
                                {
                                    var nu = NewNode(i);
                                });
                        }
                    }
                }
            }
        }
        private void DrawMenu(TtMenuItem item, string filter = "")
        {
            if (!item.FilterCheck(filter))
                return;

            if (item.OnMenuDraw != null)
            {
                item.OnMenuDraw(item, this);
                return;
            }

            if (item.SubMenuItems.Count == 0)
            {
                if (!string.IsNullOrEmpty(item.Text))
                {
                    ImGuiAPI.TreeNodeEx(item.Text, ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen);
                    if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                    {
                        if (item.Action != null)
                        {
                            item.Action(item, null);
                            ImGuiAPI.CloseCurrentPopup();
                        }
                    }
                }
            }
            else
            {
                if (ImGuiAPI.TreeNode(item.Text))
                {
                    for (int menuIdx = 0; menuIdx < item.SubMenuItems.Count; menuIdx++)
                    {
                        DrawMenu(item.SubMenuItems[menuIdx], filter);
                    }
                    ImGuiAPI.TreePop();
                }
            }
        }
        #endregion

        public override void OnNodeUI_RClick(INodeUIProvider provider)
        {
            var node = provider as GamePlay.Scene.TtNode;
            if (node == null)
            {
                mNodeMenuShow = false;
                OnDrawMenu = null;
                return;
            }
            var scene = provider as GamePlay.Scene.TtScene;
            OnDrawMenu = async () =>
            {
                if (ImGuiAPI.BeginPopupContextWindow(null, ImGuiPopupFlags_.ImGuiPopupFlags_MouseButtonRight))
                {
                    mAddToNode = node;
                    mNodeMenuShow = true;
                    DrawBaseMenu(node);
                    ImGuiAPI.EndPopup();
                }
                else
                {
                    //mAddToNode = null;
                    if (mNodeMenuShow)
                    {
                        OnDrawMenu = null;
                    }
                    mNodeMenuShow = false;
                }
            };
        }

        protected virtual void DrawBaseMenu(GamePlay.Scene.TtNode node)
        {
            if (ImGuiAPI.MenuItem($"Goto", null, false, true))
            {
                var camera = WorldViewportState.CameraController.Camera;
                var radius = (node.AABB.GetMaxSide()) * 5.0f;
                camera.LookAtLH(node.Placement.Position - camera.GetDirection().AsDVector() * radius, node.Placement.Position, Vector3.Up);
            }
            if (ImGuiAPI.MenuItem($"DoCommand", null, false, true))
            {
                node.OnCommand("WorldOutliner");
            }
            if (ImGuiAPI.BeginMenu("AddChild", true))
            {
                var drawList = ImGuiAPI.GetWindowDrawList();
                EGui.UIProxy.SearchBarProxy.OnDraw(ref mAddNodeMenuFilterFocused, in drawList, "search item", ref mAddNodeMenuFilterStr, -1);
                for (var childIdx = 0; childIdx < mAddNodeMenus.SubMenuItems.Count; childIdx++)
                    DrawMenu(mAddNodeMenus.SubMenuItems[childIdx], mAddNodeMenuFilterStr.ToLower());

                ImGuiAPI.EndMenu();
            }
            if (ImGuiAPI.MenuItem($"Delete", null, false, true))
            {
                if (World.Root != node)
                {
                    node.Parent = null;
                }
            }
        }
        public List<GamePlay.Scene.TtNode> SelectedNodes = new List<GamePlay.Scene.TtNode>();
        public override void OnNodeUI_LClick(INodeUIProvider provider)
        {
            //var ctrlKeyDown = TtEngine.Instance.InputSystem.IsCtrlKeyDown();
            //if (ctrlKeyDown)
            //{

            //}
            //else
            //{
            //    for(int i=0; i<mSelectNodes.Count; i++)
            //    {
            //        mSelectNodes[i].Selected = false;
            //    }
            //    mSelectNodes.Clear();
            //    mSelectNodes.Add(provider);
            //    provider.Selected = true;
            //    WorldViewportState.OnHitproxySelected((GamePlay.Scene.UNode)provider);
            //}

            //var appliction = TtEngine.Instance.GfxDevice.MainWindow as EngineNS.Editor.UMainEditorApplication;
            //if (appliction == null)
            //    return;
            //appliction.mMainInspector.PropertyGrid.Target = provider;
            //appliction.WorldViewportSlate.OnHitproxySelected((GamePlay.Scene.UNode)provider);

            var node = provider as GamePlay.Scene.TtNode;
            if (node == null)
                return;

            if (ImGuiAPI.IsKeyDown((ImGuiKey)Bricks.Input.Scancode.SCANCODE_LCTRL))
            {
                node.Selected = !node.Selected;
                if (node.Selected == false)
                {
                    SelectedNodes.Remove(node);
                }
                else
                {
                    if (SelectedNodes.Contains(node) == false)
                    {
                        SelectedNodes.Add(node);
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
                node.Selected = true;
                SelectedNodes.Add(node);
            }

            WorldViewportState.OnHitproxySelectedMulti(true, SelectedNodes.ToArray());
        }
    }
}
