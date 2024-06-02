using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Editor.Forms
{
    public class TtGpuProfiler : IRootForm
    {
        public TtGpuProfiler()
        {
            UEngine.RootFormManager.RegRootForm(this);
        }

        public async Thread.Async.TtTask<bool> Initialize()
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
            return true;
        }

        public void Dispose() { }
        bool mVisible = true;
        public bool Visible
        {
            get => mVisible;
            set => mVisible = value;
        }
        public uint DockId { get; set; }
        public ImGuiWindowClass DockKeyClass { get; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        public void OnDraw()
        {
            OnDrawImpl();
        }
        string mFilter;
        bool mFilterFocusd;
        private unsafe void OnDrawImpl()
        {
            if (Visible == false)
                return;

            var size = new Vector2(800, 600);
            ImGuiAPI.SetNextWindowSize(in size, ImGuiCond_.ImGuiCond_FirstUseEver);
            var result = EGui.UIProxy.DockProxy.BeginMainForm("GpuProfiler", this, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (result)
            {
                var cmdlst = ImGuiAPI.GetWindowDrawList();
                var stats = UEngine.Instance.GfxDevice.RenderCmdQueue.GetStat();
                ImGuiAPI.Text($"CmdList = {stats.NumOfCmdlist};Drawcall = {stats.NumOfDrawcall};Primitive = {stats.NumOfPrimitive};");
                EGui.UIProxy.SearchBarProxy.OnDraw(ref mFilterFocusd, cmdlst, "filter", ref mFilter, ImGuiAPI.GetWindowContentRegionWidth());
                DockId = ImGuiAPI.GetWindowDockID();
                if (ImGuiAPI.BeginChild("TimeScope", in Vector2.MinusOne, true, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                {
                    if (ImGuiAPI.BeginTable("TimeScope", 2, ImGuiTableFlags_.ImGuiTableFlags_Resizable | ImGuiTableFlags_.ImGuiTableFlags_ScrollY, in Vector2.Zero, 0.0f))
                    {
                        var startY = ImGuiAPI.GetItemRectMax().Y;
                        ImGuiAPI.TableNextRow(ImGuiTableRowFlags_.ImGuiTableRowFlags_Headers, 0);
                        ImGuiAPI.TableSetColumnIndex(0);
                        ImGuiAPI.Text("Name");
                        if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                        {
                            
                        }
                        ImGuiAPI.TableSetColumnIndex(1);
                        ImGuiAPI.Text("AvgTime");
                        if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                        {
                            
                        }

                        foreach (var j in UEngine.Instance.ProfilerModule.GpuTimeScopeManager.Scopes.Values)
                        {
                            if (string.IsNullOrEmpty(mFilter) == false && j.Name.Contains(mFilter) == false)
                                continue;

                            ImGuiAPI.TableNextRow(ImGuiTableRowFlags_.ImGuiTableRowFlags_None, 0);

                            ImGuiAPI.TableSetColumnIndex(0);
                            ImGuiAPI.Text(j.Name);
                            if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                            {
                                
                            }

                            ImGuiAPI.TableSetColumnIndex(1);
                            ImGuiAPI.Text(j.TimeUS.ToString());
                            if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Right))
                            {
                                
                            }
                        }
                        ImGuiAPI.EndTable();
                    }

                    ImGuiAPI.EndChild();
                }

                if (OnDrawMenu != null)
                    OnDrawMenu();
            }
            EGui.UIProxy.DockProxy.EndMainForm(result);
        }
        protected bool mMenuShow = false;
        private unsafe void PopItemMenu(string watchingThread, Profiler.URpcProfiler.RpcProfilerData.ScopeInfo scope, string column)
        {
            
        }
        System.Action OnDrawMenu = null;
    }
}
