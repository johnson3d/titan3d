using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;
using EngineNS.EGui.Controls;

namespace EngineNS.Bricks.Procedure
{
    [Macross.TtMacross]
    [Rtti.Meta("", NameAlias = new string[] { "EngineNS.Bricks.Procedure.UPgcGraphProgram@EngineCore", "EngineNS.Bricks.Procedure.UPgcGraphProgram" })]
    public partial class TtPgcGraphProgram : Macross.AuxMacrossObject
    {
        [Rtti.Meta("")]
        public virtual bool OnNodeInitialized(TtPgcGraph graph, TtPgcNodeBase node)
        {
            return true;
        }
        [Rtti.Meta("")]
        public virtual bool OnNodeProcedureFinished(TtPgcGraph graph, TtPgcNodeBase node)
        {
            return true;
        }
    }
    [Rtti.Meta("", NameAlias = new string[] { "EngineNS.Bricks.Procedure.UPgcGraph@EngineCore", "EngineNS.Bricks.Procedure.UPgcGraph" })]
    public partial class TtPgcGraph : TtNodeGraph
    {
        public const string PgcEditorKeyword = "PGC";
        public bool IsTryCacheBuffer { get; set; } = false;
        [Rtti.Meta("")]
        public uint Version { get; set; } = 0;
        [Rtti.Meta("")]
        public TtBufferCreator DefaultCreator { get; set; } = TtBufferCreator.CreateInstance<TtSuperBuffer<float, FFloatOperator>>(1, 1, 1);

