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
    }
    public class UTreeNodeDrawer
    {
        public void DrawTree(INodeUIProvider provider, int index)
        {
            var drawed = OnDrawNode(provider, index);
            AfterNodeShow(provider, index);
            if (drawed)
            {   
                int count = provider.NumOfChildUI();
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
        protected virtual bool OnDrawNode(INodeUIProvider provider, int index)
        {
            return ImGuiAPI.TreeNode(index.ToString(), provider.NodeName);
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
    }
}
