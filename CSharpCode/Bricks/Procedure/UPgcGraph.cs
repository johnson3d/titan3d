using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Procedure
{
    [Macross.UMacross]
    public partial class UPgcGraphProgram
    {
        [Rtti.Meta]
        public virtual bool OnNodeInitialized(UPgcGraph graph, UPgcNodeBase node)
        {
            return true;
        }
        [Rtti.Meta]
        public virtual bool OnNodeProcedureFinished(UPgcGraph graph, UPgcNodeBase node)
        {
            return true;
        }
    }
    public partial class UPgcGraph : UNodeGraph
    {
        public const string PgcEditorKeyword = "PGC";
        public bool IsTryCacheBuffer { get; set; } = false;
        [Rtti.Meta]
        public uint Version { get; set; } = 0;
        [Rtti.Meta]
        public UBufferCreator DefaultCreator { get; set; } = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(1, 1, 1);

        public UPgcEditor GraphEditor;
        public UPgcBufferCache BufferCache { get; set; } = new UPgcBufferCache();
        public Node.UEndingNode Root { get; set; }
        public UPgcGraph()
        {
            //UpdateCanvasMenus();
            //UpdateNodeMenus();
            //UpdatePinMenus();

            //Root = new Buffer2D.UEndingNode();
        }
        public override UGraphRenderer GetGraphRenderer()
        {
            return GraphEditor?.GraphRenderer;
        }
        public override void UpdateCanvasMenus()
        {
            CanvasMenus.SubMenuItems.Clear();
            CanvasMenus.Text = "Canvas";

            foreach (var service in Rtti.UTypeDescManager.Instance.Services.Values)
            {
                foreach (var typeDesc in service.Types.Values)
                {
                    var atts = typeDesc.SystemType.GetCustomAttributes(typeof(Bricks.CodeBuilder.ContextMenuAttribute), true);
                    if (atts.Length > 0)
                    {
                        var parentMenu = CanvasMenus;
                        var att = atts[0] as Bricks.CodeBuilder.ContextMenuAttribute;
                        if (!att.HasKeyString(PgcEditorKeyword))
                            continue;
                        for (var menuIdx = 0; menuIdx < att.MenuPaths.Length; menuIdx++)
                        {
                            var menuStr = att.MenuPaths[menuIdx];
                            string nodeName = null;
                            GetNodeNameAndMenuStr(menuStr, this, ref nodeName, ref menuStr);
                            if (menuIdx < att.MenuPaths.Length - 1)
                                parentMenu = parentMenu.AddMenuItem(menuStr, null, null);
                            else
                            {
                                parentMenu.AddMenuItem(menuStr, att.FilterStrings, null,
                                    (UMenuItem item, object sender) =>
                                    {
                                        var node = Rtti.UTypeDescManager.CreateInstance(typeDesc) as UNodeBase;
                                        if (nodeName != null)
                                            node.Name = nodeName;
                                        node.UserData = this;
                                        node.Position = PopMenuPosition;
                                        SetDefaultActionForNode(node);
                                        this.AddNode(node);
                                    });
                            }
                        }
                    }

                    var expandAtts = typeDesc.GetCustomAttributes(typeof(ExpandableAttribute), false);
                    if(expandAtts.Length > 0)
                    {
                        var att = expandAtts[0] as ExpandableAttribute;
                        if (!att.HasKeyString(PgcEditorKeyword))
                            continue;

                        var parentMenu = CanvasMenus.AddMenuItem("Struct", null, null);
                        parentMenu.AddMenuItem("Pack " + typeDesc.Name, typeDesc.Name, null,
                            (UMenuItem item, object sender) =>
                            {
                                var node = new Node.UPackNode();
                                node.Name = "Pack " + typeDesc.Name;
                                node.Type = typeDesc;
                                node.UserData = this;
                                node.Position = PopMenuPosition;
                                SetDefaultActionForNode(node);
                                this.AddNode(node);
                            });
                        parentMenu.AddMenuItem("Unpack " + typeDesc.Name, typeDesc.Name, null,
                            (UMenuItem item, object sender) =>
                            {
                                var node = new Node.UUnpackNode();
                                node.Name = "Unpack " + typeDesc.Name;
                                node.Type = typeDesc;
                                node.UserData = this;
                                node.Position = PopMenuPosition;
                                SetDefaultActionForNode(node);
                                this.AddNode(node);
                            });
                    }
                }
            }
        }
        static void GetNodeNameAndMenuStr(in string menuString, UPgcGraph graph, ref string nodeName, ref string menuName)
        {
            menuName = menuString;
            nodeName = menuName;
            //var idx = menuString.IndexOf('@');
            //if (idx >= 0)
            //{
            //    var idxEnd = menuString.IndexOf('@', idx + 1);
            //    var subStr = menuString.Substring(idx + 1, idxEnd - idx - 1);
            //    subStr = subStr.Replace("serial", graph.GenSerialId().ToString());
            //    menuName = menuString.Remove(idx, idxEnd - idx + 1);
            //    nodeName = menuName.Insert(idx, subStr);
            //}
        }
        public void Compile(UPgcNodeBase root, bool resetCache = true)
        {
            if (resetCache)
                this.BufferCache.ResetCache();
            var nodes = this.CompileGraph(root);
            int NumOfLayer = 0;
            foreach (var i in nodes)
            {
                if (i.RootDistance >= NumOfLayer)
                    NumOfLayer = i.RootDistance;
            }
            NumOfLayer += 1;
            List<UPgcNodeBase>[] Layers = new List<UPgcNodeBase>[NumOfLayer];
            for (int i = 0; i < NumOfLayer; i++)
            {
                Layers[i] = new List<UPgcNodeBase>();
            }

            foreach (var i in nodes)
            {
                i.InitProcedure(this);

                Layers[i.RootDistance].Add(i);
            }
            foreach (var i in nodes)
            {
                this.McProgram?.Get()?.OnNodeInitialized(this, i);
            }

            for (int i = Layers.Length - 1; i >= 0; i--)
            {
                foreach (var j in Layers[i])
                {
                    //AssetGraph.BufferCache .... clear
                    //AssetGraph.BufferCache .... SaveBuffferToTempFile
                    var t1 = Support.Time.HighPrecision_GetTickCount();
                    j.DoProcedure(this);
                    this.McProgram?.Get().OnNodeProcedureFinished(this, j);
                    var t2 = Support.Time.HighPrecision_GetTickCount();
                    Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Procedure", $"Node:{j.Name} = {(t2 - t1) / 1000000.0f}");
                }
            }
        }
        public List<UPgcNodeBase> CompileGraph(UPgcNodeBase root)
        {
            List<UPgcNodeBase> allNodes = new List<UPgcNodeBase>();
            allNodes.Add(root);
            //foreach (UPgcNodeBase i in Nodes)
            //{
            //    //i.InitProcedure(this);
            //    allNodes.Add(i);
            //}
            root.InvTourNodeTree((pin, linker) =>
            {
                if (linker == null)
                    return true;
                var inNode = linker.InNode as UPgcNodeBase;
                var outNode = linker.OutNode as UPgcNodeBase;

                inNode.RootDistance = -1;
                if (!allNodes.Contains(inNode))
                {
                    allNodes.Add(inNode);
                }
                outNode.RootDistance = -1;
                if (!allNodes.Contains(outNode))
                {
                    allNodes.Add(outNode);
                }
                return true;
            });

            root.RootDistance = 0;
            root.InvTourNodeTree((pin, linker) =>
            {
                if (linker == null)
                    return true;
                var inNode = linker.InNode as UPgcNodeBase;
                var outNode = linker.OutNode as UPgcNodeBase;
                if (inNode.RootDistance + 1 > outNode.RootDistance)
                {
                    outNode.RootDistance = inNode.RootDistance + 1;
                }
                return true;
            });

            allNodes.Sort((lh, rh) =>
            {
                return rh.RootDistance.CompareTo(lh.RootDistance);
            });
            return allNodes;
        }
        #region Macross
        [Rtti.Meta]
        [RName.PGRName(FilterExts = CodeBuilder.UMacross.AssetExt, MacrossType = typeof(UPgcGraphProgram))]
        public RName ProgramName
        {
            get
            {
                if (mMcProgram == null)
                    return null;
                return mMcProgram.Name;
            }
            set
            {
                if (mMcProgram == null)
                {
                    mMcProgram = Macross.UMacrossGetter<UPgcGraphProgram>.NewInstance();
                }
                mMcProgram.Name = value;
            }
        }
        Macross.UMacrossGetter<UPgcGraphProgram> mMcProgram;
        public Macross.UMacrossGetter<UPgcGraphProgram> McProgram
        {
            get
            {
                return mMcProgram;
            }
        }
        [Rtti.Meta]
        public UBufferConponent RegBuffer(PinOut pin, UBufferConponent buffer)
        {
            return this.BufferCache.RegBuffer(pin, buffer);
        }
        [Rtti.Meta]
        public UPgcNodeBase FindPgcNodeByName(string name,
            [Rtti.MetaParameter(FilterType = typeof(UPgcNodeBase),
            ConvertOutArguments = Rtti.MetaParameterAttribute.EArgumentFilter.R)]
            System.Type type)
        {
            return this.FindFirstNode(name) as UPgcNodeBase;
        }
        #endregion

        public override void CollapseNodes(List<UNodeBase> nodeList)
        {
            var node = IUnionNode.CreateUnionNode<Node.UUnionNode, Node.UNodePinDefine, Node.UEndPointNode>(this, nodeList);
            node.Name = "Collapse Node";
            DeleteSelectedNodes();
        }

        public override void SetConfigUnionNode(IUnionNode node)
        {
            this.GraphEditor.SetConfigUnionNode(node);
        }
    }
}
