using System;
using System.Collections.Generic;
using System.ComponentModel;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Graphics.Pipeline.Common
{
    [Bricks.CodeBuilder.ContextMenu("Find", "Find", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    [Rtti.Meta("",NameAlias = new string[] { "EngineNS.Graphics.Pipeline.Common.UFindNode@EngineCore", "EngineNS.Graphics.Pipeline.Common.UFindNode" })]
    public class TtFindNode : TAuxRenderGraphNode<TtFindNode>
    {
        public TtRenderGraphPin ResultPinOut = TtRenderGraphPin.CreateOutput("Result", false, EPixelFormat.PXF_UNKNOWN, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);
        TtRenderGraphNode mNode;
        public string mProxyNodeName = "";
        [Rtti.Meta("")]
        [Category("Option")]
        public string ProxyNodeName
        {
            get => mProxyNodeName;
            set
            {
                mProxyNodeName = value;
                mNode = RenderGraph?.FindNodeIgnore(mProxyNodeName, typeof(TtFindNode));
            }
        }

        private Guid mProxyNodeId = Guid.Empty;
        /// <summary>
        /// PropertyGrid 下拉列表编辑器：在 TtFindNode.ProxyNodeId 属性上弹出编辑器图中所有可选节点，
        /// 选择后把节点的 UniqueId 回写到 ProxyNodeId，并同步把 Name 写到 mProxyNodeName（兼容旧逻辑 + 编辑期 RenderGraph 尚未建立的情况）。
        /// 数据源说明：
        ///   - 运行期 TtFindNode 的 RenderGraph 不为 null，可直接 graph.GraphNodes 拿到所有节点
        ///   - 编辑期 TtFindNode 仅作为 TtPolicyNode.GraphNode 的子对象存在，RenderGraph 为 null；
        ///     此时 info.ObjectInstance 是顶层的 TtPolicyNode，从它的 ParentGraph (TtPolicyGraph)
        ///     遍历 Nodes，每个 TtPolicyNode.GraphNode 才是真正的 TtRenderGraphNode。
        /// 参考实现：PGStateMachineSelectAttribute。
        /// </summary>
        public class PGFindNodeSelectAttribute : EGui.Controls.PropertyGrid.TtPGCustomValueEditorAttribute
        {
            // 把候选项抽象成 (Name, Type, UniqueId) 三元组，兼容两种数据源
            private struct NodeCandidate
            {
                public string Name;
                public string TypeName;
                public Guid UniqueId;
            }

            private static TtFindNode ResolveFindNode(object objectInstance)
            {
                // 直接选中了 TtFindNode (罕见, 保险起见)
                if (objectInstance is TtFindNode direct)
                    return direct;

                // 正常情况：PropertyGrid Target 是编辑器外壳 TtPolicyNode, GraphNode 才是 TtFindNode
                if (objectInstance is Bricks.RenderPolicyEditor.TtPolicyNode policyNode)
                    return policyNode.GraphNode as TtFindNode;

                return null;
            }

            private static IEnumerable<NodeCandidate> EnumerateCandidates(object objectInstance, TtFindNode findNode)
            {
                // ① 运行期：TtFindNode 已经被注册到 RenderGraph
                if (findNode != null && findNode.RenderGraph != null)
                {
                    foreach (var kv in findNode.RenderGraph.GraphNodes)
                    {
                        var n = kv.Value;
                        if (n is TtFindNode) continue;
                        yield return new NodeCandidate
                        {
                            Name = n.Name,
                            TypeName = n.GetType().Name,
                            UniqueId = n.UniqueId,
                        };
                    }
                    yield break;
                }

                // ② 编辑期：优先从 TtPolicyNode.ParentGraph 直接取（当 objectInstance 恰好是 TtPolicyNode 时）
                var policyGraph = (objectInstance as TtFindNode)?.BindingPolicyNode?.ParentGraph;

                if (policyGraph == null)
                    yield break;

                foreach (var nb in policyGraph.Nodes)
                {
                    var pn = nb as Bricks.RenderPolicyEditor.TtPolicyNode;
                    var rg = pn?.GraphNode;
                    if (rg == null) continue;
                    if (rg is TtFindNode) continue;
                    yield return new NodeCandidate
                    {
                        Name = rg.Name,
                        TypeName = rg.GetType().Name,
                        UniqueId = rg.UniqueId,
                    };
                }
            }

            public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
            {
                newValue = info.Value;

                var findNode = ResolveFindNode(info.ObjectInstance);
                var currentId = info.Value is Guid g ? g : Guid.Empty;

                // 计算当前选中项显示名（从候选集合里查，而非依赖 findNode.RenderGraph）
                string currentLabel = "None";
                if (currentId != Guid.Empty)
                {
                    bool matched = false;
                    foreach (var c in EnumerateCandidates(info.ObjectInstance, findNode))
                    {
                        if (c.UniqueId == currentId)
                        {
                            currentLabel = $"{c.Name} ({c.TypeName})";
                            matched = true;
                            break;
                        }
                    }
                    if (!matched)
                        currentLabel = $"<Missing> {currentId}";
                }

                if (EGui.UIProxy.ComboBox.BeginCombo("##SelectFindNode", currentLabel))
                {
                    var comboDrawList = ImGuiAPI.GetWindowDrawList();
                    var searchBar = TtEngine.Instance.UIProxyManager["FindNodeSelectSearchBar"] as EGui.UIProxy.SearchBarProxy;
                    if (searchBar == null)
                    {
                        searchBar = new EGui.UIProxy.SearchBarProxy()
                        {
                            InfoText = "Search render graph node",
                            Width = -1,
                        };
                        TtEngine.Instance.UIProxyManager["FindNodeSelectSearchBar"] = searchBar;
                    }
                    // 注意：不要在这里调用 SetKeyboardFocusHere——它会每帧把焦点抢回搜索框，
                    // 导致鼠标在列表项上点击/滚动时被打断，combo 直接关闭并落到第一项。
                    // 需要搜索时，用户主动点击搜索框即可。
                    searchBar.OnDraw(in comboDrawList, in Support.TtAnyPointer.Default);

                    bool bSelected = true;

                    // 第一项：清空选择
                    if (ImGuiAPI.Selectable("None", ref bSelected, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in Vector2.Zero))
                    {
                        newValue = Guid.Empty;
                        if (findNode != null)
                            findNode.mProxyNodeName = "";
                    }

                    var filter = searchBar.SearchText;
                    var hasFilter = !string.IsNullOrEmpty(filter);
                    var filterLower = hasFilter ? filter.ToLower() : null;

                    foreach (var c in EnumerateCandidates(info.ObjectInstance, findNode))
                    {
                        var label = $"{c.Name} ({c.TypeName})";
                        if (hasFilter && !label.ToLower().Contains(filterLower))
                            continue;

                        if (ImGuiAPI.Selectable(label, ref bSelected, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in Vector2.Zero))
                        {
                            newValue = c.UniqueId;
                            // 编辑期 RenderGraph 为 null, setter 里无法同步 mProxyNodeName, 所以在这里直接写入
                            if (findNode != null)
                                findNode.mProxyNodeName = c.Name;
                        }
                        if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                        {
                            EGui.Controls.CtrlUtility.DrawHelper($"{label}\nUniqueId: {c.UniqueId}");
                        }
                    }

                    EGui.UIProxy.ComboBox.EndCombo();
                }

                return true;
            }
        }
        // 通过 UniqueId 引用目标节点，比按 Name 查找更稳健（能抗重名、重命名、输入错误）。
        // Setter 里会在当前 RenderGraph 中查到节点，并把它的 Name 同步到 mProxyNodeName，
        // 兼容存量基于 Name 的查找逻辑（GetReferNode / BeforeTick 里的 fallback）。
        [Rtti.Meta("")]
        [Category("Option")]
        [PGFindNodeSelect()]
        public Guid ProxyNodeId
        {
            get => mProxyNodeId;
            set
            {
                mProxyNodeId = value;
                if (value == Guid.Empty)
                {
                    mNode = null;
                    return;
                }
                var node = RenderGraph?.FindNode(in mProxyNodeId);
                if (node != null)
                {
                    mNode = node;
                    // 同步 Name，让旧的基于 Name 的逻辑仍然可用（存盘/兼容旧资产）
                    mProxyNodeName = node.Name;

                    // 当 ProxyPinName 为空或目标节点上找不到对应 output pin 时，缺省选第一个 output
                    if (string.IsNullOrEmpty(mProxyPinName) || node.FindOutput(mProxyPinName) == null)
                    {
                        if (node.NumOfOutput > 0)
                            mProxyPinName = node.GetOutput(0).Name;
                        else
                            mProxyPinName = "";
                    }
                }
                else
                {
                    // 目标暂未就绪（例如刚反序列化时 RenderGraph 还没装好），
                    // 保持 mProxyNodeName 原样，后续 BeforeTick 会再尝试按 Name 兜底。
                    mNode = null;
                }
            }
        }

        public TtRenderGraphNode GetReferNode()
        {
            if (mNode == null)
            {
                // 优先走 Id；Id 为空时回退到按 Name 查（兼容老数据）
                if (mProxyNodeId != Guid.Empty)
                {
                    ProxyNodeId = ProxyNodeId;
                }
                if (mNode == null)
                {
                    ProxyNodeName = ProxyNodeName;
                }
            }
            return mNode;
        }
        public override Color4b GetTileColor()
        {
            return Color4b.FromRgb(0, 255, 255);
        }
        /// <summary>
        /// PropertyGrid 下拉列表编辑器：在 TtFindNode.ProxyPinName 属性上，
        /// 根据当前 ProxyNodeId 指向的目标节点，列出该节点所有 output pin 的名字供用户选择。
        /// 与 PGFindNodeSelectAttribute 的数据源兜底策略一致：运行期走 RenderGraph，
        /// 编辑期走 TtPolicyNode.ParentGraph / 当前活动 TtPolicyEditor 下的 TtPolicyGraph。
        /// </summary>
        public class PGFindPinSelectAttribute : EGui.Controls.PropertyGrid.TtPGCustomValueEditorAttribute
        {
            private static TtFindNode ResolveFindNode(object objectInstance)
            {
                if (objectInstance is TtFindNode direct)
                    return direct;
                if (objectInstance is Bricks.RenderPolicyEditor.TtPolicyNode policyNode)
                    return policyNode.GraphNode as TtFindNode;
                return null;
            }

            /// <summary>
            /// 返回当前 ProxyNodeId 指向的目标 TtRenderGraphNode。兼容运行期 + 编辑期。
            /// </summary>
            private static TtRenderGraphNode ResolveTargetNode(object objectInstance, TtFindNode findNode)
            {
                if (findNode == null || findNode.ProxyNodeId == Guid.Empty)
                    return null;

                // ① 运行期：RenderGraph 已建立
                if (findNode.RenderGraph != null)
                {
                    var id = findNode.ProxyNodeId;
                    return findNode.RenderGraph.FindNode(in id);
                }

                // ② 编辑期：从 TtPolicyGraph 按 UniqueId 反查
                var policyGraph = (objectInstance as Bricks.RenderPolicyEditor.TtPolicyNode)?.ParentGraph
                                  as Bricks.RenderPolicyEditor.TtPolicyGraph;
                if (policyGraph == null)
                {
                    var app = TtEngine.Instance.GfxDevice.SlateApplication as Editor.TtMainEditorApplication;
                    var policyEditor = app?.AssetEditorManager?.CurrentActiveEditor as Bricks.RenderPolicyEditor.TtPolicyEditor;
                    policyGraph = policyEditor?.PolicyGraph?.PolicyGraph;
                }
                if (policyGraph == null)
                    return null;

                foreach (var nb in policyGraph.Nodes)
                {
                    var pn = nb as Bricks.RenderPolicyEditor.TtPolicyNode;
                    var rg = pn?.GraphNode;
                    if (rg == null) continue;
                    if (rg.UniqueId == findNode.ProxyNodeId)
                        return rg;
                }
                return null;
            }

            public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
            {
                newValue = info.Value;

                var findNode = ResolveFindNode(info.ObjectInstance);
                var currentPin = info.Value as string ?? "";

                var targetNode = ResolveTargetNode(info.ObjectInstance, findNode);

                // 当前显示标签：目标节点为空就提示用户先选 ProxyNodeId；否则显示当前选中项
                string currentLabel;
                if (targetNode == null)
                    currentLabel = string.IsNullOrEmpty(currentPin) ? "<Select ProxyNodeId first>" : currentPin;
                else
                    currentLabel = string.IsNullOrEmpty(currentPin) ? "<None>" : currentPin;

                if (EGui.UIProxy.ComboBox.BeginCombo("##SelectFindPin", currentLabel))
                {
                    bool bSelected = true;

                    if (targetNode != null)
                    {
                        int numOut = targetNode.NumOfOutput;
                        for (int i = 0; i < numOut; i++)
                        {
                            var pin = targetNode.GetOutput(i);
                            if (pin == null) continue;

                            if (ImGuiAPI.Selectable(pin.Name, ref bSelected, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in Vector2.Zero))
                            {
                                newValue = pin.Name;
                            }
                            if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                            {
                                EGui.Controls.CtrlUtility.DrawHelper($"Pin: {pin.Name}\nLinkType: {pin.LinkType}");
                            }
                        }
                        if (numOut == 0)
                        {
                            ImGuiAPI.TextDisabled("(no output pin)");
                        }
                    }
                    else
                    {
                        ImGuiAPI.TextDisabled("(select ProxyNodeId first)");
                    }

                    EGui.UIProxy.ComboBox.EndCombo();
                }

                return true;
            }
        }
        public string mProxyPinName = "";
        [Rtti.Meta("")]
        [Category("Option")]
        [PGFindPinSelect()]
        public string ProxyPinName
        {
            get => mProxyPinName;
            set => mProxyPinName = value;
        }
        public TtFindNode()
        {
            
        }
        public override string Name
        {
            get
            {
                return $"Ref->{ProxyNodeName}:{ProxyPinName}";
            }
            set
            {

            }
        }
        public override void InitNodePins()
        {
            ResultPinOut.LifeMode = TtAttachBuffer.ELifeMode.Imported;
            AddOutput(ResultPinOut);
        }
        public override async Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
        }
        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref ResultAttachement);
            base.Dispose();
        }
        TtAttachBuffer ResultAttachement = new TtAttachBuffer();
        public override void BeforeTick(TtRenderPolicy policy)
        {
            if (mNode == null)
            {
                // 优先按 Id 解析，Id 为空时回退到按 Name 兜底
                if (mProxyNodeId != Guid.Empty)
                {
                    ProxyNodeId = ProxyNodeId;
                }
                if (mNode == null && !string.IsNullOrEmpty(ProxyNodeName))
                {
                    ProxyNodeName = ProxyNodeName;
                }
            }
            if (mNode == null)
            {
                if (mProxyNodeId == Guid.Empty && string.IsNullOrEmpty(ProxyNodeName))
                    return;
                Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Error, $"ProxyNode(Id={mProxyNodeId}, Name={ProxyNodeName}) is not found");
                return;
            }
            var pin = mNode.FindOutput(ProxyPinName);
            if (pin == null)
            {
                Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Error, $"{ProxyNodeName}:{ProxyPinName} is not found");
                return;
            }
            var refAttachement = RenderGraph.AttachmentCache.FindAttachement(pin.Attachement.AttachmentName);
            if (refAttachement == null)
                return;
            var attachement = RenderGraph.AttachmentCache.ImportAttachment(ResultPinOut, ResultAttachement);
            attachement.Srv = refAttachement.Srv;
            attachement.Uav = refAttachement.Uav;
        }
    }

    /// <summary>
    /// "整理线条"节点 (TtPolyLineNode):
    /// - 只暴露一个 InputOutput pin, 用作图上"中转节点", 让用户把杂乱的连线通过它整理走线
    /// - 当 input 被连上时:
    ///   1) 同步本节点 InputOutput pin 的名字 = 上游 OutputPin 的名字
    ///   2) 显示名改成 "Link->{上游节点 Name}:{上游节点 OutPin}"
    /// </summary>
    [Bricks.CodeBuilder.ContextMenu("PolyLine", "PolyLine", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    [Rtti.Meta]
    public class TtPolyLineNode : TAuxRenderGraphNode<TtPolyLineNode>
    {
        // 单一 InputOutput pin: 同时承担 input (被上游连入) 和 output (供下游连出) 的角色
        // 视图类型放开常用的 SRV/UAV/RTV/DSV, 让大多数 RenderGraph pin 都能透传
        public TtRenderGraphPin LinePinInOut = TtRenderGraphPin.CreateInputOutput(
            "Line",
            NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_RTV | NxRHI.EBufferType.BFT_DSV);

        [Rtti.Meta("")]
        public string LinkedNodeName { get; set; }

        public TtPolyLineNode()
        {
            // 参考 TtCullClusterNode: 构造器里设置默认显示名;
            // Name setter 是空实现 (显示走 getter), 这一行主要是给基类反序列化路径走默认值
            Name = "PolyLineNode";
        }

        public override Color4b GetTileColor()
        {
            return Color4b.FromRgb(180, 180, 180);
        }

        public override string Name
        {
            get
            {
                return LinkedNodeName;
            }
            set
            {
                // 显示名由上游节点名驱动 (mLinkedNodeName + getter), 这里不接受外部写入
            }
        }

        public override void InitNodePins()
        {
            AddInputOutput(LinePinInOut);
        }

        public override async Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
        }

        /// <summary>
        /// UI 层"我的 PinIn 被连上"事件 (语义上等价于 RenderGraph 层的 OnLinkIn, 所以不再 override OnLinkIn):
        ///   - iPin: 本节点 UI 层 PinIn (此时仍是旧名)
        ///   - OutNode: 上游 UI 节点 (TtPolicyNode)
        ///   - oPin: 上游 PinOut
        /// 同步本节点 RenderGraph pin / UI PinIn / UI PinOut 的 Name, 把 input 连线改透明, 更新显示名。
        /// AttachmentName 由 RenderGraph BuildGraph 根据连线情况自动传导, 本节点不必手写。
        /// </summary>
        public override void OnUI_LinkedFrom(PinIn iPin, TtNodeBase OutNode, PinOut oPin, TtPinLinker linker)
        {
            base.OnUI_LinkedFrom(iPin, OutNode, oPin, linker);

            if (iPin == null || oPin == null)
                return;

            linker.ShowState = TtPinLinker.EShowState.Hide;
            var newName = oPin.Name;

            LinkedNodeName = "Link->";
            LinkedNodeName += OutNode != null ? OutNode.Name : "";
            LinkedNodeName += ":" + newName;

            BindingPolicyNode.LayoutDirty = true;
        }

        /// <summary>
        /// UI 层"连线被删除"事件: 只响应"自己 input 端"的断开, 把状态还原成初始态。
        /// </summary>
        public override void OnUI_RemoveLinker(TtPinLinker linker)
        {
            base.OnUI_RemoveLinker(linker);

            if (linker == null || linker.InPin == null)
                return;
            if (linker.InPin.HostNode != this.BindingPolicyNode)
                return;

            linker.ShowState = TtPinLinker.EShowState.Normal;
            LinkedNodeName = "Link->";

            BindingPolicyNode.LayoutDirty = true;
        }
    }
}
