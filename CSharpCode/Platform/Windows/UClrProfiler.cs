using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Windows
{
    public class UClrProfiler : IRootForm
    {
        public UClrProfiler()
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

        }

        public bool Visible { get; set; } = true;
        public uint DockId { get; set; }
        public ImGuiWindowClass DockKeyClass { get; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        public List<ClrString> mClrLogs = new List<ClrString>();
        protected void UpdateLogs()
        {
            mClrLogs.Clear();
            ClrString clrStr = new ClrString();
            var ok = ClrLogger.PopLogInfo(ref clrStr);
            while (ok)
            {
                if (clrStr.mType == EClrLogStringType.ObjectAlloc)
                {
                    mClrLogs.Add(clrStr);
                }
                ok = ClrLogger.PopLogInfo(ref clrStr);
            }
        }
        public unsafe void OnDraw()
        {
            if (Visible == false)
                return;
            
            Vector2 size = new Vector2(0, 0);
            if (EGui.UIProxy.DockProxy.BeginMainForm("ClrProfiler", this, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                UpdateLogs();
                foreach (var i in mClrLogs)
                {
                    ImGuiAPI.TextAsPointer((sbyte*)&i.m_mString);
                }
            }
            EGui.UIProxy.DockProxy.EndMainForm();
        }
    }
}

namespace EngineNS.Editor
{
    public partial class UMainEditorApplication
    {
        public EngineNS.Windows.UClrProfiler mClrProfiler = new Windows.UClrProfiler();
    }
}

