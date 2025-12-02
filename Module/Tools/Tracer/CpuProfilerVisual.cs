using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineNS;

namespace Tracer
{
    public class TtCpuProfilerVisual : EngineNS.IRootForm
    {
        #region IRootForm
        public TtCpuProfilerVisual()
        {
            TtEngine.RootFormManager.RegRootForm(this);
            ExcludeThreads.Add("TPool");
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
        bool mVisible = true;
        public bool Visible
        {
            get => mVisible;
            set => mVisible = value;
        }
        public uint DockId { get; set; }
        public ImGuiWindowClass DockKeyClass { get; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        #endregion

        public EngineNS.Profiler.Trace.TtCpuFrameProfilerAction FrameProfiler;
        public List<string> ExcludeThreads { get; set; } = new List<string>();
        private bool IsExcludeThread(string name)
        {
            foreach (var i in ExcludeThreads)
            {
                if (name.StartsWith(i))
                    return true;
            }
            return false;
        }
        public unsafe void OnDraw()
        {
            var size = new Vector2(800, 600);
            ImGuiAPI.SetNextWindowSize(in size, ImGuiCond_.ImGuiCond_FirstUseEver);
            var result = EngineNS.EGui.UIProxy.DockProxy.BeginMainForm("CpuVisual", this, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (result)
            {
                var cmdlst = ImGuiAPI.GetWindowDrawList();
                if (ImGuiAPI.BeginTabBar("CPU", ImGuiTabBarFlags_.ImGuiTabBarFlags_None))
                {
                    if (FrameProfiler!=null)
                    {
                        foreach (var i in FrameProfiler.Threads)
                        {
                            if (IsExcludeThread(i.ThreadName))
                                continue;
                            if (ImGuiAPI.BeginTabItem(i.ThreadName, null, ImGuiTabItemFlags_.ImGuiTabItemFlags_None))
                            {
                                if (ImGuiAPI.BeginChild("TimeScope", in Vector2.MinusOne, ImGuiChildFlags_.ImGuiChildFlags_Borders, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                                {
                                    if (ImGuiAPI.BeginTabBar("ShowMode", ImGuiTabBarFlags_.ImGuiTabBarFlags_None))
                                    {
                                        if (ImGuiAPI.BeginTabItem("ByTree", null, ImGuiTabItemFlags_.ImGuiTabItemFlags_None))
                                        {
                                            TimeScopeTree.SetTreeNodes(i.Scopes);
                                            if (ImGuiAPI.BeginChild("ShowTree", in Vector2.MinusOne, ImGuiChildFlags_.ImGuiChildFlags_Borders, ImGuiWindowFlags_.ImGuiWindowFlags_HorizontalScrollbar))
                                            {
                                                DrawByTree(cmdlst, i.ThreadName);
                                            }
                                            ImGuiAPI.EndChild();
                                            ImGuiAPI.EndTabItem();
                                        }

                                        if (ImGuiAPI.BeginTabItem("ByList", null, ImGuiTabItemFlags_.ImGuiTabItemFlags_None))
                                        {
                                            SetTimeList(i.Scopes);
                                            if (ImGuiAPI.BeginChild("ShowList", in Vector2.MinusOne, ImGuiChildFlags_.ImGuiChildFlags_Borders, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                                            {
                                                DrawByList(cmdlst, i.ThreadName);
                                            }
                                            ImGuiAPI.EndChild();
                                            ImGuiAPI.EndTabItem();
                                        }
                                        
                                        ImGuiAPI.EndTabBar();
                                    }
                                }
                                ImGuiAPI.EndChild();

                                ImGuiAPI.EndTabItem();
                            }
                        }
                    }

                    ImGuiAPI.EndTabBar();
                }
            }
            EngineNS.EGui.UIProxy.DockProxy.EndMainForm(result);
        }
        enum ESortMode
        {
            None = 0,
            ByName,
            ByTime,
        }
        ESortMode mSortMode = ESortMode.None;
        List<EngineNS.Profiler.TtRpcProfiler.RpcProfilerData.ScopeInfo> Scopes = new List<EngineNS.Profiler.TtRpcProfiler.RpcProfilerData.ScopeInfo>();
        void SetTimeList(List<EngineNS.Profiler.TtRpcProfiler.RpcProfilerData.ScopeInfo> src)
        {
            if (mSortMode != ESortMode.None)
            {
                for (int j = 0; j < Scopes.Count; j++)
                {
                    bool find = false;
                    foreach (var i in src)
                    {
                        if (Scopes[j].ShowName == i.ShowName)
                        {
                            find = true;
                            break;
                        }
                    }
                    if (find == false)
                    {
                        Scopes.RemoveAt(j);
                        j--;
                    }
                }
                foreach (var i in src)
                {
                    bool find = false;
                    for (int j = 0; j < Scopes.Count; j++)
                    {
                        if (Scopes[j].ShowName == i.ShowName)
                        {
                            Scopes[j] = i;
                            find = true;
                            break;
                        }
                    }
                    if (find == false)
                    {
                        Scopes.Add(i);
                    }
                }
            }
            else
            {
                Scopes.Clear();
                Scopes.AddRange(src);
                //Scopes = src;
            }
            //SortScopes();
        }
        void SortScopes()
        {
            switch (mSortMode)
            {
                case ESortMode.ByName:
                    {
                        Scopes.Sort((x, y) =>
                        {
                            return x.ShowName.CompareTo(y.ShowName);
                        });
                    }
                    break;
                case ESortMode.ByTime:
                    {
                        Scopes.Sort((x, y) =>
                        {
                            return y.AvgTime.CompareTo(x.AvgTime);
                        });
                    }
                    break;
            }


        }
        string mFilter;
        string CurrentName = null;
        internal bool mMenuShow = false;
        private unsafe void PopItemMenu(string watchingThread, EngineNS.Profiler.TtRpcProfiler.RpcProfilerData.ScopeInfo scope, string column)
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
                                    var arg = new EngineNS.Profiler.TtRpcProfiler.ResetMaxTimeArg();
                                    arg.ThreadName = watchingThread;
                                    arg.ScopeName = scope.ShowName;
                                    //Profiler.TtRpcProfiler_RpcCaller.ResetMaxTime(arg, new());
                                    TtEngine.Instance.RpcModule.RpcManager.RpcProfiler.RPC_ResetMaxTime(arg);
                                    OnDrawMenu = null;
                                }
                                ImGuiAPI.EndPopup();
                            }
                            else
                            {
                                OnDrawMenu = null;
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
                case "GotoSource":
                    {
                        OnDrawMenu = () =>
                        {
                            ImGuiAPI.OpenPopup($"ScopeGotoSource", ImGuiPopupFlags_.ImGuiPopupFlags_None);
                            if (ImGuiAPI.BeginPopupContextWindow("ScopeGotoSource", ImGuiPopupFlags_.ImGuiPopupFlags_MouseButtonRight))
                            {
                                mMenuShow = true;
                                if (ImGuiAPI.MenuItem($"GotoSource", null, false, true))
                                {
                                    var plugin = EngineNS.Bricks.DevIDE.TtDevIDEPlugin.FindDevIDEPlugin();
                                    if (plugin!=null)
                                    {
                                        var file = scope.SourceFile;
                                        plugin.OpenFileAtLine(file, scope.SourceLine);
                                    }
                                    OnDrawMenu = null;
                                }
                                ImGuiAPI.EndPopup();
                            }
                            else
                            {
                                OnDrawMenu = null;
                                mMenuShow = false;
                            }
                            ImGuiAPI.CloseCurrentPopup();
                        };
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
        private void DrawByList(ImDrawList cmdlst, string i)
        {
            if (ImGuiAPI.BeginTable("ByList", 5, ImGuiTableFlags_.ImGuiTableFlags_Resizable | ImGuiTableFlags_.ImGuiTableFlags_ScrollY, in Vector2.Zero, 0.0f))
            {
                var startY = ImGuiAPI.GetItemRectMax().Y;
                ImGuiAPI.TableNextRow(ImGuiTableRowFlags_.ImGuiTableRowFlags_Headers, 0);
                ImGuiAPI.TableSetColumnIndex(0);
                ImGuiAPI.Text("Name");
                if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                {
                    mSortMode = ESortMode.ByName;
                    SortScopes();
                    CurrentName = null;
                }
                ImGuiAPI.TableSetColumnIndex(1);
                ImGuiAPI.Text("AvgTime");
                if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                {
                    mSortMode = ESortMode.ByTime;
                    SortScopes();
                    CurrentName = null;
                }
                ImGuiAPI.TableSetColumnIndex(2);
                ImGuiAPI.Text("AvgHit");
                ImGuiAPI.TableSetColumnIndex(3);
                ImGuiAPI.Text("MaxTime");
                ImGuiAPI.TableSetColumnIndex(4);
                ImGuiAPI.Text("Parent");

                foreach (var j in Scopes)
                {
                    if (string.IsNullOrEmpty(mFilter) == false && j.ShowName.Contains(mFilter) == false)
                        continue;

                    ImGuiAPI.TableNextRow(ImGuiTableRowFlags_.ImGuiTableRowFlags_None, 0);

                    ImGuiAPI.TableSetColumnIndex(0);
                    ImGuiAPI.Text(j.ShowName);
                    if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                    {
                        ImGuiAPI.SetTooltip(j.ShowName);
                    }
                    if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                    {
                        CurrentName = j.ShowName;
                    }
                    if (j.ShowName == CurrentName)
                    {
                        var start = ImGuiAPI.GetItemRectMin();
                        if (startY <= start.Y)
                        {
                            var textSize = ImGuiAPI.CalcTextSize(j.ShowName, false, 0);
                            var end = new Vector2(start.X + ImGuiAPI.GetWindowContentRegionWidth(), start.Y + textSize.Y);
                            cmdlst.AddRectFilled(in start, in end, 0x80808080, 1, ImDrawFlags_.ImDrawFlags_None);
                        }
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
                    Vector4 clr = new Vector4(0.5f, 0.69f, 0.93f, 1);
                    if (j.Callers != null)
                    {
                        foreach (var k in j.Callers)
                        {
                            ImGuiAPI.TextColored(in clr, $"[{k.Value}]" + k.Key);
                            if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                            {
                                ImGuiAPI.SetTooltip(k.Key);
                            }
                            var min = ImGuiAPI.GetItemRectMin();
                            var max = ImGuiAPI.GetItemRectMax();
                            min.Y = max.Y;
                            var cmdlist = ImGuiAPI.GetWindowDrawList();
                            cmdlist.AddLine(in min, in max, EngineNS.EGui.UIProxy.StyleConfig.Instance.LinkStringColor, 1);
                            if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                            {
                                CurrentName = k.Key;
                            }
                        }
                    }
                    else
                    {
                        ImGuiAPI.TextColored(in clr, "null");
                    }
                    if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Right))
                    {
                        PopItemMenu(i, j, "Parent");
                    }
                }
                ImGuiAPI.EndTable();
            }
        }

        internal class TtTimeScopeTree : EngineNS.Editor.TtTreeNodeDrawer
        {
            internal class TtTimeScopeNode : EngineNS.Editor.INodeUIProvider
            {
                public System.DateTime LastAccessTime;
                public long NotCountedTime = 0;
                public EngineNS.Profiler.TtRpcProfiler.RpcProfilerData.ScopeInfo TimeInfo;
                public List<TtTimeScopeNode> Children = new List<TtTimeScopeNode>();
                public int NumOfChildUI()
                {
                    return Children.Count;
                }
                public EngineNS.Editor.INodeUIProvider GetChildUI(int index)
                {
                    return Children[index];
                }
                public string NodeName
                {
                    get
                    {
                        return TimeInfo.ShowName;
                    }
                }
                public bool Selected { get; set; }
                public bool DrawNode(EngineNS.Editor.INodeUIProvider parent, EngineNS.Editor.TtTreeNodeDrawer tree, int index, int NumOfChild)
                {
                    ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnArrow | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_SpanFullWidth;
                    if (this.Selected)
                        flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Selected;
                    bool ret = false;
                    var name = (string.IsNullOrEmpty(NodeName) ? "EmptyName" : NodeName) + "##" + index;
                    if (NumOfChild == 0)
                    {
                        flags |= ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf;
                    }
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
                    ImGuiAPI.SameLine(0, -1);
                    float ratio = 1;
                    var pnode = parent as TtTimeScopeNode;
                    if (pnode!=null && TimeInfo.Callers!=null && TimeInfo.Callers.Length>1)
                    {
                        foreach (var i in TimeInfo.Callers)
                        {
                            if (i.Key == pnode.TimeInfo.Name)
                            {
                                ratio = i.Value;
                                break;
                            }
                        }
                    }
                    var txt = $"[Time={TimeInfo.AvgTime},Hit={TimeInfo.AvgHit},Ratio={ratio}]";
                    ImGuiAPI.TextColored(Color4b.DarkGoldenrod.ToColor4Float(), txt);
                    if (this.Children.Count > 0)
                    {
                        txt = $"NC={NotCountedTime}";
                        ImGuiAPI.SameLine(0, -1);
                        ImGuiAPI.TextColored(Color4b.OrangeRed.ToColor4Float(), txt);
                    }

                    return ret;
                }
                public EngineNS.GamePlay.TtWorld GetWorld()
                {
                    return null;
                }
            }
            internal TtTimeScopeNode TimeScopeRootNode = new TtTimeScopeNode();
            internal Dictionary<string, TtTimeScopeNode> TreeNodes = new Dictionary<string, TtTimeScopeNode>();
            internal TtCpuProfilerVisual Host;
            internal string Thread;
            internal TtTimeScopeTree()
            {
                TimeScopeRootNode.TimeInfo.ShowName = "Root";
            }
            internal void Reset()
            {
                TimeScopeRootNode.Children.Clear();
                TreeNodes.Clear();
            }
            internal unsafe void OnDraw(TtCpuProfilerVisual host, string thread)
            {
                Host = host;
                Thread = thread;
                DrawTree(null, TimeScopeRootNode, 0);
            }
            private List<string> rmvNodes = new List<string>();
            internal void SetTreeNodes(List<EngineNS.Profiler.TtRpcProfiler.RpcProfilerData.ScopeInfo> src)
            {
                var now = System.DateTime.UtcNow;
                //bool bAdd = false;
                //update
                foreach (var i in src)
                {
                    TtTimeScopeNode node;
                    if (TreeNodes.TryGetValue(i.ShowName, out node))
                    {
                        node.TimeInfo = i;
                        node.LastAccessTime = now;
                    }
                    else
                    {
                        node = new TtTimeScopeNode();
                        node.TimeInfo = i;
                        node.LastAccessTime = now;
                        TreeNodes.Add(i.ShowName, node);
                        //bAdd = true;
                    }
                }

                //remove timeout
                {
                    rmvNodes.Clear();
                    foreach (var i in TreeNodes)
                    {
                        var timeSpan = now - i.Value.LastAccessTime;
                        if (timeSpan.Seconds > 3)
                        {
                            rmvNodes.Add(i.Key);
                        }
                    }
                    foreach (var i in rmvNodes)
                    {
                        TreeNodes.Remove(i);
                    }
                }

                //rebuild tree
                //if (bAdd || rmvNodes.Count > 0)
                {
                    TimeScopeRootNode.Children.Clear();
                    foreach (var i in TreeNodes)
                    {
                        i.Value.Children.Clear();
                    }
                    foreach (var i in TreeNodes)
                    {
                        if (i.Value.TimeInfo.Callers!=null)
                        {
                            foreach (var j in i.Value.TimeInfo.Callers)
                            {
                                TtTimeScopeNode node;
                                if (TreeNodes.TryGetValue(j.Key, out node))
                                {
                                    node.Children.Add(i.Value);
                                }
                                else
                                {
                                    TimeScopeRootNode.Children.Add(i.Value);
                                }
                            }
                        }
                        else
                        {
                            TimeScopeRootNode.Children.Add(i.Value);
                        }
                    }
                }
                rmvNodes.Clear();

                //count delta
                foreach (var i in TreeNodes)
                {
                    long t = 0;
                    foreach (var j in i.Value.Children)
                    {
                        t += j.TimeInfo.AvgTime;
                    }
                    i.Value.NotCountedTime = i.Value.TimeInfo.AvgTime - t;
                }
            }
            internal void SortNodes()
            {
                foreach (var i in TreeNodes)
                {
                    i.Value.Children.Sort((x, y) =>
                    {
                        return y.TimeInfo.AvgTime.CompareTo(x.TimeInfo.AvgTime);
                    });
                }
            }

            public override void OnNodeUI_RClick(EngineNS.Editor.INodeUIProvider provider)
            {
                //Host.PopItemMenu(Thread, (provider as TtTimeScopeNode).TimeInfo, "GotoSource");
            }
        }
        internal TtTimeScopeTree TimeScopeTree = new TtTimeScopeTree();

        private void DrawByTree(ImDrawList cmdlst, string i)
        {
            TimeScopeTree.SortNodes();
            TimeScopeTree.OnDraw(this, i);
        }
    }
}
