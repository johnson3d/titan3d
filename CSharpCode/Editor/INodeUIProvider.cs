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
        bool DrawNode(UTreeNodeDrawer tree, int index, int NumOfChild);
        GamePlay.UWorld GetWorld();
    }
    public class UTreeNodeDrawer
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
        protected virtual void OnNodeUI_LClick(INodeUIProvider provider)
        {

        }
        protected virtual void OnNodeUI_RClick(INodeUIProvider provider)
        {

        }
        protected virtual void OnNodeUI_Activated(INodeUIProvider provider)
        {

        }
        protected virtual bool OnDrawNode(INodeUIProvider provider, int index, int NumOfChild)
        {
            return provider.DrawNode(this, index, NumOfChild);
            //return ImGuiAPI.TreeNode(index.ToString(), provider.NodeName);
        }
        protected virtual void AfterNodeShow(INodeUIProvider provider, int index)
        {
            if (ImGuiAPI.IsItemActivated())
            {
                OnNodeUI_Activated(provider);
            }
            if (ImGuiAPI.IsItemDeactivated())
            {
                //OnNodeUI_Activated(provider);
            }
            if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
            {
                OnNodeUI_LClick(provider);
            }
            if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Right))
            {
                OnNodeUI_RClick(provider);
            }
        }
    }
}

namespace EngineNS.GamePlay.Scene
{
    public partial class UNode : Editor.INodeUIProvider
    {
        public bool Selected { get; set; }
        public int NumOfChildUI()
        {
            return Children.Count;
        }
        public Editor.INodeUIProvider GetChildUI(int index)
        {
            return Children[index];
        }
        public virtual bool DrawNode(Editor.UTreeNodeDrawer tree, int index, int NumOfChild)
        {
            ImGuiTreeNodeFlags_ flags = 0;
            if (this.Selected)
                flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Selected;
            if (NumOfChild == 0)
            {
                var ret = ImGuiAPI.TreeNodeEx(index.ToString(), flags | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf, "");

                ImGuiAPI.SameLine(0, -3);
                ImGuiAPI.Text(string.IsNullOrEmpty(NodeName) ? "EmptyName" : NodeName);
                return ret;
            }
            else
            {
                var ret = ImGuiAPI.TreeNodeEx(index.ToString(), flags, "");

                ImGuiAPI.SameLine(0, -3);
                ImGuiAPI.Text(string.IsNullOrEmpty(NodeName) ? "EmptyName" : NodeName);
                return ret;
            }
        }
    }
}