        public TtPgcEditor GraphEditor;
        [System.ComponentModel.Browsable(false)]
        public GamePlay.TtWorld HostWorld { get; set; }
        [System.ComponentModel.Browsable(false)]
        public GamePlay.Scene.TtPgcVolumeNode HostVolume { get; set; }
        public TtPgcBufferCache BufferCache { get; set; } = new TtPgcBufferCache();
        public Node.TtEndingNode Root { get; set; }
        public TtPgcGraph()
        {
            //UpdateCanvasMenus();
            //UpdateNodeMenus();
            //UpdatePinMenus();

            //Root = new Buffer2D.UEndingNode();
        }
        public override TtGraphRenderer GetGraphRenderer()
        {
            return GraphEditor?.GraphRenderer;
        }
        public override void UpdateCanvasMenus()
        {
            CanvasMenus.SubMenuItems.Clear();
            CanvasMenus.Text = "Canvas";

            foreach (var service in Rtti.TtTypeDescManager.Instance.Services.Values)
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
                                    (TtMenuItem item, object sender) =>
                                    {
                                        var node = Rtti.TtTypeDescManager.CreateInstance(typeDesc) as TtNodeBase;
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
                            (TtMenuItem item, object sender) =>
                            {
                                var node = new Node.TtPackNode();
                                node.Name = "Pack " + typeDesc.Name;
                                node.Type = typeDesc;
                                node.UserData = this;
                                node.Position = PopMenuPosition;
                                SetDefaultActionForNode(node);
                                this.AddNode(node);
                            });
                        parentMenu.AddMenuItem("Unpack " + typeDesc.Name, typeDesc.Name, null,
                            (TtMenuItem item, object sender) =>
                            {
                                var node = new Node.TtUnpackNode();
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
        static void GetNodeNameAndMenuStr(in string menuString, TtPgcGraph graph, ref string nodeName, ref string menuName)
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
        public TtPgcExecutionContext ExecutionContext { get; private set; }

        public TtBufferComponent ResolveInput(PinIn pin)
        {
            return ResolveInput(pin, out _);
        }

        public TtBufferComponent ResolveInput(PinIn pin, out bool isExternal)
        {
            isExternal = false;
            if (pin == null)
                return null;
            if (FindInLinkerSingle(pin) != null)
                return BufferCache.FindBuffer(pin);
            if (ExecutionContext != null &&
                ExecutionContext.TryGetExternalInput(pin.NodeId, pin.Name, out var externalBuffer))
            {
                isExternal = true;
                return externalBuffer;
            }
            return null;
        }

        public void ReleaseInput(PinIn pin)
        {
            var buffer = ResolveInput(pin, out var isExternal);
            if (!isExternal && buffer != null)
                buffer.LifeCount--;
        }

        public void Compile(TtPgcNodeBase root, bool resetCache = true)
        {
            Compile(root, null, resetCache);
        }

        public void Compile(TtPgcNodeBase root, TtPgcExecutionContext context, bool resetCache = true)
        {
            ExecutionContext = context;
            try
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
                List<TtPgcNodeBase>[] Layers = new List<TtPgcNodeBase>[NumOfLayer];
                for (int i = 0; i < NumOfLayer; i++)
                {
                    Layers[i] = new List<TtPgcNodeBase>();
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
                        var t1 = Support.TtTime.HighPrecision_GetTickCount();
                        j.DoProcedure(this);
                        this.McProgram?.Get().OnNodeProcedureFinished(this, j);
                        var t2 = Support.TtTime.HighPrecision_GetTickCount();
                        Profiler.Log.WriteLine<Profiler.TtPgcGategory>(Profiler.ELogTag.Info, $"Node:{j.Name} = {(t2 - t1) / 1000000.0f}");
                    }
                }
            }
            finally
            {
                ExecutionContext = null;
            }
        }
        public List<TtPgcNodeBase> CompileGraph(TtPgcNodeBase root)
        {
            List<TtPgcNodeBase> allNodes = new List<TtPgcNodeBase>();
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
                var inNode = linker.InNode as TtPgcNodeBase;
                var outNode = linker.OutNode as TtPgcNodeBase;

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
                var inNode = linker.InNode as TtPgcNodeBase;
                var outNode = linker.OutNode as TtPgcNodeBase;
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
        [Rtti.Meta("")]
        [RName.PGRName(FilterExts = CodeBuilder.TtMacross.AssetExt, MacrossType = typeof(TtPgcGraphProgram))]
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
                    mMcProgram = Macross.TtMacrossGetter<TtPgcGraphProgram>.NewInstance();
                }
                mMcProgram.Name = value;
            }
        }
        Macross.TtMacrossGetter<TtPgcGraphProgram> mMcProgram;
        public Macross.TtMacrossGetter<TtPgcGraphProgram> McProgram
        {
            get
            {
                return mMcProgram;
            }
        }
        [Rtti.Meta("")]
        public TtBufferComponent RegBuffer(PinOut pin, TtBufferComponent buffer)
        {
            return this.BufferCache.RegBuffer(pin, buffer);
        }
        [Rtti.Meta("")]
        public TtPgcNodeBase FindPgcNodeByName(string name,
            [Rtti.MetaParameter(FilterType = typeof(TtPgcNodeBase),
            ConvertOutArguments = Rtti.MetaParameterAttribute.EArgumentFilter.R)]
            System.Type type)
        {
            return this.FindFirstNode(name) as TtPgcNodeBase;
        }
        #endregion

        public override void CollapseNodes(List<TtNodeBase> nodeList)
        {
            var node = IUnionNode.CreateUnionNode<Node.TtUnionNode, Node.UNodePinDefine, Node.TtEndPointNode>(this, nodeList);
            node.Name = "Collapse Node";
            DeleteSelectedNodes();
        }

        public override void SetConfigUnionNode(IUnionNode node)
        {
            this.GraphEditor.SetConfigUnionNode(node);
        }
    }
}


#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace EngineNS.Bricks.Procedure
{
	partial class TtPgcGraphProgram
	{
		public unsafe bool macross_OnNodeInitialized (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, TtPgcGraph graph, TtPgcNodeBase node) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = OnNodeInitialized(graph, node);
			return _return_value;
		}
		public unsafe bool macross_OnNodeProcedureFinished (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, TtPgcGraph graph, TtPgcNodeBase node) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = OnNodeProcedureFinished(graph, node);
			return _return_value;
		}
	}
}


namespace EngineNS.Bricks.Procedure
{
	partial class TtPgcGraph
	{
		public unsafe TtBufferComponent macross_RegBuffer (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, PinOut pin, TtBufferComponent buffer) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = RegBuffer(pin, buffer);
			return _return_value;
		}
		public unsafe TtPgcNodeBase macross_FindPgcNodeByName (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, string name, System.Type type) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = FindPgcNodeByName(name, type);
			return _return_value;
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross