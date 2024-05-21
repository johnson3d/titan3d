using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui.Controls
{
    public class UItemDragging : IRootForm
    {
        public UItemDragging()
        {
            UEngine.RootFormManager.RegRootForm(this);
        }

        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
            return true;
        }
        public void Dispose() 
        {
            UEngine.RootFormManager.UnregRootForm(this);
        }
        protected bool mVisible = true;
        public bool Visible { get => mVisible; set => mVisible = value; }
        uint mDockId = uint.MaxValue;
        public uint DockId { get => mDockId; set => mDockId = value; }
        protected ImGuiWindowClass mDockKeyClass;
        public ImGuiWindowClass DockKeyClass => mDockKeyClass;
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
            var result = EGui.UIProxy.DockProxy.BeginMainForm("ItemDragging", this, ImGuiWindowFlags_.ImGuiWindowFlags_NoCollapse | ImGuiWindowFlags_.ImGuiWindowFlags_NoResize |
                ImGuiWindowFlags_.ImGuiWindowFlags_NoDocking | ImGuiWindowFlags_.ImGuiWindowFlags_NoTitleBar | ImGuiWindowFlags_.ImGuiWindowFlags_NoBackground |
                ImGuiWindowFlags_.ImGuiWindowFlags_NoNav | ImGuiWindowFlags_.ImGuiWindowFlags_NoDecoration);
            if (result)
            {
                var cmdlst = ImGuiAPI.GetWindowDrawList();
                var start = ImGuiAPI.GetWindowPos();
                //CurItem.Icon.OnDraw(ref cmdlst, start, start + CurItem.Icon.Size, 0);
                CurItem.AMeta.OnDraw(in cmdlst, in CurItem.Size, CurItem.Browser);
            }
            EGui.UIProxy.DockProxy.EndMainForm(result);
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
