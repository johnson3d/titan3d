using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EngineNS.Editor.Forms
{
    public class UCpuProfiler : Graphics.Pipeline.IRootForm
    {
        public UCpuProfiler()
        {
            UEngine.RootFormManager.RegRootForm(this);
        }

        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            return true;
        }

        public void Cleanup() { }
        public bool Visible { get; set; } = true;
        public uint DockId { get; set; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        Task<Profiler.URpcProfiler.RpcProfilerData> mRpcProfilerData;
        Task<Profiler.URpcProfiler.RpcProfilerThreads> mRpcProfilerThreads;
        List<string> ProfilerThreadNames = new List<string>();
        List<Profiler.URpcProfiler.RpcProfilerData.ScopeInfo> Scopes = new List<Profiler.URpcProfiler.RpcProfilerData.ScopeInfo>();
        public unsafe void OnDraw()
        {
            if (mRpcProfilerThreads == null || mRpcProfilerThreads.IsCompleted)
            {
                if (UEngine.Instance.RpcModule.RpcManager != null)
                {
                    if (mRpcProfilerThreads != null)
                        ProfilerThreadNames = mRpcProfilerThreads.Result.ThreadNames;
                    mRpcProfilerThreads = Profiler.URpcProfiler.GetProfilerThreads(0);
                }
            }
                
            if (Visible == false)
                return;
            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);

            var size = new Vector2(800, 600);
            ImGuiAPI.SetNextWindowSize(in size, ImGuiCond_.ImGuiCond_FirstUseEver);
            if (ImGuiAPI.Begin("CpuProfiler", null, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                DockId = ImGuiAPI.GetWindowDockID();
                if (ImGuiAPI.BeginTabBar("CPU", ImGuiTabBarFlags_.ImGuiTabBarFlags_None))
                {
                    foreach (var i in ProfilerThreadNames)
                    {
                        if (ImGuiAPI.BeginTabItem(i, null, ImGuiTabItemFlags_.ImGuiTabItemFlags_None))
                        {
                            if (mRpcProfilerData == null || mRpcProfilerData.IsCompleted)
                            {
                                if (mRpcProfilerData != null)
                                    Scopes = mRpcProfilerData.Result.Scopes;
                                mRpcProfilerData = Profiler.URpcProfiler.GetProfilerData(i);
                            }

                            if (ImGuiAPI.BeginTable("TimeScope", 5, ImGuiTableFlags_.ImGuiTableFlags_Resizable, in Vector2.Zero, 0.0f))
                            {
                                ImGuiAPI.TableNextRow(ImGuiTableRowFlags_.ImGuiTableRowFlags_Headers, 0);
                                ImGuiAPI.TableSetColumnIndex(0);
                                ImGuiAPI.Text("Name");
                                ImGuiAPI.TableSetColumnIndex(1);
                                ImGuiAPI.Text("AvgTime");
                                ImGuiAPI.TableSetColumnIndex(2);
                                ImGuiAPI.Text("AvgHit");
                                ImGuiAPI.TableSetColumnIndex(3);
                                ImGuiAPI.Text("MaxTime");
                                ImGuiAPI.TableSetColumnIndex(4);
                                ImGuiAPI.Text("Parent");

                                foreach (var j in Scopes)
                                {
                                    ImGuiAPI.TableNextRow(ImGuiTableRowFlags_.ImGuiTableRowFlags_None, 0);
                                    
                                    ImGuiAPI.TableSetColumnIndex(0);
                                    if (j.ShowName != null)
                                    {
                                        ImGuiAPI.Text(j.ShowName);
                                        //if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                                        //    EGui.Controls.CtrlUtility.DrawHelper(j.Value.mCoreObject.GetName());
                                    }
                                    else
                                    {
                                        ImGuiAPI.Text(j.ShowName);
                                    }
                                    ImGuiAPI.TableSetColumnIndex(1);
                                    ImGuiAPI.Text(j.AvgTime.ToString());
                                    if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Right))
                                    {
                                        PopItemMenu(i, j, "AvgTime");
                                    }
                                    ImGuiAPI.TableSetColumnIndex(2);
                                    ImGuiAPI.Text(j.AvgHit.ToString());
                                    if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Right))
                                    {
                                        PopItemMenu(i, j, "AvgHit");
                                    }
                                    ImGuiAPI.TableSetColumnIndex(3);
                                    ImGuiAPI.Text(j.MaxTime.ToString());
                                    if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Right))
                                    {
                                        PopItemMenu(i, j, "MaxTime");
                                    }
                                    ImGuiAPI.TableSetColumnIndex(4);
                                    Vector4 clr = new Vector4(1, 0, 1, 1);
                                    if (j.Parent != "null")
                                    {
                                        ImGuiAPI.TextColored(in clr, j.Parent);
                                        var min = ImGuiAPI.GetItemRectMin();
                                        var max = ImGuiAPI.GetItemRectMax();
                                        min.Y = max.Y;
                                        var cmdlist = ImGuiAPI.GetWindowDrawList();
                                        cmdlist.AddLine(in min, in max, 0xFFFF00FF, 1);
                                        if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                                        {
                                            Console.WriteLine("Jump to parent");
                                        }
                                    }
                                    else
                                        ImGuiAPI.TextColored(in clr, "null");
                                    if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Right))
                                    {
                                        PopItemMenu(i, j, "Parent");
                                    }
                                }
                                ImGuiAPI.EndTable();
                            }
                            ImGuiAPI.EndTabItem();
                        }
                    }
                    ImGuiAPI.EndTabBar();
                }

                if (OnDrawMenu != null)
                    OnDrawMenu();
            }
            ImGuiAPI.End();
        }
        bool mMenuShow = false;
        private unsafe void PopItemMenu(string watchingThread, Profiler.URpcProfiler.RpcProfilerData.ScopeInfo scope, string column)
        {
            switch (column)
            {
                case "AvgTime":
                    {
                        OnDrawMenu = null;
                    }
                    break;
                case "AvgHit":
                    {
                        OnDrawMenu = null;
                    }
                    break;
                case "MaxTime":
                    {
                        OnDrawMenu = () =>
                        {
                            if (ImGuiAPI.BeginPopupContextWindow(null, ImGuiPopupFlags_.ImGuiPopupFlags_MouseButtonRight))
                            {
                                mMenuShow = true;
                                if (ImGuiAPI.MenuItem($"Reset", null, false, true))
                                {
                                    var arg = new Profiler.URpcProfiler.ResetMaxTimeArg();
                                    arg.ThreadName = watchingThread;
                                    arg.ScopeName = scope.ShowName;
                                    Profiler.URpcProfiler.ResetMaxTime(arg);
                                    OnDrawMenu = null;
                                }
                                ImGuiAPI.EndPopup();
                            }
                            else
                            {
                                if (mMenuShow)
                                {
                                    OnDrawMenu = null;
                                }
                                mMenuShow = false;
                            }
                        };
                    }
                    break;
                case "Parent":
                    {
                        OnDrawMenu = null;
                    }
                    break;
                default:
                    {
                        OnDrawMenu = null;
                    }
                    break;
            }
        }
        System.Action OnDrawMenu = null;
    }
}
