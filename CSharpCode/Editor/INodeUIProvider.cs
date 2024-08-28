using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Editor
{
    public interface INodeUIProvider
    {
        int NumOfChildUI();
        INodeUIProvider GetChildUI(int index);
        string NodeName { get; }
        bool Selected { get; set; }
        bool DrawNode(TtTreeNodeDrawer tree, int index, int NumOfChild);
        GamePlay.UWorld GetWorld();
    }
    public class TtTreeNodeDrawer
    {
        public void DrawTree(INodeUIProvider provider, int index)
        {
            int count = provider.NumOfChildUI();
            var drawed = OnDrawNode(provider, index, count);
            AfterNodeShow(provider, index);
            if (drawed)
            {
                for (int i = 0; i < count; i++)
                {
                    var cld = provider.GetChildUI(i);
                    DrawTree(cld, i);
                }
                ImGuiAPI.TreePop();
            }
        }
        public virtual void OnNodeUI_LClick(INodeUIProvider provider)
        {

        }
        public virtual void OnNodeUI_RClick(INodeUIProvider provider)
        {

        }
        public virtual void OnNodeUI_Activated(INodeUIProvider provider)
        {

        }
        protected virtual bool OnDrawNode(INodeUIProvider provider, int index, int NumOfChild)
        {
            return provider.DrawNode(this, index, NumOfChild);
            //return ImGuiAPI.TreeNode(index.ToString(), provider.NodeName);
        }
        public virtual void AfterNodeShow(INodeUIProvider provider, int index)
        {

        }
    }
}

namespace EngineNS.GamePlay.Scene
{
    public partial class TtNode : Editor.INodeUIProvider
    {
        public virtual void OnCommand(object cmd)
        {

        }
        [System.ComponentModel.Browsable(false)]
        public bool Selected { get; set; }
        public int NumOfChildUI()
        {
            return Children.Count;
        }
        public Editor.INodeUIProvider GetChildUI(int index)
        {
            return Children[index];
        }
        public virtual bool DrawNode(Editor.TtTreeNodeDrawer tree, int index, int NumOfChild)
        {
            ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnArrow | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_SpanFullWidth;
            if (this.Selected)
                flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Selected;
            bool ret = false;
            var name = (string.IsNullOrEmpty(NodeName) ? "EmptyName" : NodeName) + "##" + index;
            if (NumOfChild == 0)
            {
                ret = ImGuiAPI.TreeNodeEx(name, flags | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf);
                if (ImGuiAPI.IsItemActivated())
                {
                    tree.OnNodeUI_Activated(this);
                }
                if (ImGuiAPI.IsItemDeactivated())
                {
                }
                if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                {
                    tree.OnNodeUI_LClick(this);
                }
                if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Right))
                {
                    tree.OnNodeUI_RClick(this);
                }
            }
            else
            {
                ret = ImGuiAPI.TreeNodeEx(name, flags);
                if (ImGuiAPI.IsItemActivated())
                {
                    tree.OnNodeUI_Activated(this);
                }
                if (ImGuiAPI.IsItemDeactivated())
                {
                }
                if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                {
                    tree.OnNodeUI_LClick(this);
                }
                if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Right))
                {
                    tree.OnNodeUI_RClick(this);
                }
            }
            return ret;
        }
    }
}
