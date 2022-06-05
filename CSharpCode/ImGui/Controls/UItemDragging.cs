using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui.Controls
{
    public class UItemDragging : Graphics.Pipeline.IRootForm
    {
        public UItemDragging()
        {
            Editor.UMainEditorApplication.RegRootForm(this);
        }

        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            return true;
        }
        public void Cleanup() 
        { 
            Editor.UMainEditorApplication.UnregRootForm(this); 
        }
        public bool Visible { get; set; } = true;
        public uint DockId { get; set; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        public unsafe void OnDraw()
        {
            if (CurItem == null)
                return;

            if (ImGuiAPI.IsMouseReleased(ImGuiMouseButton_.ImGuiMouseButton_Left))
            {
                if (CurItem.OnRelease != null)
                    CurItem.OnRelease();
                CurItem = null;

                return;
            }
            //var size = new Vector2();
            ImGuiAPI.SetNextWindowSize(CurItem.Size, ImGuiCond_.ImGuiCond_Always);
            var msPt = ImGuiAPI.GetMousePos();
            ImGuiAPI.SetNextWindowPos(in msPt, ImGuiCond_.ImGuiCond_Always, new Vector2(0.5f));
            if (ImGuiAPI.Begin("ItemDragging", null, ImGuiWindowFlags_.ImGuiWindowFlags_NoCollapse | 
                ImGuiWindowFlags_.ImGuiWindowFlags_NoDocking |
                ImGuiWindowFlags_.ImGuiWindowFlags_NoNav | ImGuiWindowFlags_.ImGuiWindowFlags_NoDecoration))
            {
                var cmdlst = ImGuiAPI.GetWindowDrawList();
                var start = ImGuiAPI.GetWindowPos();
                //CurItem.Icon.OnDraw(ref cmdlst, start, start + CurItem.Icon.Size, 0);
                CurItem.AMeta.OnDraw(in cmdlst, in CurItem.Size, CurItem.Browser);
            }
            ImGuiAPI.End();
        }
        public class UItem
        {
            public object Tag;
            public Vector2 Size;
            public IO.IAssetMeta AMeta;
            public System.Action OnRelease;
            public UContentBrowser Browser;
        }

        protected UItem CurItem;
        public void SetCurItem(UItem item, System.Action action)
        {
            CurItem = item;
            if (CurItem != null)
                CurItem.OnRelease = action;
        }
        public UItem GetCurItem()
        {
            return CurItem;
        }
    }
}
