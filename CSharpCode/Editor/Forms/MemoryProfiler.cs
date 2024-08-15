using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Editor.Forms
{
    public class TtMemoryProfiler : IRootForm
    {
        public TtMemoryProfiler()
        {
            TtEngine.RootFormManager.RegRootForm(this);
        }
        public bool Visible { get; set; } = true;
        public uint DockId { get; set; }
        public ImGuiWindowClass DockKeyClass { get; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        public async Thread.Async.TtTask<bool> Initialize()
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
            return true;
        }

        public void Dispose() { }

        public void OnDraw()
        {
            var size = new Vector2(800, 600);
            ImGuiAPI.SetNextWindowSize(in size, ImGuiCond_.ImGuiCond_FirstUseEver);
            var result = EGui.UIProxy.DockProxy.BeginMainForm("MemProfiler", this, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (result)
            {
                if (ImGuiAPI.BeginTabBar("RHI", ImGuiTabBarFlags_.ImGuiTabBarFlags_None))
                {
                    ImGuiAPI.Text($"GraphicsDrawcall = {TtStatistic.Instance.GraphicsDrawcall.Value} / {TtStatistic.Instance.NativeGraphicsDrawcall}");
                    ImGuiAPI.Text($"ComputeDrawcall = {TtStatistic.Instance.ComputeDrawcall.Value} / {TtStatistic.Instance.NativeComputeDrawcall}");
                    ImGuiAPI.Text($"TransferDrawcall = {TtStatistic.Instance.TransferDrawcall.Value} / {TtStatistic.Instance.NativeTransferDrawcall}");

                    var stats = TtStatistic.Instance.RenderCmdQueue;
                    ImGuiAPI.Text($"CmdList = {stats.NumOfCmdlist};Drawcall = {stats.NumOfDrawcall};Primitive = {stats.NumOfPrimitive};");
                    ImGuiAPI.EndTabBar();
                }
                
            }
            EGui.UIProxy.DockProxy.EndMainForm(result);
        }
    }
}
