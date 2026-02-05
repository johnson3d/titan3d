using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EngineNS;
using EngineNS.EGui;
using EngineNS.EGui.Controls;

namespace AloneGame
{
    public class TtGameForm : IRootForm
    {
        public TtGameForm()
        {
            TtEngine.RootFormManager.RegRootForm(this);

            Visible = true;
        }
        ~TtGameForm()
        {
        }
        public async EngineNS.Thread.Async.TtTask<bool> Initialize()
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
            return true;
        }
        public void Dispose()
        {
            TtEngine.RootFormManager.UnregRootForm(this);
        }
        public bool Visible { get; set; }
        public uint DockId { get; set; }
        public ImGuiWindowClass DockKeyClass { get; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        public unsafe void OnDraw()
        {
            if (Visible == false)
                return;
            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);
            var size = new Vector2(800, 600);
            ImGuiAPI.SetNextWindowSize(in size, ImGuiCond_.ImGuiCond_FirstUseEver);
            var result = EngineNS.EGui.UIProxy.DockProxy.BeginMainForm("TtGameForm", this, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (result)
            {
                DockId = ImGuiAPI.GetWindowDockID();
                //Draw Frames
                //TtTraceApplication.Instance
                foreach (var i in TtEngine.Instance.Tracer.Frames)
                {
                    //draw i.DurationMillisecond,click to show details
                }
            }
            EngineNS.EGui.UIProxy.DockProxy.EndMainForm(result);
        }
    }
}
